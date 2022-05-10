using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class UI_NoSeedsMessage : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] GameObject noSeedsMessage;

        private void Start()
        {
            GardenManager.Instance.NoSeedsMessageDisplayed.AddListener(StartDisplayNoSeedsMessage);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            noSeedsMessage.SetActive(false);
        }

        void StartDisplayNoSeedsMessage() {
            StopAllCoroutines();
            StartCoroutine(DisplayNoSeedsMessage());
        }

        IEnumerator DisplayNoSeedsMessage()
        {
            noSeedsMessage.SetActive(true);
            yield return new WaitForSeconds(2.3f);
            noSeedsMessage.SetActive(false);

            yield return null;
        }
    }
}