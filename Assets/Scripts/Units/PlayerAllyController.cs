using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAllyController : UnitController
{
    private const float aggressionRange = 8;
    private const float threatRange = 10;
    private const float capturePointRange = 5;
    private const float collabDistance = 30;

    private CapturePoint[] capturePoints;

    protected override void Awake()
    {
        base.Awake();
        capturePoints = Object.FindObjectsOfType<CapturePoint>();
    }

    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        ChooseNextAction();
    }

    // Determine which action the unit will do next.
    private void ChooseNextAction()
    {
        // Crisp input
        var hpRatio = ((float)hitPoints / totalHitPoints);
        var targetCapturePoint = ChooseCapturePoint();
        var capturePoint = capturePoints[targetCapturePoint];
        var capturePointDistance = Vector3.Distance(transform.position, capturePoint.transform.position);
        var closestMinion = FindClosestEnemy(enemyTeam + "NPC");
        var closestChampion = FindClosestEnemy(enemyTeam + "Player");

        // Fuzzy input
        float healthy = Mathf.Clamp01((10f / 6f) * hpRatio - (1f / 3f));
        float hurt = 1 - healthy;
        float onPoint = (capturePointDistance < 5)? 1 : 0;
        float closeToPoint = Mathf.Clamp01(-0.1f * capturePointDistance + 1.5f);
        float farFromPoint = 1 - closeToPoint;
        float captured = capturePoint.IsOwnedByTeam(team)? 1 : 0;

        // Fuzzy output after rule evaluation
        float moveToPoint = Mathf.Min(1 - onPoint, 1 - captured); // not on point and not captured

        // Crisp output after defuzzification
        if (moveToPoint == 1)
        {
            SetTargetCapturePoint(targetCapturePoint);
        }
    }

    //Gets the closest point of interest based on neutrality or enemy capture.
    int ChooseCapturePoint()
    {
        float shortestDistance = Mathf.Infinity;
        int bestPoint = 0;
        for (int i = 0; i < capturePoints.Length; i++)
        {
            if (!capturePoints[i].IsOwnedByTeam(team))
            {
                float distanceToPoint = Vector3.Distance(transform.position, capturePoints[i].transform.position);
                if (distanceToPoint < shortestDistance)
                {
                    shortestDistance = distanceToPoint;
                    bestPoint = i;
                }
            }
        }
        return bestPoint;
    }
    
    // Set the target capture point of the unit.
    // The unit will move toward it.
    private void SetTargetCapturePoint(int capturePointIndex)
    {
        var capturePoint = capturePoints[capturePointIndex];
        Arrive(capturePoint.transform.position, capturePointRange, ChooseNextAction);
    }

    // Find the closest enemy with the given targ within the aggression range.
    private UnitController FindClosestEnemy(string tag)
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
            if (collider.tag == tag)
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
