using System;
using RemoteTech.Common.Interfaces;
using RemoteTech.Common.Utils;

namespace RemoteTech.Common.Settings
{
    public abstract class RemoteTechAbstractSettings : IRemoteTechSettings
    {
        public abstract string ConfigNodeName { get; }
        public abstract string DefaultSettingCfgUrl { get; }

        public abstract bool FirstStart { get; set; }

        public abstract string SaveFileFullPath { get; }
        public abstract string SaveFileName { get; }

        public abstract bool SettingsLoaded { get; set; }

        public bool CanSave => !((HighLogic.CurrentGame == null) || GameUtil.IsGameScenario);

        public abstract event Action<Game.Modes> OnSettingsLoadGameMode;

        public bool Save()
        {
            try
            {
                if (!CanSave)
                    return false;

                var configNode = new ConfigNode(ConfigNodeName);
                ConfigNode.CreateConfigFromObject(this, 0, configNode);

                var saveconfigNode = new ConfigNode();
                saveconfigNode.AddNode(configNode);
                saveconfigNode.Save(SaveFileFullPath);

                //TODO: fire settings saved event
            }
            catch (Exception ex)
            {
                Logging.Error($"An error occurred while attempting to save the setting file '{SaveFileFullPath}':\n\tError:{ex}");
                return false;
            }

            return true;
        }

        public IRemoteTechSettings Load(IRemoteTechSettings settings)
        {
            var defaultSuccess = false;

            // Exploit KSP's GameDatabase to find our MM-patched cfg of default settings (from GameData/RemoteTech/Default_Settings.cfg)
            var cfgs = GameDatabase.Instance.GetConfigs(ConfigNodeName);
            foreach (var urlConfig in cfgs)
            {
                if (!urlConfig.url.Equals(DefaultSettingCfgUrl))
                    continue;

                defaultSuccess = ConfigNode.LoadObjectFromConfig(settings, urlConfig.config);
                Logging.Info($"Load default settings into object with: {urlConfig.config}; LOADED: {defaultSuccess}");
                break;
            }

            if (!defaultSuccess) // disable itself and write explanation to KSP's log
            {
                Logging.Error(
                    $"RemoteTech is disabled because the default configuration file '{DefaultSettingCfgUrl}' was not found");
                return null;
                // the main impact of returning null is the endless loop of invoking Load() in the KSP's loading screen
            }

            settings.SettingsLoaded = true;

            // stop and return default settings if we are on the KSP loading screen OR in training scenarios
            if (string.IsNullOrEmpty(SaveFileFullPath))
                return settings;

            // try to load from the save-settings.cfg (MM-patches will not touch because it is outside GameData)
            var loadedNode = ConfigNode.Load(SaveFileFullPath);
            if (loadedNode == null)
            {
                // write the RT settings to the player's save folder
                if (!settings.Save())
                {
                    Logging.Error("Couldn't save settings for Load().");
                    settings.SettingsLoaded = false;
                    return null;
                }

                settings.FirstStart = true;
            }
            else
            {
                loadedNode = loadedNode.GetNode(ConfigNodeName);

                // replace the default settings with save-setting file
                var success = ConfigNode.LoadObjectFromConfig(settings, loadedNode);
                Logging.Info($"Found and load save settings into object with {loadedNode}: LOADED {success}");
            }

            //TODO load presets
            //TODO fire settings loaded event

            return settings;
        }
    }
}