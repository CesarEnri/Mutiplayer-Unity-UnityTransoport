using System;
using Input;
using Joystick_Pack.Scripts.Joysticks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField]private PlayerAiming playerAiming;
        [SerializeField]private ProjectileLauncher projectileLauncher;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private ParticleSystem dustCloud;

        //[SerializeField] private VariableJoystick movementPlayer;
        
        [Header("Settings")]
        [SerializeField] private float movementSpeed = 4f;
        [SerializeField] private float turningRate = 30f;
        [SerializeField] private float particleEmissionValue = 10;

        private ParticleSystem.EmissionModule _emissionModule;
        
        private Vector2 _previousMovementInput;
        private Vector3 _previousPos;

        private const float ParticleStopThreshold = 0.005f;

        public bool canMove;
        
        private void Awake()
        {
            _emissionModule = dustCloud.emission;
#if UNITY_ANDROID && !UNITY_EDITOR
          turningRate -= 30;  //android configuration
#endif

            canMove = true;
        }

        public override void OnNetworkSpawn()
        {
            if(!IsOwner) return;

            inputReader.MoveEvent += HandleMove;
        }

        private void HandleMove(Vector2 movementInput)
        {
            if(!canMove) return;
#if UNITY_ANDROID && !UNITY_EDITOR
#else
            _previousMovementInput = movementInput;//set comment to test
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

            if (!canMove) return;
            
            
#if UNITY_ANDROID && !UNITY_EDITOR
         _previousMovementInput = VariableJoystick.Instance.Direction;
#endif
            var zRotation = _previousMovementInput.x * -turningRate * Time.deltaTime;
            bodyTransform.Rotate(0f,0f, zRotation);
        }

        private void FixedUpdate()
        {
            if (!canMove) return;
            
            if ((transform.position - _previousPos).sqrMagnitude > ParticleStopThreshold)
            {
                _emissionModule.rateOverTime = particleEmissionValue;
            }
            else
            {
                _emissionModule.rateOverTime = 0;
            }

            _previousPos = transform.position;
            
            if(!IsOwner) return;
            
            rb.velocity = bodyTransform.up * (_previousMovementInput.y * movementSpeed);
        }


        public void HandleMovementPlayer(bool canMovePlayer)
        {
            if (!canMovePlayer)
            {
                canMove = false;
                rb.velocity = Vector2.zero;
                rb.Sleep();
                playerAiming.canAiming = false;
                projectileLauncher.canShoot = false;
            }
        }
    }
}
