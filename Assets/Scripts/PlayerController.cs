using UnityEngine;
using System.Text.RegularExpressions;

public class PlayerController : PathfindingAgent
{
    [SerializeField] public GameObject fireball;

    private bool firemode;
    private float speed = 10.0f;
    private int hitPoints = 10;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            firemode = !firemode;
            Debug.Log("Firemode changed");
            GetComponent<Animator>().SetBool("attacking", firemode);
        }
        // Move to the clicked spot on the level
        if (Input.GetKeyDown(KeyCode.Mouse0) && !firemode)
        {
            GetComponent<Animator>().SetBool("attacking", false);
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
        else if (Input.GetMouseButtonUp(0) && firemode)
        {
            FireBall();
            
            firemode = !firemode;
        }
        // For testing damage
        else if (Input.GetKeyDown(KeyCode.A))
        {
            TakeDamage(5);
        }
    }

    private void FireBall()
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
            Vector3 pos = new Vector3(hit.point.x, 1.5f, hit.point.z);
            Vector3 currentPos = new Vector3(transform.position.x, 1.5f, transform.position.z);
            GameObject fball = Object.Instantiate(fireball) as GameObject;
            fball.transform.position = currentPos;

            var direction = (pos - currentPos).normalized;
            fball.transform.position += direction;

            fball.GetComponent<Rigidbody>().velocity = direction * speed;
            if (Regex.IsMatch(this.tag, "blue"))
            {
                fball.GetComponent<Fireball>().target = "red";
            }
            else
            {
                fball.GetComponent<Fireball>().target = "blue";
            }
        }
    }

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;
        GetComponent<Animator>().SetFloat("health", hitPoints);
        if (hitPoints <= 0)
        {
            /* Respawn */
            //Destroy(this.gameObject);
        }
    }
}
