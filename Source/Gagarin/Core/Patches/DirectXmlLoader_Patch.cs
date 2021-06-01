using System;
using System.Xml;
using Verse;

namespace Gagarin
{
    public static class DirectXmlLoader_Patch
    {
        [GagarinPatch(typeof(DirectXmlLoader), nameof(DirectXmlLoader.DefFromNode))]
        public static class DirectXmlLoader_DefFromNode_Patch
        {
            public static void Postfix(XmlNode node, LoadableXmlAsset loadingAsset, Def __result)
            {
                if (!Context.IsUsingCache && __result != null)
                    CachedDefHelper.Register(__result, node, loadingAsset);
            }
        }
    }
}
