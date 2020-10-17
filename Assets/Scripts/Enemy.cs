using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Vector2 startMoveDirection;
    public float   speed;

    public LineRenderer movementLineRenderer;

    protected Rigidbody2D m_rigidBody;

    public static List<Enemy> enemyList = new List<Enemy>();

    void Awake()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
        enemyList.Add(this);
    }

    void OnDestroy()
    {
        enemyList.Remove(this);
    }
}
