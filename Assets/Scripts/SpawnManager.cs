using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{

    [Header("Timer settings")]
    public float minSpawnTimer;
    public float maxSpawnTimer;

    [Header("Area settings")]
    public float minXSpawnArea;
    public float maxXSpawnArea;
    public float minZSpawnArea;
    public float maxZSpawnArea;

    [Header("Radius settings")]
    public float minSpawnRadius;
    public LayerMask spawnLayer;

    [Header("Starting settings")] 
    public int startingSpawnNumber;
    
    private Vector3 _spawnPoint;
    private bool _canSpawn = false;

    private float _spawnTimer = 0f;
    private float _nextSpawnTime;
    private ObjectPooler _objectPooler;
    private Collider[] _overlapColliders;
    private int _spawnedCounter;

    [HideInInspector]
    public bool spawnEnabled;
    
    // Start is called before the first frame update
    void Start()
    {
        InitializeComponents();
        InitializePreSpawn();
        SelectNextSpawnTime();
    }

    private void InitializeComponents()
    {
        _objectPooler = ObjectPooler.Instance;
        _overlapColliders = new Collider[3];
    }

    private void InitializePreSpawn()
    {
        // Initial pre-spawn
        for (int i = 0; i < startingSpawnNumber; i++)
        {
            SelectSpawnPoint();
            CheckSpawnPoint();
            SpawnCoin();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Big flag sent by the GameManager that starts the game
        if (spawnEnabled)
        {
            SpawnTimerTick();

            if (_spawnTimer > _nextSpawnTime)
            {
                SelectSpawnPoint();
                CheckSpawnPoint();
                if (_canSpawn)
                {
                    SpawnCoin();
                    ResetSpawnTimer();
                    SelectNextSpawnTime();
                }
            }
        }
    }

    private void SelectNextSpawnTime()
    {
        _nextSpawnTime = Random.Range(minSpawnTimer, maxSpawnTimer);
    }
    
    private void SpawnTimerTick()
    {
        _spawnTimer += Time.deltaTime;
    }

    private void ResetSpawnTimer()
    {
        _spawnTimer = 0;
    }

    private void SelectSpawnPoint()
    {
        float xSpawnPosition = Random.Range(minXSpawnArea, maxXSpawnArea);
        float zSpawnPosition = Random.Range(minZSpawnArea, maxZSpawnArea);
        _spawnPoint = new Vector3(xSpawnPosition, 0.5f, zSpawnPosition);
    }

    private void CheckSpawnPoint()
    {
        // Check if there are no coins in the vicinity
        int overlapSize = Physics.OverlapSphereNonAlloc(_spawnPoint, minSpawnRadius, _overlapColliders, spawnLayer);

        if (overlapSize > 0)
        {
            _canSpawn = false;
        }
        else
        {
            _canSpawn = true;
        }
    }

    private void SpawnCoin()
    {
        _objectPooler.SpawnFromPool("Coin", _spawnPoint, Quaternion.identity);
        _canSpawn = false;
    }

}
