using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RimWorld;
using RimWorld.Planet;
using RocketMan;
using Soyuz.Profiling;
using Verse;
using Verse.AI;

namespace Soyuz
{
    public static partial class ContextualExtensions
    {
        private static readonly Dictionary<Pawn, PawnPerformanceModel> pawnPerformanceModels =
           new Dictionary<Pawn, PawnPerformanceModel>();
        private static readonly Dictionary<Pawn, PawnPatherModel> pawnPatherModels =
            new Dictionary<Pawn, PawnPatherModel>();
        private static readonly Dictionary<Pawn, Dictionary<Type, PawnNeedModel>> pawnNeedModels =
            new Dictionary<Pawn, Dictionary<Type, PawnNeedModel>>();
        private static readonly Dictionary<Pawn, Dictionary<Hediff, PawnHediffModel>> pawnHediffsModels =
            new Dictionary<Pawn, Dictionary<Hediff, PawnHediffModel>>();

        public static void UpdateModels(Pawn pawn)
        {
            var performanceModel = pawn.GetPerformanceModel();
            performanceModel.AddResult((float)_stopwatch.ElapsedTicks / (float)Stopwatch.Frequency * (float)1000f);

            var needsModel = pawn.GetNeedModels();
            if (pawn.needs?.needs != null)
            {
                foreach (var need in pawn.needs?.needs)
                {
                    var type = need.GetType();
                    if (needsModel.TryGetValue(type, out var model))
                    {
                        model.AddResult(need.CurLevelPercentage);
                    }
                    else
                    {
                        needsModel[type] = new PawnNeedModel(need.def.label);
                    }
                }
            }
            Dictionary<Hediff, PawnHediffModel> hediffModel = pawn.GetHediffModels();
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediffModel.TryGetValue(hediff, out var model))
                {
                    model.AddResult(hediff.Severity);
                }
                else
                {
                    hediffModel[hediff] = new PawnHediffModel(hediff.def.label);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RaceSettings GetRaceSettings(this Pawn pawn)
        {
            if (pawn?.def != null && Context.DilationByDef.TryGetValue(pawn.def, out RaceSettings settings))
            {
                return settings;
            }
            ThingDef def = pawn.def;
            Context.Settings.AllRaceSettings.Add(settings = new RaceSettings()
            {
                def = def,
                enabled = def.race.Animal && !def.race.Humanlike && !def.race.IsMechanoid,
                ignoreFactions = false
            });
            settings.Prepare();
            return settings;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobSettings GetCurJobSettings(this Pawn pawn)
        {
            Job job = pawn.jobs?.curJob;
            if (job == null)
            {
                return null;
            }
            if (job.def != null && Context.JobDilationByDef.TryGetValue(job.def, out JobSettings settings))
            {
                return settings;
            }
            JobDef def = job.def;
            Context.Settings.AllJobsSettings.Add(settings = new JobSettings(def));
            settings.Prepare();
            return settings;
        }

        private static CachedDict<Pawn, bool> _hediffCache = new CachedDict<Pawn, bool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasHediffPreventingThrottling(Pawn p)
        {
            if (_hediffCache.TryGetValue(p, out bool result, 250))
            {
                return result;
            }
            List<Hediff> hediffs = p.health?.hediffSet?.hediffs;
            if (hediffs == null)
            {
                return _hediffCache[p] = false;
            }
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (!hediffs[i].def.AlwaysAllowMothball && !hediffs[i].IsPermanent())
                {
                    return _hediffCache[p] = true;
                }
            }
            return _hediffCache[p] = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCastingVerb(Pawn p)
        {
            List<Verb> verbs = p.verbTracker?.AllVerbs;
            if (verbs != null)
            {
                foreach (Verb v in verbs)
                {
                    if (v.WarmingUp)
                        return true;
                }
            }
            return false;
        }

        public static PawnPerformanceModel GetPerformanceModel(this Pawn pawn)
        {
            if (pawn == null)
                return null;
            if (pawnPerformanceModels.TryGetValue(pawn, out var model))
                return model;
            return pawnPerformanceModels[pawn] = new PawnPerformanceModel("Performance");
        }

        public static Dictionary<Type, PawnNeedModel> GetNeedModels(this Pawn pawn)
        {
            if (pawn == null)
                return null;
            if (pawnNeedModels.TryGetValue(pawn, out var model))
                return model;
            return pawnNeedModels[pawn] = new Dictionary<Type, PawnNeedModel>();
        }

        public static Dictionary<Hediff, PawnHediffModel> GetHediffModels(this Pawn pawn)
        {
            if (pawn == null)
                return null;
            if (pawnHediffsModels.TryGetValue(pawn, out var model))
                return model;
            return pawnHediffsModels[pawn] = new Dictionary<Hediff, PawnHediffModel>();
        }

        public static PawnPatherModel GetPatherModel(this Pawn pawn)
        {
            if (pawn == null)
                return null;
            if (pawnPatherModels.TryGetValue(pawn, out var model))
                return model;
            return pawnPatherModels[pawn] = new PawnPatherModel("Pathing");
        }
    }
}
