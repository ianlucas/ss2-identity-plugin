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
        Swiftly.Initialize();
        ConVars.Initialize();
        Core.GameData.ApplyPatch("CCSPlayerController::m_iCompetitiveRanking1");
        Core.GameData.ApplyPatch("CCSPlayerController::m_iCompetitiveRanking2");
        Core.GameData.ApplyPatch("CCSPlayerController::m_iCompetitiveRankType1");
        Core.Event.OnTick += OnTick;
        Core.Event.OnClientSteamAuthorize += OnClientSteamAuthorize;
        Core.Event.OnClientProcessUsercmds += OnClientProcessUsercmds;
        Core.GameEvent.HookPre<EventPlayerDisconnect>(OnPlayerDisconnect);
        Natives.CCSPlayerController_m_iszPlayerName1.AddHook(OnSetPlayerName1);
        Natives.CCSPlayerController_m_iszPlayerName2.AddHook(OnSetPlayerName2);
        Natives.CCSPlayerController_m_iszPlayerName3.AddHook(OnSetPlayerName3);
        Natives.CCSPlayerController_m_iszPlayerName4.AddHook(OnSetPlayerName4);
    }

    public override void Unload() { }
}
