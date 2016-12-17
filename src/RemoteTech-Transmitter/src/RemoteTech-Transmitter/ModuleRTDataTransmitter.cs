using CommNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RemoteTech.Transmitter
{
    // TODO: 
    //   - add KSPEvents for enabling and disabling antenna and others from ModuleRTAntenna (RTClassic)

    // STOCKBUG #13381 This attribute has no effect
    [KSPModule("Data Transmitter (RT)")]
    public class ModuleRTDataTransmitter : ModuleDataTransmitter, IRelayEnabler
    {
        // TODO: move this to settings?
        private static readonly double stockToRTTelemetryConsumptionFactor = 0.05;
        private static readonly double stockToRTTransmitConsumptionFactor = 0.2;
        private static readonly double stockToRTTransmitDataRateFactor = 0.2;

        [KSPField]
        public double telemetryConsumptionRate = 0.1;

        [KSPField]
        public double transmitConsumptionRate = 5.0;

        [KSPField]
        public double transmitDataRate = 0.5;

        [KSPField(isPersistant = true)]
        public bool antennaEnabled = false;

        [KSPField]
        public bool allowRelay = false;

        [KSPField(isPersistant = true)]
        public bool relayEnabled = false;

        [KSPField]
        public string antennaGUIName = string.Empty;

        [KSPField(guiName = "Partial", guiActive = true),
            UI_Toggle(disabledText = "Prevent", enabledText = "Allow", scene = UI_Scene.Flight)]
        public bool incompleteAllowed = false;

        private float showProgressInterval = 2f;
        private float timeElapsed = 0f;
        private bool shouldConsume = false;
        private bool isDeployable = false;
        private string[] antennaModuleNames;
        private List<ModuleRTDeployableAntenna> antennasToDeploy = new List<ModuleRTDeployableAntenna>();

        public override float DataRate
        {
            get
            {
                return (float)transmitDataRate;
            }
        }

        public override double DataResourceCost
        {
            get
            {
                return transmitConsumptionRate;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            // generate our values if they're missing in cfg (eg. module replacement MM with no content change)
            if (!node.HasValue("transmitConsumptionRate"))
            {
                transmitConsumptionRate = Math.Max(Math.Round((packetResourceCost / packetInterval) * stockToRTTransmitConsumptionFactor, 3), 0.001);
            }
            if (!node.HasValue("telemetryConsumptionRate"))
            {
                telemetryConsumptionRate = Math.Max(Math.Round(transmitConsumptionRate * stockToRTTelemetryConsumptionFactor, 3), 0.001);
            }
            if (!node.HasValue("transmitDataRate"))
            {
                transmitDataRate = Math.Max(Math.Round((packetSize / packetInterval) * stockToRTTransmitDataRateFactor, 3), 0.001);
            }
            // this assumes stock is setting resource consumption rate to 1.0
            for (var i = 0; i < resHandler.inputResources.Count; i++)
            {
                resHandler.inputResources[i].rate *= telemetryConsumptionRate;
            }
            if (node.HasValue("antennaModules"))
            {
                antennaModuleNames = KSPUtil.ParseArray<string>(node.GetValue("antennaModules"), new ParserMethod<string>(s => s));
            }
            else
            {
                antennaModuleNames = new string[0];
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (!(state == StartState.None || state == StartState.Editor))
            {
                shouldConsume = true;
            }
            if (antennaModuleNames.Length > 0)
            {
                for (var i = 0; i < antennaModuleNames.Length; i++)
                {
                    var modules = part.Modules.GetModules<ModuleRTDeployableAntenna>();
                    var found = false;
                    for (var j = 0; j < modules.Count; j++)
                    {
                        if (modules[j].antennaModuleName == antennaModuleNames[i])
                        {
                            antennasToDeploy.Add(modules[j]);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // throw exception?
                    }
                }
            }
            else if (deployFxModules.Count > 0)
            {
                var list = deployFxModules.OfType<ModuleRTDeployableAntenna>().ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    antennasToDeploy.Add(list[i]);
                }
            }
            isDeployable = antennasToDeploy.Count > 0;
            if (!isDeployable)
            {
                Events["ToggleAntenna"].active = true;
                Events["ToggleAntenna"].guiActive = true;
                Events["ToggleAntenna"].guiActiveEditor = true;
                //Events["EnableAntenna"].active = true;
                //Events["DisableAntenna"].active = true;
            }
            else
            {
                Actions["ToggleAntennaAction"].active = false;
                Actions["EnableAntennaAction"].active = false;
                Actions["DisableAntennaAction"].active = false;
            }
            if (antennaGUIName.Length > 0)
            {
                var name = " (" + antennaGUIName + ")";

                Events["ToggleAntenna"].guiName += name;
                Events["EnableAntenna"].guiName += name;
                Events["DisableAntenna"].guiName += name;
                Events["StartTransmission"].guiName += name;
                Events["StopTransmission"].guiName += name;

                Actions["ToggleAntennaAction"].guiName += name;
                Actions["EnableAntennaAction"].guiName += name;
                Actions["DisableAntennaAction"].guiName += name;
                Actions["StartTransmissionAction"].guiName += name;

                Fields["statusText"].guiName += name;
                Fields["powerText"].guiName += name;
                Fields["incompleteAllowed"].guiName += name;
            }
            Events["TransmitIncompleteToggle"].active = false;
            Events["TransmitIncompleteToggle"].guiActive = false;
            Events["TransmitIncompleteToggle"].guiActiveEditor = false;

            Events["StartTransmission"].active = antennaEnabled;
            Events["StartTransmission"].guiActive = antennaEnabled;
            Events["StartTransmission"].guiActiveEditor = antennaEnabled;
        }

        public override string GetInfo()
        {
            var text = new StringBuilder();
            text.Append("<b>Antenna Type: </b>");
            text.AppendLine(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(antennaType.ToString().ToLowerInvariant()));
            text.Append("<b>Antenna Power Rating: </b>");
            text.AppendLine(powerText);
            text.Append("to lvl1 DSN: ");
            text.AppendLine(KSPUtil.PrintSI(CommNetScenario.RangeModel.GetMaximumRange(antennaPower, GameVariables.Instance.GetDSNRange(0f)), "m", 3, false));
            text.Append("to lvl2 DSN: ");
            text.AppendLine(KSPUtil.PrintSI(CommNetScenario.RangeModel.GetMaximumRange(antennaPower, GameVariables.Instance.GetDSNRange(0.5f)), "m", 3, false));
            text.Append("to lvl3 DSN: ");
            text.AppendLine(KSPUtil.PrintSI(CommNetScenario.RangeModel.GetMaximumRange(antennaPower, GameVariables.Instance.GetDSNRange(1f)), "m", 3, false));
            if (antennaType != AntennaType.INTERNAL)
            {
                //text.AppendLine();
                text.Append("<b>Bandwidth: </b>");
                text.AppendLine(transmitDataRate.ToString("###0.### Mits/s"));
            }

            //text.AppendLine();
            text.Append("<b><color=orange>Active antenna requires:");
            var tmpText = resHandler.PrintModuleResources(telemetryConsumptionRate);
            var index = tmpText.IndexOf(":");
            text.AppendLine(tmpText.Substring(index + 1));
            if (antennaType != AntennaType.INTERNAL)
            {
                text.Append("<b><color=orange>Science transmission requires:");
                tmpText = resHandler.PrintModuleResources(transmitConsumptionRate);
                index = tmpText.IndexOf(":");
                text.AppendLine(tmpText.Substring(index + 1));
            }
            else
            {
                text.Append("<b><color=orange>Cannot transmit Science</color></b>");
            }
            if (antennaEnabled)
            {
                text.Append("<b><color=#808080ff>Antenna starts enabled</color></b>");
            }
            else
            {
                text.Append("<b><color=#808080ff>Antenna starts disabled</color></b>");
            }
            return text.ToString();
        }

        public override bool CanCommUnloaded(ProtoPartModuleSnapshot mSnap)
        {
            if (mSnap == null)
            {
                return base.CanCommUnloaded(mSnap);
            }
            if (mSnap.moduleValues.HasValue("antennaEnabled"))
            {
                bool isOn;
                var success = bool.TryParse(mSnap.moduleValues.GetValue("antennaEnabled"), out isOn);
                return success && isOn && base.CanCommUnloaded(mSnap);
            }
            else
            {
                return base.CanCommUnloaded(mSnap);
            }
        }

        public virtual bool CanRelay()
        {
            return allowRelay && relayEnabled;
        }

        public virtual bool CanRelayUnloaded(ProtoPartModuleSnapshot mSnap)
        {
            if (mSnap == null)
            {
                return false;
            }
            if (mSnap.moduleValues.HasValue("allowRelay") && mSnap.moduleValues.HasValue("relayEnabled"))
            {
                bool
                    allowed = false,
                    enabled = false,
                    success = false;
                if (bool.TryParse(mSnap.moduleValues.GetValue("allowRelay"), out allowed))
                {
                    success = bool.TryParse(mSnap.moduleValues.GetValue("relayEnabled"), out enabled);
                }
                return success && allowed && enabled && base.CanCommUnloaded(mSnap);
            }
            else
            {
                return false;
            }
        }

        public void FixedUpdate()
        {
            CheckDeployed();
            ProcessPower();
            UpdateStatus();
        }

        public void Update()
        {
            if (busy)
            {
                timeElapsed += TimeWarp.deltaTime;
            }
            if (incompleteAllowed != xmitIncomplete)
            {
                TransmitIncompleteToggle();
            }
        }

        protected override IEnumerator transmitQueuedData(float transmitInterval, float dataPacketSize, Callback callback = null, bool sendData = true)
        {
            busy = true;
            timeElapsed = 0f;
            Events["StopTransmission"].active = true;

            while (transmissionQueue.Any() && !xmitAborted)
            {
                var dataThrough = 0.0f;
                var progress = 0.0f;

                var scienceData = transmissionQueue[0];
                var dataAmount = (float)scienceData.dataAmount;

                scienceData.triggered = true;

                statusMessage.message = string.Format("[{0}]: Starting Transmission of {1}", part.partInfo.title, scienceData.title);
                ScreenMessages.PostScreenMessage(statusMessage);

                var subject = ResearchAndDevelopment.GetSubjectByID(scienceData.subjectID);
                if (subject == null)
                {
                    AbortTransmission(string.Format("[{0}]: Unable to identify science subjectID:{1}!", part.partInfo.title, scienceData.subjectID));
                }
                if (ResearchAndDevelopment.Instance != null)
                {
                    commStream = new RnDCommsStream(subject, scienceData.dataAmount, 1.0f,
                        scienceData.baseTransmitValue * scienceData.transmitBonus,
                        xmitIncomplete, ResearchAndDevelopment.Instance);
                }
                else
                {
                    AbortTransmission("Could not find Research and Development facility!");
                }
                statusText = "Transmitting";
                while (dataThrough < dataAmount && !xmitAborted)
                {
                    yield return new WaitForFixedUpdate();
                    var pushData = (float)(TimeWarp.fixedDeltaTime * transmitDataRate);
                    commStream.StreamData(pushData, vessel.protoVessel);
                    dataThrough += pushData;
                    progress = dataThrough / dataAmount;
                    statusText = string.Format("Transmitting ({0:P0})", progress);
                    if (timeElapsed >= showProgressInterval)
                    {
                        progressMessage.message = string.Format("[{0}]: Transmission progress: {1:P0}", part.partInfo.title, progress);
                        ScreenMessages.PostScreenMessage(progressMessage);
                        timeElapsed -= showProgressInterval;
                    }
                }
                if (dataThrough < dataAmount && dataThrough > 0 && xmitIncomplete)
                {
                    statusMessage.message = string.Format("[{0}]: <color=orange>Partial</color> transmission of {1} completed", part.partInfo.title, scienceData.title);
                    GameEvents.OnTriggeredDataTransmission.Fire(scienceData, vessel, false);
                    transmissionQueue.RemoveAt(0);
                }
                if (dataThrough >= dataAmount)
                {
                    GameEvents.OnTriggeredDataTransmission.Fire(scienceData, vessel, false);
                    transmissionQueue.RemoveAt(0);
                    statusMessage.message = string.Format("[{0}]: Transmission of {1} completed", part.partInfo.title, scienceData.title);
                    ScreenMessages.PostScreenMessage(statusMessage);
                }
            }

            if (xmitAborted && transmissionQueue.Any())
            {
                statusMessage.message = string.Format("[{0}]: Returning unsent data.", part.partInfo.title);
                ScreenMessages.PostScreenMessage(statusMessage);
                foreach (var data in transmissionQueue)
                {
                    ReturnDataToContainer(data);
                }
            }
            timeElapsed = 0f;
            Events["StopTransmission"].active = false;
            busy = false;
            statusText = "Idle";
            xmitAborted = false;

            if (callback != null)
            {
                callback.Invoke();
            }
        }

        [KSPAction(guiName = "Toggle antenna", actionGroup = KSPActionGroup.None)]
        public virtual void ToggleAntennaAction(KSPActionParam param)
        {
            SetAntennaState(!antennaEnabled);
        }

        [KSPEvent(guiName = "Toggle antenna")]
        public void ToggleAntenna()
        {
            SetAntennaState(!antennaEnabled);
        }

        [KSPAction(guiName = "Enable antenna", actionGroup = KSPActionGroup.None)]
        public void EnableAntennaAction(KSPActionParam param)
        {
            SetAntennaState(true);
        }

        [KSPEvent(guiName = "Enable antenna")]
        public void EnableAntenna()
        {
            SetAntennaState(true);
        }

        [KSPAction(guiName = "Disable antenna", actionGroup = KSPActionGroup.None)]
        public void DisableAntennaAction(KSPActionParam param)
        {
            SetAntennaState(false);
        }

        [KSPEvent(guiName = "Disable antenna")]
        public void DisableAntenna()
        {
            SetAntennaState(false);
        }

        protected virtual void SetAntennaState(bool state)
        {
            if (state != antennaEnabled)
            {
                antennaEnabled = state;
                Events["StartTransmission"].active = state;
                Events["StartTransmission"].guiActive = state;
                Events["StartTransmission"].guiActiveEditor = state;
            }
        }

        private void ProcessPower()
        {
            if (shouldConsume)
            {
                if (antennaEnabled)
                {
                    var resErrorMsg = "";
                    var resAvailable = 1.0d;
                    if (busy)
                    {
                        resAvailable = resHandler.UpdateModuleResourceInputs(ref resErrorMsg, transmitConsumptionRate / telemetryConsumptionRate, 0.99, true, false, true);
                        if (resAvailable < 0.99)
                        {
                            AbortTransmission(resErrorMsg);
                        }
                    }
                    if (!busy || resAvailable < 0.99)
                    {
                        resAvailable = resHandler.UpdateModuleResourceInputs(ref resErrorMsg, 1.0, 0.99, true, false, true);
                        if (resAvailable < 0.99)
                        {
                            antennaEnabled = false;
                            errorMessage.message = string.Format("[{0}]: Antenna shutting down, {1}", part.partInfo.title, resErrorMsg);
                            ScreenMessages.PostScreenMessage(errorMessage);
                        }
                    }
                }
                else if (busy)
                {
                    AbortTransmission("Antenna disabled!");
                }
            }
        }

        private void CheckDeployed()
        {
            if (isDeployable)
            {
                var res = true;
                for (var i = 0; i < antennasToDeploy.Count; i++)
                {
                    if (antennasToDeploy[i].GetScalar < 0.99f)
                    {
                        res = false;
                        break;
                    }
                }
                SetAntennaState(res);
            }
        }

        private void UpdateStatus()
        {
            if (!busy)
            {
                if (antennaEnabled)
                {
                    statusText = "Enabled";
                }
                else
                {
                    statusText = "Disabled";
                }
            }
        }
    }
}

// Debug.Log(string.Format("[ModuleRTDataTransmitter] var={0}", var));