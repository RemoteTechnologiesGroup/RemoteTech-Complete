using System;
using System.Collections.Generic;
using System.Reflection;

namespace RemoteTech.Common.Api
{
    public static class RemoteTechModules
    {
        /// <summary>
        ///     Name of the <b>RemoteTech-Delay</b> assembly.
        /// </summary>
        public const string RemoteTechDelayAssemblyName = "RemoteTech-Delay";

        /// <summary>
        ///     Cache for loaded assemblies.
        ///     <list type="bullet">
        ///         <item>
        ///             <description>key is the assembly name.</description>
        ///         </item>
        ///         <item>
        ///             <description>value is a boolean indicating whether the assembly is loaded (true) or not (false).</description>
        ///         </item>
        ///     </list>
        /// </summary>
        private static readonly Dictionary<string, bool> RemoteTechAssembliesLoadedCache =
            new Dictionary<string, bool>();

        /// <summary>
        ///     Cache for assembly types.
        ///     <list type="bullet">
        ///         <item>
        ///             <description>key is the assembly name.</description>
        ///         </item>
        ///         <item>
        ///             <description>value is a dictionary.</description>
        ///             <list type="bullet">
        ///                 <item>
        ///                     <description>key is a type.</description>
        ///                 </item>
        ///                 <item>
        ///                     <description>
        ///                         value is a list of types used for the constructor of the type (similar to
        ///                         Type.GetConstructor() arguments)
        ///                     </description>
        ///                 </item>
        ///             </list>
        ///         </item>
        ///     </list>
        /// </summary>
        private static readonly Dictionary<string, Dictionary<Type, List<Type>>> RemoteTechInterfaceCache =
            new Dictionary<string, Dictionary<Type, List<Type>>>();

        /// <summary>
        ///     Gets whether or not <b>RemoteTech-Delay</b> is loaded or not.
        /// </summary>
        public static bool RemoteTechDelayAssemblyLoaded => IsRemoteTechAssemblyLoaded(RemoteTechDelayAssemblyName);

        /// <summary>
        ///     Get a <see cref="AssemblyLoader.LoadedAssembly" /> by its assembly name.
        /// </summary>
        /// <param name="assemblyName">The assembly to search for?</param>
        /// <returns>
        ///     The <see cref="AssemblyLoader.LoadedAssembly" /> if an assembly with the corresponding
        ///     <paramref name="assemblyName" /> is found. null otherwise.
        /// </returns>
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

        /// <summary>
        ///     Given a type parameter, return a constructed object (an instance of the type) from it.
        /// </summary>
        /// <typeparam name="T">The type parameter to search for.</typeparam>
        /// <param name="assemblyName">The assembly in which to search for the type parameter.</param>
        /// <param name="constructorTypes">
        ///     constructor parameter (list of types). Use <see cref="Type.EmptyTypes" /> if the
        ///     constructor takes no parameter.
        /// </param>
        /// <returns>A constructed object.</returns>
        public static T GetObjectFromInterface<T>(string assemblyName, Type[] constructorTypes) where T : class
        {
            //
            var constructor = GetFirstInterfaceFromAssembly<T>(assemblyName, constructorTypes);
            if (constructor == null)
                return null;

            // check if the type implementing the interface is a singleton
            var propertyInfo = constructor.DeclaringType?.GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var instance = propertyInfo?.GetValue(null, null);
            if (instance != null)
                return instance as T;

            var @object = constructor?.Invoke(null) as T;
            return @object;
        }

        /// <summary>
        ///     Given a type parameter, an assembly name and constructor parameters for the type, return the first type found that
        ///     is implementing the given type parameter.
        /// </summary>
        /// <typeparam name="T">The type to search for (one of the type in the assembly must implement this type).</typeparam>
        /// <param name="assemblyName">The assembly name in which to search the type parameter.</param>
        /// <param name="constructorTypes">
        ///     A list of type given to the constructor implemented by the type parameter. Use
        ///     <see cref="Type.EmptyTypes" /> if the constructor takes no parameter.
        /// </param>
        /// <returns>The <see cref="ConstructorInfo" /> corresponding to the type if one is found. null otherwise.</returns>
        public static ConstructorInfo GetFirstInterfaceFromAssembly<T>(string assemblyName, Type[] constructorTypes)
            where T : class
        {
            // Get all types implementing 'T' in assembly (with name 'assemblyName')
            var typeList = GetTypesImplementingInterfaceFromAssembly<T>(assemblyName);

            // loop through those types.
            var typeCount = typeList.Count;
            for (var i = 0; i < typeCount; i++)
            {
                // check if the given type has a constructor with the required parameters
                var constructor = typeList[i].GetConstructor(constructorTypes);
                if (constructor != null)
                    return constructor;
            }

            return null;
        }

        /// <summary>
        ///     Given an assembly name and a type, get all types implementing the type parameter.
        /// </summary>
        /// <typeparam name="T">The type to search for in the assembly.</typeparam>
        /// <param name="assemblyName">The assembly in which to search for the type.</param>
        /// <returns>Return a list of all the types implementing the type parameter in the given assembly.</returns>
        public static List<Type> GetTypesImplementingInterfaceFromAssembly<T>(string assemblyName) where T : class
        {
            // Get the loaded assembly by its name
            var loadedAssembly = AssemblyByName(assemblyName);
            if (loadedAssembly == null)
                return null;

            // the interface we are searching for
            var interfaceType = typeof(T);

            /* use the type cache */

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

        /// <summary>
        ///     Given an assembly name, check if the assembly has been loaded by KSP.
        /// </summary>
        /// <param name="assemblyName">The assembly name to check (note: without extension).</param>
        /// <returns>true if the <paramref name="assemblyName" /> is loaded, false otherwise.</returns>
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