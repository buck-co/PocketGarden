using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Buck.PocketGarden
{
    public class GardenManager : Singleton<GardenManager>
    {
        /// References
        [Header("References")]
        [SerializeField] private GameObject splashScreen;
        [SerializeField] private UI_Carousel seedCarousel;
        [SerializeField] private SoundLib _soundLib;
        [SerializeField] private List<GameObject> _delayStart = new List<GameObject>();

        /// Properties
        [Header("Garden Config")]
        public List<GameObject> PlantPrefabs = new List<GameObject>();
        public int StartingSeedQuantity { get => _startingSeedQuantity; }
        public List<ItemData> Items { get => _inventory.items; }

        public string LearnMoreUrl = "https://developers.google.com/ar/data-privacy";
        public string RepoUrl = "https://github.com/buck-co/PocketGarden";

        /// App State
        public AppState CurrentAppState { get => _currentAppState; }
        public AppState PreviousAppState { get => _previouState; }

        /// Events
        [HideInInspector] public UnityEvent<AppState> AppStateChanged;
        [HideInInspector] public UnityEvent<List<ItemData>> InventoryUpdated;
        [HideInInspector] public UnityEvent<Plant> PlantGrew;
        [HideInInspector] public UnityEvent NoSeedsMessageDisplayed;

        /// Vars
        private AudioSource _audioSource;
        private AppState _defaultAppState = AppState.Splash;
        private AppState _previouState;
        private AppState _currentAppState;
        private int _startingSeedQuantity = 5;
        private InventoryData _inventory;
        
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();

            /// Set the PlaceablesManager prefabs list to plant prefabs
            PlaceablesManager.Instance.PrefabsList = PlantPrefabs;

            /// Init seeds
            InitSeedInventory();

            /// Event subscriptions
            GeospatialManager.Instance.InitCompleted.AddListener(OnInitCompleted);
            PlaceablesManager.Instance.ObjectPlaced.AddListener(OnPlaceObject);
            InteractionManager.Instance.InteractionStateChanged.AddListener(OnInteractionStateChanged);
        }

        /// <summary>
        /// Removes the splash screen and starts the user experience.
        /// </summary>
        public void StartDismissSplashScreen()
        {
            StartCoroutine(DismissSplashScreen());
        }

        IEnumerator DismissSplashScreen()
        {
            yield return new WaitForSeconds(.5f);
            Destroy(splashScreen);
            SetAppState(AppState.Placing);

            yield return null;
        }

        /// <summary>
        /// Sets the state of the app and its corresponding Interaction State. UI responds to the AppStateChanged event.
        /// </summary>
        /// <param name="newState">New app state</param>
        public void SetAppState(AppState newState)
        {
            if (!PlayerPrefs.HasKey("Inventory"))
                ResetInventory();

            if (_currentAppState != newState)
            {
                _previouState = _currentAppState;
                _currentAppState = newState;

                switch (newState)
                {
                    case AppState.Null:
                        InteractionManager.Instance.CurrentInteractionState = InteractionState.None;
                        break;
                    case AppState.Splash:
                        InteractionManager.Instance.CurrentInteractionState = InteractionState.None;
                        break;
                    case AppState.Placing:
                        InteractionManager.Instance.CurrentInteractionState = InteractionState.Placing;
                        break;
                    case AppState.Previewing:
                        PlaySound("PlantPreview");
                        break;
                    case AppState.Viewing:
                        InteractionManager.Instance.CurrentInteractionState = InteractionState.None;
                        break;
                    case AppState.Watering:
                        InteractionManager.Instance.CurrentInteractionState = InteractionState.None;
                        break;
                    case AppState.Developer:
                        InteractionManager.Instance.CurrentInteractionState = InteractionState.None;
                        break;
                    case AppState.Info:
                        InteractionManager.Instance.CurrentInteractionState = InteractionState.None;
                        break;
                    default:
                        break;
                }

                AppStateChanged?.Invoke(_currentAppState);
            }
        }

        /// <summary>
        /// Displays "no more seeds" message.
        /// </summary>
        public void DisplayNoSeedsMessage() {
            NoSeedsMessageDisplayed.Invoke();
        }

        /// <summary>
        /// Handles playback of audio clips defined in AudioSource scriptable object list.
        /// </summary>
        /// <param name="id">ID of clip to be played</param>
        public void PlaySound(string id, float volumeScale = 1f)
        {
            _soundLib.Sounds.ForEach(s =>
            {
                if (s.id == id)
                    _audioSource.PlayOneShot(s.clip, volumeScale);
            });
        }

        /// <summary>
        /// Replaces seed inventory with default starting inventory.
        /// </summary>
        public void ResetInventory() {
            _inventory = StartingInventory();
            InventoryUpdated.Invoke(_inventory.items);

            PlayerPrefs.SetString("Inventory", JsonUtility.ToJson(_inventory));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Remove a seed from your seed inventory.
        /// </summary>
        /// <param name="index">Seed index</param>
        public void RemoveSeedFromInventory(int index)
        {
            foreach (ItemData item in _inventory.items)
            {
                if (item.index == index)
                {
                    item.quantity--;
                }
            }

            InventoryUpdated.Invoke(_inventory.items);
            PlayerPrefs.SetString("Inventory", JsonUtility.ToJson(_inventory));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Adds a seed to your seed inventory.
        /// </summary>
        /// <param name="index">Seed index</param>
        public void AddSeedToInventory(int index)
        {
            foreach (ItemData item in _inventory.items)
            {
                if (item.index == index)
                {
                    item.quantity++;
                }
            }

            seedCarousel.TapToScroll(index);
            InventoryUpdated.Invoke(_inventory.items);

            PlayerPrefs.SetString("Inventory", JsonUtility.ToJson(_inventory));
            PlayerPrefs.Save();

        }

        /// <summary>
        /// Defines the initial seed inventory.
        /// </summary>
        /// <returns>InventoryData</returns>
        InventoryData StartingInventory()
        {

            InventoryData initialInventory = new InventoryData();
            int i = 0;
            foreach (GameObject placeable in PlaceablesManager.Instance.PrefabsList)
            {
                ItemData item = new ItemData();
                item.quantity = _startingSeedQuantity;
                item.index = i;
                initialInventory.items.Add(item);
                i++;
            }
            return initialInventory;
        }

        private void InitSeedInventory()
        {
            if (PlayerPrefs.HasKey("Inventory"))
            {
                _inventory = JsonUtility.FromJson<InventoryData>(PlayerPrefs.GetString("Inventory", JsonUtility.ToJson(StartingInventory())));
            }
            else
            {
                ResetInventory();
            }
        }

        private void OnInitCompleted()
        {
            /// Toggle visibility of startup objects
            _delayStart.ForEach(go =>
            {
                bool activate = !go.activeSelf;
                go.SetActive(activate);
            });

            SetAppState(_defaultAppState);
        }

        /// <summary>
        /// Sets the app state to Previewing when InteractionState is set to Previewing.
        /// </summary>
        /// <param name="newState">InteractionState</param>
        void OnInteractionStateChanged(InteractionState newState)
        {
            if (newState == InteractionState.Previewing)
                SetAppState(AppState.Previewing);
        }

        void OnPlaceObject(GameObject placedObject)
        {
            RemoveSeedFromInventory(PlaceablesManager.Instance.CurrentPrefabIndex);
        }
    }

    [System.Flags] public enum AppState { Null = 0, Splash = 1, Placing = 2, Previewing = 4, Viewing = 8, Watering = 16, Developer = 32, Info = 64 }

    [System.Serializable]
    public class InventoryData
    {
        public List<ItemData> items = new List<ItemData>();
    }

    [System.Serializable]
    public class ItemData
    {
        public string name;
        public int index;
        public int quantity;
    }

}