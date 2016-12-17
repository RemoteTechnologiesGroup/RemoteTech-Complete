namespace RemoteTech.Transmitter
{
    /// <summary>
    ///     ModuleRTDeployableAntenna class based on KSP's ModuleDeployableAntenna, whose purpose is to provide antenna-related
    ///     functionalities, such as the deployment safety in a high-speed atmospheric flight or invoke the model-asset animation.
    ///     This class is to re-base RemoteTech's own deployable antennas to the stock antenna's deployment function.
    /// </summary>

    public class ModuleRTDeployableAntenna : ModuleDeployableAntenna
    {
        [KSPField]
        public string antennaModuleName = string.Empty;

        [KSPField]
        public string antennaGUIName = string.Empty;

        /// <summary>
        ///     Execute a set of conditional actions at the flight/launch start
        /// </summary>
        /// <param name="state">Enum-type state of the active vessel</param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (antennaGUIName != string.Empty)
            {
                var name = " (" + antennaGUIName + ")";
                Fields["status"].guiName += name;
                Actions["ExtendPanelsAction"].guiName += name;
                Actions["ExtendAction"].guiName += name;
                Actions["RetractAction"].guiName += name;
                Events["Extend"].guiName += name;
                Events["Retract"].guiName += name;
            }
        }
    }
}
