using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class ModsConfig_Patch
    {
        [GagarinPatch(typeof(ModsConfig), nameof(ModsConfig.Reset))]
        public static class ModsConfig_Reset_Patch
        {
            public static bool Prefix()
            {
                if (Directory.Exists(GagarinEnvironmentInfo.CacheFolderPath))
                {
                    Directory.Delete(GagarinEnvironmentInfo.CacheFolderPath, recursive: true);
                    if (File.Exists(RocketEnvironmentInfo.DevKeyFilePath))
                        File.Delete(RocketEnvironmentInfo.DevKeyFilePath);

                    Logger.Debug("GAGARIN: Removed dev key to recover from error!");
                }
                return !RocketEnvironmentInfo.IsDevEnv;
            }
        }
    }
}
