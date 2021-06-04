using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Proton
{
    public static class Context
    {
        public static ProtonSettings Settings;

        public static Dictionary<string, AlertSettings> TypeIdToSettings = new Dictionary<string, AlertSettings>();

        public static Dictionary<Alert, AlertSettings> AlertToSettings = new Dictionary<Alert, AlertSettings>();

        public static AlertsReadout ReadoutInstance;

        public static AlertSettings[] AlertSettingsByIndex;

        public static Alert[] Alerts;
    }
}
