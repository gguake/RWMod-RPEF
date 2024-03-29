using RimWorld;
using UnityEngine;
using Verse;

namespace RPEF
{
    public class PawnRenderNodeWorker_BodyApparelBase : PawnRenderNodeWorker_Body
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms))
            {
                return false;
            }

            if (!parms.flags.FlagSet(PawnRenderFlags.Clothes))
            {
                return false;
            }

            return true;
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            var offset = base.OffsetFor(node, parms, out pivot);
            if (IsRenderAsPack(node.Props) && TryGetWornGraphicData(node.Props, out var wornGraphicData))
            {
                var beltOffset = wornGraphicData.BeltOffsetAt(parms.facing, parms.pawn.story.bodyType);
                offset.x += beltOffset.x;
                offset.z += beltOffset.y;
            }

            return offset;
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var scale = base.ScaleFor(node, parms);
            if (IsRenderAsPack(node.Props) && TryGetWornGraphicData(node.Props, out var wornGraphicData))
            {
                var beltScale = wornGraphicData.BeltScaleAt(parms.facing, parms.pawn.story.bodyType);
                scale.x *= beltScale.x;
                scale.z *= beltScale.y;
            }

            return scale;
        }

        public override float LayerFor(PawnRenderNode node, PawnDrawParms parms)
        {
            if (node is PawnRenderNode_BodyApparelBase node_BodyApparelBase)
            {
                var baseLayer = (node_BodyApparelBase.baseLayerOverride ?? node.Props.baseLayer) + node.debugLayerOffset;
                if (IsRenderAsPack(node.Props))
                {
                    parms.facing = parms.facing.Opposite;
                    parms.flipHead = false;
                }

                if (node_BodyApparelBase.drawDataOverride != null)
                {
                    return node_BodyApparelBase.drawDataOverride.LayerForRot(parms.facing, baseLayer) + node.debugLayerOffset;
                }
                else
                {
                    return (node.Props.drawData?.LayerForRot(parms.facing, baseLayer) ?? baseLayer) + node.debugLayerOffset;
                }
            }

            return base.LayerFor(node, parms);
        }

        private bool IsRenderAsPack(PawnRenderNodeProperties props)
        {
            return props is PawnRenderNodeProperties_ApparelBase props_BodyApparelBase &&
                props_BodyApparelBase.apparelThingDef.apparel.LastLayer.IsUtilityLayer &&
                props_BodyApparelBase.apparelThingDef.apparel.wornGraphicData.renderUtilityAsPack;
        }

        private bool TryGetWornGraphicData(PawnRenderNodeProperties props, out WornGraphicData wornGraphicData)
        {
            wornGraphicData = props is PawnRenderNodeProperties_ApparelBase props_BodyApparelBase ? props_BodyApparelBase.apparelThingDef.apparel.wornGraphicData : null;
            return wornGraphicData != null;
        }
    }
}
