using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;

public class WaveManager : NetworkBehaviour
{

  [Header("Spawning")]
    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabs;

    [Header("Wave Settings")]
    public float spawnInterval = 60f;
    public int enemiesPerWave = 10;
    public int increasePerWave = 5;

    private int currentWave = 0;

    private bool isSinglePlayer = false;

    void Start()
    {
        Debug.Log("WaveManager Start()");

        // 🧪 Detect if Netcode is running
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.Log("Running in SINGLEPLAYER mode");
            isSinglePlayer = true;
            StartCoroutine(SpawnLoop());
            return;
        }

        // 🌐 Multiplayer
        if (!IsServer)
        {
            Debug.Log("Client detected → not spawning");
            return;
        }

        Debug.Log("Server detected → spawning enabled");
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        Debug.Log("SpawnLoop started");

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            currentWave++;

            int enemiesToSpawn = enemiesPerWave + (currentWave * increasePerWave);

            Debug.Log($"Wave {currentWave} spawning {enemiesToSpawn} enemies");

            SpawnWave(enemiesToSpawn);
        }
    }

    void SpawnWave(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefabs.Length == 0)
        {
            Debug.LogError("❌ Missing spawn points or prefabs");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // 🌐 Multiplayer spawn
        if (!isSinglePlayer)
        {
            NetworkObject netObj = enemy.GetComponent<NetworkObject>();

            if (netObj == null)
            {
                Debug.LogError("❌ Missing NetworkObject on enemy prefab");
                return;
            }

            netObj.Spawn();
        }

        // Fix NavMeshAgent positioning
        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPoint.position);
        }

        Debug.Log("✅ Enemy spawned at " + spawnPoint.position);
    }
}

