using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    [RequireComponent(typeof(UI_ProgressSender))]
    public class UI_AlphaTransition : MonoBehaviour
    {
        // References
        [Header("References")]
        [SerializeField] UI_ProgressSender progressSender;
        [SerializeField] CanvasGroup canvasGroup;

        // Properties
        [Header("Properties")]
        [SerializeField] float enabledAlpha = 1f;
        [SerializeField] float disabledAlpha = 0f;

        private float targetAlpha;
        private float startingAlpha;

        [Header("Debug")]
        [SerializeField] bool showLogs;

        private void Start()
        {
            progressSender = GetComponent<UI_ProgressSender>();
            canvasGroup = progressSender.stateResponder.canvasGroup;

            
            canvasGroup.alpha = progressSender.stateResponder.enabledOnStart ? enabledAlpha : disabledAlpha;
            SetupTransition(progressSender.stateResponder.enabledOnStart);
        }

        private void OnEnable()
        {
            progressSender.OnStartTransition.AddListener(SetupTransition);
            progressSender.OnUpdateProgress.AddListener(TransitionAlpha);
        }

        private void OnDisable()
        {
            progressSender.OnStartTransition.RemoveListener(SetupTransition);
            progressSender.OnUpdateProgress.RemoveListener(TransitionAlpha);
        }
        void SetupTransition(bool transitioningToEnabledState) {
            startingAlpha = canvasGroup.alpha;
            targetAlpha = transitioningToEnabledState ? enabledAlpha : disabledAlpha;

        }
        void TransitionAlpha(float progress) {
            canvasGroup.alpha = Mathf.Lerp(startingAlpha, targetAlpha, progress);
            if (showLogs) Debug.Log(progress);
        }

    }
}