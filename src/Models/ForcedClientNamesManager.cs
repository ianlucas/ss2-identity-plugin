/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.Natives;

namespace Identity;

/// <summary>
/// The value stored in each RB tree node's Data region.
/// Layout (16 bytes):
///   +0: uint SteamAccountId (4 bytes)
///   +4: padding (4 bytes)
///   +8: nint NamePointer (8 bytes) — pointer to a null-terminated string
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ForcedNameEntry
{
    public uint SteamAccountId;
    private uint _padding;
    public nint NamePointer;

    public string? Name => NamePointer != nint.Zero ? Marshal.PtrToStringAnsi(NamePointer) : null;
}

/// <summary>
/// Manages the game's forced client names CUtlRBTree in server memory.
///
/// The tree is a CUtlRBTree&lt;ForcedNameEntry, int&gt; keyed by SteamAccountId.
/// Each node is 32 bytes:
///   +0:  Links.Left   (int)
///   +4:  Links.Right  (int)
///   +8:  Links.Parent (int)
///   +12: Links.Tag    (int) — RB color: 0=RED, 1=BLACK
///   +16: Data.SteamAccountId (uint)
///   +20: Data._padding
///   +24: Data.NamePointer (char*)
///
/// Tree struct layout from unk_244D460:
///   +0x00: LessFunc (8 bytes, function pointer)
///   +0x08: Elements — CUtlLeanVector (Count, AllocFlags, Data pointer)
///   +0x18: Root (int)
///   +0x1C: NumElements (int)
///   +0x20: FirstFree (int)
///   +0x24: LastAlloc (int)
/// </summary>
public class ForcedClientNamesManager : IDisposable
{
    private readonly nint _treeBase;
    private readonly List<nint> _allocatedStrings = new();

    // Signature: "4C 8D 2D ? ? ? ? 4C 89 EF E8 ? ? ? ? 4C 8B 25"
    // This resolves to qword_244D468 (Elements field).
    // Tree base (LessFunc) is 8 bytes before that.
    //
    // Resolution:
    //   nint sigAddr = <result of signature scan>;
    //   nint elementsAddr = sigAddr + 7 + *(int*)(sigAddr + 3);
    //   nint treeBase = elementsAddr - 8;

    public ForcedClientNamesManager(nint treeBase)
    {
        _treeBase = treeBase;
    }

    /// <summary>
    /// Resolve the tree base address from the signature scan result.
    /// The sig "4C 8D 2D ? ? ? ? 4C 89 EF E8 ? ? ? ? 4C 8B 25"
    /// points to a LEA r13, [rip + offset] instruction.
    /// </summary>
    public static nint ResolveTreeBase(nint signatureAddress)
    {
        // LEA r13, [rip + disp32] — 7 byte instruction
        // RIP-relative: target = instruction_addr + 7 + disp32
        int displacement = Marshal.ReadInt32(signatureAddress + 3);
        nint elementsAddr = signatureAddress + 7 + displacement;

        // Elements is at tree base + 0x08 (LessFunc is 8 bytes)
        nint treeBase = elementsAddr - 8;
        return treeBase;
    }

    /// <summary>
    /// Get a managed reference to the game's tree struct in memory.
    /// </summary>
    public unsafe ref CUtlRBTree<ForcedNameEntry, int> Tree =>
        ref *(CUtlRBTree<ForcedNameEntry, int>*)_treeBase;

    /// <summary>
    /// Find a node index by SteamID account ID.
    /// We walk the tree manually since the LessFunc is a native function pointer.
    /// </summary>
    /// <returns>Node index, or -1 if not found.</returns>
    public int Find(uint steamAccountId)
    {
        ref var tree = ref Tree;
        int current = tree.Root;

        while (tree.IsValidIndex(current))
        {
            uint nodeId = tree[current].SteamAccountId;

            if (steamAccountId < nodeId)
                current = tree.LeftChild(current);
            else if (steamAccountId > nodeId)
                current = tree.RightChild(current);
            else
                return current;
        }

        return -1;
    }

    /// <summary>
    /// Find where a new node should be inserted in the tree.
    /// </summary>
    private (int parent, bool isLeft) FindInsertionPosition(uint steamAccountId)
    {
        ref var tree = ref Tree;
        int parent = -1;
        bool isLeft = false;
        int current = tree.Root;

        while (tree.IsValidIndex(current))
        {
            parent = current;
            uint nodeId = tree[current].SteamAccountId;

            if (steamAccountId < nodeId)
            {
                isLeft = true;
                current = tree.LeftChild(current);
            }
            else if (steamAccountId > nodeId)
            {
                isLeft = false;
                current = tree.RightChild(current);
            }
            else
            {
                // Already exists — return this node as "parent"
                // Caller should check and update instead of inserting.
                return (current, false);
            }
        }

        return (parent, isLeft);
    }

    /// <summary>
    /// Allocate a stable unmanaged ANSI string. The pointer remains valid
    /// until this manager is disposed.
    /// </summary>
    private nint AllocateUnmanagedString(string name)
    {
        nint ptr = Marshal.StringToHGlobalAnsi(name);
        _allocatedStrings.Add(ptr);
        return ptr;
    }

    /// <summary>
    /// Force a player's display name by their Steam Account ID.
    /// </summary>
    /// <param name="steamAccountId">
    /// The account ID portion of the SteamID64: (steamId64 &amp; 0xFFFFFFFF)
    /// </param>
    /// <param name="forcedName">The display name to enforce.</param>
    /// <returns>True if a new entry was inserted, false if an existing entry was updated.</returns>
    public bool SetForcedName(uint steamAccountId, string forcedName)
    {
        ref var tree = ref Tree;
        nint namePtr = AllocateUnmanagedString(forcedName);

        // Check if this SteamID already has an entry
        int existing = Find(steamAccountId);
        if (existing != -1)
        {
            // Update the name pointer in-place
            tree[existing].NamePointer = namePtr;
            return false;
        }

        // Find where to insert
        var (parent, isLeft) = FindInsertionPosition(steamAccountId);

        // InsertAt allocates a node (from free list or grows the array),
        // calls LinkToParent, and does RB rebalancing.
        int newNode = tree.InsertAt(parent, isLeft);

        // Write the entry data
        tree[newNode] = new ForcedNameEntry
        {
            SteamAccountId = steamAccountId,
            NamePointer = namePtr,
        };

        return true;
    }

    /// <summary>
    /// Get the currently forced name for a player.
    /// </summary>
    /// <returns>The forced name, or null if no entry exists.</returns>
    public string? GetForcedName(uint steamAccountId)
    {
        int node = Find(steamAccountId);
        if (node == -1)
            return null;

        return Tree[node].Name;
    }

    /// <summary>
    /// Remove a forced name entry using the tree's own RemoveAt
    /// (handles unlinking, RB rebalancing, and freeing the node).
    /// </summary>
    /// <returns>True if an entry was removed, false if not found.</returns>
    public bool RemoveForcedName(uint steamAccountId)
    {
        int node = Find(steamAccountId);
        if (node == -1)
            return false;

        Tree.RemoveAt(node);
        return true;
    }

    /// <summary>
    /// Enumerate all currently forced names by in-order traversal.
    /// </summary>
    public Dictionary<uint, string> GetAllForcedNames()
    {
        ref var tree = ref Tree;
        var result = new Dictionary<uint, string>();

        int current = tree.FirstInorder();
        while (tree.IsValidIndex(current))
        {
            ref var entry = ref tree[current];
            result[entry.SteamAccountId] = entry.Name ?? "";
            current = tree.NextInorder(current);
        }

        return result;
    }

    /// <summary>
    /// Get the number of forced name entries.
    /// </summary>
    public uint Count => Tree.Count;

    public void Dispose()
    {
        foreach (var ptr in _allocatedStrings)
            Marshal.FreeHGlobal(ptr);
        _allocatedStrings.Clear();
    }
}
