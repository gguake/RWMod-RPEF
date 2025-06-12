using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RPEF
{
    public class PawnRenderNodeProperties_ApparelBase : PawnRenderNodeProperties
    {
        public ThingDef apparelThingDef;
    }

    [StaticConstructorOnStartup]
    public abstract class PawnRenderNode_ApparelBase : PawnRenderNode_Apparel
    {
        public float? baseLayerOverride = 0f;
        public DrawData drawDataOverride;

        protected abstract PawnRenderNodeTagDef ParentTagDef { get; }

        private static FieldInfo _field_PawnRenderTree_nodesByTag = AccessTools.Field(typeof(PawnRenderTree), "nodesByTag");
        private static FieldInfo _field_PawnRenderTree_layerOffsets = AccessTools.Field(typeof(PawnRenderTree), "layerOffsets");

        public PawnRenderNode_ApparelBase(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel, bool useHeadMesh) : base(pawn, props, tree, apparel, useHeadMesh)
        {
            Init(props);
        }

        public PawnRenderNode_ApparelBase(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Apparel apparel) : base(pawn, props, tree, apparel)
        {
            Init(props);
        }

        private void Init(PawnRenderNodeProperties props)
        {
            var parentTagDef = ParentTagDef;
            if (props.parentTagDef == ParentTagDef)
            {
                var nodesByTag = _field_PawnRenderTree_nodesByTag.GetValue(tree) as Dictionary<PawnRenderNodeTagDef, PawnRenderNode>;
                var layerOffsets = _field_PawnRenderTree_layerOffsets.GetValue(tree) as Dictionary<PawnRenderNode, float>;

                if (nodesByTag == null || !nodesByTag.TryGetValue(parentTagDef, out var parentNode)) { return; }

                var parentLayerOffset = 0f;
                if (layerOffsets != null && layerOffsets.ContainsKey(parentNode))
                {
                    parentLayerOffset = layerOffsets[parentNode];
                }

                baseLayerOverride = parentNode.Props.baseLayer + parentLayerOffset;

                if (parentNode != null)
                {
                    if (layerOffsets.ContainsKey(parentNode))
                    {
                        layerOffsets[parentNode]++;
                    }
                    else
                    {
                        layerOffsets.Add(parentNode, 1f);
                    }
                }
            }
        }
    }
}
