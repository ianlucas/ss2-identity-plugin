/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace IdentityPlugin;

public partial class IdentityPlugin
{
    public void OnTick()
    {
        var gameRules = Core.EntitySystem.GetGameRules();
        if (gameRules == null)
            return;
        HandleTick(gameRules);
    }
}
