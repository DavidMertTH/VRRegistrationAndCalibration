using System;
using UnityEngine;
using UnityEngine.Serialization;

public class MarkerPoint : MonoBehaviour
{
    public LineRenderer toFloor;
    public LineRenderer toLeft;
    public LineRenderer toRight;
    public GameObject markerLeft;
    public GameObject markerRight;


    public void Update()
    {
        

        if (markerLeft != null &&markerRight != null)
        {
            //toFloor.SetPosition(0, transform.position);
            //toFloor.SetPosition(1, transform.position + Vector3.down);
            toLeft.SetPosition(0, transform.position);
            toLeft.SetPosition(1, markerLeft.transform.position);
            toRight.SetPosition(0, transform.position);
            toRight.SetPosition(1, markerRight.transform.position);
        }
        else
        {
            //toFloor.SetPosition(0, transform.position);
            //toFloor.SetPosition(1, transform.position);
            toLeft.SetPosition(0, transform.position);
            toLeft.SetPosition(1, transform.position);
            toRight.SetPosition(0,transform.position);
            toRight.SetPosition(1, transform.position);
        }
    }

    
}