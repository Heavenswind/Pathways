using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;

    [SerializeField] internal GameObject blueBase = null;
    [SerializeField] internal GameObject redBase = null;
    internal Color blueColor;
    internal Color redColor;
    
    void Awake()
    {
        instance = this;
        blueColor = blueBase.GetComponent<Renderer>().material.color;
        redColor = redBase.GetComponent<Renderer>().material.color;
    }
}
