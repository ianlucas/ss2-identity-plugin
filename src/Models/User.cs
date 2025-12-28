/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json.Serialization;
using SwiftlyS2.Shared.Players;

namespace IdentityPlugin;

public class User
{
    [JsonPropertyName("nickname")]
    public required string Nickname { get; set; }

    [JsonPropertyName("rating")]
    public required int Rating { get; set; }

    [JsonPropertyName("flags")]
    public required string[] Flags { get; set; }

    public IPlayer? Player;
}
