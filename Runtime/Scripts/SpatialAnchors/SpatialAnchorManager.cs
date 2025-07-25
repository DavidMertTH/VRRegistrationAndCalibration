// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Discover.SpatialAnchors
{
    public class SpatialAnchorManager<TData> where TData : SpatialAnchorSaveData
    {
        private Dictionary<Guid, TData> m_anchorUidToData;
        private List<TData> m_anchorSavedData;
        private ISpatialAnchorFileManager<TData> m_fileManager;

        public Func<TData, GameObject> OnAnchorDataLoadedCreateGameObject;

        public SpatialAnchorManager(ISpatialAnchorFileManager<TData> fileManager)
        {
            m_fileManager = fileManager;
            m_anchorUidToData = new Dictionary<Guid, TData>();
            m_anchorSavedData = new List<TData>();
        }

        public async void SaveAnchor(OVRSpatialAnchor anchor, TData data)
        {
            while (anchor.Uuid == Guid.Empty) await Task.Yield();

            var result = await anchor.SaveAnchorAsync();
            if (result.Success)
            {
                Debug.Log($"Anchor with {AnchorUtils.GuidToString(anchor.Uuid)} saved");
                m_anchorUidToData[anchor.Uuid] = data;
                OnSpaceSaveComplete(anchor.Uuid, data);
            }
            else
            {
                Debug.Log($"Anchor with {AnchorUtils.GuidToString(anchor.Uuid)} failed to saved");
                OnSpaceSaveComplete(anchor.Uuid, data);
            }
        }

        public async void EraseAnchor(
            OVRSpatialAnchor anchor, bool saveOnErase = true, Action<OVRSpatialAnchor, bool> onAnchorErased = null)
        {
            var uuid = anchor.Uuid;
            var result = await anchor.EraseAnchorAsync();
            if (result.Success)
            {
                Debug.Log($"Erased anchor data {uuid}");
                var dataToRemove = m_anchorSavedData.Find(data => data.AnchorUuid == uuid);
                _ = m_anchorSavedData.Remove(dataToRemove);
                _ = m_anchorUidToData.Remove(uuid);
                if (saveOnErase)
                {
                    SaveToFile();
                }
            }
            else
            {
                Debug.LogError($"Failed to erased anchor data {uuid}");
            }

            onAnchorErased?.Invoke(anchor, result.Success);
        }

        public void LoadAnchors()
        {
            m_anchorSavedData = m_fileManager.ReadDataFromFile();
            var anchorsToQuery = new HashSet<Guid>();
            foreach (var data in m_anchorSavedData)
            {
                _ = anchorsToQuery.Add(data.AnchorUuid);
                m_anchorUidToData[data.AnchorUuid] = data;
            }

            if (anchorsToQuery.Count < 1)
            {
                Debug.Log("No anchors to load");
                return;
            }

            QueryAnchors(anchorsToQuery);
        }

        private async void QueryAnchors(HashSet<Guid> anchorUuids)
        {
            var anchorIds = anchorUuids.ToList();

            Debug.Log($"Querying for anchors {anchorIds.Count}");
            var unboundAnchors = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(
                anchorUuids, new List<OVRSpatialAnchor.UnboundAnchor>(anchorUuids.Count));
            OnCompleteUnboundAnchors(unboundAnchors.Value.ToArray());
        }

        private void OnCompleteUnboundAnchors(OVRSpatialAnchor.UnboundAnchor[] unboundAnchors)
        {
            if (unboundAnchors == null)
                return;

            foreach (var queryResult in unboundAnchors)
            {
                Debug.Log($"Initializing app with guid {AnchorUtils.GuidToString(queryResult.Uuid)}");
                var appData = m_anchorUidToData[queryResult.Uuid];
                var gameObject = OnAnchorDataLoadedCreateGameObject(appData);
                var anchor = gameObject.AddComponent<OVRSpatialAnchor>();
                queryResult.BindTo(anchor);
            }
        }

        private void OnSpaceSaveComplete(Guid anchorUuid, TData data)
        {
            data.AnchorUuid = anchorUuid;
            m_anchorSavedData.Add(data);

            SaveToFile();
        }

        private void SaveToFile()
        {
            m_fileManager.WriteDataToFile(m_anchorSavedData);
        }

        public void ClearData()
        {
            m_anchorSavedData.Clear();
            m_anchorUidToData.Clear();
            SaveToFile();
        }
    }
}