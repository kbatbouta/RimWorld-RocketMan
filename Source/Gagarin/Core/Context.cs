using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace Gagarin
{
    public static class Context
    {
        public static bool IsUsingCache = false;

        public static bool IsLoadingModXML = false;

        public static bool IsRecovering = false;

        public static bool LoadingFinished = false;

        public static ModContentPack Core;

        public static Dictionary<XmlNode, LoadableXmlAsset> DefsXmlAssets = new Dictionary<XmlNode, LoadableXmlAsset>();

        public static Dictionary<string, LoadableXmlAsset> XmlAssets = new Dictionary<string, LoadableXmlAsset>();

        public static List<ModContentPack> RunningMods = new List<ModContentPack>();

        public static Dictionary<string, UInt64> AssetsHashes = new Dictionary<string, UInt64>();

        public static ModContentPack CurrentLoadingMod;
    }
}
