using RimWorld;
using Verse;

namespace RPEF
{
    public class PawnRenderNode_HeadApparelBase : PawnRenderNode_ApparelBase
    {
        protected override PawnRenderNodeTagDef ParentTagDef => PawnRenderNodeTagDefOf.ApparelHead;

        public PawnRenderNode_HeadApparelBase(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel) : base(pawn, props, tree, apparel)
        {
        }

        public PawnRenderNode_HeadApparelBase(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel, bool useHeadMesh) : base(pawn, props, tree, apparel, useHeadMesh)
        {
        }
    }
}
