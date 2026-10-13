using System;
using UnityEngine;
using Waylith.Graphics.Meshes;
using Mathf = Waylith.Math.Mathf;

namespace RPEF
{
    internal sealed class RibbonMeshGenerator
    {
        private const float MinLength = 0.000001f;

        private static readonly VertexLayout Layout = new VertexLayout(new[]
        {
            new VertexAttribute(VertexAttributeSemantic.Position, VertexAttributeFormat.Float32, 3, stream: 0),
            new VertexAttribute(VertexAttributeSemantic.TexCoord0, VertexAttributeFormat.Float32, 4, stream: 1)
        });

        private readonly float[] distances;
        private readonly Vector3[] outgoingDirections;

        public Vector3[] Vertices { get; }
        public Vector2[] UVs { get; }
        public int[] Triangles { get; }

        public RibbonMeshGenerator(int pointCount)
        {
            if (pointCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(pointCount));
            }

            distances = new float[pointCount];
            outgoingDirections = new Vector3[pointCount];
            Vertices = new Vector3[pointCount * 2];
            UVs = new Vector2[pointCount * 2];
            Triangles = new int[(pointCount - 1) * 6];

            for (int i = 0; i < pointCount - 1; ++i)
            {
                int vertex = i * 2;
                int triangle = i * 6;
                Triangles[triangle] = vertex;
                Triangles[triangle + 1] = vertex + 2;
                Triangles[triangle + 2] = vertex + 3;
                Triangles[triangle + 3] = vertex;
                Triangles[triangle + 4] = vertex + 3;
                Triangles[triangle + 5] = vertex + 1;
            }
        }

        // Input is newest first; vertices and UVs run from the tail to the head,
        // matching the order previously passed to LineRenderer.
        public bool Update(Vector3[] points, AnimationCurve widthCurve, LineTextureMode textureMode, Vector3 viewDirection)
        {
            if (points == null || points.Length != distances.Length)
            {
                throw new ArgumentException("Point count must match the ribbon buffers.", nameof(points));
            }

            int count = points.Length;
            distances[0] = 0f;
            for (int i = 1; i < count; ++i)
            {
                distances[i] = distances[i - 1] + Vector3.Distance(points[count - i], points[count - 1 - i]);
            }

            float length = distances[count - 1];
            if (length <= MinLength)
            {
                return false;
            }

            // Carry the nearest nonzero direction across coincident points.
            Vector3 outgoing = Vector3.zero;
            for (int i = count - 1; i >= 0; --i)
            {
                if (i < count - 1)
                {
                    Vector3 segment = points[count - 2 - i] - points[count - 1 - i];
                    if (segment.sqrMagnitude > 0f)
                    {
                        outgoing = segment / segment.magnitude;
                    }
                }
                outgoingDirections[i] = outgoing;
            }

            Vector3 incoming = Vector3.zero;
            Vector3 previousSide = Vector3.zero;
            for (int i = 0; i < count; ++i)
            {
                Vector3 point = points[count - 1 - i];
                if (i > 0)
                {
                    Vector3 segment = point - points[count - i];
                    if (segment.sqrMagnitude > 0f)
                    {
                        incoming = segment / segment.magnitude;
                    }
                }

                Vector3 tangent = incoming + outgoingDirections[i];
                if (tangent.sqrMagnitude <= MinLength * MinLength)
                {
                    tangent = outgoingDirections[i].sqrMagnitude > 0f ? outgoingDirections[i] : incoming;
                }

                Vector3 side = Vector3.Cross(tangent, viewDirection);
                if (side.sqrMagnitude <= MinLength * MinLength)
                {
                    side = previousSide;
                    if (side.sqrMagnitude <= MinLength * MinLength)
                    {
                        Vector3 axis = Mathf.Abs(tangent.normalized.y) < 0.9f ? Vector3.up : Vector3.forward;
                        side = Vector3.Cross(tangent, axis);
                    }
                }
                side /= side.magnitude;
                previousSide = side;

                float progress = distances[i] / length;
                float width = Mathf.Max(0f, widthCurve?.Evaluate(progress) ?? 1f);
                Vector3 halfWidth = side * (width * 0.5f);
                Vertices[i * 2] = point - halfWidth;
                Vertices[i * 2 + 1] = point + halfWidth;

                float u;
                switch (textureMode)
                {
                    case LineTextureMode.Stretch:
                        u = progress;
                        break;
                    case LineTextureMode.DistributePerSegment:
                        u = (float)i / (count - 1);
                        break;
                    case LineTextureMode.RepeatPerSegment:
                        u = i;
                        break;
                    default:
                        // Tile and LineRenderer's Static mode use accumulated distance.
                        u = distances[i];
                        break;
                }
                UVs[i * 2] = new Vector2(u, 0f);
                UVs[i * 2 + 1] = new Vector2(u, 1f);
            }

            return true;
        }

        public void Upload(UnityEngine.Mesh mesh)
        {
            var engineMesh = mesh.EngineMesh;
            var storage = engineMesh.CpuStorage;
            if (storage.VertexCount != Vertices.Length || storage.IndexCount != Triangles.Length || storage.Layout != Layout)
            {
                storage.SetVertexLayout(Layout, Vertices.Length);
                storage.SetIndexBufferParams(Triangles.Length, MeshIndexFormat.UInt32);
                storage.SetSubMeshCount(1);
                Triangles.AsSpan().CopyTo(storage.GetIndexBuffer().AsSpan<int>());
            }

            // The Unity-compatible property setters rebuild the entire mesh on
            // each assignment. Update the existing Waylith buffers instead.
            Vertices.AsSpan().CopyTo(storage.GetVertexStream(0).AsSpan<Vector3>());
            var uvBuffer = storage.GetVertexStream(1).AsSpan<Vector4>();
            Vector3 min = Vertices[0];
            Vector3 max = Vertices[0];
            for (int i = 0; i < Vertices.Length; ++i)
            {
                Vector2 uv = UVs[i];
                uvBuffer[i] = new Vector4(uv.x, uv.y, 0f, 0f);
                min = Vector3.Min(min, Vertices[i]);
                max = Vector3.Max(max, Vertices[i]);
            }

            var bounds = new MeshBounds(min.x, min.y, min.z, max.x, max.y, max.z);
            storage.Bounds = bounds;
            storage.SetSubMesh(0, new SubMesh(0, Triangles.Length, Waylith.Graphics.Meshes.MeshTopology.Triangles, 0, bounds));
            engineMesh.MarkCpuStorageChanged();
        }
    }
}
