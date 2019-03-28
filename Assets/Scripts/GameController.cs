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
    internal bool gameEnded = false;

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
        if (gameEnded) return;
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
        CheckWin();
    }

    private void CheckWin()
    {
        if (blueScore >= scoreToWin) StartCoroutine(EndGame("blue"));
        else if (redScore >= scoreToWin) StartCoroutine(EndGame("red"));
    }

    private IEnumerator EndGame(string winningTeam)
    {
        gameEnded = true;
        SpawnManager.spawning = false;
        foreach (UnitController unit in Object.FindObjectsOfType<UnitController>())
        {
            if (!unit.tag.StartsWith(winningTeam))
            {
                unit.Kill();
            }
            else
            {
                unit.Disable();
            }
        }
        yield return new WaitForSeconds(4);
        HUD.instance.EndGame((winningTeam == "blue")? true : false);
    }
}
