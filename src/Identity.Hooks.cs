/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.SchemaDefinitions;

namespace Identity;

public partial class Identity
{
    public Natives.CCSPlayerController_SetPlayerNameDelegate OnSetPlayerName(
        Func<Natives.CCSPlayerController_SetPlayerNameDelegate> next
    )
    {
        return (thisPtr, playerName) =>
        {
            next()(thisPtr, playerName);
            var controller = Core.Memory.ToSchemaClass<CCSPlayerController>(thisPtr);
            if (ConVars.IsForceNickname.Value && controller.SteamID != 0)
            {
                var nickname = controller.ToPlayer()?.State.Data?.Nickname;
                if (nickname != null)
                    controller.SetPlayerName(nickname);
            }
        };
    }
}
