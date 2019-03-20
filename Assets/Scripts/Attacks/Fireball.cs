using UnityEngine;
using System.Text.RegularExpressions;

public class Fireball : MonoBehaviour
{
    private Rigidbody body;
    public string target;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (Regex.IsMatch(target, "red") && Regex.IsMatch(col.gameObject.tag, "red"))
        {
            Debug.Log("Hit a red enemy");
            Destroy(this.gameObject);
        }
        else if (Regex.IsMatch(target, "blue") && Regex.IsMatch(col.gameObject.tag, "blue"))
        {
            Debug.Log("Hit a blue enemy");
            Destroy(this.gameObject);
        }
        else if (col.gameObject.layer == LayerMask.NameToLayer("Level"))
        {
            Destroy(this.gameObject);
        }
    }
}
