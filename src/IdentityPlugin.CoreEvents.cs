/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace IdentityPlugin;

public partial class IdentityPlugin
{
    public readonly ConcurrentDictionary<ulong, GameButtonFlags> LastPlayerPressedButtons = [];

    public void OnTick()
    {
        var gameRules = Core.EntitySystem.GetGameRules();
        if (gameRules == null)
            return;
        var revealRecipients = new List<int>();
        foreach (var (steamId, user) in PlayersOnTick)
            if (user.Player?.IsValid == true)
            {
                if (IsForceNickname.Value)
                    user.Player.SetName(user.Nickname);
                if (IsForceRating.Value)
                    if (gameRules.TeamIntroPeriod)
                        user.Player.HideRating();
                    else
                        user.Player.SetRating(user.Rating);
                var pressedButtons = user.Player.PressedButtons;
                if (
                    (pressedButtons & GameButtonFlags.Tab) != 0
                    && (LastPlayerPressedButtons[steamId] & GameButtonFlags.Tab) == 0
                )
                    revealRecipients.Add(user.Player.PlayerID);

                LastPlayerPressedButtons[steamId] = pressedButtons;
            }
        if (revealRecipients.Count > 0)
            Core.NetMessage.Send<CCSUsrMsg_ServerRankRevealAll>(msg =>
            {
                foreach (var playerID in revealRecipients)
                    msg.Recipients.AddRecipient(playerID);
            });
    }
}
