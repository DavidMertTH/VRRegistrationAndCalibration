using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
    public class RegistrationVR : MonoBehaviour
    {
        [HideInInspector] public static RegistrationVR instance;
        [HideInInspector] public RegiTarget regiTarget;
        [HideInInspector] public State currentState;

        public Registration Registration;
        public GameObject regiTargetPrefab;
        public GameObject rightController;
        public GameObject previewPrefab;
        public SpatialPanel panel;

        private bool _isSetup;
        private List<GameObject> _markers;
        private List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();
        private AnchorLoaderManager _anchorLoaderManager;
        private Vector3 _tipPosition;
        private Calibrator _calibrator;

        public enum State
        {
            Calibration,
            MarkerSetup,
            Confirmation,
            Done,
        }

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            Debug.unityLogger.logEnabled = true;
            Registration = new Registration();
            _markers = new List<GameObject>();
            GameObject go = Instantiate(regiTargetPrefab);
            regiTarget = go.GetComponent<RegiTarget>();
            regiTarget.SetVisible(false);
            _anchorLoaderManager = gameObject.AddComponent<AnchorLoaderManager>();
            _anchorLoaderManager.NumUuidsPlayerPref = "numUuids";

            _calibrator = gameObject.AddComponent<Calibrator>();
            _calibrator.toCalibrate = rightController;
            _calibrator.demoPrefab = previewPrefab;
        
            if (panel != null)
            {
                panel.registrationVR = this;
                panel.anchorObject = rightController;
            }
        }

        private void Update()
        {
            panel.SetColor(Registration.GetColorForIndex(_markers.Count));

            switch (currentState)
            {
                case State.Calibration:
                    CalibrationActions();
                    break;
                case State.MarkerSetup:
                    MarkerStateActions();
                    break;
                case State.Confirmation:
                    ConfirmationStateActions();
                    break;
            }
        }

        private void CalibrationActions()
        {
            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                currentState = State.MarkerSetup;
            }

            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                _calibrator.StartRecording();
            }

            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                _calibrator.StopRecording();
            }
        }

        private void MarkerStateActions()
        {
            _tipPosition = _calibrator.GetCalibratedCurrentPosition();

            LeftHandInteractions();
            RightHandInteractions();
        }

        private void RightHandInteractions()
        {
            if (AnyTriggerPressed())
            {
                AddMarker(_tipPosition);
                panel.SetColor(Registration.GetColorForIndex(_markers.Count));

                if (ReachedMaxMarkerAmount())
                {
                    Align(regiTarget);
                    currentState = State.Confirmation;
                    regiTarget.AddComponent<OVRSpatialAnchor>();
                }
            }

            if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            {
                regiTarget.SetVisible(false);
                _markers.ForEach(Destroy);
                _markers.Clear();
            }
        }

        private async void LeftHandInteractions()
        {
            if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
            {
                await _anchorLoaderManager.DeleteAllAnchors();
                regiTarget.SetVisible(false);
            }

            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
            {
                LinkPositionFromDevice();
            }
        }

        private void LinkPositionFromDevice()
        {
            List<Guid> uuids = AnchorStorage.LoadAllAnchorUuids();
            Debug.Log("LOAD ANCHORS: " + uuids.Count);
            _anchorLoaderManager.AnchorLoader.LoadAnchorsByUuid(regiTarget);
            print("LOADED");
            currentState = State.Confirmation;
        }


        private IEnumerator SaveAnchorsDelayed()
        {
            yield return new WaitForSeconds(1f);
            _anchorLoaderManager.SaveAnchor(regiTarget.GetComponent<OVRSpatialAnchor>());
        }

        private async void ConfirmationStateActions()
        {
            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                Debug.Log("Save ANCHORS");
                await _anchorLoaderManager.DeleteAllAnchors();
                regiTarget.AddComponent<OVRSpatialAnchor>();
                StartCoroutine(SaveAnchorsDelayed());
            }

            if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            {
                OVRSpatialAnchor anchor = regiTarget.GetComponent<OVRSpatialAnchor>();
                if (anchor != null)
                {
                    Destroy(anchor);
                }

                DeleteAllMarker();
                currentState = State.MarkerSetup;
                regiTarget.SetVisible(false);
            }
        }

        private void SetColor(GameObject go)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;

            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            int amountMarker = 0;
            if (_markers != null) amountMarker = _markers.Count;
            propertyBlock.SetColor("_BaseColor", Registration.GetColorForIndex(amountMarker));
            renderer.SetPropertyBlock(propertyBlock);
        }

        private void AddMarker(Vector3 position)
        {
            if (_markers.Count >= regiTarget.amountControlPoints) return;

            GameObject go = Instantiate(previewPrefab);
            go.transform.position = position;
            go.AddComponent<OVRSpatialAnchor>();
            SetColor(go);

            _markers.Add(go);
        }

        private void Align(RegiTarget target)
        {
            if (_markers == null || _markers.Count == 0 || target == null) return;
            Registration.AlignMesh(_markers.Select(marker => marker.transform.position).ToList(), target);
        }

        private void DeleteAllMarker()
        {
            if (_markers == null || _markers.Count == 0) return;
            regiTarget.SetVisible(false);
            _markers.ForEach(Destroy);
            _markers.Clear();
        }

        private bool ReachedMaxMarkerAmount()
        {
            return _markers.Count >= regiTarget.amountControlPoints;
        }

        private bool AnyTriggerPressed()
        {
            return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) ||
                   OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        }
    }
}