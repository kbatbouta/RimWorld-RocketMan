using System;
using System.Collections.Generic;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class ModContentPack_Patch
    {
        [GagarinPatch(typeof(ModContentPack), nameof(ModContentPack.LoadDefs))]
        public class ModContentPack_LoadDefs_Patch
        {
            public static void Prefix(ModContentPack __instance)
            {
                Context.CurrentLoadingMod = __instance;
            }

            public static void Postfix(ModContentPack __instance)
            {
                if (Context.IsUsingCache)
                {
                    //
                    // __instance.LoadPatches();
                }
                Context.CurrentLoadingMod = null;
            }
        }

        [GagarinPatch(typeof(ModContentPack), nameof(ModContentPack.LoadPatches))]
        public class ModContentPack_LoadPatches_Patch
        {
            public static bool Prefix(ModContentPack __instance)
            {
                Context.CurrentLoadingMod = __instance;
                Context.IsLoadingPatchXML = true;
                return !Context.IsUsingCache;
            }

            public static void Postfix(ModContentPack __instance)
            {

                if (Context.IsUsingCache)
                {
                    //
                    // __instance.LoadPatches();
                }
                Context.CurrentLoadingMod = null;
                Context.IsLoadingPatchXML = false;
            }
        }
    }
}
