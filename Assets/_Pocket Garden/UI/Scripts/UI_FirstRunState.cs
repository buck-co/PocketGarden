using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Buck.PocketGarden
{
    public class UI_FirstRunState : Singleton<UI_FirstRunState>
    {
        [Header("Planting First Run")]
        [SerializeField] GameObject theresMoreFirstRunUI;

        [Header("Harvesting First Run")]
        [SerializeField] GameObject yourPlantHasGrownFirstRunUI;
        [SerializeField] GameObject shakeToHarvestFirstRunUI;

        // Properties
        public bool FirstPlantingComplete { get { return _firstPlantingComplete; } }
        public bool FirstWateringComplete { get { return _firstWateringComplete; } }
        public bool FirstHarvestingComplete { get { return _firstHarvestingComplete; } }
        public bool FirstRunComplete { get { return _firstRunComplete; } }

        // Events
        [HideInInspector] public UnityEvent CompletedPlantingFirstRun;

        // Vars
        private bool _firstRunComplete;
        private bool _firstPlantingComplete;
        private bool _firstWateringComplete;
        private bool _firstHarvestingComplete;


        [Header("Debug")]
        [SerializeField] bool showLogs;
        [SerializeField] bool skipFirstRun;

        private void OnEnable()
        {
            if (PlayerPrefs.HasKey("LocalStorageData")) {
                _firstPlantingComplete = true;
            }   
        }

        public void CompleteFirstPlanting()
        {
            _firstPlantingComplete = true;
            CompletedPlantingFirstRun.Invoke();
        }

        public void CompleteFirstWatering()
        {
            _firstWateringComplete = true;
        }

        public void CompleteFirstHarvesting()
        {
            _firstHarvestingComplete = true;
        }

    }
}