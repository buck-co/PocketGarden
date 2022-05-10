using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class UI_ErrorStateResponder : UI_StateResponder
    {
        // Properties
        [SerializeField] ErrorState enabledStates;

        private ErrorState lastErrorState;

        public override void Subscribe()
        {
            StateUpdated(GeospatialManager.Instance.CurrentErrorState, GeospatialManager.Instance.CurrentErrorMessage);
            GeospatialManager.Instance.ErrorStateChanged.AddListener(StateUpdated);
        }
        public override void Unsubscribe()
        {
            GeospatialManager.Instance.ErrorStateChanged.RemoveListener(StateUpdated);
        }

        public virtual void StateUpdated(ErrorState errorState, string message)
        {
            if (showLogs) Debug.Log(gameObject.name + " responding to error state change. " + errorState, this);

            if (errorState == lastErrorState)
                return;

            bool currentEnabledState = enabledStates.HasFlag(errorState);

            if (showLogs) Debug.Log("StateUpdated: Error State: " + errorState, this);
            if (showLogs) Debug.Log(gameObject.name + " has visible flag: " + currentEnabledState, this);

            UpdateEnabledState(currentEnabledState);
            lastErrorState = errorState;

            base.StateUpdated();
        }
    }
}