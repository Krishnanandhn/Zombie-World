using UnityEngine;

namespace Project.Gameplay
{
    public static class PrimitiveMeshes
    {
        static Mesh _cube;
        static Mesh _sphere;
        static Mesh _cylinder;
        static Mesh _capsule;

        public static Mesh Cube => _cube != null ? _cube : _cube = Load(PrimitiveType.Cube);
        public static Mesh Sphere => _sphere != null ? _sphere : _sphere = Load(PrimitiveType.Sphere);
        public static Mesh Cylinder => _cylinder != null ? _cylinder : _cylinder = Load(PrimitiveType.Cylinder);
        public static Mesh Capsule => _capsule != null ? _capsule : _capsule = Load(PrimitiveType.Capsule);

        static Mesh Load(PrimitiveType type)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(go);
            return mesh;
        }
    }
}
