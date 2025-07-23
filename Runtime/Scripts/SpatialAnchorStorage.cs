using Discover.SpatialAnchors;
using UnityEngine;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
    public class SpatialAnchorStorage: MonoBehaviour
    {
        public SpatialAnchorManager<SpatialAnchorSaveData> AnchorManager;
        public string fileName;
        public string filePath;
        public GameObject anchorPrefab;
    
        public void Awake()
        {
            AnchorManager = new SpatialAnchorManager<SpatialAnchorSaveData>(new AnchorJsonFileManager<SpatialAnchorSaveData>(fileName, filePath));
            AnchorManager.OnAnchorDataLoadedCreateGameObject = data =>
            {
                var go = Instantiate(anchorPrefab);
                go.name = data.Name;
                //go.transform.position += data.positionOffset;
                return go;
            };
        }
        void Start()
        {
            AnchorManager.LoadAnchors();
        }
        // public async void CreateAndSaveAnchor(Vector3 atPosition, string name, string description)
        // {
        //     // 1. GameObject mit OVRSpatialAnchor
        //     var go = Instantiate(anchorPrefab, atPosition, Quaternion.identity);
        //     var anchor = go.AddComponent<OVRSpatialAnchor>();
        //
        //     // 2. Daten-Objekt befüllen
        //     SpatialAnchorSaveData data = new SpatialAnchorSaveData()
        //     {
        //         Name = name,
        //         Gg = Vector3.zero // oder was du brauchst
        //     };
        //
        //     // 3. Speichern (wartet intern auf anchor.Uuid, ruft SaveAnchorAsync, …
        //     _anchorManager.SaveAnchor(anchor, data);
        // }
    }
}
