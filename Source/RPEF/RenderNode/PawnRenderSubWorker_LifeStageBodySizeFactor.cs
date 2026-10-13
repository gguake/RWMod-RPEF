using Verse;
using Waylith.Math;

namespace RPEF
{
    public class PawnRenderSubWorker_LifeStageBodySizeFactor : PawnRenderSubWorker
    {
        public override void TransformOffset(PawnRenderNode node, PawnDrawParms parms, ref UnityEngine.Vector3 offset, ref UnityEngine.Vector3 pivot)
        {
            if (parms.pawn.ageTracker?.CurLifeStage != null)
            {
                offset *= Mathf.Sqrt(parms.pawn.ageTracker.CurLifeStage.bodySizeFactor);
            }
        }
    }
}
