using RemoteTech.Common;
using UnityEngine;

namespace RemoteTech.Delay
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RemoteTechDelayCoreFlight : RemoteTechDelayCore
    {
        public new void Start()
        {
            base.Start();
        }
    }

    public abstract class RemoteTechDelayCore : MonoBehaviour
    {
        public void Start()
        {
            Logging.Debug($"RemoteTech-Delay Starting. Scene: {HighLogic.LoadedScene}");
        }
    }
}
