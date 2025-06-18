using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RPEF
{
    public class CompProperties_TrailRenderer : CompProperties
    {
        public string texPath;
        public ShaderTypeDef shader;
        public Color color = Color.white;
        public SimpleCurve widthCurve = new SimpleCurve(new CurvePoint[] { new CurvePoint(0f, 1f), new CurvePoint(1f, 1f) });
        public LineTextureMode lineTextureMode = LineTextureMode.Stretch;

        public Vector3 additionalOffset;

        public int maxTrailPoint = 10;
        public int refreshIntervalTicks = 30;

        [Unsaved]
        public Material material;

        [Unsaved]
        public AnimationCurve aniCurve;

        public CompProperties_TrailRenderer()
        {
            compClass = typeof(CompTrailRenderer);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);

            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (texPath != null)
                {
                    material = MaterialPool.MatFrom(texPath, shader.Shader, color);
                }

                if (widthCurve != null)
                {
                    aniCurve = widthCurve.ToAnimationCurve();
                }
            });
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (parentDef.tickerType != TickerType.Normal)
            {
                yield return $"{parentDef.defName} with CompProperties_LineTrail must have tickerType as Normal.";
            }
        }
    }

    public class CompTrailRenderer : ThingComp
    {
        private Vector3[] points;
        private Mesh mesh;
        private bool trailStart;

        public CompProperties_TrailRenderer Props => (CompProperties_TrailRenderer)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            points = new Vector3[Props.maxTrailPoint];
        }

        public override void CompTick()
        {
            if (!parent.Spawned)
            {
                trailStart = false;
                return;
            }

            if (Props.refreshIntervalTicks > 0)
            {
                var position = parent.DrawPos + Props.additionalOffset;
                RegisterNewTrail(position);
            }
        }

        public override void PostDraw()
        {
            if (mesh != null)
            {
                Graphics.DrawMesh(mesh, new Vector3(), Quaternion.identity, Props.material, 0);
            }
        }

        public void RegisterNewTrail(Vector3 position)
        {
            if (!trailStart)
            {
                for (int i = 0; i < points.Length; ++i)
                {
                    points[i] = position;
                }

                trailStart = true;
            }

            if (parent.IsHashIntervalTick(Props.refreshIntervalTicks))
            {
                for (int i = points.Length - 1; i >= 1; i--)
                {
                    points[i] = points[i - 1];
                }
                points[0] = position;

                var lineRenderer = LineRendererManager.LineRenderer;
                lineRenderer.positionCount = points.Length;
                lineRenderer.material = Props.material;
                lineRenderer.textureMode = Props.lineTextureMode;
                lineRenderer.widthCurve = Props.aniCurve;

                for (int i = 0; i < points.Length; ++i)
                {
                    lineRenderer.SetPosition(i, points[points.Length - 1 - i]);
                }

                if (mesh == null)
                {
                    mesh = new Mesh();
                }
                lineRenderer.BakeMesh(mesh, Find.Camera);
            }
        }
    }
}
