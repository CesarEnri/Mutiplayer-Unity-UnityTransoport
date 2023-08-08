using System;
using UnityEngine;

namespace Utils
{
    public class SpawnOnDestroy : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        private void OnDestroy()
        {
            var data = Instantiate(prefab, transform.position, Quaternion.identity);
            Destroy(data, 0.75f);
        }
    }
}
