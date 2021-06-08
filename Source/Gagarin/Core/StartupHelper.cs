using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class StartupHelper
    {
        [Main.OnInitialization]
        public static void StartUpStarted()
        {
            Context.RunningMods = LoadedModManager.RunningMods.ToList();
            Context.Core = LoadedModManager.RunningMods.First(m => m.IsCoreMod);
            Context.IsUsingCache = false;

            Log.Message("GAGARIN: <color=green>StartUpStarted called!</color>");

            if (GagarinEnvironmentInfo.CacheExists)
            {
                Log.Warning("GAGARIN: <color=green>Cache found</color>");

                Context.IsUsingCache = true;

                if (GagarinEnvironmentInfo.ModListChanged)
                {
                    Log.Warning("GAGARIN: Mod list changed! Deleting cache");

                    if (File.Exists(GagarinEnvironmentInfo.ModListFilePath))
                        File.Delete(GagarinEnvironmentInfo.ModListFilePath);
                    Context.IsUsingCache = false;
                }
            }
            else if (!Directory.Exists(GagarinEnvironmentInfo.CacheFolderPath))
            {
                Directory.CreateDirectory(GagarinEnvironmentInfo.CacheFolderPath);
            }

            Log.Warning("GAGARIN: <color=green>Cache not found!</color>");

            RunningModsSetUtility.Dump(Context.RunningMods, GagarinEnvironmentInfo.ModListFilePath);
        }

        [Main.OnStaticConstructor]
        public static void StartUpFinished()
        {
            Log.Message("GAGARIN: <color=green>StartUpFinished called!</color>");

            Context.AssetsHashes.Clear();
            Context.DefsXmlAssets.Clear();
            Context.XmlAssets.Clear();
            Context.CurrentLoadingMod = null;

            CachedDefHelper.Clean();
        }
    }
}
