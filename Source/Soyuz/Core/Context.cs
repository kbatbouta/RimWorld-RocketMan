using System;
using System.Collections.Generic;
using RocketMan;
using Soyuz.Profiling;
using Verse;

namespace Soyuz
{
    public static class Context
    {
        public static CameraZoomRange zoomRange;
        public static CellRect curViewRect;

        public static SoyuzSettings settings;

        public static readonly int[] dilationInts = new int[ushort.MaxValue];
        public static readonly bool[] dilationEnabled = new bool[ushort.MaxValue];
        public static readonly bool[] dilationFastMovingRace = new bool[ushort.MaxValue];

        public static readonly Dictionary<ThingDef, RaceSettings> dilationByDef = new Dictionary<ThingDef, RaceSettings>();

        public static int DilationRate
        {
            get
            {
                switch (Context.zoomRange)
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