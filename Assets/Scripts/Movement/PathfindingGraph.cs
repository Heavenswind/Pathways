using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Graph that is overlaid ontop of the level geometry.
// It is an implementation of a tile graph with up to 8 connections per node.
// It is used to perform agent pathfinding through the level.
public class PathfindingGraph : MonoBehaviour
{
    [SerializeField] internal float nodeDistance = 1.0f; // distance at which nodes are placed
    [SerializeField] internal float nodeWidth = 1.0f; // clearance width required for placing a node
    [SerializeField] internal float graphHeight = 0.0f; // height at which the graph performs checks

    internal static int levelLayerMask; // layer mask which contains the static level geometry
    
    // set of graph nodes
    private HashSet<Vector2> nodes
        = new HashSet<Vector2>();

    // dictionary of node edges and costs
    // format: Dictionary<fromNode, Dictionary<toNode, edgeCost>>
    private Dictionary<Vector2, Dictionary<Vector2, float>> edges
        = new Dictionary<Vector2, Dictionary<Vector2, float>>();

    private Bounds bounds; // bounds of the level, equal to the bounds of the ground plane

    // Initialize the pathfinding graph.
    void Start()
    {
        bounds = GameObject.FindWithTag("Ground").GetComponent<Collider>().bounds;
        levelLayerMask = LayerMask.GetMask("Level");
        CreateGraph();
    }

    // Draw the pathfinding graph nodes and edges in the scene if the gizmo is enabled.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector2 node in nodes)
        {
            Gizmos.DrawSphere(new Vector3(node.x, graphHeight, node.y), 0.1f);
        }
        foreach (Vector2 from in edges.Keys)
        {
            foreach (Vector2 to in edges[from].Keys)
            {
                Gizmos.DrawLine(
                    new Vector3(from.x, graphHeight, from.y),
                    new Vector3(to.x, graphHeight, to.y)
                );
            }
        }
    }

    // Compute and return the shortest path from the position to the target.
    // This is an implementation of the A* pathfinding algorithm.
    // It uses the Euclidean distance as the heuristic.
    // The approximate flag allows the character to move the the nearest node if the target position
    // is obstructed.
    public List<Vector2> ComputePath(Vector2 position, Vector2 target, bool approximate = true)
    {
        // Check target validity
        if (approximate && Physics.CheckSphere(
            new Vector3(target.x, graphHeight + nodeWidth, target.y),
            nodeWidth,
            layerMask: Physics.DefaultRaycastLayers,
            queryTriggerInteraction: UnityEngine.QueryTriggerInteraction.Ignore))
        {
            target = ClosestNode(target);
        }

        // Setup required data structures
        HashSet<Vector2> closed = new HashSet<Vector2>();
        HashSet<Vector2> open = new HashSet<Vector2>();
        Dictionary<Vector2, Vector2> connections = new Dictionary<Vector2, Vector2>();
        Dictionary<Vector2, float> costSoFar = new Dictionary<Vector2, float>();
        Dictionary<Vector2, float> estimatedTotalCost = new Dictionary<Vector2, float>();
        Vector2 startNode = nodes.Contains(position)? position : ClosestNode(position);
        Vector2 endNode = nodes.Contains(target)? target : ClosestNode(target);

        open.Add(startNode);
        costSoFar.Add(startNode, 0);
        estimatedTotalCost.Add(startNode, Heuristic(startNode, endNode));

        // Perform traversal through graph
        Vector2 current = startNode;
        while (open.Count > 0)
        {
            // Select which node to process next
            current = open.First();
            foreach (Vector2 node in open)
            {
                if (estimatedTotalCost[node] < estimatedTotalCost[current])
                {
                    current = node;
                }
            }
            if (current == endNode)
            {
                break;
            }
            open.Remove(current);
            closed.Add(current);

            // Visit accessible connected nodes
            foreach (Vector2 neighbor in edges[current].Keys)
            {
                if (closed.Contains(neighbor))
                {
                    continue;
                }
                float cost = costSoFar[current] + edges[current][neighbor];
                if (!open.Contains(neighbor))
                {
                    open.Add(neighbor);
                }
                else if (cost >= costSoFar[neighbor])
                {
                    continue;
                }
                connections[neighbor] = current;
                costSoFar[neighbor] = cost;
                estimatedTotalCost[neighbor] = cost + Heuristic(neighbor, endNode);
            }
        }

        // Retrieve path
        List<Vector2> path = new List<Vector2>{endNode, target};
        Vector2 pathNode = endNode;
        while (pathNode != startNode)
        {
            pathNode = connections[pathNode];
            path.Insert(0, pathNode);
        }

        return path;
    }

        // Create the pathfinding graph.
    // Nodes and edges are generated automatically using physics checks to detect obstacles.
    private void CreateGraph()
    {
        // Create the nodes
        for (float x = bounds.min.x; x <= bounds.max.x; x += nodeDistance)
        {
            for (float y = bounds.min.z; y <= bounds.max.z; y += nodeDistance)
            {
                if (!Physics.CheckSphere(
                    new Vector3(x, graphHeight + nodeWidth, y),
                    nodeWidth,
                    layerMask: levelLayerMask,
                    queryTriggerInteraction: UnityEngine.QueryTriggerInteraction.Ignore))
                {
                    Vector2 node = new Vector2(x, y);
                    nodes.Add(new Vector2(x, y));
                    edges.Add(node, new Dictionary<Vector2, float>());
                }
            }
        }

        // Create the edges
        foreach (Vector2 node in nodes)
        {
            Vector2[] neighbors = {
                node + new Vector2(-nodeDistance, 0),
                node + new Vector2(-nodeDistance, nodeDistance),
                node + new Vector2(0, nodeDistance),
                node + new Vector2(nodeDistance, nodeDistance)
            };
            foreach (Vector2 neighbor in neighbors)
            {
                if (nodes.Contains(neighbor) && !Physics.CheckCapsule(
                    new Vector3(node.x, graphHeight + nodeWidth, node.y),
                    new Vector3(neighbor.x, graphHeight + nodeWidth, neighbor.y),
                    nodeWidth,
                    layerMask: levelLayerMask,
                    queryTriggerInteraction: UnityEngine.QueryTriggerInteraction.Ignore))
                {
                    float distance = Vector2.Distance(node, neighbor);
                    edges[node].Add(neighbor, distance);
                    edges[neighbor].Add(node, distance);
                }
            }
        }
    }

    // Return the position of the closest graph node to the given position.
    private Vector2 ClosestNode(Vector2 position)
    {
        Vector2 closest = nodes.First();
        foreach (Vector2 node in nodes)
        {
            if (Vector2.Distance(node, position) < Vector2.Distance(closest, position))
            {
                closest = node;
            }
        }
        return closest;
    }

    // Heuristic to use by the A* pathfinding algorithm.
    // Returns the Euclidean distance between the position and the target.
    private float Heuristic(Vector2 position, Vector2 target)
    {
        return Vector2.Distance(position, target);
    }
}
