using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepRotating : MonoBehaviour
{
    
    public float speed = 0.2f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(0, speed, 0);
    }
}
