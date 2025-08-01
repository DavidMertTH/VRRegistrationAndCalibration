using System;
using UnityEngine;

public class RegistrationVrController : MonoBehaviour
{
    public Registration registration;
    
    [SerializeField] private Handedness controllerSelection;
    [SerializeField] private bool useTip;

    [HideInInspector] public GameObject controllerInUse;
    
    private Calibrator _calibrator;
    private Vector3 _tipPosition;
    private GameObject _demoObject;

    public enum Handedness
    {
        RightHanded,
        LeftHanded
    }

    private void Awake()
    {
        _calibrator = gameObject.AddComponent<Calibrator>();
        SetupController();
        _demoObject = Helper.CreateSmallSphere();
        _demoObject.name = "Demo Object";
        _demoObject.transform.SetParent(transform);
        registration.StateChanged += OnStateChanged;
        
    }

    private void Start()
    {
        if(useTip)registration.SetState(Registration.State.Calibration);
        else registration.SetState(Registration.State.MarkerSetup);
      
    }

    private void OnStateChanged()
    {
        switch (registration.currentState)
        {
            case Registration.State.Calibration:
                _demoObject.SetActive(false);
                break;
        }
    }
    private void OnEnable()
    {
        SetupController();
    }

    private void SetupController()
    {
        controllerInUse = SearchForController(controllerSelection);
        _calibrator.toCalibrate = controllerInUse;
    }

    private void Update()
    {
        if (registration.currentState == Registration.State.Inactive) return;

        switch (registration.currentState)
        {
            case Registration.State.Calibration:
                CalibrationActions();
                break;
            case Registration.State.MarkerSetup:
                MarkerStateActions();
                break;
            case Registration.State.Confirmation:
                ConfirmationStateActions();
                break;
        }
    }

    private void CalibrationActions()
    {
        UpdateTipPosition();
        UpdateDemoObject();

        if (CommitButtonPressed()) registration.SetState(Registration.State.MarkerSetup);
        if (AnyTriggerDown())
        {
            _demoObject.SetActive(true);
            _calibrator.StartRecording();
        }
        if (AnyTriggerUp()) _calibrator.StopRecording();
    }

    private void MarkerStateActions()
    {
        UpdateTipPosition();
        UpdateDemoObject();

        LeftHandMarkerInteractions();
        RightHandMarkerInteractions();
    }

    private void ConfirmationStateActions()
    {
        if (CommitButtonPressed())
        {
            registration.SaveRegistration();
            registration.SetState(Registration.State.Inactive);
        }

        if (CancelButtonPressed())
        {
            registration.SetState(Registration.State.MarkerSetup);
        }
    }

    private void UpdateTipPosition()
    {
        if (useTip)
            _tipPosition = _calibrator.GetCalibratedCurrentPosition();
        else if (controllerInUse != null)
            _tipPosition = controllerInUse.transform.position + controllerInUse.transform.forward * 0.06f;
        else
            Debug.LogWarning("No Controller in Use!");
    }

    private void UpdateDemoObject()
    {
        if (_demoObject == null) return;

        _demoObject.transform.position = _tipPosition;
        Helper.SetColor(_demoObject, Helper.GetColorForIndex(registration.markers.Count));
    }

    private void RightHandMarkerInteractions()
    {
        if (AnyTriggerDown()) registration.AddMarker(_tipPosition);
        if (CancelButtonPressed()) registration.ResetEverything();
    }

    private GameObject SearchForController(Handedness handedness)
    {
        string controllerName =
            handedness == Handedness.RightHanded ? "RightControllerAnchor" : "LeftControllerAnchor";
        GameObject controllerToUse = GameObject.Find(controllerName);
        return controllerToUse;
    }

    private void LeftHandMarkerInteractions()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            registration.RestoreLastPlacedAnchor();
        }
    }

    private static bool CommitButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
    }

    private static bool CancelButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
    }

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
}