using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAllyController : UnitController
{
    private const float capturePointRange = 5;
    private const float aggressionRange = 8;
    private const float threatRange = 10;

    private Vector3 teamBase;
    private CapturePoint[] capturePoints;

    protected override void Awake()
    {
        base.Awake();
        teamBase = GameObject.FindWithTag(team + "Base").transform.position;
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
        var closestEnemy = FindClosestEnemy();
        var enemyDistance = (closestEnemy != null)? Vector3.Distance(transform.position, closestEnemy.transform.position) : Mathf.Infinity;

        // Fuzzy input
        float healthy = Mathf.Clamp01((10f / 6f) * hpRatio - (1f / 3f));
        float hurt = 1 - healthy;
        float onPoint = (capturePointDistance < capturePointRange)? 1 : 0;
        float closeToPoint = Mathf.Clamp01(-0.1f * capturePointDistance + 1.5f);
        float farFromPoint = 1 - closeToPoint;
        float captured = capturePoint.IsOwnedByTeam(team)? 1 : 0;
        float canAttack = (Time.time >= nextAttack)? 1 : 0;
        float enemyInRange = Mathf.Clamp01(-0.5f * enemyDistance + 5);
        //float threat = EvaluateThreat();

        // Fuzzy output after rule evaluation
        float returnToBase = hurt;
        float moveToPoint = Mathf.Min(1 - onPoint, 1 - captured, 1 - enemyInRange);
        float stay = Mathf.Max(Mathf.Min(onPoint, 1 - captured), enemyInRange);
        float attack = Mathf.Min(canAttack, enemyInRange);
        //Debug.Log(string.Format("Return to base: {0}, move to point: {1}, stay: {2}, attack: {3}", returnToBase, moveToPoint, stay, attack));

        // Crisp output after defuzzification
        if (returnToBase >= Mathf.Max(attack, moveToPoint, stay))
        {
            Arrive(teamBase, capturePointRange, Heal);
        }
        else if (attack >= Mathf.Max(moveToPoint, stay))
        {
            Fire(closestEnemy.transform.position);
        }
        else if (moveToPoint > stay)
        {
            SetTargetCapturePoint(targetCapturePoint);
        }
        else
        {
            Stop();
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

    private float EvaluateThreat()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            threatRange,
            LayerMask.GetMask("Units"),
            QueryTriggerInteraction.Ignore);
        var orderedColliders = colliders.OrderBy(
            collider => Vector3.Distance(collider.transform.position, transform.position));
        foreach (Collider collider in orderedColliders)
        {
            //if (collider.tag == tag)
            //{
            //    var enemy = collider.GetComponent<UnitController>();
            //    if (enemy != null)
            //    {
            //        return enemy;
            //    }
            //}
        }
        return 0;
    }
}
