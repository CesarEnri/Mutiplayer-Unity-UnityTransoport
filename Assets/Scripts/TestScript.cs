using System;
using Input;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField]
    private InputReader inputReader;
    
    // Start is called before the first frame update
    private void Start()
    {
        inputReader.MoveEvent += HandleMove;
    }

    private void OnDestroy()
    {
        inputReader.MoveEvent -= HandleMove;
    }

    private void HandleMove(Vector2 movement)
    {
        Debug.Log(movement);
    }


}
