using UnityEngine;

namespace Buck
{
    [System.Serializable]
    public class PlaceableObjectData
    {
        public int PrefabIndex;
        public Pose LocalPose;
        public string AuxData;

        public PlaceableObjectData(int prefabIndex, Pose localPose, string auxData = null)
        {
            PrefabIndex = prefabIndex;
            LocalPose = localPose;
            AuxData = auxData;
        }
    }
}
