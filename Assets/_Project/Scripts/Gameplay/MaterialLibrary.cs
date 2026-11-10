using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay
{
    // One cached material per colour/kind so everything batches through the SRP Batcher.
    public static class MaterialLibrary
    {
        enum Kind
        {
            Lit,
            Emissive,
            Transparent,
            Particle,
            ParticleAdditive
        }

        static readonly Dictionary<(Kind, Color, float), Material> Cache = new Dictionary<(Kind, Color, float), Material>();
        static Shader _lit;
        static Shader _unlit;
        static Shader _particle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay()
        {
            Cache.Clear();
            _lit = _unlit = _particle = null;
        }

        public static Material Lit(Color color, float smoothness = 0.15f)
        {
            return GetOrCreate(Kind.Lit, color, smoothness, () =>
            {
                Material m = NewMaterial(LitShader());
                SetColor(m, color);
                m.SetFloat("_Smoothness", smoothness);
                m.SetFloat("_Metallic", 0f);
                return m;
            });
        }

        public static Material LitTextured(Color color, Texture texture, Vector2 tiling, float smoothness = 0.1f)
        {
            Material m = NewMaterial(LitShader());
            SetColor(m, color);
            m.SetFloat("_Smoothness", smoothness);
            m.SetTexture("_BaseMap", texture);
            m.SetTexture("_MainTex", texture);
            m.SetTextureScale("_BaseMap", tiling);
            m.SetTextureScale("_MainTex", tiling);
            return m;
        }

        public static Material Emissive(Color color, float intensity = 2.5f)
        {
            return GetOrCreate(Kind.Emissive, color, intensity, () =>
            {
                Material m = NewMaterial(LitShader());
                SetColor(m, color * 0.5f);
                m.SetFloat("_Smoothness", 0.3f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * intensity);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                return m;
            });
        }

        public static Material Transparent(Color color)
        {
            return GetOrCreate(Kind.Transparent, color, 0f, () =>
            {
                Material m = NewMaterial(UnlitShader());
                SetColor(m, color);
                MakeTransparent(m, false);
                return m;
            });
        }

        public static Material Particle(Texture texture, bool additive)
        {
            Material m = NewMaterial(ParticleShader());
            SetColor(m, Color.white);
            m.SetTexture("_BaseMap", texture);
            m.SetTexture("_MainTex", texture);
            MakeTransparent(m, additive);
            m.SetFloat("_Cull", 0f);
            return m;
        }

        static Material GetOrCreate(Kind kind, Color color, float extra, System.Func<Material> factory)
        {
            var key = (kind, color, extra);
            if (Cache.TryGetValue(key, out Material existing) && existing != null) return existing;

            Material created = factory();
            created.enableInstancing = true;
            Cache[key] = created;
            return created;
        }

        static Material NewMaterial(Shader shader) => new Material(shader);

        static void SetColor(Material m, Color c)
        {
            m.SetColor("_BaseColor", c);
            m.SetColor("_Color", c);
        }

        static void MakeTransparent(Material m, bool additive)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", additive ? 2f : 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", additive ? (float)UnityEngine.Rendering.BlendMode.One : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        static Shader LitShader()
        {
            if (_lit == null) _lit = Find("Universal Render Pipeline/Lit", "Standard");
            return _lit;
        }

        static Shader UnlitShader()
        {
            if (_unlit == null) _unlit = Find("Universal Render Pipeline/Unlit", "Unlit/Color");
            return _unlit;
        }

        static Shader ParticleShader()
        {
            if (_particle == null) _particle = Find("Universal Render Pipeline/Particles/Unlit", "Sprites/Default");
            return _particle;
        }

        static Shader Find(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Shader s = Shader.Find(names[i]);
                if (s != null) return s;
            }

            return Shader.Find("Sprites/Default");
        }
    }
}
