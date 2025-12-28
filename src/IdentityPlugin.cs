/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
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
    public override void Load(bool hotReload)
    {
        Natives.Initialize(Core);
        Core.GameData.ApplyPatch("CCSPlayerController_m_iCompetitiveRankingPatch");
        Core.GameData.ApplyPatch("CCSPlayerController_m_iCompetitiveRankingCleanupPatch");
        Core.GameData.ApplyPatch("CCSPlayerController_m_iCompetitiveRankTypePatch");
        Core.Event.OnTick += OnTick;
        Core.Event.OnClientSteamAuthorize += OnClientSteamAuthorize;
        Core.Event.OnClientProcessUsercmds += OnClientProcessUsercmds;
        Core.GameEvent.HookPre<EventPlayerDisconnect>(OnPlayerDisconnect);
        Natives.CCSPlayerController_SetPlayerName.AddHook(OnSetPlayerName);
    }

    public override void Unload() { }
}
