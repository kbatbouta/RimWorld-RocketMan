using System;
using System.Collections.Generic;
using RocketMan;
using Soyuz.Profiling;
using Verse;

namespace Soyuz
{
    public static class Context
    {
        public static CameraZoomRange ZoomRange;

        public static CellRect CurViewRect;

        public static SoyuzSettings Settings;

        public static readonly int[] DilationInts = new int[ushort.MaxValue];

        public static readonly bool[] DilationEnabled = new bool[ushort.MaxValue];

        public static readonly bool[] DilationFastMovingRace = new bool[ushort.MaxValue];

        public static readonly Dictionary<ThingDef, RaceSettings> DilationByDef = new Dictionary<ThingDef, RaceSettings>();

        public static int DilationRate
        {
            get
            {
                switch (Context.ZoomRange)
                {
                    default:
                        return 1;
                    case CameraZoomRange.Closest:
                        return 60;
                    case CameraZoomRange.Close:
                        return 20;
                    case CameraZoomRange.Middle:
                        return 15;
                    case CameraZoomRange.Far:
                        return 15;
                    case CameraZoomRange.Furthest:
                        return 7;
                }
            }
        }
    }
}