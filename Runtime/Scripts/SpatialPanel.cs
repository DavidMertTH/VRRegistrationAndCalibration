using UnityEngine;
using UnityEngine.UI;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
    public class SpatialPanel : MonoBehaviour
    {
        [HideInInspector]public GameObject focusCamera;
        [HideInInspector]public GameObject anchorObject;
        public GameObject colorPicker;
        public GameObject calibrationInfo;

        public Image colorImage;
        public GameObject ConfirmationImage;
    
        [HideInInspector]public RegistrationVR registrationVR;

        private GameObject _activePanel;
        private Color _markerColor;

        private void Start()
        {
            if (Camera.main != null) focusCamera = Camera.main.gameObject;
        
        }

        void Update()
        {
            if (focusCamera == null || anchorObject == null) return;

            Vector3 toCamera = focusCamera.transform.position - anchorObject.transform.position;
            Vector3 toPanel = Vector3.Cross(toCamera, Vector3.up);
            toPanel.Normalize();
            transform.position = anchorObject.transform.position + toPanel * (0.1f);
            transform.position += Vector3.up * 0.1f;
            transform.LookAt(focusCamera.transform);
            UpdateState();
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
                    SetActive(ConfirmationImage);
                    break;
            }
        }

        private void SetActive(GameObject go)
        {
            if (_activePanel == go) return;

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
}