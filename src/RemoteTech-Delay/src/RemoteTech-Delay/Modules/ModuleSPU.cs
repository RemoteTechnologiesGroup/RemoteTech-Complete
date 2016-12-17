using System;
using RemoteTech.Common.Interfaces;

namespace RemoteTech.Delay.Modules
{
    /// <summary>
    ///     Signal Processing Unit Module.
    ///     <para>
    ///         This module represents the “autopilot” living in a probe core. A vessel will only filter commands according
    ///         to network availability and time delay if all parts with ModuleCommand also have ModuleSPU; Otherwise, the
    ///         vessel can be controlled in real time. Having at least one ModuleSPU on board is also required to use the
    ///         flight computer.
    ///     </para>
    ///     <para>
    ///         Signal Processors are any part that can receive commands over a working connection (this include all stock
    ///         probe cores).
    ///     </para>
    ///     <para>
    ///         Thus, controlling a vessel is made only through the ModuleSPU unit. Players are only able to control a signal
    ///         processor unit (SPU) as long as they have a working connection (which might be subjected to signal delay).
    ///     </para>
    /// </summary>
    [KSPModule("Signal Processor")]
    public class ModuleSPU : PartModule, ISignalProcessorUnit
    {
        public string Name => $"ModuleSPU({part.vessel.name})";
        public Vessel Vessel => vessel;
    }
}