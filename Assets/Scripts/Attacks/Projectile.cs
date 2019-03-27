using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float maxTravelDistance = 1.0f;
    
    internal string target;
    
    private Rigidbody body;

    void Awake()
    {
        body = GetComponentInChildren<Rigidbody>();
    }

    void Start()
    {
        body.velocity = transform.forward * speed;
        var lifespan = maxTravelDistance / speed;
        Destroy(gameObject, lifespan);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Obstacle" || collider.tag == "Ground")
        {
            Destroy(gameObject);
        }
        else if (collider.tag.StartsWith(target))
        {
            var unit = collider.GetComponent<UnitController>();
            if (unit != null)
            {
                unit.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
