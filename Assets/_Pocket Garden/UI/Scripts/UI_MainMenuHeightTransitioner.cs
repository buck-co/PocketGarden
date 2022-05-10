using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{

    public class UI_MainMenuHeightTransitioner : MonoBehaviour
    {
        // Refernces
        [Header("References")]
        public RectTransform rectTransform;
        public UI_ProgressSender progressSender;
        public CanvasGroup canvasGroup;

        // Properties
        [Header("Height Definitions")]
        [SerializeField] float minimizedHeight = 160f;
        [SerializeField] float expandedHeight = 840f;

        // Vars
        float targetHeight;
        float startingHeight;

        [Header("Debug")]
        public bool showLogs;

        private void Start()
        {
            InitTransform(UI_MainMenu.Instance.CurrentMenuState);
            UI_MainMenu.Instance.MenuStateChanged.AddListener(StartTransitionHeight);
            progressSender.OnUpdateProgress.AddListener(TransitionHeight);
        }

        void StartTransitionHeight(MainMenuState menuState) {
            SetupTransition(menuState);
            progressSender.StartAnimateProgress();
        }

        private void InitTransform(MainMenuState menuState)
        {
            float height = 100;

            switch (menuState)
            {
                case MainMenuState.Expanded:
                    height = expandedHeight;
                    break;
                case MainMenuState.Minimized:
                    height = minimizedHeight;
                    break;
            }

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        }
        public virtual void SetupTransition(MainMenuState menuState)
        {

            // Set starting hight
            startingHeight = rectTransform.rect.height;

            // Set target height
            switch (menuState)
            {
                case MainMenuState.Expanded:
                    targetHeight = expandedHeight;
                    break;
                case MainMenuState.Minimized:
                    targetHeight = minimizedHeight;
                    break;
            }
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