using System.IO;
using System.Reflection;
using UnityEngine;

namespace RemoteTech.Common.Utils
{
    public static class UiUtils
    {
        /// <summary>
        ///     Load a texture by its filename (without extension).
        /// </summary>
        /// <param name="fileName">The file name of the texture (without file extension!)</param>
        /// <returns>The <see cref="Texture2D" /> object if the texture file is found, otherwise a black texture.</returns>
        public static Texture2D LoadTexture(string fileName)
        {
            var caller = Assembly.GetCallingAssembly();
            var textureDirectory = PathUtils.TextureDirectory(caller);
            if (string.IsNullOrEmpty(textureDirectory))
                return Texture2D.blackTexture;

            var textureFileName = PathUtils.CanonicalizePathToKspPath(Path.Combine(textureDirectory, fileName));
            if (GameDatabase.Instance.ExistsTexture(textureFileName))
                return GameDatabase.Instance.GetTexture(textureFileName, false);

            Logging.Error($"Cannot Find Texture: {textureFileName}");
            return Texture2D.blackTexture;
        }
    }
}