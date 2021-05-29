using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RocketMan
{
    public static class IgnoreMeDatabase
    {
        private static bool[] defsIgnored = new bool[ushort.MaxValue];

        private static HashSet<string> defNames = new HashSet<string>();

        private static HashSet<string> packageIds = new HashSet<string>();

        private static Dictionary<Def, string> reportlookup = new Dictionary<Def, string>();

        private static string report = "ROCKETMAN: <color=red>IgnoreMe report</color>";

        public static bool ShouldIgnore(Def def) => defsIgnored[def.index];

        public static void Add(Def def, string reason)
        {
            report += $"\nROCKETRULES: IgnoreMe add def by name: { def.defName }";

            defsIgnored[def.index] = true;
            reportlookup[def] = reason;
        }

        public static void Add(string defName)
        {
            defNames.Add(defName);
        }

        public static void AddPackageId(string packageId)
        {
            packageIds.Add(packageId.ToLower());

            Log.Message($"ROCKETRULES: IgnoreMeRule for { packageId }");
        }

        public static string Report(Def def)
        {
            return reportlookup.TryGetValue(def, out string report) ? report : string.Empty;
        }

        public static void ParsePrepare()
        {
            try
            {
                // foreach (ModContentPack mod in LoadedModManager.RunningMods)
                // {
                //    if (!packageIds.Any(id => ComparePackageIds(id, mod)))
                //    {
                //        continue;
                //    }
                //    foreach (Def def in mod.AllDefs)
                //    {
                //        if (!(def is ThingDef thingDef && thingDef.race != null && thingDef.thingClass != typeof(Pawn)))
                //        {
                //            Add(def, $"Ignored because of a compatibility consideration with " +
                //                $"<color=red>{mod.Name}</color>[<color=blue>{mod.PackageId}</color>]");
                //        }
                //    }
                // }

                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
                {
                    if (thingDef.race == null)
                    {
                        continue;
                    }
                    if (thingDef.thingClass != typeof(Pawn))
                    {
                        Add(thingDef, $"Ignored because of a custom thingClass { thingDef.thingClass.Name }");
                    }
                }
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETRULES: Parsing error! {er}");
            }
            finally
            {
                Log.Message(report);
            }
        }

        private static bool ComparePackageIds(string packageId, ModContentPack mod)
        {
            string other = mod.PackageId.ToLower();
            return !other.EndsWith("_steam")
                ? packageId.ToLower() == other : mod.PackageIdPlayerFacing.ToLower() == packageId.ToLower();
        }
    }
}
