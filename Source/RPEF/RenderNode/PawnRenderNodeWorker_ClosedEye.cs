using RimWorld;
using Verse;

namespace RPEF
{
    public class PawnRenderNodeWorker_ClosedEye : PawnRenderNodeWorker
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms)) { return false; }

            if (parms.pawn.Dead) { return true; }

            if (parms.pawn.Downed && !parms.pawn.Awake()) { return true; }

            return false;
        }

        protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
        {
            return base.GetGraphic(node, parms);
        }
    }
}
