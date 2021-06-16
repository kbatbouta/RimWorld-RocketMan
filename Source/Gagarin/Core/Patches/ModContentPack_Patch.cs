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
                if (!Context.IsRecovering)
                    Context.CurrentLoadingMod = __instance;
            }

            public static void Postfix(ModContentPack __instance)
            {
                if (!Context.IsRecovering)
                {
                    if (Context.IsUsingCache)
                    {
                        __instance.LoadPatches();
                    }

                    Context.CurrentLoadingMod = null;
                }
            }
        }
    }
}
