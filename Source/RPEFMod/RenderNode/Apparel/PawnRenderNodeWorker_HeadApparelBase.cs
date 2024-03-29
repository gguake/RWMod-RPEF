using Verse;

namespace RPEF
{
    public class PawnRenderNodeWorker_HeadApparelBase : PawnRenderNodeWorker_Apparel_Head
    {
        public override float LayerFor(PawnRenderNode node, PawnDrawParms parms)
        {
            if (node is PawnRenderNode_HeadApparelBase node_HeadApparelBase)
            {
                var baseLayer = (node_HeadApparelBase.baseLayerOverride ?? node.Props.baseLayer) + node.debugLayerOffset;

                if (node_HeadApparelBase.drawDataOverride != null)
                {
                    return node_HeadApparelBase.drawDataOverride.LayerForRot(parms.facing, baseLayer) + node.debugLayerOffset;
                }
                else
                {
                    return (node.Props.drawData?.LayerForRot(parms.facing, baseLayer) ?? baseLayer) + node.debugLayerOffset;
                }
            }

            return base.LayerFor(node, parms);
        }
    }
}
