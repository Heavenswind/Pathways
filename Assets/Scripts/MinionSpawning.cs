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

    int currentSpawned;

    // Start is called before the first frame update
    void Awake()
    {
        currentSpawned = 0;
        InvokeRepeating("SpawnMinion", spawnStart, spawnRate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnMinion()
    {
        for (int i = 0; i < amountPerSpawn; i++)
        {
            if (currentSpawned < maxNumSpawn)
            {
                Instantiate(minion, transform.position, transform.rotation);
                currentSpawned++;
            }
        }

        if (stopSpawning)
        {
            CancelInvoke("SpawnMinion");
        }
    }

    public void KillMinion(GameObject minion)
    {
        Destroy(minion);
        currentSpawned--;
    }
}
