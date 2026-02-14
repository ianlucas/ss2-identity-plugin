/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Identity;

public static class ForcedClientNames
{
    private static ForcedClientNamesManager _forcedNames = null!;

    public static void Initialize()
    {
        nint? address = Swiftly.Core.GameData.GetSignature("pForcedClientNames");
        if (address == null)
            throw new InvalidOperationException(
                "Failed to locate the address for pForcedClientNames!"
            );
        _forcedNames = new(ForcedClientNamesManager.ResolveTreeBase(address.Value));
    }

    public static bool SetForcedName(uint steamAccountId, string forcedName)
    {
        return _forcedNames.SetForcedName(steamAccountId, forcedName);
    }
}
