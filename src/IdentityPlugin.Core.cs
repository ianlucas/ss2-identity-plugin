/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace IdentityPlugin;

public partial class IdentityPlugin
{
    public readonly ConcurrentDictionary<ulong, bool> UserInFetchManager = [];
    public readonly ConcurrentDictionary<ulong, User> UserManager = [];
    public readonly ConcurrentDictionary<ulong, GameButtonFlags> LastPlayerPressedButtons = [];
    public bool LastTeamIntroPeriod = false;

    public bool IsEnabled() => Url.Value.Contains("{userId}");

    public async void HandleConnectingPlayer(IPlayer player)
    {
        var steamId = player.SteamID;
        var name = player.Controller.PlayerName;
        if (player.IsFakeClient || !IsEnabled() || UserInFetchManager.ContainsKey(steamId))
            return;
        Core.Logger.LogInformation("Player {Name} (id: {Id}) is authenticating...", name, steamId);
        UserInFetchManager.TryAdd(steamId, true);
        var user = await Api.FetchUser(steamId);
        UserInFetchManager.Remove(steamId, out _);
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (!player.Controller.IsValid)
            {
                Core.Logger.LogInformation(
                    "Player {Name} (id: {Id}) is no longer valid.",
                    name,
                    steamId
                );
                return;
            }
            if (user == null)
            {
                if (IsStrict.Value)
                    player.Kick(
                        "Failed to fetch player data.",
                        ENetworkDisconnectionReason.NETWORK_DISCONNECT_CLIENT_CONSISTENCY_FAIL
                    );
                return;
            }
            user.Player = player;
            UserManager.TryAdd(player.SteamID, user);
            if (IsForceNickname.Value)
                player.Controller.SetName(user.Nickname);
            if (IsForceRating.Value)
                player.Controller.SetRating(user.Rating);
            if (user.Flags.Length > 0)
                foreach (var flag in user.Flags)
                    Core.Permission.AddPermission(steamId, flag);
            Core.Logger.LogInformation(
                "Player {Name} (id: {Id}) is authenticated (rating={Rating}, flags={Flags}).",
                name,
                steamId,
                user.Rating,
                user.Flags
            );
        });
    }

    public void HandleTick()
    {
        if (!IsForceRating.Value)
            return;
        var teamIntroPeriod = Core.EntitySystem.GetGameRules()?.TeamIntroPeriod;
        if (teamIntroPeriod == null)
            return;
        var isUpdateRating = LastTeamIntroPeriod != teamIntroPeriod;
        LastTeamIntroPeriod = teamIntroPeriod.Value;
        if (!isUpdateRating)
            return;
        foreach (var (_, user) in UserManager)
            if (user.Player?.IsValid == true)
                if (teamIntroPeriod.Value)
                    user.Player.Controller.HideRating();
                else
                    user.Player.Controller.SetRating(user.Rating);
    }

    public void HandlePlayerInput(IPlayer player)
    {
        if (IsForceRating.Value && UserManager.TryGetValue(player.SteamID, out var user))
        {
            var pressedButtons = player.PressedButtons;
            var lastPressedButtons = LastPlayerPressedButtons.GetOrAdd(
                player.SteamID,
                GameButtonFlags.None
            );
            var isSendNetMessage = (
                (pressedButtons & GameButtonFlags.Tab) != 0
                && (lastPressedButtons & GameButtonFlags.Tab) == 0
            );
            LastPlayerPressedButtons[player.SteamID] = pressedButtons;
            if (isSendNetMessage)
                Core.NetMessage.Send<CCSUsrMsg_ServerRankRevealAll>(msg =>
                    msg.Recipients.AddRecipient(player.PlayerID)
                );
        }
    }

    public void HandleDisconnectingPlayer(IPlayer player)
    {
        if (player.IsFakeClient)
            return;
        foreach (var (steamId, user) in UserManager)
            if (steamId == player.SteamID || user.Player?.IsValid != true)
                UserManager.TryRemove(steamId, out _);
    }
}
