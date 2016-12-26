namespace RemoteTech.Common.Utils
{
    public static class GameUtil
    {
        public static bool IsGameScenario
            =>
                HighLogic.CurrentGame != null &&
                (HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO ||
                 HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO_NON_RESUMABLE);
    }
}