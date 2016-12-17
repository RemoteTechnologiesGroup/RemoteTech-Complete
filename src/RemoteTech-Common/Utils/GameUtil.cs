using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteTech.Common.Utils
{
    public static class GameUtil
    {
        public static bool IsGameScenario => (HighLogic.CurrentGame != null && (HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO || HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO_NON_RESUMABLE));
    }
}
