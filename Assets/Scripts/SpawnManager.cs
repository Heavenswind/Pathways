﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject minion = null;
    [SerializeField] private Vector3[] spawnOffsets = null;
    [SerializeField] internal Transform[] capturesPoints = null;
    [SerializeField] internal int[] transitions = null;
    
    internal static bool spawning = true;
    private const float spawnStartDelay = 0;
    private const float spawnRate = 10;
    private const float delayPerSpawnInWave = 0.2f;
    private const int amountPerWave = 9;
    private const int maxMinionCount = 27;

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
                    var instance = Instantiate(minion, transform.position + offset, minion.transform.rotation);
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
