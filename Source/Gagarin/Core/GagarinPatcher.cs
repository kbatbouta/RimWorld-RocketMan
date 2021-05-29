using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Gagarin
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class GagarinPatch : IPatch
    {
        public GagarinPatch()
        {
        }

        public GagarinPatch(Type targetType, string targetMethod = null, MethodType methodType = MethodType.Normal, Type[] parameters = null, Type[] generics = null) : base(targetType, targetMethod, methodType, parameters, generics)
        {
        }
    }

    public class GagarinPatchInfo : IPatchInfo<GagarinPatch>
    {
        public override string PluginName => "Gagarin";
        public override string PatchTypeUniqueIdentifier => nameof(GagarinPatch);

        public GagarinPatchInfo(Type type) : base(type)
        {
        }

        public override void OnPatchingSuccessful(MethodBase replacement)
        {
            base.OnPatchingSuccessful(replacement);
            //
            //Log.Message($"GAGARIN: Patched {replacement}");
        }

        public override void OnPatchingFailed(Exception er)
        {
            base.OnPatchingFailed(er);
            Log.Error($"GAGARIN: Patching failed! {DeclaringType}");
        }
    }

    public class GagarinPatcher
    {
        public static GagarinPatchInfo[] patches = null;

        public readonly static Harmony harmony = new Harmony(Finder.HarmonyID + ".Gagarin");
    }
}
