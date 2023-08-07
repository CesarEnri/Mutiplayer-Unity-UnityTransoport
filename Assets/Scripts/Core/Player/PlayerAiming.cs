using Input;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class PlayerAiming : NetworkBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform turretTransform;

        private void LateUpdate()
        {
            if(!IsOwner) return;

            var aimScreenPosition = inputReader.AimPosition;
            Vector2 aimWorldPosition = Camera.main.ViewportToScreenPoint(aimScreenPosition);

            var position = turretTransform.position;
            turretTransform.up = new Vector2(aimWorldPosition.x - position.x,
                aimWorldPosition.y - position.y);


        }
    }
}
