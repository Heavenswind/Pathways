using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturePoint : MonoBehaviour
{

    public float score = 5.0f;
    public float max_score = 10f;
    public float min_score = 0f;

    public int redTeam,blueTeam,redNPC,blueNPC = 0;
    float multiplier = 0f;

    public bool redCapture, blueCapture = false;

    Renderer rend;

    List<GameObject> inRange = new List<GameObject>();

    enum CaptureState {Red, Blue, Neutral};
    CaptureState status;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        
        status = CaptureState.Neutral;
    }

    // Update is called once per frame
    void Update()
    {
        //Update visual affinity of capture point. Point will gradually change to the teams color as it is closer to "capturing it"
        if (score >= 5f)
        {
            //Color adjusts between black (neutral) and blue
            rend.material.color = Color.Lerp(Color.black, Color.blue, (score - (max_score / 2)) / (max_score/2));
        } else {
            //Color adjusts between black (netural) and red
            rend.material.color = Color.Lerp(Color.red, Color.black, score / (max_score / 2));
        }

        //if more red team players are on point, increase affinity for red team
        if (redTeam > blueTeam)
        {
            if (score > min_score)
            {
                score = Mathf.Max( score - 0.25f * multiplier * Time.deltaTime, min_score);
            }
            
        }

        //if more blue team players are on point, increase affinity for blue team
        if (blueTeam > redTeam)
        {
            if (score < max_score)
            {
                score = Mathf.Min(score + 0.25f * multiplier * Time.deltaTime, max_score);
            }
        }

        //update point
        updatePoint();
    }

    //updates point to determine if it is captured or not. If captured should add to overal score (control for this should be on the main camera and mutliplied by towers captured)
    void updatePoint()
    {
        if (score == min_score)
        {
            redCapture = true;
            blueCapture = false;
            status = CaptureState.Red;
        }

        if (score == max_score)
        {
            redCapture = false;
            blueCapture = true;
            status = CaptureState.Blue;
        }
        else
        {
            redCapture = false;
            blueCapture = false;
            status = CaptureState.Neutral;
        }
    }


    //Multiplier for # points for capturing a point. If one team has more players on the point than the other they can have a 1.5 or 2x multiplier (1 more player/ 2 more players)
    void updateMultiplier()
    {
        multiplier = (Mathf.Abs(redTeam - blueTeam)/2 + 1) + Mathf.Min(Mathf.Abs(redNPC - blueNPC)/3,1);
    }

    void OnTriggerEnter(Collider collider)
    {
        GameObject temp = collider.gameObject;

        //Add all Players/NPC to a list to keep track of all units on capture point
        if (temp != null)
        {
            //Adding object to lsit
            inRange.Add(temp);

            if (temp.gameObject.tag == "redPlayer")
            {
                redTeam += 1;
                //update multipler
                updateMultiplier();
            }
            if (temp.gameObject.tag == "bluePlayer")
            {
                Debug.Log("BluePlayer in");
                blueTeam += 1;
                Debug.Log(blueTeam);
                //update multipler
                updateMultiplier();
            }
            if (temp.gameObject.tag == "redNPC")
            {
                redNPC += 1;
            }
            if (temp.gameObject.tag == "blueNPC")
            {
                blueNPC += 1;
            }

        }
    }

    void OnTriggerExit(Collider collider)
    {
        GameObject temp = collider.gameObject;

        if (temp != null)
        {
            inRange.Remove(temp);

            if (temp.gameObject.tag == "redPlayer")
            {
                redTeam -= 1;
                //update multipler
                updateMultiplier();
            }
            if (temp.gameObject.tag == "bluePlayer")
            {
                
                blueTeam -= 1;
                //update multipler
                updateMultiplier();
            }
            if (temp.gameObject.tag == "redNPC")
            {
                redNPC -= 1;
            }
            if (temp.gameObject.tag == "blueNPC")
            {
                blueNPC -= 1;
            }

        }
    }

    //checks to see if any objects were destroyed (null). Any destroyed unit should call this.
    public void listUpdate()
    {

        //temp ints to compare 
        int rt = 0;
        int bt = 0;
        int rn = 0;
        int bn = 0;

        //temp list to fill with all !null objects
        List<GameObject> temp = new List<GameObject>();


        //if list is not empty
        if (inRange.Count != 0)
        {
            //iterate through list
            for (int i = 0; i < inRange.Count; i++)
            {
                //if object exits, add to proper variable
                if (inRange[i].gameObject != null)
                {
                    switch (inRange[i].gameObject.tag)
                    {
                        case "redPlayer": rt += 1;
                            break;
                        case "bluePlayer":
                            bt += 1;
                            break;
                        case "redNPC":
                            rn += 1;
                            break;
                        case "blueNPC":
                            bn += 1;
                            break;

                    }

                    // add to temp list
                    temp.Add(inRange[i].gameObject);
                }
                
            }
            
            //comparision of list values vs capture point values
            if (rt < redTeam) { redTeam = rt; }
            if (bt < blueTeam) { blueTeam = rt; }
            if (rn < redNPC) { redNPC = rt; }
            if (bn < blueNPC) { blueNPC = rt; }

        }
        //clears list and update with new one.
        inRange.Clear();
        inRange.AddRange(temp);
    }
}
