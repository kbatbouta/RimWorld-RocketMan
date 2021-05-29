using System;
using System.Collections.Generic;
using System.Linq;
using RocketMan;
using Verse;

namespace Proton
{
    public static class ProtonSettingsUtility
    {
        [Main.OnScribe]
        public static void PostScribe()
        {
            Scribe_Deep.Look(ref Context.settings, "protonSettings");
            if (Context.settings == null)
                Context.settings = new ProtonSettings();
            RocketEnvironmentInfo.ProtonLoaded = true;
        }
    }
}
