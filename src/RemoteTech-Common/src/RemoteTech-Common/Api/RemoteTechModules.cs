using System;
using System.Collections.Generic;
using System.Reflection;

namespace RemoteTech.Common.Api
{
    public static class RemoteTechModules
    {
        public const string RemoteTechDelayAssemblyName = "RemoteTech-Delay";

        private static readonly Dictionary<string, bool> RemoteTechAssembliesLoadedCache =
            new Dictionary<string, bool>();

        private static readonly Dictionary<string, Dictionary<Type, List<Type>>> RemoteTechInterfaceCache =
            new Dictionary<string, Dictionary<Type, List<Type>>>();

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

        public static T GetObjectFromInterface<T>(string assemblyName, Type[] constructorTypes) where T : class
        {
            var constructor = GetFirstInterfaceFromAssembly<T>(assemblyName, constructorTypes);
            var @object = constructor?.Invoke(null) as T;
            return @object;
        }


        public static ConstructorInfo GetFirstInterfaceFromAssembly<T>(string assemblyName, Type[] constructorTypes)
            where T : class
        {
            var typeList = GetTypesImplementingInterfaceFromAssembly<T>(assemblyName);
            var typeCount = typeList.Count;
            for (var i = 0; i < typeCount; i++)
            {
                // check if the type has a constructor with the required parameters
                var constructor = typeList[i].GetConstructor(constructorTypes);
                if (constructor != null)
                    return constructor;
            }

            return null;
        }

        public static List<Type> GetTypesImplementingInterfaceFromAssembly<T>(string assemblyName) where T : class
        {
            // Get the loaded assembly by its name
            var loadedAssembly = AssemblyByName(assemblyName);
            if (loadedAssembly == null)
                return null;

            // the interface we are searching for
            var interfaceType = typeof(T);

            /* use the cache */

            Dictionary<Type, List<Type>> typeDict;
            List<Type> typeList;

            // check if the assembly is in the dictionary
            if (RemoteTechInterfaceCache.TryGetValue(assemblyName, out typeDict))
            {
                // check if the dictionary as the required type
                if (typeDict.TryGetValue(typeof(T), out typeList))
                    return typeList;

                typeList = new List<Type>();
                typeDict[typeof(T)] = typeList;
            }
            else
            {
                typeList = new List<Type>();
                RemoteTechInterfaceCache[assemblyName] = new Dictionary<Type, List<Type>>
                {
                    [typeof(T)] = typeList
                };
            }

            /* end cache */

            // get all the types in the given assembly
            var types = loadedAssembly.assembly.GetTypes();

            // Go through each type in the assembly
            var numberOfTypes = types.Length;
            for (var typeIndex = 0; typeIndex < numberOfTypes; typeIndex++)
            {
                // for a given type, get all its interfaces
                var interfaces = types[typeIndex].GetInterfaces();

                // loop through all interfaces implemented by the type
                var numberOfInterfaces = interfaces.Length;
                for (var interfaceIndex = 0; interfaceIndex < numberOfInterfaces; interfaceIndex++)
                {
                    // check if the interface corresponds to the searched interface
                    if (interfaces[interfaceIndex] != interfaceType)
                        continue;

                    // save the type
                    typeList.Add(types[typeIndex]);
                    break;
                }
            }
            return typeList;
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