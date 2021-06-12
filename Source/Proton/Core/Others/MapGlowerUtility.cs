using System;
using Verse;

namespace Proton
{
    public static class MapGlowerUtility
    {
        private const int _MaxMapNum = 40;

        private static readonly Map[] _maps = new Map[_MaxMapNum];
        private static readonly GlowerTracker[] _trackers = new GlowerTracker[_MaxMapNum];

        public static GlowerTracker GetGlowerTracker(this Map map)
        {
            int mapIndex = map.Index;
            if (mapIndex > _MaxMapNum || mapIndex < 0)
            {
                return null;
            }
            GlowerTracker tracker = _trackers[mapIndex];
            if (_maps[mapIndex] != map || tracker?.map != map)
            {
                _maps[mapIndex] = map;
                _trackers[mapIndex] = tracker = new GlowerTracker(map);
            }
            return tracker;
        }

        public static GlowerProperties GetProperties(this CompGlower comp)
        {
            return GetGlowerTracker(comp.parent.Map).GetProperties(comp);
        }
    }
}
