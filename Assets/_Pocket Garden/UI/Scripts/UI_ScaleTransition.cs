using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class UI_ScaleTransition : MonoBehaviour
    {
        // References
        [Header("References")]
        [SerializeField] UI_ProgressSender progressSender;
        [SerializeField] RectTransform rectTransform;

        // Properties
        [Header("Properties")]
        [SerializeField] float enabledScale = 1f;
        [SerializeField] float disabledScale = 0f;

        float startingScale;
        float targetScale;
        
        [Header("Debug")]
        [SerializeField] bool showLogs;

        private void Start()
        {
            progressSender.OnStartTransition.AddListener(SetupTransition);
            progressSender.OnUpdateProgress.AddListener(TransitionScale);
        }
        void SetupTransition(bool transitioningToEnabledState)
        {
            startingScale = rectTransform.localScale.x;
            targetScale = transitioningToEnabledState ? enabledScale : disabledScale;

        }
        void TransitionScale(float progress)
        {
            float scale = Mathf.Lerp(startingScale, targetScale, progress);
            rectTransform.localScale = new Vector3(scale, scale, scale);
            if (showLogs) Debug.Log(progress);
        }

    }
}