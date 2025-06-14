using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace RPEF
{
    public class PawnRenderNodeProperties_ApparelBase : PawnRenderNodeProperties
    {
        public ThingDef apparelThingDef;
    }

    [StaticConstructorOnStartup]
    public class PawnRenderNode_ApparelBase : PawnRenderNode_Apparel
    {
        public float? baseLayerOverride = 0f;
        public DrawData drawDataOverride;

        private static FieldInfo _field_PawnRenderTree_nodesByTag = AccessTools.Field(typeof(PawnRenderTree), "nodesByTag");

        public PawnRenderNode_ApparelBase(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel, bool useHeadMesh) :
            base(pawn, GenerateNewProperties(pawn, tree, props, apparel), tree, apparel, useHeadMesh)
        {
        }

        public PawnRenderNode_ApparelBase(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel) :
            base(pawn, GenerateNewProperties(pawn, tree, props, apparel), tree, apparel)
        {
        }

        private static PawnRenderNodeProperties GenerateNewProperties(Pawn pawn, PawnRenderTree tree, PawnRenderNodeProperties originalProps, Apparel apparel)
        {
            var nodesByTag = _field_PawnRenderTree_nodesByTag.GetValue(tree) as Dictionary<PawnRenderNodeTagDef, PawnRenderNode>;

            var apparelWornIndex = pawn.apparel.WornApparel.IndexOf(apparel);


            PawnRenderNodeTagDef abstractParentApparelTagDef = originalProps.parentTagDef;
            if (originalProps.parentTagDef == null)
            {
                abstractParentApparelTagDef = (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) ?
                PawnRenderNodeTagDefOf.ApparelHead :
                PawnRenderNodeTagDefOf.ApparelBody;
            }

            PawnRenderNode parentNode = null;
            if (abstractParentApparelTagDef != null)
            {
                parentNode = nodesByTag[abstractParentApparelTagDef];
            }

            int layerOffset = 0;
            for (int i = 0; i < apparelWornIndex; ++i)
            {
                var wornApparel = pawn.apparel.WornApparel[i];
                if (wornApparel.def.IsApparel)
                {
                    var parentTagDef = wornApparel.def.apparel.parentTagDef;
                    if (parentTagDef == null)
                    {
                        parentTagDef = (wornApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || wornApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) ?
                            PawnRenderNodeTagDefOf.ApparelHead :
                            PawnRenderNodeTagDefOf.ApparelBody;
                    }

                    if (abstractParentApparelTagDef == parentTagDef)
                    {
                        layerOffset++;
                    }
                }
            }

            var props = new PawnRenderNodeProperties()
            {
                nodeClass = originalProps.nodeClass,
                workerClass = originalProps.workerClass,
                subworkerClasses = originalProps.subworkerClasses,
                texPath = originalProps.texPath,
                texPathFemale = originalProps.texPathFemale,
                parentTagDef = originalProps.parentTagDef,

                baseLayer = (parentNode?.Props.baseLayer ?? 0) + layerOffset,
                drawData = originalProps.drawData,
                children = originalProps.children,
            };

            return props;
        }
    }
}
