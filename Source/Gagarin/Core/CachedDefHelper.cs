using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Xml;
using Mono.Security.X509.Extensions;
using Verse;
using static Verse.XmlInheritance;

namespace Gagarin
{
    public static class CachedDefHelper
    {
        private static XmlDocument document;

        private static List<DefXmlUnit> defs = new List<DefXmlUnit>();

        private static HashSet<string> registeredNames = new HashSet<string>();

        private class DefXmlUnit
        {
            public Def def;
            public XmlNode node;
            public LoadableXmlAsset asset;
            public XmlInheritanceNode inheritanceNode;
        }

        public static void Prepare()
        {
            if (Context.IsUsingCache)
                return;

            document = new XmlDocument();
            document.AppendChild(document.CreateElement("DefXmlStorage"));
        }

        public static void Register(Def def, XmlNode node, LoadableXmlAsset asset)
        {
            defs.Add(new DefXmlUnit()
            {
                def = def,
                node = node,
                asset = asset,
                inheritanceNode = XmlInheritance.resolvedNodes.TryGetValue(node, out XmlInheritanceNode inheritanceNode)
                ? inheritanceNode : null
            });
        }

        public static void Save()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            XmlElement root = document.DocumentElement;
            XmlElement wrapper;

            foreach (DefXmlUnit unit in defs)
            {
                XmlElement node = unit.node as XmlElement;
                if (unit.inheritanceNode == null)
                {
                    wrapper = WrapXmlNode(node, unit.asset?.FullFilePath);
                    root.AppendChild(wrapper);
                    continue;
                }
                ResolvePushRecursively(unit.inheritanceNode);
            }
            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = false,
                Indent = true,
                NewLineChars = "\n"
            };
            using (XmlWriter writer = XmlWriter.Create(GagarinEnvironmentInfo.UnifiedXmlFilePath, settings))
            {
                document.Save(writer);
            }

            stopwatch.Stop();
            Log.Warning($"GAGARIN: <color=white>Cache created!</color> creating cache took <color=green>{stopwatch.ElapsedMilliseconds / 1000} seconds</color>");
        }

        public static void Load(XmlDocument document, Dictionary<XmlNode, LoadableXmlAsset> assets)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                CheckCharacters = false
            };
            using StringReader input = new StringReader(File.ReadAllText(GagarinEnvironmentInfo.UnifiedXmlFilePath));
            using XmlReader xmlReader = XmlReader.Create(input, settings);
            LoadableXmlAsset defaultLoadable = new LoadableXmlAsset(Context.Core.Name, GagarinEnvironmentInfo.UnifiedXmlFilePath, "<Empty />")
            {
                mod = Context.Core
            };
            string path;
            XmlNode defXml;
            assets.Clear();
            document.RemoveAll();
            document.AppendChild(document.CreateElement("Defs"));
            XmlDocument unifiedDocument = new XmlDocument();
            unifiedDocument.RemoveAll();
            unifiedDocument.Load(xmlReader);

            foreach (XmlElement element in unifiedDocument.DocumentElement.ChildNodes)
            {
                defXml = document.ImportNode(element.FirstChild, true);
                path = element.GetAttribute("path");

                if (!Context.XmlAssets.TryGetValue(path, out LoadableXmlAsset asset))
                    asset = defaultLoadable;

                assets[defXml] = asset;
                document.DocumentElement.AppendChild(defXml);
            }

            stopwatch.Stop();
            Log.Warning($"GAGARIN: <color=green>Loaded from cache!</color> Loading cache took <color=red>{stopwatch.ElapsedMilliseconds / 1000} seconds</color>");
        }

        private static void ResolvePushRecursively(XmlInheritanceNode inheritanceNode)
        {
            var node = inheritanceNode.xmlNode as XmlElement;
            var name = node.HasAttribute("Name") ? node.GetAttribute("Name") : null;

            if (name != null)
            {
                if (registeredNames.Contains(name))
                    return;

                registeredNames.Add(name);
            }
            if (inheritanceNode.parent != null)
                ResolvePushRecursively(inheritanceNode.parent);

            XmlElement wrapper = WrapXmlNode(inheritanceNode.xmlNode,
                Context.DefsXmlAssets.TryGetValue(inheritanceNode.xmlNode, out LoadableXmlAsset asset) ? asset.FullFilePath : null);
            document.DocumentElement.AppendChild(wrapper);
        }

        private static XmlElement WrapXmlNode(XmlNode node, string path = null)
        {
            XmlElement xml = document.CreateElement("Item");
            xml.SetAttribute("path", path ?? string.Empty);
            xml.AppendChild(document.ImportNode(node, true));
            return xml;
        }
    }
}
