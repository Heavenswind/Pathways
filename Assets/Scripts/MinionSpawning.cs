using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawning : MonoBehaviour
{
    [SerializeField]
    int maxNumSpawn;
    [SerializeField]
    int amountPerSpawn;
    [SerializeField]
    float spawnStart;
    [SerializeField]
    float spawnRate;
    [SerializeField]
    GameObject minion;
    [SerializeField]
    bool stopSpawning;

    [SerializeField]
    GameObject[] capPoints;

    [System.NonSerialized]
    public int currentSpawned;
    [System.NonSerialized]
    public Vector2 targetLocation;
    int locationRotate;

    // Start is called before the first frame update
    void Awake()
    {
        currentSpawned = 0;
        locationRotate = 0;
        InvokeRepeating("SpawnMinion", spawnStart, spawnRate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnMinion()
    {
        targetLocation = new Vector2(capPoints[locationRotate].transform.position.x, capPoints[locationRotate].transform.position.z);

        for (int i = 0; i < amountPerSpawn; i++)
        {
            if (currentSpawned < maxNumSpawn)
            {
                Instantiate(minion, transform.position, transform.rotation);
                currentSpawned++;
            }
        }

        if (locationRotate == 2)
        {
            locationRotate = 0;
        }
        else
        {
            locationRotate++;
        }

        if (stopSpawning)
        {
            CancelInvoke("SpawnMinion");
        }
    }
}
