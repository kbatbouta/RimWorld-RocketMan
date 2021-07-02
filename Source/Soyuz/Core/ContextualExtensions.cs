using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        private static int DilationRate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (Context.ZoomRange)
                {
                    default:
                        return 1;
                    case CameraZoomRange.Closest:
                        return 15;
                    case CameraZoomRange.Close:
                        return 15;
                    case CameraZoomRange.Middle:
                        return 19;
                    case CameraZoomRange.Far:
                        return 20;
                    case CameraZoomRange.Furthest:
                        return 21;
                }
            }
        }

        public static Pawn Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        private static Stopwatch _stopwatch = new Stopwatch();

        public static void BeginTick(this Pawn pawn)
        {
            Context.CurRaceSettings = pawn.GetRaceSettings();
            Context.CurJobSettings = pawn.GetCurJobSettings();

            _stopwatch.Restart();
            _pawnTick = pawn;

            if (false
                || !RocketPrefs.Enabled
                || !RocketPrefs.TimeDilation
                || !pawn.IsValidWildlifeOrWorldPawn()
                || !timers.ContainsKey(pawn.thingIDNumber))
            {
                UpdateTimers(pawn);
                return;
            }
        }

        public static void EndTick(this Pawn pawn)
        {
            _pawnTick = null;
            _stopwatch.Stop();

            try
            {
                if (true
                    && RocketDebugPrefs.LogData
                    && Time.frameCount - RocketStates.LastFrame < 60
                    && pawn == Context.ProfiledPawn)
                {
                    UpdateModels(pawn);
                }
            }
            finally
            {
                Reset();
            }
        }

        public static void Reset()
        {
            _pawnScreen = null;
            _validPawn = null;
            _pawnTick = null;

            Context.CurRaceSettings = null;
            Context.CurRaceSettings = null;
        }

        public static bool IsCustomTickInterval(this Thing thing, int interval)
        {
            if (Current == thing
                && Current.IsValidWildlifeOrWorldPawn()
                && RocketPrefs.Enabled
                && RocketPrefs.TimeDilation)
            {
                return IsCustomTickInterval_newtemp(thing, interval);
            }
            return (thing.thingIDNumber + GenTicks.TicksGame) % interval == 0;
        }

        public static bool IsCustomTickInterval_newtemp(Thing thing, int interval)
        {
            if (WorldPawnsTicker.isActive)
            {
                return WorldPawnsTicker.IsCustomWorldTickInterval(thing, interval);
            }
            else if (Current.IsBeingThrottled() && RocketPrefs.Enabled && RocketPrefs.TimeDilation)
            {
                return (thing.thingIDNumber + GenTicks.TicksGame) % RoundTransform(interval) == 0;
            }
            return (thing.thingIDNumber + GenTicks.TicksGame) % interval == 0;
        }

        public static int GetDeltaT(this Thing thing)
        {
            if (!RocketPrefs.Enabled || !RocketPrefs.TimeDilation)
                return 1;
            if (thing == Current)
                return curDelta;
            if (deltas.TryGetValue(thing?.thingIDNumber ?? -1, out int delta))
                return delta;
            if (thing is Pawn pawn)
            {
                Log.Warning($"SOYUZ: Tried to get delta for unregistered pawn {pawn}!");

                timers[pawn.thingIDNumber] = GenTicks.TicksGame;
                deltas[pawn.thingIDNumber] = 1;
                return 1;
            }
            throw new ArgumentException("Argument should be a Verse.Pawn");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                || (pawn.jobs?.curJob != null && pawn.jobs?.curJob?.expiryInterval > 0 && (tick - pawn.jobs.curJob.startTick) % (pawn.jobs.curJob.expiryInterval) == 0))
                return true;
            if (Context.DilationFastMovingRace[pawn.def.index])
                return (pawn.thingIDNumber + tick) % 2 == 0;
            if (pawn.OffScreen() == true)
                return (pawn.thingIDNumber + tick) % DilationRate == 0;
            if (Context.ZoomRange == CameraZoomRange.Far || Context.ZoomRange == CameraZoomRange.Furthest)
                return (pawn.thingIDNumber + tick) % 3 == 0;
            if (Context.ZoomRange == CameraZoomRange.Middle)
                return (pawn.thingIDNumber + tick) % 4 == 0;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        private static bool _isBeingThrottled;
        private static Pawn _throttledPawn;

        public static bool IsBeingThrottled(this Pawn pawn)
        {
            if (Current != pawn
                || pawn == null
                || !pawn.IsValidWildlifeOrWorldPawn()
                || !RocketPrefs.TimeDilation
                || !RocketPrefs.Enabled)
                return false;
            if (_throttledPawn != pawn)
            {
                _throttledPawn = pawn;
                _isBeingThrottled = pawn.IsBeingThrottled_newtemp();
            }
            return _isBeingThrottled;
        }

        private static bool _isValidPawn = false;
        private static Pawn _validPawn = null;

        public static bool IsValidWildlifeOrWorldPawn(this Pawn pawn)
        {
            if (Current != pawn
                || pawn == null
                || !RocketPrefs.TimeDilation
                || !RocketPrefs.Enabled)
                return false;
            if (_validPawn != pawn)
            {
                _validPawn = pawn;
                _isValidPawn = IsValidWildlifeOrWorldPawnInternal_newtemp(pawn);
            }
            return _isValidPawn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidWildlifeOrWorldPawnInternal_newtemp(this Pawn pawn)
        {
            if (!RocketPrefs.Enabled || !RocketPrefs.TimeDilation)
                return false;

            if (WorldPawnsTicker.isActive)
            {
                if (pawn.IsCaravanMember())
                    return false;
                if (!RocketPrefs.TimeDilationWorldPawns)
                    return false;
                return true;
            }
            else if (true
                && !IgnoreMeDatabase.ShouldIgnore(pawn.def)
                && !IsCastingVerb(pawn)
                && !(!RocketPrefs.TimeDilationCriticalHediffs && HasHediffPreventingThrottling(pawn))
                && Context.CurJobSettings != null
                && Context.CurJobSettings.throttleMode != JobThrottleMode.None)
            {
                if (pawn.def.race.Humanlike && Context.CurJobSettings.def != JobDefOf.DoBill && GenTicks.TicksGame - (pawn.jobs?.curJob?.startTick ?? 0) >= 19)
                {
                    return IsValidHuman(pawn);
                }
                else if (Context.DilationEnabled[pawn.def.index])
                {
                    return IsValidAnimal(pawn);
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidHuman(Pawn pawn)
        {
            if (true
                && Context.CurJobSettings.throttleFilter != JobThrottleFilter.Humanlikes
                && Context.CurJobSettings.throttleFilter != JobThrottleFilter.All)
                return false;
            Faction playerFaction = Faction.OfPlayer;

            if (!RocketPrefs.TimeDilationColonists && pawn.factionInt == playerFaction)
                return false;
            if (!RocketPrefs.TimeDilationColonists && (pawn.guest?.isPrisonerInt ?? false) && pawn.guest?.hostFactionInt == playerFaction)
                return false;
            if (Context.CurJobSettings != null && (RocketPrefs.TimeDilationVisitors || RocketPrefs.TimeDilationColonists))
                return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidAnimal(Pawn pawn)
        {
            if (true
                && Context.CurJobSettings.throttleFilter != JobThrottleFilter.Animals
                && Context.CurJobSettings.throttleFilter != JobThrottleFilter.All)
                return false;
            RaceSettings raceSettings = Context.CurRaceSettings;

            if (pawn.factionInt == Faction.OfPlayer)
                return !raceSettings.ignorePlayerFaction && RocketPrefs.TimeDilationColonyAnimals;
            if (pawn.factionInt != null)
                return !raceSettings.ignoreFactions && RocketPrefs.TimeDilationVisitors;

            return RocketPrefs.TimeDilationWildlife;
        }

        private static bool IsBeingThrottled_newtemp(this Pawn pawn)
        {
            if (!pawn.Spawned && WorldPawnsTicker.isActive && pawn.GetCaravan() != null)
                return true;
            if (pawn.OffScreen())
                return true;
            if (Context.ZoomRange == CameraZoomRange.Far || Context.ZoomRange == CameraZoomRange.Furthest || Context.ZoomRange == CameraZoomRange.Middle)
                return true;
            return false;
        }
    }
}