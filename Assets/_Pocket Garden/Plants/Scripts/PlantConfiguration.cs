using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    [CreateAssetMenu(fileName = "PlantConfiguration", menuName = "Buck/PlantConfiguration", order = 1)]

    public class PlantConfiguration : ScriptableObject
    {
        [Header("Prefabs")]
        public string PlantName;
        public GameObject[] BasePrefabs;
        public List<GameObject> LeafPrefabs = new List<GameObject>();
        public List<GameObject> FlowerPrefabs = new List<GameObject>();
        public List<GameObject> FruitPrefabs = new List<GameObject>();
        public List<Material> Material = new List<Material>();
        public List<Texture2D> Textures = new List<Texture2D>();

        [Header("Settings")]
        public float GrowthPerTickWatered = 1.0f;
        public float GrowthPerTickThirsty = 0.01f;
        public float LeafProbability = 0.8f;
        public float FlowerProbability = 0.8f;
        public float FruitProbabiltiy = 0.5f;  
        public float RandomNoiseScale = 1;
        public float RandomScaleAmount = 0.5f;

        [Header("Shake to Harvest Settings")]
        public float ShakeDistance = 0.75f;
        public float ShakeSmoothSpd = 10;
        public float ShakeAnimateThresh = 0.2f;
        public float ShakeHarvestSpeed = 0.01f;

        [Header("UI")]
        public Sprite InventoryIcon;
        public Sprite InventoryIconDisabled;
    }
}