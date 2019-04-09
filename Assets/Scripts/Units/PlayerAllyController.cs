using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAllyController : UnitController
{
    [Header("Player Ally Controller")]
    public Transform ally;

    private const float capturePointRange = 2.5f;
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

        float inDanger = Mathf.Clamp01(-1 * enemyDistance + 5);
        float inCombat = Mathf.Clamp01(-1 * enemyDistance + 8) - inDanger;
        float enemyInSight = Mathf.Clamp01(-0.5f * enemyDistance + 6) - (inDanger + inCombat);
        float outOfCombat = 1 - (inDanger + inCombat + enemyInSight);
        float canAttack = (Time.time >= nextAttack)? 1 : 0;
        float cannotAttack = 1 - canAttack;
        //Debug.Log(string.Format("danger: {0}, combat: {1}, sight: {2}, out: {3}", inDanger, inCombat, enemyInSight, outOfCombat));
        
        float onPoint = (capturePointDistance <= capturePointRange)? 1 : 0;
        float awayFromPoint = 1 - onPoint;
        float captured = capturePoint.IsOwnedByTeam(team)? 1 : 0;
        float notCaptured = 1 - captured;

        // Fuzzy output after rule evaluation
        float returnToBase = Mathf.Max(critical, Mathf.Min(hurt, outOfCombat)); // critical || (hurt && safe)
        float attack = Mathf.Min(canAttack, Mathf.Max(inCombat, inDanger)); // canAttack && (inCombat || inDanger)
        float flee = Mathf.Min(cannotAttack, inDanger); // cannotAttack && inDanger
        float chase = Mathf.Min(enemyInSight, 1 - inCombat);
        float moveToPoint = Mathf.Min(awayFromPoint, notCaptured, outOfCombat); // awayFromPoint && notCaptured && safe
        float stay = Mathf.Max(Mathf.Min(onPoint, notCaptured, 1 - inDanger), inCombat); // (onPoint && notCaptured && !inDanger) || inCombat
        //Debug.Log(string.Format("Return to base: {0}, attack: {1}, flee: {2}, chase: {3}, move to point: {4}, stay: {5}", returnToBase, attack, flee, chase, moveToPoint, stay));

        // Crisp output after defuzzification
        if (returnToBase >= Mathf.Max(flee, attack, chase, moveToPoint, stay))
        {
            //Debug.Log("return to base");
            Arrive(teamBase, false, 5, Heal);
        }
        else if (flee >= Mathf.Max(attack, chase, moveToPoint, stay))
        {
            //Debug.Log("flee");
            Arrive(transform.position + (transform.position - closestEnemy.transform.position).normalized * 3, false);
        }
        else if (attack >= Mathf.Max(chase, moveToPoint, stay))
        {
            if (PathfindingGraph.instance.HasClearPath(transform.position, closestEnemy.transform.position, 0.5f))
            {
                //Debug.Log("attack");
                var distance = Vector3.Distance(transform.position, closestEnemy.transform.position);
                var timeToTarget = distance / projectile.GetComponent<Projectile>().speed;
                Fire(closestEnemy.transform.position + (closestEnemy.velocity * timeToTarget));
            }
            else
            {
                //Debug.Log("chase to attack");
                Chase(closestEnemy.transform, false);
            }
        }
        else if (chase >= Mathf.Max(moveToPoint, stay))
        {
            //Debug.Log("chase");
            Chase(closestEnemy.transform, false);
        }
        else if (moveToPoint >= stay)
        {
            //Debug.Log("move to base");
            SetTargetCapturePoint(targetCapturePoint);
        }
        else
        {
            //Debug.Log("stay");
            Stop();
        }
    }

    // Gets the closest point of interest based on neutrality or enemy capture.
    private int ChooseCapturePoint()
    {
        float shortestDistance = Mathf.Infinity;
        int bestPoint = 0;
        for (int i = 0; i < capturePoints.Length; i++)
        {
            if (!capturePoints[i].IsOwnedByTeam(team))
            {
                var point = capturePoints[i].transform;
                float distanceToPoint = Vector3.Distance(transform.position, point.position);
                if (distanceToPoint < shortestDistance
                    && (Vector3.Distance(ally.position, point.position) > 15
                        || Vector3.Distance(transform.position, ally.position) < 10
                        || IsAllyInDanger()))
                {
                    shortestDistance = distanceToPoint;
                    bestPoint = i;
                }
            }
        }
        return bestPoint;
    }

    private bool IsAllyInDanger()
    {
        Collider[] colliders = Physics.OverlapSphere(
            ally.position,
            aggressionRange * 2,
            LayerMask.GetMask("Units"),
            QueryTriggerInteraction.Ignore);
        var enemyCount = colliders.Count(collider => collider.tag.StartsWith(enemyTeam));
        return enemyCount > 5;
    }
    
    // Set the target capture point of the unit.
    // The unit will move toward it.
    private void SetTargetCapturePoint(int capturePointIndex)
    {
        var capturePoint = capturePoints[capturePointIndex];
        Arrive(capturePoint.transform.position, false, capturePointRange, ChooseNextAction);
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
