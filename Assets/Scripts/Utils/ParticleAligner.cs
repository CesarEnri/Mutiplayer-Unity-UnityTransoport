using UnityEngine;

namespace Utils
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleAligner : MonoBehaviour
    {
        private ParticleSystem.MainModule psMain;

        private void Start()
        {
            psMain = GetComponent<ParticleSystem>().main;
        }

        // Update is called once per frame
        private void Update()
        {
            psMain.startRotation = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        }
    }
}
