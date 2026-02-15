/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Memory;

namespace Identity;

public static partial class Natives
{
    public delegate void CCSPlayerController_m_iszPlayerName1Delegate(nint a1, nint a2);

    public static readonly IUnmanagedFunction<CCSPlayerController_m_iszPlayerName1Delegate> CCSPlayerController_m_iszPlayerName1 =
        GetFunctionBySignature<CCSPlayerController_m_iszPlayerName1Delegate>(
            "CCSPlayerController::m_iszPlayerName1"
        );
}
