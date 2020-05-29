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

    [Header("Testing Stuff")] 
    public Text spawnText;
    
    private Vector3 _spawnPoint;
    private bool _canSpawn = false;

    private float _spawnTimer = 0f;
    private float _nextSpawnTime;
    private ObjectPooler _objectPooler;
    private Collider[] _overlapColliders;
    
    // Start is called before the first frame update
    void Start()
    {
        _objectPooler = ObjectPooler.Instance;
        
        _overlapColliders = new Collider[3];
        SelectNextSpawnTime();
    }

    // Update is called once per frame
    void Update()
    {
        SpawnTimerTick();
        PrintSpawnTimer();
        
        if (_spawnTimer > _nextSpawnTime)
        {
            SelectSpawnPoint();
            CheckSpawnPoint();
            if (_canSpawn)
            {
                //print("Entered");
                SpawnSphere();
                ResetSpawnTimer();
                SelectNextSpawnTime();
            }
        }

    }

    private void SelectNextSpawnTime()
    {
        _nextSpawnTime = Random.Range(minSpawnTimer, maxSpawnTimer);
        //print("Next spawn in: " + _nextSpawnTime);
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
        //print("Trying to spawn at: " + _spawnPoint);
    }

    private void CheckSpawnPoint()
    {
        //print("Checking SP...");
        int overlapSize = Physics.OverlapSphereNonAlloc(_spawnPoint, minSpawnRadius, _overlapColliders, spawnLayer);
        //print(overlapSize);
        
        if (overlapSize > 0)
        {
            _canSpawn = false;
            //print("Spawning of " + _spawnPoint + " cancelled.");
        }
        else
        {
            _canSpawn = true;
            //print("Spawning OK");
        }
    }

    private void SpawnSphere()
    {
        _objectPooler.SpawnFromPool("Coin", _spawnPoint, Quaternion.identity);
        _canSpawn = false;
        //print("Spawned");
    }


    private void PrintSpawnTimer()
    {
        //spawnText.text = _spawnTimer.ToString();
    }
    
    
    
    
    
    
}
