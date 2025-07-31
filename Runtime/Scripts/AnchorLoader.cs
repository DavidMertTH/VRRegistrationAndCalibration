using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public class AnchorLoader
{
    private AnchorLoaderManager _spatialAnchorManager;
    private GameObject _prefab;
    private Action<bool, OVRSpatialAnchor.UnboundAnchor> _onLocalized;
    private HashSet<Guid> _anchorUuids = new();
    private List<OVRSpatialAnchor> _allAnchorsInSystem;
    public RegiTarget LoadedObject;

    public AnchorLoader(AnchorLoaderManager spatialAnchorManager)
    {
        _spatialAnchorManager = spatialAnchorManager;
        _onLocalized = OnLocalized;
    }

    public async void LoadAnchorsByUuid(RegiTarget target)
    {
        LoadedObject = target;

        RemoveExistingAnchor(target);

        var uuids = LoadAnchorUuidsFromPrefs();
        if (uuids.Length == 0) return;

        _spatialAnchorManager.Uuids.AddRange(uuids);

        var unboundAnchors = await LoadUnboundAnchorsAsync(uuids);
        _allAnchorsInSystem = new List<OVRSpatialAnchor>();

        LocalizeAnchors(unboundAnchors);
    }

    private void RemoveExistingAnchor(RegiTarget target)
    {
        var existingAnchor = target.GetComponent<OVRSpatialAnchor>();
        if (existingAnchor != null)
        {
            Object.Destroy(existingAnchor);
        }
    }

    private Guid[] LoadAnchorUuidsFromPrefs()
    {
        var key = _spatialAnchorManager.numUuidsPlayerPref;

        if (!PlayerPrefs.HasKey(key))
            PlayerPrefs.SetInt(key, 0);

        int count = PlayerPrefs.GetInt(key);
        if (count == 0)
            return Array.Empty<Guid>();

        var uuids = new Guid[count];
        for (int i = 0; i < count; i++)
        {
            var uuidStr = PlayerPrefs.GetString("uuid" + i, string.Empty);
            if (Guid.TryParse(uuidStr, out var uuid))
            {
                uuids[i] = uuid;
            }
            else
            {
                Debug.LogWarning($"Invalid UUID at index {i}: {uuidStr}");
            }
        }

        return uuids;
    }

    private async Task<List<OVRSpatialAnchor.UnboundAnchor>> LoadUnboundAnchorsAsync(Guid[] uuids)
    {
        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unboundAnchors);

        if (!result.Success)
        {
            Debug.LogError($"Load anchors failed with status: {result.Status}");
            return new List<OVRSpatialAnchor.UnboundAnchor>();
        }

        return unboundAnchors;
    }

    private void LocalizeAnchors(List<OVRSpatialAnchor.UnboundAnchor> anchors)
    {
        foreach (var anchor in anchors)
        {
            anchor.LocalizeAsync().ContinueWith(_onLocalized, anchor);
        }
    }


    private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
    {
        if (!success)
        {
            Debug.Log("NO SUCCESS");
            return;
        }

        unboundAnchor.TryGetPose(out Pose pose);
        LoadedObject.transform.position = pose.position;
        LoadedObject.transform.rotation = pose.rotation;
        LoadedObject.GetComponent<RegiTarget>().SetVisible(true);
        OVRSpatialAnchor spatialAnchor = LoadedObject.gameObject.AddComponent<OVRSpatialAnchor>();
        unboundAnchor.BindTo(LoadedObject.gameObject.GetComponent<OVRSpatialAnchor>());
        _spatialAnchorManager.LinkNewAnchor(spatialAnchor);
    }
}