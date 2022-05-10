using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    [RequireComponent(typeof(UI_ProgressSender))]
    public class UI_PivotTransition : MonoBehaviour
    {
        // Refernces
        [Header("References")]
        [SerializeField] UI_ProgressSender progressSender;
        [SerializeField] RectTransform rectTransform;

        // Properties
        [Header("Properties")]
        [SerializeField] float enabledY = .5f;
        [SerializeField] float disabledY = 2.75f;
        [SerializeField] float enabledX = .5f;
        [SerializeField] float disabledX = .5f;

        float targetY;
        float targetX;
        float startingY;
        float startingX;

        [Header("Debug")]
        public bool showLogs;

        private void Start()
        {
            rectTransform = progressSender.stateResponder.canvasGroup.transform.GetComponent<RectTransform>();
            InitAnchor(progressSender.stateResponder.enabledOnStart);
        }

        private void InitAnchor(bool enabledOnStart)
        {

            float y = enabledOnStart ? enabledY : disabledY;
            float x = enabledOnStart ? enabledX : disabledX;
            rectTransform.pivot = new Vector2(x, y);

            // Logs
            if (showLogs) Debug.Log("InitAnchor");
            if (showLogs) Debug.Log(enabledOnStart, this);
            if (showLogs) Debug.Log(y, this);
        }

        private void OnEnable()
        {
            progressSender.OnStartTransition.AddListener(SetupTransition);
            progressSender.OnUpdateProgress.AddListener(TransitionPivot);
        }

        private void OnDisable()
        {
            progressSender.OnStartTransition.RemoveListener(SetupTransition);
            progressSender.OnUpdateProgress.RemoveListener(TransitionPivot);
        }

        void SetupTransition(bool transitioningToEnabledState)
        {
            startingY = rectTransform.pivot.y;
            startingX = rectTransform.pivot.x;

            targetY = transitioningToEnabledState ? enabledY : disabledY;
            targetX = transitioningToEnabledState ? enabledX : disabledX;

            // Logs
            if (showLogs) Debug.Log("SetupTransition");
            if (showLogs) Debug.Log("transitioningToEnabledState " + transitioningToEnabledState);
            if (showLogs) Debug.Log("startingY " + startingY + " --> targetY: " + targetY);
            if (showLogs) Debug.Log("startingX " + startingX + " --> targetX: " + targetX);

        }

        void TransitionPivot(float progress)
        {
            float y = Mathf.Lerp(startingY, targetY, progress);
            float x = Mathf.Lerp(startingX, targetX, progress);

            rectTransform.pivot = new Vector2(x, y);

            // Logs
            if (showLogs) Debug.Log("TransitionPivot" + progress, this);
            if (showLogs) Debug.Log("rectTransform.pivot " + rectTransform.pivot, this);
        }

    }
}