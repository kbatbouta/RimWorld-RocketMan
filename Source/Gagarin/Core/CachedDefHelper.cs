using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Xml;
using Mono.Security.X509.Extensions;
using RocketMan;
using Verse;
using static Verse.XmlInheritance;

namespace Gagarin
{
    public static class CachedDefHelper
    {
        private static XmlDocument document;

        // private static XmlDocument t_unresolvedDocument = new XmlDocument();
        // private static XmlDocument t_resolvedDocument = new XmlDocument();
        // private static XmlDocument t_bases = new XmlDocument();

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

            // t_unresolvedDocument.AppendChild(t_unresolvedDocument.CreateElement("Def"));
            // t_resolvedDocument.AppendChild(t_resolvedDocument.CreateElement("Def"));
        }

        public static void Clean()
        {
            defs.Clear();
            registeredNames.Clear();
            document.RemoveAll();
            document = null;
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
            XmlElement resolvedNode;

            foreach (DefXmlUnit unit in defs)
            {
                XmlElement node = unit.node as XmlElement;
                if (unit.inheritanceNode == null)
                {
                    wrapper = WrapXmlNode(node, unit.asset?.FullFilePath);
                    root.AppendChild(wrapper);
                    continue;
                }
                if (unit.inheritanceNode.resolvedXmlNode == null)
                {
                    Log.Error($"GAGARIN: {unit.def.defName} has <color=yellow>resolvedXmlNode == null!</color>");
                    continue;
                }

                resolvedNode = unit.inheritanceNode.resolvedXmlNode as XmlElement;
                resolvedNode.RemoveAttribute("ParentName");

                if (resolvedNode.Name != node.Name)
                {
                    XmlElement temp = document.CreateElement(node.Name);
                    foreach (XmlNode n in resolvedNode.ChildNodes)
                    {
                        if (n.NodeType != XmlNodeType.Element)
                            continue;
                        temp.AppendChild(document.ImportNode(n, true));
                    }
                    resolvedNode = temp;
                }
                else if (node.HasAttribute("Class") && !resolvedNode.HasAttribute("Class"))
                    resolvedNode.SetAttribute("Class", node.GetAttribute("Class"));

                wrapper = WrapXmlNode(resolvedNode, unit.asset?.FullFilePath);
                wrapper.SetAttribute("resolved", "true");

                root.AppendChild(wrapper);

                // wrapper = WrapXmlNode(unit.inheritanceNode.resolvedXmlNode,
                //        Context.DefsXmlAssets.TryGetValue(unit.inheritanceNode.xmlNode, out LoadableXmlAsset asset) ? asset.FullFilePath : null);
                // document.DocumentElement.AppendChild(wrapper);
                //
                // if (unit.inheritanceNode.resolvedXmlNode != null)
                // {
                //    t_unresolvedDocument.DocumentElement.AppendChild(t_unresolvedDocument.ImportNode(unit.inheritanceNode.xmlNode, true));
                //    t_resolvedDocument.DocumentElement.AppendChild(t_resolvedDocument.ImportNode(unit.inheritanceNode.resolvedXmlNode, true));
                // }
                // ResolvePushRecursively(unit.inheritanceNode);
            }

            // t_resolvedDocument.Save(Path.Combine(RocketEnvironmentInfo.ConfigFolderPath, "Dump/resolved_def.xml"));
            // t_unresolvedDocument.Save(Path.Combine(RocketEnvironmentInfo.ConfigFolderPath, "Dump/unresolved_def.xml"));

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
            Stopwatch documentStopwatch = new Stopwatch();
            documentStopwatch.Start();
            unifiedDocument.Load(xmlReader);
            documentStopwatch.Stop();
            Log.Warning($"GAGARIN: <color=green>Loadeding XmlDocument</color> took <color=red>{(float)documentStopwatch.ElapsedTicks / Stopwatch.Frequency} seconds</color>");

            foreach (XmlElement element in unifiedDocument.DocumentElement.ChildNodes)
            {
                defXml = document.ImportNode(element.FirstChild, true);
                path = element.GetAttribute("path");

                if (Context.XmlAssets.TryGetValue(path, out LoadableXmlAsset asset))
                    assets[defXml] = asset;

                document.DocumentElement.AppendChild(defXml);
            }

            stopwatch.Stop();
            Log.Warning($"GAGARIN: <color=green>Loaded from cache!</color> Loading cache took <color=red>{stopwatch.ElapsedMilliseconds / 1000} seconds</color>");
        }

        private static XmlElement WrapXmlNode(XmlNode node, string path = null)
        {
            XmlElement xml = document.CreateElement("Item");
            xml.SetAttribute("path", path ?? string.Empty);
            xml.AppendChild(document.ImportNode(node, true));
            return xml;
        }

        // private static void ResolvePushRecursively(XmlInheritanceNode inheritanceNode)
        // {
        //    var node = inheritanceNode.xmlNode as XmlElement;
        //    var name = node.HasAttribute("Name") ? node.GetAttribute("Name") : null;
        //
        //    if (name != null)
        //    {
        //        if (registeredNames.Contains(name))
        //            return;
        //
        //        registeredNames.Add(name);
        //    }
        //    if (inheritanceNode.parent != null)
        //        ResolvePushRecursively(inheritanceNode.parent);
        //
        //    XmlElement wrapper = WrapXmlNode(inheritanceNode.xmlNode,
        //        Context.DefsXmlAssets.TryGetValue(inheritanceNode.xmlNode, out LoadableXmlAsset asset) ? asset.FullFilePath : null);
        //    document.DocumentElement.AppendChild(wrapper);
        // }        
    }
}
