using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Buck.PocketGarden
{
    public class Plant : PlaceableObject
    {
        /// References
        [SerializeField] PlantConfiguration _plantConfig;

        /// Properties

        public PlantConfiguration Config { get => _plantConfig; }
        public PlantData PlantData { get => _plantData; }
        public bool IsWatered { get => _isWatered; }
        public float Growth { get => _growth; }

        /// Events

        [HideInInspector] public UnityEvent<float> WasShaken;

        /// Vars

        protected PlantData _plantData;
        protected GameObject _currentPlantGeo;
        protected PlantAttachPoints _plantAttachPoints;
        protected List<Transform> _flowersFruits = new List<Transform>();

        protected float _growth,
                        _ikLocalY;

        protected bool _isWatered,
                       _isTapped,
                       _needsBuild,
                       _harvested,
                       _firstGrow,
                       _canFruit,
                       _isIKAnimating;

        protected int _maxSize;

        protected Vector3 _ikTargetPosition,
                          _ikCameraLocalPos;

        float _prevShakeX,
              _touchDragDistance,
              _prevDrag;

        protected virtual void Awake()
        {
            /// Cap max possible amount of growth a max number of base prefabs we have and reduce to int
            _maxSize = _plantConfig.BasePrefabs.Length - 1;

        }

        protected void Update()
        {
            AnimateIK();

            if (_placeableState != PlaceableState.Finalized)
            {
                if (GardenManager.Instance.Items[_placeableData.PrefabIndex].quantity <= 0)
                    SetMaterials(_errorMaterial);
            }

            /// Debug Only
            KeyInput();
        }

        /// <summary>
        /// Initialize plant properties and build first time
        /// </summary>
        public override void Init(int prefabIndex)
        {
            base.Init(prefabIndex);

            _plantData = new PlantData();
            _plantData.Seed = UnityEngine.Random.value * 1000;

            Build();
            SetupRenderers();
        }

        /// <summary>
        /// Rebuild plant from stored data
        /// </summary>
        /// <param name="data"></param>
        public override void Restore(PlaceableObjectData data, PlaceablesGroup groupAnchor)
        {
            base.Restore(data, groupAnchor);

            /// Restore & build plant
            _plantData = JsonUtility.FromJson<PlantData>(data.AuxData);
            _growth = 0;

            LogInfo();
            _needsBuild = true;
            _canFruit = true;
            _isWatered = false;
            _firstGrow = true;

            /// Continuously update growth state
            UpdateGrowth();
        }

        public override void FinalizePlacement()
        {
            base.FinalizePlacement();

            InvokeRepeating("UpdateGrowth", 0, 1);

            /// Listen for screen taps to enable shaking behavior
            InteractionManager.Instance.ObjectTapped.AddListener(OnTappedToShake);
        }

        protected override void OnObjectTapped(GameObject go)
        {
            bool tapped = false;

            /// If no seed inventory
            if (GardenManager.Instance.Items[_placeableData.PrefabIndex].quantity <= 0)
            {
                GardenManager.Instance.DisplayNoSeedsMessage();
                InteractionManager.Instance.ObjectTapped.RemoveListener(OnObjectTapped);
                return;
            } 

            /// Was this plant tapped?
            if (go.transform == transform) tapped = true;
            foreach (Transform child in transform)
                if (go.transform == child) tapped = true;

            if (!tapped) return;

            /// Change to Preview
            if (PlaceablesManager.Instance.ValidPlacement)
                InteractionManager.Instance.CurrentInteractionState = InteractionState.Previewing;
        }

        /// <summary>
        /// Add growth per tick and determine plant size and stage
        /// </summary>
        protected virtual void UpdateGrowth()
        {
            _growth += _isWatered ? _plantConfig.GrowthPerTickWatered : _plantConfig.GrowthPerTickThirsty;
            _isWatered = false;

            /// Determine size
            if (_plantData.Size < _maxSize)
            {
                float totalGrowth = 0;
                int newSize = 0;

                if (_canFruit) /// this is not the first session
                {
                    _plantData.Size += 1;
                    _growth = 0;
                }
                else
                {
                    totalGrowth = _plantData.Size + _growth;
                    newSize = Mathf.FloorToInt(Mathf.Min(totalGrowth, _maxSize));
                }

                /// Did we grow in size?
                if (newSize > _plantData.Size)
                {

                    _growth -= newSize - _plantData.Size;
                    _plantData.Size = newSize;
                    _needsBuild = true;
                }
            }

            /// Build
            if (_needsBuild)
            {
                Build(_plantData.Size);
                _plantData.Stage = -1;

                /// Save progress
                PlaceablesManager.Instance.Save();

                /// FX
                if (_plantData.Size > 0 && !_firstGrow)
                {
                    ParticleSystem ps = _currentPlantGeo.GetComponentInChildren<ParticleSystem>();
                    if (ps != null) ps.Play(true);

                    if (_plantData.Size == 1) GardenManager.Instance.PlaySound("Grow1", .5f);
                    if (_plantData.Size == 2) GardenManager.Instance.PlaySound("Grow2", .5f);

                    GardenManager.Instance.PlantGrew.Invoke(this);
                }

                _firstGrow = false;
            }

            /// Determine stage
            if (_plantData.Size == 2)
            {
                if (_plantData.Stage == 2 && !_harvested) return;

                if (_canFruit) SetStage(2);
                else
                {
                    if (_harvested)
                    {
                        if (_growth >= 1) SetStage(1);
                    }
                    else SetStage(1);
                }
            }
            else
                SetStage(_plantData.Size);

            _needsBuild = false;

            LogInfo();
        }

        /// <summary>
        /// Set the plant to have flowers, fruits, or none
        /// </summary>
        /// <param name="stage"></param>
        protected virtual void SetStage(int stage)
        {
            if (_plantData.Stage == stage) return;

            _plantData.Stage = stage;

            if (stage == 0) StripCurrentFlowersFruits();
            if (stage == 1) PopulateFlowers();
            if (stage == 2) { PopulateFruit(); _canFruit = false; };
        }

        /// <summary>
        /// Instantiate prefab based on current plant size
        /// </summary>
        /// <param name="size"></param>
        protected virtual void Build(int size = 0)
        {
            if (_currentPlantGeo != null)
                Destroy(_currentPlantGeo);

            _currentPlantGeo = Instantiate(_plantConfig.BasePrefabs[size], transform);

            /// Setting to "Placeables" layer so another plant can't be placed too close
            if (_placeableState == PlaceableState.Finalized)
                _currentPlantGeo.layer = 3;

            /// Randomize rotation
            _currentPlantGeo.transform.eulerAngles = new Vector3(_currentPlantGeo.transform.eulerAngles.x,
                                                                  StaticFunctions.SeedRandom(_plantData.Seed, _plantData.Seed / 2.4f) * 3600.9865f,
                                                                  _currentPlantGeo.transform.eulerAngles.z);

            /// Randomize scale
            float scaleVar = (StaticFunctions.SeedRandom(_plantData.Seed, _plantData.Seed * 0.678f) - 0.5f) * _plantConfig.RandomScaleAmount;
            Vector3 newScale = _currentPlantGeo.transform.localScale * (1 + scaleVar);
            _currentPlantGeo.transform.localScale = newScale;

            SetupIK();

            PopulateLeaves();
        }

        /// <summary>
        /// Populate the plant with leaves
        /// </summary>
        protected virtual void PopulateLeaves()
        {
            StripCurrentFlowersFruits();

            for (int i = 0; i < _plantAttachPoints.LeafPoints.Nulls.Count; i++)
            {
                /// Does it get populated?
                bool willPopulate = StaticFunctions.SeedRandom(_plantData.Seed, i * _plantConfig.RandomNoiseScale) < _plantConfig.LeafProbability;

                if (willPopulate)
                {
                    /// Which prefab?
                    Transform leafNull = _plantAttachPoints.LeafPoints.Nulls[i];
                    Transform newLeaf = Instantiate(_plantConfig.LeafPrefabs[_plantData.LeafPrefabIndex], leafNull).transform;
                    newLeaf.localPosition = Vector3.zero;
                    newLeaf.localRotation = Quaternion.identity;

                    /// Rotate it?
                    newLeaf.transform.Rotate(_plantAttachPoints.LeafPoints.RotationTollerances.x * (StaticFunctions.SeedRandom(_plantData.Seed, i + 1000) - 0.5f),
                                             _plantAttachPoints.LeafPoints.RotationTollerances.y * (StaticFunctions.SeedRandom(_plantData.Seed, i + 2000) - 0.5f),
                                             _plantAttachPoints.LeafPoints.RotationTollerances.z * (StaticFunctions.SeedRandom(_plantData.Seed, i + 3000) - 0.5f));

                    /// Scale it?
					newLeaf.transform.localScale += new Vector3(_plantAttachPoints.LeafPoints.ScaleTollerances.x * (StaticFunctions.SeedRandom(_plantData.Seed, i + 1000) - 0.5f),
                                                                _plantAttachPoints.LeafPoints.ScaleTollerances.y * (StaticFunctions.SeedRandom(_plantData.Seed, i + 2000) - 0.5f),
                                                                _plantAttachPoints.LeafPoints.ScaleTollerances.z * (StaticFunctions.SeedRandom(_plantData.Seed, i + 3000) - 0.5f));
                }
            };
        }

        /// <summary>
        /// Populate the plant with flowers
        /// </summary>
        protected virtual void PopulateFlowers()
        {
            StripCurrentFlowersFruits();

            if (_plantConfig.FlowerPrefabs.Count == 0) { Debug.Log(gameObject.name + " has no flower prefabs"); return; }

            for (int i = 0; i < _plantAttachPoints.BudPoints.Nulls.Count; i++)
            {
                /// Does it get populated?
                bool willPopulate = StaticFunctions.SeedRandom(_plantData.Seed, i * _plantConfig.RandomNoiseScale) < _plantConfig.FlowerProbability;

                if (willPopulate)
                {
                    /// Which prefab?
                    Transform budNull = _plantAttachPoints.BudPoints.Nulls[i];
                    Transform newFlower = Instantiate(_plantConfig.FlowerPrefabs[_plantData.FlowerPrefabIndex], budNull).transform;
                    newFlower.localPosition = Vector3.zero;
                    newFlower.localRotation = Quaternion.identity;

                    /// Rotate it?
                    newFlower.transform.Rotate(_plantAttachPoints.BudPoints.RotationTollerances.x * (StaticFunctions.SeedRandom(_plantData.Seed, i + 1000) - 0.5f),
                                               _plantAttachPoints.BudPoints.RotationTollerances.y * (StaticFunctions.SeedRandom(_plantData.Seed, i + 2000) - 0.5f),
                                               _plantAttachPoints.BudPoints.RotationTollerances.z * (StaticFunctions.SeedRandom(_plantData.Seed, i + 3000) - 0.5f));

                    ///Scale it?
					newFlower.transform.localScale += new Vector3(_plantAttachPoints.BudPoints.ScaleTollerances.x * (StaticFunctions.SeedRandom(_plantData.Seed, i + 1000) - 0.5f),
                                                                  _plantAttachPoints.BudPoints.ScaleTollerances.y * (StaticFunctions.SeedRandom(_plantData.Seed, i + 2000) - 0.5f),
                                                                  _plantAttachPoints.BudPoints.ScaleTollerances.z * (StaticFunctions.SeedRandom(_plantData.Seed, i + 3000) - 0.5f));

                    _flowersFruits.Add(newFlower);
                }
            };
        }

        /// <summary>
        /// Popuate the plant with fruit
        /// </summary>
        protected virtual void PopulateFruit()
        {
            StripCurrentFlowersFruits();

            if (_plantConfig.FlowerPrefabs.Count == 0) { Debug.Log(gameObject.name + " has no fruit prefabs"); return; }

            for (int i = 0; i < _plantAttachPoints.BudPoints.Nulls.Count; i++)
            {
                /// Does it get populated?
                bool willPopulate = StaticFunctions.SeedRandom(_plantData.Seed, i * _plantConfig.RandomNoiseScale) < _plantConfig.FruitProbabiltiy;

                if (willPopulate)
                {
                    /// Which prefab?
                    Transform budNull = _plantAttachPoints.BudPoints.Nulls[i];
                    Transform newFruit = Instantiate(_plantConfig.FruitPrefabs[_plantData.FruitPrefabIndex], budNull).transform;

                    newFruit.GetComponent<FruitBehavior>().Init(this);

                    newFruit.localPosition = Vector3.zero;
                    newFruit.localRotation = Quaternion.identity;

                    /// Rotate it?
                    newFruit.transform.Rotate(_plantAttachPoints.BudPoints.RotationTollerances.x * (StaticFunctions.SeedRandom(_plantData.Seed, i + 1000) - 0.5f),
                                              _plantAttachPoints.BudPoints.RotationTollerances.y * (StaticFunctions.SeedRandom(_plantData.Seed, i + 2000) - 0.5f),
                                              _plantAttachPoints.BudPoints.RotationTollerances.z * (StaticFunctions.SeedRandom(_plantData.Seed, i + 3000) - 0.5f));

                    /// Scale it?
                    newFruit.transform.localScale += new Vector3(_plantAttachPoints.BudPoints.ScaleTollerances.x * (StaticFunctions.SeedRandom(_plantData.Seed, i + 1000) - 0.5f),
                                                                 _plantAttachPoints.BudPoints.ScaleTollerances.y * (StaticFunctions.SeedRandom(_plantData.Seed, i + 2000) - 0.5f),
                                                                 _plantAttachPoints.BudPoints.ScaleTollerances.z * (StaticFunctions.SeedRandom(_plantData.Seed, i + 3000) - 0.5f));

                    _flowersFruits.Add(newFruit);
                }
            };
        }

        /// <summary>
        /// Remove any current flowers or fruits
        /// </summary>
        protected virtual void StripCurrentFlowersFruits()
        {
            _flowersFruits.ForEach(f => { Destroy(f.gameObject); });
            _flowersFruits = new List<Transform>();
        }

        protected virtual void SetupIK()
        {
            _currentPlantGeo.transform.localPosition = Vector3.zero;

            _plantAttachPoints = _currentPlantGeo.GetComponent<PlantAttachPoints>();
            _ikLocalY = _plantAttachPoints.IKTarget.localPosition.y;
            _isIKAnimating = false;
            SetIKTargetToCenter();
        }

        protected virtual void SetIKTargetToCenter()
        {
            _currentPlantGeo.transform.localPosition = Vector3.zero;
            _ikTargetPosition = _currentPlantGeo.transform.TransformPoint(new Vector3(0, _ikLocalY, 0));
        }

        protected virtual void AnimateIK()
        {
            if (!_isTapped)
            {
                if (_isIKAnimating)
                {
                    if (Vector3.Distance(_plantAttachPoints.IKTarget.position, _ikTargetPosition) < _plantConfig.ShakeAnimateThresh)
                        _isIKAnimating = false;
                }
                else
                    SetIKTargetToCenter();
            }

            /// Do the animation
            if (_plantAttachPoints != null)
                _plantAttachPoints.IKTarget.position = Vector3.Lerp(_plantAttachPoints.IKTarget.position, _ikTargetPosition, Time.deltaTime * _plantConfig.ShakeSmoothSpd);
        }

        protected virtual void OnTappedToShake(GameObject tappedObject)
        {
            Plant tappedPlant = tappedObject.GetComponentInParent<Plant>();
            if (tappedPlant == null ||
                tappedPlant != this ||
                _placeableState != PlaceableState.Finalized ||
                GardenManager.Instance.CurrentAppState == AppState.Watering)
                return;

            _isTapped = true;
            InteractionManager.Instance.TouchPositionChanged.AddListener(OnTouchPositionChanged);
            InteractionManager.Instance.ObjectReleased.AddListener(OnObjectReleased);
            _prevShakeX = InteractionManager.Instance.NormalizedTouchPoint.x;
        }

        protected virtual void OnTouchPositionChanged(Vector2 normalizedTouchPoint)
        {
            Vector3 plantScreenPosition = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            Vector2 plantScreenPosNormalized = InteractionManager.Instance.NormalizedScreenPosition(plantScreenPosition);

            /// How far the touch has moved from the plant in screen space (-0.5 to 0.5)
            _touchDragDistance = Mathf.Clamp(normalizedTouchPoint.x - plantScreenPosNormalized.x, -0.5f, 0.5f);

            _ikTargetPosition = CalculateShakeTargetPosition(_touchDragDistance);

            /// Shake sounds
            if (_touchDragDistance > 0 && _prevDrag < 0) GardenManager.Instance.PlaySound("PlantShakeL");
            if (_touchDragDistance < 0 && _prevDrag > 0) GardenManager.Instance.PlaySound("PlantShakeR");
            _prevDrag = _touchDragDistance;

            /// Shake detection for fruit
            float shakeAmount = Math.Abs(_prevShakeX - normalizedTouchPoint.x) * _plantConfig.ShakeHarvestSpeed;
            WasShaken.Invoke(shakeAmount);
        }

        protected virtual Vector3 CalculateShakeTargetPosition(float touchDragDistance)
        {
            /// Distance the IK target is being pulled
            float plantPulledDistance = Mathf.Lerp(-_plantConfig.ShakeDistance, _plantConfig.ShakeDistance, touchDragDistance + 0.5f);

            /// IK target's position in camera's local space
            Vector3 ikCameraLocalPos = Camera.main.transform.InverseTransformPoint(_plantAttachPoints.IKTarget.position);

            /// Plant's position in camera's local space
            Vector3 _plantCameraLocal = Camera.main.transform.InverseTransformPoint(transform.position);

            /// IK target destination in world space
            return Camera.main.transform.TransformPoint(new Vector3(plantPulledDistance + _plantCameraLocal.x, ikCameraLocalPos.y, ikCameraLocalPos.z));
        }

        protected virtual void OnObjectReleased(GameObject go)
        {
            _isTapped = false;

            /// Snap to opposite side when let go
            _ikTargetPosition = CalculateShakeTargetPosition(-_touchDragDistance);
            _isIKAnimating = true;

            InteractionManager.Instance.ObjectReleased.RemoveListener(OnObjectReleased);
            InteractionManager.Instance.TouchPositionChanged.RemoveListener(OnTouchPositionChanged);
        }

        public virtual void Water()
        {
            _isWatered = true;
        }

        /// <summary>
        /// Called by fruit object when harvested
        /// </summary>
        /// <param name="fruit"></param>
        public virtual void HarvestFruit(Transform fruit)
        {
            _flowersFruits.Remove(fruit);
            GardenManager.Instance.PlaySound("AddingSeeds");

            if (_flowersFruits.Count == 0)
            {
                SetStage(0);
                GardenManager.Instance.AddSeedToInventory(_placeableData.PrefabIndex);
                _harvested = true;
                _growth = 0;
            }
        }

        public override PlaceableObjectData GetData()
        {
            _placeableData.AuxData = JsonUtility.ToJson(_plantData);
            return base.GetData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (InteractionManager.Instance != null)
            {
                InteractionManager.Instance.ObjectTapped.RemoveListener(OnObjectTapped);
                InteractionManager.Instance.ObjectReleased.RemoveListener(OnObjectReleased);
                InteractionManager.Instance.TouchPositionChanged.RemoveListener(OnTouchPositionChanged);
            }
        }

        #region Debug Only

        private void KeyInput()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                Water();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetPlant(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetPlant(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetPlant(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SetPlant(-1, 0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                SetPlant(-1, 1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                SetPlant(-1, 2);
            }
        }

        private void SetPlant(int size = -1, int stage = -1)
        {
            CancelInvoke();

            if (size > -1)
            {
                _plantData.Size = size;
                Build(_plantData.Size);
            }

            if (stage > -1)
                SetStage(stage);
        }

        private void LogInfo()
        {
            string info = "";
            info += "Growth: " + _growth;
            info += " | Size: " + _plantData.Size;
            info += " | Stage: " + _plantData.Stage;

            //Debug.Log(info);
        }

        #endregion
    }

    [System.Serializable]
    public class PlantData
    {
        public float Seed;
        public int Size; /// 0 sm, 1 med, 2 lg
        public int Stage; /// 0 none, 1 flowers, 2 fruit
        public int LeafPrefabIndex;
        public int FlowerPrefabIndex;
        public int FruitPrefabIndex;
    }
}
