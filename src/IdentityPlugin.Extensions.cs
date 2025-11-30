/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Players;

namespace IdentityPlugin;

public static class IdentityPluginExtensions
{
    public static bool SetName(this IPlayer player, string name)
    {
        if (player.Controller.PlayerName != name)
        {
            player.Controller.PlayerName = name;
            player.Controller.PlayerNameUpdated();
            return true;
        }
        return false;
    }

    public static void SetRating(this IPlayer player, int rating)
    {
        player.Controller.CompetitiveRankType = 11;
        player.Controller.CompetitiveRanking = rating;
        player.Controller.CompetitiveRankingPredicted_Loss = 0;
        player.Controller.CompetitiveRankingPredicted_Tie = 0;
        player.Controller.CompetitiveRankingPredicted_Win = 0;
        player.Controller.CompetitiveWins = 0;
    }

    public static void HideRating(this IPlayer player)
    {
        player.Controller.CompetitiveRankType = 0;
    }
}
