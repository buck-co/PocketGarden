using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Buck
{
    public enum InteractionState { None, Placing, Previewing }

    /// <summary>
    /// Singleton Class
    /// Handles AR Plane detection, touch screen input, and current interaction state.
    /// We are constantly raycasting from the camera position, looking for ARPlanes.
    /// When we hit a plane, we emit an event with the world space position of the intersection.
    /// </summary>
    public class InteractionManager : Singleton<InteractionManager>
    {
        [SerializeField] private ARPlaneManager ARPlaneManager;
        [SerializeField] private ARRaycastManager ARRaycastManager;
        [SerializeField] private Camera mainCamera;

        /// <summary>
        /// Get the current Interaction State
        /// </summary>
        public InteractionState CurrentInteractionState { get => _interactionState; set => SetInteractionState(value); }

        /// <summary>
        /// The ARPlane currently detected by raycast where the camera is pointing.
        /// Updated every frame. Null if none.
        /// </summary>
        public ARPlane CurrentARPlane { get => _currentARPlane; }

        /// <summary>
        /// ARPlane raycast hits beyond this distace are discarded
        /// </summary>
        public float MaxInteractionDistance { get => _maxRaycastDistance; }

        /// <summary>
        /// All current tracked ARPlanes
        /// </summary>
        public TrackableCollection<ARPlane> AllPlanes { get => ARPlaneManager.trackables; }

        /// <summary>
        /// Point on the screen the user is touching, in screen space values
        /// </summary>
        public Vector2 TouchPoint { get => _touchPoint; }

        /// <summary>
        /// Point on the screen the user is touching, normalized
        /// </summary>
        public Vector2 NormalizedTouchPoint { get => NormalizedScreenPosition(_touchPoint); }

        /// <summary>
        /// Raised whenever the Interaction State is changed
        /// </summary>        
        [HideInInspector] public UnityEvent<InteractionState> InteractionStateChanged;

        /// <summary>
        /// Raised whenever the current detected ARPlane changes. Can be null.
        /// </summary>
        [HideInInspector] public UnityEvent<ARPlane> ARPlaneChanged;

        /// <summary>
        /// Raised every frame. Pose is either ray interstion with a valid ARPlane, or else the set default position.
        /// </summary>
        [HideInInspector] public UnityEvent<Pose> ARHitPoseUpdated;

        /// <summary>
        /// Raised when a Placeable Object is tapped, returns it's GameObject.
        /// </summary> 
        [HideInInspector] public UnityEvent<GameObject> ObjectTapped;

        /// <summary>
        /// Raised when a previously touched Placeable Object is now released, returns its GameObject
        /// </summary>
        [HideInInspector] public UnityEvent<GameObject> ObjectReleased;

        /// <summary>
        /// Raised each frame on screen swipes and provides normalized coordinates
        /// </summary>
        [HideInInspector] public UnityEvent<Vector2> TouchPositionChanged;

        /// <summary>
        /// Fires every frame, returns a Placeable Object if a raycast from the camera hits or null if none
        /// </summary>
        [HideInInspector] public UnityEvent<PlaceableObject> PlaceableHovered;

        private InteractionState _interactionState = InteractionState.Placing;
        private List<ARRaycastHit> _hits = new List<ARRaycastHit>();
        private List<ARPlane> _allPlanes = new List<ARPlane>();
        private ARPlane _currentARPlane;
        private RaycastHit _raycastHit;
        private Ray _ray;
        private Vector2 _touchPoint; /// normalized
        private GameObject _touchedObject;
        private Transform _fallbackPlacementTransform;
        private bool _run;

        [Header("[ AR Plane Settings ]")]
        [Tooltip("Filter raycasts for Placeable Objects")]
        public LayerMask PlaceablesLayerMask; /// using: 3

        [Tooltip("Filter raycasts for ARPlanes")]
        public LayerMask ARPlanesLayerMask; /// using: 1

        [Tooltip("Filter raycasts for ARPlanes by type")]
        [SerializeField] private TrackableType _planeTypes;

        [Tooltip("ARPlane raycast hits beyond this distace are discarded")]
        [SerializeField] private float _maxRaycastDistance = 3;

        [Tooltip("Where to position the current Placeable when no ARPlane is found")]
        [SerializeField] private Vector3 _fallbackPlacementPos = new Vector3(0, 0, 2);

        /// <summary>
        /// Experimental ARPlane detection filtering
        /// </summary>
        [Header("[ Experimental ARPlane Culling ]")]
        [Tooltip("Use hit position on only the largest ARPlane detected by raycast")]
        [SerializeField] private bool _filterHitsBySize;

        [Tooltip("Smallest raycast detected ARPlanes will be disabled")]
        [SerializeField] private bool _cullBySizeOnHit;

        [Tooltip("Smallest ARPlanes in scene will always be disabled")]
        [SerializeField] private bool _cullAllButLargestPlane;

        [Header("[ Input Settings ]")]
        [Tooltip("Screen taps below this normalized height will be ignored")]
        [SerializeField] private float _screenTapLowerCutoff = 0.15f;

        private void Start()
        {
            _fallbackPlacementTransform = new GameObject("Fallback Placement").transform;
            _fallbackPlacementTransform.parent = mainCamera.transform;
            _fallbackPlacementTransform.localPosition = _fallbackPlacementPos;
            _fallbackPlacementTransform.localRotation = Quaternion.identity;

            GeospatialManager.Instance.InitCompleted.AddListener(OnGeoInitCompleted);
        }

        private void OnGeoInitCompleted()
        {
            GeospatialManager.Instance.InitCompleted.RemoveListener(OnGeoInitCompleted);

            if (ARPlaneManager != null)
                ARPlaneManager.planesChanged += UpdatePlanes;

            _run = true;
        }

        private void Update()
        {
            if (!_run) return;

            HandleScreenTap();
            HandleTapRelease();
            HandleSwipe();

            /// Optional forced culling of AR planes
            if (_cullAllButLargestPlane && AllPlanes.count > 1)
                CullPlanesBySize();

            /// Raycast for Placeables in scene
            if (_interactionState == InteractionState.Placing)
                DetectPlaceables();

            /// Raycast for AR Planes in scene
            if (_interactionState == InteractionState.Placing || _interactionState == InteractionState.Previewing)
                DetectARPlane();
        }


        /// <summary>
        /// Maintains a list of AR Planes in scene.
        /// </summary>
        private void UpdatePlanes(ARPlanesChangedEventArgs planes)
        {
            planes.added.ForEach(plane =>
            {
                if (!_allPlanes.Contains(plane)) _allPlanes.Add(plane);
            });
            planes.removed.ForEach(plane =>
            {
                if (_allPlanes.Contains(plane)) _allPlanes.Remove(plane);
            });
        }

        /// <summary>
        /// Invokes ObjectTapped event when a placeable object is tapped.
        /// </summary>
        private void HandleScreenTap()
        {
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                _touchPoint = Input.touches[0].position;
            }

            else return;

            if (NormalizedTouchPoint.y < _screenTapLowerCutoff)
                return;

            Ray ray = Camera.main.ScreenPointToRay(_touchPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                _touchedObject = hit.collider.gameObject;
                ObjectTapped?.Invoke(_touchedObject);
            }
        }

        /// <summary>
        /// Invokes ObjectReleased event when tap is released on a touched object.
        /// </summary>
        private void HandleTapRelease()
        {
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended)
            {

                if (_touchedObject != null)
                {
                    ObjectReleased?.Invoke(_touchedObject);
                    _touchedObject = null;
                }
            }
        }

        /// <summary>
        /// Invokes TouchPositionChanged event the screen is swiped.
        /// Returned Vec2 is normalized screen position
        /// </summary>
        private void HandleSwipe()
        {
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Moved)
            {
                if (_touchedObject != null)
                {
                    Vector2 pos = new Vector2(Input.touches[0].position.x, Input.touches[0].position.y);
                    Vector2 normalizedPos = NormalizedScreenPosition(pos);
                    TouchPositionChanged?.Invoke(normalizedPos);
                }
            }
        }

        /// <summary>
        /// Fires ARRaycastManager.Raycast 
        /// </summary>
        private void DetectARPlane()
        {
            /// Raycast from the center of the screen
            if (ARRaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), _hits, _planeTypes))
            {
                bool hitARPlane = false;
                int index = 0;
                float area = 0;

                /// Did we hit any active planes?
                foreach (ARRaycastHit hit in _hits)
                {
                    if (ARPlaneManager.GetPlane(hit.trackableId).gameObject.activeSelf && hit.distance <= _maxRaycastDistance)
                    {
                        hitARPlane = true;

                        /// Optional hit filtering by size
                        if (_filterHitsBySize || _cullBySizeOnHit)
                        {
                            Vector2 size = ARPlaneManager.GetPlane(hit.trackableId).size;
                            if (size.x * size.y > area)
                            {
                                area = size.x * size.y;
                                index = _hits.IndexOf(hit);
                            }
                        }
                    }
                }

                ARPlane plane = hitARPlane ? ARPlaneManager.GetPlane(_hits[index].trackableId) : null;

                /// Optional set the smallest hit planes inactive
                if (plane != null && _cullBySizeOnHit)
                {
                    foreach (ARRaycastHit hit in _hits)
                    {
                        ARPlaneManager.GetPlane(hit.trackableId).gameObject.SetActive(hit.trackableId == plane.trackableId);
                    }
                }

                /// Fire update event on change
                if (plane != _currentARPlane)
                {
                    _currentARPlane = plane;
                    ARPlaneChanged?.Invoke(_currentARPlane);
                }

                /// Fire pose update event
                if (hitARPlane)
                {
                    ARHitPoseUpdated?.Invoke(_hits[0].pose);
                    return;
                }
            }

            /// Fallback
            ARHitPoseUpdated?.Invoke(new Pose(_fallbackPlacementTransform.position, Quaternion.identity));

        }

        /// <summary>
        /// Invokes PlaceableHovered event when camera is pointed at a Placeable object.
        /// </summary>
        private void DetectPlaceables()
        {
            _ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

            PlaceableObject placeable = null;
            if (Physics.Raycast(_ray, out _raycastHit, _maxRaycastDistance, PlaceablesLayerMask))
            {
                placeable = _raycastHit.collider.transform.GetComponentInParent<PlaceableObject>();
            }

            PlaceableHovered?.Invoke(placeable);
        }

        /// <summary>
        /// Invokes InteractionStateChanged event. Used to control whether we're in Placing or Previewing modes.
        /// </summary>
        private void SetInteractionState(InteractionState interactionState)
        {
            if (_interactionState != interactionState)
            {
                _interactionState = interactionState;
                InteractionStateChanged?.Invoke(_interactionState);
            }
        }

        /// <summary>
        /// Disables all AR planes other than the current largest plane.
        /// </summary>
        private void CullPlanesBySize()
        {
            float area = 0;
            ARPlane biggestPlane = null;
            foreach (ARPlane plane in AllPlanes)
            {
                if (plane.size.x * plane.size.y > area)
                {
                    biggestPlane = plane;
                    area = plane.size.x * plane.size.y;
                }
            }

            foreach (ARPlane plane in AllPlanes)
                plane.gameObject.SetActive(plane == biggestPlane);
        }

        /// <summary>
        /// Send haptic pulse using AndroidHaptics.
        /// </summary>
        public void VibrateOnce()
        {
            AndroidHaptics.Vibrate(5);
        }

        /// <summary>
        /// Returns Vec2 screen position, normalized.
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        public Vector2 NormalizedScreenPosition(Vector2 screenPos)
        {
            float x = Remap(screenPos.x, 0, Screen.width, 0, 1);
            float y = Remap(screenPos.y, 0, Screen.height, 0, 1);

            return new Vector2(x, y);
        }

        private float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float fromAbs = value - fromMin;
            float fromMaxAbs = fromMax - fromMin;

            float normal = fromAbs / fromMaxAbs;

            float toMaxAbs = toMax - toMin;
            float toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }
    }
}
