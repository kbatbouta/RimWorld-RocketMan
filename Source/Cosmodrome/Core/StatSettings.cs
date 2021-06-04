using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RocketMan
{
    public class StatSettings : IExposable
    {
        public float expireAfter;

        public string defName;

        public StatDef statDef;

        public StatSettings()
        {
        }

        public StatSettings(StatDef statDef)
        {
            this.statDef = statDef;
            this.defName = statDef.defName;
            this.expireAfter = Tools.PredictValueFromString(defName);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref expireAfter, "expiryTime_newTemp", 5f);
        }
    }

    public class StatSettingsGroup : IExposable
    {
        public List<StatSettings> AllStatSettings = new List<StatSettings>();

        public StatSettingsGroup()
        {
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref AllStatSettings, "statsSettings", LookMode.Deep);
            if (AllStatSettings == null)
            {
                AllStatSettings = new List<StatSettings>();
            }
        }
    }
}
