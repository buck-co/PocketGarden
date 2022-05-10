using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class UI_PlantingFirstRun : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] GameObject letsStartGardeningFirstRunUI;
        [SerializeField] GameObject tapToPreviewUI;
        [SerializeField] GameObject sucessfulPlantingFirstRunUI;
        [SerializeField] GameObject theresMoreFirstRunUI;

        bool _haveRun;

        private void OnEnable()
        {
            PlaceablesManager.Instance.AllGroupsLoaded.RemoveListener(Run);

            if (_haveRun) return;

            if (PlaceablesManager.Instance.GroupsLoadingComplete) Run(PlaceablesManager.Instance.PlaceablesGroups);
            else PlaceablesManager.Instance.AllGroupsLoaded.AddListener(Run); /// fallback
        }

        void Run(List<PlaceablesGroup> groups)
        {
            if (_haveRun) return;

            /// In case user hasn't cleared splash yet
            if (GardenManager.Instance.CurrentAppState == AppState.Splash)
            {
                GardenManager.Instance.AppStateChanged.AddListener(OnAppStateChanged);
                return;
            }

            /// If this is our first time planting
            if (groups == null ||
                PlaceablesManager.Instance.CurrentGroup == null ||
                PlaceablesManager.Instance.CurrentGroup.Placeables.Count == 0)
            {
                StartCoroutine(LetsStartGardening());
            }
        }

        private void OnAppStateChanged(AppState state)
        {
            GardenManager.Instance.AppStateChanged.RemoveListener(OnAppStateChanged);
            Run(PlaceablesManager.Instance.PlaceablesGroups);
        }

        private void OnDisable()
        {
            letsStartGardeningFirstRunUI.SetActive(false);
            sucessfulPlantingFirstRunUI.SetActive(false);
            theresMoreFirstRunUI.SetActive(false);
            tapToPreviewUI.SetActive(false);
        }

        IEnumerator LetsStartGardening()
        {
            _haveRun = true;

            letsStartGardeningFirstRunUI.SetActive(true);
            yield return new WaitForSeconds(2.5f);

            letsStartGardeningFirstRunUI.SetActive(false);

            SetTapToPreview(GardenManager.Instance.CurrentAppState);
        }

        void SetTapToPreview(AppState appState)
        {
            if (appState != AppState.Placing)
                return;

            GardenManager.Instance.AppStateChanged.AddListener(WaitForConfirmation);

            letsStartGardeningFirstRunUI.SetActive(false);
            tapToPreviewUI.SetActive(true);
        }

        void WaitForConfirmation(AppState appState)
        {
            if (appState != AppState.Previewing)
                return;

            tapToPreviewUI.SetActive(false);
            GardenManager.Instance.AppStateChanged.RemoveListener(WaitForConfirmation);
            PlaceablesManager.Instance.ObjectPlaced.AddListener(StartShowFirstPlantedMessages);
        }

        void StartShowFirstPlantedMessages(GameObject planted)
        {
            PlaceablesManager.Instance.ObjectPlaced.RemoveListener(StartShowFirstPlantedMessages);
            StartCoroutine(ShowFirstPlantingMessages());
        }

        IEnumerator ShowFirstPlantingMessages()
        {
            /// Subscribe to menu state change. When user taps menu button, we'll hide the "there's more" message
            UI_MainMenu.Instance.MenuStateChanged.AddListener(MenuButtonTappedForFirstTimeAfterFirstPlanting);

            sucessfulPlantingFirstRunUI.SetActive(true);

            yield return new WaitForSeconds(2.0f);

            sucessfulPlantingFirstRunUI.SetActive(false);
            theresMoreFirstRunUI.SetActive(true);
            this.Invoke(() => { theresMoreFirstRunUI.SetActive(false); }, 2.5f);

            UI_FirstRunState.Instance.CompleteFirstPlanting();

            yield return null;
        }

        void MenuButtonTappedForFirstTimeAfterFirstPlanting(MainMenuState state)
        {
            theresMoreFirstRunUI.SetActive(false);
            UI_MainMenu.Instance.MenuStateChanged.RemoveListener(MenuButtonTappedForFirstTimeAfterFirstPlanting);
            gameObject.SetActive(false);
        }

    }
}