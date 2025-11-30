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
public partial class IdentityPlugin : BasePlugin
{
    public readonly IConVar<string> Url;
    public readonly IConVar<bool> IsStrict;
    public readonly IConVar<bool> IsForceNickname;
    public readonly IConVar<bool> IsForceRating;

    public IdentityPlugin(ISwiftlyCore core)
        : base(core)
    {
        Url = core.ConVar.Create("identity_url", "URL for fetching player identity.", "");
        IsStrict = core.ConVar.Create(
            "identity_strict",
            "Whether to kick the player if we fail to get their data.",
            true
        );
        IsForceNickname = core.ConVar.Create(
            "identity_force_nickname",
            "Whether to force player nickname.",
            true
        );
        IsForceRating = core.ConVar.Create(
            "identity_force_rating",
            "Whether to force player rating.",
            true
        );

        core.Event.OnTick += OnTick;

        core.GameEvent.HookPre<EventPlayerConnect>(OnPlayerConnect);
        core.GameEvent.HookPre<EventPlayerConnectFull>(OnPlayerConnectFull);
        core.GameEvent.HookPre<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    public override void Load(bool hotReload) { }

    public override void Unload() { }
}
