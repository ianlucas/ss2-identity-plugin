/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.SchemaDefinitions;

namespace IdentityPlugin;

public static class CCSPlayerControllerExtensions
{
    public static bool SetName(this CCSPlayerController self, string name)
    {
        if (self.PlayerName != name)
        {
            self.PlayerName = name;
            self.PlayerNameUpdated();
            return true;
        }
        return false;
    }

    public static void SetRating(this CCSPlayerController self, int rating)
    {
        self.CompetitiveRankType = 11;
        self.CompetitiveRankTypeUpdated();
        self.CompetitiveRanking = rating;
        self.CompetitiveRankingUpdated();
    }

    public static void HideRating(this CCSPlayerController self)
    {
        self.CompetitiveRankType = 0;
        self.CompetitiveRankTypeUpdated();
        self.CompetitiveRanking = 0;
        self.CompetitiveRankingUpdated();
    }
}
