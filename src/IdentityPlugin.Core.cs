/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace IdentityPlugin;

public partial class IdentityPlugin
{
    public readonly ConcurrentDictionary<ulong, bool> PendingFetchPlayers = [];
    public readonly ConcurrentDictionary<ulong, User> PlayersOnTick = [];

    public bool IsEnabled() => Url.Value.Contains("{userId}");

    public async void HandleConnectingPlayer(IPlayer player)
    {
        var steamId = player.SteamID;
        var name = player.Controller.PlayerName;
        if (player.IsFakeClient || !IsEnabled() || PendingFetchPlayers.ContainsKey(steamId))
            return;
        Core.Logger.LogInformation("Player {Name} (id: {Id}) is authenticating.", name, steamId);
        PendingFetchPlayers.TryAdd(steamId, true);
        var user = await FetchUser(steamId);
        PendingFetchPlayers.Remove(steamId, out _);
        Core.Scheduler.NextTick(() =>
        {
            if (
                !player.IsValid
                || player.Controller.Connected > PlayerConnectedState.PlayerConnecting
            )
                return;
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
            if (IsForceNickname.Value || IsForceRating.Value)
            {
                PlayersOnTick.TryAdd(player.SteamID, user);
                Core.Logger.LogInformation("Player {Name} has rating {Rating}.", name, user.Rating);
            }
            if (user.Flags.Length > 0)
            {
                foreach (var flag in user.Flags)
                    Core.Permission.AddPermission(steamId, flag);
                Core.Logger.LogInformation("Player {Name} has flags {Flags}.", name, user.Flags);
            }
        });
    }

    public void HandleDisconnectingPlayer(IPlayer player)
    {
        if (player.IsFakeClient)
            return;
        foreach (var (steamId, user) in PlayersOnTick)
            if (steamId == player.SteamID || user.Player?.IsValid != true)
                PlayersOnTick.TryRemove(steamId, out _);
    }
}
