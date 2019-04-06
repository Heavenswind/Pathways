using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinionController : UnitController
{
    internal SpawnManager manager;
    private int targetCapturePoint = 0;
    private float aggressionRange = 5;

    private const float capturePointRange = 5;

    void Update()
    {
        var enemy = FindClosestEnemy();
        if (enemy != null && enemy != target)
        {
            Attack(enemy, false);
        }
        else if (isStill && (!InRangeOfPoint() || TargetPointIsCaptured()))
        {
            SetTargetCapturePoint(targetCapturePoint);
        }
        else if (InRangeOfPoint() && TargetPointIsCaptured())
        {
            SetTargetCapturePoint(manager.transitions[targetCapturePoint]);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, aggressionRange);
    }

    // Set the target capture point of the minion.
    // The minion will move toward it.
    public void SetTargetCapturePoint(int capturePointIndex)
    {
        targetCapturePoint = capturePointIndex;
        var capturePoint = manager.capturesPoints[targetCapturePoint];
        Arrive(capturePoint.position, false, capturePointRange, OnEnterCapturePoint);
    }

    // Check if the minion is in range of its target capture point.
    private bool InRangeOfPoint()
    {
        var capturePoint = manager.capturesPoints[targetCapturePoint];
        var distance = Vector3.Distance(transform.position, capturePoint.transform.position);
        return distance <= capturePointRange;
    }

    // Check if the target capture point is captured.
    private bool TargetPointIsCaptured()
    {
        var capturePoint = manager.capturesPoints[targetCapturePoint].GetComponent<CapturePoint>();
        return capturePoint != null && capturePoint.IsOwnedByTeam(team);
    }

    // Check if the minion should stay where it is.
    // This checks if it is on its target capture point and if the capture point
    // is owned.
    private bool ShouldStay()
    {
        var capturePoint = manager.capturesPoints[targetCapturePoint].GetComponent<CapturePoint>();
        if (capturePoint != null && InRangeOfPoint() && !capturePoint.IsOwnedByTeam(team))
        {
            return true;
        }
        return false;
    }

    // Callback called when the minion enters its target capture point.
    private void OnEnterCapturePoint()
    {
        if (targetCapturePoint == manager.capturesPoints.Length - 1)
        {
            Stop();
            Kill();
            return;
        }
        if (TargetPointIsCaptured())
        {
            SetTargetCapturePoint(manager.transitions[targetCapturePoint]);
        }
    }

    // Check if there are enemies within the aggression range.
    // If there are, one is target and the unit will move to attack it.
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
