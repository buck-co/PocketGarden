using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck
{
    [CreateAssetMenu(fileName = "SoundLib", menuName = "Buck/SoundLib")]
    public class SoundLib : ScriptableObject
    {
        public List<Sound> Sounds = new List<Sound>();
    }

    [System.Serializable]
    public class Sound
    {
        public string id;
        public AudioClip clip;
    }

}
