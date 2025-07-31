using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
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
            if (target.GetComponent<OVRSpatialAnchor>() != null) Object.Destroy(target.GetComponent<OVRSpatialAnchor>());
        
            if (!PlayerPrefs.HasKey(_spatialAnchorManager.NumUuidsPlayerPref)) PlayerPrefs.SetInt(_spatialAnchorManager.NumUuidsPlayerPref, 0);
            var playerUuidCount = PlayerPrefs.GetInt(_spatialAnchorManager.NumUuidsPlayerPref);

            if (playerUuidCount == 0) return;

            var uuids = new Guid[playerUuidCount];

            for (int i = 0; i < playerUuidCount; i++)
            {
                var uuidKey = "uuid" + i;
                var currentUuid = PlayerPrefs.GetString(uuidKey);
                uuids[i] = new Guid(currentUuid);
            }
            _spatialAnchorManager.Uuids.AddRange(uuids);

            var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
            var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unboundAnchors);
            _allAnchorsInSystem = new List<OVRSpatialAnchor>();
            if (result.Success)
            {
                foreach (var anchor in unboundAnchors)
                {
                    anchor.LocalizeAsync().ContinueWith(_onLocalized, anchor);
                }
            }
            else
            {
                Debug.LogError($"Load anchors failed with {result.Status}.");
            }
        }

   

        private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
        {
            if (!success)
            {
                Debug.Log("NO SUCCESS");
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
}