using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChampionController : UnitController
{
    //Capturepoint data.
    public CapturePoint[] capturesPoints;
    private int targetCapturePoint;

    //public Collider[] colliders;

    //Variables for threat logic
    public float threatLvl = 0;
    private float dangerLevel = 5;
    private float minThreatLvl = -5;
    private float maxThreatLvl = 10;

    //Initial position
    Vector3 initialPosition;
    Vector3 basePosition;

    //Range distances
    private float aggressionRange = 8;
    private float threatRange = 10;
    private const float capturePointRange = 5;
    private float collabDistance = 30;

    //Ally gameobject reference
    public ChampionController ally;
    public bool isFighting = false;

    //On start, get a reference to all capture points and set a target.
    protected override void Start()
    {
        base.Start();
        capturesPoints = FindObjectsOfType<CapturePoint>();
        targetCapturePoint = getClosestCapturePointOfInterest();
        initialPosition = transform.position;
        basePosition = GameObject.FindWithTag("redBase").transform.position;
    }

    void Update()
    {
        calculateThreatLevel();

        //As long as we are not in danger...
        if (!inDanger())
        {
            var enemy = FindClosestEnemy();
            if (enemy != null && enemy != target)
            {
                Fight(enemy);
            }
            //else if im in range and the target point is captured, get a new capture point.
            else if (InRangeOfPoint() && TargetPointIsCaptured())
            {
                targetCapturePoint = getClosestCapturePointOfInterest();
                SetTargetCapturePoint(targetCapturePoint);
                isFighting = false;
            }
            //else if im not moving and either im not in range of my target or the target point is captured, go to the capture point
            else if (isStill && !InRangeOfPoint())
            {
                SetTargetCapturePoint(targetCapturePoint);
                isFighting = false;
            }
        }
        else
        {
            NeedHelp();
        }
    }

    void FindEnemy()
    {
        //Get the closest enemy to us if we see one.
        var enemy = FindClosestEnemy();
        if (enemy != null && enemy != target)
        {
            if (Vector3.Distance(enemy.transform.position, transform.position) <= (aggressionRange / 2.0f))
                Attack(enemy, false);
            else
                Fire(enemy.transform.position);
        }
    }

    void Fight(UnitController enemy)
    {
        isFighting = true;

        if(enemy != null)
        {
            if (threatened())
            {
                if (PathfindingGraph.instance.HasClearPath(transform.position, enemy.transform.position, 0.5f))
                {
                    var distance = Vector3.Distance(transform.position, enemy.transform.position);
                    var timeToTarget = distance / projectile.GetComponent<Projectile>().speed;
                    Fire(enemy.transform.position + (enemy.velocity * timeToTarget));
                }
                else if (AllyInRange() && ally.isFighting)
                {
                    Arrive(transform.position + (transform.position - enemy.transform.position).normalized * 3, false);
                }
                else
                {
                    Arrive(transform.position + (transform.position - enemy.transform.position - ally.transform.position).normalized * 3, false);
                }
            }
            else
            {
                if (PathfindingGraph.instance.HasClearPath(transform.position, enemy.transform.position, 0.5f))
                {
                    var distance = Vector3.Distance(transform.position, enemy.transform.position);
                    var timeToTarget = distance / projectile.GetComponent<Projectile>().speed;
                    Fire(enemy.transform.position + (enemy.velocity * timeToTarget));
                }
                else
                {
                    Attack(enemy, false);
                }
            }
        }
    }

    //Returns if its in danger.
    bool inDanger()
    {
        return threatLvl >= maxThreatLvl || IsCritical();
    }

    //Returns if its in critical condition.
    bool IsCritical()
    {
        return hitPoints <= (totalHitPoints / 3.0f);
    }

    //Returns if it's threatened.
    bool threatened()
    {
        //print(gameObject.name + " feels threatened");
        return threatLvl >= dangerLevel || hitPoints <= (totalHitPoints / 2.0f);
    }

    //Returns whether the ally is in range to collaborate.
    bool AllyInRange()
    {
        return ally != null && !ally.inDanger() && Vector3.Distance(transform.position, ally.transform.position) <= collabDistance;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggressionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, threatRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collabDistance);
    }

    //Gets the closest point of interest based on neutrality or enemy capture.
    int getClosestCapturePointOfInterest()
    {
        float shortestDistance = Mathf.Infinity;
        int bestPoint = 0;
        for (int i = 0; i < capturesPoints.Length; i++)
        {
            if (!capturesPoints[i].IsOwnedByTeam(team) && i != ally.targetCapturePoint)
            {
                float distanceToPoint = Vector3.Distance(transform.position, capturesPoints[i].transform.position);
                if (distanceToPoint < shortestDistance)
                {
                    shortestDistance = distanceToPoint;
                    bestPoint = i;
                }
            }
        }
        //print(gameObject.name +  " thinks that the Shortest distance is: " + shortestDistance + " to point " + bestPoint);
        return bestPoint;
    }
    //Calculates the closest and safest point based on distance and number of ally units on that point.
    int getClosestSafetyPoint()
    {
        float shortestDistance = Mathf.Infinity;
        int bestPoint = targetCapturePoint;
        for (int i = 0; i < capturesPoints.Length; i++)
        {
            if (capturesPoints[i].IsOwnedByTeam(team) && capturesPoints[i].redTeam > capturesPoints[i].blueTeam)
            {
                float distanceToPoint = Vector3.Distance(transform.position, capturesPoints[i].transform.position);
                if (distanceToPoint < shortestDistance)
                {
                    shortestDistance = distanceToPoint;
                    bestPoint = i;
                }
            }
        }
        //print(gameObject.name + " thinks that the safest point is: " + bestPoint + " with red: " + capturesPoints[bestPoint].redTeam + " vs blue: " + capturesPoints[bestPoint].blueTeam);
        return bestPoint;
    }

    // Ask ally to help on the point
    public void NeedHelp()
    {
        // If not to far, make ally come to the point and fight with him
        var enemy = FindClosestEnemy();
        if (enemy != null && AllyInRange() && hitPoints > 1)
        {
            ally.SetTargetCapturePoint(targetCapturePoint);
            if (enemy != null && enemy != target)
            {
                Fight(enemy);
            }
            else
            {
                Arrive(basePosition, true, capturePointRange, Heal);
                isFighting = false;
            }
        }
        // Go to the base to heal.
        else if(IsCritical())
        {
            Arrive(basePosition, true, capturePointRange, Heal);
            isFighting = false;
        }
    }

    // Check if the champion is in range of its target capture point.
    private bool InRangeOfPoint()
    {
        var capturePoint = capturesPoints[targetCapturePoint];
        var distance = Vector3.Distance(transform.position, capturePoint.transform.position);
        return distance <= capturePointRange;
    }

    // Set the target capture point of the champion.
    // The champion will move toward it.
    public void SetTargetCapturePoint(int capturePointIndex)
    {
        targetCapturePoint = capturePointIndex;
        var capturePoint = capturesPoints[targetCapturePoint];
        Arrive(capturePoint.transform.position, true, capturePointRange, OnEnterCapturePoint);
    }

    // Callback called when the champion enters its target capture point.
    private void OnEnterCapturePoint()
    {
        if (TargetPointIsCaptured())
        {
            SetTargetCapturePoint(getClosestCapturePointOfInterest());
        }
    }
    // Check if the target capture point is captured.
    private bool TargetPointIsCaptured()
    {
        var capturePoint = capturesPoints[targetCapturePoint].GetComponent<CapturePoint>();
        return capturePoint != null && capturePoint.IsOwnedByTeam(team);
    }

    //Calculates the threat level by counting ally units vs enemy units, when there is no threat, lvl is -5
    //Can be put in with closest enemy method but for now we can leave it like this.
    void calculateThreatLevel()
    {
        Collider[] colliders = Physics.OverlapSphere(
        transform.position,
        threatRange,
        LayerMask.GetMask("Units"),
        QueryTriggerInteraction.Ignore);
        if (colliders.Length <= 1)
        {
            threatLvl = minThreatLvl;
        }
        var orderedColliders = colliders.OrderBy(
        collider => Vector3.Distance(collider.transform.position, transform.position));
        foreach(Collider col in orderedColliders)
        {
            if(threatLvl < maxThreatLvl)
            {
                if (col.tag == "bluePlayer")
                {
                    threatLvl += 5;
                }
                else if (col.tag == "blueNPC")
                {
                    threatLvl++;
                }
            }
            if(threatLvl > minThreatLvl)
            {
                if (col.tag == "redPlayer")
                {
                    threatLvl -= 5;
                }
                else if (col.tag == "redNPC")
                {
                    threatLvl--;
                }
            }
        }
    }

    private UnitController FindClosestEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            aggressionRange,
            LayerMask.GetMask("Units"),
            QueryTriggerInteraction.Ignore);

        //Check to see if there is a player around first.
        foreach (Collider collider in colliders)
        {
            if (collider.tag == "bluePlayer")
            {
                var enemy = collider.GetComponent<UnitController>();
                if (enemy != null)
                {
                    return enemy;
                }
            }
        }

        var orderedColliders = colliders.OrderBy(
            collider => Vector3.Distance(collider.transform.position, transform.position));
        foreach (Collider collider in orderedColliders)
        {
            if (collider.tag.StartsWith(enemyTeam))
            {
                var enemy = collider.GetComponent<UnitController>();
                if (enemy != null)
                {
                    return enemy;
                }
            }
        }
        return null;
    }
}
