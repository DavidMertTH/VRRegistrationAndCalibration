using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
    public class SceneCleaner : MonoBehaviour
    {
        private bool _roomIsDeactivated =  false;
        private GameObject _roomData;

        private void Update()
        {
            DeactivateRoom();
        }

        public void DeactivateRoom()
        {
            MRUKRoom[] found = FindObjectsByType<MRUKRoom>(FindObjectsSortMode.None);
            foreach (MRUKRoom room in found)
            {
                if (found == null) return;
                _roomData = room.gameObject;
                _roomData.SetActive(false);
            }
        }

        public void DisableVolumeMeshes(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (mat != null && mat.name.StartsWith("Volume"))
                        {
                            renderer.enabled = false;
                            print("MeshRenderer deaktiviert: " + child.name);
                            break;
                        }
                    }
                }

                DisableVolumeMeshes(child.gameObject);
            }
        }
    }
}