using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Agent that performs pathfinding to move across the level.
[RequireComponent(typeof(CharacterController))]
public class PathfindingAgent : MonoBehaviour
{
    [Header("Pathfiding Agent")]
    public float maxLinearVelocity = 3.0f;
    
    private const float maxAngularVelocity = 30.0f;
    private const float maxMovementAngle = 90.0f;
    private const float satisfactionRadius = 1.0f;
    private const float timeToTarget = 0.25f;
    private const float arrivalThreshold = 0.1f;

    protected CharacterController controller;
    protected Animator animator;
    protected bool activated = true;
    
    private IEnumerator movement; // coroutine of the movement
    internal Vector3 velocity; // velocity of the agent
    private Vector3? movementTargetPosition; // target movement position
    private float arrivalAcceptanceRange;
    private Action onMovementCompletionAction; // action performed after completing a movement coroutine
    private const float pathRecalculationThreshold = 1;
    private string[] ignoredColliders = new string[]{"Ground", "capturePoint"};
    private bool avoid = false;

    public bool isStill
    {
        get { return velocity == Vector3.zero; }
    }

    // Initialize the pathfinding agent.
    protected virtual void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        // Method implemented in case of future need
    }

    // Action to perform when the controller collides.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!ignoredColliders.Contains(hit.collider.tag))
        {
            if (movementTargetPosition.HasValue
                && Vector3.Distance(transform.position, movementTargetPosition.Value) < arrivalAcceptanceRange)
            {
                Stop(true);
            }
            else if (tag.EndsWith("Player") && hit.collider.tag.EndsWith("NPC"))
            {
                var controller = hit.gameObject.GetComponent<CharacterController>();
                controller.SimpleMove(new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z));
            }
            else if (tag.EndsWith("NPC"))
            {
                var agent = hit.gameObject.GetComponent<PathfindingAgent>();
                if (agent != null && agent.isStill)
                {
                    avoid = true;
                }
            }
        }
    }

    // Disable the unit.
    public void Disable()
    {
        Stop();
        activated = false;
    }

    // Reset the state of the pathfinding agent.
    // It stops any prior movement coroutine.
    // If the movementCompleted flag is set, the current movement completion
    // action is performed.
    public void Stop(bool movementCompleted = false)
    {
        // Halting movement
        if (movement != null)
        {
            StopCoroutine(movement);
        }
        
        // Reset properties
        SetVelocity(Vector3.zero);
        movementTargetPosition = null;
        arrivalAcceptanceRange = 0;
        
        // Movement completion action
        if (movementCompleted && onMovementCompletionAction != null)
        {
            onMovementCompletionAction();
        }
    }

    // Face the target position.
    public virtual void Face(Vector3 position, Action completionAction = null)
    {
        if (!activated) return;
        Stop();
        onMovementCompletionAction = completionAction;
        movement = FaceCoroutine(position);
        StartCoroutine(movement);
    }

    // Arrive at the target position.
    public virtual void Arrive(Vector3 position, float acceptanceRange = 0, Action completionAction = null)
    {
        if (!activated) return;
        Stop();
        movementTargetPosition = position;
        arrivalAcceptanceRange = acceptanceRange;
        onMovementCompletionAction = completionAction;
        movement = ArriveCoroutine(position);
        StartCoroutine(movement);
    }

    // Chase the target.
    public virtual void Chase(Transform target, float acceptanceRange = 0, Action completionAction = null)
    {
        if (!activated) return;
        Stop();
        arrivalAcceptanceRange = acceptanceRange;
        onMovementCompletionAction = completionAction;
        movement = ChaseCoroutine(target);
        StartCoroutine(movement);
    }

    // Smooth the path by checking which is the further node that the agent is
    // unobstructed to move toward.
    // Return the index of the next target node as well as its world position.
    private (int, Vector3) SmoothPath(List<Vector2> path, int currentNodeIndex)
    {
        // Check the furthest visible node
        GetComponent<Collider>().enabled = false;
        var targetNodeIndex = currentNodeIndex;
        for (int i = currentNodeIndex + 1; i < path.Count; ++i)
        {
            var checkHeight = PathfindingGraph.instance.graphHeight
                + PathfindingGraph.instance.nodeWidth;
            var pathIsClear = PathfindingGraph.instance.HasClearPath(
                transform.position,
                new Vector3(path[i].x, 0, path[i].y));
            var withinRange = arrivalAcceptanceRange > 0
                && Vector2.Distance(path[i], path.Last()) <= arrivalAcceptanceRange + 1;
            if (pathIsClear || withinRange)
            {
                targetNodeIndex = i;
            }
            else
            {
                break;
            }
        }
        GetComponent<Collider>().enabled = true;

        // Recompute target node position
        var targetNodePosition = new Vector3(
            path[targetNodeIndex].x,
            transform.position.y,
            path[targetNodeIndex].y);

        return (targetNodeIndex, targetNodePosition); 
    }

    // Set the velocity of the agent.
    private void SetVelocity(Vector3 velocity)
    {
        this.velocity = velocity;
        if (avoid)
        {
            this.velocity += transform.right;
            avoid = false;
        }
        animator.SetFloat("speed", velocity.magnitude);
    }

    // Make the agent rotate toward the given position.
    // Return true when the agent is facing the target position.
    private bool RotateToward(Vector3 position)
    {
        Vector3 direction = (position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                maxAngularVelocity * Time.timeScale);
        }
        return transform.forward == direction;
    }

    // Make the agent move toward the given position.
    // The agent will arrive smoothly if the flag is set.
    // Return true when the agent has arrived at the target position.
    private bool MoveToward(Vector3 position, bool arriveSmoothly = false)
    {
        if (!arriveSmoothly)
        {
            SetVelocity(transform.forward * maxLinearVelocity);
        }
        else
        {
            var distance = (position - transform.position).magnitude;
            if (distance > satisfactionRadius)
            {
                SetVelocity(transform.forward * maxLinearVelocity);
            }
            else if (distance > arrivalThreshold)
            {
                var magnitude = Mathf.Min(maxLinearVelocity, distance / timeToTarget);
                SetVelocity(transform.forward * magnitude);
            }
            else
            {
                return true;
            }
        }
        controller.SimpleMove(velocity);
        return false;
    }

    // Make the agent rotate toward the given target position.
    private IEnumerator FaceCoroutine(Vector3 position)
    {
        while (!RotateToward(position))
        {
            yield return null;
        }
        Stop(true);
    }

    // Make the agent arrive smoothly at the given target position.
    private IEnumerator ArriveCoroutine(Vector3 position)
    {
        var exactArrival = arrivalAcceptanceRange == 0;
        var path = PathfindingGraph.instance.ComputePath(transform.position, position, exactArrival);
        var targetNodeIndex = 0;
        var targetNodePosition = transform.position;
        var arrived = false;
        do
        {
            (targetNodeIndex, targetNodePosition) = SmoothPath(path, targetNodeIndex);
            RotateToward(targetNodePosition);
            arrived = MoveToward(targetNodePosition, true);
            if (arrived)
            {
                if (targetNodeIndex == path.Count - 1)
                {
                    break;
                }
                else
                {
                    ++targetNodeIndex;
                    arrived = false;
                }
            }
            yield return null;
        }
        while (!arrived);
        Stop(true);
    }

    // Make the agent chase the given target.
    private IEnumerator ChaseCoroutine(Transform target)
    {
        var position = target.position;
        var path = PathfindingGraph.instance.ComputePath(transform.position, position, true);
        var targetNodeIndex = 0;
        var targetNodePosition = transform.position;
        var arrived = false;
        do
        {
            if (Vector3.Distance(position, target.position) > pathRecalculationThreshold)
            {
                position = target.position;
                path = PathfindingGraph.instance.ComputePath(transform.position, position);
                targetNodeIndex = 0;
                targetNodePosition = transform.position;
            }
            (targetNodeIndex, targetNodePosition) = SmoothPath(path, targetNodeIndex);
            RotateToward(targetNodePosition);
            arrived = MoveToward(targetNodePosition);
            if (arrived && targetNodeIndex < path.Count - 2)
            {
                ++targetNodeIndex;
                arrived = false;
            }
            yield return null;
            if (target == null)
            {
                Stop(false);
                yield break;
            }
        }
        while (Vector3.Distance(position, transform.position) > arrivalAcceptanceRange);
        Stop(true);
    }
}
