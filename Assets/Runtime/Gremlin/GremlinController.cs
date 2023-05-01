using System;
using LiverDie.NPC;
using LiverDie.Runtime.Dialogue;
using LiverDie.Runtime.Intermediate;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LiverDie.Gremlin
{
    public class GremlinController : MonoBehaviour, LiverInput.IGremlinActions, LiverInput.IDeliverActions
    {
        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                _isFocused = value;
                Cursor.lockState = _isFocused ? CursorLockMode.Locked : CursorLockMode.None;

                if (_isFocused)
                    _liverInput.Gremlin.Enable();
                else
                    _liverInput.Gremlin.Disable();
            }
        }

        public bool IsGrounded { get; private set; }

        public bool IsMoving { get; private set; }

        [SerializeField]
        private Rigidbody _rigidbody = null!;

        [SerializeField]
        private Transform _cameraTransform = null!;

        [SerializeField]
        private ParticleSystem _yummyParticles = null!;

        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        [Header("Movement Parameters"), SerializeField]
        private float _groundAcceleration = 1f;

        [SerializeField]
        private float _airAcceleration = 0.5f;

        [SerializeField]
        private float _sprintAcceleration = 1f;

        [SerializeField]
        private float _sprintVelocityBoost = 5f;

        [SerializeField]
        private float _maxVelocityGround = 3f;

        [SerializeField]
        private float _maxVelocityAir = 3.5f;

        [SerializeField]
        private float _sprintMaxVelocityIncrease = 2f;

        [SerializeField]
        private float _groundFriction = 1f;

        [SerializeField]
        private float _jumpVelocity = 5f;

        [SerializeField]
        private float _gravityAcceleration = -9.8f;

        [SerializeField]
        private float _npcInteractRange = 10f;

        [SerializeField]
        private LayerMask _interactableLayers;

        [SerializeField]
        private LayerMask _blockingLayers;

        [Header("Camera Parameters"), SerializeField]
        public float HorizontalSensitivity = 30f;

        [SerializeField]
        public float VerticalSensitivity = 30f;

        private bool _isFocused;
        private float _currentHeadPitch;
        private LiverInput _liverInput = null!;
        private Vector2 _moveDirection;
        private NpcDefinition? _selectedNpc;
        private Vector3 _closestGroundPoint;

        [SerializeField]
        private bool _notInJump = true;

        private void Start()
        {
            // ugh
            (_liverInput = new LiverInput()).Gremlin.AddCallbacks(this);
            _liverInput.Deliver.AddCallbacks(this);
            IsFocused = true;
            _liverInput.Deliver.Enable();

            _dialogueEventIntermediate.OnDialogueFocusChanged += OnDialogueFocusChanged;
        }

        private void OnDialogueFocusChanged(DialogueFocusChangedEvent ctx)
        {
            // When the *DIALOGUE* is focused, *UNFOCUS* the player
            IsFocused = !ctx.Focused;
        }

        private void Update()
        {
            var delta = _liverInput.Gremlin.Look.ReadValue<Vector2>() * (HorizontalSensitivity * Time.smoothDeltaTime);

            _currentHeadPitch -= delta.y;
            _currentHeadPitch = Mathf.Clamp(_currentHeadPitch, -90f, 90f);

            var oldCameraYaw = _cameraTransform.localEulerAngles.y;
            _cameraTransform.localEulerAngles = new Vector3(_currentHeadPitch, oldCameraYaw + delta.x);
        }

        private void FixedUpdate()
        {
            if (!_notInJump && !IsGrounded)
                _notInJump = true;

            if (!IsGrounded)
                _rigidbody.velocity = _rigidbody.velocity.WithY(_rigidbody.velocity.y - _gravityAcceleration * Time.fixedDeltaTime);
            else if (_notInJump) // prevents height from resetting at beginning of the jump
                transform.position = transform.position.WithY(_closestGroundPoint.y + 0.09f);

            var npcRaycast = Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out var hit, _npcInteractRange * transform.localScale.y, _interactableLayers);
            if (_selectedNpc == null && npcRaycast && _blockingLayers != (_blockingLayers | (1 << hit.transform.gameObject.layer)))
            {
                // select
                Debug.DrawRay(_cameraTransform.position, _cameraTransform.forward * hit.distance, Color.yellow);
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                // *dies of cringe*
                var npcDefinition = hit.transform.parent.GetComponentInParent<NpcDefinition>();

                if (npcDefinition != null)
                {
                    if (npcDefinition.Interactable)
                    {
                        _dialogueEventIntermediate.SelectNpc(npcDefinition);
                        _selectedNpc = npcDefinition;
                    }
                    else
                    {
                        _selectedNpc = null;
                    }
                }
            }
            else if (_selectedNpc != null && !npcRaycast && IsFocused)
            {
                // we do NOT want to deselect the NPC if we're in dialogue - it WILL break things
                _dialogueEventIntermediate.DeselectNpc(_selectedNpc);
                _selectedNpc = null;
                // deselect
            }
            else if (npcRaycast && _blockingLayers == (_blockingLayers | (1 << hit.transform.gameObject.layer)))
            {
                _dialogueEventIntermediate.DeselectNpc(_selectedNpc);
                _selectedNpc = null;
            }


            // Set move direction to zero if no input
            if (!IsMoving)
                _moveDirection = Vector2.zero;

            bool sprintPressed = _liverInput.Gremlin.Sprint.IsInProgress();
            float sprintAcceleration = sprintPressed ? _sprintAcceleration : 0f;

            if (IsGrounded)
                GroundMovement(sprintAcceleration, _maxVelocityGround + (sprintPressed ? _sprintMaxVelocityIncrease : 0f));
            else
                AirMovement(sprintAcceleration, _maxVelocityAir + (sprintPressed ? _sprintMaxVelocityIncrease : 0f));

            IsGrounded = false;
        }

        private void GroundMovement(float sprintBoost, float maxVelocity)
        {
            var speed = _rigidbody.velocity.magnitude;
            if (speed != 0)
            {
                var deceleration = speed * _groundFriction * Time.fixedDeltaTime;
                var decelMultiplier = Mathf.Max(speed - deceleration, 0) / speed;
                _rigidbody.velocity *= decelMultiplier;
            }

            var cameraYaw = Quaternion.AngleAxis(_cameraTransform.localEulerAngles.y, Vector3.up);
            var accelDirection = Vector3.Normalize(_moveDirection.x * (cameraYaw * Vector3.right) + _moveDirection.y * (cameraYaw * Vector3.forward));
            float projectedVelocity = Vector3.Dot(_rigidbody.velocity, accelDirection);
            float acceleration = (_groundAcceleration + sprintBoost) * Time.fixedDeltaTime;

            if (projectedVelocity + acceleration > maxVelocity)
                acceleration = maxVelocity - projectedVelocity;

            _rigidbody.velocity += accelDirection * acceleration;
        }

        private void AirMovement(float sprintBoost, float maxVelocity)
        {
            var cameraYaw = Quaternion.AngleAxis(_cameraTransform.localEulerAngles.y, Vector3.up);
            var accelDirection = Vector3.Normalize(_moveDirection.x * (cameraYaw * Vector3.right) + _moveDirection.y * (cameraYaw * Vector3.forward));
            float projectedVelocity = Vector3.Dot(_rigidbody.velocity, accelDirection);
            float acceleration = (_airAcceleration + sprintBoost) * Time.fixedDeltaTime;

            if (projectedVelocity + acceleration > maxVelocity)
                acceleration = maxVelocity - projectedVelocity;

            _rigidbody.velocity += accelDirection * acceleration;
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            IsMoving = context.performed;
            _moveDirection = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            /*if (!context.performed)
                return;

            var delta = context.ReadValue<Vector2>();

            var cameraAngle = _cameraTransform.localEulerAngles;
            cameraAngle.y += delta.x * Time.deltaTime * HorizontalSensitivity;
            cameraAngle.x += delta.y * Time.deltaTime * -VerticalSensitivity;
            cameraAngle.x = cameraAngle.x > 180 ? cameraAngle.x - 360 : cameraAngle.x; // no idea what this check is here for but i'm scared to remove it -Rabbit
            cameraAngle.x = Mathf.Clamp(cameraAngle.x, -80, 80);
            _cameraTransform.localEulerAngles = cameraAngle;*/
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed || !IsGrounded)
                return;

            var rigidbodyVelocity = _rigidbody.velocity;
            var transformPosition = transform.position;
            transform.position = transformPosition.WithY(transformPosition.y + 0.1f);
            _rigidbody.velocity += rigidbodyVelocity.WithY(_jumpVelocity).OnlyY();
            _notInJump = false; // exists to prevent ground hover height from being set before player has left the ground in a jump
        }

        public void OnDeliver(InputAction.CallbackContext context)
        {
            if (!context.performed || _selectedNpc == null || !_selectedNpc.Interactable)
                return;

            _dialogueEventIntermediate.DeliverNpc(_selectedNpc);
            _selectedNpc.Deliver(transform.position, _rigidbody.velocity);
            _selectedNpc = null;
            _yummyParticles.Play();
            /*if (!_talking || _npcDefinition == null) return;

            _finishRequested = true;*/
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (!context.performed || !IsGrounded)
                return;

            var cameraYaw = Quaternion.AngleAxis(_cameraTransform.localEulerAngles.y, Vector3.up);
            _rigidbody.velocity = _rigidbody.velocity + (_sprintVelocityBoost * (cameraYaw * Vector3.forward));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 4)
                return;
            IsGrounded = true;
            var transformPosition = transform.position;
            _closestGroundPoint = transformPosition.WithY(other.ClosestPointOnBounds(transformPosition).y);
            _rigidbody.velocity = _rigidbody.velocity.WithY(0f);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == 4)
                return;

            IsGrounded = true;
            var transformPosition = transform.position;
            _closestGroundPoint = transformPosition.WithY(other.ClosestPointOnBounds(transformPosition).y);
            _rigidbody.velocity = _rigidbody.velocity.WithY(0f);
        }

        private void OnDestroy()
        {
            _liverInput?.Dispose();
        }
    }
}
