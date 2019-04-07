using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : UnitController
{
    private Vector3 playerBase;

    protected override void Start()
    {
        base.Start();
        playerBase = GameObject.FindWithTag("blueBase").transform.position;
    }

    void Update()
    {
        // Poll user input
        if (Time.timeScale != 0)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 targetPosition;
                if (PlayerTargetPosition(out targetPosition))
                {
                    Arrive(targetPosition);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Vector3 targetPosition;
                if (PlayerTargetPosition(out targetPosition))
                {
                    Fire(targetPosition);
                }
            }
        }

        // Heal player at base
        if (Vector3.Distance(transform.position, playerBase) <= 5)
        {
            Heal();
        }
    }

    // Return the player target position computed from their mouse position.
    private bool PlayerTargetPosition(out Vector3 targetPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool targetExists = Physics.Raycast(ray, out hit, Mathf.Infinity,
            layerMask: Physics.DefaultRaycastLayers,
            queryTriggerInteraction: QueryTriggerInteraction.Ignore);
        targetPosition = new Vector3(hit.point.x, 0, hit.point.z);
        return targetExists;
    }
}
