using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RocketMan.Optimizations
{
#if !DEBUG
    [RocketPatch]
    public class GraphicConsts_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(SectionLayer), nameof(SectionLayer.DrawLayer));
            foreach (var subClass in typeof(SectionLayer).AllSubclassesNonAbstract())
            {
                var method = AccessTools.Method(subClass, nameof(SectionLayer.DrawLayer));
                if (method != null && method.HasMethodBody() && !method.IsVirtual) yield return method;
            }

            foreach (var method in typeof(Gizmos).GetMethods())
                if (method != null && method.HasMethodBody() && !method.IsVirtual)
                    yield return method;
            foreach (var method in typeof(GUIUtility).GetMethods())
                if (method != null && method.HasMethodBody() && !method.IsVirtual)
                    yield return method;
            foreach (var method in typeof(Graphic).GetMethods())
                if (method != null && method.HasMethodBody() && !method.IsVirtual)
                    yield return method;
            foreach (var method in typeof(Graphics).GetMethods())
                if (method != null && method.HasMethodBody() && !method.IsVirtual)
                    yield return method;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return TranspilerUtility.FixConsts(instructions);
        }
    }
#endif
}