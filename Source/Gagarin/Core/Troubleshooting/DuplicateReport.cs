using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace Gagarin
{
    public class DuplicateReport
    {
        private readonly string name;

        private readonly List<DuplicationRecord> records = new List<DuplicationRecord>();

        private bool coreModInvolved = false;

        public struct DuplicationRecord
        {
            public string xmlFilePath;

            public XmlNode node;

            public ModContentPack mod;

            public bool Parentless
            {
                get => xmlFilePath == null;
            }

            public static DuplicationRecord Invalid
            {
                get => new DuplicationRecord() { node = null, mod = null };
            }

            public static DuplicationRecord Create(XmlNode node, ModContentPack mod, string xmlFilePath)
            {
                return new DuplicationRecord() { mod = mod, node = node, xmlFilePath = xmlFilePath };
            }

            public override bool Equals(object obj)
            {
                return obj is DuplicationRecord other && other.mod == mod && other.node == node && other.xmlFilePath == xmlFilePath;
            }

            public override int GetHashCode()
            {
                return node.GetHashCode();
            }
        }

        public string Name
        {
            get => name;
        }

        public bool IsCritical
        {
            get => coreModInvolved && HasDuplicates;
        }

        public bool HasDuplicates
        {
            get => records.Count > 1;
        }

        public int Length
        {
            get => records.Count;
        }

        public List<DuplicationRecord> Records
        {
            get => records;
        }

        public IEnumerable<ModContentPack> CulpritMods
        {
            get => records.Select(r => r.mod);
        }

        public DuplicateReport(string name)
        {
            this.name = name;
        }

        public void AddXmlNode(XmlNode node, ModContentPack mod, string xmlFilePath)
        {
            if (mod?.IsCoreMod ?? true)
            {
                coreModInvolved = true;
            }
            records.Add(DuplicationRecord.Create(node, mod, xmlFilePath));
        }

        public ModContentPack GetCulprit(XmlNode node)
        {
            foreach (DuplicationRecord record in records)
            {
                if (record.node == node)
                    return record.mod;
            }
            return null;
        }

        public void Write(string path)
        {
            XmlDocument document = new XmlDocument();
            XmlElement root;
            document.AppendChild(root = document.CreateElement("DuplicateReport"));
            root.SetAttribute("Name", Name);
            root.SetAttribute("Length", Length.ToString());
            root.SetAttribute("Critical", coreModInvolved ? "True" : "False");
            foreach (DuplicationRecord record in Records)
            {
                XmlElement node = document.CreateElement("DuplicationRecord");
                node.SetAttribute("PackageId", record.mod?.PackageIdPlayerFacing);
                node.SetAttribute("ModName", record.mod?.Name);
                node.SetAttribute("XmlFilePath", record.xmlFilePath);
                node.AppendChild(document.ImportNode(record.node, true));
                root.AppendChild(node);
            }
            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = false,
                Indent = true,
                NewLineChars = "\n"
            };
            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                document.Save(writer);
            }
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() ^ records.GetHashCode();
        }
    }
}
