using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Registration : MonoBehaviour
{
    public RegiTarget regiTarget;
    public RegistrationPlaneProjection RegistrationPlaneProjection;
    public Algorithm algorithmToUse;
    public event Action StateChanged;
    public string numUuidsKey = "demoTargetUuidKey";

    [HideInInspector] public State currentState;
    [HideInInspector] public List<GameObject> markers;

    private AnchorLoaderManager _anchorLoaderManager;
    private Vector3 _tipPosition;
    private Calibrator _calibrator;
    private Kabsch.Kabsch _kabsch = new();

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

    private void Awake()
    {
        RegistrationPlaneProjection = new RegistrationPlaneProjection();
        markers = new List<GameObject>();
        regiTarget.SetVisible(false);

        _anchorLoaderManager = gameObject.AddComponent<AnchorLoaderManager>();
        _anchorLoaderManager.numUuidsPlayerPref = numUuidsKey;
    }

    public void SetState(State nextState)
    {
        currentState = nextState;
        StateChanged?.Invoke();

        switch (nextState)
        {
            case State.MarkerSetup:
                ResetEverything();
                OVRSpatialAnchor anchor = regiTarget.GetComponent<OVRSpatialAnchor>();
                if (anchor != null) Destroy(anchor);
                regiTarget.SetVisible(false);
                break;

            case State.Inactive:
                ResetMarker();
                break;
        }
    }

    public void AddMarker(Vector3 position)
    {
        if (markers.Count >= regiTarget.amountControlPoints) return;

        GameObject go = Helper.CreateSmallSphere();
        go.transform.position = position;
        go.AddComponent<OVRSpatialAnchor>();
        Helper.SetColor(go, Helper.GetColorForIndex(markers.Count));
        markers.Add(go);
        go.transform.SetParent(transform);

        if (ReachedMaxMarkerAmount())
        {
            Align(regiTarget);
            SetState(State.Confirmation);
            regiTarget.gameObject.AddComponent<OVRSpatialAnchor>();
        }
    }

    public void RestoreLastPlacedAnchor()
    {
        LinkPositionFromDevice();
    }

    public void ResetEverything()
    {
        ResetTarget();
        ResetMarker();
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

    private void LinkPositionFromDevice()
    {
        List<Guid> uuids = AnchorStorage.LoadAllAnchorUuids();
        Debug.Log("LOAD ANCHORS: " + uuids.Count);
        _anchorLoaderManager.AnchorLoader.LoadAnchorsByUuid(regiTarget);
        SetState(State.Confirmation);
    }

    private IEnumerator SaveAnchorsDelayed()
    {
        yield return new WaitForSeconds(0.01f);
        _anchorLoaderManager.SaveAnchor(regiTarget.GetComponent<OVRSpatialAnchor>());
    }

    public async void SaveRegistration()
    {
        Debug.Log("Save ANCHOR");
        await _anchorLoaderManager.DeleteAllAnchors();
        regiTarget.gameObject.AddComponent<OVRSpatialAnchor>();
        StartCoroutine(SaveAnchorsDelayed());
    }

    private void Align(RegiTarget target)
    {
        if (markers == null || markers.Count == 0 || target == null) return;
        if (algorithmToUse == Algorithm.Kabsch)
            AlignMeshKabsch(markers.Select(marker => marker.transform.position).ToList(), target);
        else
            RegistrationPlaneProjection.AlignMesh(markers.Select(marker => marker.transform.position).ToList(), target);
    }

    public void AlignMeshKabsch(List<Vector3> selectedPositions, RegiTarget toTransform)
    {
        toTransform.transform.position = Vector3.zero;
        toTransform.transform.rotation = Quaternion.identity;
        _kabsch.ReferencePoints = selectedPositions.ToArray();
        _kabsch.InPoints = toTransform.GetActiveRelativeMarkerPositions();
        _kabsch.TargetObject = toTransform.gameObject;
        _kabsch.SolveKabsch();
        toTransform.SetVisible(true);
    }

    private bool ReachedMaxMarkerAmount()
    {
        return markers.Count >= regiTarget.amountControlPoints;
    }
}