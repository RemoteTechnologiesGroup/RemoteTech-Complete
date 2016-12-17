namespace RemoteTech.Transmitter
{
    class ModuleRTDeployableAntenna: ModuleDeployableAntenna
    {
        [KSPField]
        public string antennaModuleName = string.Empty;

        [KSPField]
        public string antennaGUIName = string.Empty;

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
