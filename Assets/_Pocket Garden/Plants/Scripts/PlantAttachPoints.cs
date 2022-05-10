using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class PlantAttachPoints : MonoBehaviour
    {
        public Transform IKTarget;
        public NullConfig LeafPoints;
        public NullConfig BudPoints;
    }

    [System.Serializable]
    public class NullConfig
    {
        public Vector3 RotationTollerances;
        public Vector3 ScaleTollerances;
        public List<Transform> Nulls;
    }
}


