/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.SchemaDefinitions;

namespace Identity;

public partial class Identity
{
    public Natives.CCSPlayerController_m_iszPlayerName1Delegate OnSetPlayerName1(
        Func<Natives.CCSPlayerController_m_iszPlayerName1Delegate> next
    )
    {
        return (a1, a2) =>
        {
            next()(a1, a2);
            var controller = Swiftly.Core.Memory.ToSchemaClass<CCSPlayerController>(a1);
            if (controller.IsValid)
                HandleClientPlayerNameChange(controller);
        };
    }

    public Natives.CCSPlayerController_m_iszPlayerName2Delegate OnSetPlayerName2(
        Func<Natives.CCSPlayerController_m_iszPlayerName2Delegate> next
    )
    {
        return (a1, a2, a3, a4) =>
        {
            var ret = next()(a1, a2, a3, a4);
            var controller = Swiftly.Core.Memory.ToSchemaClass<CCSPlayerController>(a1);
            if (controller.IsValid)
                HandleClientPlayerNameChange(controller);
            return ret;
        };
    }

    public Natives.CCSPlayerController_m_iszPlayerName3Delegate OnSetPlayerName3(
        Func<Natives.CCSPlayerController_m_iszPlayerName3Delegate> next
    )
    {
        return (a1, a2) =>
        {
            var ret = next()(a1, a2);
            var controller = Swiftly.Core.Memory.ToSchemaClass<CCSPlayerController>(a1);
            if (controller.IsValid)
                HandleClientPlayerNameChange(controller);
            return ret;
        };
    }

    public Natives.CCSPlayerController_m_iszPlayerName4Delegate OnSetPlayerName4(
        Func<Natives.CCSPlayerController_m_iszPlayerName4Delegate> next
    )
    {
        return (a1) =>
        {
            var ret = next()(a1);
            var controller = Swiftly.Core.Memory.ToSchemaClass<CCSPlayerController>(a1);
            if (controller.IsValid)
                HandleClientPlayerNameChange(controller);
            return ret;
        };
    }
}
