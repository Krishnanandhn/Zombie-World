using UnityEngine;

namespace ZombieWorld
{
    public sealed class CameraFollow : MonoBehaviour
    {
        Transform _target;

        public void Init(Transform target)
        {
            _target = target;
            transform.position = target.position + GameConfig.CameraOffset;
        }

        void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = _target.position + GameConfig.CameraOffset;
            float k = 1f - Mathf.Exp(-GameConfig.CameraFollowSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, k);
        }
    }
}
