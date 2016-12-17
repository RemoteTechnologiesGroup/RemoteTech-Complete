
using RemoteTech.Common.Interfaces;

namespace RemoteTech.Delay.Modules
{
    /// <summary>
    /// This module allows any vessel with an antenna to participate in a RemoteTech network, even if it does not have a <see cref="ModuleSPU"/>.
    /// <para>It should be included in all RemoteTech antennas. Unlike ModuleSPU, it does not filter commands or provide a flight computer.</para>
    /// </summary>
    public class ModuleSPUPassive : PartModule, ISignalProcessor
    {
    }
}
