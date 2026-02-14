/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Identity;

public static class ForcedClientNames
{
    private static NativeForcedClientNames _forcedNames = null!;

    public static void Initialize()
    {
        nint? address = Swiftly.Core.GameData.GetSignature("g_ForcedClientNames");
        if (address == null)
            throw new InvalidOperationException(
                "Failed to locate the address for g_ForcedClientNames!"
            );
        _forcedNames = new(NativeForcedClientNames.ResolveTreeBase(address.Value));
    }

    public static bool SetForcedName(uint steamAccountId, string forcedName)
    {
        return _forcedNames.SetForcedName(steamAccountId, forcedName);
    }

    public static bool RemoveForcedName(uint steamAccountId)
    {
        return _forcedNames.RemoveForcedName(steamAccountId);
    }
}
