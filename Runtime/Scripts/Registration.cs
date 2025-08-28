using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages registration workflow, including marker setup, state changes, and alignment algorithms for a target object.
/// </summary>
/// <remarks>
/// David Mertens, TH Koeln.
/// </remarks>
/// 
public class Registration : MonoBehaviour
{
    public RegiTarget regiTarget;
    public RegistrationPlaneProjection RegistrationPlaneProjection;
    public Algorithm algorithmToUse;
    public event Action StateChanged;
    public string numUuidsKey = "demoTargetUuidKey";
    public bool onlyCorrectYAxis;

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
        ProjectionPlaneMapping
    }

    private void Awake()
    {
        RegistrationPlaneProjection = new RegistrationPlaneProjection();
        markers = new List<GameObject>();
        regiTarget.SetVisible(false);

        _anchorLoaderManager = gameObject.AddComponent<AnchorLoaderManager>();
        _anchorLoaderManager.numUuidsPlayerPref = numUuidsKey;
    }

    /// <summary>
    /// Sets the current registration state and triggers state change events.
    /// </summary>
    /// <param name="nextState">The new state to set.</param>
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

    /// <summary>
    /// Adds a marker at the specified position and aligns the target if the maximum number of markers is reached.
    /// </summary>
    /// <param name="position">World position for the marker.</param>
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

    /// <summary>
    /// Restores the last placed anchor using device anchor data.
    /// </summary>
    public void RestoreLastPlacedAnchor()
    {
        LinkPositionFromDevice();
    }

    /// <summary>
    /// Resets the registration target and all placed markers.
    /// </summary>
    public void ResetEverything()
    {
        ResetTarget();
        ResetMarker();
    }
    
    /// <summary>
    /// Saves the current registration data asynchronously.
    /// </summary>
    public async void SaveRegistration()
    {
        Debug.Log("Save ANCHOR");
        await _anchorLoaderManager.DeleteAllAnchors();
        regiTarget.gameObject.AddComponent<OVRSpatialAnchor>();
        StartCoroutine(SaveAnchorsDelayed());
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


    private void Align(RegiTarget target)
    {
        if (markers == null || markers.Count == 0 || target == null) return;
        
        if (algorithmToUse == Algorithm.Kabsch)
            AlignMeshKabsch(markers.Select(marker => marker.transform.position).ToList(), target);
        
        if (algorithmToUse == Algorithm.ProjectionPlaneMapping)
            RegistrationPlaneProjection.AlignMesh(markers.Select(marker => marker.transform.position).ToList(), target);
        
    }

    private void AlignMeshKabsch(List<Vector3> selectedPositions, RegiTarget toTransform)
    {
        toTransform.transform.position = Vector3.zero;
        toTransform.transform.rotation = Quaternion.identity;
        _kabsch.ReferencePoints = selectedPositions.ToArray();
        _kabsch.InPoints = toTransform.GetActiveRelativeMarkerPositions();
        _kabsch.TargetObject = toTransform.gameObject;
        _kabsch.SolveKabsch();
        toTransform.SetVisible(true);

        if (onlyCorrectYAxis)
        {
            var rotation = toTransform.transform.rotation;
            rotation.x = 0f;
            rotation.z = 0f;
            toTransform.transform.rotation = rotation;
        }
    }


    private bool ReachedMaxMarkerAmount()
    {
        return markers.Count >= regiTarget.amountControlPoints;
    }
}