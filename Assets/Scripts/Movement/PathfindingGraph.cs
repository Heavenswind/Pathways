using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Graph that is overlaid ontop of the level geometry.
// It is an implementation of a tile graph with up to 8 connections per node.
// It is used to perform agent pathfinding through the level.
public class PathfindingGraph : MonoBehaviour
{
    [SerializeField] Vector2 start = Vector2.zero; // coordinates of the start corner of the graph
    [SerializeField] Vector2 end = Vector2.zero; // coordinates of the end corner of the graph
    [SerializeField] float nodeDistance = 1.0f; // distance at which nodes are placed
    [SerializeField] float nodeWidth = 1.0f; // clearance width required for placing a node
    [SerializeField] float graphHeight = 0.0f; // height at which the graph performs physics checks

    // set of graph nodes
    HashSet<Vector2> nodes
        = new HashSet<Vector2>();

    // dictionary of node edges and costs
    // format: Dictionary<fromNode, Dictionary<toNode, edgeCost>>
    Dictionary<Vector2, Dictionary<Vector2, float>> edges
        = new Dictionary<Vector2, Dictionary<Vector2, float>>(); 

    // Initialize the graph.
    // Automatically generate the nodes and edges between them using physics checks to detect
    // obstacles.
    void Start()
    {
        // Create the nodes
        for (float x = start.x; x <= end.x; x += nodeDistance)
        {
            for (float y = start.y; y <= end.y; y += nodeDistance)
            {
                if (!Physics.CheckSphere(
                    new Vector3(x, graphHeight + nodeWidth, y),
                    nodeWidth,
                    layerMask: Physics.DefaultRaycastLayers,
                    queryTriggerInteraction: UnityEngine.QueryTriggerInteraction.Ignore
                ))
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
                    layerMask: Physics.DefaultRaycastLayers,
                    queryTriggerInteraction: UnityEngine.QueryTriggerInteraction.Ignore
                ))
                {
                    float distance = Vector2.Distance(node, neighbor);
                    edges[node].Add(neighbor, distance);
                    edges[neighbor].Add(node, distance);
                }
            }
        }
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
}
