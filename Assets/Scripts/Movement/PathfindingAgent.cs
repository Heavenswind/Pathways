using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
// Agent that performs pathfinding to move across the level.
public class PathfindingAgent : MonoBehaviour
{
    public float maxLinearVelocity = 3.0f;
    
    static float maxAngularVelocity = 7.5f;
    static float threshold = 0.1f;
    static float satisfactionRadius = 1.0f;
    static float timeToTarget = 0.25f;
    static float maxMovementAngle = 90.0f;

    PathfindingGraph graph; // pathfinding graph of the level
    new Rigidbody rigidbody; // rigidbody of the agent
    new Collider collider; // collider of the agent
    Animator animator;
    
    Vector3 velocity; // velocity of the agent
    IEnumerator movement; // coroutine of the movement

    // Initialize the pathfinding agent.
    public virtual void Start()
    {
        graph = GameObject.FindWithTag("Level").GetComponent<PathfindingGraph>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        animator = GetComponent<Animator>();
    }

    // Make the agent move toward the target position following the shortest path possible.
    public void MoveTo(Vector2 targetPosition)
    {
        if (movement != null)
        {
            StopCoroutine(movement);
        }
        Vector2 currentPosition = new Vector2(rigidbody.position.x, rigidbody.position.z);
        List<Vector2> path = graph.ComputePath(currentPosition, targetPosition);
        movement = FollowPath(path);
        StartCoroutine(movement);
    }

    // Make the agent stop its movement.
    public void Stop()
    {
        velocity = Vector3.zero;
        animator.SetFloat("speed", velocity.magnitude);
    }

    // Make the agent follow the given path.
    // The path is smoothed during the process.
    IEnumerator FollowPath(List<Vector2> path)
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
            collider.enabled = false;
            for (int i = targetIndex + 1; i < path.Count; ++i)
            {
                if (!Physics.CheckCapsule(
                    new Vector3(
                        rigidbody.position.x,
                        graph.graphHeight + graph.nodeWidth,
                        rigidbody.position.z
                    ),
                    new Vector3(path[i].x, graph.graphHeight + graph.nodeWidth, path[i].y),
                    graph.nodeWidth,
                    layerMask: Physics.DefaultRaycastLayers,
                    queryTriggerInteraction: UnityEngine.QueryTriggerInteraction.Ignore
                ))
                {
                    targetIndex = i;
                }
            }
            collider.enabled = true;
            
            // Rotate towards direction
            Vector3 targetPosition = new Vector3(
                path[targetIndex].x,
                rigidbody.position.y,
                path[targetIndex].y
            );
            Vector3 direction = (targetPosition - rigidbody.transform.position).normalized;
            rigidbody.MoveRotation(Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                maxAngularVelocity
            ));
            if (Vector3.Angle(transform.forward, direction) > maxMovementAngle)
            {
                yield return new WaitForFixedUpdate();
                continue;
            }

            // Move forward
            float distance = (targetPosition - rigidbody.transform.position).magnitude;
            if (targetIndex < path.Count - 1)
            {
                velocity = transform.forward * maxLinearVelocity;
            }
            else
            {
                if (distance > satisfactionRadius)
                {
                    velocity = transform.forward * maxLinearVelocity;
                }
                else if (distance > threshold)
                {
                    velocity = transform.forward
                        * Mathf.Min(maxLinearVelocity, distance / timeToTarget);
                }
                else
                {
                    Stop();
                    yield break;
                }
            }
            rigidbody.MovePosition(rigidbody.position + velocity * Time.deltaTime);
            distance = (targetPosition - rigidbody.transform.position).magnitude;
            if (targetIndex < path.Count - 1 && distance < threshold)
            {
                targetIndex += 1;
            }
            
            animator.SetFloat("speed", velocity.magnitude);
            yield return new WaitForFixedUpdate();
        }
    }
}
