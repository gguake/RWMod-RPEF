using UnityEngine;
using Verse;

namespace RPEF
{
    public class PawnRenderNode_SleepFace : PawnRenderNode_Head
    {
        public PawnRenderNode_SleepFace(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) 
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (!pawn.health.hediffSet.HasHead)
            {
                return null;
            }

            var modExtension = pawn.story.headType.GetModExtension<SleepFaceHeadHook>();
            if (modExtension == null || modExtension.sleepGraphicPath == null) { return null; }

            var path = modExtension.sleepGraphicPath;
            if (path.NullOrEmpty())
            {
                return null;
            }

            var shader = ShaderFor(pawn);
            if (shader == null)
            {
                return null;
            }

            return GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, ColorFor(pawn));
        }
    }
}
