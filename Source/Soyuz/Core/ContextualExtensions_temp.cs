using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RocketMan;
using Verse;

namespace Soyuz
{
    public static partial class ContextualExtensions
    {
        public static bool IsValidWildlifeOrWorldPawnInternal_newtemp(this Pawn pawn)
        {
            if (pawn?.def == null)
                return false;
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
            if (IgnoreMeDatabase.ShouldIgnore(pawn.def))
                return false;
            if (!RocketPrefs.TimeDilationCriticalHediffs && HasHediffPreventingThrottling(pawn))
                return false;
            if (IsCastingVerb(pawn))
                return false;
            if (pawn.def.race.Humanlike)
            {
                Faction playerFaction = Faction.OfPlayer;
                if (!RocketPrefs.TimeDilationColonists && pawn.factionInt == playerFaction)
                    return false;
                if (!RocketPrefs.TimeDilationColonists && (pawn.guest?.isPrisonerInt ?? false) && pawn.guest?.hostFactionInt == playerFaction)
                    return false;
                if (RocketPrefs.TimeDilationVisitors || RocketPrefs.TimeDilationColonists)
                {
                    JobDef jobDef = pawn.jobs?.curJob?.def;
                    if (jobDef == JobDefOf.Wait)
                        return true;
                    if (jobDef == JobDefOf.Wait_Wander)
                        return true;
                    if (jobDef == JobDefOf.GotoWander)
                        return true;
                    if (jobDef == JobDefOf.LayDown)
                        return true;
                    if (jobDef == JobDefOf.Follow)
                        return true;
                }
                return false;
            }
            if (Context.DilationEnabled[pawn.def.index])
            {
                RaceSettings raceSettings = pawn.GetRaceSettings();
                if (pawn.factionInt == Faction.OfPlayer)
                    return !raceSettings.ignorePlayerFaction && RocketPrefs.TimeDilationColonyAnimals;
                if (pawn.factionInt != null)
                    return !raceSettings.ignoreFactions && RocketPrefs.TimeDilationVisitors;
                return RocketPrefs.TimeDilationWildlife;
            }
            return false;
        }

        public static bool IsSkippingTicks_newtemp(this Pawn pawn)
        {
            if (!pawn.Spawned && WorldPawnsTicker.isActive)
                return true;
            if (pawn.OffScreen())
                return true;
            if (Context.ZoomRange == CameraZoomRange.Far || Context.ZoomRange == CameraZoomRange.Furthest || Context.ZoomRange == CameraZoomRange.Middle)
                return true;
            return false;
        }

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

        private static CachedDict<Pawn, bool> _hediffCache = new CachedDict<Pawn, bool>();

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

        private static bool IsCastingVerb(Pawn p)
        {
            return p.verbTracker?.AllVerbs.Any(v => v.WarmingUp) ?? false;
        }
    }
}
