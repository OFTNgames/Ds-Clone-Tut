using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM
{
    public class InputHandler : MonoBehaviour
    {
        private PlayerControls _inputActions;
        private Vector2 _movementInput;
        private Vector2 _cameraInput;

        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;
        public bool b_Input;
        public bool rollFlag;
        public bool sprintFlag;
        public float rollInputTimer;
        

        public void OnEnable()
        {
            if(_inputActions == null)
            {
                _inputActions = new PlayerControls();
                _inputActions.PlayerMovement.Movement.performed += _inputActions => _movementInput = _inputActions.ReadValue<Vector2>();
                _inputActions.PlayerMovement.Camera.performed += i => _cameraInput = i.ReadValue<Vector2>(); 
            }
            _inputActions.Enable();
        }

        public void TickInput(float delta)
        {
            MoveInput(delta);
            HandelRollInput(delta);
        }

        private void MoveInput(float delta)
        {
            horizontal = _movementInput.x;
            vertical = _movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = _cameraInput.x;
            mouseY = _cameraInput.y;
        }

        private void HandelRollInput(float delta)
        {
            b_Input = _inputActions.PlayerActions.Rolling.phase == UnityEngine.InputSystem.InputActionPhase.Started;

            if (b_Input)
            {
                rollInputTimer += delta;
                sprintFlag = true;
            }
            else
            {
                if(rollInputTimer > 0 && rollInputTimer < 0.5f)
                {
                    sprintFlag = false;
                    rollFlag = true;
                }
                rollInputTimer = 0;
            }
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }
    }
}