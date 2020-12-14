using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Astrarium.Plugins.ASCOM
{
    /// <summary>
    /// Provides singleton instance to access to proxy for ASCOM platform
    /// </summary>
    public static class Ascom
    {
        /// <summary>
        /// List of ASCOM assemblies names to search for in Global Assembly Cache (GAC)
        /// </summary>
        private static readonly string[] ASCOMAssemblies = new string[] { "ASCOM.DriverAccess" };

        /// <summary>
        /// Proxy instance, backing field for singleton
        /// </summary>
        private static IAscomProxy proxy = null;

        /// <summary>
        /// Locker object for thread-safety
        /// </summary>
        private static object locker = new object();

        /// <summary>
        /// Gets instance of ASCOM proxy
        /// </summary>
        public static IAscomProxy Proxy
        {
            get
            {
                lock (locker)
                {
                    if (proxy == null)
                    {
                        if (ASCOMAssemblies.All(a => IsAssemblyInGAC(a)))
                            proxy = new AscomProxy();
                        else
                            proxy = new NoAscomProxy();
                    }
                    return proxy;
                }
            }
        }

        /// <summary>
        /// Checks is the assembly with provided name found in Global Assembly Cache.
        /// The ASCOM platform assemblies always installed in GAC, so look for them there.
        /// </summary>
        /// <param name="name">Name of assembly to check.</param>
        /// <returns>True if the assembly is found in GAC.</returns>
        private static bool IsAssemblyInGAC(string name)
        {
            try
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetReferencedAssemblies().FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (assemblyName == null) return false;
                var assembly = Assembly.ReflectionOnlyLoad(assemblyName.FullName);
                return assembly.GlobalAssemblyCache;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error on checking assembly in GAC: {ex}");
                return false;
            }
        }
    }
}
