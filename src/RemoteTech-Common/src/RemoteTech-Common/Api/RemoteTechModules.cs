using System.Collections.Generic;

namespace RemoteTech.Common.Api
{
    public static class RemoteTechModules
    {
        public const string RemoteTechDelayAssemblyName = "RemoteTech-Delay.dll";

        private static readonly Dictionary<string, bool> RemoteTechAssembliesLoadedCache = new Dictionary<string, bool>();

        public static bool RemoteTechDelayAssemblyLoaded => IsRemoteTechAssemblyLoaded(RemoteTechDelayAssemblyName);

        internal static AssemblyLoader.LoadedAssembly AssemblyByName(string assemblyName)
        {
            var assemblyCount = AssemblyLoader.loadedAssemblies.Count;
            for (var i = 0; i < assemblyCount; i++)
            {
                var assembly = AssemblyLoader.loadedAssemblies[i];
                if (assembly.name == assemblyName)
                    return assembly;
            }

            return null;
        }

        internal static bool IsRemoteTechAssemblyLoaded(string assemblyName)
        {
            bool cache;
            if (RemoteTechAssembliesLoadedCache.TryGetValue(assemblyName, out cache))
                return cache;

            var result = AssemblyByName(assemblyName) != null;
            RemoteTechAssembliesLoadedCache[assemblyName] = result;

            return result;
        }
    }
}
