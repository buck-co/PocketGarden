using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Buck
{
    public enum PlaceableState { Init, Placing, Previewing, Finalized }

    /// <summary>
    /// An object that can be placed in the scene on an ARPlane by the PlaceablesManager.
    /// Will register with and be parented to a PlaceablesGroup.
    /// </summary>
    public class PlaceableObject : MonoBehaviour
    {
        protected enum TapBehavior { Finalize, Preview, None }

        [Header("[ Placeable Settings ]")]
        [Tooltip("Behavior when user taps on this object during Placing mode")]
        [SerializeField] protected TapBehavior _tapToPlaceBehavior;

        [Tooltip("Material to swap for during Placing mode")]
        [SerializeField] protected Material _placingMaterial;

        [Tooltip("Material to swap for when there's an error during Placing mode")]
        [SerializeField] protected Material _errorMaterial;

        [Tooltip("Layer to set object when finalized")]
        [SerializeField] protected int _finalizedLayer = 3;

        /// <summary>
        /// Get the current PlaceableState. 
        /// </summary>
        public PlaceableState State { get { return _placeableState; } }

        /// <summary>
        /// Can this object be placed, currently?
        /// </summary>
        public bool IsPlaceable { get { return _isPlaceable; } }

        /// <summary>
        /// Get data for Player Prefs storage
        /// </summary>
        public PlaceableObjectData PlaceableData { get { return _placeableData; } }

        protected PlaceablesGroup _group;
        protected PlaceableState _placeableState;
        protected PlaceableObjectData _placeableData;
        protected Pose _sessionPose = new Pose();
        protected PlaceableObject _lastHit;
        protected Renderer[] _renderers;
        protected List<Material> _originalMaterials = new List<Material>();
        protected bool _isPlaceable = true,
                       _lockWorldPose;

        [Header("[ Snap To AR Planes ]")]
        [Tooltip("Continuously attempt to adjust position as ARPlanes are updated - during session this Placeable's Group was created only")]
        [SerializeField] protected bool _snapToARPlanesOnGroupInit;

        [Tooltip("Continuously attempt to adjust position as ARPlanes are updated - during all restored sessions")]
        [SerializeField] protected bool _snapToARPlanesOnRestore;

        [Tooltip("Ray is cast downward from this height to look for ARPlanes")]
        [SerializeField] protected float _raycastFromY = 100;

        [Tooltip("How often to fire the ARPlane raycast search")]
        [SerializeField] protected float _interval = 1.0f;

        protected virtual void LateUpdate()
        {
            if (_lockWorldPose)
                CorrectUnityWorldPose();
        }

        /// <summary>
        /// First creation of a new Placeable Object
        /// </summary>
        public virtual void Init(int prefabIndex)
        {
            _placeableData = new PlaceableObjectData(prefabIndex, new Pose());

            SetPlaceableState(PlaceableState.Init);
            OnInteractionStateChanged(InteractionManager.Instance.CurrentInteractionState);

            InteractionManager.Instance.InteractionStateChanged.AddListener(OnInteractionStateChanged);
        }

        /// <summary>
        /// Restore a Placeable Object from a previous session
        /// </summary>
        /// <param name="data"></param>
        public virtual void Restore(PlaceableObjectData data, PlaceablesGroup groupAnchor)
        {
            _placeableData = data;

            RegisterWithPlaceablesGroup(groupAnchor);
            RestoreGroupLocalPose();

            FinalizePlacement();
        }

        /// <summary>
        /// Registers with and parents to appropriate PlaceablesGroup.
        /// Will begin watching for ARPlane changes if options set
        /// </summary>
        public virtual void FinalizePlacement()
        {
            RegisterWithPlaceablesGroup(PlaceablesManager.Instance.CurrentGroup);

            transform.parent = null;
            transform.parent = _group.transform;
            SetPlaceableState(PlaceableState.Finalized);

            if (_group.IsInit && _snapToARPlanesOnGroupInit)
                BeginSnappingToARPlanes();

            if (!_group.IsInit && _snapToARPlanesOnRestore)
                BeginSnappingToARPlanes();
        }

        /// <summary>
        /// Save current local pose and return data
        /// </summary>
        /// <returns></returns>
        public virtual PlaceableObjectData GetData()
        {
            _placeableData.LocalPose.position = transform.localPosition;
            _placeableData.LocalPose.rotation = transform.localRotation;
            return _placeableData;
        }

        /// <summary>
        /// When error mode set, this object is not able to be placed
        /// </summary>
        /// <param name="errorMode"></param>
        public virtual void SetErrorMode(bool errorMode)
        {
            _isPlaceable = !errorMode;

            if (errorMode)
                SetMaterials(_errorMaterial);
            else if (_placeableState == PlaceableState.Placing)
                SetMaterials(_placingMaterial);
        }

        /// <summary>
        /// Make the appropriate PlaceablesGroup aware of this object
        /// </summary>
        /// <param name="group"></param>
        protected virtual void RegisterWithPlaceablesGroup(PlaceablesGroup group)
        {
            _group = group;
            _group.RegisterPlaceable(this);
        }

        /// <summary>
        /// Respond to user taps on object and set appropriate onTap behavior
        /// When user taps on this object, either set mode to preview or finalize it's placement.
        /// </summary>
        /// <param name="go"></param>
        protected virtual void OnObjectTapped(GameObject go)
        {
            bool tapped = false;

            if (go.transform == transform) tapped = true;
            foreach (Transform child in transform)
                if (go.transform == child) tapped = true;

            if (!tapped || !PlaceablesManager.Instance.ValidPlacement)
                return;

            switch (_tapToPlaceBehavior)
            {
                case TapBehavior.Finalize:
                    if (PlaceablesManager.Instance.CurrentGroup.IsInit) PinUnityWorldPose();
                    PlaceablesManager.Instance.FinalizeSelectedPlaceable();
                    SetMaterials();
                    break;

                case TapBehavior.Preview:
                    InteractionManager.Instance.CurrentInteractionState = InteractionState.Previewing;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Responds to changes in InteractionState on the InteractionManager
        /// When in placing mode, this object will listen for screen taps and it's pose will be updated by the PlaceablesManager
        /// In preview, it's pose is locked while awaiting final confirmation or cancellation 
        /// </summary>
        /// <param name="placingState"></param>
        protected virtual void OnInteractionStateChanged(InteractionState placingState)
        {
            InteractionManager.Instance.ObjectTapped.RemoveListener(OnObjectTapped);

            switch (placingState)
            {
                case InteractionState.Placing:
                    _lockWorldPose = false;
                    SetPlaceableState(PlaceableState.Placing);
                    InteractionManager.Instance.ObjectTapped.AddListener(OnObjectTapped);
                    break;

                case InteractionState.Previewing:
                    SetPlaceableState(PlaceableState.Previewing);
                    if (PlaceablesManager.Instance.CurrentGroup.IsInit) PinUnityWorldPose();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Responds to local state changes.
        /// Init will fetch refs to renderer and materials.
        /// Placing, Previewing and Finalized will swap desired/original materials.
        /// On Finalize, event listeners are removed and the object layer is set.
        /// </summary>
        /// <param name="state"></param>
        protected virtual void SetPlaceableState(PlaceableState state)
        {
            _placeableState = state;

            InteractionManager.Instance.PlaceableHovered.RemoveListener(OnOtherPlaceableDetected);

            switch (state)
            {
                case PlaceableState.Init:
                    SetupRenderers();
                    break;

                case PlaceableState.Placing:
                    SetMaterials(_placingMaterial);
                    InteractionManager.Instance.PlaceableHovered.AddListener(OnOtherPlaceableDetected);
                    break;

                case PlaceableState.Previewing:
                    SetMaterials();
                    break;

                case PlaceableState.Finalized:
                    InteractionManager.Instance.ObjectTapped.RemoveListener(OnObjectTapped);
                    InteractionManager.Instance.InteractionStateChanged.RemoveListener(OnInteractionStateChanged);
                    gameObject.layer = _finalizedLayer; 
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Sets an error mode to prevent being placed on top of another placeable
        /// </summary>
        /// <param name="placeable"></param>
        protected void OnOtherPlaceableDetected(PlaceableObject placeable)
        {
            if (placeable == _lastHit) return;

            if (placeable == null || placeable == this)
            {
                SetMaterials(_placingMaterial);
            }
            else
            {
                if (placeable != this)
                    SetMaterials(_errorMaterial);
            }

            _lastHit = placeable;
        }

        /// <summary>
        /// Save and pin world space position, used only during the first session that a Group Anchor is created.
        /// </summary>
        protected virtual void PinUnityWorldPose()
        {
            _sessionPose.position = transform.position;
            _sessionPose.rotation = transform.rotation;

            _lockWorldPose = true;
        }

        /// <summary>
        /// On restore, set local position relative to PlaceablesGroup
        /// </summary>
        protected virtual void RestoreGroupLocalPose()
        {
            transform.localPosition = _placeableData.LocalPose.position;
            transform.localRotation = _placeableData.LocalPose.rotation;
        }

        /// <summary>
        /// Keeps object pinned in world space position when PlaceablesManager position updates
        /// </summary>
        public virtual void CorrectUnityWorldPose()
        {
            transform.position = _sessionPose.position;
            transform.rotation = _sessionPose.rotation;
        }

        /// <summary>
        /// Loop object children and grab refs to renderers/materials
        /// </summary>
        protected virtual void SetupRenderers()
        {
            _originalMaterials = new List<Material>();

            _renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < _renderers.Length; i++)
                _originalMaterials.Add(_renderers[i].material);

            /// set material based on current placement 
            if (PlaceablesManager.Instance.ValidPlacement)
            {
                if (_placeableState == PlaceableState.Placing) SetMaterials(_placingMaterial);
                if (_placeableState == PlaceableState.Previewing) SetMaterials();
            }
            else
                SetErrorMode(true);
        }

        /// <summary>
        /// Change all materials, resets to original if no parameter given
        /// </summary>
        /// <param name="material"></param>
        protected virtual void SetMaterials(Material material = null)
        {
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].material = material == null ? _originalMaterials[i] : material;
        }

        /// <summary>
        /// Start raycasting at specified interval to look for changes in ARPlanes
        /// </summary>
        protected virtual void BeginSnappingToARPlanes()
        {
            InvokeRepeating("SnapToARPlane", _interval, _interval);
        }
        
        /// <summary>
        /// Move Y-position to compensate for changes in ARPlanes
        /// </summary>
        protected virtual void SnapToARPlane()
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position + new Vector3(0f, _raycastFromY, 0f), Vector3.down, out hit, Mathf.Infinity, InteractionManager.Instance.ARPlanesLayerMask))
            {
                ARPlane detectedPlane = hit.transform.GetComponent<ARPlane>();
                if (detectedPlane)
                {
                    transform.position = new Vector3(transform.position.x, hit.transform.position.y, transform.position.z);
                    if (PlaceablesManager.Instance.CurrentGroup.IsInit) PinUnityWorldPose();
                }
            }
        }

        /// <summary>
        /// Cleanup if needed
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_group != null) _group.RemovePlaceable(this);

            if (InteractionManager.Instance != null)
            {
                InteractionManager.Instance.InteractionStateChanged.RemoveListener(OnInteractionStateChanged);
                InteractionManager.Instance.PlaceableHovered.RemoveListener(OnOtherPlaceableDetected);
            }
        }
    }
}
