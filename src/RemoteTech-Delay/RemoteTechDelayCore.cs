using RemoteTech.Common;
using UnityEngine;

namespace RemoteTech.Delay
{
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
