using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ZombieWorld
{
    // Builds flat-shaded meshes whose UVs point at a single palette texel, so one material covers everything.
    public sealed class LowPolyMeshBuilder
    {
        public const int PaletteSize = 16;

        static readonly Vector3[] FaceNormals =
        {
            Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
        };

        readonly List<Vector3> _vertices = new List<Vector3>();
        readonly List<Vector3> _normals = new List<Vector3>();
        readonly List<Vector2> _uvs = new List<Vector2>();
        readonly List<int> _triangles = new List<int>();

        public LowPolyMeshBuilder AddBox(Vector3 center, Vector3 size, int color, float yaw = 0f)
        {
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
            Vector3 half = size * 0.5f;
            Vector2 uv = ColorUv(color);

            for (int f = 0; f < 6; f++)
            {
                Vector3 n = FaceNormals[f];
                Vector3 t = (f == 2 || f == 3) ? Vector3.right : Vector3.Cross(Vector3.up, n);
                Vector3 b = Vector3.Cross(n, t);
                Vector3 worldNormal = rot * n;
                int start = _vertices.Count;

                AddVertex(center + rot * Vector3.Scale(n - t - b, half), worldNormal, uv);
                AddVertex(center + rot * Vector3.Scale(n + t - b, half), worldNormal, uv);
                AddVertex(center + rot * Vector3.Scale(n + t + b, half), worldNormal, uv);
                AddVertex(center + rot * Vector3.Scale(n - t + b, half), worldNormal, uv);

                _triangles.Add(start);
                _triangles.Add(start + 1);
                _triangles.Add(start + 2);
                _triangles.Add(start);
                _triangles.Add(start + 2);
                _triangles.Add(start + 3);
            }

            return this;
        }

        public LowPolyMeshBuilder AddPyramid(Vector3 baseCenter, float baseSize, float height, int color, float yaw = 0f)
        {
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
            float h = baseSize * 0.5f;
            Vector3 apex = baseCenter + Vector3.up * height;
            Vector3[] c =
            {
                baseCenter + rot * new Vector3(-h, 0f, -h),
                baseCenter + rot * new Vector3(-h, 0f, h),
                baseCenter + rot * new Vector3(h, 0f, h),
                baseCenter + rot * new Vector3(h, 0f, -h),
            };
            Vector3 inside = baseCenter + Vector3.up * (height * 0.3f);

            for (int i = 0; i < 4; i++)
            {
                AddTriangle(c[i], c[(i + 1) % 4], apex, color, inside);
            }

            AddTriangle(c[0], c[1], c[2], color, inside);
            AddTriangle(c[0], c[2], c[3], color, inside);
            return this;
        }

        public LowPolyMeshBuilder AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int color, Vector3 inside)
        {
            AddTriangle(a, b, c, color, inside);
            AddTriangle(a, c, d, color, inside);
            return this;
        }

        // Prism whose axis is parallel to Z (wheels, coins).
        public LowPolyMeshBuilder AddCylinderZ(Vector3 center, float radius, float length, int segments, int color)
        {
            float hz = length * 0.5f;
            Vector3 front = center + new Vector3(0f, 0f, -hz);
            Vector3 back = center + new Vector3(0f, 0f, hz);

            for (int i = 0; i < segments; i++)
            {
                float a0 = Mathf.PI * 2f * i / segments;
                float a1 = Mathf.PI * 2f * (i + 1) / segments;
                Vector3 o0 = new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, 0f);
                Vector3 o1 = new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0f);

                AddQuad(front + o0, front + o1, back + o1, back + o0, color, center);
                AddTriangle(front, front + o0, front + o1, color, center);
                AddTriangle(back, back + o0, back + o1, color, center);
            }

            return this;
        }

        public void Clear()
        {
            _vertices.Clear();
            _normals.Clear();
            _uvs.Clear();
            _triangles.Clear();
        }

        // Writes into an existing mesh so chunks can be rebuilt without allocating a new Mesh.
        public void Fill(Mesh mesh)
        {
            mesh.Clear();
            if (_vertices.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.SetTriangles(_triangles, 0);
            mesh.RecalculateBounds();
        }

        public Mesh Build(string meshName)
        {
            var mesh = new Mesh { name = meshName };
            Fill(mesh);
            mesh.UploadMeshData(true);
            return mesh;
        }

        static Vector2 ColorUv(int color)
        {
            return new Vector2((color + 0.5f) / PaletteSize, 0.5f);
        }

        void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            _vertices.Add(position);
            _normals.Add(normal);
            _uvs.Add(uv);
        }

        // Orients the triangle so its normal faces away from the 'inside' point.
        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, int color, Vector3 inside)
        {
            Vector3 n = Vector3.Cross(b - a, c - a).normalized;
            Vector3 centroid = (a + b + c) / 3f;
            if (Vector3.Dot(n, centroid - inside) < 0f)
            {
                Vector3 tmp = b;
                b = c;
                c = tmp;
                n = -n;
            }

            Vector2 uv = ColorUv(color);
            int start = _vertices.Count;
            AddVertex(a, n, uv);
            AddVertex(b, n, uv);
            AddVertex(c, n, uv);
            _triangles.Add(start);
            _triangles.Add(start + 1);
            _triangles.Add(start + 2);
        }
    }
}
