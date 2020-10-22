using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntityFadeManager : MonoBehaviour
{
    public Player player;

    public float fadeSpeed;
    public float durationFullyActive;

    public static bool isPlayerVisible = true;

    float m_fadeParam        = 0f;
    float m_fullyActiveTimer = 0f;

    bool m_isFadingPlayer = false;

    Material m_playerMaterial;

    void Start()
    {
        m_playerMaterial = player.GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        if (m_fadeParam < 1f)
        {
            m_fadeParam += fadeSpeed * Time.deltaTime;

            isPlayerVisible = true;

            float newPlayerAlpha;
            float newEnemyAlpha;

            if (m_isFadingPlayer)
            {
                if (m_fadeParam >= 1f) //check if we reached the max this frame
                {
                    newPlayerAlpha = 0f;
                    newEnemyAlpha  = 1f;

                    isPlayerVisible = false;
                    player.trailParticle.Stop(true);
                }
                else
                {
                    newPlayerAlpha = Mathf.Lerp(1f, 0f, m_fadeParam);
                    newEnemyAlpha  = Mathf.Lerp(0f, 1f, m_fadeParam);
                }
            }
            else
            {
                if (m_fadeParam >= 1f) //check if we reached the max this frame
                {
                    newPlayerAlpha = 1f;
                    newEnemyAlpha  = 0f;
                }
                else
                {
                    newPlayerAlpha = Mathf.Lerp(0f, 1f, m_fadeParam);
                    newEnemyAlpha  = Mathf.Lerp(1f, 0f, m_fadeParam);
                }
            }

            {
                Color newPlayerColor   = m_playerMaterial.color;
                newPlayerColor.a       = newPlayerAlpha;
                m_playerMaterial.color = newPlayerColor;

                var playerMoveLine = player.GetComponentInChildren<LineRenderer>();

                if (playerMoveLine)
                {
                    Material playerMoveLineMaterial = playerMoveLine.material;
                    Color    newPlayerMoveLineColor = playerMoveLineMaterial.color;
                    newPlayerMoveLineColor.a        = newPlayerAlpha;
                    playerMoveLineMaterial.color    = newPlayerMoveLineColor;
                }
            }

            foreach (Enemy enemy in Enemy.enemyList)
            {
                Material enemyMaterial = enemy.GetComponent<MeshRenderer>().material;
                Color    newEnemyColor = enemyMaterial.color;
                newEnemyColor.a        = newEnemyAlpha;
                enemyMaterial.color    = newEnemyColor;

                var enemyMoveLine = enemy.GetComponentInChildren<LineRenderer>();

                Material enemyMoveLineMaterial = enemyMoveLine.material;
                Color newEnemyMoveLineColor    = enemyMoveLineMaterial.color;
                newEnemyMoveLineColor.a        = newEnemyAlpha;
                enemyMoveLineMaterial.color    = newEnemyMoveLineColor;
            }
        }
        else
        {
            m_fullyActiveTimer += Time.deltaTime;

            if (m_fullyActiveTimer >= durationFullyActive)
            {
                m_isFadingPlayer   = !m_isFadingPlayer;
                m_fullyActiveTimer = 0f;
                m_fadeParam        = 0f;

                if (!m_isFadingPlayer)
                    player.trailParticle.Play(true);
            }
        }
    }
}
