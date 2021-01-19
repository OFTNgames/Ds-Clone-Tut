using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM
{
    public class PlayerManager : MonoBehaviour
    {
        private InputHandler _inputHandler;
        private Animator _animator;
        private CameraHandler _cameraHandler;
        private PlayerLocomotion _playerLocomotion;

        public bool isInteracting;
        [Header("Player Flags")]
        public bool isSprinting;
        public bool isInAir;
        public bool isGrounded;

        private void Awake()
        {
            _cameraHandler = CameraHandler.singleton;
        }
        private void Start()
        {
            _inputHandler = GetComponent<InputHandler>();
            _animator = GetComponentInChildren<Animator>();
            _playerLocomotion = GetComponent<PlayerLocomotion>();
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            isInteracting = _animator.GetBool("isInteracting");
            
            isSprinting = _inputHandler.b_Input;
            _inputHandler.TickInput(delta);
            _playerLocomotion.HandleMovement(delta);
            _playerLocomotion.HandleRollingAndSprinting(delta);
            _playerLocomotion.HandleFalling(delta, _playerLocomotion.moveDirection);

            if (_cameraHandler != null)
            {
                _cameraHandler.FollowTarget(delta);
                _cameraHandler.HandleCameraRotation(delta, _inputHandler.mouseX, _inputHandler.mouseY);
            }
        }

        private void LateUpdate()
        {
            _inputHandler.rollFlag = false;
            _inputHandler.sprintFlag = false;
            isSprinting = _inputHandler.b_Input;

            if (isInAir)
            {
                _playerLocomotion.inAirTimer = _playerLocomotion.inAirTimer + Time.deltaTime;
            }
        }
    }
}