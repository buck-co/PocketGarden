using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    [RequireComponent(typeof(UI_ProgressSender))]
    public class UI_RectTransformHeightTransitioner : MonoBehaviour
    {
        // Refernces
        [Header("References")]
        [SerializeField] UI_ProgressSender progressSender;
        public RectTransform rectTransform;

        // Properties
        [Header("Properties")]
        [SerializeField] float enabledHeight = 1f;
        [SerializeField] float disabledHeight = 0f;

        [HideInInspector] public float targetHeight;
        [HideInInspector] public float startingHeight;

        [Header("Debug")]
        public bool showLogs;

        private void Start()
        {
            InitTransform(progressSender.stateResponder.enabledOnStart);
        }

        public virtual void InitTransform(bool enabledOnStart)
        {

            float height = enabledOnStart ? enabledHeight : disabledHeight;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            // Logs
            if (showLogs) Debug.Log("InitAnchor");
            if (showLogs) Debug.Log(enabledOnStart, this);
            if (showLogs) Debug.Log(height, this);
        }

        private void OnEnable()
        {
            progressSender.OnStartTransition.AddListener(SetupTransition);
            progressSender.OnUpdateProgress.AddListener(TransitionHeight);
        }

        private void OnDisable()
        {
            progressSender.OnStartTransition.RemoveListener(SetupTransition);
            progressSender.OnUpdateProgress.RemoveListener(TransitionHeight);
        }

        public virtual void SetupTransition(bool transitioningToEnabledState)
        {
            startingHeight = rectTransform.rect.height;
            targetHeight = transitioningToEnabledState ? enabledHeight : disabledHeight;

            // Logs
            if (showLogs) Debug.Log("SetupTransition");
            if (showLogs) Debug.Log("transitioningToEnabledState " + transitioningToEnabledState);
            if (showLogs) Debug.Log("startingY " + startingHeight);
            if (showLogs) Debug.Log("targetY " + targetHeight);
        }

        void TransitionHeight(float progress)
        {
            float height = Mathf.Lerp(startingHeight, targetHeight, progress);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            // Logs
            if (showLogs) Debug.Log("TransitionHeight" + progress, this);
            if (showLogs) Debug.Log("height " + height, this);
            if (showLogs) Debug.Log("rectTransform.rect.height " + rectTransform.rect.height, this);
        }

    }
}