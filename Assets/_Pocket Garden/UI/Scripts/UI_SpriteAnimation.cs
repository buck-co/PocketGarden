using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Buck.PocketGarden
{
    public class UI_SpriteAnimation : MonoBehaviour
    {

        [Header("References")]
        public Image sprite;

        [Header("Properties")]
        [SerializeField] string resourcesPath;
        [SerializeField] string spriteBaseName;
        [SerializeField] float fps = 12f;
        [SerializeField] int totalFrames;
        [SerializeField] int loopCount = 1;
        [SerializeField] bool endless;

        // Events
        [HideInInspector] public UnityEvent looped;
        [HideInInspector] public UnityEvent finishedLooping;

        // vars
        private float frameDuration;
        private int currentLoop;
        private int currentFrame;

        private void Awake()
        {
            frameDuration = 1 / fps;
        }
        void OnEnable()
        {
            if (endless) { StartCoroutine(PlayEndlessLoop()); }
            else { StartCoroutine(PlayAnimation()); }
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator PlayAnimation()
        {
            while (currentLoop < loopCount)
            {
                if (currentFrame >= totalFrames)
                {
                    currentLoop++;
                    currentFrame = 0;
                    looped.Invoke();
                }

                sprite.sprite = Resources.Load<Sprite>($"{resourcesPath}/{spriteBaseName}{currentFrame}");

                currentFrame++;
                yield return new WaitForSeconds(frameDuration);
                
            }

            finishedLooping.Invoke();

            yield return null;

        }

        IEnumerator PlayEndlessLoop()
        {
            while (true)
            {
                if (currentFrame >= totalFrames)
                {
                    currentFrame = 0;
                    looped.Invoke();
                }

                sprite.sprite = Resources.Load<Sprite>($"{resourcesPath}/{spriteBaseName}{currentFrame}");

                currentFrame++;
                yield return new WaitForSeconds(frameDuration);
            }

        }

    }
}