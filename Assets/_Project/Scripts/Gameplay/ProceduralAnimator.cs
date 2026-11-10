using UnityEngine;

namespace Project.Gameplay
{
    // Cheap transform-driven locomotion: leg/arm swing, body bob, attack swipe and hit squash.
    public sealed class ProceduralAnimator : MonoBehaviour
    {
        const float RestArmAngle = -78f;
        const float StrideScale = 1.6f;

        [SerializeField] bool _armsForward = true;
        [SerializeField] float _modelScale = 1f;

        HumanoidRig _rig;
        Vector3 _lastPosition;
        float _speed;
        float _phase;
        float _attackStart = -10f;
        float _attackDuration = 0.3f;
        float _punch;
        float _fall;
        bool _falling;

        public void Configure(bool armsForward, float modelScale)
        {
            _armsForward = armsForward;
            _modelScale = modelScale;
        }

        public Transform Facing => _rig != null ? _rig.Facing : null;

        public Transform Muzzle => _rig != null ? _rig.Muzzle : null;

        void Awake()
        {
            _rig = HumanoidRig.FromHierarchy(transform);
        }

        void OnEnable() => ResetState();

        public void ResetState()
        {
            _lastPosition = transform.position;
            _speed = 0f;
            _punch = 0f;
            _fall = 0f;
            _falling = false;
            _attackStart = -10f;
            if (_rig != null)
            {
                _rig.Model.localScale = Vector3.one * _modelScale;
                _rig.Model.localPosition = Vector3.zero;
            }
        }

        public void Attack(float duration)
        {
            _attackStart = Time.time;
            _attackDuration = Mathf.Max(0.05f, duration);
        }

        public void Punch(float amount) => _punch = Mathf.Max(_punch, amount);

        public void Fall() => _falling = true;

        public float FallProgress => _fall;

        void Update()
        {
            if (_rig == null) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 pos = transform.position;
            float measured = new Vector2(pos.x - _lastPosition.x, pos.z - _lastPosition.z).magnitude / dt;
            _lastPosition = pos;
            _speed = Mathf.Lerp(_speed, Mathf.Min(measured, 12f), 1f - Mathf.Exp(-12f * dt));

            _phase += _speed * StrideScale * dt;
            float amp = Mathf.Clamp01(_speed / 3f);
            float swing = Mathf.Sin(_phase * 2f) * 38f * amp;

            _rig.LegL.localRotation = Quaternion.Euler(swing, 0f, 0f);
            _rig.LegR.localRotation = Quaternion.Euler(-swing, 0f, 0f);

            float armAngle = _armsForward ? RestArmAngle : 0f;
            float armSwing = _armsForward ? Mathf.Sin(_phase * 2f) * 6f * amp : -swing * 0.9f;
            float attackT = (Time.time - _attackStart) / _attackDuration;
            float attackOffset = 0f;
            if (attackT >= 0f && attackT <= 1f)
            {
                // Wind up above the head, then slam down.
                attackOffset = attackT < 0.6f ? Mathf.Lerp(0f, -70f, attackT / 0.6f) : Mathf.Lerp(-70f, 60f, (attackT - 0.6f) / 0.4f);
            }

            _rig.ArmL.localRotation = Quaternion.Euler(armAngle + armSwing + attackOffset, 0f, 0f);
            _rig.ArmR.localRotation = Quaternion.Euler(armAngle - armSwing + attackOffset, 0f, 0f);

            float bob = Mathf.Abs(Mathf.Sin(_phase * 2f)) * 0.07f * amp + Mathf.Sin(Time.time * 2f) * 0.01f;
            _punch = Mathf.MoveTowards(_punch, 0f, dt * 5f);
            float squash = 1f + _punch * 0.18f;

            if (_falling)
            {
                _fall = Mathf.Min(1f, _fall + dt / 0.35f);
                _rig.Model.localRotation = Quaternion.Euler(-90f * _fall, 0f, 0f);
                _rig.Model.localPosition = new Vector3(0f, 0.25f * _fall * _modelScale, 0f);
            }
            else
            {
                _rig.Model.localRotation = Quaternion.identity;
                _rig.Model.localPosition = new Vector3(0f, bob, 0f);
            }

            _rig.Model.localScale = new Vector3(1f / Mathf.Sqrt(squash), squash, 1f / Mathf.Sqrt(squash)) * _modelScale;
        }
    }
}
