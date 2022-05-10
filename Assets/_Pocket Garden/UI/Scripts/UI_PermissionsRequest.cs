using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

namespace Buck.PocketGarden
{
    public class UI_PermissionsRequest : MonoBehaviour
    {
        [SerializeField] Button requestCamera;
        [SerializeField] Button requestLocation;

        private void Start()
        {
            requestCamera.onClick.AddListener(RequestCameraPermission);
            requestLocation.onClick.AddListener(RequestLocationPermission);
        }

        void RequestCameraPermission()
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        void RequestLocationPermission()
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

    }
}