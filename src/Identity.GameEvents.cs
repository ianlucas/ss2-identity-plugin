/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;

namespace Identity;

public partial class Identity
{
    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event)
    {
        HandlePlayerDisconnect(@event.UserIdPlayer);
        return HookResult.Continue;
    }
}
