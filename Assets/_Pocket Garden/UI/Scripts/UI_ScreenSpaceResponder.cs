using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Buck.PocketGarden
{

    public class UI_ScreenSpaceResponder : MonoBehaviour
    {

        [Header("References")]
        public SeedIcon seedIcon;
        public RectTransform constrained;
        public CanvasGroup canvasGroup;
        public ScrollRect scrollRect;

        [Header("Scale")]
        public bool animateScale;
        public float minScale = .75f;
        public float maxScale = 1f;
        public AnimationCurve scaleCurve;
        private float initScale;

        [Header("Alpha")]
        public bool animateAlpha;
        public float minAlpha = .55f;
        public float maxAlpha = 1f;
        public AnimationCurve alphaCurve;

        [Header("Offset Y Pivot")]
        public bool offsetYPivot;
        public float targetMaxOffset = .5f;
        public AnimationCurve offsetYPivotCurve;
        private float initYPivot;

        [Header("Scroll Dampening")]
        public bool dampenScrolling;
        public float stickySpot = .01f;
        public float maxDecelerationRate;
        public float minDecelerationRate;
        public AnimationCurve dampCurve;

        // Vars
        //private int width;
        private RectTransform sourceRect;
        
        void Start()
        {
            sourceRect = GetComponent<RectTransform>();
            initScale = constrained.localScale.x;
            initYPivot = constrained.pivot.y;
            maxDecelerationRate = scrollRect.decelerationRate;

            //width = Screen.width;
        }

        
        void Update()
        {
            float t = sourceRect.position.x / Screen.width;

            if (animateScale) {
                //float scale = initScale + (scaleExpansion * scaleCurve.Evaluate(t));
                float scale = initScale * Mathf.Lerp(minScale, maxScale, scaleCurve.Evaluate(t));
                constrained.localScale = new Vector3(scale, scale, scale);
            }

            if (animateAlpha)
            {
                float progress = alphaCurve.Evaluate(t);
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, progress);

                if (seedIcon.HasSeeds)
                {
                    canvasGroup.alpha = alpha;
                }
                else {
                    canvasGroup.alpha = minAlpha;
                }
            }

            if (dampenScrolling)
            {
                //float progress = dampCurve.Evaluate(t);
                //float decelRate = Mathf.Lerp(maxDecelerationRate, minDecelerationRate, progress);

                //if (decelRate > 0) scrollRect.decelerationRate = decelRate;
            }

            if (offsetYPivot)
            {
                float progress = offsetYPivotCurve.Evaluate(t);
                float yOffset = Mathf.Lerp(0, targetMaxOffset, progress);

                constrained.pivot = new Vector2(constrained.pivot.x, initYPivot + yOffset);
            }


        }

    }
}