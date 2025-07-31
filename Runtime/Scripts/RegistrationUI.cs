using UnityEngine;
using UnityEngine.UI;

public class SpatialPanel : MonoBehaviour
{
    public RegistrationVR registrationVR;
    public Image colorImage;

    [SerializeField] private GameObject confirmationImage;
    [SerializeField] private GameObject colorPicker;
    [SerializeField] private GameObject calibrationInfo;
    [SerializeField] private GameObject background;

    [HideInInspector] private GameObject _focusCamera;
    [HideInInspector] public GameObject anchorObject;

    private GameObject _activePanel;
    private Color _markerColor;

    private void Awake()
    {
        confirmationImage.SetActive(false);
        colorPicker.SetActive(false);
        calibrationInfo.SetActive(false);
        registrationVR.StateChanged += UpdateState;

    }

    private void Start()
    {
        if (Camera.main != null) _focusCamera = Camera.main.gameObject;
        anchorObject = registrationVR.controllerInUse;
    }

    void Update()
    {
        if (_focusCamera == null || anchorObject == null) return;

        AdjustPanelPosition();
        SetColor(Helper.GetColorForIndex(registrationVR.markers.Count));
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
        switch (registrationVR.currentState)
        {
            case (RegistrationVR.State.Calibration):
                SetActive(calibrationInfo);
                break;
            case (RegistrationVR.State.MarkerSetup):
                SetActive(colorPicker);
                break;
            case (RegistrationVR.State.Confirmation):
                SetActive(confirmationImage);
                break;
            case (RegistrationVR.State.Inactive):
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
        _markerColor = color;
        colorImage.color = color;
    }
}