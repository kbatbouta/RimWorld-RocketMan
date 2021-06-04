using System;
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
            if (pawn.IsCaravanMember())
                return false;
            if (WorldPawnsTicker.isActive)
                return RocketPrefs.TimeDilationWorldPawns;
            if (!Context.DilationEnabled[pawn.def.index] || IgnoreMeDatabase.ShouldIgnore(pawn.def))
                return false;
            if (pawn.IsBleeding() || (!RocketPrefs.TimeDilationCriticalHediffs && pawn.HasCriticalHediff()))
                return false;
            if (pawn.def.race.Humanlike)
            {
                Faction playerFaction = Faction.OfPlayer;
                if (pawn.factionInt == playerFaction)
                    return false;
                if (pawn.guest?.isPrisonerInt ?? false && pawn.guest?.hostFactionInt == playerFaction)
                    return false;
                if (RocketPrefs.TimeDilationVisitors)
                {
                    JobDef jobDef = pawn.jobs?.curJob?.def;
                    if (jobDef == null)
                        return false;
                    if (IgnoreMeDatabase.ShouldIgnore(jobDef))
                        return false;
                    if (jobDef == JobDefOf.Wait_Wander)
                        return true;
                    if (jobDef == JobDefOf.GotoWander)
                        return true;
                    if (jobDef == JobDefOf.Wait)
                        return true;
                    if (jobDef == JobDefOf.SocialRelax)
                        return true;
                    if (jobDef == JobDefOf.LayDown)
                        return true;
                    if (jobDef == JobDefOf.Follow)
                        return true;
                }
                return WorldPawnsTicker.isActive;
            }
            RaceSettings raceSettings = pawn.GetRaceSettings();
            if (pawn.factionInt == Faction.OfPlayer)
                return !raceSettings.ignorePlayerFaction && RocketPrefs.TimeDilationColonyAnimals;
            if (pawn.factionInt != null)
                return !raceSettings.ignoreFactions && RocketPrefs.TimeDilationVisitors;
            return RocketPrefs.TimeDilationWildlife;
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
    }
}
