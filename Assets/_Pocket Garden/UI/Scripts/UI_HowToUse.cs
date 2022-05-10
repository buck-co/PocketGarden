using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

namespace Buck.PocketGarden
{
    public class UI_HowToUse : MonoBehaviour
    {
        // Refs
        [Header("References")]
        [SerializeField] UI_InfoMenu infoMenu;
        [SerializeField] Image sprite;
        [SerializeField] List<string> pathAndBaseSpriteNames = new List<string>();
        [SerializeField] List<Vector2> loopFrames = new List<Vector2>();
        [SerializeField] List<Button> tabs = new List<Button>();
        [SerializeField] Button previousTabButton;
        [SerializeField] Button nextTabButton;

        // Props
        [Header("Properties")]
        [SerializeField] float fps = 12f;

        // Vars
        private int currentTab = 0;
        private float frameDuration;
        private int currentFrame;

        // Debug
        public bool showLogs;

        private void Awake()
        {
            frameDuration = 1 / fps;
        }

        private void Start()
        {
            // Subscribe to tab onClick events
            for (int i = 0; i < tabs.Count; i++)
            {
                int myBigIndex = i;
                tabs[i].onClick.AddListener(() => PlayAnimation(myBigIndex));
            }

            // Subscribe to next/previous tab onClick
            previousTabButton.onClick.AddListener(SetPreviousTab);
            nextTabButton.onClick.AddListener(SetNextTab);

        }

        private void OnEnable()
        {
            PlayAnimation(0);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        void SetNextTab() {

            if (showLogs) Debug.Log("SetNextTab");

            if (currentTab >= tabs.Count)
                return;

            StopAllCoroutines();
            PlayAnimation(currentTab+1);
        }

        void SetPreviousTab() {

            if(showLogs) Debug.Log("SetPreviousTab");

            if (currentTab <= 0)
                return;

            StopAllCoroutines();
            PlayAnimation(currentTab - 1);
        }

        private void PlayAnimation(int index)
        {
            if (showLogs) Debug.Log("SetVideoPlayer " + index);

            currentTab = index;
            StopAllCoroutines();
            StartCoroutine(Animate());
        }

        IEnumerator Animate()
        {
            currentFrame = 0;

            while (true)
            {
                if (currentFrame >= loopFrames[currentTab].y) // loopFrames[currentTab].y <--- this is the last frame of each anim sequence
                {
                    currentFrame = (int)loopFrames[currentTab].x; // <--- this is the start of the looped section
                }

                sprite.sprite = Resources.Load<Sprite>($"{pathAndBaseSpriteNames[currentTab]}{currentFrame}");
                if (showLogs) Debug.Log($"{pathAndBaseSpriteNames[currentTab]}{currentFrame}");

                currentFrame++;
                yield return new WaitForSeconds(frameDuration);
                yield return 0;
            }

        }
    }
}