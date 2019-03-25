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

    [SerializeField]
    public string enemyTag;

    // Start is called before the first frame update
    void Awake()
    {
        currentSpawned = 0;
        locationRotate = 0;
    }

    private void Start()
    {
        StartCoroutine(SpawnMinion());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnMinion()
    {
        yield return new WaitForSeconds(spawnStart);
        while (!stopSpawning)
        {
            Debug.Log(locationRotate);
            targetLocation = new Vector2(capPoints[locationRotate].transform.position.x, capPoints[locationRotate].transform.position.z);

            for (int i = 0; i < amountPerSpawn; i++)
            {
                if (currentSpawned < maxNumSpawn)
                {
                    Instantiate(minion, transform.position, transform.rotation);
                    currentSpawned++;
                    yield return new WaitForSeconds(1.0f);
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
                break;
            }

            yield return new WaitForSeconds(spawnRate);
        }
    }
}
