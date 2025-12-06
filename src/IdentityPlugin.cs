/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Convars;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Plugins;

namespace IdentityPlugin;

[PluginMetadata(
    Id = "IdentityPlugin",
    Version = "1.0.0",
    Name = "IdentityPlugin",
    Author = "Ian Lucas",
    Description = "Authenticates external users."
)]
public partial class IdentityPlugin(ISwiftlyCore core) : BasePlugin(core)
{
    public readonly IConVar<string> Url = core.ConVar.Create(
        "identity_url",
        "URL for fetching player identity.",
        ""
    );
    public readonly IConVar<bool> IsStrict = core.ConVar.Create(
        "identity_strict",
        "Whether to kick the player if we fail to get their data.",
        true
    );
    public readonly IConVar<bool> IsForceNickname = core.ConVar.Create(
        "identity_force_nickname",
        "Whether to force player nickname.",
        true
    );
    public readonly IConVar<bool> IsForceRating = core.ConVar.Create(
        "identity_force_rating",
        "Whether to force player rating.",
        true
    );

    public override void Load(bool hotReload)
    {
        Core.Event.OnTick += OnTick;
        Core.Event.OnClientSteamAuthorize += OnClientSteamAuthorize;
        Core.GameEvent.HookPre<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    public override void Unload() { }
}
