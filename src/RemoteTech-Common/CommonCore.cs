using UnityEngine;

namespace RemoteTech.Common
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class CommonCoreMainMenu : CommonCore
    {
        public new void Start()
        {
            base.Start();
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class CommonCoreFlight : CommonCore
    {
        public new void Start()
        {
            base.Start();
        }
    }

    public abstract class CommonCore: MonoBehaviour
    {
        public void Start()
        {
            Logging.Debug($"RemoteTech-Common Starting. Scene: {HighLogic.LoadedScene}");
        }
    }
}
