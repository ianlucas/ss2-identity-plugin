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
        return (thisPtr, playerNamePtr) =>
        {
            next()(thisPtr, playerNamePtr); /*
            var controller = Core.Memory.ToSchemaClass<CCSPlayerController>(thisPtr);
            Core.Scheduler.NextWorldUpdate(() =>
            {
                if (controller.IsValid && ConVars.IsForceNickname.Value && controller.SteamID != 0)
                {
                    var nickname = controller.ToPlayer()?.GetState().Data?.Nickname;
                    if (nickname != null)
                        controller.SetPlayerName(nickname);
                }
            });*/
        };
    }
}
