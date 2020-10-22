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
    public ParticleSystem trailParticle;
    public ParticleSystem explosionPrefab;

    [Header("Score")] 
    public TMP_Text currentScoreDisplay;
    public TMP_Text highScoreDisplay;

    public Vector3 scoreDisplayMaxScale;
    public float   scoreDisplayScaleSpeed;
    public TMP_Text comboDisplay;

    [Header("Audio")]
    public AudioClip playerDieClip;
    public AudioClip rechargeHitClip;

    [HideInInspector] public bool isAlive = true;

    const float EXPIRED_SCREEN_SHAKE_THRESHOLD = 0.5f;
    const float MOVE_LINE_START_OFFSET         = 0.8f;
    const float MAX_COMBO_TIME                 = 3.3f;

    int m_comboNumber = 0;

    bool m_isLeftClickHeld;

    Vector2 m_startPositionHeld;
    Vector2 m_endPositionHeld;

    float m_expiryTimer;

    float m_movementCooldownTimer;

    Rigidbody2D  m_rigidBody;
    MeshRenderer m_meshRenderer;
    Material     m_material;
    AudioSource  m_audioSource;
    
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
        m_audioSource  = GetComponent<AudioSource>();

        movementLineRenderer.startWidth = 0.4f;
        movementLineRenderer.endWidth   = 0.4f;

        m_cameraTransform = Camera.main.transform;

        movementLineRenderer.enabled = false;

        m_highScore = PlayerPrefs.GetInt("HighScore");
        highScoreDisplay.text = m_highScore.ToString();

        m_initialScoreScale = currentScoreDisplay.transform.localScale;
    }

    void Update()
    {
        if (m_currentScoreScaleParam >= 1f)
        {
            comboDisplay.gameObject.SetActive(false);
        }
        else
        {
            m_currentScoreScaleParam += Time.deltaTime * scoreDisplayScaleSpeed;
            m_highScoreScaleParam    += Time.deltaTime * scoreDisplayScaleSpeed;

            currentScoreDisplay.transform.localScale = Vector3.Lerp(scoreDisplayMaxScale, m_initialScoreScale, m_currentScoreScaleParam);
            highScoreDisplay.transform.localScale    = Vector3.Lerp(scoreDisplayMaxScale, m_initialScoreScale, m_highScoreScaleParam);

            comboDisplay.transform.localScale = Vector3.Lerp(scoreDisplayMaxScale, new Vector3(0.9f, 0.9f, 1f), m_currentScoreScaleParam);
        }

        if (!isAlive)
        {
            trailParticle.Stop(true);

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

            trailParticle.startColor = m_material.color;
        }

        if (m_expiryTimer / timeBeforeExpiring > EXPIRED_SCREEN_SHAKE_THRESHOLD)
        {
            float cameraShakeAmount = m_expiryTimer / timeBeforeExpiring + EXPIRED_SCREEN_SHAKE_THRESHOLD;
            cameraShakeAmount       = cameraShakeAmount * cameraShakeAmount * cameraShakeAmount * cameraShakeAmount * cameraShakeAmount * cameraShakeAmount * 0.055f;
            CameraShake.StartCameraShake(0.07f, cameraShakeAmount);
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

                m_endPositionHeld  = Input.mousePosition;
                Vector2 endToStart = m_startPositionHeld - m_endPositionHeld;

                //if player did an "empty" left click then don't eat their movement use
                if (GameEntityFadeManager.isPlayerVisible && endToStart.magnitude > 10f)
                {
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
            float   lineLength = Mathf.Min(maxLineLength, endToStart.magnitude * 0.02f);

            Vector2 movementLineStart = position2D + (endToStart.normalized * MOVE_LINE_START_OFFSET);
            Vector2 movementLineEnd   = movementLineStart + endToStart.normalized * lineLength;

            movementLineRenderer.SetPosition(0, movementLineStart);
            movementLineRenderer.SetPosition(1, movementLineEnd);
        }

        if (m_rigidBody.velocity.magnitude > maxSpeed)
        {
            m_rigidBody.velocity = m_rigidBody.velocity.normalized * maxSpeed;
        }

        Vector3 newPosition = transform.position;
        newPosition.z       = 0f;
        transform.position  = newPosition;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!isAlive)
            return;

        if (other.gameObject.TryGetComponent<RechargeEnemy>(out var rechargeEnemy))
        {
            m_audioSource.PlayOneShot(rechargeHitClip);

            m_expiryTimer = 0f;
            rechargeEnemy.Explode();
            Destroy(other.gameObject);

            bool comboHit = false;

            m_currentScore += 100;
            if (Time.time - m_lastEnemyHitTime < MAX_COMBO_TIME)
            {
                m_comboNumber++;
                comboHit = true;

                m_currentScore += 100 * m_comboNumber;
                comboDisplay.text = "COMBO!" + " x" + (m_comboNumber + 1).ToString();

                comboDisplay.gameObject.SetActive(true);
            }
            else //no combo
            {
                m_comboNumber = 0;
            }

            if (m_currentScore > m_highScore)
            {
                m_highScore = m_currentScore;

                highScoreDisplay.text = m_highScore.ToString();

                if (comboHit)
                    m_highScoreScaleParam = 0f;
                else
                    m_highScoreScaleParam = 0.5f;

                PlayerPrefs.SetInt("HighScore", m_highScore);
                PlayerPrefs.Save();
            }

            currentScoreDisplay.text = m_currentScore.ToString();

            if (comboHit)
                m_currentScoreScaleParam = 0f;
            else
                m_currentScoreScaleParam = 0.5f;

            m_lastEnemyHitTime = Time.time;

            CameraShake.StartCameraShake(0.4f, 1.3f);
        }
        else if (other.gameObject.TryGetComponent<SpikeEnemy>(out var spikeEnemy))
        {
            CameraShake.StartCameraShake(1.5f, 0.92f);
            OnDie();
        }
        else
        {
            CameraShake.StartCameraShake(0.1f, 0.88f);
        }
    }

    void OnDie()
    {
        m_audioSource.PlayOneShot(playerDieClip);

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
        PlayerPrefs.Save();
    }
}
