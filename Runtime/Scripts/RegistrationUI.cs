using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(RegistrationVrController))]
public class SpatialPanel : MonoBehaviour
{

    public Image colorImage;

    [SerializeField] private GameObject confirmationImage;
    [SerializeField] private GameObject colorPicker;
    [SerializeField] private GameObject calibrationInfo;
    [SerializeField] private GameObject background;

    [HideInInspector] public GameObject anchorObject;

    private GameObject _activePanel;
    private GameObject _focusCamera;
    private RegistrationVrController _vrRegistration;

    private void Awake()
    {
        _vrRegistration = GetComponent<RegistrationVrController>();
        confirmationImage.SetActive(false);
        colorPicker.SetActive(false);
        calibrationInfo.SetActive(false);
        _vrRegistration.registration.StateChanged += UpdateState;
    }

    private void Start()
    {
        if (Camera.main != null) _focusCamera = Camera.main.gameObject;
        anchorObject = _vrRegistration.controllerInUse;
    }

    private void Update()
    {
        if (_focusCamera == null || anchorObject == null || _vrRegistration == null) return;
        if (_vrRegistration.registration.currentState == Registration.State.Inactive) return;
        AdjustPanelPosition();
        SetColor(Helper.GetColorForIndex(_vrRegistration.registration.markers.Count));
    }

    private void AdjustPanelPosition()
    {
        Vector3 toCamera = _focusCamera.transform.position - anchorObject.transform.position;
        Vector3 toPanel = Vector3.Cross(toCamera, Vector3.up);
        toPanel.Normalize();
        transform.position = anchorObject.transform.position + toPanel * (0.1f);
        transform.position += Vector3.up * 0.1f;
        transform.LookAt(_focusCamera.transform);
    }

    public void UpdateState()
    {
        switch (_vrRegistration.registration.currentState)
        {
            case (Registration.State.Calibration):
                SetActive(calibrationInfo);
                break;
            case (Registration.State.MarkerSetup):
                SetActive(colorPicker);
                break;
            case (Registration.State.Confirmation):
                SetActive(confirmationImage);
                break;
            case (Registration.State.Inactive):
                DeactivateCurrent();
                break;
        }
    }

    private void DeactivateCurrent()
    {
        if (_activePanel == null) return;

        if (_activePanel != null) _activePanel.SetActive(false);
        background.SetActive(false);
    }

    private void SetActive(GameObject go)
    {
        background.SetActive(true);

        if (_activePanel == go)
        {
            _activePanel.SetActive(true);
            return;
        }

        if (_activePanel != null) _activePanel.SetActive(false);
        _activePanel = go;
        _activePanel.SetActive(true);
    }

    public void SetColor(Color color)
    {
        colorImage.color = color;
    }
}