using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using HarmonyLib;
using RocketMan;
using System.Xml;

namespace Gagarin
{
    public static class DirectXmlToObject_Patch
    {
        //[GagarinPatch]
        //public static class ObjectFromXml_Patch
        //{
        //    public static IEnumerable<MethodBase> TargetMethods()
        //    {
        //        yield return AccessTools.Method(typeof(DirectXmlToObject), nameof(DirectXmlToObject.ObjectFromXml), generics: new[] { typeof(Verse.PatchOperation) });
        //    }

        //    private static void Finalizer(XmlNode xmlRoot, Exception __exception)
        //    {
        //        try
        //        {
        //            if (__exception != null)
        //            {
        //                Log.Error($"GAGARIN: XmlNode failed to parse in  DirectXmlToObject:DirectXmlToObject\n{xmlRoot} {__exception}");
        //            }
        //        }
        //        finally
        //        {

        //        }
        //    }
        //}
    }
}
