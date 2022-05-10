using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class UI_SimpleStateResponder : UI_StateResponder
    {
        // References
        [SerializeField] UI_SimpleState _simpleState;

        // Properties
        [SerializeField] bool reverse;

        public override void Subscribe()
        {
            _simpleState.SimpleStateChanged.AddListener(StateUpdated);
        }
        public override void Unsubscribe()
        {
            _simpleState.SimpleStateChanged.RemoveListener(StateUpdated);
        }

        public virtual void StateUpdated(SimpleState simpleState)
        {
            if (showLogs) Debug.Log("StateUpdated() ", this);

            bool enabledState = simpleState == SimpleState.Enabled;
            if (reverse) enabledState = !enabledState;

            UpdateEnabledState(enabledState);

            base.StateUpdated();
        }
    }
}