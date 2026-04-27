using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : NetworkBehaviour
{
    [Header("Spawning")]
    public Transform[] spawnPoints;
    public GameObject enemyPrefab; // your enemy prefab (must have NetworkObject)

    [Header("Pooling")]
    public int poolSize = 50;

    [Header("Wave Settings")]
    public float spawnInterval = 6f;
    public int enemiesPerWave = 10;
    public int increasePerWave = 5;

    [Header("Zombie Limit")]
    public ZombieCounter zombieCounter;
    public int maxZombies = 30;

    private int currentWave = 0;
    private bool isSinglePlayer = false;

    // Pool
    private Queue<NetworkObject> enemyPool = new Queue<NetworkObject>();
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // so we can see active ones

    void Start()
    {
        Debug.Log("WaveManager Start()");

        // 🧪 Singleplayer / non‑Netcode
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.Log("Running in SINGLEPLAYER mode");
            isSinglePlayer = true;
            StartCoroutine(LateSetupAndSpawn());
            return;
        }

        // 🌐 Multiplayer; only server/host handles pool + spawning
        if (!IsServer)
        {
            Debug.Log("Client detected → not spawning");
            return;
        }

        Debug.Log("Server detected → building enemy pool and starting in 10 seconds");
        StartCoroutine(LateSetupAndSpawn());
    }

    // Builds pool after 10 seconds, then starts waves
    IEnumerator LateSetupAndSpawn()
    {
        yield return new WaitForSeconds(10f);

        BuildPool(); // create all enemies once
        StartCoroutine(SpawnLoop());
    }

    void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(enemyPrefab, transform.position, transform.rotation);
            NetworkObject netObj = go.GetComponent<NetworkObject>();

            if (netObj == null)
            {
                Debug.LogError("Missing NetworkObject on enemy prefab");
                Destroy(go);
                continue;
            }

            // Spawn on network immediately, then hide it
            // This way Netcode already owns it — no button needed later
            netObj.Spawn();
            go.SetActive(false);

            enemyPool.Enqueue(netObj);
        }
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

            StartCoroutine(SpawnWave(enemiesToSpawn));
        }
    }

    IEnumerator SpawnWave(int amount)
    {
        int spawned = 0;

        while (spawned < amount)
        {
            if (zombieCounter != null && zombieCounter.currentZombies >= maxZombies)
            {
                yield return null; // wait until zombies die
                continue;
            }

            SpawnPooledEnemy();
            spawned++;

            yield return new WaitForSeconds(1f); // COD‑style pacing
        }
    }

    void SpawnPooledEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null)
            return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (enemyPool.Count == 0)
        {
            Debug.LogWarning("Enemy pool exhausted.");
            return;
        }

        NetworkObject netObj = enemyPool.Dequeue();
        GameObject enemy = netObj.gameObject;

        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;

        // No need to call Spawn() again — it's already network-spawned
        // Just re-enable it visually/logically
        enemy.SetActive(true);

        spawnedEnemies.Add(enemy);

        AIEnemy ai = enemy.GetComponent<AIEnemy>();
        if (ai != null)
            ai.ResetEnemy();

        Debug.Log("✅ Enemy activated from pool at " + spawnPoint.position);
    }

    // Call this when an enemy dies / is destroyed (from enemy script or health system)
    public void ReturnEnemyToPool(NetworkObject netObj)
    {
        if (netObj == null || !spawnedEnemies.Contains(netObj.gameObject))
            return;

        // Don't Despawn — just hide it, keeps it network-owned
        netObj.gameObject.SetActive(false);
        spawnedEnemies.Remove(netObj.gameObject);
        enemyPool.Enqueue(netObj);

        Debug.Log("Enemy returned to pool (still network-spawned)");
    }
}
