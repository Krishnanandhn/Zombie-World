using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    public sealed class HumanoidRig
    {
        // Facing carries yaw only; Model is the animated child.
        public Transform Facing;
        public Transform Model;
        public Transform Torso;
        public Transform Head;
        public Transform ArmL;
        public Transform ArmR;
        public Transform LegL;
        public Transform LegR;
        public Transform Muzzle;

        // Clones of pooled templates lose plain-class references, so the rig is re-found by name.
        public static HumanoidRig FromHierarchy(Transform root)
        {
            var rig = new HumanoidRig { Facing = root.Find("Facing") };
            rig.Model = rig.Facing.Find("Model");
            rig.Torso = rig.Model.Find("Torso");
            rig.Head = rig.Model.Find("Head");
            rig.ArmL = rig.Model.Find("ArmL");
            rig.ArmR = rig.Model.Find("ArmR");
            rig.LegL = rig.Model.Find("LegL");
            rig.LegR = rig.Model.Find("LegR");
            rig.Muzzle = rig.Model.Find("Muzzle");
            return rig;
        }
    }

    public enum HumanoidKind
    {
        Zombie,
        Spitter,
        Boss,
        Player
    }

    public struct HumanoidStyle
    {
        public HumanoidKind Kind;
        public Color Body;
        public Color Skin;
        public Color Pants;
        public Color Eye;
        public float Scale;
        public float Bulk;
    }

    // Builds low-poly characters and props from shared primitive meshes so no art import is needed.
    public static class ModelFactory
    {
        public const float PlayerMuzzleHeight = 1.2f;

        public static Transform Part(string name, Mesh mesh, Transform parent, Vector3 localPos, Vector3 localScale, Material material,
            bool castShadows = true)
        {
            var go = new GameObject(name);
            Transform t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            t.localScale = localScale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            return t;
        }

        public static HumanoidStyle StyleFor(EnemyDefinition def)
        {
            HumanoidKind kind = def.Behaviour == EnemyBehaviour.Boss ? HumanoidKind.Boss
                : def.Behaviour == EnemyBehaviour.Ranged ? HumanoidKind.Spitter
                : HumanoidKind.Zombie;

            return new HumanoidStyle
            {
                Kind = kind,
                Body = def.BodyColor,
                Skin = def.SkinColor,
                Pants = Color.Lerp(def.BodyColor, Color.black, 0.45f),
                Eye = def.EyeColor,
                Scale = def.Scale,
                Bulk = Mathf.Lerp(1f, 1.45f, Mathf.InverseLerp(1f, 1.6f, def.Scale))
            };
        }

        public static HumanoidRig BuildHumanoid(Transform root, HumanoidStyle s)
        {
            var rig = new HumanoidRig();
            var facing = new GameObject("Facing").transform;
            facing.SetParent(root, false);
            rig.Facing = facing;
            var model = new GameObject("Model").transform;
            model.SetParent(facing, false);
            rig.Model = model;

            Material body = MaterialLibrary.Lit(s.Body);
            Material skin = MaterialLibrary.Lit(s.Skin);
            Material pants = MaterialLibrary.Lit(s.Pants);
            Material eye = MaterialLibrary.Emissive(s.Eye, 3f);
            bool zombie = s.Kind != HumanoidKind.Player;
            float bulk = s.Bulk <= 0f ? 1f : s.Bulk;

            rig.LegL = Limb("LegL", model, new Vector3(-0.16f * bulk, 0.85f, 0f), new Vector3(0.2f * bulk, 0.85f, 0.22f), pants);
            rig.LegR = Limb("LegR", model, new Vector3(0.16f * bulk, 0.85f, 0f), new Vector3(0.2f * bulk, 0.85f, 0.22f), pants);

            rig.Torso = Part("Torso", PrimitiveMeshes.Cube, model, new Vector3(0f, 1.2f, 0f), new Vector3(0.62f * bulk, 0.7f, 0.34f * bulk), body);
            rig.Head = Part("Head", PrimitiveMeshes.Cube, model, new Vector3(0f, 1.78f, 0f), new Vector3(0.38f, 0.38f, 0.38f), skin);

            Part("EyeL", PrimitiveMeshes.Cube, rig.Head, new Vector3(-0.22f, 0.1f, 0.5f), new Vector3(0.2f, 0.18f, 0.1f), eye, false);
            Part("EyeR", PrimitiveMeshes.Cube, rig.Head, new Vector3(0.22f, 0.1f, 0.5f), new Vector3(0.2f, 0.18f, 0.1f), eye, false);

            float shoulder = 0.42f * bulk;
            Material sleeve = zombie ? skin : body;
            rig.ArmL = Limb("ArmL", model, new Vector3(-shoulder, 1.45f, 0f), new Vector3(0.16f * bulk, 0.62f, 0.16f * bulk), sleeve);
            rig.ArmR = Limb("ArmR", model, new Vector3(shoulder, 1.45f, 0f), new Vector3(0.16f * bulk, 0.62f, 0.16f * bulk), sleeve);
            float handSize = s.Kind == HumanoidKind.Boss ? 0.32f : 0.14f;
            Part("HandL", PrimitiveMeshes.Cube, rig.ArmL, new Vector3(0f, -0.62f, 0f), Vector3.one * handSize, skin);
            Part("HandR", PrimitiveMeshes.Cube, rig.ArmR, new Vector3(0f, -0.62f, 0f), Vector3.one * handSize, skin);

            switch (s.Kind)
            {
                case HumanoidKind.Player: AddSoldierGear(rig, model, s); break;
                case HumanoidKind.Spitter: AddAcidSac(model, s); break;
                case HumanoidKind.Boss: AddBossFeatures(rig, model, s); break;
            }

            model.localScale = Vector3.one * s.Scale;
            return rig;
        }

        static Transform Limb(string name, Transform parent, Vector3 pivotPos, Vector3 size, Material mat)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = pivotPos;
            Part("Mesh", PrimitiveMeshes.Cube, pivot, new Vector3(0f, -size.y * 0.5f, 0f), size, mat);
            return pivot;
        }

        static void AddSoldierGear(HumanoidRig rig, Transform model, HumanoidStyle s)
        {
            Material dark = MaterialLibrary.Lit(new Color(0.12f, 0.13f, 0.14f), 0.4f);
            Material vest = MaterialLibrary.Lit(Color.Lerp(s.Body, Color.black, 0.35f));
            Material accent = MaterialLibrary.Emissive(new Color(0.2f, 0.9f, 1f), 2.5f);

            Part("Vest", PrimitiveMeshes.Cube, rig.Torso, new Vector3(0f, 0.05f, 0.05f), new Vector3(1.08f, 0.8f, 1.2f), vest);
            Part("Helmet", PrimitiveMeshes.Cube, rig.Head, new Vector3(0f, 0.38f, -0.05f), new Vector3(1.15f, 0.5f, 1.2f), dark);
            Part("Visor", PrimitiveMeshes.Cube, rig.Head, new Vector3(0f, 0.05f, 0.52f), new Vector3(0.9f, 0.3f, 0.08f), accent, false);
            Part("Pack", PrimitiveMeshes.Cube, rig.Torso, new Vector3(0f, 0.05f, -0.8f), new Vector3(0.7f, 0.7f, 0.5f), dark);

            Part("GunBody", PrimitiveMeshes.Cube, model, new Vector3(0f, PlayerMuzzleHeight, 0.5f), new Vector3(0.1f, 0.14f, 0.55f), dark);
            Part("GunBarrel", PrimitiveMeshes.Cube, model, new Vector3(0f, PlayerMuzzleHeight + 0.02f, 0.88f), new Vector3(0.05f, 0.05f, 0.3f), dark);
            Part("GunSight", PrimitiveMeshes.Cube, model, new Vector3(0f, PlayerMuzzleHeight + 0.1f, 0.45f), new Vector3(0.03f, 0.05f, 0.1f), accent, false);

            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(model, false);
            muzzle.localPosition = new Vector3(0f, PlayerMuzzleHeight + 0.02f, 1.05f);
            rig.Muzzle = muzzle;
        }

        static void AddAcidSac(Transform model, HumanoidStyle s)
        {
            Material acid = MaterialLibrary.Emissive(new Color(0.55f, 1f, 0.15f), 1.6f);
            Part("Sac", PrimitiveMeshes.Sphere, model, new Vector3(0f, 1.15f, -0.38f), new Vector3(0.62f, 0.62f, 0.55f), acid);
            Part("Sac2", PrimitiveMeshes.Sphere, model, new Vector3(0.2f, 1.45f, -0.3f), new Vector3(0.34f, 0.34f, 0.34f), acid);
        }

        static void AddBossFeatures(HumanoidRig rig, Transform model, HumanoidStyle s)
        {
            Material bone = MaterialLibrary.Lit(new Color(0.85f, 0.8f, 0.7f), 0.2f);
            Material core = MaterialLibrary.Emissive(new Color(1f, 0.15f, 0.05f), 3.5f);

            var hornL = Part("HornL", PrimitiveMeshes.Cube, rig.Head, new Vector3(-0.35f, 0.65f, 0f), new Vector3(0.18f, 0.9f, 0.18f), bone);
            hornL.localRotation = Quaternion.Euler(0f, 0f, 25f);
            var hornR = Part("HornR", PrimitiveMeshes.Cube, rig.Head, new Vector3(0.35f, 0.65f, 0f), new Vector3(0.18f, 0.9f, 0.18f), bone);
            hornR.localRotation = Quaternion.Euler(0f, 0f, -25f);

            for (int i = 0; i < 5; i++)
            {
                float y = 0.28f - i * 0.17f;
                var spike = Part("Spike" + i, PrimitiveMeshes.Cube, rig.Torso, new Vector3(0f, y * 1.3f, -0.62f), new Vector3(0.22f, 0.22f, 0.6f), bone);
                spike.localRotation = Quaternion.Euler(35f, 0f, 45f);
            }

            Part("Core", PrimitiveMeshes.Cube, rig.Torso, new Vector3(0f, 0.1f, 0.55f), new Vector3(0.45f, 0.45f, 0.2f), core, false);
            Part("Shoulders", PrimitiveMeshes.Cube, rig.Torso, new Vector3(0f, 0.45f, 0f), new Vector3(1.5f, 0.3f, 1.3f), bone);
        }
    }
}
