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
        public string name;

        public bool enabled = true;
        public bool ignoreFactions;
        public bool ignorePlayerFaction;
        public bool isFastMoving;

        private int version;
        private bool isFastMovingInitialized;

        private const int SETTINGS_VERSION = 1;

        public int DilationInt
        {
            get
            {
                int val = 0;
                if (enabled) val = val | 1;
                if (ignoreFactions) val = val | 2;
                if (ignorePlayerFaction) val = val | 4;
                return val;
            }
        }

        public RaceSettings()
        {
        }

        public RaceSettings(string defName)
        {
            this.name = defName;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref version, "version", -1);
            Scribe_Values.Look(ref name, "pawnDefName");
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref ignoreFactions, "ignoreFactions");
            Scribe_Values.Look(ref ignorePlayerFaction, "ignorePlayerFaction");
            Scribe_Values.Look(ref isFastMoving, "isFastMoving", false);
            Scribe_Values.Look(ref isFastMovingInitialized, "isFastMovingInitialized", false);
        }

        public void ResolveContent()
        {
            if (false
                || !DefDatabase<ThingDef>.defsByName.TryGetValue(this.name, out var def)
                || this.isFastMovingInitialized == true)
                return;
            if (this.version != SETTINGS_VERSION)
            {
                this.Notify_VersionChanged();
                this.version = SETTINGS_VERSION;
            }
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
            Context.DilationByDef[pawnDef] = this;
            Context.DilationEnabled[pawnDef.index] = this.enabled;
            if (!this.isFastMovingInitialized && this.pawnDef.StatBaseDefined(StatDefOf.MoveSpeed))
            {
                this.isFastMoving = this.pawnDef.GetStatValueAbstract(StatDefOf.MoveSpeed) > 8;
                this.isFastMovingInitialized = true;
            }
            Context.DilationInts[pawnDef.index] = DilationInt;
            Context.DilationFastMovingRace[pawnDef.index] = isFastMoving;
            if (this.version != SETTINGS_VERSION)
            {
                this.Notify_VersionChanged();
                this.version = SETTINGS_VERSION;
            }
        }

        private void Notify_VersionChanged()
        {
            if (this.pawnDef?.race?.IsMechanoid ?? false)
            {
                this.enabled = false;
                this.ignoreFactions = false;
                this.ignorePlayerFaction = false;
            }
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