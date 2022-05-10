using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Buck
{
    public class UI_MessageError : MonoBehaviour
    {

        [SerializeField] TMP_Text messageText;
        [SerializeField] CanvasGroup _console;
        [SerializeField] bool _showConsoleOnError;


        private void Start()
        {
            GeospatialManager.Instance.ErrorStateChanged.AddListener(OnErrorStateUpdated);
        }

        void OnErrorStateUpdated(ErrorState errorState, string message) {

            if (message != null) {
                messageText.text = message + "\n Please restart the app.";
                if(_showConsoleOnError) _console.alpha = 1;
            }
            
        }
    }
}
