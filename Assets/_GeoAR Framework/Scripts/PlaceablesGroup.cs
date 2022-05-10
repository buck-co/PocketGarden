using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions;

namespace Buck
{
    /// <summary>
    /// A Gameobject/Transform that serves as a parent for grouping Placeable Objects.
    /// This object will ask for and align itself to Geospatial Anchors, and keep record of any child Placeable Objects.
    /// During the session it is first created, its world position is flexible; it will align itself to a new Geospatial Anchor any time tracking accuracy increases.
    /// By grouping Placeables under a single parent anchor, we can maintain their positions relative to each other better than if each Placeable was subject to individual Geo anchor accuracy.
    /// </summary>
    public class PlaceablesGroup : MonoBehaviour
    {
        /// <summary>
        /// Get data for Player Prefs storage
        /// </summary>
        public GroupData GroupData { get => _groupData; }

        /// <summary>
        /// All Placeable Objects that are children of this Group
        /// </summary>
        public List<PlaceableObject> Placeables { get => _placeables; }

        /// <summary>
        /// Was this Group created on this session? False if restored from a previous session.
        /// If so, then we are updating its Geo accuracy in the background while it's placeable objects are pinned in world space
        /// If restored from a previous session, then its Geo coordinates are fixed as are it's child objects relative local positions
        /// </summary>
        public bool IsInit { get => _isInit; }

        protected Transform _geoAnchorTransform;
        protected GroupData _groupData;
        protected List<PlaceableObject> _placeables = new List<PlaceableObject>();
        protected bool _isInit;

        /// <summary>
        /// Option to "unhook" the Group from its Geospatial Anchor once we're at a set accuracy target.
        /// The target accuracy reached event is emitted by GeospatialManager.
        /// This effectively fixes the Group in space so there's no further drifting from Geoposition adjustments.
        /// </summary>
        [Tooltip("Detach from Geospatial anchors if accuracy target reached. Prevents continous drift.")]
        [SerializeField] protected bool _detachAtAccuracyTarget;

        /// <summary>
        /// First creation of a new Placeables Group
        /// </summary>
        public virtual void Init()
        {
            _isInit = true;
            AttachToNewGeoAnchor(GeospatialManager.Instance.RequestGeospatialAnchor());
            SaveData();

            if (_detachAtAccuracyTarget)
                GeospatialManager.Instance.AccuracyImproved.AddListener(OnAccuracyImproved);
        }

        /// <summary>
        /// Restore a Placeables Group from a previous session
        /// </summary>
        /// <param name="data"></param>
        public virtual void Restore(GroupData data)
        {
            _placeables = new List<PlaceableObject>();
            _groupData = data;

            GeospatialPose geoPose = new GeospatialPose();
            geoPose.Latitude = data.Latitude;
            geoPose.Longitude = data.Longitude;
            geoPose.Altitude = data.Altitude;
            geoPose.Heading = data.Heading;

            AttachToNewGeoAnchor(GeospatialManager.Instance.RequestGeospatialAnchor(geoPose));

            /// Once target accuracy is reached, stop updating position 
            if (GeospatialManager.Instance.IsAccuracyTargetReached)
                DetachFromGeoAnchor();
            else
                GeospatialManager.Instance.TargetAccuracyReached.AddListener(DetachFromGeoAnchor);
        }

        /// <summary>
        /// Add Placeable to list
        /// </summary>
        /// <param name="placeable"></param>
        public virtual void RegisterPlaceable(PlaceableObject placeable)
        {
            if (!_placeables.Contains(placeable))
                _placeables.Add(placeable);
        }

        /// <summary>
        /// Remove Placeable from list
        /// </summary>
        /// <param name="placeable"></param>
        public virtual void RemovePlaceable(PlaceableObject placeable)
        {
            if (_placeables.Contains(placeable))
                _placeables.Remove(placeable);
        }

        /// <summary>
        /// Whenever our GeoAR accuracy improves, we ask for a new Geospatial Anchor and discard the old one
        /// This ensures we've used the best data available during the session to store our Group's position
        /// Only used during the initial session when this Placeables Group was created
        /// </summary>
        protected void OnAccuracyImproved()
        {
            AttachToNewGeoAnchor(GeospatialManager.Instance.RequestGeospatialAnchor());
            SaveData();
        }

        /// <summary>
        /// Fetch a new Geospatial Anchor parent at the current camera position
        /// Make this Group a child of the anchor at its local origin
        /// </summary>
        /// <param name="anchor"></param>
        protected void AttachToNewGeoAnchor(ARGeospatialAnchor anchor)
        {
            if (_geoAnchorTransform != null)
                DetachFromGeoAnchor();

            _geoAnchorTransform = anchor.transform;
            transform.parent = _geoAnchorTransform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Unparent from current anchor and destroy it
        /// </summary>
        protected void DetachFromGeoAnchor()
        {
            transform.parent = null;

            if (_geoAnchorTransform != null)
                Destroy(_geoAnchorTransform.gameObject);
        }

        /// <summary>
        /// Save current GeospatialPose and child Placeables for recall in later session
        /// We only save this Group if it has some Placeables in it
        /// </summary>
        protected void SaveData()
        {          
            _groupData = new GroupData(GeospatialManager.Instance.EarthManager.CameraGeospatialPose.Latitude,
                                       GeospatialManager.Instance.EarthManager.CameraGeospatialPose.Longitude,
                                       GeospatialManager.Instance.EarthManager.CameraGeospatialPose.Altitude,
                                       GeospatialManager.Instance.EarthManager.CameraGeospatialPose.Heading,
                                       new List<PlaceableObjectData>());

            if (_placeables.Count > 0) PlaceablesManager.Instance.Save();
        }

        /// <summary>
        /// Clean up if needed
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_geoAnchorTransform != null)
                Destroy(_geoAnchorTransform.gameObject);

            if (PlaceablesManager.Instance != null)
                PlaceablesManager.Instance.RemoveGroup(this);

            if (GeospatialManager.Instance != null)
            {
                GeospatialManager.Instance.AccuracyImproved.RemoveListener(OnAccuracyImproved);
                GeospatialManager.Instance.TargetAccuracyReached.RemoveListener(DetachFromGeoAnchor);
            }
        }
    }
}