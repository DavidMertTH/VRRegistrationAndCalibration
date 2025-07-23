using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
    public class SpawnBox : MonoBehaviour
    {
        public GameObject prefab;
        public GameObject bottlePrefab;
        public OVRPassthroughLayer passthroughLayer;
        public GameObject leftController;
        public GameObject rightController;
        private GameObject[] _markerPoints;
        private bool _isSelecting = false;
        private MarkerPoint _currentMarker;
        private Vector3 _originalBoxRotation;
        private GameObject _demoObject;
        private GameObject _demoBottle;
        private GameObject _anchorBottle;
        private bool _roomIsDeactivated;
        private GameObject _roomData;
        private bool _active;

        private void Start()
        {
            _demoObject = Instantiate(prefab);
            _demoBottle = Instantiate(bottlePrefab);
            EffectMesh a;
        }

        void Update()
        {
            if (!_roomIsDeactivated) DeactivateRoom();
            /*Vector3 positionR = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch) +
                            OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward * 0.1f;

        Vector3 positionL = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch) +
                            OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch) * Vector3.forward * 0.05f;

        */

            Vector3 positionL = leftController.transform.position +
                                leftController.transform.rotation * Vector3.forward * 0.1f;
            ;
            Vector3 positionR = rightController.transform.position +
                                rightController.transform.rotation * Vector3.forward * 0.05f;

            _demoObject.transform.position = positionR;
            _demoBottle.transform.position = positionL;

            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
            {
                _isSelecting = true;
                CreateMarkerPoints(positionR);
            }

            if (_isSelecting)
            {
                RotateMarkers();
            }

            if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) && _isSelecting)
            {
                _isSelecting = false;
                foreach (var marker in _markerPoints)
                {
                    marker.AddComponent<OVRSpatialAnchor>();
                }
            }

            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
            {
                CreateBottle(positionL);
            }

            if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.LTouch))
            {
                passthroughLayer.textureOpacity = Mathf.Abs(1 - passthroughLayer.textureOpacity);
            }

            if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.LTouch))
            {
                _roomData.SetActive(!_roomData.activeSelf);
            }

            if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.RTouch))
            {
                DeactivateAllRenderers(_active);
                _active = !_active;
            }
        }

        private void DeactivateAllRenderers(bool active)
        {
            if (_demoBottle != null)
            {
                _anchorBottle.GetComponent<MeshRenderer>().enabled = active;
            }

            if (_markerPoints != null && _markerPoints.Length > 0)
            {
                foreach (var marker in _markerPoints)
                {
                    marker.SetActive(active);
                }
            }
        }

        private void DeactivateRoom()
        {
            MRUKRoom found = FindAnyObjectByType<MRUKRoom>();
            if (found == null) return;
            _roomData = found.gameObject;
            DisableVolumeMeshes(found.gameObject);

            _roomIsDeactivated = true;
        }

        public void DisableVolumeMeshes(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                // Prüfe auf MeshRenderer
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (mat != null && mat.name.StartsWith("Volume"))
                        {
                            renderer.enabled = false;
                            Debug.Log("MeshRenderer deaktiviert: " + child.name);
                            break; // Falls eins der Materialien passt, reicht das
                        }
                    }
                }

                // Rekursiver Aufruf für Kinder
                DisableVolumeMeshes(child.gameObject);
            }
        }

        private void CreateBottle(Vector3 position)
        {
            if (_anchorBottle != null)
            {
                Destroy(_anchorBottle);
            }

            _anchorBottle = Instantiate(bottlePrefab);
            _anchorBottle.transform.position = position;
            _anchorBottle.AddComponent<OVRSpatialAnchor>();
        }

        private void RotateMarkers()
        {
            if (_markerPoints == null || _markerPoints.Length == 0) return;
            Vector3 toMarker =
                Vector3.ProjectOnPlane(
                    _markerPoints[0].transform.position - rightController.transform.position,
                    Vector3.up);
            float angle = Vector3.SignedAngle(Vector3.forward, toMarker, Vector3.up);
            _markerPoints[0].transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        }

        private void CreateMarkerPoints(Vector3 position)
        {
            if (_markerPoints != null)
            {
                for (int i = 0; i < _markerPoints.Length; i++)
                {
                    if (_markerPoints[i] == null) continue;
                    Destroy(_markerPoints[i].gameObject);
                    _markerPoints[i] = null;
                }
            }

            _markerPoints = new GameObject[8];
            GameObject go = Instantiate(prefab, position,
                Quaternion.identity);
            _markerPoints[0] = go;
            _markerPoints[1] = Instantiate(prefab, position + (Vector3.right * 0.215f), Quaternion.identity);
            _markerPoints[2] = Instantiate(prefab, position + (Vector3.forward * 0.215f), Quaternion.identity);
            _markerPoints[3] = Instantiate(prefab, position + (Vector3.right * 0.215f) + (Vector3.forward * 0.215f),
                Quaternion.identity);

            for (int i = 4; i < 8; i++)
            {
                _markerPoints[i] = Instantiate(prefab, _markerPoints[i - 4].transform.position + Vector3.down * 0.12f,
                    Quaternion.identity);
            }

            for (int i = 1; i < _markerPoints.Length; i++)
            {
                _markerPoints[i].transform.SetParent(go.transform);
            }

            _markerPoints[0].GetComponent<MarkerPoint>().markerLeft = _markerPoints[1];
            _markerPoints[0].GetComponent<MarkerPoint>().markerRight = _markerPoints[2];

            _markerPoints[1].GetComponent<MarkerPoint>().markerLeft = _markerPoints[3];
            _markerPoints[1].GetComponent<MarkerPoint>().markerRight = _markerPoints[0];

            _markerPoints[2].GetComponent<MarkerPoint>().markerLeft = _markerPoints[3];
            _markerPoints[2].GetComponent<MarkerPoint>().markerRight = _markerPoints[0];

            _markerPoints[3].GetComponent<MarkerPoint>().markerLeft = _markerPoints[1];
            _markerPoints[3].GetComponent<MarkerPoint>().markerRight = _markerPoints[2];

            _markerPoints[4].GetComponent<MarkerPoint>().markerLeft = _markerPoints[0];
            _markerPoints[4].GetComponent<MarkerPoint>().markerRight = _markerPoints[6];

            _markerPoints[5].GetComponent<MarkerPoint>().markerLeft = _markerPoints[1];
            _markerPoints[5].GetComponent<MarkerPoint>().markerRight = _markerPoints[4];

            _markerPoints[6].GetComponent<MarkerPoint>().markerLeft = _markerPoints[2];
            _markerPoints[6].GetComponent<MarkerPoint>().markerRight = _markerPoints[7];

            _markerPoints[7].GetComponent<MarkerPoint>().markerLeft = _markerPoints[3];
            _markerPoints[7].GetComponent<MarkerPoint>().markerRight = _markerPoints[5];
        }
    }
}