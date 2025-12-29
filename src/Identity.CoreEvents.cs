/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Events;

namespace Identity;

public partial class Identity
{
    public void OnTick()
    {
        HandleTick();
    }

    public void OnClientSteamAuthorize(IOnClientSteamAuthorizeEvent @event)
    {
        var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
        if (player != null)
            HandleClientSteamAuthorize(player);
    }

    public void OnClientProcessUsercmds(IOnClientProcessUsercmdsEvent @event)
    {
        var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
        if (player != null && player.IsValid && !player.IsFakeClient)
            HandleClientProcessUsercmds(player);
    }
}
