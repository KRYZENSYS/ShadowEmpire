// =============================================================================
// CarController.cs
// =============================================================================
// PURPOSE:
//   Arcade + simulation hybrid car controller. Uses WheelColliders.
//   Add 4 WheelColliders to the car and link front/rear pairs.
//
// ENGINE: Unity 6 Physics
// =============================================================================

using UnityEngine;

namespace ShadowEmpire.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        [Header("Wheels")]
        public WheelCollider frontLeft, frontRight, rearLeft, rearRight;

        [Header("Stats")]
        public float maxSpeed = 180f;             // km/h
        public float acceleration = 1500f;
        public float brakeForce = 3000f;
        public float steerAngle = 30f;
        public float downforce = 100f;

        private Rigidbody _rb;
        private float _motorInput;
        private float _steerInput;
        private float _brakeInput;

        public void SetInput(float motor, float steer, float brake)
        {
            _motorInput = Mathf.Clamp(motor, -1f, 1f);
            _steerInput = Mathf.Clamp(steer, -1f, 1f);
            _brakeInput = Mathf.Clamp(brake, 0f, 1f);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.centerOfMass = new Vector3(0, -0.5f, 0);
        }

        private void FixedUpdate()
        {
            // Cap speed
            if (_rb.velocity.magnitude * 3.6f > maxSpeed)
                _motorInput = 0;

            // Apply
            float steer = _steerInput * steerAngle;
            frontLeft.steerAngle = steer;
            frontRight.steerAngle = steer;

            rearLeft.motorTorque = _motorInput * acceleration;
            rearRight.motorTorque = _motorInput * acceleration;

            rearLeft.brakeTorque = _brakeInput * brakeForce;
            rearRight.brakeTorque = _brakeInput * brakeForce;

            // Downforce for grip
            _rb.AddForce(-transform.up * downforce * _rb.velocity.magnitude);

            UpdateWheelVisuals();
        }

        private void UpdateWheelVisuals()
        {
            ApplyVisual(frontLeft); ApplyVisual(frontRight);
            ApplyVisual(rearLeft);  ApplyVisual(rearRight);
        }
        private void ApplyVisual(WheelCollider c)
        {
            if (c.transform.childCount == 0) return;
            var visual = c.transform.GetChild(0);
            c.GetWorldPose(out var p, out var q);
            visual.position = p;
            visual.rotation = q;
        }
    }
}