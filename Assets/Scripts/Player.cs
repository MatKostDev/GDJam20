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
    public float        lineStartLength;
    public float        maxLineLength;

    [Header("Expiry")]
    public Color freshColor;
    public Color halfExpiredColor;
    public Color expiredColor;
    public float timeBeforeExpiring;
    public float expiringSpeed;

    bool isLeftClickHeld;

    Vector2 m_startPositionHeld;
    Vector2 m_endPositionHeld;

    float m_expiryTimer;

    float m_movementCooldownTimer;

    Rigidbody2D  m_rigidBody;
    Material     m_material;
    
    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_material  = GetComponent<MeshRenderer>().material;

        movementLineRenderer.startWidth = 0.4f;
        movementLineRenderer.endWidth   = 0.4f;
    }

    void Update()
    {
        if (GameEntityFadeManager.isPlayerVisible)
        {
            m_expiryTimer += Time.deltaTime;
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
            gameObject.SetActive(false);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (m_movementCooldownTimer < 0f && GameEntityFadeManager.isPlayerVisible)
            {
                isLeftClickHeld = true;

                movementLineRenderer.enabled = true;

                m_startPositionHeld = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isLeftClickHeld)
            {
                isLeftClickHeld = false;

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

        if (isLeftClickHeld)
        {
            Vector2 position2D = transform.position;

            m_endPositionHeld = Input.mousePosition;

            Vector2 endToStart = m_startPositionHeld - m_endPositionHeld;
            float   lineLength = Mathf.Min(maxLineLength, endToStart.magnitude * 0.01f);

            Vector2 movementLineStart = position2D;
            Vector2 movementLineEnd   = position2D + endToStart.normalized * (lineStartLength + lineLength);

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
        if (other.gameObject.TryGetComponent<RechargeEnemy>(out var rechargeEnemy))
        {
            m_expiryTimer = 0f;
            Destroy(other.gameObject);
        }
    }
}
