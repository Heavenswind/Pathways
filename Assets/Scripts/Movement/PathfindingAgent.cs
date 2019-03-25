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
    
    private const float maxAngularVelocity = 15.0f;
    private const float maxMovementAngle = 90.0f;
    private const float satisfactionRadius = 1.0f;
    private const float timeToTarget = 0.25f;
    private const float arrivalThreshold = 0.1f;

    protected CharacterController controller;
    protected Animator animator;
    protected bool activated = true;
    
    private PathfindingGraph graph; // pathfinding graph of the level
    private IEnumerator movement; // coroutine of the movement
    private Vector3 velocity; // velocity of the agent
    private Action onMovementCompletionAction; // action performed after completing a movement coroutine

    public bool isStill
    {
        get { return velocity == Vector3.zero; }
    }

    // Initialize the pathfinding agent.
    protected virtual void Awake()
    {
        graph = GameObject.FindWithTag("Level").GetComponent<PathfindingGraph>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Disable the unit.
    public void Disable()
    {
        Stop();
        activated = false;
    }

    // Make the agent move toward the target position following the shortest path possible.
    // The pathing stops when the agent is within the acceptance range of the target position.
    public void MoveTo(Vector2 targetPosition, float acceptanceRange = 0, Action completionAction = null)
    {
        if (!activated) return;
        Stop();
        onMovementCompletionAction = completionAction;
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
        List<Vector2> path = graph.ComputePath(currentPosition, targetPosition, acceptanceRange == 0);
        movement = FollowPathCoroutine(path, acceptanceRange);
        StartCoroutine(movement);
    }

    // Make the agent move toward the target position following the shortest path possible.
    // The pathing stops when the agent is within the acceptance range of the target position.
    public void MoveTo(Vector3 targetPosition, float acceptanceRange = 0, Action completionAction = null)
    {
        MoveTo(new Vector2(targetPosition.x, targetPosition.z), acceptanceRange, completionAction);
    }

    // Make the agent rotate toward the given target position.
    public void RotateTowards(Vector2 targetPosition, Action completionAction = null)
    {
        if (!activated) return;
        Stop();
        onMovementCompletionAction = completionAction;
        movement = RotateTowardsCoroutine(targetPosition);
        StartCoroutine(movement);
    }

    // Make the agent rotate toward the given target position.
    public void RotateTowards(Vector3 targetPosition, Action completionAction = null)
    {
        RotateTowards(new Vector2(targetPosition.x, targetPosition.z), completionAction);
    }

    // Reset the state of the pathfinding agent.
    // It stops any prior movement coroutine.
    // If the movementCompleted flag is set, the current movement completion action is performed.
    public void Stop(bool movementCompleted = false)
    {
        // Halting movement
        if (movement != null)
        {
            StopCoroutine(movement);
        }
        velocity = Vector3.zero;
        animator.SetFloat("speed", velocity.magnitude);
        
        // Movement completion action
        if (movementCompleted && onMovementCompletionAction != null)
        {
            onMovementCompletionAction();
        }
    }

    // Make the agent follow the given path.
    // The path is smoothed during the process.
    // The pathing stops when the agent is within the acceptance range of the goal node.
    private IEnumerator FollowPathCoroutine(List<Vector2> path, float acceptanceRange = 0)
    {
        if (path == null)
        {
            Debug.LogError("A path could not be computed");
            Stop();
            yield break;
        }

        int targetIndex = 0;
        while (true)
        {            
            // Path smoothing
            GetComponent<Collider>().enabled = false;
            for (int i = targetIndex + 1; i < path.Count; ++i)
            {
                var pathIsClear = !Physics.CheckCapsule(
                    new Vector3(transform.position.x, graph.graphHeight + graph.nodeWidth, transform.position.z),
                    new Vector3(path[i].x, graph.graphHeight + graph.nodeWidth, path[i].y),
                    graph.nodeWidth,
                    layerMask: PathfindingGraph.levelLayerMask,
                    queryTriggerInteraction: UnityEngine.QueryTriggerInteraction.Ignore);
                if (pathIsClear || (i == path.Count - 1 && acceptanceRange > 0))
                {
                    targetIndex = i;
                }
                else
                {
                    break;
                }
            }
            GetComponent<Collider>().enabled = true;
            
            // Rotate towards direction
            Vector3 targetPosition = new Vector3(
                path[targetIndex].x,
                transform.position.y,
                path[targetIndex].y
            );
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(direction, Vector3.up),
                    maxAngularVelocity);
            }
            if (Vector3.Angle(transform.forward, direction) > maxMovementAngle)
            {
                yield return new WaitForFixedUpdate();
                continue;
            }

            // Move forward
            float distance = (targetPosition - transform.position).magnitude;
            if (targetIndex < path.Count - 1)
            {
                velocity = transform.forward * maxLinearVelocity;
            }
            else
            {
                if (acceptanceRange > 0 && distance <= acceptanceRange)
                {
                    Stop(true);
                    yield break;
                }
                else if (distance > satisfactionRadius)
                {
                    velocity = transform.forward * maxLinearVelocity;
                }
                else if (distance > arrivalThreshold)
                {
                    velocity = transform.forward
                        * Mathf.Min(maxLinearVelocity, distance / timeToTarget);
                }
                else
                {
                    Stop(true);
                    yield break;
                }
            }
            controller.Move(velocity * Time.deltaTime);
            distance = (targetPosition - transform.position).magnitude;
            if (targetIndex < path.Count - 1 && distance < arrivalThreshold)
            {
                targetIndex += 1;
            }
            
            animator.SetFloat("speed", velocity.magnitude);
            yield return new WaitForFixedUpdate();
        }
    }

    // Make the agent rotate toward the given target position.
    private IEnumerator RotateTowardsCoroutine(Vector2 position)
    {
        var targetPosition = new Vector3(position.x, transform.position.y, position.y);
        Vector3 direction = (targetPosition - transform.transform.position).normalized;
        while (transform.forward != direction)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                maxAngularVelocity);
            yield return new WaitForFixedUpdate();
        }
        Stop(true);
    }
}
