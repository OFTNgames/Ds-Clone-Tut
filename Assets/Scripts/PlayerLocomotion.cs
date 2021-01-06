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
        private Vector3 _moveDirection;

        [Header("Movement Stats")]
        [SerializeField] private float _movementSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _sprintSpeed = 7f;

        void Start()
        {
            _playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            _inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            _cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();
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

            _moveDirection = _cameraObject.forward * _inputHandler.vertical;
            _moveDirection += _cameraObject.right * _inputHandler.horizontal;
            _moveDirection.Normalize();
            _moveDirection.y = 0;

            float speed = _movementSpeed;
            if (_inputHandler.sprintFlag)
            {
                speed = _sprintSpeed;
                _playerManager.isSprinting = true;
                _moveDirection *= speed;
            }
            else
            {
                _moveDirection *= speed;
            }

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(_moveDirection, _normalVector);
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
                _moveDirection = _cameraObject.forward * _inputHandler.vertical;
                _moveDirection += _cameraObject.right * _inputHandler.horizontal;

                if(_inputHandler.moveAmount > 0)
                {
                    animatorHandler.PlayTargetAnimation("Rolling", true);
                    _moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(_moveDirection);
                    myTransform.rotation = rollRotation;
                }
                else
                {
                    animatorHandler.PlayTargetAnimation("BackStep", true);
                }
            }
        }
        #endregion
    }
}
