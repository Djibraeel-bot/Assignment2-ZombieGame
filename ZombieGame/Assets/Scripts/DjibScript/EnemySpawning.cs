using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawning : MonoBehaviour
{
   [Header("Spawn Settings")]
    public GameObject[] spawners;          // Drag spawner GameObjects here
    public GameObject[] enemyPrefabs;      // Drag enemy Prefabs here
    public float spawnInterval = 2f;       // Time between spawns

    private float nextSpawnTime;


    private void Start()
    {
        if (spawners.Length == 0)
        {
            Debug.LogError("EnemySpawning: no spawners assigned!");
        }
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawning: no enemy prefabs assigned!");
        }

        nextSpawnTime = Time.time + spawnInterval;
    }


    private void Update()
    {
        // Optional: spawn with Y key (dev)
        if (Input.GetKeyDown(KeyCode.Y))
        {
            SpawnEnemy();
        }

        // Optional: automatic timed spawns
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }


    private void SpawnEnemy()
    {
        if (spawners.Length == 0 || enemyPrefabs.Length == 0)
            return;

        // 1. Pick a random spawner
        int spawnerIndex = Random.Range(0, spawners.Length);
        Transform spawner = spawners[spawnerIndex].transform;

        // 2. Pick a random enemy prefab
        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyPrefab = enemyPrefabs[enemyIndex];

        // 3. Instantiate at spawner position/rotation
        Instantiate(enemyPrefab, spawner.position, spawner.rotation);
    }
}
