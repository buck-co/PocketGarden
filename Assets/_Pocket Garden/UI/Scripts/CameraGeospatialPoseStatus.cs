using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.XR.ARCoreExtensions;
using UnityEngine.UI;

namespace Buck
{
    public class CameraGeospatialPoseStatus : MonoBehaviour
    {
        // References
        [SerializeField] TMP_Text altitudeText;
        [SerializeField] TMP_Text headingText;
        [SerializeField] TMP_Text latitudeText;
        [SerializeField] TMP_Text longitudeText;
        [SerializeField] TMP_Text headingAccuracyText;
        [SerializeField] TMP_Text verticalAccuracyText;
        [SerializeField] TMP_Text horizontalAccuracyText;
        [SerializeField] RectTransform headingArrow;

        private void Update()
        {
            if (!GeospatialManager.Instance.IsTracking) return;

            altitudeText.text = GeospatialManager.Instance.EarthManager.CameraGeospatialPose.Altitude.ToString("F2");
            headingText.text = GeospatialManager.Instance.EarthManager.CameraGeospatialPose.Heading.ToString("F2");
            latitudeText.text = GeospatialManager.Instance.EarthManager.CameraGeospatialPose.Latitude.ToString("F6");
            longitudeText.text = GeospatialManager.Instance.EarthManager.CameraGeospatialPose.Longitude.ToString("F6");
            headingAccuracyText.text = "Accuracy: " + GeospatialManager.Instance.EarthManager.CameraGeospatialPose.HeadingAccuracy.ToString("F3");
            verticalAccuracyText.text = "Accuracy: " + GeospatialManager.Instance.EarthManager.CameraGeospatialPose.VerticalAccuracy.ToString("F3");
            horizontalAccuracyText.text = "Accuracy: " + GeospatialManager.Instance.EarthManager.CameraGeospatialPose.HorizontalAccuracy.ToString("F3");

            headingArrow.localRotation = Quaternion.Euler(0f, 0f, (float)GeospatialManager.Instance.EarthManager.CameraGeospatialPose.Heading);
        }
    }
}