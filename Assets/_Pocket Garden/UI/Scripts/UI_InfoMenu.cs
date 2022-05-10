using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Buck.PocketGarden
{

    public class UI_InfoMenu : MonoBehaviour
    {

        // Ref
        [Header("References")]
        [SerializeField] RectTransform contentRect;
        [SerializeField] Button closeButton;
        [SerializeField] Button backButton;
        [SerializeField] Button githubButton;
        [SerializeField] Button learnMoreButton;
        [SerializeField] List<Button> sectionButtons;
        [SerializeField] List<GameObject> sections;

        // Properties
        public float animationDuration = .15f;
        public Easings.Functions scrollToContentEasing;
        public Easings.Functions scrollBackEasing;

        // Events
        [HideInInspector] public UnityEvent TransitionToContentPanelCompleted;

        // Vars
        private IEnumerator animator;
        private float backXPosition = 0f ;
        private float contentXPosition = -1080f;


        // Debug
        [Header("Debug")]
        [SerializeField] bool showDebug;

        private void OnDisable()
        {
            contentRect.transform.position = new Vector3(backXPosition, contentRect.transform.position.y, contentRect.transform.position.z);
            HideSections();
        }

        private void Awake()
        {
            for (int i = 0; i < sectionButtons.Count; i++)
            {
                int myBigIndex = i;
                sectionButtons[i].onClick.AddListener(() => SetSection(myBigIndex));
            }

            backButton.onClick.AddListener(StartScrollBack);
            closeButton.onClick.AddListener(CloseInfo);
            githubButton.onClick.AddListener(OpenGithubLink);
            learnMoreButton.onClick.AddListener(OpenLearnMoreLink);

            sections[0].SetActive(true); // 0 = How to vids. It's out of viewport, but we're enabling it so the video player can prepare in the bg
        }

        void SetSection(int index) {
            foreach (GameObject section in sections) {
                section.SetActive(false);
            }
            sections[index].SetActive(true);

            StartScrollToContent();
        }

        void HideSections() {
            foreach (GameObject section in sections)
            {
                section.SetActive(false);
            }
        }

        void CloseInfo() {
            GardenManager.Instance.SetAppState(GardenManager.Instance.PreviousAppState);
        }

        void StartScrollToContent() {

            if (animator != null) StopCoroutine(animator);
            animator = ScrollToContent();
            StartCoroutine(animator);

        }
        void StartScrollBack()
        {
            if(showDebug) Debug.Log("StartScrollBack");

            if (animator != null) StopCoroutine(animator);
            animator = ScrollBack();
            StartCoroutine(animator);
        }

        IEnumerator ScrollToContent()
        {

            if (showDebug) Debug.Log("ScrollToContent");

            float time = 0;
            float startX = contentRect.anchoredPosition.x;

            while (time < animationDuration)
            {
                float progress = time / animationDuration;
                float t = Easings.Interpolate(progress, scrollBackEasing);
                float x = Mathf.Lerp(startX, contentXPosition, t);
                contentRect.anchoredPosition = new Vector2(x, contentRect.anchoredPosition.y);

                time += Time.deltaTime;

                yield return null;
            }

            contentRect.anchoredPosition = new Vector2(contentXPosition, contentRect.anchoredPosition.y);
            TransitionToContentPanelCompleted.Invoke();

            yield return null;

        }

        IEnumerator ScrollBack()
        {
            Debug.Log("StartScrollBack");

            float time = 0;
            float startX = contentRect.anchoredPosition.x;

            while (time < animationDuration)
            {
                float progress = time / animationDuration;
                float t = Easings.Interpolate(progress, scrollBackEasing);
                float x = Mathf.Lerp(startX, backXPosition, t);
                contentRect.anchoredPosition = new Vector2(x, contentRect.anchoredPosition.y);

                time += Time.deltaTime;

                yield return null;
            }
            contentRect.anchoredPosition = new Vector2(backXPosition, contentRect.anchoredPosition.y);

            yield return null;

        }

        void OpenGithubLink() {
            //Application.OpenURL(GardenManager.Instance.RepoUrl + Application.identifier);
            Application.OpenURL(GardenManager.Instance.RepoUrl);
        }

        void OpenLearnMoreLink()
        {
            //Application.OpenURL(GardenManager.Instance.LearnMoreUrl + Application.identifier);
            Application.OpenURL(GardenManager.Instance.LearnMoreUrl);
        }

    }
}