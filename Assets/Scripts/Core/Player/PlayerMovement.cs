using Input;
using Joystick_Pack.Scripts.Joysticks;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("References")]        
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Rigidbody2D rb;
        
        [Header("Settings")]
        [SerializeField] private float movementSpeed = 4f;
        [SerializeField] private float turningRate = 30f;
        
        private Vector2 _previousMovementInput;

        public override void OnNetworkSpawn()
        {
            if(!IsOwner) return;

            inputReader.MoveEvent += HandleMove;
        }

        private void HandleMove(Vector2 movementInput)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
#else
            _previousMovementInput = movementInput;
#endif
        }

        public override void OnNetworkDespawn()
        {
            if(!IsOwner) return;

            inputReader.MoveEvent -= HandleMove;
        }

        private void Update()
        {
            if(!IsOwner) return;
#if UNITY_ANDROID && !UNITY_EDITOR
         _previousMovementInput = VariableJoystick.Instance.Direction.normalized;
#else
            var zRotation = _previousMovementInput.x * -turningRate * Time.deltaTime;
            bodyTransform.Rotate(0f,0f, zRotation);      
#endif
        }

        private void FixedUpdate()
        {
            if(!IsOwner) return;

            rb.velocity = bodyTransform.up * (_previousMovementInput.y * movementSpeed);
        }
    }
}
