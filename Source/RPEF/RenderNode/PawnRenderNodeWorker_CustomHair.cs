using UnityEngine;
using Verse;

namespace RPEF
{
    public class PawnRenderNodeWorker_CustomHair : PawnRenderNodeWorker_FlipWhenCrawling
    {
        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var baseScale = base.ScaleFor(node, parms);
            if (parms.pawn?.story?.hairDef is ScaleableHairDef scalableHairDef)
            {
                baseScale = new Vector3(baseScale.x * scalableHairDef.scale.x, baseScale.y, baseScale.z * scalableHairDef.scale.y);
            }

            return baseScale;
        }
    }
}
