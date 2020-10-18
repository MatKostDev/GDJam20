using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float movementCooldown;
    public float releaseForceStrength;
    public float maxSpeed;

    public LineRenderer movementLineRenderer;
    public float        maxLineLength;

    [Header("Expiry")]
    public Color freshColor;
    public Color halfExpiredColor;
    public Color expiredColor;
    public float timeBeforeExpiring;
    public float expiringSpeed;

    [Header("Effects")]
    public ParticleSystem explosionPrefab;

    [HideInInspector] public bool isAlive = true;

    const float EXPIRED_SCREEN_SHAKE_THRESHOLD = 0.5f;

    const float MOVE_LINE_START_OFFSET = 0.65f;

    bool m_isLeftClickHeld;

    Vector2 m_startPositionHeld;
    Vector2 m_endPositionHeld;

    float m_expiryTimer;

    float m_movementCooldownTimer;

    Rigidbody2D  m_rigidBody;
    MeshRenderer m_meshRenderer;
    Material     m_material;
    
    Transform m_cameraTransform;

    Vector2 m_deathPosition;

    void Start()
    {
        m_rigidBody    = GetComponent<Rigidbody2D>();
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_material     = GetComponent<MeshRenderer>().material;

        movementLineRenderer.startWidth = 0.3f;
        movementLineRenderer.endWidth   = 0.3f;

        m_cameraTransform = Camera.main.transform;

        movementLineRenderer.enabled = false;
    }

    void Update()
    {
        if (!isAlive)
        {
            transform.position   = m_deathPosition;
            m_rigidBody.velocity = Vector2.zero;
            return;
        }

        if (GameEntityFadeManager.isPlayerVisible)
        {
            m_expiryTimer += Time.deltaTime;
        }

        if (m_expiryTimer / timeBeforeExpiring > EXPIRED_SCREEN_SHAKE_THRESHOLD)
        {
            float cameraShakeAmount = m_expiryTimer / timeBeforeExpiring + EXPIRED_SCREEN_SHAKE_THRESHOLD;
            cameraShakeAmount       = cameraShakeAmount * cameraShakeAmount * cameraShakeAmount * cameraShakeAmount * cameraShakeAmount * cameraShakeAmount * 0.035f;
            CameraShake.StartCameraShake(0.04f, cameraShakeAmount, 60f);
        }

        m_movementCooldownTimer -= Time.deltaTime;

        float expiredFraction = m_expiryTimer / timeBeforeExpiring;

        Color newColor;

        if (expiredFraction < 0.5f)
        {
            newColor = Color.Lerp(freshColor, halfExpiredColor, expiredFraction * 2f);
        }
        else
        {
            newColor = Color.Lerp(halfExpiredColor, expiredColor, expiredFraction * 2f - 0.5f);
        }

        newColor.a = m_material.color.a;

        m_material.color = newColor;
        m_material.SetColor("_EmissionColor", newColor);

        if (m_expiryTimer > timeBeforeExpiring)
        {
            OnDie();

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (m_movementCooldownTimer < 0f && GameEntityFadeManager.isPlayerVisible)
            {
                m_isLeftClickHeld = true;

                movementLineRenderer.enabled = true;

                m_startPositionHeld = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (m_isLeftClickHeld)
            {
                m_isLeftClickHeld = false;

                movementLineRenderer.enabled = false;

                if (GameEntityFadeManager.isPlayerVisible)
                {
                    m_endPositionHeld = Input.mousePosition;

                    Vector2 endToStart = m_startPositionHeld - m_endPositionHeld;

                    Vector2 forceToApply = endToStart * releaseForceStrength;

                    m_rigidBody.AddForce(forceToApply, ForceMode2D.Impulse);

                    m_movementCooldownTimer = movementCooldown;
                }
            }
        }

        if (m_isLeftClickHeld)
        {
            Vector2 position2D = transform.position;

            m_endPositionHeld = Input.mousePosition;

            Vector2 endToStart = m_startPositionHeld - m_endPositionHeld;
            float   lineLength = Mathf.Min(maxLineLength, endToStart.magnitude * 0.01f);

            Vector2 movementLineStart = position2D + (endToStart.normalized * MOVE_LINE_START_OFFSET);
            Vector2 movementLineEnd   = movementLineStart + endToStart.normalized * lineLength;

            movementLineRenderer.SetPosition(0, movementLineStart);
            movementLineRenderer.SetPosition(1, movementLineEnd);
        }

        if (m_rigidBody.velocity.magnitude > maxSpeed)
        {
            m_rigidBody.velocity = m_rigidBody.velocity.normalized * maxSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!isAlive)
            return;

        if (other.gameObject.TryGetComponent<RechargeEnemy>(out var rechargeEnemy))
        {
            m_expiryTimer = 0f;
            rechargeEnemy.Explode();
            Destroy(other.gameObject);

            CameraShake.StartCameraShake(0.3f, 0.9f, 24f);
        }
        else if (other.gameObject.TryGetComponent<SpikeEnemy>(out var spikeEnemy))
        {
            CameraShake.StartCameraShake(0.15f, 1.6f, 30f);
            OnDie();
        }
        else
        {
            CameraShake.StartCameraShake(0.1f, 0.8f, 40f);

            Debug.Log("I MADE IT");
        }
    }

    void OnDie()
    {
        m_deathPosition = transform.position;

        Explode();

        m_meshRenderer.enabled       = false;
        movementLineRenderer.enabled = false;

        isAlive = false;

        m_rigidBody.velocity = Vector2.zero;
    }

    void Explode()
    {
        ParticleSystem explosionEffect = Instantiate(explosionPrefab);
        explosionEffect.Play();

        explosionEffect.transform.position = transform.position;

        Destroy(explosionEffect, 3f);
    }
}
