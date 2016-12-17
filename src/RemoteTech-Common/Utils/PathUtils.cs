using System.IO;
using System.Reflection;

namespace RemoteTech.Common.Utils
{
    /// <summary>
    ///     Class managing and handling everything related to paths in plug-ins.
    /// </summary>
    public static class PathUtils
    {
        public static string TextureDirName = "Textures";

        /// <summary>
        ///     Automatically finds the proper texture directory from the assembly location. Assumes the assembly is in the
        ///     proper location of GameData/RemoteTech/Plugins.
        /// </summary>
        /// <returns>The texture directory path (as a string) if found otherwise a null reference.</returns>
        private static string TextureDirectory
        {
            get
            {
                var location = Assembly.GetCallingAssembly().Location;
                if (string.IsNullOrEmpty(location))
                {
                    Logging.Error("Couldn't get executing assembly location.");
                    return null;
                }

                var parentLocation = Directory.GetParent(location).Parent;
                if (parentLocation != null)
                    return
                        $"{parentLocation.Name}{Path.DirectorySeparatorChar}{TextureDirName}{Path.DirectorySeparatorChar}";

                Logging.Error("TextureDirectory: cannot Find parent location");
                return null;
            }
        }
    }
}