using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Buck.PocketGarden
{
    public class UI_ProgressSender : MonoBehaviour
    {

        // References
        [Header("References")]
        public UI_StateResponder stateResponder;

        // Properties
        [Header("Properties")]
        public float animationDuration = .15f;
        public float Progress { get { return _progress; } }

        public bool animateOnStart;
        public bool toggleVisibility;
        public bool toggleInteractivity;
        public bool toggleEnabled;
        public bool reverseToggle;

        public Easings.Functions showEasing;
        public Easings.Functions hideEasing;
        

        // Events
        [HideInInspector] public UnityEvent<float> OnUpdateProgress;
        [HideInInspector] public UnityEvent<bool> OnStartTransition; // True = Transitioning to enabled state
        [HideInInspector] public UnityEvent<bool> OnFinished;

        // Vars
        private float _progress;
        private IEnumerator progressAnimator;
        private Easings.Functions currentEasing;

        [Header("Debug")]
        [SerializeField] bool showLogs;
        

        private void Start()
        {
            Subscribe();
        }

        public virtual void Subscribe() {
            if (stateResponder == null) stateResponder = GetComponent<UI_StateResponder>();
            if (stateResponder != null) stateResponder.StateUpdatedEvent.AddListener(StartProgress);
            if (stateResponder != null) if (animateOnStart) StartProgress(stateResponder.enabledOnStart);
        }
        public void StartAnimateProgress()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(AnimateProgress());
        }
        void StartProgress(bool isEnabled) {

            if (showLogs) Debug.Log("StartProgress("+ isEnabled+")", this);

            if (reverseToggle)
                isEnabled = !isEnabled;

            if (isEnabled)
            {
                if (gameObject.activeInHierarchy)
                    StartCoroutine(AnimateOn());
            }
            else {
                if(gameObject.activeInHierarchy)
                    StartCoroutine(AnimateOff());
            }
        }

        public IEnumerator AnimateProgress() {

            OnStartTransition.Invoke(true);
            yield return new WaitForEndOfFrame();

            currentEasing = showEasing;
            if (progressAnimator != null) StopCoroutine(progressAnimator);
            progressAnimator = ProgressAnimator();

            yield return progressAnimator;

            OnFinished.Invoke(true);

            yield return null;
        }

        IEnumerator AnimateOn() {

            if (showLogs) Debug.Log("AnimateOn()", this);

            // Enable game object
            if (showLogs) Debug.Log("Enable Game Object", this);
            if (toggleEnabled) stateResponder.canvasGroup.gameObject.SetActive(true);
            if(toggleVisibility) stateResponder.SetVisibility(true);

            // Invoke OnStartTransition Event
            if (showLogs) Debug.Log("Invoke OnStartTransition", this);
            OnStartTransition.Invoke(true);

            // Wait for end of frame (I wanna be sure we have the correct to/from transition values before starting. Might remove)
            if (showLogs) Debug.Log("Wait for end of frame", this);
            yield return new WaitForEndOfFrame();

            // Set up an animation progress coroutine
            if (showLogs) Debug.Log("Create AnimateProgress coroutine", this);
            currentEasing = showEasing;
            if (progressAnimator != null) StopCoroutine(progressAnimator);
            progressAnimator = ProgressAnimator();

            // Yield while progress animates
            if (showLogs) Debug.Log("Wait for AnimateProgress", this);
            yield return progressAnimator;

            OnFinished.Invoke(true);

            // Make UI interactable
            if (showLogs) Debug.Log("Make object interactable", this);
            if (toggleInteractivity) stateResponder.SetInteractibility(true);

            yield return null;
        }

        IEnumerator AnimateOff()
        {

            if (showLogs) Debug.Log("AnimateOff()", this);

            // Make UI non-interactable
            if (showLogs) Debug.Log("Make UI non-interactable", this);
            if (toggleInteractivity) stateResponder.SetInteractibility(false);

            // Invoke OnStartTransition Event
            if (showLogs) Debug.Log("Invoke OnStartTransition", this);
            OnStartTransition.Invoke(false);

            // Wait for end of frame
            if (showLogs) Debug.Log("Wait for end of frame", this);
            yield return new WaitForEndOfFrame();

            // Set up an animation progress coroutine
            if (showLogs) Debug.Log("Create AnimateProgress coroutine", this);
            currentEasing = hideEasing;
            if (progressAnimator != null) StopCoroutine(progressAnimator);
            progressAnimator = ProgressAnimator();

            // Yield while progress animates
            if (showLogs) Debug.Log("Wait for AnimateProgress", this);
            yield return progressAnimator;

            OnFinished.Invoke(false);

            // Disable game object
            if (showLogs) Debug.Log("Disable Game Object", this);
            if (toggleVisibility) stateResponder.SetVisibility(false);
            if (toggleEnabled) stateResponder.canvasGroup.gameObject.SetActive(false);

            yield return null;
        }

        IEnumerator ProgressAnimator()
        {
            if (showLogs) Debug.Log("AnimateProgress Start", this);

            float time = 0;
            while (time < animationDuration)
            {
                float progress = time / animationDuration;
                float t = Easings.Interpolate(progress, currentEasing);

                _progress = t;

                OnUpdateProgress.Invoke(t);

                // Logs
                if (showLogs) Debug.Log(_progress, this);

                time += Time.deltaTime;

                yield return null;
            }

            _progress = 1f;

            OnUpdateProgress.Invoke(_progress);

            // Logs
            if (showLogs) Debug.Log(_progress, this);
            if (showLogs) Debug.Log("Invoke OnFinished()", this);

            yield return null;

        }
    }
}