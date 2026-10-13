using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RPEF
{
    /// <summary>
    /// 대상에게 트레일을 남기는 컴포넌트. 틱마다 현재 위치를 기록해 잔상을 남김.
    /// </summary>
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
        public int fadeOutTicks;

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

            if (maxTrailPoint < 2)
            {
                yield return $"{parentDef.defName} with CompProperties_TrailRenderer must have maxTrailPoint of at least 2.";
            }

            if (fadeOutTicks < 0)
            {
                yield return $"{parentDef.defName} with CompProperties_TrailRenderer must have nonnegative fadeOutTicks.";
            }
        }
    }

    public class CompTrailRenderer : ThingComp
    {
        private MapComponent_Trails trailManager;

        public CompProperties_TrailRenderer Props => (CompProperties_TrailRenderer)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            ResetTrail();
            trailManager = parent.Map.GetComponent<MapComponent_Trails>();
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            FinishTrail();
            base.PostDeSpawn(map, mode);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            FinishTrail();
            base.PostDestroy(mode, previousMap);
        }

        public override void CompTick()
        {
            if (!parent.Spawned)
            {
                FinishTrail();
                return;
            }

            if (Props.refreshIntervalTicks > 0)
            {
                var position = parent.DrawPos + Props.additionalOffset;
                RegisterNewTrail(position);
            }
        }

        public void RegisterNewTrail(Vector3 position)
        {
            if (!parent.Spawned)
            {
                return;
            }

            if (trailManager == null || trailManager.map != parent.Map)
            {
                FinishTrail();
                trailManager = parent.Map.GetComponent<MapComponent_Trails>();
            }

            trailManager.RegisterPoint(this, position);
        }

        public void FinishTrail()
        {
            trailManager?.FinishTrail(this);
            trailManager = null;
        }

        public void ResetTrail()
        {
            trailManager?.ResetTrail(this);
        }
    }
}
