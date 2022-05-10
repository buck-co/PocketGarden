using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class UI_HarvestingFirstRun : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] GameObject aPlantHasGrownFruit;
        [SerializeField] GameObject shakePlantToHarvest;

        bool _displayedGrown,
             _haveRun;

        private void OnEnable()
        {
            PlaceablesManager.Instance.AllGroupsLoaded.RemoveListener(Run);

            if (_haveRun) return;

            if (PlaceablesManager.Instance.CurrentGroup != null) Run(PlaceablesManager.Instance.PlaceablesGroups);
            else PlaceablesManager.Instance.AllGroupsLoaded.AddListener(Run); /// fallback
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Run(List<PlaceablesGroup> groups)
        {
            PlaceablesManager.Instance.AllGroupsLoaded.RemoveListener(Run);

            if (groups == null)
            {
                return;
            }

            /// In case user hasn't cleared splash yet
            if (GardenManager.Instance.CurrentAppState == AppState.Splash)
            {
                GardenManager.Instance.AppStateChanged.AddListener(OnAppStateChanged);
                return;
            }

            if (!_displayedGrown)
                Invoke("TryDisplayGrown", 1);
        }

        private void OnAppStateChanged(AppState state)
        {
            GardenManager.Instance.AppStateChanged.RemoveListener(OnAppStateChanged);
            Run(PlaceablesManager.Instance.PlaceablesGroups);
        }

        private void TryDisplayGrown()
        {
            bool foundFruit = (FindObjectOfType<FruitBehavior>() != null);

            //PlaceablesManager.Instance.PlaceablesGroups.ForEach(group =>
            //{
            //    group.Placeables.ForEach(placeable =>
            //    {
            //        if (placeable.GetType() == typeof(Plant))
            //        {
            //            Plant p = (Plant)placeable;
            //            if (p.PlantData.Stage == 2)
            //                foundFruit = true;
            //        }
            //    });
            //});

            if (foundFruit && !_displayedGrown)
            {
                aPlantHasGrownFruit.SetActive(true);
                StartCoroutine(WillHideGrown());

                InteractionManager.Instance.PlaceableHovered.AddListener(LookingForFruit);
                GardenManager.Instance.InventoryUpdated.AddListener(StartedHarvesting);
            }
            else
            {
                _haveRun = true; /// we're done, this behavior won't run again
            }
        }

        IEnumerator WillHideGrown()
        {
            yield return new WaitForSeconds(2.3f);

            aPlantHasGrownFruit.SetActive(false);
            _displayedGrown = true;
        }

        void LookingForFruit(PlaceableObject placeable)
        {
            if (placeable == null) return;

            FruitBehavior fruit = placeable.GetComponentInChildren<FruitBehavior>();

            if (fruit != null)
            {
                SetShakePlantToHarvest();
                InteractionManager.Instance.PlaceableHovered.RemoveListener(LookingForFruit);

                _haveRun = true; /// we're done, this behavior won't run again
            }
        }

        private void SetShakePlantToHarvest()
        {
            aPlantHasGrownFruit.SetActive(false);
            shakePlantToHarvest.SetActive(true);
        }

        private void StartedHarvesting(List<ItemData> inventory)
        {
            shakePlantToHarvest.SetActive(false);
            aPlantHasGrownFruit.SetActive(false);
            GardenManager.Instance.InventoryUpdated.RemoveListener(StartedHarvesting);
            InteractionManager.Instance.PlaceableHovered.RemoveListener(LookingForFruit);

            UI_FirstRunState.Instance.CompleteFirstHarvesting();
        }
    }
}