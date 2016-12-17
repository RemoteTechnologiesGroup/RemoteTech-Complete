using RemoteTech.Common.Interfaces;
using UnityEngine;

namespace RemoteTech.Delay
{
    public class DelayManager : MonoBehaviour, IDelayManager, IInstanciable<IDelayManager>
    {
        /// <summary>
        ///     Speed of light in vacuum, in meters per second.
        /// </summary>
        public const double LightSpeed = 299792458;

        private static DelayManager _delayManager;

        public static IDelayManager Instance => _delayManager;

        public double GetVesselDelay(Vessel vessel)
        {
            // check if there's a vessel and a working connection (a CommNetVessel)
            if (vessel == null || vessel.Connection == null)
                return 0;

            // get the CommNetVessel
            var commNetVessel = vessel.Connection;

            // vessel must be able to communicate and must be connected
            if (!commNetVessel.CanComm || !commNetVessel.IsConnected)
                return 0;

            double distance = 0;

            // traverse all paths in the network to the nearest point of command
            var numPaths = commNetVessel.ControlPath.Count;
            for (var i = 0; i < numPaths; i++)
            {
                var commNetLink = commNetVessel.ControlPath[i];
                distance += commNetLink.cost;
            }

            var delay = distance / LightSpeed;

            return delay;
        }

        public IDelayManager GetInstance()
        {
            return _delayManager;
        }

        public void Awake()
        {
            if (_delayManager != null && _delayManager != this)
                Destroy(gameObject);
            else
                _delayManager = this;
        }
    }
}