using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RPEF
{
    public sealed class MapComponent_Trails : MapComponent
    {
        private readonly Dictionary<CompTrailRenderer, Trail> activeTrails = new Dictionary<CompTrailRenderer, Trail>();
        private readonly List<Trail> trails = new List<Trail>();

        public MapComponent_Trails(Map map) : base(map) { }

        internal void RegisterPoint(CompTrailRenderer source, Vector3 position)
        {
            if (!activeTrails.TryGetValue(source, out var trail))
            {
                trail = new Trail(source.parent, source.Props);
                activeTrails.Add(source, trail);
                trails.Add(trail);
            }

            trail.RegisterPoint(position);
        }

        internal void FinishTrail(CompTrailRenderer source)
        {
            if (activeTrails.Remove(source, out var trail) && !trail.Finish(GenTicks.TicksGame))
            {
                RemoveTrail(trail);
            }
        }

        internal void ResetTrail(CompTrailRenderer source)
        {
            if (activeTrails.Remove(source, out var trail))
            {
                RemoveTrail(trail);
            }
        }

        public override void MapComponentTick()
        {
            int tick = GenTicks.TicksGame;
            for (int i = trails.Count - 1; i >= 0; --i)
            {
                if (!trails[i].Tick(tick))
                {
                    trails[i].ReleaseMesh();
                    trails.RemoveAt(i);
                }
            }
        }

        public override void MapComponentDraw()
        {
            Vector3 viewDirection = Find.Camera != null ? Find.Camera.transform.forward : Vector3.down;
            foreach (var trail in trails)
            {
                trail.Draw(viewDirection);
            }
        }

        public override void MapRemoved()
        {
            foreach (var trail in trails)
            {
                trail.ReleaseMesh();
            }
            trails.Clear();
            activeTrails.Clear();
        }

        private void RemoveTrail(Trail trail)
        {
            trail.ReleaseMesh();
            trails.Remove(trail);
        }

        private sealed class Trail
        {
            private readonly Vector3[] points;
            private readonly RibbonMeshGenerator ribbon;
            private readonly CompProperties_TrailRenderer props;
            private Thing owner;
            private Mesh mesh;
            private bool hasPoints;
            private bool dirty;
            private bool finishing;
            private int finishTick;
            private int lifetimeTicks;
            private int segmentCount;
            private int removedSegments;

            public Trail(Thing owner, CompProperties_TrailRenderer props)
            {
                this.owner = owner;
                this.props = props;
                points = new Vector3[System.Math.Max(2, props.maxTrailPoint)];
                ribbon = new RibbonMeshGenerator(points.Length);
            }

            public void RegisterPoint(Vector3 position)
            {
                if (!hasPoints)
                {
                    for (int i = 0; i < points.Length; ++i)
                    {
                        points[i] = position;
                    }
                    hasPoints = true;
                }

                for (int i = points.Length - 1; i >= 1; --i)
                {
                    points[i] = points[i - 1];
                }
                points[0] = position;
                dirty = true;
            }

            public bool Finish(int tick)
            {
                finishing = true;
                finishTick = tick;
                lifetimeTicks = props.fadeOutTicks;
                owner = null;

                // Ignore the repeated initial position used to fill a short trail's buffer.
                int pointCount = points.Length;
                while (pointCount > 1 && (points[pointCount - 1] - points[pointCount - 2]).sqrMagnitude == 0f)
                {
                    --pointCount;
                }
                segmentCount = pointCount - 1;
                return lifetimeTicks > 0 && segmentCount > 0;
            }

            public bool Tick(int tick)
            {
                if (!finishing)
                {
                    return true;
                }

                int age = System.Math.Max(0, tick - finishTick);
                if (age >= lifetimeTicks)
                {
                    return false;
                }

                int targetRemovedSegments = (int)((long)age * segmentCount / lifetimeTicks);
                if (targetRemovedSegments != removedSegments)
                {
                    int tailIndex = segmentCount - targetRemovedSegments;
                    Vector3 tail = points[tailIndex];
                    for (int i = tailIndex + 1; i < points.Length; ++i)
                    {
                        points[i] = tail;
                    }
                    removedSegments = targetRemovedSegments;
                    dirty = true;
                }
                return true;
            }

            public void Draw(Vector3 viewDirection)
            {
                if (props.material == null || (owner != null && !owner.def.seeThroughFog && owner.Fogged()))
                {
                    return;
                }

                if (dirty)
                {
                    dirty = false;
                    if (!ribbon.Update(points, props.aniCurve, props.lineTextureMode, viewDirection))
                    {
                        mesh?.Clear();
                        return;
                    }

                    if (mesh == null)
                    {
                        mesh = new Mesh();
                        mesh.MarkDynamic();
                    }
                    ribbon.Upload(mesh);
                }

                if (mesh != null && mesh.vertexCount > 0)
                {
                    Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, props.material, 0);
                }
            }

            public void ReleaseMesh()
            {
                if (mesh != null)
                {
                    Object.Destroy(mesh);
                    mesh = null;
                }
            }
        }
    }
}
