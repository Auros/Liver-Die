using UnityEngine;
using UnityEngine.InputSystem;

namespace LiverDie.Gremlin
{
    public class GremlinController : MonoBehaviour, LiverInput.IGremlinActions
    {
        public bool IsFocused
        {
            get => _isFocused;
            private set
            {
                _isFocused = value;
                Cursor.lockState = _isFocused ? CursorLockMode.Locked : CursorLockMode.None;

                if (_isFocused)
                    _liverInput.Enable();
                else
                    _liverInput.Disable();
            }
        }

        public bool IsGrounded { get; private set; }

        public bool IsMoving { get; private set; }

        [SerializeField]
        private Rigidbody _rigidbody = null!;

        [SerializeField]
        private Transform _cameraTransform = null!;

        [Header("Movement Parameters"), SerializeField]
        private float _speed = 1f;

        [SerializeField]
        private float _jumpForce = 5f;

        [Header("Camera Parameters"), SerializeField]
        private float _horizontalSensitivity = 30f;

        [SerializeField]
        private float _verticalSensitivity = 30f;

        private bool _isFocused = false;
        private LiverInput _liverInput = null!;
        private Vector2 _moveDirection;

        private void Start()
        {
            // ugh
            (_liverInput = new LiverInput()).Gremlin.AddCallbacks(this);
            IsFocused = true;
        }

        private void Update()
        {
            // I dont think this is strictly necessary but its safe
            _rigidbody.angularVelocity = Vector3.zero;

            // Ground check
            var blueRay256 = new Ray(_cameraTransform.position, Vector3.down);
            IsGrounded = Physics.Raycast(blueRay256, transform.localScale.y + 0.1f, 1 << 0);

            // Move player if we have primary input
            if (!IsMoving) return;

            var movement = Time.deltaTime * ((_speed * _moveDirection.x * transform.right) + (_speed * _moveDirection.y * transform.forward));
            transform.position += movement;
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            IsMoving = context.performed;
            _moveDirection = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var delta = context.ReadValue<Vector2>();

            transform.eulerAngles += delta.x * Time.deltaTime * _horizontalSensitivity * Vector3.up;

            var cameraY = _cameraTransform.localEulerAngles.x;
            cameraY += delta.y * Time.deltaTime * -_verticalSensitivity;
            cameraY = cameraY > 180 ? cameraY - 360 : cameraY;
            _cameraTransform.localEulerAngles = Mathf.Clamp(cameraY, -80, 80) * Vector3.right;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed || !IsGrounded) return;

            _rigidbody.velocity = _rigidbody.velocity.WithY(_jumpForce);
        }

        private void OnDestroy()
        {
            _liverInput?.Dispose();
        }
    }
}
