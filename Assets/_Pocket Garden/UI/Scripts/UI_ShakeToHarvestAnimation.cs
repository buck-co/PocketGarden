using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Buck.PocketGarden
{
    public class UI_ShakeToHarvestAnimation : MonoBehaviour
    {

        public Image sprite;
        public float fps = 12f;
        public int howManyTimesToLoopBeforeHiding = 2;

        private float frameDuration;
        private int currentLoop;
        private int currentFrame;
        private int totalFrames = 25;

        private void Awake()
        {
            frameDuration = fps / 60;
        }

        void OnEnable() {
            StartCoroutine(PlayAnimation());
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator PlayAnimation() {

            while (currentLoop < howManyTimesToLoopBeforeHiding) {

                if (currentFrame >= totalFrames) {
                    currentLoop++;
                    currentFrame = 0;
                }

                sprite.sprite = Resources.Load<Sprite>($"First Run Harvest Sequence/hand_asset_12fps_v03_{currentFrame}");

                currentFrame++;
                yield return new WaitForSeconds(frameDuration);
            }

            UI_FirstRunState.Instance.CompleteFirstHarvesting();

            gameObject.transform.parent.gameObject.SetActive(false);
            

            yield return null;

        }

        //private void Update()
        //{
        //    if (currentFrame == totalFrames)
        //        currentFrame = 0;
        //}

    }
}