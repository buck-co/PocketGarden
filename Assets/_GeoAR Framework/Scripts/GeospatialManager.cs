using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;
using UnityEngine.Android;

/// <summary>
/// Singleton Class
/// Handles initialization and error state management of the AR Core Session, AR Core Extensions, and Earth Manager.
/// Contains public properties for and events raised on error state and geospatial tracking/accuracy changes.
/// Public methods are provided for getting new Geospatial Anchors.
/// </summary>
namespace Buck
{
    /// <summary>
    /// All possible error states. Used to inform other components' behaviors.
    /// </summary>
    [System.Flags] public enum ErrorState { Null = 0, NoError = 1, Tracking = 2, Message = 4, Camera = 8, Location = 16 }

    public class GeospatialManager : Singleton<GeospatialManager>
    {
        [Header("[ AR Components ]")]
        public ARSessionOrigin SessionOrigin;
        public ARSession Session;
        public ARAnchorManager AnchorManager;
        public AREarthManager EarthManager;
        public ARCoreExtensions ARCoreExtensions;

        /// <summary>
        /// True while Earth Manager is tracking and accuracy minimums are met
        /// </summary>
        public bool IsTracking { get => _trackingValid; }

        /// <summary>
        /// True once we've reached target accuracy and for the remainder of the session
        /// </summary>
        public bool IsAccuracyTargetReached { get => _targetAccuracyReached; }

        /// <summary>
        /// The current error message, if there is one
        /// </summary>
        public string CurrentErrorMessage { get => _errorMessage; }

        /// <summary>
        /// Best horizontal accuracy value reached at any point during the current session
        /// </summary>
        public double BestHorizontalAccuracy { get => _bestHorizontalAccuracy; }

        /// <summary>
        /// Best heading accuracy value reached at any point during the current session
        /// </summary>
        public double BestHeadingAccuracy { get => _bestHeadingAccuracy; }

        /// <summary>
        /// Best altitude accuracy value reached at any point during the current session
        /// </summary>
        public double BestVerticalAccuracy { get => _bestVerticalAccuracy; }

        /// <summary>
        /// Current error state enum
        /// </summary>
        public ErrorState CurrentErrorState { get => _errorState; }

        /// <summary>
        /// Raised once when all components are ready
        /// </summary>
        [HideInInspector] public UnityEvent InitCompleted;

        /// <summary>
        /// Raised on any frame that accuracy has reached better values than any previous 
        /// </summary>
        [HideInInspector] public UnityEvent AccuracyImproved;

        /// <summary>
        /// Raised once when the specified target accuracy values are reached
        /// </summary>
        [HideInInspector] public UnityEvent TargetAccuracyReached;

        /// <summary>
        /// Raised on any frame that there is a change in error state
        /// Includes the error state enum and error message string if applicable
        /// </summary>
        [HideInInspector] public UnityEvent<ErrorState, string> ErrorStateChanged;

        [Header("[ Accuracy Minimums ] - Required to start experience")]
        [SerializeField] private float _minimumHorizontalAccuracy = 10;
        [SerializeField] private float _minimumHeadingAccuracy = 15;
        [SerializeField] private float _minimumVerticalAccuracy = 1.5f;

        [Header("[ Accuracy Targets ] - Event raised when reached")]
        [SerializeField] private float _targetHorizontalAccuracy = 1;
        [SerializeField] private float _targetHeadingAccuracy = 2;
        [SerializeField] private float _targetVerticalAccuracy = 0.5f;

        [Header("[ Settings ]")]
        [SerializeField] private float _initTime = 3f;

        private ErrorState _errorState = ErrorState.NoError;
        private string _errorMessage;
        private double _bestHorizontalAccuracy = Mathf.Infinity;
        private double _bestHeadingAccuracy = Mathf.Infinity;
        private double _bestVerticalAccuracy = Mathf.Infinity;
        private bool _trackingValid,
                     _enablingGeospatial,
                     _initComplete,
                     _targetAccuracyReached,
                     _requestCamPerm,
                     _requestLocPerm,
                     _startedAR;

        private void Start()
        {
            SetErrorState(ErrorState.NoError);

#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("Start location services.");
            Input.location.Start();
#endif
        }

        private void Update()
        {
            if (!CheckCameraPermission())
                return;

            if (!CheckLocationPermission())
                return;

            if (!_startedAR)
            {
                SessionOrigin.gameObject.SetActive(true);
                Session.gameObject.SetActive(true);
                ARCoreExtensions.gameObject.SetActive(true);
                _startedAR = true;
            }

            UpdateSessionState();

            if (ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            FeatureSupported featureSupport = EarthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);

            switch (featureSupport)
            {
                case FeatureSupported.Unknown:
                    SetErrorState(ErrorState.Message, "Geospatial API encountered an unknown error.");
                    return;
                case FeatureSupported.Unsupported:
                    SetErrorState(ErrorState.Message, "Geospatial API is not supported by this device.");
                    enabled = false;
                    return;
                case FeatureSupported.Supported:
                    if (ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode == GeospatialMode.Disabled)
                    {
                        Debug.Log("Enabling Geospatial Mode...");
                        ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode =
                            GeospatialMode.Enabled;
                        _enablingGeospatial = true;
                        return;
                    }
                    break;
            }

            /// Waiting for new configuration taking effect
            if (_enablingGeospatial)
            {
                _initTime -= Time.deltaTime;

                if (_initTime < 0)
                    _enablingGeospatial = false;
                else
                    return;
            }

            /// Check earth state
            EarthState earthState = EarthManager.EarthState;

            if (earthState != EarthState.Enabled)
            {
                SetErrorState(ErrorState.Message, "Error: Unable to start Geospatial AR");
                enabled = false;
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
            bool isSessionReady = ARSession.state == ARSessionState.SessionTracking &&
            Input.location.status == LocationServiceStatus.Running;
#else
            bool isSessionReady = ARSession.state == ARSessionState.SessionTracking;

#endif

            /// **** Init Complete ****
            if (!_initComplete)
            {
                InitCompleted.Invoke();
                _initComplete = true;
                SetErrorState(ErrorState.Tracking);
            }

            if (TrackingIsValid())
            {
                if (CheckAccuracyImproved())
                {
                    /// Raise event if accuracy has improved since last check
                    AccuracyImproved.Invoke();
                }

                if (!_targetAccuracyReached && CheckTargetAccuracyReached())
                {
                    Debug.Log("** Target Accuracy Reached!! **");
                    /// Raise event if target accuracy reached
                    TargetAccuracyReached.Invoke();
                    _targetAccuracyReached = true;
                }
            }
        }

        /// <summary>
        /// Ensure we have Camera usage permission
        /// </summary>
        /// <returns></returns>
        private bool CheckCameraPermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                if (_errorState != ErrorState.Camera)
                    SetErrorState(ErrorState.Camera);


                if (!_requestCamPerm) Permission.RequestUserPermission(Permission.Camera);
                _requestCamPerm = true;
                return false;
            }

            if (_errorState == ErrorState.Camera)
                SetErrorState(ErrorState.NoError);

            return true;
        }

        /// <summary>
        /// Ensure we have Location usage permission
        /// </summary>
        /// <returns></returns>
        private bool CheckLocationPermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                if (_errorState != ErrorState.Location)
                    SetErrorState(ErrorState.Location);

                if (!_requestLocPerm) Permission.RequestUserPermission(Permission.FineLocation);
                _requestLocPerm = true;
                return false;
            }

            if (_errorState == ErrorState.Location)
                SetErrorState(ErrorState.NoError);

            return true;
        }

        /// <summary>
        /// Monitor the state of the AR session
        /// </summary>
        private void UpdateSessionState()
        {
            /// Pressing 'back' button quits the app.
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }

            /// Only allow the screen to sleep when not tracking.
            var sleepTimeout = SleepTimeout.NeverSleep;
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }

            Screen.sleepTimeout = sleepTimeout;

            /// ARSession Status
            if (ARSession.state != ARSessionState.CheckingAvailability &&
                ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                Debug.Log("ARSession error state: " + ARSession.state);
                SetErrorState(ErrorState.Message, "AR Error Encountered: " + ARSession.state);
                enabled = false;
            }

#if UNITY_IOS && !UNITY_EDITOR
            else if (Input.location.status == LocationServiceStatus.Failed)
            {
                SetErrorState(ErrorState.Message, "Please start the app again and grant precise location permission.");
            }
#endif
            else if (SessionOrigin == null || Session == null || ARCoreExtensions == null)
            {
                Debug.Log("Missing AR Components.");
                SetErrorState(ErrorState.Message, "Error: Something Went Wrong");
                return;
            }
        }

        /// <summary>
        /// Set error state and raise event if needed
        /// </summary>
        /// <param name="errorState"></param>
        /// <param name="message"></param>
        private void SetErrorState(ErrorState errorState, string message = null)
        {
            if (_errorState != errorState)
            {
                _errorState = errorState;
                ErrorStateChanged.Invoke(_errorState, message);
            }
        }

        /// <summary>
        /// Returns whether or not both conditions are true:
        /// <list type="bullet"><item>
        /// Earth Manager is tracking correctly</item><item>
        /// Current accuracy meets the specified minimums</item></list>
        /// Sets error state appropriately.
        /// </summary>
        /// <returns></returns>
        private bool TrackingIsValid()
        {
            bool valid = false;

            if (!valid && EarthManager.EarthTrackingState == TrackingState.Tracking)
            {
                /// Have we met the minimums?
                valid = EarthManager.CameraGeospatialPose.HeadingAccuracy <= _minimumHeadingAccuracy &&
                        EarthManager.CameraGeospatialPose.VerticalAccuracy <= _minimumVerticalAccuracy &&
                        EarthManager.CameraGeospatialPose.HorizontalAccuracy <= _minimumHorizontalAccuracy;
            }

            if (valid != _trackingValid)
            {
                _trackingValid = valid;
                SetErrorState(_trackingValid ? ErrorState.NoError : ErrorState.Tracking);
            }

            return valid;
        }

        /// <summary>
        /// Compare current tracking accuracy against best values.
        /// Return whether or not accuracy has improved since the last check.
        /// </summary>
        /// <returns></returns>
        private bool CheckAccuracyImproved()
        {
            bool horizontal = EarthManager.CameraGeospatialPose.HorizontalAccuracy < _bestHorizontalAccuracy;
            bool heading = EarthManager.CameraGeospatialPose.HeadingAccuracy < _bestHeadingAccuracy;
            bool vertical = EarthManager.CameraGeospatialPose.VerticalAccuracy < _bestVerticalAccuracy;

            bool improved = false;

            if (horizontal)
            {
                improved = true;
                _bestHorizontalAccuracy = EarthManager.CameraGeospatialPose.HorizontalAccuracy;
            }
            if (heading)
            {
                improved = true;
                _bestHeadingAccuracy = EarthManager.CameraGeospatialPose.HeadingAccuracy;
            }
            if (vertical)
            {
                improved = true;
                _bestVerticalAccuracy = EarthManager.CameraGeospatialPose.VerticalAccuracy;
            }

            return improved;
        }

        /// <summary>
        /// Return whether or not we've reached our specified target tracking accuracy values
        /// </summary>
        /// <returns></returns>
        private bool CheckTargetAccuracyReached()
        {
            return EarthManager.CameraGeospatialPose.HorizontalAccuracy <= _targetHorizontalAccuracy &&
                   EarthManager.CameraGeospatialPose.HeadingAccuracy <= _targetHeadingAccuracy &&
                   EarthManager.CameraGeospatialPose.VerticalAccuracy <= _targetVerticalAccuracy;
        }

        /// <summary>
        /// Creates and returns a new Geospatial Anchor at the current camera position
        /// We use this when creating and updating new Placeables Groups
        /// </summary>
        /// <returns></returns>
        public ARGeospatialAnchor RequestGeospatialAnchor()
        {
            GeospatialPose pose = EarthManager.CameraGeospatialPose;
            Quaternion quaternion = Quaternion.AngleAxis(180f - (float)pose.Heading, Vector3.up);
            return AnchorManager.AddAnchor(pose.Latitude, pose.Longitude, pose.Altitude, quaternion);
        }

        /// <summary>
        /// Creates and returns a new Geospatial Anchor at the specified Geospatial Pose
        /// We use this when restoring saved Placeables Groups from memory
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public ARGeospatialAnchor RequestGeospatialAnchor(GeospatialPose pose)
        {
            Quaternion quaternion = Quaternion.AngleAxis(180f - (float)pose.Heading, Vector3.up);
            return AnchorManager.AddAnchor(pose.Latitude, pose.Longitude, pose.Altitude, quaternion);
        }
    }
}