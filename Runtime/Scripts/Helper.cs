using UnityEngine;

public class Helper : MonoBehaviour
{
    public static bool AnyTriggerDown()
    {
        return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) ||
               OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
    }

    public static bool AnyTriggerUp()
    {
        return OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) ||
               OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
    }
        
    public static Color GetColorForIndex(int index)
    {
        switch (index)
        {
            case 0:
                return Color.blue;
            case 1:
                return Color.red;
            case 2:
                return Color.green;
            case 3:
                return Color.yellow;
            case 4:
                return Color.black;
            default:
                return Color.white;
        }
    }
}