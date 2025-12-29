/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using SwiftlyS2.Shared.Players;

namespace Identity;

public static class IPlayerExtensions
{
    private static readonly ConcurrentDictionary<ulong, IPlayerState> _playerStateManager = [];

    extension(IPlayer self)
    {
        public IPlayerState State => _playerStateManager.GetOrAdd(self.SteamID, _ => new());

        public void RemoveState()
        {
            _playerStateManager.TryRemove(self.SteamID, out var _);
        }
    }
}
