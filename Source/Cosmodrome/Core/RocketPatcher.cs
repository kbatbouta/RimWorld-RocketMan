using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using Verse;

namespace RocketMan
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RocketPatch : IPatch
    {
        public RocketPatch()
        {
        }

        public RocketPatch(Type targetType, string targetMethod = null, MethodType methodType = MethodType.Normal, Type[] parameters = null, Type[] generics = null) : base(targetType, targetMethod, methodType, parameters, generics)
        {
        }
    }

    public class RocketPatchInfo : IPatchInfo<RocketPatch>
    {
        public override string PluginName => "ROCKETMAN";
        public override string PatchTypeUniqueIdentifier => nameof(RocketPatch);

        public RocketPatchInfo(Type type) : base(type)
        {
        }
    }

    [StaticConstructorOnStartup]
    public class RocketPatcher
    {
        public static RocketPatchInfo[] patches = null;

        public static void PatchAll()
        {
            foreach (var patch in patches)
                patch.Patch(Finder.Harmony);
            if (RocketDebugPrefs.Debug) Log.Message($"ROCKETMAN: Patching finished");
        }

        static RocketPatcher()
        {
            IEnumerable<Type> flaggedTypes = GetPatches();
            List<RocketPatchInfo> patchList = new List<RocketPatchInfo>();
            foreach (Type type in flaggedTypes)
            {
                RocketPatchInfo patch = new RocketPatchInfo(type);
                patchList.Add(patch);
                if (RocketDebugPrefs.Debug) Log.Message($"ROCKETMAN: Found patch in {type} and is {(patch.IsValid ? "valid" : "invalid") }");
            }
            patches = patchList.Where(p => p.IsValid).ToArray();
        }

        private static IEnumerable<Type> GetPatches()
        {
            return typeof(RocketPatcher).Assembly.GetLoadableTypes().Where(t => t.HasAttribute<RocketPatch>());
        }
    }
}