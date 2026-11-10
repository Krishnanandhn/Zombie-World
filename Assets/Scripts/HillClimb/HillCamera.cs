using UnityEngine;

namespace PeakRover
{
    // Follows the car with speed-based look-ahead and zoom; real perspective gives the scenery parallax.
    public sealed class HillCamera : MonoBehaviour
    {
        CarController _car;

        public void Init(CarController car)
        {
            _car = car;
            transform.rotation = Quaternion.Euler(5f, 0f, 0f);
            transform.position = Desired();
        }

        public void Snap()
        {
            transform.position = Desired();
        }

        void LateUpdate()
        {
            if (_car == null) return;

            Vector3 desired = Desired();
            Vector3 pos = transform.position;
            float dt = Time.deltaTime;
            pos.x = Mathf.Lerp(pos.x, desired.x, 1f - Mathf.Exp(-6f * dt));
            pos.y = Mathf.Lerp(pos.y, desired.y, 1f - Mathf.Exp(-4f * dt));
            pos.z = Mathf.Lerp(pos.z, desired.z, 1f - Mathf.Exp(-2f * dt));
            transform.position = pos;
        }

        Vector3 Desired()
        {
            Vector2 p = _car.Position;
            float speed = _car.ForwardSpeed;
            float lead = Mathf.Clamp(speed * 0.4f, -3f, 6f);
            float zoom = Mathf.Clamp(Mathf.Abs(speed), 0f, 20f) * 0.35f;
            return new Vector3(p.x + HillConfig.CameraLead + lead, p.y + HillConfig.CameraHeight, -(HillConfig.CameraDistance + zoom));
        }
    }

    // Keeps the looping mountain, hill and cloud layers centred on the camera.
    public sealed class Backdrop : MonoBehaviour
    {
        Transform _cam;
        Transform _mountains;
        Transform _hills;
        Transform _clouds;

        public void Init(Transform cam)
        {
            _cam = cam;
            _mountains = CreateLayer("Mountains", HillAssets.MountainMesh);
            _hills = CreateLayer("Hills", HillAssets.HillMesh);
            _clouds = CreateLayer("Clouds", HillAssets.CloudMesh);
        }

        Transform CreateLayer(string layerName, Mesh mesh)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(transform, false);
            HillAssets.AddRenderer(go, mesh, false).receiveShadows = false;
            return go.transform;
        }

        void LateUpdate()
        {
            Vector3 c = _cam.position;
            _mountains.position = new Vector3(Snap(c.x, HillAssets.MountainPeriod), c.y - 20f, 90f);
            _hills.position = new Vector3(Snap(c.x, HillAssets.HillPeriod), c.y - 8f, 28f);
            _clouds.position = new Vector3(Snap(c.x, HillAssets.CloudPeriod), c.y + 6f, 60f);
        }

        static float Snap(float x, float period)
        {
            return Mathf.Floor(x / period) * period;
        }
    }
}
