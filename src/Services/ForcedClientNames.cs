/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Identity;

public static class ForcedClientNames
{
    private static NativeForcedClientNames _forcedClientNames = null!;

    public static void Initialize()
    {
        nint? address = Swiftly.Core.GameData.GetSignature("g_ForcedClientNames");
        if (address == null)
            throw new InvalidOperationException(
                "Failed to locate the address for g_ForcedClientNames!"
            );
        _forcedClientNames = new(NativeForcedClientNames.ResolveTreeBase(address.Value));
    }

    public static bool Set(uint steamAccountId, string forcedName)
    {
        return _forcedClientNames.SetForcedName(steamAccountId, forcedName);
    }

    public static bool Remove(uint steamAccountId)
    {
        return _forcedClientNames.RemoveForcedName(steamAccountId);
    }
}
