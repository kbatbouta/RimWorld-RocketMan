using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public static class PatchingUtility
    {
        public static bool IsValidTarget(this MethodBase method)
        {
            return method != null && !method.IsAbstract && method.DeclaringType == method.ReflectedType && method.HasMethodBody() && method.GetMethodBody()?.GetILAsByteArray()?.Length > 1;
        }

        public static string GetMethodPath(this MethodBase method)
        {
            return string.Format("{0}.{1}:{2}", method.DeclaringType.Namespace, method.ReflectedType.Name, method.Name);
        }

        public static IEnumerable<T> GetPatches<T, P>(Assembly assembly) where P : IPatch where T : IPatchInfo<P>
        {
            IEnumerable<Type> types = assembly.GetLoadableTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.HasAttribute<P>());
            foreach (var type in types)
            {
                T patchInfo = (T)Activator.CreateInstance(typeof(T), type);
                if (!patchInfo.IsValid)
                {
                    Log.Message($"{type} is not a valid patch!");
                    continue;
                }
                yield return patchInfo;
            }
        }
    }
}
