using HarmonyLib;
using RocketMan;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(CameraDriver), nameof(CameraDriver.Update))]
    public class CameraDriver_Patch
    {
        public static void Postfix(CameraDriver __instance)
        {
            Context.zoomRange = __instance.CurrentZoom;
            Context.curViewRect = __instance.CurrentViewRect;
            if (RocketDebugPrefs.Debug && RocketDebugPrefs.StatLogging)
                Log.Message($"SOYUZ: Zoom range is {Context.zoomRange}");
        }
    }
}