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

    private const float capturePointRange = 4;

    void Update()
    {
        var enemy = FindClosestEnemy();
        if (enemy != null && enemy != target)
        {
            Attack(enemy);
        }
        else if (isStill && !ShouldStay())
        {
            SetTargetCapturePoint(targetCapturePoint);
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
        Arrive(capturePoint.position, capturePointRange, OnEnterCapturePoint);
    }

    // Check if the minion should stay where it is.
    // This checks if it is on its target capture point and if the capture point
    // is owned.
    private bool ShouldStay()
    {
        var capturePoint = manager.capturesPoints[targetCapturePoint].GetComponent<CapturePoint>();
        var distance = Vector3.Distance(transform.position, capturePoint.transform.position);
        if (distance <= capturePointRange && !capturePoint.IsOwnedByTeam(team))
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
            TakeDamage(hitPoints);
            return;
        }
        var capturePoint = manager.capturesPoints[targetCapturePoint].GetComponent<CapturePoint>();
        if (capturePoint.IsOwnedByTeam(team))
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
