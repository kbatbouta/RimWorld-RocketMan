using System;
using System.Linq;
using RimWorld;
using RocketMan;
using Verse;
using Verse.AI;

namespace Soyuz
{
    public static partial class SoyuzSettingsUtility
    {
        private static JobDef[] fullyThrottledJobs = null;

        private static JobDef[] partiallyThrottledJobs = null;

        private static JobDef[] notThrottledJobs = null;

        private static bool initialized = false;

        private static void PreparePresets()
        {
            if (fullyThrottledJobs == null)
            {
                fullyThrottledJobs = new JobDef[]
                {
                    JobDefOf.Clean,
                    JobDefOf.Sow,
                    JobDefOf.CutPlant,
                    JobDefOf.CutPlantDesignated,
                    JobDefOf.Harvest,
                    JobDefOf.HarvestDesignated,
                    JobDefOf.HaulToCell,
                    JobDefOf.Goto,
                    JobDefOf.LayDown,
                    JobDefOf.Follow,
                    JobDefOf.Wait,
                    JobDefOf.Wait_Wander,
                    JobDefOf.GotoWander,
                    JobDefOf.SocialRelax,
                    DefDatabase<JobDef>.defsByName.TryGetValue("GoForWalk", fallback: null)
                };
            }
            if (partiallyThrottledJobs == null)
            {
                partiallyThrottledJobs = new JobDef[]
                {
                    JobDefOf.Mine,
                    JobDefOf.Refuel,
                    JobDefOf.Follow,
                    JobDefOf.FollowClose,
                    JobDefOf.FinishFrame,
                    JobDefOf.DoBill,
                };
            }
            if (notThrottledJobs == null)
            {
                notThrottledJobs = new JobDef[]
                {
                    JobDefOf.Ingest,
                    JobDefOf.TakeInventory,
                    JobDefOf.DeliverFood,
                    JobDefOf.FeedPatient,
                    JobDefOf.Capture,
                    JobDefOf.Repair,
                    JobDefOf.AttackMelee,
                    JobDefOf.AttackStatic,
                    JobDefOf.CastAbilityOnThing,
                    JobDefOf.CastAbilityOnWorldTile,
                    JobDefOf.CastJump,
                    JobDefOf.BeatFire,
                    JobDefOf.Rescue,
                    JobDefOf.Mate,
                };
            }
        }

        [Main.OnWorldLoaded]
        public static void SetRecommendedJobConfig()
        {
            if (initialized)
            {
                PreparePresets();
                initialized = true;
            }
            foreach (JobDef def in fullyThrottledJobs)
            {
                if (def != null && Context.JobDilationByDef.TryGetValue(def, out JobSettings settings))
                {
                    settings.throttleFilter = JobThrottleFilter.All;
                    settings.throttleMode = JobThrottleMode.Full;
                }
                else
                {
                    Log.Warning($"SOYUZ: Job {def.defName}:{def.label} settings not found while setting presets!");
                }
            }
            foreach (JobDef def in partiallyThrottledJobs)
            {
                if (def != null && Context.JobDilationByDef.TryGetValue(def, out JobSettings settings))
                {
                    settings.throttleFilter = JobThrottleFilter.All;
                    settings.throttleMode = JobThrottleMode.Partial;
                }
                else
                {
                    Log.Warning($"SOYUZ: Job {def.defName}:{def.label} settings not found while setting presets!");
                }
            }
            foreach (JobDef def in notThrottledJobs)
            {
                if (def != null && Context.JobDilationByDef.TryGetValue(def, out JobSettings settings))
                {
                    settings.throttleFilter = JobThrottleFilter.All;
                    settings.throttleMode = JobThrottleMode.None;
                }
                else
                {
                    Log.Warning($"SOYUZ: Job {def.defName}:{def.label} settings not found while setting presets!");
                }
            }
            foreach (JobSettings settings in Context.JobDilationByDef
                .Where(p => !partiallyThrottledJobs.Contains(p.Key) && !fullyThrottledJobs.Contains(p.Key) && !notThrottledJobs.Contains(p.Key))
                .Select(p => p.Value))
            {
                settings.throttleFilter = JobThrottleFilter.Animals;
                settings.throttleMode = JobThrottleMode.Full;
            }
            Log.Message("SOYUZ: Preset loaded!");
        }
    }
}
