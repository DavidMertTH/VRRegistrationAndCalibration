using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
    public class AnchorLoaderManager : MonoBehaviour
    {
        public string NumUuidsPlayerPref = "NumUuids";
        public List<OVRSpatialAnchor> anchors;
        public AnchorLoader AnchorLoader;
        public List<Guid> Uuids;

        private void Awake()
        {
            AnchorLoader = new AnchorLoader(this);
            anchors = new List<OVRSpatialAnchor>();
            Uuids = new List<Guid>();
        }

        public async Task DeleteAllAnchors()
        {
            var result = await OVRSpatialAnchor.EraseAnchorsAsync(anchors, anchors.Select(a => a.Uuid));
            if (result.Success)
            {
                anchors.ForEach(a => Destroy(a.gameObject));
            }
            else
            {
                Debug.LogError($"Anchors NOT erased {result.Status}");
            }
            Uuids.Clear();
            anchors.Clear();
            Debug.Log($"Anchors erased.");
            DeleteSavedUuids();
        }

        public void LinkNewAnchor(OVRSpatialAnchor anchor)
        {
            anchors.Add(anchor);
        }

        public void SaveAnchor(OVRSpatialAnchor anchor)
        {
            anchor.SaveAnchorAsync();
            SaveUuid(anchor.Uuid);
        }

        private void SaveUuid(Guid uuid)
        {
            if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
            {
                PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
            }

            int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            PlayerPrefs.SetString("uuid" + playerNumUuids, uuid.ToString());
            PlayerPrefs.SetInt(NumUuidsPlayerPref, ++playerNumUuids);
            PlayerPrefs.Save();
        }

        private void DeleteSavedUuids()
        {
            if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
                return;
            int numUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            for (int i = 0; i < numUuids; i++)
            {
                string key = $"uuid{i}";
                if (PlayerPrefs.HasKey(key))
                    PlayerPrefs.DeleteKey(key);
            }

            PlayerPrefs.DeleteKey(NumUuidsPlayerPref);
            PlayerPrefs.Save();
        }

        private IEnumerator AnchorCreated(OVRSpatialAnchor instancedAnchor)
        {
            while (!instancedAnchor.Created && !instancedAnchor.Localized)
            {
                yield return new WaitForEndOfFrame();
            }

            Guid anchorGuid = instancedAnchor.Uuid;
            RegiTarget tracker = instancedAnchor.GetComponent<RegiTarget>();
            tracker.uuidName = anchorGuid;
            anchors.Add(instancedAnchor);
        }
    }
}