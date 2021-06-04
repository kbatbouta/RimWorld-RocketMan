using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using Verse;

namespace Proton
{
    public class ProtonSettings : IExposable
    {
        public float executionTimeLimit = 35f;

        public float minInterval = 2.5f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref executionTimeLimit, "executionTimeLimit_NewTemp", 35);
            Scribe_Values.Look(ref minInterval, "minInterval_NewTemp", 2f);

            List<AlertSettings> alertsSettings = Context.alertSettingsByIndex?.ToList() ?? new List<AlertSettings>();
            Scribe_Collections.Look(ref alertsSettings, "settings", LookMode.Deep);

            if (Scribe.mode != LoadSaveMode.Saving && alertsSettings != null)
            {
                foreach (var s in alertsSettings)
                {
                    Context.typeIdToSettings[s.typeId] = s;
                }
            }
        }
    }
}
