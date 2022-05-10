using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

namespace Buck
{
    /// <summary>
    /// Light Estimation basic implementation
    /// Based on example script from https://github.com/Unity-Technologies/arfoundation-samples
    /// </summary>
    public class LightEstimation : MonoBehaviour
    {
        [SerializeField] private ARCameraManager _arCameraManager;
        [SerializeField] Light _mainLight;
        [SerializeField] Camera _camera;

        /// <summary>
        /// The estimated brightness of the physical environment, if available.
        /// </summary>
        public float? brightness { get; private set; }
        /// <summary>
        /// The estimated color temperature of the physical environment, if available.
        /// </summary>
        public float? colorTemperature { get; private set; }
        /// <summary>
        /// The estimated color correction value of the physical environment, if available.
        /// </summary>
        public Color? colorCorrection { get; private set; }
        /// <summary>
        /// The estimated direction of the main light of the physical environment, if available.
        /// </summary>
        public Vector3? mainLightDirection { get; private set; }
        /// <summary>
        /// The estimated color of the main light of the physical environment, if available.
        /// </summary>
        public Color? mainLightColor { get; private set; }
        /// <summary>
        /// The estimated intensity in lumens of main light of the physical environment, if available.
        /// </summary>
        public float? mainLightIntensityLumens { get; private set; }
        /// <summary>
        /// The estimated spherical harmonics coefficients of the physical environment, if available.
        /// </summary>
        public SphericalHarmonicsL2? sphericalHarmonics { get; private set; }

        void OnEnable()
        {
            if (_arCameraManager != null)
                _arCameraManager.frameReceived += FrameChanged;
        }
        void OnDisable()
        {
            if (_arCameraManager != null)
                _arCameraManager.frameReceived -= FrameChanged;
        }

        void FrameChanged(ARCameraFrameEventArgs args)
        {
            if (args.lightEstimation.averageBrightness.HasValue)
            {
                brightness = args.lightEstimation.averageBrightness.Value;
                _mainLight.intensity = brightness.Value;
            }
            if (args.lightEstimation.averageColorTemperature.HasValue)
            {
                colorTemperature = args.lightEstimation.averageColorTemperature.Value;
                _mainLight.colorTemperature = colorTemperature.Value;
            }

            if (args.lightEstimation.colorCorrection.HasValue)
            {
                colorCorrection = args.lightEstimation.colorCorrection.Value;
                _mainLight.color = colorCorrection.Value;
            }
            if (args.lightEstimation.mainLightDirection.HasValue)
            {
                mainLightDirection = args.lightEstimation.mainLightDirection;
                _mainLight.transform.rotation = Quaternion.LookRotation(mainLightDirection.Value);
            }
            if (args.lightEstimation.mainLightColor.HasValue)
            {
                mainLightColor = args.lightEstimation.mainLightColor;

#if PLATFORM_ANDROID
                // ARCore needs to apply energy conservation term (1 / PI) and be placed in gamma
                _mainLight.color = mainLightColor.Value / Mathf.PI;
                _mainLight.color = _mainLight.color.gamma;

                // ARCore returns color in HDR format (can be represented as FP16 and have values above 1.0)
                if (_camera == null || !_camera.allowHDR)
                {
                    Debug.LogWarning($"HDR Rendering is not allowed.  Color values returned could be above the maximum representable value.");
                }
#endif
            }
            if (args.lightEstimation.mainLightIntensityLumens.HasValue)
            {
                mainLightIntensityLumens = args.lightEstimation.mainLightIntensityLumens;
                _mainLight.intensity = args.lightEstimation.averageMainLightBrightness.Value;
            }
            if (args.lightEstimation.ambientSphericalHarmonics.HasValue)
            {
                sphericalHarmonics = args.lightEstimation.ambientSphericalHarmonics;
                RenderSettings.ambientMode = AmbientMode.Skybox;
                RenderSettings.ambientProbe = sphericalHarmonics.Value;
            }
        }
    }
}
