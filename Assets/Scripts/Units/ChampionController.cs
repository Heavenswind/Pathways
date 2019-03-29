using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChampionController : UnitController
{
    public CapturePoint[] capturesPoints;
    private int targetCapturePoint = 0;
    private const float capturePointRange = 5;
    private float aggressionRange = 6;
    public CapturePoint cp = null;
    int capturePointOfInterest = 0;

    public float threatLvl = 0;
    public float dangerLevel = 6;
    public float minThreatLvl = 0;
    public float maxThreatLvl = 10;

    private float threatRange = 5;
    public ChampionController ally;

    void Update()
    {
        var enemy = FindClosestEnemy();
        if (enemy != null && enemy != target)
        {
            Attack(enemy);
        }
        if (threatLvl < dangerLevel && cp == null)
        {
            targetCapturePoint = getClosestCapturePointOfInterest();
            cp = capturesPoints[getClosestCapturePointOfInterest()];
        }
        else if(threatLvl >= dangerLevel)
        {
            AskForHelp();
        }
        else if (isStill && (!InRangeOfPoint() || TargetPointIsCaptured()))
        {
            SetTargetCapturePoint(targetCapturePoint);
        }
        else if (InRangeOfPoint() && TargetPointIsCaptured())
        {
            cp = null;
            SetTargetCapturePoint(targetCapturePoint);
        }
    }
    private void FixedUpdate()
    {
        calculateThreatLevel();
    }

    int getClosestCapturePointOfInterest()
    {
        float shortestDistance = Mathf.Infinity;
        for (int i = 0; i < capturesPoints.Length; i++)
        {
            print("Shortest distance: " + shortestDistance);
            if(!capturesPoints[i].IsOwnedByTeam(team) && capturesPoints[i] != ally.cp)
            {
                float distanceToPoint = Vector3.Distance(transform.position, capturesPoints[i].transform.position);
                if(distanceToPoint < shortestDistance)
                {
                    shortestDistance = distanceToPoint;
                    capturePointOfInterest = i;
                }
            }
        }
        return capturePointOfInterest;
    }

    public void AskForHelp()
    {
        if(ally != null)
        {
            ally.SetTargetCapturePoint(targetCapturePoint);
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
        if (targetCapturePoint == capturesPoints.Length - 1)
        {
            Stop();
            Kill();
            return;
        }
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


    void calculateThreatLevel()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            threatRange,
            LayerMask.GetMask("Units"),
            QueryTriggerInteraction.Ignore);
        if(colliders.Length <= 1)
        {
            threatLvl = 0;
        }
        else
        {
            var orderedColliders = colliders.OrderBy(
            collider => Vector3.Distance(collider.transform.position, transform.position));
            foreach (Collider collider in orderedColliders)
            {
                if (threatLvl < maxThreatLvl)
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
                }
                if(threatLvl > minThreatLvl)
                {
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
