/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Convars;

namespace Identity;

public static class ConVars
{
    public static IConVar<string> Url { get; private set; } = null!;
    public static IConVar<bool> IsStrict { get; private set; } = null!;
    public static IConVar<bool> IsForceNickname { get; private set; } = null!;
    public static IConVar<bool> IsForceRating { get; private set; } = null!;

    public static void Initialize(ISwiftlyCore core)
    {
        Url = core.ConVar.Create(
            "identity_url",
            "URL endpoint for fetching player identity data.",
            ""
        );

        IsStrict = core.ConVar.Create(
            "identity_strict",
            "Kick players when their identity data cannot be retrieved.",
            true
        );

        IsForceNickname = core.ConVar.Create(
            "identity_force_nickname",
            "Override player nicknames with their identity nickname.",
            true
        );

        IsForceRating = core.ConVar.Create(
            "identity_force_rating",
            "Override player ratings with their identity rating.",
            true
        );
    }
}
