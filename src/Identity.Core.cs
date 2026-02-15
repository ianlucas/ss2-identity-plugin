/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace Identity;

public partial class Identity
{
    public void HandleTick()
    {
        if (!ConVars.IsForceRating.Value)
            return;
        var gameRules = Core.EntitySystem.GetGameRules();
        if (gameRules == null)
            return;
        var teamIntroPeriod = gameRules.TeamIntroPeriod;
        var isUpdateRating = gameRules.LastTeamIntroPeriod != teamIntroPeriod;
        gameRules.LastTeamIntroPeriod = teamIntroPeriod;
        if (!isUpdateRating)
            return;
        var players = Core.PlayerManager.GetAllPlayers();
        foreach (var player in players)
            if (!player.IsFakeClient)
            {
                var rating = player.GetState().Data?.Rating;
                if (teamIntroPeriod)
                    player.Controller.HideCompetitiveRanking();
                else if (rating != null)
                    player.Controller.SetCompetitiveRanking(rating.Value);
            }
    }

    public async void HandleClientSteamAuthorize(IPlayer player)
    {
        var steamId = player.SteamID;
        var name = player.Controller.PlayerName;
        var playerState = player.GetState();
        if (player.IsFakeClient || !Api.IsActive() || playerState.IsFetching)
            return;
        Core.Logger.LogInformation("Player {Name} (id: {Id}) is authenticating...", name, steamId);
        playerState.IsFetching = true;
        var user = await Api.FetchUser(steamId);
        playerState.Data = user;
        playerState.IsFetching = false;
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
                if (ConVars.IsStrict.Value)
                    player.Kick(
                        "Failed to fetch player data.",
                        ENetworkDisconnectionReason.NETWORK_DISCONNECT_CLIENT_CONSISTENCY_FAIL
                    );
                return;
            }
            if (ConVars.IsForceNickname.Value)
                player.Controller.SetPlayerName(user.Nickname);
            if (
                ConVars.IsForceRating.Value
                && Core.EntitySystem.GetGameRules()?.TeamIntroPeriod != true
            )
                player.Controller.SetCompetitiveRanking(user.Rating);
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

    public void HandleClientPlayerNameChange(CCSPlayerController controller)
    {
        if (controller.SteamID == 0)
            return;
        var nickname = controller.ToPlayer()?.GetState().Data?.Nickname;
        if (nickname != null)
            controller.SetPlayerName(nickname);
    }

    public void HandleClientProcessUsercmds(IPlayer player)
    {
        if (!ConVars.IsForceRating.Value)
            return;
        var playerState = player.GetState();
        var pressedButtons = player.PressedButtons;
        var isSendNetMessage = (
            (pressedButtons & GameButtonFlags.Tab) != 0
            && (playerState.LastPressedButtons & GameButtonFlags.Tab) == 0
        );
        playerState.LastPressedButtons = pressedButtons;
        if (isSendNetMessage)
            Core.NetMessage.Send<CCSUsrMsg_ServerRankRevealAll>(msg =>
                msg.Recipients.AddRecipient(player.PlayerID)
            );
    }

    public static void HandlePlayerDisconnect(IPlayer player)
    {
        if (player.IsFakeClient)
            return;
        player.RemoveState();
    }
}
