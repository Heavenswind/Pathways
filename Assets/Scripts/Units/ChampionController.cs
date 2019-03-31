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

    //Variables for threat logic
    public float threatLvl = 0;
    private float dangerLevel = 6;
    private float minThreatLvl = 0;
    private float maxThreatLvl = 10;

    //Initial position
    Vector3 initialPosition;

    //Range distances
    private float aggressionRange = 8;
    private float threatRange = 10;
    private const float capturePointRange = 5;
    private float collabDistance = 10;

    //Ally gameobject reference
    public ChampionController ally;

    //On start, get a reference to all capture points and set a target.
    new void Start()
    {
        base.Start();
        capturesPoints = FindObjectsOfType<CapturePoint>();
        targetCapturePoint = getClosestCapturePointOfInterest();
        initialPosition = transform.position;
    }

    void Update()
    {
        //As long as we are not in danger...
        if (!inDanger())
        {
            //Get the closest enemy to us if we see one.
            var enemy = FindClosestEnemy();
            if (enemy != null && enemy != target)
            {
                if (Vector3.Distance(enemy.transform.position, transform.position) <= (aggressionRange / 2.0f))
                    Attack(enemy);
                else
                    Fire(enemy.transform.position);
            }

            //If im not moving and either im not in range of my target or the target point is captured, go to the capture point
            if (isStill && (!InRangeOfPoint() || TargetPointIsCaptured()))
            {
                SetTargetCapturePoint(targetCapturePoint);
            }
            //else if im in range and the target point is captured, get a new capture point.
            else if (InRangeOfPoint() && TargetPointIsCaptured())
            {
                targetCapturePoint = getClosestCapturePointOfInterest();
                SetTargetCapturePoint(targetCapturePoint);
            }
        }
        else
        {
            print(gameObject.name + " is in danger, returning to base");
            //Michael Jordan: Stop it. Get some help
            GetHelpOnPoint();
        }
        
    }
    private void FixedUpdate()
    {
        calculateThreatLevel();
    }

    //Returns if its in danger.
    bool inDanger()
    {
        return threatLvl >= dangerLevel || hitPoints <= (totalHitPoints / 3.0f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggressionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, threatRange);
    }

    //Gets the closest point of interest based on neutrality or enemy capture.
    int getClosestCapturePointOfInterest()
    {
        float shortestDistance = Mathf.Infinity;
        int bestPoint = 0;
        for (int i = 0; i < capturesPoints.Length; i++)
        {
            if(!capturesPoints[i].IsOwnedByTeam(team) && i != ally.targetCapturePoint)
            {
                float distanceToPoint = Vector3.Distance(transform.position, capturesPoints[i].transform.position);
                if(distanceToPoint < shortestDistance)
                {
                    shortestDistance = distanceToPoint;
                    bestPoint = i;
                }
            }
        }
        print(gameObject.name +  " thinks that the Shortest distance is: " + shortestDistance + " to point " + bestPoint);
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
        print(gameObject.name + " thinks that the safest point is: " + bestPoint + " with red: " + capturesPoints[bestPoint].redTeam + " vs blue: " + capturesPoints[bestPoint].blueTeam);
        return bestPoint;
    }
    
    // Ask ally to help on the point
    public void GetHelpOnPoint()
    {
        // If not to far, make ally come to the point
        if (ally != null && Vector3.Distance(transform.position, ally.transform.position) <= collabDistance)
        {
            ally.SetTargetCapturePoint(targetCapturePoint);
        }
        // Go to the base to heal.
        else
        {
            Arrive(initialPosition, capturePointRange);
            if(Vector3.Distance(transform.position, initialPosition) <= 1)
            {
                hitPoints = totalHitPoints;
            }
        }
    }

    // Check if the champion is in range of its target capture point.
    private bool InRangeOfPoint()
    {
        var capturePoint = capturesPoints[targetCapturePoint];
        var distance = Vector3.Distance(transform.position, capturePoint.transform.position);
        return distance <= capturePointRange;
    }

    // Set the target capture point of the minion.
    // The minion will move toward it.
    public void SetTargetCapturePoint(int capturePointIndex)
    {
        targetCapturePoint = capturePointIndex;
        var capturePoint = capturesPoints[targetCapturePoint];
        Arrive(capturePoint.transform.position, capturePointRange, OnEnterCapturePoint);
    }

    // Callback called when the minion enters its target capture point.
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
        if(threatLvl > minThreatLvl && threatLvl < maxThreatLvl)
        {
            Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            threatRange,
            LayerMask.GetMask("Units"),
            QueryTriggerInteraction.Ignore);
            if (colliders.Length <= 1)
            {
                threatLvl = -5;
            }
            else
            {
                var orderedColliders = colliders.OrderBy(
                collider => Vector3.Distance(collider.transform.position, transform.position));
                foreach (Collider collider in orderedColliders)
                {
                    if (collider.tag.StartsWith(enemyTeam))
                    {
                        var enemy = collider.GetComponent<UnitController>();
                        if (enemy != null)
                        {
                            threatLvl++;
                        }
                        else if (enemy != null && enemy.GetComponent<PlayerController>())
                        {
                            threatLvl += 5;
                        }
                    }
                    if (collider.tag.StartsWith(team))
                    {
                        var ally = collider.GetComponent<UnitController>();
                        if (ally != null)
                        {
                            threatLvl--;
                        }
                        else if (ally != null && ally.GetComponent<ChampionController>())
                        {
                            threatLvl -= 5;

                        }
                    }
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
