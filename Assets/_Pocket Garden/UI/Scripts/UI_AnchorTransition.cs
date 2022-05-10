using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    [RequireComponent(typeof(UI_ProgressSender))]
    public class UI_AnchorTransition : MonoBehaviour
    {
        // Refernces
        [Header("References")]
        [SerializeField] UI_ProgressSender progressSender;
        [SerializeField] RectTransform rectTransform;

        // Properties
        [Header("Properties")]
        [SerializeField] float enabledY = 1f;
        [SerializeField] float disabledY = 0f;

        float targetY;
        float startingY;

        [Header("Debug")]
        public bool showLogs;

        private void Start()
        {
            rectTransform = progressSender.stateResponder.canvasGroup.transform.GetComponent<RectTransform>();
            InitAnchor(progressSender.stateResponder.enabledOnStart);
        }

        private void InitAnchor(bool enabledOnStart) {

            float y = enabledOnStart ? enabledY : disabledY;
            rectTransform.anchorMax = new Vector2(1, y);

            // Logs
            if (showLogs) Debug.Log("InitAnchor");
            if (showLogs) Debug.Log(enabledOnStart, this);
            if (showLogs) Debug.Log(y, this);
        }

        private void OnEnable()
        {
            progressSender.OnStartTransition.AddListener(SetupTransition);
            progressSender.OnUpdateProgress.AddListener(TransitionAnchor);
        }

        private void OnDisable()
        {
            progressSender.OnStartTransition.RemoveListener(SetupTransition);
            progressSender.OnUpdateProgress.RemoveListener(TransitionAnchor);
        }

        void SetupTransition(bool transitioningToEnabledState)
        {
            startingY = rectTransform.anchorMax.y;
            targetY = transitioningToEnabledState ? enabledY : disabledY;

            // Logs
            if (showLogs) Debug.Log("SetupTransition");
            if (showLogs) Debug.Log("transitioningToEnabledState " + transitioningToEnabledState);
            if (showLogs) Debug.Log("startingY " + startingY);
            if (showLogs) Debug.Log("targetY " + targetY);
        }

        void TransitionAnchor(float progress)
        {
            float y = Mathf.Lerp(startingY, targetY, progress);
            rectTransform.anchorMax = new Vector2(1, y);

            // Logs
            if (showLogs) Debug.Log("TransitionAnchor" + progress, this);
            if (showLogs) Debug.Log("y " + y, this);
            if (showLogs) Debug.Log("rectTransform.anchorMax " + rectTransform.anchorMax, this);
        }

    }
}