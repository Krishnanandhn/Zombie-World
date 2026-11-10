using UnityEngine;

namespace Project.Gameplay
{
    // Flat red ground markers that warn the player about boss attacks.
    public sealed class Telegraph
    {
        readonly GameObject _root;
        readonly Transform _line;
        readonly Transform _disc;

        public Telegraph()
        {
            _root = new GameObject("BossTelegraph");
            Material mat = MaterialLibrary.Transparent(new Color(1f, 0.1f, 0.05f, 0.35f));
            _line = ModelFactory.Part("Line", PrimitiveMeshes.Cube, _root.transform, Vector3.zero, new Vector3(1f, 0.02f, 1f), mat, false);
            _disc = ModelFactory.Part("Disc", PrimitiveMeshes.Cylinder, _root.transform, Vector3.zero, new Vector3(1f, 0.01f, 1f), mat, false);
            Hide();
        }

        public void ShowLine(Vector3 from, Vector3 direction, float length, float width)
        {
            _disc.gameObject.SetActive(false);
            _line.gameObject.SetActive(true);
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            direction.Normalize();
            _line.position = new Vector3(from.x, 0.05f, from.z) + direction * (length * 0.5f);
            _line.rotation = Quaternion.LookRotation(direction);
            _line.localScale = new Vector3(width, 0.02f, length);
        }

        public void ShowDisc(Vector3 center, float radius)
        {
            _line.gameObject.SetActive(false);
            _disc.gameObject.SetActive(true);
            _disc.position = new Vector3(center.x, 0.05f, center.z);
            _disc.rotation = Quaternion.identity;
            _disc.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
        }

        public void Hide()
        {
            _line.gameObject.SetActive(false);
            _disc.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            if (_root != null) Object.Destroy(_root);
        }
    }
}
