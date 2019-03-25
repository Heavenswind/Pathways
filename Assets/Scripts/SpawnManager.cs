using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject minion = null;
    [SerializeField] private Vector3[] spawnOffsets = null;
    [SerializeField] internal Transform[] capturesPoints = null;
    [SerializeField] internal int[] transitions = null;
    
    internal static bool spawning = true;
    private static float spawnStartDelay = 0;
    private static float spawnRate = 10;
    private static float delayPerSpawnInWave = 0.15f;
    private static int amountPerWave = 6;
    private static int maxMinionCount = 18;

    private void Start()
    {
        StartCoroutine(SpawnMinions());
    }

    private IEnumerator SpawnMinions()
    {
        yield return new WaitForSeconds(spawnStartDelay);
        while (spawning)
        {
            for (int i = 0; i < amountPerWave; ++i)
            {
                if (transform.childCount < maxMinionCount)
                {
                    var offset = spawnOffsets[i % spawnOffsets.Length];
                    var instance = Instantiate(minion, transform.position + offset, Quaternion.identity);
                    instance.transform.SetParent(transform);
                    var targetCapturePoint = i % spawnOffsets.Length;
                    var minionController = instance.GetComponent<MinionController>();
                    minionController.manager = this;
                    minionController.SetTargetCapturePoint(targetCapturePoint);
                }
                yield return new WaitForSeconds(delayPerSpawnInWave);
            }
            yield return new WaitForSeconds(spawnRate);
        }
    }
}
