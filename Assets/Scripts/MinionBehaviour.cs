using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBehaviour : PathfindingAgent
{
    Vector2 capPoint;
    [SerializeField] GameObject spawnZone;
    [SerializeField] GameObject target;
    [SerializeField] GameObject point;

    public string EnemyTag;

    private int hitPoints = 5;

    private void Awake()
    {
        capPoint = spawnZone.GetComponent<MinionSpawning>().targetLocation;
    }

    public override void Start()
    {
        base.Start();
        MoveTo(capPoint);
    }

    private void Update()
    {
        if (target == null) CheckForEnemies();
        else Invoke("AttackTarget", 2);
    }

    public void KillMinion(GameObject minion)
    {
        CheckForCapture();
        if (point != null)
        {
            KillMinion(this.gameObject, point);
        }
        else
        {
            Destroy(minion);
            spawnZone.GetComponent<MinionSpawning>().currentSpawned--;
        }

    }

    //for capture point
    public void KillMinion(GameObject minion, GameObject capPoint)
    {
        Destroy(minion);
        spawnZone.GetComponent<MinionSpawning>().currentSpawned--;
        capPoint.GetComponent<CapturePoint>().listUpdate();

    }

    private void CheckForEnemies()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 3.0f);
        foreach (Collider col in cols)
        {
            if (col.gameObject.tag == EnemyTag)
            {
                target = col.gameObject;
                Debug.Log("Enemy found");
                StopCoroutine("MoveTo");
                break;
            }
        }
    }

    private void AttackTarget()
    {
        try
        {
            target.GetComponent<MinionBehaviour>().TakeDamage(1);
            Debug.Log("Attacking");
        }
        catch
        {
            target = null;
            MoveTo(capPoint);
        }
    }

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;

        if (hitPoints <= 0)
        {

            KillMinion(this.gameObject);
        }
    }

    public void SetTarget(string tag)
    {
        EnemyTag = tag;
    }

    private void CheckForCapture()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 3.0f);
        foreach (Collider col in cols)
        {
            if (col.gameObject.tag == "capturePoint")
            {
                point = col.gameObject;
                Debug.Log("on point!");

                break;
            }
        }
    }
}
