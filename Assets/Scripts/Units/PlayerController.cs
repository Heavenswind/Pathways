using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : UnitController
{
    void Update()
    {
        // Poll user input
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 targetPosition;
            if (PlayerTargetPosition(out targetPosition))
            {
                MoveTo(targetPosition);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Vector2 targetPosition;
            if (PlayerTargetPosition(out targetPosition))
            {
                FireAt(targetPosition);
            }
        }
    }

    // Return the player target position computed from their mouse position.
    private bool PlayerTargetPosition(out Vector2 targetPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool targetExists = Physics.Raycast(ray, out hit, Mathf.Infinity,
            layerMask: Physics.DefaultRaycastLayers,
            queryTriggerInteraction: QueryTriggerInteraction.Ignore);
        targetPosition = new Vector2(hit.point.x, hit.point.z);
        return targetExists;
    }
}
