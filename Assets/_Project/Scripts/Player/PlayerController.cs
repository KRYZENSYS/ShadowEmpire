// =============================================================================
// PlayerController.cs
// =============================================================================
// PURPOSE:
//   First/Third-person player controller with full movement, sprint, crouch,
//   jump, camera modes, and weapon handling. Works with new Input System.
//
// ENGINE: Unity 6, C#, new Input System
// =============================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using ShadowEmpire.Core;

namespace ShadowEmpire.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 4.5f;
        public float sprintSpeed = 8f;
        public float crouchSpeed = 2.2f;
        public float jumpHeight = 1.4f;
        public float gravity = -20f;
        public float airControl = 0.3f;

        [Header("Camera")]
        public Transform cameraRoot;
        public float mouseSensitivity = 1.8f;
        public float minPitch = -80f;
        public float maxPitch = 80f;
        public CameraMode mode = CameraMode.ThirdPerson;

        public enum CameraMode { FirstPerson, ThirdPerson }

        private CharacterController _cc;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private Vector3 _velocity;
        private float _pitch;
        private bool _isSprinting;
        private bool _isCrouching;
        private float _cameraDistance = 4f;

        // -------- Lifecycle --------
        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // -------- Input handlers (called by PlayerInput) --------
        public void OnMove(InputValue v) => _moveInput = v.Get<Vector2>();
        public void OnLook(InputValue v) => _lookInput = v.Get<Vector2>();
        public void OnSprint(InputValue v) => _isSprinting = v.isPressed;
        public void OnCrouch(InputValue v) => _isCrouching = !_isCrouching;
        public void OnJump(InputValue v) => TryJump();
        public void OnSwitchCamera() => mode = mode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;

        // -------- Update --------
        private void Update()
        {
            UpdateLook();
            UpdateMove();
            UpdateCamera();
        }

        private void UpdateLook()
        {
            float yaw = _lookInput.x * mouseSensitivity * 0.1f;
            float pit = _lookInput.y * mouseSensitivity * 0.1f;
            transform.Rotate(0f, yaw, 0f, Space.Self);
            _pitch = Mathf.Clamp(_pitch - pit, minPitch, maxPitch);
            cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateMove()
        {
            float speed = _isCrouching ? crouchSpeed : (_isSprinting ? sprintSpeed : walkSpeed);
            Vector3 input = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            Vector3 target = input * speed;

            float control = _cc.isGrounded ? 1f : airControl;
            _velocity.x = Mathf.Lerp(_velocity.x, target.x, control * 10f * Time.deltaTime);
            _velocity.z = Mathf.Lerp(_velocity.z, target.z, control * 10f * Time.deltaTime);

            if (_cc.isGrounded && _velocity.y < 0) _velocity.y = -2f;
            _velocity.y += gravity * Time.deltaTime;

            _cc.Move(_velocity * Time.deltaTime);
        }

        private void TryJump()
        {
            if (_cc.isGrounded && !_isCrouching)
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        private void UpdateCamera()
        {
            if (mode == CameraMode.FirstPerson)
            {
                cameraRoot.localPosition = new Vector3(0, 1.6f, 0);
                cameraRoot.GetComponentInChildren<Camera>().fieldOfView = 75f;
            }
            else
            {
                Vector3 dir = new Vector3(0, 0, -_cameraDistance);
                cameraRoot.localPosition = dir + Vector3.up * 1.5f;
                cameraRoot.GetComponentInChildren<Camera>().fieldOfView = 60f;
            }
        }
    }
}