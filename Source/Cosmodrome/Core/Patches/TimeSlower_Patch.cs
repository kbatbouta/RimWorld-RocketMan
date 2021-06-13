using HarmonyLib;
using Verse;

namespace RocketMan.Patches
{
    [RocketPatch(typeof(TimeSlower), nameof(TimeSlower.SignalForceNormalSpeed))]
    public class TimeSlower_SignalForceNormalSpeed_Patch
    {
        public static bool Prefix() => !(RocketPrefs.Enabled && RocketPrefs.DisableForcedSlowdowns);
    }

    [RocketPatch(typeof(TimeSlower), nameof(TimeSlower.SignalForceNormalSpeedShort))]
    public class TimeSlower_SignalForceNormalSpeedShort_Patch
    {
        public static bool Prefix() => !(RocketPrefs.Enabled && RocketPrefs.DisableForcedSlowdowns);
    }
}