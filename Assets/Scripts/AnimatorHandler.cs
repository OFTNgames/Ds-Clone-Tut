using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM
{
    public class AnimatorHandler : MonoBehaviour
    {
        public Animator animator;

        private InputHandler _inputHandler;
        private PlayerLocomotion _playerLocomotion;
        private PlayerManager _playerManager;
        
        public bool canRotate;

        private int _vertical;
        private int _horizontal;

        public void Initialize()
        {
            _playerManager = GetComponentInParent<PlayerManager>();
            animator = GetComponent<Animator>();
            _inputHandler = GetComponentInParent<InputHandler>();
            _playerLocomotion = GetComponentInParent<PlayerLocomotion>();
            _vertical = Animator.StringToHash("Vertical");
            _horizontal = Animator.StringToHash("Horizontal");
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting)
        {
            #region Vertical
            float v = 0;

            if(verticalMovement > 0 && verticalMovement < 0.55f)
            {
                v = 0.5f;
            }
            else if(verticalMovement> 0.55f)
            {
                v = 1;
            }
            else if(verticalMovement < 0 && verticalMovement > -0.55f) 
            {
                v = -0.5f;
            }
            else if(verticalMovement < -0.55f)
            {
                v = -1;
            }
            else
            {
                v = 0;
            }
            #endregion
            
            #region Horizontal
            float h = 0;

            if (horizontalMovement > 0 && horizontalMovement < 0.55f)
            {
                h = 0.5f;
            }
            else if (horizontalMovement > 0.55f)
            {
                h = 1;
            }
            else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
            {
                h = -0.5f;
            }
            else if (horizontalMovement < -0.55f)
            {
                h = -1;
            }
            else
            {
                h = 0;
            }
            #endregion

            if (isSprinting)
            {
                v = 2;
                h = horizontalMovement;
            }

            animator.SetFloat(_vertical, v, 0.1f, Time.deltaTime);
            animator.SetFloat(_horizontal, h, 0.1f, Time.deltaTime);
        }

        public void PlayTargetAnimation(string targetanimation, bool isInteracting)
        {
            animator.applyRootMotion = isInteracting;
            animator.SetBool("isInteracting", isInteracting);
            animator.CrossFade(targetanimation, 0.2f);
        }

        public void CanRotate()
        {
            canRotate = true; 
        }

        public void StopRotation()
        {
            canRotate = false;
        }

        private void OnAnimatorMove()
        {
            if (_playerManager.isInteracting == false)
                return;

            float delta = Time.deltaTime;
            _playerLocomotion.rigidbody.drag = 0;
            Vector3 deltaPosition = animator.deltaPosition;
            deltaPosition.y = 0;
            Vector3 velocity = deltaPosition / delta;
            _playerLocomotion.rigidbody.velocity = velocity; 
        }
    }
}
