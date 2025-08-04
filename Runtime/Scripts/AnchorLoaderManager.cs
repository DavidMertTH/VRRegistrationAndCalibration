using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Manages spatial anchor creation, deletion, saving, and linking within the application.
/// </summary>
/// <remarks>
/// David Mertens, TH Koeln.
/// </remarks>
/// 
public class AnchorLoaderManager : MonoBehaviour
{
    public string numUuidsPlayerPref = "NumUuids";
    public List<OVRSpatialAnchor> anchors;
    public AnchorLoader AnchorLoader;
    public List<Guid> Uuids;

    private void Awake()
    {
        AnchorLoader = new AnchorLoader(this);
        anchors = new List<OVRSpatialAnchor>();
        Uuids = new List<Guid>();
    }
    
    /// <summary>
    /// Asynchronously deletes all anchors and their saved UUIDs.
    /// </summary>
    /// 
    public async Task DeleteAllAnchors()
    {
        var result = await OVRSpatialAnchor.EraseAnchorsAsync(anchors, anchors.Select(a => a.Uuid));
        if (result.Success)
        {
            anchors.ForEach(a => Destroy(a.gameObject));
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
        if (!PlayerPrefs.HasKey(numUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(numUuidsPlayerPref, 0);
        }

        int playerNumUuids = PlayerPrefs.GetInt(numUuidsPlayerPref);
        PlayerPrefs.SetString("uuid" + playerNumUuids, uuid.ToString());
        PlayerPrefs.SetInt(numUuidsPlayerPref, ++playerNumUuids);
        PlayerPrefs.Save();
    }

    private void DeleteSavedUuids()
    {
        if (!PlayerPrefs.HasKey(numUuidsPlayerPref))
            return;
        int numUuids = PlayerPrefs.GetInt(numUuidsPlayerPref);
        for (int i = 0; i < numUuids; i++)
        {
            string key = $"uuid{i}";
            if (PlayerPrefs.HasKey(key))
                PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.DeleteKey(numUuidsPlayerPref);
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