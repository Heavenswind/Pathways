using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinionController : UnitController
{
    internal SpawnManager manager;
    private int targetCapturePoint = 0;
    private float aggressionRange = 4;

    private const float capturePointRange = 2;

    void Update()
    {
        if (target == null)
        {
            if (CheckForEnemies())
            {
                Attack(target);
            }
            else if (isStill)
            {
                SetTargetCapturePoint(targetCapturePoint);
            }
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
        MoveTo(capturePoint.position, capturePointRange, OnEnterCapturePoint);
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
    private bool CheckForEnemies()
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
                target = collider.GetComponent<UnitController>();
                if (target != null)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
