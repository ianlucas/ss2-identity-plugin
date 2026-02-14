/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Identity;

/// <summary>
/// Manages the game's forced client names CUtlRBTree in native memory using
/// raw pointer arithmetic.
///
/// Native tree struct layout (40 bytes):
///   +0x00: LessFunc        (nint, 8 bytes - native function pointer, untouched)
///   +0x08: Elements.Count  (int)  - vector slot high-water mark
///   +0x0C: Elements.Alloc  (int)  - capacity (lower 31 bits), heap flag (bit 31)
///   +0x10: Elements.Data   (nint) - pointer to node array
///   +0x18: Root            (int)
///   +0x1C: NumElements     (int)  - active node count
///   +0x20: FirstFree       (int)
///   +0x24: LastAlloc       (int)
///
/// Native node struct layout (32 bytes):
///   +0x00: Left            (int)
///   +0x04: Right           (int)
///   +0x08: Parent          (int)
///   +0x0C: Tag             (int)  - 0=RED, 1=BLACK
///   +0x10: SteamAccountId  (uint)
///   +0x14: padding         (uint)
///   +0x18: NamePointer     (nint) - pointer to null-terminated ANSI string
/// </summary>
public unsafe class NativeForcedClientNames(nint treeBase) : IDisposable
{
    // Tree field offsets from base.
    private const int TREE_ELEM_COUNT = 0x08;
    private const int TREE_ELEM_ALLOC = 0x0C;
    private const int TREE_ELEM_DATA = 0x10;
    private const int TREE_ROOT = 0x18;
    private const int TREE_NUM_ELEMENTS = 0x1C;
    private const int TREE_FIRST_FREE = 0x20;
    private const int TREE_LAST_ALLOC = 0x24;

    // Node field offsets (32 bytes per node).
    private const int NODE_SIZE = 32;
    private const int NODE_LEFT = 0;
    private const int NODE_RIGHT = 4;
    private const int NODE_PARENT = 8;
    private const int NODE_TAG = 12;
    private const int NODE_STEAM_ID = 16;
    private const int NODE_NAME_PTR = 24;

    private const int RED = 0;
    private const int BLACK = 1;
    private const int INVALID = -1;

    private readonly byte* _base = (byte*)treeBase;
    private readonly List<nint> _allocatedStrings = [];

    public static nint ResolveTreeBase(nint signatureAddress)
    {
        int displacement = *(int*)(signatureAddress + 3);
        nint elementsAddr = signatureAddress + 7 + displacement;
        return elementsAddr - 8;
    }

    private ref int ElemCount => ref *(int*)(_base + TREE_ELEM_COUNT);
    private ref int ElemAlloc => ref *(int*)(_base + TREE_ELEM_ALLOC);
    private ref nint ElemData => ref *(nint*)(_base + TREE_ELEM_DATA);
    private ref int Root => ref *(int*)(_base + TREE_ROOT);
    private ref int NumElements => ref *(int*)(_base + TREE_NUM_ELEMENTS);
    private ref int FirstFree => ref *(int*)(_base + TREE_FIRST_FREE);
    private ref int LastAlloc => ref *(int*)(_base + TREE_LAST_ALLOC);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte* NodePtr(int i) => (byte*)ElemData + (long)NODE_SIZE * i;

    private ref int Left(int i) => ref *(int*)(NodePtr(i) + NODE_LEFT);

    private ref int Right(int i) => ref *(int*)(NodePtr(i) + NODE_RIGHT);

    private ref int Parent(int i) => ref *(int*)(NodePtr(i) + NODE_PARENT);

    private ref int Tag(int i) => ref *(int*)(NodePtr(i) + NODE_TAG);

    private ref uint SteamId(int i) => ref *(uint*)(NodePtr(i) + NODE_STEAM_ID);

    private ref nint NamePtr(int i) => ref *(nint*)(NodePtr(i) + NODE_NAME_PTR);

    private bool IsValid(int i) => i >= 0 && i < ElemCount && Left(i) != i;

    private bool IsRoot(int i) => Root == i;

    private bool IsLeftChild(int i) => Left(Parent(i)) == i;

    private bool IsRed(int i) => Tag(i) == RED;

    private bool IsBlack(int i) => Tag(i) == BLACK;

    private void GrowVector()
    {
        int capacity = ElemAlloc & 0x7FFFFFFF;
        int newCapacity = Math.Max(16, capacity * 2);
        nint data = ElemData;

        if (data == 0)
            data = (nint)NativeMemory.AllocZeroed((nuint)(newCapacity * NODE_SIZE));
        else
            data = (nint)NativeMemory.Realloc((void*)data, (nuint)(newCapacity * NODE_SIZE));

        NativeMemory.Clear(
            (void*)(data + capacity * NODE_SIZE),
            (nuint)((newCapacity - capacity) * NODE_SIZE)
        );

        ElemData = data;
        ElemAlloc = newCapacity; // bit 31 = 0 means we own the memory.
    }

    private int NewNode()
    {
        int elem;
        if (FirstFree == INVALID)
        {
            int capacity = ElemAlloc & 0x7FFFFFFF;
            if (ElemCount >= capacity)
                GrowVector();

            elem = ElemCount;
            ElemCount++;
            LastAlloc = elem;
        }
        else
        {
            elem = FirstFree;
            FirstFree = Right(FirstFree);
        }
        return elem;
    }

    private void FreeNode(int i)
    {
        Left(i) = i; // Mark as freed (sentinel).
        Right(i) = FirstFree;
        FirstFree = i;
    }

    private void RotateLeft(int elem)
    {
        int right = Right(elem);
        Right(elem) = Left(right);
        if (Left(right) != INVALID)
            Parent(Left(right)) = elem;

        if (right != INVALID)
            Parent(right) = Parent(elem);
        if (!IsRoot(elem))
        {
            if (IsLeftChild(elem))
                Left(Parent(elem)) = right;
            else
                Right(Parent(elem)) = right;
        }
        else
            Root = right;

        Left(right) = elem;
        if (elem != INVALID)
            Parent(elem) = right;
    }

    private void RotateRight(int elem)
    {
        int left = Left(elem);
        Left(elem) = Right(left);
        if (Right(left) != INVALID)
            Parent(Right(left)) = elem;

        if (left != INVALID)
            Parent(left) = Parent(elem);
        if (!IsRoot(elem))
        {
            if (IsLeftChild(elem))
                Left(Parent(elem)) = left;
            else
                Right(Parent(elem)) = left;
        }
        else
            Root = left;

        Right(left) = elem;
        if (elem != INVALID)
            Parent(elem) = left;
    }

    private void InsertRebalance(int elem)
    {
        Tag(elem) = RED;

        while (elem != Root && IsRed(Parent(elem)))
        {
            int parent = Parent(elem);
            int grandparent = Parent(parent);

            if (IsLeftChild(parent))
            {
                int uncle = Right(grandparent);
                if (IsValid(uncle) && IsRed(uncle))
                {
                    Tag(parent) = BLACK;
                    Tag(uncle) = BLACK;
                    Tag(grandparent) = RED;
                    elem = grandparent;
                }
                else
                {
                    if (!IsLeftChild(elem))
                    {
                        elem = parent;
                        RotateLeft(elem);
                        parent = Parent(elem);
                        grandparent = Parent(parent);
                    }
                    Tag(parent) = BLACK;
                    Tag(grandparent) = RED;
                    RotateRight(grandparent);
                }
            }
            else
            {
                int uncle = Left(grandparent);
                if (IsValid(uncle) && IsRed(uncle))
                {
                    Tag(parent) = BLACK;
                    Tag(uncle) = BLACK;
                    Tag(grandparent) = RED;
                    elem = grandparent;
                }
                else
                {
                    if (IsLeftChild(elem))
                    {
                        elem = parent;
                        RotateRight(elem);
                        parent = Parent(elem);
                        grandparent = Parent(parent);
                    }
                    Tag(parent) = BLACK;
                    Tag(grandparent) = RED;
                    RotateLeft(grandparent);
                }
            }
        }

        Tag(Root) = BLACK;
    }

    private void LinkToParent(int i, int parent, bool isLeft)
    {
        Parent(i) = parent;
        Left(i) = INVALID;
        Right(i) = INVALID;
        Tag(i) = RED;

        if (parent != INVALID)
        {
            if (isLeft)
                Left(parent) = i;
            else
                Right(parent) = i;
        }
        else
            Root = i;

        InsertRebalance(i);
    }

    private int InsertAt(int parent, bool isLeft)
    {
        int i = NewNode();
        LinkToParent(i, parent, isLeft);
        NumElements++;
        return i;
    }

    private void RemoveRebalance(int elem)
    {
        while (elem != Root && IsBlack(elem))
        {
            int parent = Parent(elem);

            if (elem == Left(parent))
            {
                int sibling = Right(parent);
                if (IsRed(sibling))
                {
                    Tag(sibling) = BLACK;
                    Tag(parent) = RED;
                    RotateLeft(parent);
                    parent = Parent(elem);
                    sibling = Right(parent);
                }
                if (IsBlack(Left(sibling)) && IsBlack(Right(sibling)))
                {
                    if (sibling != INVALID)
                        Tag(sibling) = RED;
                    elem = parent;
                }
                else
                {
                    if (IsBlack(Right(sibling)))
                    {
                        Tag(Left(sibling)) = BLACK;
                        Tag(sibling) = RED;
                        RotateRight(sibling);
                        parent = Parent(elem);
                        sibling = Right(parent);
                    }
                    Tag(sibling) = Tag(parent);
                    Tag(parent) = BLACK;
                    Tag(Right(sibling)) = BLACK;
                    RotateLeft(parent);
                    elem = Root;
                }
            }
            else
            {
                int sibling = Left(parent);
                if (IsRed(sibling))
                {
                    Tag(sibling) = BLACK;
                    Tag(parent) = RED;
                    RotateRight(parent);
                    parent = Parent(elem);
                    sibling = Left(parent);
                }
                if (IsBlack(Right(sibling)) && IsBlack(Left(sibling)))
                {
                    if (sibling != INVALID)
                        Tag(sibling) = RED;
                    elem = parent;
                }
                else
                {
                    if (IsBlack(Left(sibling)))
                    {
                        Tag(Right(sibling)) = BLACK;
                        Tag(sibling) = RED;
                        RotateLeft(sibling);
                        parent = Parent(elem);
                        sibling = Left(parent);
                    }
                    Tag(sibling) = Tag(parent);
                    Tag(parent) = BLACK;
                    Tag(Left(sibling)) = BLACK;
                    RotateRight(parent);
                    elem = Root;
                }
            }
        }
        Tag(elem) = BLACK;
    }

    private void Unlink(int elem)
    {
        if (elem == INVALID)
            return;

        int x,
            y;

        if (Left(elem) == INVALID || Right(elem) == INVALID)
            y = elem;
        else
        {
            y = Right(elem);
            while (Left(y) != INVALID)
                y = Left(y);
        }

        x = Left(y) != INVALID ? Left(y) : Right(y);

        if (x != INVALID)
            Parent(x) = Parent(y);
        if (!IsRoot(y))
        {
            if (IsLeftChild(y))
                Left(Parent(y)) = x;
            else
                Right(Parent(y)) = x;
        }
        else
            Root = x;

        int yColor = Tag(y);
        if (y != elem)
        {
            Parent(y) = Parent(elem);
            Right(y) = Right(elem);
            Left(y) = Left(elem);

            if (!IsRoot(elem))
            {
                if (IsLeftChild(elem))
                    Left(Parent(elem)) = y;
                else
                    Right(Parent(elem)) = y;
            }
            else
                Root = y;

            if (Left(y) != INVALID)
                Parent(Left(y)) = y;
            if (Right(y) != INVALID)
                Parent(Right(y)) = y;

            Tag(y) = Tag(elem);
        }

        if (x != INVALID && yColor == BLACK)
            RemoveRebalance(x);
    }

    private int FirstInorder()
    {
        int current = Root;
        if (!IsValid(current))
            return INVALID;
        while (IsValid(Left(current)))
            current = Left(current);
        return current;
    }

    private int NextInorder(int i)
    {
        if (!IsValid(i))
            return INVALID;
        if (IsValid(Right(i)))
        {
            i = Right(i);
            while (IsValid(Left(i)))
                i = Left(i);
            return i;
        }
        int parent = Parent(i);
        while (IsValid(parent) && i == Right(parent))
        {
            i = parent;
            parent = Parent(i);
        }
        return parent;
    }

    private nint AllocateUnmanagedString(string name)
    {
        nint ptr = Marshal.StringToHGlobalAnsi(name);
        _allocatedStrings.Add(ptr);
        return ptr;
    }

    public int Find(uint steamAccountId)
    {
        int current = Root;
        while (IsValid(current))
        {
            uint nodeId = SteamId(current);
            if (steamAccountId < nodeId)
                current = Left(current);
            else if (steamAccountId > nodeId)
                current = Right(current);
            else
                return current;
        }
        return INVALID;
    }

    private (int parent, bool isLeft) FindInsertionPosition(uint steamAccountId)
    {
        int parent = INVALID;
        bool isLeft = false;
        int current = Root;

        while (IsValid(current))
        {
            parent = current;
            uint nodeId = SteamId(current);
            if (steamAccountId < nodeId)
            {
                isLeft = true;
                current = Left(current);
            }
            else if (steamAccountId > nodeId)
            {
                isLeft = false;
                current = Right(current);
            }
            else
                return (current, false); // Already exists.
        }

        return (parent, isLeft);
    }

    public bool SetForcedName(uint steamAccountId, string forcedName)
    {
        nint namePtr = AllocateUnmanagedString(forcedName);

        int existing = Find(steamAccountId);
        if (existing != INVALID)
        {
            NamePtr(existing) = namePtr;
            return false;
        }

        var (parent, isLeft) = FindInsertionPosition(steamAccountId);
        int newNode = InsertAt(parent, isLeft);
        SteamId(newNode) = steamAccountId;
        *(uint*)(NodePtr(newNode) + 0x14) = 0; // padding
        NamePtr(newNode) = namePtr;
        return true;
    }

    public string? GetForcedName(uint steamAccountId)
    {
        int node = Find(steamAccountId);
        if (node == INVALID)
            return null;
        nint ptr = NamePtr(node);
        return ptr != 0 ? Marshal.PtrToStringAnsi(ptr) : null;
    }

    public bool RemoveForcedName(uint steamAccountId)
    {
        int node = Find(steamAccountId);
        if (node == INVALID)
            return false;
        Unlink(node);
        FreeNode(node);
        NumElements--;
        return true;
    }

    public Dictionary<uint, string> GetAllForcedNames()
    {
        var result = new Dictionary<uint, string>();
        int current = FirstInorder();
        while (IsValid(current))
        {
            nint ptr = NamePtr(current);
            string name = ptr != 0 ? Marshal.PtrToStringAnsi(ptr) ?? "" : "";
            result[SteamId(current)] = name;
            current = NextInorder(current);
        }
        return result;
    }

    public uint Count => (uint)NumElements;

    public void Dispose()
    {
        foreach (var ptr in _allocatedStrings)
            Marshal.FreeHGlobal(ptr);
        _allocatedStrings.Clear();
    }
}
