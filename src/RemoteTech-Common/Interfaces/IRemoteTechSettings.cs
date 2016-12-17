using System;

namespace RemoteTech.Common.Interfaces
{
    public interface IRemoteTechSettings
    {
        /*
         * Member Fields
         */

        /// <summary>
        ///     Name and extension of the save file (<b>without</b> full path).
        /// </summary>
        string SaveFileName { get; }

        /// <summary>
        ///     Full path of the save file.
        /// </summary>
        string SaveFileFullPath { get; }

        /// <summary>
        /// Indicates whether the settings are loaded or not.
        /// </summary>
        bool SettingsLoaded { get; set; }

        /// <summary>
        ///     Default settings file, relative to the GameData/RemoteTech folder.
        /// </summary>
        string DefaultSettingCfgUrl { get; }

        /// <summary>
        ///     Name of the <see cref="ConfigNode" /> used by this save.
        /// </summary>
        string ConfigNodeName { get; }

        /// <summary>
        ///     True if its the first start of RemoteTech for this save, false otherwise.
        /// </summary>
        bool FirstStart { get; set; }

        /// <summary>
        ///  Indicates whether or not it is possible to save the settings.
        /// </summary>
        bool CanSave { get;  }

        event Action<Game.Modes> OnSettingsLoadGameMode;

        /*
         * Member Functions
         */

        /// <summary>
        ///     Saves the current setting object to the save file (<seealso cref="SaveFileName" /> and
        ///     <seealso cref="SaveFileFullPath" />).
        /// </summary>
        bool Save();

        /// <summary>
        ///     Load the current settings.
        /// </summary>
        IRemoteTechSettings Load(IRemoteTechSettings settings);
    }
}