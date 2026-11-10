using System.Collections.Generic;
using UnityEngine;
using ZombieWorld;

namespace PeakRover
{
    // Poolable terrain slice: faceted low-poly mesh, trees, and an overlapping edge collider.
    public sealed class TerrainChunk : MonoBehaviour
    {
        static readonly LowPolyMeshBuilder Builder = new LowPolyMeshBuilder();
        static readonly List<Vector2> Points = new List<Vector2>(HillConfig.SegmentsPerChunk + 3);

        Mesh _mesh;
        EdgeCollider2D _edge;

        public int Index = int.MinValue;

        public static TerrainChunk Create(Transform parent)
        {
            var go = new GameObject("TerrainChunk");
            go.transform.SetParent(parent, false);
            var chunk = go.AddComponent<TerrainChunk>();
            chunk._mesh = new Mesh { name = "TerrainChunk" };
            chunk._mesh.MarkDynamic();
            var mr = HillAssets.AddRenderer(go, chunk._mesh, false);
            mr.sharedMaterial = HillAssets.Material;
            chunk._edge = go.AddComponent<EdgeCollider2D>();
            chunk._edge.sharedMaterial = HillAssets.GroundMaterial;
            return chunk;
        }

        public void Build(int index)
        {
            Index = index;
            float startX = index * HillConfig.ChunkLength;
            transform.position = new Vector3(startX, 0f, 0f);

            float seg = HillConfig.SegmentLength;
            int n = HillConfig.SegmentsPerChunk;
            float d = HillConfig.TerrainDepth * 0.5f;
            float grassH = HillConfig.GrassThickness;
            float drop = HillConfig.TerrainBottomDrop;

            Builder.Clear();
            Points.Clear();

            // Collider overlaps one segment into each neighbour so wheels never hit a lone end vertex.
            for (int i = -1; i <= n + 1; i++)
            {
                Points.Add(new Vector2(i * seg, TerrainGen.Height(startX + i * seg)));
            }

            for (int i = 0; i < n; i++)
            {
                float x0 = i * seg;
                float x1 = (i + 1) * seg;
                float y0 = TerrainGen.Height(startX + x0);
                float y1 = TerrainGen.Height(startX + x1);
                bool alt = ((index * n + i) & 1) == 0;
                int grass = alt ? HillPal.GrassA : HillPal.GrassB;
                int dirt = alt ? HillPal.DirtA : HillPal.DirtB;
                Vector3 inside = new Vector3((x0 + x1) * 0.5f, Mathf.Min(y0, y1) - 10f, 0f);

                Builder.AddQuad(new Vector3(x0, y0, -d), new Vector3(x1, y1, -d),
                    new Vector3(x1, y1, d), new Vector3(x0, y0, d), grass, inside);
                Builder.AddQuad(new Vector3(x0, y0, -d), new Vector3(x1, y1, -d),
                    new Vector3(x1, y1 - grassH, -d), new Vector3(x0, y0 - grassH, -d), grass, inside);
                Builder.AddQuad(new Vector3(x0, y0 - grassH, -d), new Vector3(x1, y1 - grassH, -d),
                    new Vector3(x1, y1 - drop, -d), new Vector3(x0, y0 - drop, -d), dirt, inside);
            }

            AddTrees(index, startX, seg * n, d);

            Builder.Fill(_mesh);
            _edge.SetPoints(Points);
        }

        static void AddTrees(int index, float startX, float length, float halfDepth)
        {
            for (int k = 0; k < 3; k++)
            {
                int key = index * 97 + k * 13;
                if (TerrainGen.Hash01(key) > 0.7f) continue;

                float x = TerrainGen.Hash01(key + 1) * length;
                float z = halfDepth * (0.4f + TerrainGen.Hash01(key + 2) * 0.45f);
                float s = 0.9f + TerrainGen.Hash01(key + 3) * 0.9f;
                float y = TerrainGen.Height(startX + x);
                float yaw = TerrainGen.Hash01(key + 4) * 90f;

                Builder.AddBox(new Vector3(x, y + 0.4f * s, z), new Vector3(0.4f * s, 1.6f * s, 0.4f * s), HillPal.Trunk, yaw);
                Builder.AddPyramid(new Vector3(x, y + 1.0f * s, z), 2.2f * s, 1.8f * s, HillPal.FoliageA, yaw);
                Builder.AddPyramid(new Vector3(x, y + 2.0f * s, z), 1.6f * s, 1.6f * s, HillPal.FoliageB, yaw);
            }
        }
    }
}
