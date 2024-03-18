using RimWorld;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace RPEF
{
    public class BodyTextureOverride
    {
        public BodyTypeDef bodyTypeDef;
        public string path;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "bodyTypeDef", xmlRoot.Name);
            path = xmlRoot.FirstChild.Value;
        }
    }

    public class PawnRenderNodeProperties_BodyOverride : PawnRenderNodeProperties
    {
        public List<BodyTextureOverride> dessicatedGraphicPathOverrides;
        public List<BodyTextureOverride> rottingGraphicPathOverrides;
        public List<BodyTextureOverride> nakedGraphicPathOverrides;

        public bool allowsMutantOverride = true;
        public bool allowCreepJoinerOverride = true;
    }

    public class PawnRenderNode_BodyOverride : PawnRenderNode_Body
    {
        public new PawnRenderNodeProperties_BodyOverride Props => (PawnRenderNodeProperties_BodyOverride)props;

        public PawnRenderNode_BodyOverride(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            Shader shader = ShaderFor(pawn);
            if (shader == null)
            {
                return null;
            }

            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Rotting)
            {
                var overrides = Props.rottingGraphicPathOverrides?.FirstOrDefault(v => v.bodyTypeDef == pawn.story?.bodyType);
                if (overrides != null)
                {
                    return GraphicDatabase.Get<Graphic_Multi>(overrides.path, shader);
                }
            }

            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                var overrides = Props.dessicatedGraphicPathOverrides?.FirstOrDefault(v => v.bodyTypeDef == pawn.story?.bodyType);
                if (overrides != null)
                {
                    return GraphicDatabase.Get<Graphic_Multi>(overrides.path, shader);
                }
                else
                {
                    return GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, shader);
                }
            }

            if (Props.allowsMutantOverride && ModsConfig.AnomalyActive && pawn.IsMutant && !pawn.mutant.Def.bodyTypeGraphicPaths.NullOrEmpty())
            {
                return GraphicDatabase.Get<Graphic_Multi>(pawn.mutant.Def.GetBodyGraphicPath(pawn), shader, Vector2.one, ColorFor(pawn));
            }

            if (Props.allowCreepJoinerOverride && ModsConfig.AnomalyActive && pawn.IsCreepJoiner && pawn.story.bodyType != null && !pawn.creepjoiner.form.bodyTypeGraphicPaths.NullOrEmpty())
            {
                return GraphicDatabase.Get<Graphic_Multi>(pawn.creepjoiner.form.GetBodyGraphicPath(pawn), shader, Vector2.one, ColorFor(pawn));
            }

            if (pawn.story?.bodyType?.bodyNakedGraphicPath == null)
            {
                return null;
            }
            else
            {
                var overrides = Props.nakedGraphicPathOverrides?.FirstOrDefault(v => v.bodyTypeDef == pawn.story?.bodyType);
                if (overrides != null)
                {
                    return GraphicDatabase.Get<Graphic_Multi>(overrides.path, shader, Vector2.one, ColorFor(pawn));
                }
                else
                {
                    return GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, shader, Vector2.one, ColorFor(pawn));
                }
            }
        }
    }
}
