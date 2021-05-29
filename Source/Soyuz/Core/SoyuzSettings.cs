using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using HarmonyLib;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz
{
    public class RaceSettings : IExposable
    {
        public ThingDef pawnDef;
        public string pawnDefName;

        public bool dilated = true;
        public bool enabled = true;
        public bool ignoreFactions;
        public bool ignorePlayerFaction;

        public bool isFastMoving;

        private bool isFastMovingInitialized;

        public int DilationInt
        {
            get
            {
                int val = 0;
                if (dilated) val = val | 1;
                if (ignoreFactions) val = val | 2;
                if (ignorePlayerFaction) val = val | 4;
                return val;
            }
        }

        public RaceSettings()
        {
        }

        public RaceSettings(string pawnDefName)
        {
            this.pawnDefName = pawnDefName;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref pawnDefName, "pawnDefName");
            Scribe_Values.Look(ref dilated, "dilated", true);
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref ignoreFactions, "ignoreFactions");
            Scribe_Values.Look(ref ignorePlayerFaction, "ignorePlayerFaction");
            Scribe_Values.Look(ref isFastMoving, "isFastMoving", false);
            Scribe_Values.Look(ref isFastMovingInitialized, "isFastMovingInitialized", false);
        }

        public void ResolveContent()
        {
            if (DefDatabase<ThingDef>.defsByName.TryGetValue(this.pawnDefName, out var def))
                this.pawnDef = def;
            if (this.pawnDef == null || this.isFastMovingInitialized == true)
                return;
            try
            {
                if (this.pawnDef.StatBaseDefined(StatDefOf.MoveSpeed))
                {
                    this.isFastMoving = this.pawnDef.GetStatValueAbstract(StatDefOf.MoveSpeed) > 8;
                    this.isFastMovingInitialized = true;
                }
            }
            catch (Exception er)
            {
                Log.Error($"SOYUZ: Error initializing race def {this.pawnDef} with error:{er}");
            }
        }

        public void Cache()
        {
            Context.dilationByDef[pawnDef] = this;
            Context.dilationEnabled[pawnDef.index] = this.enabled;
            if (!this.isFastMovingInitialized && this.pawnDef.StatBaseDefined(StatDefOf.MoveSpeed))
            {
                this.isFastMoving = this.pawnDef.GetStatValueAbstract(StatDefOf.MoveSpeed) > 8;
                this.isFastMovingInitialized = true;
            }
            Context.dilationInts[pawnDef.index] = DilationInt;
            Context.dilationFastMovingRace[pawnDef.index] = isFastMoving;
        }
    }

    public class SoyuzSettings : IExposable
    {
        public List<RaceSettings> raceSettings = new List<RaceSettings>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref raceSettings, "raceSettings", LookMode.Deep);
            if (raceSettings == null) raceSettings = new List<RaceSettings>();
        }
    }
}