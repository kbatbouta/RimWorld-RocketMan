using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Proton
{
    public static class Context
    {
        public static ProtonSettings settings;
        public static Dictionary<string, AlertSettings> typeIdToSettings = new Dictionary<string, AlertSettings>();
        public static Dictionary<Alert, AlertSettings> alertToSettings = new Dictionary<Alert, AlertSettings>();
        public static AlertsReadout readoutInstance;
        public static AlertSettings[] alertSettingsByIndex;
        public static Alert[] alerts;
    }
}
