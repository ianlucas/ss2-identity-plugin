/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.SchemaDefinitions;

namespace Identity;

public static class CCSPlayerControllerExtensions
{
    extension(CCSPlayerController self)
    {
        public void SetPlayerName(string name)
        {
            if (self.PlayerName == name)
                return;
            // self.PlayerName = name;
            // self.PlayerNameUpdated();
        }

        public void SetCompetitiveRanking(int rating)
        {
            self.CompetitiveRankType = 11;
            self.CompetitiveRankTypeUpdated();
            self.CompetitiveRanking = rating;
            self.CompetitiveRankingUpdated();
        }

        public void HideCompetitiveRanking()
        {
            self.CompetitiveRankType = 0;
            self.CompetitiveRankTypeUpdated();
        }
    }
}
