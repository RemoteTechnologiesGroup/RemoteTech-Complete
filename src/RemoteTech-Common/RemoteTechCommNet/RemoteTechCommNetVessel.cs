using System;
using CommNet;
using RemoteTech.Common.Api;
using RemoteTech.Common.Interfaces;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetVessel : CommNetVessel
    {
        private IDelayManager _delayManager;

        public override void OnNetworkPostUpdate()
        {
            base.OnNetworkPostUpdate();

            UpdateDelay();
        }

        public virtual void UpdateDelay()
        {
            if (!RemoteTechModules.RemoteTechDelayAssemblyLoaded)
                return;

            if (_delayManager == null)
                _delayManager =
                    RemoteTechModules.GetObjectFromInterface<IDelayManager>(
                        RemoteTechModules.RemoteTechDelayAssemblyName, Type.EmptyTypes);

            // set up the delay
            signalDelay = _delayManager?.GetVesselDelay(vessel) ?? 0;
        }
    }
}