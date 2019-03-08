using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PathfindingAgent
{
    void Update()
    {
        // Move to the clicked spot on the level
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(
                ray,
                out hit,
                100.0f,
                layerMask: Physics.DefaultRaycastLayers,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore
            ))
            {
                Vector2 target = new Vector2(hit.point.x, hit.point.z);
                MoveTo(target);
            }
        }
    }
}
