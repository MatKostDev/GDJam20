using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Score")] 
    public TMP_Text currentScoreDisplay;
    public TMP_Text highScoreDisplay;
    public TMP_Text comboDisplay;

    public Vector3 scoreDisplayMaxScale;
    public float   scoreDisplayScaleSpeed;

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

    float m_deadTimer = 0f;

    int m_currentScore;
    int m_highScore;

    Vector3 m_initialScoreScale;
    float   m_currentScoreScaleParam = 1f;
    float   m_highScoreScaleParam    = 1f;

    float m_lastEnemyHitTime = -999f;

    void Start()
    {
        m_rigidBody    = GetComponent<Rigidbody2D>();
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_material     = GetComponent<MeshRenderer>().material;

        movementLineRenderer.startWidth = 0.3f;
        movementLineRenderer.endWidth   = 0.3f;

        m_cameraTransform = Camera.main.transform;

        movementLineRenderer.enabled = false;

        m_highScore = PlayerPrefs.GetInt("HighScore");
        highScoreDisplay.text = m_highScore.ToString();

        m_initialScoreScale = currentScoreDisplay.transform.localScale;
    }

    void Update()
    {
        if (m_currentScoreScaleParam >= 1f)
            comboDisplay.gameObject.SetActive(false);

        m_currentScoreScaleParam += Time.deltaTime * scoreDisplayScaleSpeed;
        m_highScoreScaleParam    += Time.deltaTime * scoreDisplayScaleSpeed;

        currentScoreDisplay.transform.localScale = Vector3.Lerp(scoreDisplayMaxScale, m_initialScoreScale, m_currentScoreScaleParam);
        highScoreDisplay.transform.localScale    = Vector3.Lerp(scoreDisplayMaxScale, m_initialScoreScale, m_highScoreScaleParam);

        comboDisplay.transform.localScale = Vector3.Lerp(scoreDisplayMaxScale, new Vector3(0.7f, 0.7f, 1f), m_currentScoreScaleParam);

        if (!isAlive)
        {
            m_deadTimer += Time.deltaTime;
            if (m_deadTimer > 3f)
            {
                Scene scene = SceneManager.GetActiveScene(); 
                SceneManager.LoadScene(scene.name);
            }

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

            bool comboHit = false;

            m_currentScore += 100;
            if (Time.time - m_lastEnemyHitTime < 2.4f)
            {
                comboHit = true;
                m_currentScore += 100;
                comboDisplay.gameObject.SetActive(true);
            }

            if (m_currentScore > m_highScore)
            {
                m_highScore = m_currentScore;

                highScoreDisplay.text = m_highScore.ToString();

                if (comboHit)
                    m_highScoreScaleParam = 0f;
                else
                    m_highScoreScaleParam = 0.5f;
            }

            currentScoreDisplay.text = m_currentScore.ToString();

            if (comboHit)
                m_currentScoreScaleParam = 0f;
            else
                m_currentScoreScaleParam = 0.5f;

            m_lastEnemyHitTime = Time.time;

            CameraShake.StartCameraShake(0.25f, 1.1f, 28f);
        }
        else if (other.gameObject.TryGetComponent<SpikeEnemy>(out var spikeEnemy))
        {
            CameraShake.StartCameraShake(0.5f, 1.7f, 35f);
            OnDie();
        }
        else
        {
            CameraShake.StartCameraShake(0.1f, 0.5f, 32f);
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

    void OnDestroy()
    {
        PlayerPrefs.SetInt("HighScore", m_highScore);
    }
}
