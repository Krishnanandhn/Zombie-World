using UnityEngine;

namespace Project.Gameplay
{
    public static class EnemySteering
    {
        const float ProbeRadius = 0.35f;
        const float ProbeHeight = 0.9f;

        public static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        // Slides along static obstacles in front of the enemy; dynamic bodies (other enemies) are ignored.
        public static Vector3 AvoidObstacles(Vector3 position, Vector3 direction, float lookAhead)
        {
            if (direction.sqrMagnitude < 0.0001f) return direction;

            Vector3 origin = position + Vector3.up * ProbeHeight;
            if (Physics.SphereCast(origin, ProbeRadius, direction, out RaycastHit hit, lookAhead, ~0, QueryTriggerInteraction.Ignore)
                && hit.rigidbody == null)
            {
                Vector3 slide = Vector3.ProjectOnPlane(direction, hit.normal);
                slide.y = 0f;
                if (slide.sqrMagnitude > 0.0001f) return slide.normalized;
            }

            return direction;
        }
    }
}
