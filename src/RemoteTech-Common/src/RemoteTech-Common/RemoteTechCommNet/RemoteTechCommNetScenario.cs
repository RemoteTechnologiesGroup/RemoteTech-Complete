using CommNet;
using RemoteTech.Common.RangeModels;

namespace RemoteTech.Common.RemoteTechCommNet
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.EDITOR)]
    public class RemoteTechCommNetScenario : CommNetScenario
    {
        protected override void Start()
        {
            base.Start();

            //TODO pick a range model depending on settings
            RangeModel = new StandardRangeModel();
        }
    }
}
