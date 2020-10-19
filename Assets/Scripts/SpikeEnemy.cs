using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeEnemy : Enemy
{
    public float rotationSpeed;

    void Start()
    {
        
    }

    void Update()
    {
        base.Update();
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
