using System;
using System.IO;
using Verse;

namespace RocketMan
{
    public static class RocketEnvironmentInfo
    {
        private static bool isDevEnvInitialized = false;

        private static bool isDevEnv = false;

        private const string PluginFolderSubPath = "1.2/Plugins";

        private const string CustomConfigSubPath = "Custom/RocketMan/";

        public static bool IsDevEnv
        {
            get
            {
                if (!isDevEnvInitialized)
                {
                    string path = Path.GetFullPath(Path.Combine(GenFilePaths.ConfigFolderPath, "rocketeer.0102.txt"));
                    Log.Message($"ROCKETMAN: config path {path}");
                    isDevEnvInitialized = true;
                    isDevEnv = File.Exists(path);
                    if (isDevEnv)
                    {
                        Log.Warning($"ROCKETMAN: dev environment detected!");
                    }
                }
                return isDevEnv;
            }
        }

        public static string DevKeyFilePath
        {
            get => Path.GetFullPath(Path.Combine(GenFilePaths.ConfigFolderPath, "rocketeer.0102.txt"));
        }

        public static string PluginsFolderPath
        {
            get => Path.Combine(Finder.ModContentPack.RootDir, PluginFolderSubPath);
        }

        public static string ConfigFolderPath
        {
            get => GenFilePaths.ConfigFolderPath;
        }

        public static string CustomConfigFolderPath
        {
            get => Path.Combine(GenFilePaths.ConfigFolderPath, CustomConfigSubPath);
        }

        public static bool IncompatibilityUnresolved = false;

        public static bool SoyuzLoaded = false;

        public static bool ProtonLoaded = false;

        public static bool RocketeerLoaded = false;

        public static bool GagarinLoaded = false;
    }
}
