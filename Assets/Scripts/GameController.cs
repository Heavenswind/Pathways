using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;

    [SerializeField] internal GameObject blueBase = null;
    [SerializeField] internal GameObject redBase = null;
    [SerializeField] internal Transform blueTower = null;
    [SerializeField] internal Transform redTower = null;
    [SerializeField] private float scoreToWin = 100;
    [SerializeField] private float scoreRatePerPoint = 0.01f;
    internal Color blueColor;
    internal Color redColor;

    private CapturePoint[] capturePoints;
    private float blueScore = 0;
    private float redScore = 0;
    
    void Awake()
    {
        instance = this;
        blueColor = blueBase.GetComponent<Renderer>().material.color;
        redColor = redBase.GetComponent<Renderer>().material.color;
        capturePoints = GameObject.FindObjectsOfType<CapturePoint>();
    }

    void Update()
    {
        foreach (CapturePoint capturePoint in capturePoints)
        {
            var pointScore = (capturePoint.score - CapturePoint.medianScore)
                / (CapturePoint.maxScore - CapturePoint.medianScore);
            if (pointScore > 0)
            {
                blueScore += pointScore * scoreRatePerPoint;
            }
            else
            {
                redScore += -pointScore * scoreRatePerPoint;   
            }
        }
        HUD.instance.SetProgress(blueScore / scoreToWin, redScore / scoreToWin);
    }
}
