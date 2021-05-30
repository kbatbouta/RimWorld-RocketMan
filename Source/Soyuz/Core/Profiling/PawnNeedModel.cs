using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RocketMan;
using UnityEngine;
using Verse;

namespace Soyuz.Profiling
{
    public struct PawnNeedRecord
    {
        public readonly int Tick;

        public float value;

        public bool dilationEnabled;
        public bool statCachingEnabled;

        public PawnNeedRecord(float value)
        {
            this.value = (float)value;
            this.Tick = GenTicks.TicksGame;
            this.dilationEnabled = RocketPrefs.TimeDilation && RocketPrefs.Enabled;
            this.statCachingEnabled = RocketPrefs.Enabled;
        }
    }

    public class PawnNeedModel
    {
        public List<PawnNeedRecord> records = new List<PawnNeedRecord>();

        public void AddResult(float value)
        {
            records.Add(new PawnNeedRecord(value));
            if (records.Count > 240)
                records.RemoveAt(0);
        }

        public void DrawGraph(Rect rect, int historyLength = 500, string unit = "%")
        {
            Widgets.DrawBoxSolid(rect, Color.white);
            rect = rect.ContractedBy(1);
            Widgets.DrawBoxSolid(rect, Color.black);
            rect = rect.ContractedBy(5);
            var numbersRect = rect.LeftPartPixels(50);
            rect.xMin += 25;
            Widgets.DrawBox(rect);
            historyLength = Mathf.Min(historyLength, records.Count);
            if (historyLength <= 2)
                return;
            var curRecords = records.GetRange(records.Count - historyLength, historyLength).ToArray();
            float maxY = (int)-1e5;
            float minY = (int)curRecords.First().value;
            for (int i = 0; i < historyLength - 1; i++)
            {
                if (curRecords[i].value > maxY)
                    maxY = curRecords[i].value;
                if (curRecords[i].value < minY)
                    minY = curRecords[i].value;
            }
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(numbersRect.TopPartPixels(25), $"{maxY * 100}{unit}");
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(numbersRect.BottomPartPixels(25), $"{minY * 100}{unit}");
            Text.Font = font;
            Text.Anchor = anchor;
            float dTickMax = curRecords.Last().Tick - curRecords.First().Tick;
            var curA = rect.position + new Vector2(0, rect.height);
            var curB = rect.position + new Vector2(((curRecords[1].Tick - curRecords[0].Tick) / dTickMax) * rect.width, rect.height);
            for (int i = 1; i < historyLength - 1; i++)
            {
                var a = curRecords[i];
                var b = curRecords[i + 1];
                float dTick = b.Tick - a.Tick;
                curA.y = (1f - ((float)a.value - minY) / maxY) * rect.height + rect.y;
                curB.y = (1f - ((float)b.value - minY) / maxY) * rect.height + rect.y;
                curA.x = curB.x;
                curB.x = curB.x + dTick / dTickMax * ((float)rect.width);
                Widgets.DrawLine(curA, curB, a.dilationEnabled ? Color.green : Color.yellow, 1);
            }
        }
    }
}