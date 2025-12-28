/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.SchemaDefinitions;

namespace IdentityPlugin;

public partial class IdentityPlugin
{
    public Natives.CCSPlayerController_SetPlayerNameDelegate OnSetPlayerName(
        Func<Natives.CCSPlayerController_SetPlayerNameDelegate> next
    )
    {
        return (thisPtr, playerName) =>
        {
            next()(thisPtr, playerName);
            var controller = Core.Memory.ToSchemaClass<CCSPlayerController>(thisPtr);
            if (IsForceNickname.Value && UserManager.TryGetValue(controller.SteamID, out var user))
                controller.SetName(user.Nickname);
        };
    }
}
