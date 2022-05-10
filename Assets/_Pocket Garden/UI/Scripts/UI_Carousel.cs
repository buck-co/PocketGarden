using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace Buck.PocketGarden
{

    public class UI_Carousel : MonoBehaviour, IEndDragHandler, IBeginDragHandler
    {
        /// Ref
        [Header("References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject iconPrefab;
        [SerializeField] private InfiniteScroll infiniteScroll;

        /// Events
        [HideInInspector] public UnityEvent<SeedIcon> OnIconCreated;
        [HideInInspector] public UnityEvent<int> ItemSelected;

        [Header("Settings")]
        [SerializeField] private float _scrollDistanceThreshold = 0.1f;
        [SerializeField] private float _scrollSpeed = 0.001f;
        [SerializeField] private float _selectDistanceThreshold = 50;

        /// Vars     
        private List<RectTransform> _iconTransforms = new List<RectTransform>();
        private List<float> _snapPoints = new List<float>();
        private float[] _distances;
        private int _targetIndex;
        private bool _dragging,
                     _needAutoSelect = true,
                     _clicked;

        private void Start()
        {
            scrollRect = GetComponent<ScrollRect>();
            InstantiateIcons();
        }

        private void Update()
        {
            HandleScroll();
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (GardenManager.Instance.CurrentAppState == AppState.Watering) return;

            _dragging = true;
        }

        public void OnEndDrag(PointerEventData data)
        {
            _dragging = false;
        }

        /// <summary>
        /// TODO : is there a way to remove seed/plant/garden-specific refs?
        /// </summary>
        private void InstantiateIcons()
        {
            // Initialize
            _snapPoints = new List<float>();
            _distances = new float[GardenManager.Instance.PlantPrefabs.Count];

            // Create Icons
            for (int i = 0; i < (int)GardenManager.Instance.PlantPrefabs.Count; i++)
            {
                // Create an icon prefab
                GameObject itemIcon = Instantiate(iconPrefab, scrollRect.content);
                iconPrefab.gameObject.name = "Seed Icon " + i;

                SeedIcon seedIcon = itemIcon.GetComponent<SeedIcon>();
                seedIcon._index = i;

                // Save reference
                _iconTransforms.Add(seedIcon.GetComponent<RectTransform>());

                // If prefab is a placeable, set up icon using placeable data
                Plant plant = GardenManager.Instance.PlantPrefabs[i].GetComponent<Plant>();
                if (plant != null) {
                    seedIcon.Plant = plant;
                }
                
                // Give responder script a reference -- maps X pos to screenspace 0 - 1 and animates properties
                UI_ScreenSpaceResponder responder = seedIcon.GetComponent<UI_ScreenSpaceResponder>();
                if (responder != null)
                {
                    responder.scrollRect = scrollRect;
                }

                // Create snap point. For non-infinite scroller, this saves the normalized scrollrect position at which icon is at center of UI
                float point = 1 / ((float)PlaceablesManager.Instance.PrefabsList.Count - 1) * i;
                _snapPoints.Add(point);

                // Subscribe to button events
                int myBigIndex = i;
                Button itemButton = seedIcon.button;
                itemButton.onClick.AddListener(() => TapToScroll(myBigIndex));

                OnIconCreated.Invoke(seedIcon);
            }

            infiniteScroll.scrollContent.ScrollContentStart();
            TapToScroll(0);

        }


        private void OnSelectItem(int index)
        {
            PlaceablesManager.Instance.SetPrefabIndex(index);
            ItemSelected.Invoke(index);
            InteractionManager.Instance.VibrateOnce();

            if (GardenManager.Instance.CurrentAppState != AppState.Splash)
                GardenManager.Instance.SetAppState(AppState.Placing);
        }

        public void TapToScroll(int index)
        {
            _targetIndex = index;
            _clicked = true;
        }

        private void HandleScroll()
        {
            /// Calc distances
            for (int i = 0; i < _iconTransforms.Count; i++)
            {
                float distance = Mathf.Abs(Screen.width / 2 - _iconTransforms[i].position.x);
                _distances[i] = distance;
            }

            float _minDistance = Mathf.Min(_distances) == 0 ? -1 : Mathf.Min(_distances);

            /// Dragging cancels everything
            if (_dragging)
            {
                _clicked = false;
                _targetIndex = -1;
            }
            
            /// Find auto target if needed
            if (!_clicked && !_dragging && _minDistance > _scrollDistanceThreshold)
            {
                /// Find nearest index to target
                for (int i = 0; i < _distances.Length; i++)
                    if (_minDistance == _distances[i]) _targetIndex = i;
            }

            /// Do the scroll if we have a target
            if (_targetIndex >=0)
            {
                int direction = Screen.width / 2 - _iconTransforms[_targetIndex].position.x > 0 ? -1 : 1;

                scrollRect.horizontalNormalizedPosition += _distances[_targetIndex] * _scrollSpeed * direction;

                /// This force actives the inifite scroll even tho there's no user drag detected (only necessary in the positive direction)
                if (_clicked)
                    if (Screen.width / 2 - _iconTransforms[_targetIndex].position.x > 0) infiniteScroll.SetPositiveDrag();

                if (_distances[_targetIndex] < _scrollDistanceThreshold) {
                    _targetIndex = -1;
                    _clicked = false;
                }
            }

            if (_minDistance < 0) return;

            /// Auto select prefab if icon is within range

            /// Find nearest index to target
            int prefabIndex = 0;
            for (int i = 0; i < _distances.Length; i++)
                if (_minDistance == _distances[i]) prefabIndex = i;

            if (prefabIndex != PlaceablesManager.Instance.CurrentPrefabIndex)
            {
                OnSelectItem(prefabIndex);
            }
        }
    }
}