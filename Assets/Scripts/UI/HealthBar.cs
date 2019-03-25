using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image image = null;

    internal UnitController unit;
    internal Slider slider;

    private static float updateSpeed = 25;
    private static Vector3 placementOffset = new Vector3(0, 2, 1);
    private Vector3 offset = placementOffset;

    void Awake()
    {
        slider = GetComponentInChildren<Slider>();
    }

    void Update()
    {
        transform.position = unit.transform.position + offset;
        slider.value = Mathf.Lerp(slider.value, unit.hitPoints, updateSpeed * Time.deltaTime);
    }

    public void ConnectTo(UnitController unit)
    {
        this.unit = unit;
        transform.localScale *= unit.transform.lossyScale.x;
        image.color = (unit.team == "blue")? 
            GameController.instance.blueColor * 1.5f : GameController.instance.redColor * 1.5f;
        offset *= unit.transform.lossyScale.y;
        offset += transform.position - unit.transform.position;
        slider.maxValue = unit.totalHitPoints;
        slider.value = unit.hitPoints;
    }
}
