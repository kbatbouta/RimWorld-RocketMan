﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Proton
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ProtonPatch : IPatch
    {
        public ProtonPatch()
        {
        }

        public ProtonPatch(Type targetType, string targetMethod = null, MethodType methodType = MethodType.Normal, Type[] parameters = null, Type[] generics = null) : base(targetType, targetMethod, methodType, parameters, generics)
        {
        }
    }

    public class ProtonPatchInfo : IPatchInfo<ProtonPatch>
    {
        public override string PluginName => "PROTON";
        public override string PatchTypeUniqueIdentifier => nameof(ProtonPatch);

        public ProtonPatchInfo(Type type) : base(type)
        {
        }
    }

    public class ProtonPatcher
    {
        public static ProtonPatchInfo[] patches = null;

        private readonly static Harmony harmony = new Harmony(Finder.HarmonyID + ".PROTON");

        [Main.OnDefsLoaded]
        public static void PatchAll()
        {
            foreach (var patch in patches)
                patch.Patch(harmony);
            Log.Message($"PROTON: Patching finished");
            RocketEnvironmentInfo.ProtonLoaded = true;
        }

        [Main.OnInitialization]
        public static void Intialize()
        {
            IEnumerable<Type> flaggedTypes = GetPatches();
            List<ProtonPatchInfo> patchList = new List<ProtonPatchInfo>();
            foreach (Type type in flaggedTypes)
            {
                ProtonPatchInfo patch = new ProtonPatchInfo(type);
                patchList.Add(patch);
                if (RocketDebugPrefs.Debug) Log.Message($"PROTON: found patch in {type} and is {(patch.IsValid ? "valid" : "invalid") }");
            }
            patches = patchList.Where(p => p.IsValid).ToArray();
        }

        private static IEnumerable<Type> GetPatches()
        {
            return typeof(ProtonPatcher).Assembly.GetLoadableTypes().Where(t => t.HasAttribute<ProtonPatch>());
        }
    }
}
