/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Convars;

namespace Identity;

public static class ConVars
{
    [SwiftlyInject]
    private static ISwiftlyCore Core { get; set; } = null!;

    public static readonly IConVar<string> Url = Core.ConVar.Create(
        "identity_url",
        "URL endpoint for fetching player identity data.",
        ""
    );

    public static readonly IConVar<bool> IsStrict = Core.ConVar.Create(
        "identity_strict",
        "Kick players when their identity data cannot be retrieved.",
        true
    );

    public static readonly IConVar<bool> IsForceNickname = Core.ConVar.Create(
        "identity_force_nickname",
        "Override player nicknames with their identity nickname.",
        true
    );

    public static readonly IConVar<bool> IsForceRating = Core.ConVar.Create(
        "identity_force_rating",
        "Override player ratings with their identity rating.",
        true
    );
}
