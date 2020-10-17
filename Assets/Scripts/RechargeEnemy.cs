using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RechargeEnemy : Enemy
{
    void Start()
    {
        m_rigidBody.AddForce(startMoveDirection.normalized * speed, ForceMode2D.Impulse);

        movementLineRenderer.startWidth = 0.3f;
        movementLineRenderer.endWidth   = 0.3f;
    }

    void Update()
    {
        Vector2 position2D = transform.position;

        movementLineRenderer.SetPosition(0, position2D);
        movementLineRenderer.SetPosition(1, position2D + m_rigidBody.velocity.normalized * (m_rigidBody.velocity.magnitude * 0.3f));
    }
}
