using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegistrationVR : MonoBehaviour
{
    public Registration Registration;
    public GameObject regiTargetPrefab;
    public Algorithm algorithmToUse;
    public event Action StateChanged;
    public bool useTip;
    public string numUuidsKey = "numUuids";

    [SerializeField] private Handedness controllerSelection;

    [HideInInspector] public RegiTarget regiTarget;
    [HideInInspector] public State currentState;
    [HideInInspector] public List<GameObject> markers;
    [HideInInspector] public GameObject controllerInUse;

    private bool _isSetup;
    private AnchorLoaderManager _anchorLoaderManager;
    private Vector3 _tipPosition;
    private Calibrator _calibrator;
    private GameObject _demoObject;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    public enum State
    {
        Calibration,
        MarkerSetup,
        Confirmation,
        Inactive,
    }

    public enum Algorithm
    {
        Kabsch,
        FixedYAxis
    }

    public enum Handedness
    {
        RightHanded,
        LeftHanded
    }

    private void Awake()
    {
        controllerInUse = SearchForController(controllerSelection);
        Registration = new Registration();
        markers = new List<GameObject>();
        GameObject toRegister = regiTargetPrefab;
        regiTarget = toRegister.GetComponent<RegiTarget>();
        regiTarget.SetVisible(false);
        _demoObject = CreateSmallSphere();
        _demoObject.name = "Demo Object";
        _demoObject.transform.SetParent(transform);

        _anchorLoaderManager = gameObject.AddComponent<AnchorLoaderManager>();
        _anchorLoaderManager.numUuidsPlayerPref = numUuidsKey;

        _calibrator = gameObject.AddComponent<Calibrator>();
        _calibrator.registrationVR = this;
    }

    private void Start()
    {
        if (useTip) SetState(State.Calibration);
        else SetState(State.MarkerSetup);
    }


    private void OnEnable()
    {
        controllerInUse = SearchForController(controllerSelection);
    }

    private void Update()
    {
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

    public void SetState(State nextState)
    {
        currentState = nextState;
        StateChanged?.Invoke();
        _demoObject.SetActive(currentState == State.MarkerSetup);
    }

    public void AddMarker(Vector3 position)
    {
        if (markers.Count >= regiTarget.amountControlPoints) return;

        GameObject go = CreateSmallSphere();
        go.transform.position = position;
        go.AddComponent<OVRSpatialAnchor>();
        SetColor(go);
        markers.Add(go);
        go.transform.SetParent(transform);

        if (ReachedMaxMarkerAmount())
        {
            Align(regiTarget);
            SetState(State.Confirmation);
            regiTarget.gameObject.AddComponent<OVRSpatialAnchor>();
        }
    }

    private GameObject CreateSmallSphere()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(0, 0, 0);
        sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        return sphere;
    }

    private GameObject SearchForController(Handedness handedness)
    {
        string controllerName =
            handedness == Handedness.RightHanded ? "RightControllerAnchor" : "LeftControllerAnchor";
        GameObject controllerToUse = GameObject.Find(controllerName);
        return controllerToUse;
    }

    private void CalibrationActions()
    {
        _tipPosition = _calibrator.GetCalibratedCurrentPosition();
        _demoObject.transform.position = _tipPosition;
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            SetState(State.MarkerSetup);
        }

        if (Helper.AnyTriggerDown()) _calibrator.StartRecording();
        if (Helper.AnyTriggerUp()) _calibrator.StopRecording();
    }

    private void MarkerStateActions()
    {
        if (useTip) _tipPosition = _calibrator.GetCalibratedCurrentPosition();
        else _tipPosition = controllerInUse.transform.position + controllerInUse.transform.forward * 0.06f;
        _demoObject.transform.position = _tipPosition;
        SetColor(_demoObject);

        LeftHandMarkerInteractions();
        RightHandMarkerInteractions();
    }

    private void RightHandMarkerInteractions()
    {
        if (Helper.AnyTriggerDown()) AddMarker(_tipPosition);
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            ResetTarget();
            ResetMarker();
        }
    }

    private void ResetTarget()
    {
        regiTarget.SetVisible(false);
        regiTarget.transform.position = Vector3.zero;
        regiTarget.transform.rotation = Quaternion.identity;
    }

    private void ResetMarker()
    {
        markers.ForEach(Destroy);
        markers.Clear();
    }

    private async void LeftHandMarkerInteractions()
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
        SetState(State.Confirmation);
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
            Debug.Log("Save ANCHOR");
            await _anchorLoaderManager.DeleteAllAnchors();
            regiTarget.gameObject.AddComponent<OVRSpatialAnchor>();
            StartCoroutine(SaveAnchorsDelayed());
        }

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            OVRSpatialAnchor anchor = regiTarget.GetComponent<OVRSpatialAnchor>();
            if (anchor != null) Destroy(anchor);

            DeleteAllMarker();
            SetState(State.MarkerSetup);
            regiTarget.SetVisible(false);
        }
    }

    private void SetColor(GameObject go)
    {
        var render = go.GetComponent<Renderer>();
        if (render == null) return;

        var propertyBlock = new MaterialPropertyBlock();
        render.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColor, Helper.GetColorForIndex(markers.Count));
        render.SetPropertyBlock(propertyBlock);
    }


    private void Align(RegiTarget target)
    {
        if (markers == null || markers.Count == 0 || target == null) return;
        if (algorithmToUse == Algorithm.Kabsch)
            Registration.AlignMeshKabsch(markers.Select(marker => marker.transform.position).ToList(), target);
        else Registration.AlignMesh(markers.Select(marker => marker.transform.position).ToList(), target);
    }

    private void DeleteAllMarker()
    {
        if (markers == null || markers.Count == 0) return;
        regiTarget.SetVisible(false);
        markers.ForEach(Destroy);
        markers.Clear();
    }

    private bool ReachedMaxMarkerAmount()
    {
        return markers.Count >= regiTarget.amountControlPoints;
    }
}