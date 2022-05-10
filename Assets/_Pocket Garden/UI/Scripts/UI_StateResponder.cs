using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Buck.PocketGarden
{
    public class UI_StateResponder : MonoBehaviour
    {

        // References
        [Header("References")]
        public CanvasGroup canvasGroup;

        [Header("Options")]
        public bool enabledOnStart;
        public bool toggleVisibility;
        public bool toggleInteractivity;
        public bool toggleEnabled;
        [Tooltip("Allow a sibling component to have control of the animation")]
        public bool useProgressSender = true;

        // Events
        [HideInInspector] public UnityEvent<bool> StateUpdatedEvent;

        // Vars
        protected bool animate;

        [Header("Debug")]
        [SerializeField] protected bool showLogs;

        private void Start() { Init(); Subscribe(); }

        public virtual void Subscribe() { }
        public virtual void Unsubscribe() { }

        public virtual void Init()
        {
            if (showLogs) Debug.Log("init");

            if (useProgressSender)
                animate = TryGetComponent(out UI_ProgressSender sender);

            if (toggleEnabled) canvasGroup.gameObject.SetActive(enabledOnStart);
            if (toggleVisibility) SetVisibility(enabledOnStart);
            if (toggleInteractivity) SetInteractibility(enabledOnStart);
        }

        public virtual void StateUpdated()
        {
            if (showLogs) Debug.Log("base.StateUpdated()", this);
        }

        public void UpdateEnabledState(bool makeEnabled)
        {
            if (showLogs) Debug.Log("UpdateEnabledState() " + makeEnabled, this);

            if (!animate)
            {
                if (toggleInteractivity) SetInteractibility(makeEnabled);
                if (toggleVisibility) SetVisibility(makeEnabled);
                if (toggleEnabled) canvasGroup.gameObject.SetActive(makeEnabled);
            }

            StateUpdatedEvent.Invoke(makeEnabled);
        }

        public virtual void SetInteractibility(bool isInteractable)
        {
            canvasGroup.interactable = isInteractable;
            canvasGroup.blocksRaycasts = isInteractable;
        }

        public void SetVisibility(bool isVisible)
        {
            canvasGroup.alpha = isVisible ? 1 : 0;
        }

    }
}