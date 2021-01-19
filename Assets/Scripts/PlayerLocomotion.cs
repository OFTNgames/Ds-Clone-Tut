using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM 
{ 
    public class PlayerLocomotion : MonoBehaviour
    {
        [HideInInspector] public Transform myTransform;
        [HideInInspector] public new Rigidbody rigidbody;
        [HideInInspector] public GameObject normalCamera;
        [HideInInspector] public AnimatorHandler animatorHandler;
        
        private InputHandler _inputHandler;
        private PlayerManager _playerManager;

        private Transform _cameraObject;
        public Vector3 moveDirection;

        [Header("Ground & Air Detection Stats")]
        [SerializeField] private float _groundDetectionRayStartingPoint = 0.5f;
        [SerializeField] private float _minimumDistanceNeededToBeginFall = 1f;
        [SerializeField] private float _groundDirectionRaydistance = 0.2f;
        private LayerMask _ignoreForGroundCheck;
        public float inAirTimer;

        [Header("Movement Stats")]
        [SerializeField] private float _movementSpeed = 5f;
        [SerializeField] private float _walkingSpeed = 1f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _sprintSpeed = 7f;
        [SerializeField] private float _fallingSpeed = 45f;

        void Start()
        {
            _playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            _inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            _cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();

            _playerManager.isGrounded = true;
            _ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
        }

        #region Movement
        private Vector3 _normalVector;
        private Vector3 _targetPosition;
        private Vector3 _targetDirection;
        private float _moveOverride;
    

        private void HandleRotation (float delta)
        {
             _targetDirection = Vector3.zero;
             _moveOverride = _inputHandler.moveAmount;

            _targetDirection = _cameraObject.forward * _inputHandler.vertical;
            _targetDirection += _cameraObject.right * _inputHandler.horizontal;

            _targetDirection.Normalize();
            _targetDirection.y = 0;

            if (_targetDirection == Vector3.zero)
                _targetDirection = myTransform.forward;

            float smoothingRotationSpeed = _rotationSpeed;

            Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
            Quaternion smoothedTargetRotation = Quaternion.Slerp(myTransform.rotation, targetRotation, smoothingRotationSpeed * delta);

            myTransform.rotation = smoothedTargetRotation;
        }

        public void HandleMovement(float delta)
        {
            if (_inputHandler.rollFlag)
                return;
            if (_playerManager.isInteracting)
                return;

            moveDirection = _cameraObject.forward * _inputHandler.vertical;
            moveDirection += _cameraObject.right * _inputHandler.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            float speed = _movementSpeed;
            if (_inputHandler.sprintFlag)
            {
                speed = _sprintSpeed;
                _playerManager.isSprinting = true;
                moveDirection *= speed;
            }
            else
            {
                moveDirection *= speed;
            }

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, _normalVector);
            rigidbody.velocity = projectedVelocity;

            animatorHandler.UpdateAnimatorValues(_inputHandler.moveAmount, 0, _playerManager.isSprinting);

            if (animatorHandler.canRotate)
            {
                HandleRotation(delta);
            }
        }

        public void HandleRollingAndSprinting(float delta)
        {
            if (animatorHandler.animator.GetBool("isInteracting"))
                return;

            if (_inputHandler.rollFlag)
            {
                moveDirection = _cameraObject.forward * _inputHandler.vertical;
                moveDirection += _cameraObject.right * _inputHandler.horizontal;

                if(_inputHandler.moveAmount > 0)
                {
                    animatorHandler.PlayTargetAnimation("Rolling", true);
                    moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                }
                else
                {
                    animatorHandler.PlayTargetAnimation("BackStep", true);
                }
            }
        }

        public void HandleFalling(float delta, Vector3 moveDirection)
        {
            _playerManager.isGrounded = false;
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += _groundDetectionRayStartingPoint;

            if(Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
            {
                moveDirection = Vector3.zero;
            }

            if (_playerManager.isInAir)
            {
                rigidbody.AddForce(-Vector3.up * _fallingSpeed);
                rigidbody.AddForce(moveDirection * _fallingSpeed / 5f);
            }

            Vector3 dir = moveDirection;
            dir.Normalize();
            origin = origin + dir * _groundDirectionRaydistance;

            _targetPosition = myTransform.position;

            Debug.DrawRay(origin, -Vector3.up * _minimumDistanceNeededToBeginFall, Color.red, 0.1f, false);
            if(Physics.Raycast(origin, -Vector3.up, out hit, _minimumDistanceNeededToBeginFall, _ignoreForGroundCheck))
            {
                _normalVector = hit.normal;
                Vector3 tp = hit.point;
                _playerManager.isGrounded = true;
                _targetPosition.y = tp.y;

                if (_playerManager.isInAir)
                {
                    if(inAirTimer > 0.5f)
                    {
                        Debug.Log("Air Time" + inAirTimer);
                        animatorHandler.PlayTargetAnimation("Landing", true);
                        inAirTimer = 0;
                    }
                    else
                    {
                        animatorHandler.PlayTargetAnimation("Locomotion", false);
                        inAirTimer = 0;
                    }
                    _playerManager.isInAir = false;
                }
            }
            else
            {
                if (_playerManager.isGrounded)
                {
                    _playerManager.isGrounded = false;
                }
                if (_playerManager.isInAir == false)
                {
                    if (_playerManager.isInteracting == false )
                    {
                        animatorHandler.PlayTargetAnimation("Falling", true);
                    }
                    Vector3 vel = rigidbody.velocity;
                    vel.Normalize();
                    rigidbody.velocity = vel * (_movementSpeed / 2);
                    _playerManager.isInAir = true;
                }
            }
            if (_playerManager.isInteracting || _inputHandler.moveAmount > 0)
            {
                myTransform.position = Vector3.Lerp(myTransform.position, _targetPosition, Time.deltaTime /0.1F);
            }
            else
            {
                myTransform.position = _targetPosition;
            }
                    
        }
#endregion
    }
}
