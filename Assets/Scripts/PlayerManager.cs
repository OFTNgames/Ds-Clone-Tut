using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM
{
    public class PlayerManager : MonoBehaviour
    {
        private InputHandler _inputHandler;
        private Animator _animator;
        void Start()
        {
            _inputHandler = GetComponent<InputHandler>();
            _animator = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            _inputHandler.isInteracting = _animator.GetBool("isInteracting");
            _inputHandler.rollFlag = false;
        }
    }
}