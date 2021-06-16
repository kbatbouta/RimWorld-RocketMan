using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using RocketMan;
using UnityEngine;
using Verse;

namespace Soyuz.Profiling
{
    public class PawnPerformanceModel : IPawnModel
    {
        public PawnPerformanceModel(string name) : base(name)
        {
            this.grapher.MaxTWithoutAddtion = 1f;
        }

        public override void AddResult(float value)
        {
            queue.Add(new Tuple<float, float, bool>(GenTicks.TicksGame.TicksToSeconds(), value, RocketPrefs.TimeDilation));
        }
    }
}