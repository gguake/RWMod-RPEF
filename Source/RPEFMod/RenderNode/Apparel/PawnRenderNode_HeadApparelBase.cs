using RimWorld;
using Verse;

namespace RPEF
{
    public class PawnRenderNode_HeadApparelBase : PawnRenderNode_ApparelBase
    {
        protected override PawnRenderNodeTagDef ParentTagDef => PawnRenderNodeTagDefOf.ApparelHead;

        public PawnRenderNode_HeadApparelBase(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }
    }
}
