using HarmonyLib;
using System.Xml;
using Verse;

namespace RPEF
{
    public static partial class HarmonyPatches
    {
        private static void PatchXML(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(XmlInheritance), nameof(XmlInheritance.TryRegister)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(XmlInheritance_TryRegister_Prefix)));
        }

        private static void XmlInheritance_TryRegister_Prefix(XmlNode node, ModContentPack mod)
        {
            if (node.Attributes?.Count > 0)
            {
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    var attr = node.Attributes[i];
                    if (attr.Name == "HasImport" && attr.Value.ToLower() == "true")
                    {
                        ProcessImportingNodesRecursively(node);
                        break;
                    }
                }
            }
        }

        private static void ProcessImportingNodesRecursively(XmlNode node)
        {
            if (node == null) { return; }

            if (node.Attributes?.Count > 0)
            {
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    var attr = node.Attributes[i];
                    if (attr.Name == "ImportPath")
                    {
                        var document = node.OwnerDocument;
                        if (document == null) { continue; }

                        var selected = document.SelectNodes(attr.Value);
                        if (selected != null || selected.Count > 0)
                        {
                            for (int j = 0; j < selected.Count; ++j)
                            {
                                node.InnerXml += selected[j].InnerXml;
                            }
                        }
                    }
                }
            }

            if (node.ChildNodes?.Count > 0)
            {
                for (int i = 0; i < node.ChildNodes.Count; ++i)
                {
                    var child = node.ChildNodes[i];
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        ProcessImportingNodesRecursively(child);
                    }
                }
            }
        }

        private static AnimationDef Pawn_FlightTracker_GetBestFlyAnimation_Injection(Pawn pawn)
        {
            var modExtension = pawn.def.GetModExtension<HumanlikeFlyExtension>();
            if (modExtension != null)
            {
                var data = modExtension.animationData.FirstOrDefault(v => v.lifeStage == pawn.ageTracker.CurLifeStage);
                if (data != null)
                {
                    var rot = pawn.Rotation;
                    if (rot == Rot4.South)
                    {
                        return data.animationSouth;
                    }
                    else if (rot == Rot4.North)
                    {
                        return data.animationNorth;
                    }
                    else if (rot == Rot4.East)
                    {
                        return data.animationEast;
                    }
                    else
                    {
                        return data.animationWest;
                    }
                }
            }

            return null;
        }

    }
}
