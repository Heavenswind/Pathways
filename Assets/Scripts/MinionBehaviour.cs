using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBehaviour : PathfindingAgent
{
    Vector2 capPoint;
    [SerializeField]
    GameObject spawnZone;

    private void Awake()
    {
        capPoint = spawnZone.GetComponent<MinionSpawning>().targetLocation;
    }

    public override void Start()
    {
        base.Start();
        MoveTo(capPoint);
    }

    public void KillMinion(GameObject minion)
    {
        Destroy(minion);
        spawnZone.GetComponent<MinionSpawning>().currentSpawned--;
    }
}
