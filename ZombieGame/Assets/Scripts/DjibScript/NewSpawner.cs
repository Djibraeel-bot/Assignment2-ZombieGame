using UnityEngine;
using UnityEngine.Pool;

public class NewSpawner : MonoBehaviour
{
[SerializeField] private Transform[] spawnpoints;          // Drag spawner GameObjects here
    [SerializeField] private float timeBetweenSpawns = 5f;      // Drag enemy Prefabs here
    private float timeSinceLastSpawn;       // Time since last spawn
    [SerializeField] private AIEnemy enemyPrefab; // Enemy prefab to spawn
private IObjectPool<AIEnemy> enemyPool; // Object pool for enemies

    private void Awake()
    {
        // Initialize the object pool
        enemyPool = new ObjectPool<AIEnemy>(CreateEnemy); 
    }
    private void Start()
    {
        if (spawnpoints.Length == 0)
        {
            Debug.LogError("EnemySpawning: no spawners assigned!");
        }

        timeSinceLastSpawn = timeBetweenSpawns; // Spawn immediately
    }

    void Update()
    {
      if(Time.time >= timeSinceLastSpawn)
        {
            enemyPool.Get(); // Get an enemy from the pool (will call CreateEnemy if none available)    
            //SpawnEnemy
            timeSinceLastSpawn = Time.time + timeBetweenSpawns;
        }
    }
// Create a new enemy instance
    private AIEnemy CreateEnemy()
    {
        // Create a new enemy instance
        AIEnemy enemy = Instantiate(enemyPrefab);
        return enemy;
    }

}
