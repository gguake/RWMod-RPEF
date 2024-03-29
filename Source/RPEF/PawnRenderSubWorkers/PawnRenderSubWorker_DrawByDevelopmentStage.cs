using Verse;

namespace RPEF
{
    public abstract class PawnRenderSubWorker_DrawConditionDevelopmentStage : PawnRenderSubWorker
    {
        public abstract DevelopmentalStage DevelopmentalStage { get; }

        public override bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms)
        {
            if ((parms.pawn.DevelopmentalStage & DevelopmentalStage) == DevelopmentalStage.None)
            {
                return false;
            }

            return true;
        }
    }
}
