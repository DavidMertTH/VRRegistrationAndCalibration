using UnityEngine;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
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
                    break;
                case 1:
                    return Color.red;
                    break;
                case 2:
                    return Color.green;
                    break;
                case 3:
                    return Color.purple;
                    break;
                case 4:
                    return Color.black;
                    break;
                default:
                    return Color.white;
                    break;
            }
        }
    }
}
