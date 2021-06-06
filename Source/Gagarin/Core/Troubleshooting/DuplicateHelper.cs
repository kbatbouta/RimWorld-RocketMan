using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Verse;

namespace Gagarin
{
    public static class DuplicateHelper
    {
        private static DuplicateReport[] duplicates;

        private static Dictionary<string, DuplicateReport> nameToReport = new Dictionary<string, DuplicateReport>();

        private static Stopwatch stopwatch = new Stopwatch();

        public static IEnumerable<DuplicateReport> Duplicates
        {
            get => duplicates;
        }

        public static void ParseCreateReports(XmlDocument document, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
        {
            stopwatch.Restart();
            duplicates = Process(document, assetlookup)?.ToArray() ?? new DuplicateReport[0];
            PrepareReportFolder();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < duplicates.Length; i++)
            {
                DuplicateReport report = duplicates[i];
                if (!report.HasDuplicates)
                {
                    throw new Exception($"GAGARIN:[DUPLICATE] Processor return a DuplicateReport with length={report.Length} for name={report.Name}");
                }
                int j = 1;
                report.Write(Path.Combine(GagarinEnvironmentInfo.ReportsFolderPath, $"{report.Name}_{Rand.Int % 10000}.xml"));
                builder.Clear();
                builder.Append($"GAGARIN:[DUPLICATE] duplicate found for Name={report.Name}");
                foreach (DuplicateReport.DuplicationRecord record in report.Records)
                {
                    builder.AppendInNewLine($"\t{j++}. PackageId={record.mod?.PackageId}\t| ModName={record.mod?.Name}\t| XmlFilePath={record.xmlFilePath}");
                }
                Log.Error(builder.ToString());
            }
            Log.Message($"GAGARIN:[DUPLICATE] Finished creating reports at <color=red>{GagarinEnvironmentInfo.ReportsFolderPath}</color>");
            nameToReport.Clear();
            stopwatch.Stop();
            Log.Message($"GAGARIN:[DUPLICATE] Creating duplication reports took {stopwatch.ElapsedMilliseconds} MS");
        }

        private static IEnumerable<DuplicateReport> Process(XmlDocument document, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
        {
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                XmlElement element = node as XmlElement;
                if (false
                    || !element.HasAttribute("Abstract")
                    || !element.HasAttribute("Name")
                    || element.GetAttribute("Abstract").ToLower() == "false")
                {
                    continue;
                }
                ModContentPack mod = Context.Core;
                if (assetlookup.TryGetValue(node, out LoadableXmlAsset asset))
                {
                    mod = asset.mod;
                }
                string name = element.GetAttribute("Name");
                if (!nameToReport.TryGetValue(name, out DuplicateReport report))
                {
                    report = nameToReport[name] = new DuplicateReport(name);
                }
                report.AddXmlNode(node, mod, asset?.FullFilePath ?? null);
                if (report.HasDuplicates && report.Length == 2)
                {
                    yield return report;
                }
            }
        }

        private static void PrepareReportFolder()
        {
            if (Directory.Exists(GagarinEnvironmentInfo.ReportsFolderPath))
            {
                Directory.Delete(GagarinEnvironmentInfo.ReportsFolderPath, recursive: true);
            }
            Directory.CreateDirectory(GagarinEnvironmentInfo.ReportsFolderPath);
        }
    }
}
