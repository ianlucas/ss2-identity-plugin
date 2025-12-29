/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Events;

namespace Identity;

public class IPlayerState
{
    public bool IsFetching = false;
    public User? Data;
    public GameButtonFlags LastPressedButtons = GameButtonFlags.None;
}
