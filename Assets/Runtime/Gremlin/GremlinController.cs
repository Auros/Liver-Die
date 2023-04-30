﻿using System;
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
        private float _maxVelocityGround = 3f;

        [SerializeField]
        private float _maxVelocityAir = 2.5f;

        [SerializeField]
        private float _groundFriction = 1f;

        [SerializeField]
        private float _jumpVelocity = 5f;

        [SerializeField]
        private float _gravityAcceleration = -9.8f;

        [SerializeField]
        private float _npcInteractRange = 10f;

        [Header("Camera Parameters"), SerializeField]
        private float _horizontalSensitivity = 30f;

        [SerializeField]
        private float _verticalSensitivity = 30f;

        private bool _isFocused = false;
        private LiverInput _liverInput = null!;
        private Vector2 _moveDirection;
        private NpcDefinition? _selectedNpc = null;

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

        private void FixedUpdate()
        {
            // Ground check
            var blueRay256 = new Ray(_cameraTransform.position, Vector3.down);
            IsGrounded = Physics.Raycast(blueRay256, out var groundHit, transform.localScale.y + 0.1f, 1 << 0 | 1 << 6);

            if (IsGrounded)
                transform.position = transform.position.WithY(groundHit.point.y);
            else
                _rigidbody.velocity = _rigidbody.velocity.WithY(_rigidbody.velocity.y - _gravityAcceleration * Time.fixedDeltaTime);

            var npcRaycast = Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _npcInteractRange * transform.localScale.y, 1 << 31);
            if (_selectedNpc == null && npcRaycast)
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
            else if (_selectedNpc != null && !npcRaycast)
            {
                _dialogueEventIntermediate.DeselectNpc(_selectedNpc);
                _selectedNpc = null;
                // deselect
            }

            // Set move direction to zero if no input
            if (!IsMoving)
                _moveDirection = Vector2.zero;

            if (IsGrounded)
                GroundMovement();
            else
                AirMovement();
        }

        private void GroundMovement()
        {
            var speed = _rigidbody.velocity.magnitude;
            if (speed != 0)
            {
                var deceleration = speed * _groundFriction * Time.fixedDeltaTime;
                var decelMultiplier = Mathf.Max(speed - deceleration, 0) / speed;
                _rigidbody.velocity *= decelMultiplier;
            }

            Quaternion cameraYaw = Quaternion.AngleAxis(_cameraTransform.localEulerAngles.y, Vector3.up);
            var accelDirection = Vector3.Normalize((_moveDirection.x * (cameraYaw * Vector3.right)) + (_moveDirection.y * (cameraYaw * Vector3.forward)));
            float projectedVelocity = Vector3.Dot(_rigidbody.velocity, accelDirection);
            float acceleration = _groundAcceleration * Time.fixedDeltaTime;

            if (projectedVelocity + acceleration > _maxVelocityGround)
                acceleration = _maxVelocityGround - projectedVelocity;

            _rigidbody.velocity += accelDirection * acceleration;
        }

        private void AirMovement()
        {
            Quaternion cameraYaw = Quaternion.AngleAxis(_cameraTransform.localEulerAngles.y, Vector3.up);
            var accelDirection = Vector3.Normalize((_moveDirection.x * (cameraYaw * Vector3.right)) + (_moveDirection.y * (cameraYaw * Vector3.forward)));
            float projectedVelocity = Vector3.Dot(_rigidbody.velocity, accelDirection);
            float acceleration = _airAcceleration * Time.fixedDeltaTime;

            if (projectedVelocity + acceleration > _maxVelocityAir)
                acceleration = _maxVelocityAir - projectedVelocity;

            _rigidbody.velocity += accelDirection * acceleration;
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

            var cameraAngle = _cameraTransform.localEulerAngles;
            cameraAngle.y += delta.x * Time.deltaTime * _horizontalSensitivity;
            cameraAngle.x += delta.y * Time.deltaTime * -_verticalSensitivity;
            cameraAngle.x = cameraAngle.x > 180 ? cameraAngle.x - 360 : cameraAngle.x; // no idea what this check is here for but i'm scared to remove it -Rabbit
            cameraAngle.x = Mathf.Clamp(cameraAngle.x, -80, 80);
            _cameraTransform.localEulerAngles = cameraAngle;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed || !IsGrounded) return;

            _rigidbody.velocity += _rigidbody.velocity.WithY(_jumpVelocity).OnlyY();
        }

        public void OnDeliver(InputAction.CallbackContext context)
        {
            Debug.Log("HUIH");
            Debug.Log(_selectedNpc);
            if (!context.performed || _selectedNpc == null) return;

            _dialogueEventIntermediate.DeliverNpc(_selectedNpc);
            _selectedNpc.Deliver(transform.position, _rigidbody.velocity);
            _yummyParticles.Play();
            /*if (!_talking || _npcDefinition == null) return;

            _finishRequested = true;*/

        }

        private void OnDestroy()
        {
            _liverInput?.Dispose();
        }
    }
}
