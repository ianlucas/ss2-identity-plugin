/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Memory;

namespace IdentityPlugin;

public static partial class Natives
{
    public delegate void CCSPlayerController_SetPlayerNameDelegate(nint thisPtr, nint playerName);

    private static readonly Lazy<
        IUnmanagedFunction<CCSPlayerController_SetPlayerNameDelegate>
    > _lazySetPlayerName = new(() =>
        FromSignature<CCSPlayerController_SetPlayerNameDelegate>(
            "CCSPlayerController::SetPlayerName"
        )
    );

    public static IUnmanagedFunction<CCSPlayerController_SetPlayerNameDelegate> CCSPlayerController_SetPlayerName =>
        _lazySetPlayerName.Value;
}
