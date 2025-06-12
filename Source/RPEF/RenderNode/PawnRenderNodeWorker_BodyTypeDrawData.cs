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
            var pivot = Vector3.zero;
            var bodyTypeDrawData = GetDrawDataForBodyType(node, parms.pawn.story?.bodyType);
            if (bodyTypeDrawData != null)
            {
                pivot -= (bodyTypeDrawData.PivotForRot(parms.facing) - DrawData.PivotCenter).ToVector3();
            }
            else
            {
                return base.PivotFor(node, parms);
            }

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

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            var anchorOffset = Vector3.zero;
            pivot = PivotFor(node, parms);

            var bodyTypeDrawData = GetDrawDataForBodyType(node, parms.pawn.story?.bodyType);
            if (bodyTypeDrawData != null)
            {

                if (bodyTypeDrawData.useBodyPartAnchor)
                {
                    if (node.bodyPart == null)
                    {
                        Log.ErrorOnce($"Attempted to use a body part anchor but no body-part record has been assigned to this node {node}", node.GetHashCode());
                        return anchorOffset;
                    }

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

                //if (!bodyTypeDrawData.useHediffAnchor && (node.hediff?.Part?.flipGraphic ?? false))
                //{
                //    anchorOffset.x *= -1f;
                //}

                anchorOffset += vector;
            }
            else
            {
                return base.OffsetFor(node, parms, out pivot);
            }

            anchorOffset += node.DebugOffset;

            if (!parms.flags.FlagSet(PawnRenderFlags.Portrait) && node.TryGetAnimationOffset(parms, out var offset))
            {
                anchorOffset += offset;
            }

            return anchorOffset;
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var bodyTypeDrawData = GetDrawDataForBodyType(node, parms.pawn.story?.bodyType);
            if (bodyTypeDrawData != null)
            {
                var scale = Vector3.one;
                scale.x *= node.Props.drawSize.x * node.debugScale;
                scale.z *= node.Props.drawSize.y * node.debugScale;

                if (!parms.flags.FlagSet(PawnRenderFlags.Portrait))
                {
                    if (node.TryGetAnimationScale(parms, out var offset))
                    {
                        scale = scale.ScaledBy(offset);
                    }

                    var graphicState = GetGraphicState(node, parms);
                    if (graphicState != null && graphicState.TryGetDefaultGraphic(out var graphic))
                    {
                        scale = scale.ScaledBy(new Vector3(graphic.drawSize.x, 1f, graphic.drawSize.y));
                    }
                }

                scale *= bodyTypeDrawData.ScaleFor(parms.pawn);
                return scale;
            }
            else
            {
                return base.ScaleFor(node, parms);
            }
        }

        public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
        {
            var rotation = node.DebugAngleOffset;
            var bodyTypeDrawData = GetDrawDataForBodyType(node, parms.pawn.story?.bodyType);
            if (bodyTypeDrawData != null)
            {
                rotation += bodyTypeDrawData.RotationOffsetForRot(parms.facing);
            }
            else
            {
                return base.RotationFor(node, parms);
            }

            if (!parms.flags.FlagSet(PawnRenderFlags.Portrait) && node.TryGetAnimationRotation(parms, out var offset))
            {
                rotation += offset;
            }

            if (node.FlipGraphic(parms))
            {
                rotation *= -1f;
            }

            return Quaternion.AngleAxis(rotation, Vector3.up);
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
