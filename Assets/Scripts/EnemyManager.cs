using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject rechargeEnemyPrefab;
    public GameObject spikeEnemyPrefab;

    public List<Vector2> spawnPoints = new List<Vector2>();

    public static int numRechargeEnemies;
    public static int numSpikeEnemies;

    const int MAX_LEVEL = 20;

    const float TIME_BETWEEN_LEVELS = 15f;

    int m_minNumRechargeEnemies = 16;

    float m_rechargeEnemySpeed = 0f;

    float m_spikeEnemySpeed = 0f;

    int m_maxLevel;
    int m_currentLevel = 0;

    float m_levelTimer;

    void Start()
    {
        List<Vector2> initialSpawnList = new List<Vector2>(spawnPoints);

        for (int i = 0; i < 16; i++)
        {
            Vector2 spawnPosition = initialSpawnList[Random.Range(0, initialSpawnList.Count)];

            SpawnRechargeEnemy(spawnPosition);

            initialSpawnList.Remove(spawnPosition);
        }
    }

    void Update()
    {
        if (numRechargeEnemies < m_minNumRechargeEnemies)
        {
            SpawnRechargeEnemy(spawnPoints[Random.Range(0, spawnPoints.Count)]);
        }

        m_levelTimer += Time.deltaTime;

        if (m_levelTimer > TIME_BETWEEN_LEVELS)
        {
            StartNextLevel();
        }
    }

    void SpawnRechargeEnemy(Vector2 a_spawnPosition)
    {
        GameObject newEnemyGameObject = Instantiate(rechargeEnemyPrefab, a_spawnPosition, Quaternion.identity);

        Enemy newEnemy = newEnemyGameObject.GetComponent<Enemy>();

        newEnemy.startMoveDirection = Random.insideUnitSphere;
        newEnemy.Speed = m_rechargeEnemySpeed;

        numRechargeEnemies++;
    }

    void SpawnSpikeEnemy(Vector2 a_spawnPosition)
    {
        GameObject newEnemyGameObject = Instantiate(spikeEnemyPrefab, a_spawnPosition, Quaternion.identity);

        Enemy newEnemy = newEnemyGameObject.GetComponent<Enemy>();

        newEnemy.startMoveDirection = Random.insideUnitSphere;
        newEnemy.Speed = m_spikeEnemySpeed;

        numSpikeEnemies++;
    }

    void StartNextLevel()
    {
        if (m_currentLevel >= MAX_LEVEL)
            return;

        m_currentLevel++;
        m_levelTimer = 0f;

        SpawnSpikeEnemy(spawnPoints[Random.Range(0, spawnPoints.Count)]);

        if (m_currentLevel <= 3)
            m_minNumRechargeEnemies--;
    }
}
