using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Buck.PocketGarden
{
    public class UI_DeveloperPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Button closeDeveloperPanelButton;

        private void Awake()
        {
            closeDeveloperPanelButton.onClick.AddListener(CloseDeveloperPanel);
        }

        private void CloseDeveloperPanel() {
            GardenManager.Instance.SetAppState(GardenManager.Instance.PreviousAppState);
        }
    }
}