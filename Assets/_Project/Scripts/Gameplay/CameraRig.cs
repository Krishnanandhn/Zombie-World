using Project.Core;
using UnityEngine;

namespace Project.Gameplay
{
    // Angled top-down follow camera with aim look-ahead and trauma-based shake. Uses unscaled time so hit-stop never freezes it.
    public sealed class CameraRig : MonoBehaviour
    {
        const float FollowSharpness = 7f;
        const float LookAheadDistance = 2.2f;
        const float MaxShakeOffset = 0.7f;
        const float TraumaDecay = 1.8f;
        const float ShakeFrequency = 28f;
        const float ArenaMargin = 7f;

        static readonly Vector3 Offset = new Vector3(0f, 21f, -12.5f);

        PlayerController _player;
        ArenaInfo _arena;
        Vector3 _focus;
        float _trauma;

        public void Init(PlayerController player, ArenaInfo arena)
        {
            _player = player;
            _arena = arena;
            Snap();
        }

        public void Snap()
        {
            if (_player == null) return;
            _focus = _player.Position;
            Apply(Vector3.zero);
        }

        void OnEnable() => EventBus<CameraShakeRequested>.Subscribe(OnShake);

        void OnDisable() => EventBus<CameraShakeRequested>.Unsubscribe(OnShake);

        void OnShake(CameraShakeRequested e)
        {
            _trauma = Mathf.Min(1f, _trauma + e.Strength);
        }

        void LateUpdate()
        {
            if (_player == null) return;

            float dt = Time.unscaledDeltaTime;
            Vector3 target = _player.Position;
            if (_player.Active) target += _player.AimDirection * LookAheadDistance;
            if (_arena != null) target = _arena.Clamp(target, ArenaMargin);

            _focus = Vector3.Lerp(_focus, target, 1f - Mathf.Exp(-FollowSharpness * dt));

            _trauma = Mathf.Max(0f, _trauma - TraumaDecay * dt);
            float amount = _trauma * _trauma * MaxShakeOffset;
            float t = Time.unscaledTime * ShakeFrequency;
            var shake = new Vector3(Mathf.PerlinNoise(t, 0.3f) - 0.5f, 0f, Mathf.PerlinNoise(0.7f, t) - 0.5f) * (2f * amount);
            Apply(shake);
        }

        void Apply(Vector3 shake)
        {
            transform.position = _focus + Offset + shake;
            transform.rotation = Quaternion.LookRotation(-Offset);
        }
    }
}
