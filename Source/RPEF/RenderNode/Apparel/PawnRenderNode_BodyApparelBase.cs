using RimWorld;
using UnityEngine;
using Verse;

namespace RPEF
{
    public class PawnRenderNode_BodyApparelBase : PawnRenderNode_ApparelBase
    {
        protected override PawnRenderNodeTagDef ParentTagDef => PawnRenderNodeTagDefOf.ApparelBody;

        public PawnRenderNode_BodyApparelBase(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel) : base(pawn, props, tree, apparel)
        {
            var props_BodyApparelBase = props as PawnRenderNodeProperties_ApparelBase;
            if (props_BodyApparelBase == null) { return; }

            var lastLayer = props_BodyApparelBase.apparelThingDef.apparel.LastLayer;

            if (!pawn.Crawling)
            {
                if (lastLayer == ApparelLayerDefOf.Shell)
                {
                    drawDataOverride = DrawData.NewWithData(new DrawData.RotationalData(Rot4.North, 88f));
                }
                else if (lastLayer.IsUtilityLayer && props_BodyApparelBase.apparelThingDef.apparel.wornGraphicData.renderUtilityAsPack)
                {
                    drawDataOverride = DrawData.NewWithData(new DrawData.RotationalData(Rot4.North, 93f), new DrawData.RotationalData(Rot4.South, -3f));
                }
            }
        }
    }
}
