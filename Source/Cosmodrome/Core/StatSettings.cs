using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RocketMan
{
    public class StatSettings : IExposable
    {
        public StatDef statDef;

        public float expiryAfter;

        public StatSettings()
        {
        }

        public StatSettings(StatDef statDef)
        {
            this.statDef = statDef;
            this.expiryAfter = Tools.PredictStatExpiryFromString(statDef.defName);
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && statDef != null)
            {
                Resolve();
            }
            try
            {
                Scribe_Defs.Look(ref statDef, "statDef");
            }
            finally
            {
                Scribe_Values.Look(ref expiryAfter, "expiryAfter");
            }
        }

        public void Prepare()
        {
            RocketStates.StatExpiry[statDef.index] = this.expiryAfter;
        }

        public void Resolve()
        {
            this.expiryAfter = RocketStates.StatExpiry[statDef.index];
        }
    }

    public class StatSettingsGroup : IExposable
    {
        public List<StatSettings> AllSettings = new List<StatSettings>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref AllSettings, "AllSettings", LookMode.Deep);

            if (AllSettings == null)
            {
                AllSettings = new List<StatSettings>();
            }
        }
    }
}
