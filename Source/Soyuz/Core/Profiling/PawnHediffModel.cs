using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RocketMan;
using UnityEngine;
using Verse;

namespace Soyuz.Profiling
{
    public struct PawnHediffRecord
    {
        public float value;

        public bool dilationEnabled;
        public bool statCachingEnabled;

        public PawnHediffRecord(float value)
        {
            this.value = (float) value;
            this.dilationEnabled = RocketPrefs.TimeDilation && RocketPrefs.Enabled;
            this.statCachingEnabled = RocketPrefs.Enabled;
        }
    }
    
    public class PawnHediffModel
    {
        public List<PawnHediffRecord> records = new List<PawnHediffRecord>();
        
        public void AddResult(float value)
        {
            records.Insert(0,new PawnHediffRecord(value));
            if (records.Count > 2000)
                records.Pop();
        }

        public void DrawGraph(Rect rect, int historyLength=500, string unit = "%")
        {
            Widgets.DrawBoxSolid(rect, Color.white);
            rect = rect.ContractedBy(1);
            Widgets.DrawBoxSolid(rect, Color.black);
            rect = rect.ContractedBy(5);
            var numbersRect = rect.LeftPartPixels(50);
            rect.xMin += 25;
            Widgets.DrawBox(rect);
            var oldHistoyLenght = historyLength;
            historyLength = Mathf.Min(historyLength, records.Count);
            if (historyLength <= 1)
                return;
            var curRecords = records.GetRange(0, historyLength).ToArray();
            float maxY = (int)-1e5;
            float minY = (int) curRecords.First().value;
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
            Widgets.Label(numbersRect.TopPartPixels(25), $"{maxY  * 100}{unit}");
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(numbersRect.BottomPartPixels(25), $"{minY  * 100}{unit}");
            Text.Font = font;
            Text.Anchor = anchor;
            var stepX = rect.width / oldHistoyLenght;
            var curA = rect.position + new Vector2(0, rect.height);
            var curB = rect.position + new Vector2(stepX, rect.height);
            for (int i = 0; i < historyLength - 1; i++)
            {
                var a = curRecords[i];
                var b = curRecords[i + 1];
                curA.y = (1f - ((float)a.value - minY) / maxY) * rect.height + rect.y;
                curB.y = (1f - ((float)b.value - minY) / maxY) * rect.height + rect.y;
                curA.x += stepX;
                curB.x += stepX;
                Widgets.DrawLine(curA, curB, a.dilationEnabled ? Color.green : Color.yellow, 1);
            }
        }
    }
}