using RimWorld;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace RPEF
{
    public class BodyTypeDrawData
    {
        public BodyTypeDef bodyType;
        public DrawData drawData;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "bodyType", xmlRoot.Name);
            drawData = DirectXmlToObject.ObjectFromXml<DrawData>(xmlRoot, true);
        }
    }

    public class PawnRenderNodeProperties_BodyTypeDrawData : PawnRenderNodeProperties
    {
        public List<BodyTypeDrawData> bodyTypeDrawData;
    }

    public class PawnRenderNodeWorker_BodyTypeDrawData : PawnRenderNodeWorker
    {
        private DrawData GetDrawDataForBodyType(PawnRenderNode node, BodyTypeDef bodyType)
        {
            var props = node.Props as PawnRenderNodeProperties_BodyTypeDrawData;
            if (props?.bodyTypeDrawData != null && bodyType != null)
            {
                foreach (var bodyTypeDrawData in props.bodyTypeDrawData)
                {
                    if (bodyTypeDrawData.bodyType == bodyType)
                    {
                        return bodyTypeDrawData.drawData;
                    }
                }
            }

            return props.drawData;
        }

        protected override Vector3 PivotFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var bodyTypeDrawData = GetDrawDataForBodyType(node, parms.pawn.story?.bodyType);
            if (bodyTypeDrawData != null)
            {
                var pivot = Vector3.zero;
                pivot -= (bodyTypeDrawData.PivotForRot(parms.facing) - DrawData.PivotCenter).ToVector3();

                if (node.tree.TryGetAnimationPartForNode(node, out var animationPart))
                {
                    pivot = (animationPart.pivot - DrawData.PivotCenter).ToVector3();
                }
                if (node.debugPivotOffset != DrawData.PivotCenter)
                {
                    pivot.x += node.debugPivotOffset.x - DrawData.PivotCenter.x;
                    pivot.z += node.debugPivotOffset.y - DrawData.PivotCenter.y;
                }
                return pivot;

            }

            return base.PivotFor(node, parms);
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            var bodyTypeDrawData = GetDrawDataForBodyType(node, parms.pawn.story?.bodyType);
            if (bodyTypeDrawData != null)
            {
                var anchorOffset = Vector3.zero;
                pivot = PivotFor(node, parms);

                if (node.hediff != null && bodyTypeDrawData.useHediffAnchor)
                {
                    foreach (BodyTypeDef.WoundAnchor item in PawnDrawUtility.FindAnchors(parms.pawn, node.hediff.Part))
                    {
                        if (PawnDrawUtility.AnchorUsable(parms.pawn, item, parms.facing))
                        {
                            PawnDrawUtility.CalcAnchorData(parms.pawn, item, parms.facing, out anchorOffset, out var _);
                        }
                    }
                }

                var vector = bodyTypeDrawData.OffsetForRot(parms.facing);
                if (bodyTypeDrawData.scaleOffsetByBodySize && parms.pawn.story != null)
                {
                    Vector2 bodyGraphicScale = parms.pawn.story.bodyType.bodyGraphicScale;
                    float num = (bodyGraphicScale.x + bodyGraphicScale.y) / 2f;
                    vector *= num;
                }

                if (!bodyTypeDrawData.useHediffAnchor && (node.hediff?.Part?.flipGraphic ?? false))
                {
                    anchorOffset.x *= -1f;
                }

                anchorOffset += vector;

                anchorOffset += node.DebugOffset;
                if (node.AnimationWorker != null && node.AnimationWorker.Enabled() && !parms.flags.FlagSet(PawnRenderFlags.Portrait))
                {
                    anchorOffset += node.AnimationWorker.OffsetAtTick(node.tree.AnimationTick, parms);
                }

                return anchorOffset;
            }

            return base.OffsetFor(node, parms, out pivot);
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var bodyTypeDrawData = GetDrawDataForBodyType(node, parms.pawn.story?.bodyType);
            if (bodyTypeDrawData != null)
            {
                var scale = Vector3.one;

                scale.x *= node.Props.drawSize.x * node.debugScale;
                scale.z *= node.Props.drawSize.y * node.debugScale;
                if (node.AnimationWorker != null && node.AnimationWorker.Enabled() && !parms.flags.FlagSet(PawnRenderFlags.Portrait))
                {
                    scale = scale.MultipliedBy(node.AnimationWorker.ScaleAtTick(node.tree.AnimationTick, parms));
                }

                scale *= bodyTypeDrawData.ScaleFor(parms.pawn);
                return scale;
            }

            return base.ScaleFor(node, parms);
        }

        public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var bodyTypeDrawData = GetDrawDataForBodyType(node, parms.pawn.story?.bodyType);
            if (bodyTypeDrawData != null)
            {
                float rotation = node.DebugAngleOffset + bodyTypeDrawData.RotationOffsetForRot(parms.facing);

                if (node.AnimationWorker != null && node.AnimationWorker.Enabled() && !parms.flags.FlagSet(PawnRenderFlags.Portrait))
                {
                    rotation += node.AnimationWorker.AngleAtTick(node.tree.AnimationTick, parms);
                }
                if (node.hediff?.Part?.flipGraphic ?? false)
                {
                    rotation *= -1f;
                }
                return Quaternion.AngleAxis(rotation, Vector3.up);

            }

            return base.RotationFor(node, parms);
        }

        public override float LayerFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var bodyTypeDrawData = GetDrawDataForBodyType(node, parms.pawn.story?.bodyType);
            if (bodyTypeDrawData != null)
            {
                return bodyTypeDrawData.LayerForRot(parms.facing, node.Props.baseLayer) + node.debugLayerOffset;
            }

            return base.LayerFor(node, parms);
        }
    }
}
