using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target = null;
    public float speed = 1.0f;

    private float height;
    private Vector3 offset;
    private bool targetedMode = true;
    private Vector3 previousMousePosition;

    void Start()
    {
        if (target != null)
        {
            height = transform.position.y;
            offset = transform.position - target.position;
        }
    }

    void Update()
    {
        // Poll user input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            targetedMode = !targetedMode;
        }
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            previousMousePosition = Input.mousePosition;
        }

        // Move camera
        if (targetedMode)
        {
            if (target != null)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    target.position + offset,
                    Time.deltaTime * speed
                );
                transform.position = new Vector3(transform.position.x, height, transform.position.z);
            }
        }
        else
        {
            var zoom = Input.GetAxis("Mouse ScrollWheel");
            transform.position += Vector3.down * zoom * Time.deltaTime * 1000;
            if (Input.GetKey(KeyCode.Mouse2))
            {
                var displacement = Input.mousePosition - previousMousePosition;
                transform.position -= Vector3.right * displacement.x * 0.025f;
                transform.position -= Vector3.forward * displacement.y * 0.025f;
                previousMousePosition = Input.mousePosition;
            }
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, -65, 65),
                Mathf.Clamp(transform.position.y, 25, 75),
                Mathf.Clamp(transform.position.z, -45, 10));
        }
    }
}
