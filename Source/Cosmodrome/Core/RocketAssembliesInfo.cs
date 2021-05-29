using System;
using System.Collections.Generic;
using System.Reflection;

namespace RocketMan
{
    public static class RocketAssembliesInfo
    {
        private static string versionString;

        private static HashSet<Assembly> assemblies = new HashSet<Assembly>();

        public static string Version
        {
            get
            {
                if (versionString != null)
                    return versionString;
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                versionString = $"{version.Major}" +
                    $".{version.Minor}" +
                    $".{version.Build}" +
                    $".{version.Revision}";
                return versionString;
            }
        }

        public static HashSet<Assembly> Assemblies
        {
            get
            {
                Assembly mainAssembly = typeof(RocketPrefs).Assembly;
                if (!assemblies.Contains(mainAssembly)) assemblies.Add(mainAssembly);
                return assemblies;
            }
        }
    }
}
