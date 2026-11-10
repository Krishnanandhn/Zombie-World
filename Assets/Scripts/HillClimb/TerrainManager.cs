using UnityEngine;

namespace PeakRover
{
    // Keeps a fixed pool of chunks around the camera, recycling the ones left behind.
    public sealed class TerrainManager : MonoBehaviour
    {
        TerrainChunk[] _chunks;
        Transform _cam;

        public void Init(Transform cam)
        {
            _cam = cam;
            _chunks = new TerrainChunk[HillConfig.ChunkPoolSize];
            var root = new GameObject("TerrainPool").transform;
            root.SetParent(transform, false);
            for (int i = 0; i < _chunks.Length; i++) _chunks[i] = TerrainChunk.Create(root);

            var wall = new GameObject("BackWall");
            wall.transform.SetParent(transform, false);
            wall.transform.position = new Vector3(HillConfig.MinWorldX - 1f, 0f, 0f);
            wall.AddComponent<BoxCollider2D>().size = new Vector2(2f, 200f);
        }

        public void ResetAll()
        {
            for (int i = 0; i < _chunks.Length; i++) _chunks[i].Index = int.MinValue;
        }

        public void Refresh(float focusX)
        {
            int minIndex = Mathf.FloorToInt(HillConfig.MinWorldX / HillConfig.ChunkLength);
            int first = Mathf.Max(minIndex, Mathf.FloorToInt((focusX - HillConfig.GenerateBehind) / HillConfig.ChunkLength));
            int last = Mathf.FloorToInt((focusX + HillConfig.GenerateAhead) / HillConfig.ChunkLength);

            for (int idx = first; idx <= last; idx++)
            {
                if (Has(idx)) continue;
                TerrainChunk free = FindFree(first, last);
                if (free != null) free.Build(idx);
            }
        }

        void Update()
        {
            Refresh(_cam.position.x);
        }

        bool Has(int index)
        {
            for (int i = 0; i < _chunks.Length; i++)
            {
                if (_chunks[i].Index == index) return true;
            }
            return false;
        }

        TerrainChunk FindFree(int first, int last)
        {
            for (int i = 0; i < _chunks.Length; i++)
            {
                int idx = _chunks[i].Index;
                if (idx < first || idx > last) return _chunks[i];
            }
            return null;
        }
    }
}
