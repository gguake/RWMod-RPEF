using Verse;

namespace RPEF;

public class PawnRenderNodeWorker_SleepFace : PawnRenderNodeWorker
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        if (!base.CanDrawNow(node, parms))
        {
            return false;
        }
        Pawn pawn = parms.pawn;
        if (pawn.Dead)
        {
            return false;
        }
        if (pawn.CurJob != null && pawn.jobs.curDriver != null)
        {
            return pawn.jobs.curDriver.asleep;
        }
        return false;
    }
}