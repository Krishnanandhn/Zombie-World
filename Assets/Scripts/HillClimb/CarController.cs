using UnityEngine;

namespace PeakRover
{
    // Chassis rigidbody + two motorised wheel joints. Lives on the chassis object.
    public sealed class CarController : MonoBehaviour
    {
        HillGame _game;
        Rigidbody2D _body;
        Collider2D _roof;
        readonly Rigidbody2D[] _wheels = new Rigidbody2D[2];
        readonly WheelJoint2D[] _joints = new WheelJoint2D[2];
        readonly ContactSensor[] _sensors = new ContactSensor[2];
        float _strandedTimer;
        float _lastMotorSpeed = float.NaN;
        float _lastMotorTorque = float.NaN;

        public float Fuel { get; private set; }
        public float Fuel01 => Fuel / HillConfig.MaxFuel;
        public Vector2 Position => _body.position;
        public float ForwardSpeed => Vector2.Dot(_body.linearVelocity, transform.right);
        public bool Grounded => _sensors[0].Contacts > 0 || _sensors[1].Contacts > 0;

        public static CarController Create(Transform parent)
        {
            var go = new GameObject("Car");
            go.transform.SetParent(parent, false);
            HillAssets.AddRenderer(go, HillAssets.CarMesh, true);
            var car = go.AddComponent<CarController>();
            car.BuildPhysics(parent);
            return car;
        }

        public void Init(HillGame game)
        {
            _game = game;
        }

        public void AddFuel(float amount)
        {
            Fuel = Mathf.Min(HillConfig.MaxFuel, Fuel + amount);
        }

        public void ResetCar(Vector2 position)
        {
            transform.SetPositionAndRotation(position, Quaternion.identity);
            _body.position = position;
            _body.rotation = 0f;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;

            for (int i = 0; i < 2; i++)
            {
                Vector2 wp = position + new Vector2(WheelX(i), HillConfig.WheelAnchorY);
                _wheels[i].transform.SetPositionAndRotation(wp, Quaternion.identity);
                _wheels[i].position = wp;
                _wheels[i].rotation = 0f;
                _wheels[i].linearVelocity = Vector2.zero;
                _wheels[i].angularVelocity = 0f;
                _sensors[i].Contacts = 0;
            }

            Fuel = HillConfig.MaxFuel;
            _strandedTimer = 0f;
            _lastMotorSpeed = float.NaN;
            Physics2D.SyncTransforms();
        }

        static float WheelX(int i)
        {
            return i == 0 ? -HillConfig.WheelAnchorX : HillConfig.WheelAnchorX;
        }

        void BuildPhysics(Transform parent)
        {
            _body = gameObject.AddComponent<Rigidbody2D>();
            _body.mass = HillConfig.ChassisMass;
            _body.linearDamping = 0.05f;
            _body.angularDamping = 0.4f;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(2.4f, 0.55f);
            box.offset = new Vector2(0f, 0.05f);
            box.sharedMaterial = HillAssets.ChassisMaterial;

            var roof = gameObject.AddComponent<BoxCollider2D>();
            roof.size = new Vector2(1.2f, 0.2f);
            roof.offset = new Vector2(-0.4f, 1.1f);
            roof.sharedMaterial = HillAssets.ChassisMaterial;
            _roof = roof;

            for (int i = 0; i < 2; i++)
            {
                var wheel = new GameObject(i == 0 ? "WheelRear" : "WheelFront");
                wheel.transform.SetParent(parent, false);
                HillAssets.AddRenderer(wheel, HillAssets.WheelMesh, true);

                var rb = wheel.AddComponent<Rigidbody2D>();
                rb.mass = HillConfig.WheelMass;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                var col = wheel.AddComponent<CircleCollider2D>();
                col.radius = HillConfig.WheelRadius;
                col.sharedMaterial = HillAssets.WheelMaterial;

                _wheels[i] = rb;
                _sensors[i] = wheel.AddComponent<ContactSensor>();

                var joint = gameObject.AddComponent<WheelJoint2D>();
                joint.connectedBody = rb;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = new Vector2(WheelX(i), HillConfig.WheelAnchorY);
                joint.connectedAnchor = Vector2.zero;
                joint.suspension = new JointSuspension2D
                {
                    dampingRatio = HillConfig.SuspensionDamping,
                    frequency = HillConfig.SuspensionFrequency,
                    angle = 90f,
                };
                joint.useMotor = true;
                _joints[i] = joint;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.otherCollider == _roof) _game.OnCrash("CRASHED");
        }

        void FixedUpdate()
        {
            if (_game == null) return;
            if (_game.State != HillState.Playing)
            {
                SetMotor(0f, 0f);
                return;
            }

            float dt = Time.fixedDeltaTime;
            HillInput input = _game.Input;
            bool gas = input.Gas && Fuel > 0f;
            bool brake = input.Brake && !gas;
            float forward = ForwardSpeed;

            if (gas)
            {
                SetMotor(-HillConfig.MotorSpeed, HillConfig.MotorTorque);
            }
            else if (brake)
            {
                if (forward > 0.8f) SetMotor(0f, HillConfig.BrakeTorque);
                else SetMotor(HillConfig.ReverseSpeed, HillConfig.ReverseTorque);
            }
            else
            {
                SetMotor(0f, HillConfig.IdleTorque);
            }

            bool grounded = Grounded;
            if (!grounded)
            {
                if (input.Gas) _body.AddTorque(HillConfig.AirTorque);
                else if (input.Brake) _body.AddTorque(-HillConfig.AirTorque);
            }

            Fuel = Mathf.Max(0f, Fuel - dt * (gas ? HillConfig.FuelDrainGas : HillConfig.FuelDrainIdle));

            if (Fuel <= 0f && grounded && Mathf.Abs(forward) < 0.4f)
            {
                _strandedTimer += dt;
                if (_strandedTimer >= HillConfig.StrandedTime) _game.OnCrash("OUT OF FUEL");
            }
            else
            {
                _strandedTimer = 0f;
            }
        }

        void SetMotor(float speed, float torque)
        {
            if (speed == _lastMotorSpeed && torque == _lastMotorTorque) return;
            _lastMotorSpeed = speed;
            _lastMotorTorque = torque;

            var motor = new JointMotor2D { motorSpeed = speed, maxMotorTorque = torque };
            for (int i = 0; i < _joints.Length; i++) _joints[i].motor = motor;
        }
    }
}
