using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Buck.PocketGarden
{
    public class UI_Previewing : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Button cancelButton;
        [SerializeField] Button confirmButton;

        [Header("Debug")]
        [SerializeField] bool showLogs;

        private void Awake()
        {
            cancelButton.onClick.AddListener(CancelPreview);
            confirmButton.onClick.AddListener(ConfirmPlacement);
        }

        private void CancelPreview()
        {
            if (showLogs) Debug.Log("CancelPreview");
            GardenManager.Instance.SetAppState(AppState.Placing);
            InteractionManager.Instance.VibrateOnce();
            GardenManager.Instance.PlaySound("CancelPlacement");
        }
        private void ConfirmPlacement()
        {
            if (showLogs) Debug.Log("ConfirmPlacement");

            PlaceablesManager.Instance.FinalizeSelectedPlaceable();
            GardenManager.Instance.SetAppState(AppState.Placing);
            InteractionManager.Instance.VibrateOnce();
            GardenManager.Instance.PlaySound("AcceptPlacement");
        }

    }
}
