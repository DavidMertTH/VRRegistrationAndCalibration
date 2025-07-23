using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class RegiTarget : MonoBehaviour
{
    [Range(1, 5)] public int amountControlPoints;
    [SerializeField] public RegiMarker[] markers;
    public Vector3[] relativeMarkerPositions;
    public Guid uuidName;

    private void OnValidate()
    {
        ActivateMarkers();
    }

    public void CreateNewMarker()
    {
    }

    private void Start()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        relativeMarkerPositions = new Vector3[markers.Length];
        for (int i = 0; i < markers.Length; i++)
        {
            relativeMarkerPositions[i] = markers[i].transform.position;
        }
    }

    public List<Vector3> GetMarkerPositions()
    {
        return markers
            .Where(marker => marker.gameObject.activeSelf)
            .Select(marker => marker.transform.position)
            .ToList();
    }

    public void SetVisible(bool visible)
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }

    private bool LinkOldChildMarker()
    {
        List<MarkerPoint> childMarkers = new List<MarkerPoint>();
        gameObject.GetComponentsInChildren<MarkerPoint>(childMarkers);
        if (childMarkers.Count == 0) return false;
        markers.AddRange(childMarkers);
        return true;
    }

    private void ActivateMarkers()
    {
        if (markers == null || markers.Length == 0 || markers[0] == null) InitMarkers();
        for (int i = 0; i < markers.Length; i++)
        {
            markers[i].gameObject.SetActive(i < amountControlPoints);
        }
    }

    private void InitMarkers()
    {
        if (LinkOldChildMarker()) return;
        markers = new RegiMarker[5];
        for (int i = 0; i < markers.Length; i++)
        {
            GameObject go = new GameObject();
            go.name = "Registration Marker " + i;
            go.transform.position = transform.position;
            go.transform.parent = transform;
            markers[i] = go.AddComponent<RegiMarker>();
            markers[i].color = Registration.GetColorForIndex(i);
        }
    }
}