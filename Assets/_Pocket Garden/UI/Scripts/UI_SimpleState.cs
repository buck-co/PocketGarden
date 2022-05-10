using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Buck.PocketGarden
{
    public enum SimpleState { Disabled, Enabled }
    public class UI_SimpleState : MonoBehaviour
    {

        public SimpleState CurrentSimpleState { get { return _simpleState; } }
        [SerializeField] private SimpleState _simpleState = SimpleState.Disabled;
        [HideInInspector] public UnityEvent<SimpleState> SimpleStateChanged;

        private void Start()
        {
            SetState(_simpleState);
        }
        public void ToggleState() {
            if (_simpleState == SimpleState.Enabled)
            {
                SetState(SimpleState.Disabled);
            }
            else
            {
                SetState(SimpleState.Enabled);
            }
            
        }
        public void SetState(SimpleState newState)
        {
            if (_simpleState != newState)
            {
                _simpleState = newState;
                SimpleStateChanged.Invoke(_simpleState);
            }
        }
    }
}