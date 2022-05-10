using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Buck.PocketGarden
{
    public class SeedIcon : MonoBehaviour
    {
        /// References
        [Header("References")]
        [SerializeField] Image _icon;
        [SerializeField] TMP_Text _quantityText;
        [SerializeField] TMP_Text _indexDebugText;
        [SerializeField] RectTransform _radialProgress;
        public Button button;
        [SerializeField] Color _defaultColor;
        [SerializeField] Color _disabledColor;

        [Header("Radial Progress")]
        [Range(0, 100)]
        public float fillValue = 0;
        public Image circleFillImage;
        public RectTransform handlerEdgeImage;
        public RectTransform fillHandler;

        // Properties
        public Plant Plant { set { _plant = value; } }
        public Image Icon { get { return _icon; } }
        public bool HasSeeds { get { return _hasSeeds; } }
        public int _index;

        // Vars
        public bool _hasSeeds;
        private UI_ProgressSender progressSender;
        private float targetRadialProgress;
        private float startingRadialProgress;
        private int currentInventoryCount;
        private Plant _plant;

        [Header("Debug")]
        public bool showLogs;

        private void Awake()
        {
            progressSender = GetComponent<UI_ProgressSender>();
        }

        private void Start()
        {
            currentInventoryCount = GardenManager.Instance.Items[_index].quantity;

            _hasSeeds = currentInventoryCount > 0;

            SetSeedIcon();

            UpdateLabel(GardenManager.Instance.Items);
            GardenManager.Instance.InventoryUpdated.AddListener(UpdateLabel);

            _indexDebugText.text = _index.ToString();

            progressSender.OnStartTransition.AddListener(SetupTransition);
            progressSender.OnUpdateProgress.AddListener(TransitionRadialProgress);

            startingRadialProgress = NormalizedQuantity();
            targetRadialProgress = startingRadialProgress;

            SetProgressValue(startingRadialProgress);
            SetButtonState(true);
        }
        float NormalizedQuantity() {
           
            return (float)GardenManager.Instance.Items[_index].quantity / (float)GardenManager.Instance.StartingSeedQuantity;
        }

        void UpdateLabel(List<ItemData> items)
        {
            foreach (ItemData item in items) {
                if (_index == item.index) {

                    currentInventoryCount = item.quantity;
                    _quantityText.text = item.quantity.ToString();

                    if (item.quantity == 0 && button.interactable) SetButtonState(false);
                    if (item.quantity > 0 && !button.interactable) SetButtonState(true);

                    _hasSeeds = currentInventoryCount > 0;
                    SetSeedIcon();

                    targetRadialProgress = NormalizedQuantity();

                    if (progressSender) progressSender.StartAnimateProgress();
                }
                
            }
        }

        void SetSeedIcon() {
            _icon.sprite = _hasSeeds ? _plant.Config.InventoryIcon : _plant.Config.InventoryIconDisabled;
        }

        void SetButtonState(bool enabled) {
            _radialProgress.gameObject.SetActive(enabled);
            button.interactable = enabled;
            _icon.color = enabled ? _defaultColor : _disabledColor;
        }

        void SetupTransition(bool enabling) {

            startingRadialProgress = circleFillImage.fillAmount;
            targetRadialProgress = NormalizedQuantity();

            if (showLogs) Debug.Log("SetupTransition from " + startingRadialProgress + " to " + targetRadialProgress);
        }

        void TransitionRadialProgress(float progress) {
            float radProg = Mathf.Lerp(startingRadialProgress, targetRadialProgress, progress);

            SetProgressValue(radProg);
        }

        void SetProgressValue(float value)
        {
            float fillAmount = value;
            circleFillImage.fillAmount = fillAmount;

            float angle = fillAmount * 360;

            fillHandler.localEulerAngles = new Vector3(0, 0, -angle);
            handlerEdgeImage.localEulerAngles = new Vector3(0, 0, angle);
        }

    }
}