/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Convars;

namespace Identity;

public static class ConVars
{
    public static readonly IConVar<string> Url = Swiftly.Core.ConVar.Create(
        "identity_url",
        "URL endpoint for fetching player identity data.",
        ""
    );
    public static readonly IConVar<bool> IsStrict = Swiftly.Core.ConVar.Create(
        "identity_strict",
        "Kick players when their identity data cannot be retrieved.",
        true
    );
    public static readonly IConVar<bool> IsForceNickname = Swiftly.Core.ConVar.Create(
        "identity_force_nickname",
        "Override player nicknames with their identity nickname.",
        true
    );
    public static readonly IConVar<bool> IsForceRating = Swiftly.Core.ConVar.Create(
        "identity_force_rating",
        "Override player ratings with their identity rating.",
        true
    );

    public static void Initialize()
    {
        _ = Url;
        _ = IsStrict;
        _ = IsForceNickname;
        _ = IsForceRating;
    }
}
