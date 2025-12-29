/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Plugins;

namespace Identity;

[PluginMetadata(
    Id = "Identity",
    Version = "1.0.0",
    Name = "Identity",
    Author = "Ian Lucas",
    Description = "Authenticates external users."
)]
public partial class Identity(ISwiftlyCore core) : BasePlugin(core)
{
    public override void Load(bool hotReload)
    {
        ConVars.Initialize(Core);
        Core.GameData.ApplyPatch("CCSPlayerController::m_iCompetitiveRankingPatch");
        Core.GameData.ApplyPatch("CCSPlayerController::m_iCompetitiveRankingCleanupPatch");
        Core.GameData.ApplyPatch("CCSPlayerController::m_iCompetitiveRankTypePatch");
        Core.Event.OnTick += OnTick;
        Core.Event.OnClientSteamAuthorize += OnClientSteamAuthorize;
        Core.Event.OnClientProcessUsercmds += OnClientProcessUsercmds;
        Core.GameEvent.HookPre<EventPlayerDisconnect>(OnPlayerDisconnect);
        Natives.CCSPlayerController_SetPlayerName.AddHook(OnSetPlayerName);
    }

    public override void Unload() { }
}
