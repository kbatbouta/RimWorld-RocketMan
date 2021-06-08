using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using RimWorld.Planet;
using RocketMan;
using Soyuz.Profiling;
using UnityEngine;
using Verse;

namespace Soyuz
{

    public static partial class ContextualExtensions
    {
        private static Pawn _pawnTick;
        private static Pawn _pawnScreen;
        private static bool offScreen;
        private static int curDelta;

        private const int TransformationCacheSize = 2500;

        private static readonly int[] _transformationCache = new int[TransformationCacheSize];
        private static readonly Dictionary<int, int> timers = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> deltas = new Dictionary<int, int>();
        private static readonly Dictionary<int, Pair<bool, int>> bleeding = new Dictionary<int, Pair<bool, int>>();

        private static readonly Dictionary<Pawn, PawnPerformanceModel> pawnPerformanceModels =
            new Dictionary<Pawn, PawnPerformanceModel>();
        private static readonly Dictionary<Pawn, Dictionary<Type, PawnNeedModel>> pawnNeedModels =
            new Dictionary<Pawn, Dictionary<Type, PawnNeedModel>>();
        private static readonly Dictionary<Pawn, Dictionary<Hediff, PawnHediffModel>> pawnHediffsModels =
            new Dictionary<Pawn, Dictionary<Hediff, PawnHediffModel>>();

        private static int DilationRate
        {
            get
            {
                switch (Context.ZoomRange)
                {
                    default:
                        return 1;
                    case CameraZoomRange.Closest:
                        return 19;
                    case CameraZoomRange.Close:
                        return 17;
                    case CameraZoomRange.Middle:
                        return 15;
                    case CameraZoomRange.Far:
                        return 15;
                    case CameraZoomRange.Furthest:
                        return 7;
                }
            }
        }

        public static Pawn Current
        {
            get => _pawnTick;
        }

        [Main.OnInitialization]
        public static void Initialize()
        {
            Log.Message("SOYUZ: Created _transformationCache");
            for (int i = 0; i < _transformationCache.Length; i++)
                _transformationCache[i] = (int)Mathf.Max(Mathf.RoundToInt(i / 30) * 30, 30);
        }

        private static int RoundTransform(int interval)
        {
            if (interval >= TransformationCacheSize)
                return (int)Mathf.Max(Mathf.RoundToInt(interval / 30) * 30, 30);
            return _transformationCache[interval];
        }

        public static bool OffScreen(this Pawn pawn)
        {
            if (pawn == null)
                return false;
            if (RocketDebugPrefs.AlwaysDilating)
                return offScreen = true;
            if (_pawnScreen == pawn)
                return offScreen;
            _pawnScreen = pawn;
            if (Context.CurViewRect.Contains(pawn.positionInt))
                return offScreen = false;
            return offScreen = true;
        }

        private static bool _isSkipping;
        private static Pawn _skipingPawn;

        public static bool IsSkippingTicks(this Pawn pawn)
        {
            if (!RocketPrefs.TimeDilation)
                return false;
            if (_skipingPawn == pawn)
                return _isSkipping;
            _skipingPawn = pawn;
            _isSkipping = pawn?.IsSkippingTicks_newtemp() ?? false;
            return _isSkipping;
        }

        private static Stopwatch _stopwatch = new Stopwatch();

        public static void BeginTick(this Pawn pawn)
        {
            _pawnTick = pawn;
            if (false
                || !RocketPrefs.Enabled
                || !RocketPrefs.TimeDilation
                || !pawn.IsValidWildlifeOrWorldPawn())
            {
                Skip(pawn);
                return;
            }
            if (RocketDebugPrefs.LogData && Time.frameCount - RocketStates.LastFrame < 60)
            {
                _stopwatch.Reset();
                _stopwatch.Start();
            }
        }

        public static void EndTick(this Pawn pawn)
        {
            _pawnTick = null;
            if (RocketDebugPrefs.LogData && Time.frameCount - RocketStates.LastFrame < 60)
            {
                _stopwatch.Stop();
                var performanceModel = pawn.GetPerformanceModel();
                performanceModel.AddResult(_stopwatch.ElapsedTicks);
                if (GenTicks.TicksGame % 150 == 0)
                {
                    var needsModel = pawn.GetNeedModels();
                    if (pawn.needs?.needs != null)
                        foreach (var need in pawn.needs?.needs)
                        {
                            var type = need.GetType();
                            if (needsModel.TryGetValue(type, out var model)) model.AddResult(need.CurLevelPercentage);
                            else needsModel[type] = new PawnNeedModel();
                        }

                    Dictionary<Hediff, PawnHediffModel> hediffModel = pawn.GetHediffModels();
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if (hediffModel.TryGetValue(hediff, out var model)) model.AddResult(hediff.Severity);
                        else hediffModel[hediff] = new PawnHediffModel();
                    }
                }
            }
            else Reset();
        }

        public static void Skip(Pawn pawn)
        {
            _isValidPawn = false;
        }

        public static void Reset()
        {
            _pawnScreen = null;
            _pawnTick = null;
            _validPawn = null;
        }

        public static PawnPerformanceModel GetPerformanceModel(this Pawn pawn)
        {
            if (pawn == null)
                return null;
            if (pawnPerformanceModels.TryGetValue(pawn, out var model))
                return model;
            return pawnPerformanceModels[pawn] = new PawnPerformanceModel();
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

        public static bool IsCustomTickInterval(this Thing thing, int interval)
        {
            if (Current == thing && Current.IsValidWildlifeOrWorldPawn())
                return IsCustomTickIntervalInternel(thing, interval);
            return (thing.thingIDNumber + GenTicks.TicksGame) % interval == 0;
        }

        public static bool IsCustomTickIntervalInternel(Thing thing, int interval)
        {
            if (WorldPawnsTicker.isActive)
                return WorldPawnsTicker.IsCustomWorldTickInterval(thing, interval);
            else if (Current.IsSkippingTicks())
                return (thing.thingIDNumber + GenTicks.TicksGame) % RoundTransform(interval) == 0;
            return (thing.thingIDNumber + GenTicks.TicksGame) % interval == 0;
        }

        public static void UpdateTimers(this Pawn pawn)
        {
            int tick = GenTicks.TicksGame;
            curDelta = 1;
            if (timers.TryGetValue(pawn.thingIDNumber, out var val))
                curDelta = tick - val;
            deltas[pawn.thingIDNumber] = curDelta;
            timers[pawn.thingIDNumber] = GenTicks.TicksGame;
        }

        public static bool ShouldTick(this Pawn pawn)
        {
            if (!RocketPrefs.TimeDilation || !RocketPrefs.Enabled)
                return true;
            if (WorldPawnsTicker.isActive && RocketPrefs.TimeDilationWorldPawns)
                return true;
            int tick = GenTicks.TicksGame;
            if (false
                || (pawn.thingIDNumber + tick) % 30 == 0
                || (tick % 250 == 0)
                || (pawn.jobs?.curJob != null && pawn.jobs?.curJob?.expiryInterval > 0 &&
                (tick - pawn.jobs.curJob.startTick) % (pawn.jobs.curJob.expiryInterval * 2) == 0))
                return true;
            if (Context.DilationFastMovingRace[pawn.def.index])
                return (pawn.thingIDNumber + tick) % 2 == 0;
            if (pawn.OffScreen())
                return (pawn.thingIDNumber + tick) % DilationRate == 0;
            if (Context.ZoomRange == CameraZoomRange.Far || Context.ZoomRange == CameraZoomRange.Furthest)
                return (pawn.thingIDNumber + tick) % 4 == 0;
            if (Context.ZoomRange == CameraZoomRange.Middle)
                return (pawn.thingIDNumber + tick) % 2 == 0;
            return true;
        }

        public static int GetDeltaT(this Thing thing)
        {
            if (thing == Current)
                return curDelta;
            if (deltas.TryGetValue(thing?.thingIDNumber ?? -1, out int delta))
                return delta;
            throw new Exception($"SOYUZ: Inconsistant timer data for pawn {thing}");
        }

        private static bool _isValidPawn = false;
        private static Pawn _validPawn = null;
        public static bool IsValidWildlifeOrWorldPawn(this Pawn pawn)
        {
            if (Current != pawn || pawn == null)
                return false;
            if (_validPawn != pawn)
            {
                _validPawn = pawn;
                _isValidPawn = IsValidWildlifeOrWorldPawnInternal_newtemp(pawn);
            }
            return _isValidPawn;
        }

        public static bool IsBleeding(this Pawn pawn)
        {
            bool isBleeding = false;
            if (bleeding.TryGetValue(pawn.thingIDNumber, out var store) &&
                ((GenTicks.TicksGame - store.second < 900 && !store.first) || (GenTicks.TicksGame - store.second < 2500 && store.first)))
                isBleeding = store.first;
            else if (pawn.RaceProps.IsFlesh)
                bleeding[pawn.thingIDNumber] = new Pair<bool, int>(isBleeding = (pawn.health.hediffSet.CalculateBleedRate() > 0f), GenTicks.TicksGame);
            return isBleeding;
        }

        public static int AsInt(this Pawn pawn)
        {
            int val = 1;
            if (pawn.factionInt != null)
            {
                if (pawn.factionInt != Faction.OfPlayerSilentFail) val = val | 2;
                else val = val | 4;
            }
            return val;
        }
    }
}