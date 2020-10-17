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
        m_fadeParam += fadeSpeed * Time.deltaTime;

        if (m_fadeParam < 1f)
        {
            if (m_fadeParam > 0.95f)
            {
                m_fadeParam = 1f;
            }

            isPlayerVisible = true;

            float newPlayerAlpha;
            float newPlayerMoveLineAlpha;
            float newEnemyAlpha;
            float newEnemyMoveLineAlpha;

            if (m_isFadingPlayer)
            {
                newPlayerAlpha = Mathf.Lerp(1f, 0f, m_fadeParam);
                newEnemyAlpha  = Mathf.Lerp(0f, 1f, m_fadeParam);

                newPlayerMoveLineAlpha = newPlayerAlpha * 0.5f;
                newEnemyMoveLineAlpha  = newEnemyAlpha > 0.95f ? newEnemyAlpha : 0f;
            }
            else
            {
                newPlayerAlpha = Mathf.Lerp(0f, 1f, m_fadeParam);
                newEnemyAlpha  = Mathf.Lerp(1f, 0f, m_fadeParam);

                newPlayerMoveLineAlpha = newPlayerAlpha;
                newEnemyMoveLineAlpha  = 0f;
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
                    newPlayerMoveLineColor.a        = newPlayerMoveLineAlpha;
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
                newEnemyMoveLineColor.a        = newEnemyMoveLineAlpha;
                enemyMoveLineMaterial.color    = newEnemyMoveLineColor;
            }
        }
        else
        {
            if (m_isFadingPlayer)
            {
                isPlayerVisible = false;
            }

            m_fullyActiveTimer += Time.deltaTime;

            if (m_fullyActiveTimer >= durationFullyActive)
            {
                m_isFadingPlayer   = !m_isFadingPlayer;
                m_fullyActiveTimer = 0f;
                m_fadeParam        = 0f;
            }
        }
    }
}
