using RimWorld;
using System.Linq;
using Verse;
using static HarmonyLib.Code;

namespace RPEF
{
    public class PawnRenderNodeProperties_BodyApparel : PawnRenderNodeProperties
    {
        public ThingDef apparelDef;
        public bool useBodyTypeGraphic;
    }

    public class PawnRenderNode_BodyApparel : PawnRenderNode
    {
        private new PawnRenderNodeProperties_BodyApparel Props => (PawnRenderNodeProperties_BodyApparel)props;

        public PawnRenderNode_BodyApparel(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
            apparel = tree.pawn.apparel.WornApparel.FirstOrDefault(v => v.def == Props.apparelDef);
        }

        protected override void EnsureMaterialsInitialized()
        {
            if (Props.useBodyTypeGraphic)
            {
                if (graphic == null && ApparelGraphicRecordGetter.TryGetGraphicApparel(apparel, tree.pawn.story.bodyType, out var rec))
                {
                    graphic = rec.graphic;
                }
            }
            else
            {
                base.EnsureMaterialsInitialized();
            }
        }
    }

    public class PawnRenderNodeWorker_BodyApparel : PawnRenderNodeWorker_Body
    {

        public override float LayerFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var index = node.parent.children.FirstIndexOf(v => v == node);
            var props = (PawnRenderNodeProperties_BodyApparel)node.Props;
            var lastLayer = node.apparel.def.apparel.LastLayer;

            var layerOffset = node.parent.Worker.LayerFor(node.parent, parms) + base.LayerFor(node, parms) + index;
            if (!parms.pawn.Crawling && lastLayer == ApparelLayerDefOf.Shell && parms.facing == Rot4.North)
            {
                layerOffset = 88f + index;
            }

            return layerOffset;
        }
    }
}
