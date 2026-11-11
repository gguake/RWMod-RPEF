using RimWorld;
using UnityEngine;
using Verse;

namespace RPEF
{
    public class PawnRenderNodeWorker_CustomHair : PawnRenderNodeWorker_FlipWhenCrawling
    {
        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var baseScale = base.ScaleFor(node, parms);

            var hairDef = parms.pawn?.story?.hairDef;
            if (hairDef != null && TryGetHairScale(hairDef, out var scale))
            {
                baseScale = new Vector3(baseScale.x * scale.x, baseScale.y, baseScale.z * scale.y);
            }

            return baseScale;
        }

        public static bool TryGetHairScale(HairDef hairDef, out Vector2 scale)
        {
            var extension = hairDef.GetModExtension<HairScaleExtension>();
            if (extension != null)
            {
                scale = extension.scale;
                return true;
            }

#pragma warning disable CS0618 // 이전 XML 호환
            if (hairDef is ScaleableHairDef scaleableHairDef)
            {
                scale = scaleableHairDef.scale;
                return true;
            }
#pragma warning restore CS0618

            scale = Vector2.one;
            return false;
        }
    }
}
