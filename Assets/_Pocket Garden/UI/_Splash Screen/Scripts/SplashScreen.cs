using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Buck.PocketGarden
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField]
        private Material packageMaterial;
        public Material PlasticBackground;
        public Image DetachImage;
        public Button learnMoreButton;

        Sprite sprite;
        int lastVibrate;
        float minTouch = 110;
        float firstFrame = 1;
        float lastFrame = 14;
        float SpriteNumber = 0;
        bool firstTouch = true;
        bool packageOpen = false;

        public GameObject packageFloor;
        public GameObject packageParent;

        void Update()
        {
            ParallaxElements();
            RipPackage();
        }

        void Start()
        {
            GeospatialManager.Instance.ErrorStateChanged.AddListener(OnErrorStateUpdated);

            Input.gyro.enabled = false;
            Input.gyro.enabled = true;

            if (learnMoreButton != null)
                learnMoreButton.onClick.AddListener(OpenLearnMoreLink);
        }
        void OnErrorStateUpdated(ErrorState errorState, string message)
        {
            // If this is a fatal errror, hide splash screen
            if (message != null)
                gameObject.SetActive(false);
        }
        void ParallaxElements()
        {
            float tilt = remap(Input.gyro.attitude.x, 0.2f, 0.90f, -1.04f, 1.04f);
            float turn = remap(Input.gyro.attitude.y, -0.3f, 0.3f, -1.04f, 1.04f);
            float turnBackPlastic = remap(Input.gyro.attitude.y, -0.3f, 0.3f, -0.1f, 0.71f);
            packageMaterial.SetFloat("_Tilt", tilt);
            packageMaterial.SetFloat("_Turn", turn);
        }
        float remap(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        float ScreenPercentage(float touchX, float screen)
        {
            return ((screen - touchX) / screen) * 100;
        }
        void RipPackage()
        {
            if (Input.touchCount > 0 && !packageOpen)
            {
                Touch touch = Input.GetTouch(0);
                float touchX = touch.position.x;
                float touchY = touch.position.y;
                float percentScreen = ScreenPercentage(touchX, Screen.width);
                bool touchingBottom = ScreenPercentage(touchY, Screen.height) > 80 ? true : false;
                bool touchingLeft = ScreenPercentage(touchX, Screen.width) > 80 ? true : false;

                if (firstTouch && touchingLeft && touchingBottom)
                {
                    firstTouch = false;
                }
                if (touchingBottom && !firstTouch && percentScreen < minTouch)
                {
                    SpriteNumber = remap(percentScreen, 100, 0, firstFrame, lastFrame);
                    LoadSprite((int)Mathf.Round(SpriteNumber));
                    minTouch = percentScreen;
                }
                return;
            }

            if (SpriteNumber > lastFrame / 2)
            {
                animatedSprite();
            }
        }

        void animatedSprite()
        {
            if (SpriteNumber < lastFrame)
            {
                SpriteNumber += Time.deltaTime * 20f;
                LoadSprite((int)Mathf.RoundToInt(SpriteNumber));
            }
            else
            {
                LoadSprite(0);
                DispenseSeeds();
                packageOpen = true;
            }
        }

        void DispenseSeeds()
        {

            if (packageOpen)
                return;

            packageFloor.SetActive(false);
            GardenManager.Instance.StartDismissSplashScreen();
        }
        void LoadSprite(int id)
        {

            if (id > lastFrame)
                return;

            string tempId = id.ToString();
            sprite = Resources.Load<Sprite>($"packageRip/packageRip_{tempId}");
            DetachImage.sprite = sprite;
            GardenManager.Instance.PlaySound(tempId);

            if (id != lastVibrate)
            {
                InteractionManager.Instance.VibrateOnce();
                lastVibrate = id;
            }

        }

        void OpenLearnMoreLink()
        {
            Application.OpenURL(GardenManager.Instance.LearnMoreUrl);
        }

        void OnDestroy()
        {
            Destroy(packageParent);
        }
    }
}