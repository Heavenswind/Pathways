using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAllyController : UnitController
{
    private const float capturePointRange = 2;
    private const float aggressionRange = 10;

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
        float critical = Mathf.Clamp01(-10 * hpRatio + 3);
        float hurt = Mathf.Clamp01(-10 * hpRatio + 5) - critical;
        float healthy = 1 - hurt;
        //Debug.Log(string.Format("hp ratio: {0}, critical: {1}, hurt: {2}, healthy: {3}", hpRatio, critical, hurt, healthy));

        float inDanger = Mathf.Clamp01(-0.5f * enemyDistance + 2);
        float inCombat = Mathf.Clamp01(-0.5f * enemyDistance + 5) - inDanger;
        float safe = 1 - inCombat;
        float canAttack = (Time.time >= nextAttack)? 1 : 0;
        float cannotAttack = 1 - canAttack;
        
        float onPoint = Mathf.Clamp01(-1f/3f * capturePointDistance + 10f/6f);
        float awayFromPoint = 1 - onPoint;
        float captured = Mathf.Clamp01(0.1f * capturePoint.score);
        float notCaptured = 1 - captured;        

        // Fuzzy output after rule evaluation
        float returnToBase = Mathf.Max(critical, Mathf.Min(hurt, safe)); // critical || (hurt && safe)
        float attack = Mathf.Min(canAttack, Mathf.Max(inCombat, inDanger)); // canAttack && (inCombat || inDanger)
        float moveAway = Mathf.Min(cannotAttack, inDanger); // cannotAttack && inDanger
        float moveToPoint = Mathf.Min(awayFromPoint, notCaptured, safe); // awayFromPoint && notCaptured && safe
        float stay = Mathf.Max(Mathf.Min(onPoint, notCaptured), Mathf.Max(inCombat, inDanger)); // (onPoint && notCaptured) || (inCombat || inDanger)
        //Debug.Log(string.Format("Return to base: {0}, attack: {1}, move away: {2}, move to point: {3}, stay: {4}", returnToBase, attack, moveAway, moveToPoint, stay));

        // Crisp output after defuzzification
        if (returnToBase >= Mathf.Max(attack, moveAway, moveToPoint, stay))
        {
            Arrive(teamBase, capturePointRange, Heal);
        }
        else if (moveAway >= Mathf.Max(attack, moveToPoint, stay))
        {
            Arrive(transform.position + (transform.position - closestEnemy.transform.position).normalized * 3);
        }
        else if (attack >= Mathf.Max(moveToPoint, stay))
        {
            if (PathfindingGraph.instance.HasClearPath(transform.position, closestEnemy.transform.position, 0.5f))
            {
                Fire(closestEnemy.transform.position);
            }
            else
            {
                Chase(closestEnemy.transform);
            }
        }
        else if (moveToPoint >= stay)
        {
            SetTargetCapturePoint(targetCapturePoint);
        }
        else
        {
            Stop();
        }
    }

    // Gets the closest point of interest based on neutrality or enemy capture.
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
}
