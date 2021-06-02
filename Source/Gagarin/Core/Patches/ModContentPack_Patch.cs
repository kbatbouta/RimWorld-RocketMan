using System;
using System.Collections.Generic;
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
                __instance.LoadPatches();

                Context.CurrentLoadingMod = null;
            }
        }
    }
}
