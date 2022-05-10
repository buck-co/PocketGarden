using UnityEngine;
using TMPro;

namespace Buck
{
    public class StatusBar : MonoBehaviour
    {
        [SerializeField] private TMP_Text debugText;
        [SerializeField] private float _holdTime = 1;
        private string _previousText;
        private bool _en,
                     _allowNewValue = true;

        private void Start()
        {
            GeospatialManager.Instance.InitCompleted.AddListener(OnGeoInitCompleted);
            GeospatialManager.Instance.AccuracyImproved.AddListener(OnAccuracyImproved);
        }

        private void OnGeoInitCompleted()
        {
            _en = true;
        }

        private void OnAccuracyImproved()
        {
            Set("Accuracy Improved!", _holdTime);
        }

        private void Update()
        {
            if (!_en) return;

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (GeospatialManager.Instance.IsTracking)
            {
                Set(GeospatialManager.Instance.EarthManager.EarthTrackingState.ToString() + " [Peak]" +
                    " | H: " + GeospatialManager.Instance.BestHorizontalAccuracy.ToString("F1") +
                    " | D: " + GeospatialManager.Instance.BestHeadingAccuracy.ToString("F1") +
                    " | A: " + GeospatialManager.Instance.BestVerticalAccuracy.ToString("F1"));
            }
            else
            {
                Set(GeospatialManager.Instance.EarthManager.EarthTrackingState.ToString() + " [Current]" +
                    " | H: " + GeospatialManager.Instance.EarthManager.CameraGeospatialPose.HorizontalAccuracy.ToString("F1") +
                    " | D: " + GeospatialManager.Instance.EarthManager.CameraGeospatialPose.HeadingAccuracy.ToString("F1") +
                    " | A: " + GeospatialManager.Instance.EarthManager.CameraGeospatialPose.VerticalAccuracy.ToString("F1"));
            }
        }

        private void Set<T>(T value, float holdTime = 0)
        {
            if (!_en) return;

            if (!_allowNewValue)
            {
                if (value.ToString() == _previousText)
                {
                    CancelInvoke();
                    Invoke("Reset", holdTime);
                }

                return;
            }
            else if (holdTime > 0)
            {
                _previousText = debugText.text;
                _allowNewValue = false;
                CancelInvoke();
                Invoke("Reset", holdTime);
            }

            debugText.text = value.ToString();
        }

        private void Reset()
        {
            _allowNewValue = true;
            debugText.text = _previousText;
        }
    }
}