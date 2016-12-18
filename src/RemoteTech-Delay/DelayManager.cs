using RemoteTech.Common.Interfaces;
using RemoteTech.Common.Utils;
using UnityEngine;

namespace RemoteTech.Delay
{
    public class DelayManager : SimpleMonoBehaviorSingleton<DelayManager>, IDelayManager
    {
        /// <summary>
        ///     Speed of light in vacuum, in meters per second.
        /// </summary>
        public const double LightSpeed = 299792458;

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
    }
}