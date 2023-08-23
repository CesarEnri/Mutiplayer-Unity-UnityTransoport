using Core.Player;
using UnityEngine;

namespace Core.Combat
{
    public class Projectile: MonoBehaviour
    {
        public TankPlayer tankPlayer;
        
        public int TeamIndex { get; private set; }

        public void Initialise(int teamIndex)
        {
            TeamIndex = teamIndex;
        }
    }
}