using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AnchorStorage
{
    private const string NumAnchorsKey = "num_anchors";
    private const string AnchorKeyPrefix = "anchor_uuid_";

    public static async Task SaveAnchorAsync(OVRSpatialAnchor anchor)
    {
        await anchor.SaveAnchorAsync();

        string uuid = anchor.Uuid.ToString();
        int count = PlayerPrefs.GetInt(NumAnchorsKey, 0);
        PlayerPrefs.SetString(AnchorKeyPrefix + count, uuid);
        PlayerPrefs.SetInt(NumAnchorsKey, count + 1);
        PlayerPrefs.Save();
    }

    public static void DeleteAllAnchors()
    {
        int count = PlayerPrefs.GetInt(NumAnchorsKey, 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(AnchorKeyPrefix + i);
        }

        PlayerPrefs.DeleteKey(NumAnchorsKey);
        PlayerPrefs.Save();
    }

    public static List<Guid> LoadAllAnchorUuids()
    {
        var uuids = new List<Guid>();
        int count = PlayerPrefs.GetInt(NumAnchorsKey, 0);
        for (int i = 0; i < count; i++)
        {
            string uuidStr = PlayerPrefs.GetString(AnchorKeyPrefix + i, null);
            if (Guid.TryParse(uuidStr, out Guid uuid))
                uuids.Add(uuid);
        }

        return uuids;
    }

    public static async Task InstantiateAnchoredPrefabsAsync(GameObject prefab)
    {
        // 1) Alle gespeicherten UUIDs holen
        List<Guid> uuids = LoadAllAnchorUuids();
        if (uuids == null || uuids.Count == 0)
            return;

        // 2) UnboundAnchors laden
        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        var loadResult = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unboundAnchors);
        if (!loadResult.TryGetValue(out var anchors))
        {
            Debug.LogError($"Failed to load anchors: {loadResult.Status}"); // :contentReference[oaicite:0]{index=0}
            return;
        }

        foreach (var unb in anchors)
        {
            Pose pose = new Pose();
            unb.TryGetPose(out pose);
            GameObject createdByUuid = Object.Instantiate(prefab, pose.position, pose.rotation);
            Object.Destroy(createdByUuid.GetComponent<OVRSpatialAnchor>());
            createdByUuid.transform.position = pose.position;
            createdByUuid.transform.rotation = pose.rotation;
            createdByUuid.AddComponent<OVRSpatialAnchor>();
        }
    }
}