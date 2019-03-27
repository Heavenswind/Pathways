using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturePoint : MonoBehaviour
{
    // State of the capture point
    internal enum CaptureState {Red, Blue, Neutral};
    internal CaptureState status = CaptureState.Neutral;
    internal int redTeam, blueTeam, redNPC, blueNPC = 0;
    internal Collider[] units = new Collider[0];
    
    // Score of the capture point
    internal const float minScore = 0f;
    internal const float maxScore = 10f;
    internal const float medianScore = (maxScore - minScore) / 2;
    internal float score = medianScore;
    private const float captureRatePerUnit = 0.25f;
    private const float championMultiplier = 1.2f;

    // Bounds of the capture point
    private new Collider collider;
    private Vector3 size;
    private int unitLayerMask;
    
    // Color of the control point
    private new Renderer renderer;
    private Color neutralColor;
    private Color blueColor;
    private Color redColor;

    void Awake()
    {
        collider = GetComponent<Collider>();
        size = collider.bounds.size;
        unitLayerMask = LayerMask.GetMask("Units");
        renderer = GetComponent<Renderer>();
        neutralColor = renderer.material.color;
    }

    void Start()
    {
        blueColor = GameController.instance.blueColor;
        redColor = GameController.instance.redColor;
    }

    void Update()
    {
        CheckForUnits();
        UpdateScore();
        UpdateColor();
    }

    // Check if the capture point is owned by the given team name.
    public bool IsOwnedByTeam(string team)
    {
        if ((team == "blue" && status == CaptureState.Blue) || (team == "red" && status == CaptureState.Red))
        {
            return true;
        }
        return false;
    }

    // Check the units that are on the capture point.
    // They are counted based on their team (blue/red) and unit type (champion/minion).
    private void CheckForUnits()
    {
        redTeam = blueTeam = redNPC = blueNPC = 0;
        units = Physics.OverlapSphere(
            transform.position,
            size.x / 2,
            unitLayerMask,
            QueryTriggerInteraction.Ignore);
        foreach (Collider collider in units)
        {
            switch (collider.tag)
            {
                case "bluePlayer": ++blueTeam; break;
                case "redPlayer": ++redTeam; break;
                case "blueNPC": ++blueNPC; break;
                case "redNPC": ++redNPC; break;
                default: break;
            }
        }
    }

    // Update the score
    private void UpdateScore()
    {
        score += (blueTeam + blueNPC) * Mathf.Max(1, championMultiplier * blueTeam) * captureRatePerUnit * Time.deltaTime;
        score -= (redTeam + redNPC) * Mathf.Max(1, championMultiplier * redTeam) * captureRatePerUnit * Time.deltaTime;
        score = Mathf.Min(score, maxScore);
        score = Mathf.Max(score, minScore);
        switch (score)
        {
            case maxScore: status = CaptureState.Blue; break;
            case minScore: status = CaptureState.Red; break;
            default: status = CaptureState.Neutral; break;
        }
    }

    // Update visual affinity of capture point. Point will gradually change to the teams color as it is closer to "capturing it".
    private void UpdateColor()
    {
        if (score >= medianScore)
        {
            //Color adjusts between neutral and blue
            renderer.material.color = Color.Lerp(neutralColor, blueColor, (score - (maxScore / 2)) / (maxScore / 2));
        }
        else
        {
            //Color adjusts between netural and red
            renderer.material.color = Color.Lerp(redColor, neutralColor, score / (maxScore / 2));
        }
    }
}
