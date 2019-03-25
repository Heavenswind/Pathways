using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD instance = null;

    [SerializeField] private GameObject healthBarPrefab = null;

    void Awake()
    {
        instance = this;
    }

    public HealthBar CreateHealthBar(UnitController unit)
    {
        var instance = Instantiate(
            healthBarPrefab,
            unit.transform.position,
            healthBarPrefab.transform.rotation);
        var healthBar = instance.GetComponent<HealthBar>();
        healthBar.transform.SetParent(transform);
        healthBar.ConnectTo(unit);
        return healthBar;
    }
}
