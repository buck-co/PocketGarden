using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class UI_MainMenuStateResponder : UI_StateResponder
    {
        // Properties
        [SerializeField] MainMenuState enabledStates;

        public override void Subscribe()
        {
            UI_MainMenu.Instance.MenuStateChanged.AddListener(StateUpdated);
        }
        public override void Unsubscribe()
        {
            UI_MainMenu.Instance.MenuStateChanged.RemoveListener(StateUpdated);
        }

        public virtual void StateUpdated(MainMenuState menuState)
        {

            bool enabledState = enabledStates.HasFlag(menuState);

            UpdateEnabledState(enabledState);
            base.StateUpdated();
        }
    }
}