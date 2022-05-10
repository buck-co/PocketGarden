using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Buck.PocketGarden
{
    public class UI_ButtonOpenMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Image openMenuButtonIcon;
        [SerializeField] Image plantingModeButtonIcon;
        [SerializeField] Image wateringModeButtonIcon;

        // Vars
        private Sprite initialIcon;

        private void Awake()
        {
            initialIcon = openMenuButtonIcon.sprite;
        }

        private void Start()
        {
            OnAppStateChanged(GardenManager.Instance.CurrentAppState);
            GardenManager.Instance.AppStateChanged.AddListener(OnAppStateChanged);
        }

        void OnAppStateChanged(AppState appState)
        {
            switch (appState)
            {
                case AppState.Placing:
                    SetMenuIcon(plantingModeButtonIcon);
                    break;

                case AppState.Watering:
                    SetMenuIcon(wateringModeButtonIcon);
                    break;

                case AppState.Viewing:
                    ResetIcon();
                    break;
            }

        }

        public void ResetIcon()
        {
            openMenuButtonIcon.sprite = initialIcon;
        }

        private void SetMenuIcon(Image sourceImage) {
            openMenuButtonIcon.sprite = sourceImage.sprite;
        }

    }
}