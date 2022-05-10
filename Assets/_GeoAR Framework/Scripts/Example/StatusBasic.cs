using UnityEngine;
using TMPro;

namespace Buck.PocketGarden
{
    public class StatusBasic : Singleton<StatusBasic>
    {
        [Header("References")]
        [SerializeField] TMP_Text errorState_text;
        [SerializeField] TMP_Text interactionState_text;

        private void OnEnable()
        {
            OnErrorStateChanged(GeospatialManager.Instance.CurrentErrorState, GeospatialManager.Instance.CurrentErrorMessage);
            GeospatialManager.Instance.ErrorStateChanged.AddListener(OnErrorStateChanged);

            OnInteractionStateChanged(InteractionManager.Instance.CurrentInteractionState);
            InteractionManager.Instance.InteractionStateChanged.AddListener(OnInteractionStateChanged);
        }

        private void OnErrorStateChanged(ErrorState errorState, string message)
        {
            errorState_text.text = "Error State: " + errorState;
        }

        private void OnInteractionStateChanged(InteractionState interactionState)
        {
            interactionState_text.text = "Interaction State: " + interactionState;
        }
    }
}