using RimWorld;
using Verse;

namespace RPEF
{
    public class PawnRenderNodeWorker_ClosedEye : PawnRenderNodeWorker
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms)) { return false; }

            if (!parms.pawn.DeadOrDowned && parms.pawn.Awake()) { return false; }

            return true;
        }
    }
}
