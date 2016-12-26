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
        ///     <para>
        ///         Automatically finds the proper texture directory from the assembly location. Assumes the assembly is in a
        ///         subdirectory, e.g.: /RemoteTech/RemoteTech-xxx/Plugins.
        ///     </para>
        ///     <para>
        ///         Assume the texture directory is located in /RemoteTech/RemoteTech-xxx/Textures.
        ///     </para>
        /// </summary>
        /// <param name="callingAssembly">The assembly searching for its texture directory.</param>
        /// <returns>The texture directory path (as a string) if found otherwise a null reference.</returns>
        internal static string TextureDirectory(Assembly callingAssembly)
        {
            // KSP uses a relative path to the top folder in /GameData, for example: 
            //    RemoteTech/RemoteTech-xxx/Textures/MyTexture
            // Also note that there's no file extension


            if (callingAssembly == null)
                return null;

            var location = callingAssembly.Location;
            if (string.IsNullOrEmpty(location))
            {
                Logging.Error("Couldn't get calling assembly location.");
                return null;
            }

            // Parent folder of the module which should be the top module folder, e.g: /RemoteTech-xxx
            var moduleDirNameInfo = Directory.GetParent(location).Parent;
            if (moduleDirNameInfo == null)
            {
                Logging.Error("TextureDirectory: cannot find parent location");
                return null;
            }

            // just check that the texture directory exists using its full path.
            var textureDirectoryFullPath = Path.Combine(moduleDirNameInfo.FullName, TextureDirName);
            if (!Directory.Exists(textureDirectoryFullPath))
            {
                Logging.Error($"Texture directory '{textureDirectoryFullPath}' doesn't exists.");
                return null;
            }

            //var test = Directory.GetParent(callingAssembly.Location).Parent.Parent.Name + "/Textures/";

            // Get top folder, which should be: /RemoteTech
            var remoteTechDirInfo = moduleDirNameInfo.Parent;
            if (remoteTechDirInfo == null)
            {
                Logging.Error("Couldn't get RemoteTech top level folder.");
                return null;
            }

            // Note that KSP uses a "local" directory path to reference its assets, not the full path, so we return this local path.
            // e.g: RemoteTech/RemoteTech-xxx/Textures
            var topAndChildDir = Path.Combine(remoteTechDirInfo.Name, moduleDirNameInfo.Name);
            var textureDirectory = Path.Combine(topAndChildDir, TextureDirName);

            // replace backslash (if any) to get a path that is understandable by KSP.
            textureDirectory = CanonicalizePathToKspPath(textureDirectory);

            return textureDirectory;
        }

        /// <summary>
        ///     Make a relative path understandable by KSP.
        /// </summary>
        /// <param name="path">The path to be converted.</param>
        /// <returns>The converted path.</returns>
        /// <remarks>This method simply replaces backslashes with slashes.</remarks>
        public static string CanonicalizePathToKspPath(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.Replace("\\", "/");
        }
    }
}