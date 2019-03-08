using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target = null;
    public float speed = 1.0f;

    float height;
    Vector3 offset;

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
}
