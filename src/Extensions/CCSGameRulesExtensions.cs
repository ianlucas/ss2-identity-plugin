/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.SchemaDefinitions;

namespace Identity;

public static class CCSGameRulesExtensions
{
    private static bool _lastTeamIntroPeriod = false;

    extension(CCSGameRules self)
    {
        public ref bool LastTeamIntroPeriod => ref _lastTeamIntroPeriod;
    }
}
