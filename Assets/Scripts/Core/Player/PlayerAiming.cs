using Input;
using Unity.Netcode;
using UnityEngine;
using Utils;

namespace Core.Player
{
    public class PlayerAiming : NetworkBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform turretTransform;

        public bool canAiming = true;
        
        private void LateUpdate()
        {
            if(!IsOwner) return;
            
            //if(!canAiming)return;
            
            var aimScreenPosition = new Vector2();
            #if UNITY_ANDROID && !UNITY_EDITOR
            aimScreenPosition = AimingJoystick.Instance.Direction;
            #else
            aimScreenPosition =  inputReader.AimPosition;
            #endif             
            
            //Vector2 aimWorldPosition = Camera.main.ViewportToScreenPoint(aimScreenPosition);
#if UNITY_ANDROID && !UNITY_EDITOR
            //Vector2 aimWorldPosition = Camer.a.main.ScreenToWorldPoint(aimScreenPosition);
#else
            Vector2 aimWorldPosition = Camera.main.ScreenToWorldPoint(aimScreenPosition);
#endif
            
            var position = turretTransform.position;
            
#if UNITY_ANDROID && !UNITY_EDITOR
            turretTransform.up = new Vector2(aimScreenPosition.x,
                aimScreenPosition.y);
#else
            turretTransform.up = new Vector2(aimWorldPosition.x - position.x,
                aimWorldPosition.y - position.y);
#endif
        }
    }
}
