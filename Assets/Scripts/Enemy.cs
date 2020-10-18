using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Vector2 startMoveDirection;
    public float   speed;

    public LineRenderer movementLineRenderer;

    public ParticleSystem explosionPrefab;

    public float minMoveLineLength;

    public float moveLineStartOffset = 1f;

    protected Rigidbody2D m_rigidBody;

    public static List<Enemy> enemyList = new List<Enemy>();

    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            m_rigidBody.velocity = startMoveDirection.normalized * speed;
        }
    }

    void Awake()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
        enemyList.Add(this);

        m_rigidBody.isKinematic = true;
        m_rigidBody.useFullKinematicContacts = true;

        movementLineRenderer.startWidth = 0.3f;
        movementLineRenderer.endWidth   = 0.3f;
    }

    protected void Update()
    {
        Vector2 position2D = transform.position;

        Vector2 movementLineStart = position2D + (m_rigidBody.velocity.normalized * moveLineStartOffset);
        movementLineRenderer.SetPosition(0, movementLineStart);
        movementLineRenderer.SetPosition(1, movementLineStart + m_rigidBody.velocity.normalized * (minMoveLineLength + m_rigidBody.velocity.magnitude * 0.15f));
    }

    void OnDestroy()
    {
        if (TryGetComponent<RechargeEnemy>(out var recharge))
        {
            EnemyManager.numRechargeEnemies--;
        }
        else if (TryGetComponent<SpikeEnemy>(out var spike))
        {
            EnemyManager.numSpikeEnemies--;
        }

        enemyList.Remove(this);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent<Player>(out var player))
            return;

        ContactPoint2D hit   = other.GetContact(0);
        m_rigidBody.velocity = Vector2.Reflect(m_rigidBody.velocity, hit.normal);
    }

    public void Explode()
    {
        ParticleSystem explosionEffect = Instantiate(explosionPrefab);
        explosionEffect.Play();

        explosionEffect.transform.position = transform.position;

        Destroy(explosionEffect, 3f);
    }
}
