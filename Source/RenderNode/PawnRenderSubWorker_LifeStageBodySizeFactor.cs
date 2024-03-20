using UnityEngine;
using Verse;

namespace RPEF
{
    public class PawnRenderSubWorker_LifeStageBodySizeFactor : PawnRenderSubWorker
    {
        public override void TransformOffset(PawnRenderNode node, PawnDrawParms parms, ref Vector3 offset, ref Vector3 pivot)
        {
            if (parms.pawn.ageTracker?.CurLifeStage != null)
            {
                offset *= Mathf.Sqrt(parms.pawn.ageTracker.CurLifeStage.bodySizeFactor);
            }
        }
    }
}
