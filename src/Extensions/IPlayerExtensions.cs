/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Players;

namespace IdentityPlugin;

public static class IPlayerExtensions
{
    public static bool SetName(this IPlayer self, string name)
    {
        if (self.Controller.PlayerName != name)
        {
            self.Controller.PlayerName = name;
            self.Controller.PlayerNameUpdated();
            return true;
        }
        return false;
    }

    public static void SetRating(this IPlayer self, int rating)
    {
        self.Controller.CompetitiveRankType = 11;
        self.Controller.CompetitiveRankTypeUpdated();
        self.Controller.CompetitiveRanking = rating;
        self.Controller.CompetitiveRankingUpdated();
    }

    public static void HideRating(this IPlayer self)
    {
        self.Controller.CompetitiveRankType = 0;
        self.Controller.CompetitiveRankTypeUpdated();
        self.Controller.CompetitiveRanking = 0;
        self.Controller.CompetitiveRankingUpdated();
    }
}
