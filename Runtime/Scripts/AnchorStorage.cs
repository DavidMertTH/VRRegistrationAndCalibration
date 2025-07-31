using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRRegistrationAndCalibration.Runtime.Scripts
{
    public static class AnchorStorage
    {
        private const string NumAnchorsKey = "num_anchors";
        private const string AnchorKeyPrefix = "anchor_uuid_";

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
    }
}