using Verse;

namespace RPEF
{
    public class PawnRenderNode_HairConstant : PawnRenderNode_Hair
    {
        public PawnRenderNode_HairConstant(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) 
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (pawn.DevelopmentalStage.Baby() || pawn.DevelopmentalStage.Newborn())
            {
                if (pawn.story?.hairDef == null || pawn.story.hairDef.noGraphic)
                {
                    return null;
                }

                return pawn.story.hairDef.GraphicFor(pawn, ColorFor(pawn));
            }

            return base.GraphicFor(pawn);
        }
    }
}
