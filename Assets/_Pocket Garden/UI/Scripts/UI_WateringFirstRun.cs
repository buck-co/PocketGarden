using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class UI_WateringFirstRun : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] GameObject tapTheWateringCan;
        [SerializeField] GameObject restartTheApp;

        bool firstWateringComplete;

        private void OnEnable()
        {
            firstWateringComplete = UI_FirstRunState.Instance.FirstWateringComplete;

            if (!firstWateringComplete)
                SetTapTheWateringCan();
        }

        private void OnDisable()
        {
            tapTheWateringCan.SetActive(false);
        }

        void SetTapTheWateringCan() {
            //Debug.Log("SetTapTheWateringCan");
            StartCoroutine(ShowTapTheWateringCan());

            GardenManager.Instance.PlantGrew.AddListener(OnPlantGrew);
        }

        IEnumerator ShowTapTheWateringCan() {
            tapTheWateringCan.SetActive(true);

            yield return new WaitForSeconds(2.5f);
       
            tapTheWateringCan.SetActive(false);
        }

        private void OnPlantGrew(Plant plant)
        {
            SetRestartTheApp();
            GardenManager.Instance.PlantGrew.RemoveListener(OnPlantGrew);
        }

        private void SetRestartTheApp()
        {
            StopAllCoroutines();
            tapTheWateringCan.SetActive(false);     
            StartCoroutine(ShowRestartTheApp());
        }

        IEnumerator ShowRestartTheApp()
        {
            restartTheApp.SetActive(true);

            yield return new WaitForSeconds(2.5f);

            restartTheApp.SetActive(false);
            UI_FirstRunState.Instance.CompleteFirstWatering();
        }
    }
}