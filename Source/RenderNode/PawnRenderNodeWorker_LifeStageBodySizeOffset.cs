using RimWorld;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace RPEF
{
    public class OffsetData
    {
        public BodyTypeDef bodyType;
        public Vector3 offset;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "bodyType", xmlRoot.Name);
            offset = ParseHelper.FromString<Vector3>(xmlRoot.FirstChild.Value);
        }
    }

    public class PawnRenderNodeProperties_LifeStageBodySizeOffset : PawnRenderNodeProperties
    {
        public List<OffsetData> offsets;
    }

    public class PawnRenderNodeWorker_LifeStageBodySizeOffset : PawnRenderNodeWorker
    {
        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            var offset = base.OffsetFor(node, parms, out pivot);

            var props = node.Props as PawnRenderNodeProperties_LifeStageBodySizeOffset;
            if (props != null)
            {
                foreach (var offsetData in props.offsets)
                {
                    if (offsetData.bodyType == parms.pawn?.story?.bodyType)
                    {
                        var additionalOffset = offsetData.offset;

                        if (parms.pawn.ageTracker?.CurLifeStage != null)
                        {
                            additionalOffset *= Mathf.Sqrt(parms.pawn.ageTracker.CurLifeStage.bodySizeFactor);
                        }

                        offset += additionalOffset;
                        break;
                    }
                }
            }

            return offset;
        }
    }
}
