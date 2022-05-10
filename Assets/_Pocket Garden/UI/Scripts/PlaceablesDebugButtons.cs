using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;

namespace Buck.PocketGarden
{
    public class PlaceablesDebugButtons : MonoBehaviour
    {
        // References
        [SerializeField] Button buttonQuickWipe;
        [SerializeField] Button buttonToggleStatus;
        [SerializeField] Button buttonToggleConsole;
        [SerializeField] Button buttonToggleShowARPlanes;
        [SerializeField] ARPlaneManager aRPlaneManager;
        [SerializeField] Material debugARPlaneMaterial;

        [SerializeField] CanvasGroup _status;
        [SerializeField] CanvasGroup _console;

        private Material defaultARPlaneMaterial;
        private bool showingDebugPlanes;

        private void Start()
        {
            defaultARPlaneMaterial = aRPlaneManager.planePrefab.GetComponent<Renderer>().sharedMaterial;
        }

        private void OnEnable()
        {
            buttonToggleStatus.onClick.AddListener(ToggleStatus);
            buttonToggleConsole.onClick.AddListener(ToggleConsole);
            buttonQuickWipe.onClick.AddListener(ResetData);
            buttonToggleShowARPlanes.onClick.AddListener(ToggleShowARPlanes);

        }

        private void OnDisable()
        {
            buttonToggleStatus.onClick.RemoveListener(ToggleStatus);
            buttonToggleConsole.onClick.RemoveListener(ToggleConsole);
            buttonQuickWipe.onClick.RemoveListener(ResetData);
            buttonToggleShowARPlanes.onClick.RemoveListener(ToggleShowARPlanes);

        }

        private void ToggleShowARPlanes()
        {
            showingDebugPlanes = !showingDebugPlanes;

            if (showingDebugPlanes)
            {
                // Set existing planes to DEBUG material
                foreach (ARPlane plane in InteractionManager.Instance.AllPlanes)
                {
                    plane.GetComponent<Renderer>().material = debugARPlaneMaterial;
                    plane.GetComponent<Renderer>().material.color = new Color(
                        Random.Range(.5f, 1f),
                        Random.Range(.5f, 1f),
                        Random.Range(.5f, 1f),
                        .5f);
                }
                // Subscribe to planes changed event to catch new planes
                aRPlaneManager.planesChanged += SetDebugPlaneMat;
            } else {
                // Set existing planes to DEFAULT material
                foreach (ARPlane plane in InteractionManager.Instance.AllPlanes)
                {
                    plane.GetComponent<Renderer>().material = defaultARPlaneMaterial;
                }

                // Unsubscribe
                aRPlaneManager.planesChanged -= SetDebugPlaneMat;
            }

            // Close dev panel
            GardenManager.Instance.SetAppState(GardenManager.Instance.PreviousAppState);
        }

        private void ToggleStatus()
        {
            _status.alpha = _status.alpha == 0 ? 1 : 0;
        }

        private void ToggleConsole()
        {
            _console.alpha = _console.alpha == 0 ? 1 : 0;
        }

        private void ResetData() {
            //if (showingDebugPlanes) ToggleShowARPlanes();
            PlaceablesManager.Instance.ClearData();
            GardenManager.Instance.SetAppState(AppState.Placing);
            UI_MainMenu.Instance.SetMenuState(MainMenuState.Minimized);
            GardenManager.Instance.ResetInventory();
        }

        private void SetDebugPlaneMat(ARPlanesChangedEventArgs planes)
        {
            foreach (ARPlane plane in planes.added) {

                plane.GetComponent<Renderer>().material = debugARPlaneMaterial;
                plane.GetComponent<Renderer>().material.color = new Color(
                    Random.Range(.5f, 1f),
                    Random.Range(.5f, 1f),
                    Random.Range(.5f, 1f),
                    .5f);
            }
        }

    }
}