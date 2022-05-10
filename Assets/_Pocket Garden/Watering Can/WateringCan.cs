using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class WateringCan : MonoBehaviour
    {
        [SerializeField] GameObject _visualGeo;
        [SerializeField] ParticleSystem _collisionParticles;
        [SerializeField] ParticleSystem _visualParticles;
        [SerializeField] AudioSource _audioSource;

        private ParticleSystem.EmissionModule _em;
        private ParticleSystem.MainModule _main;
        private ParticleSystem.MinMaxCurve _curve;

        bool _en;
        [SerializeField] float _emVal, _emMaxVis, _emMaxCol, _spdMin, _spdMax;

        private void OnEnable()
        {
            _emVal = 0;

            _em = _visualParticles.emission;
            _main = _visualParticles.main;
            _emMaxVis = _em.rateOverTimeMultiplier;
            _em.rateOverTimeMultiplier = 0;
            _spdMin = _main.startSpeed.constantMin;
            _spdMax = _main.startSpeed.constantMax;
            _em.enabled = false;

            _em = _collisionParticles.emission;
            _emMaxCol = _em.rateOverTimeMultiplier;
            _em.rateOverTimeMultiplier = 0;
            _em.enabled = false;
        }

        private void Start()
        {
            GardenManager.Instance.AppStateChanged.AddListener(OnAppStateChanged);
            SetEnabled(false);
        }

        private void OnAppStateChanged(AppState state)
        {
            SetEnabled(state == AppState.Watering);
        }

        void SetEnabled(bool enable)
        {
            _en = enable;

            _em = _collisionParticles.emission;
            _em.enabled = enable;

            _em = _visualParticles.emission;
            _em.enabled = enable;

            _visualGeo.SetActive(enable);

            _audioSource.Stop();
        }

        private void Update()
        {
            if (!_en) return;

            if (Input.touchCount > 0)
            {
                _emVal = Mathf.MoveTowards(_emVal, 1, Time.deltaTime / 0.5f);
                if (!_audioSource.isPlaying && _emVal > 0.25f) _audioSource.Play();
            }
            else
            {
                _emVal = Mathf.MoveTowards(_emVal, 0, Time.deltaTime / 0.5f);
                if (_audioSource.isPlaying) _audioSource.Stop();
            }

            _curve.constantMin = _emVal * _spdMin;
            _curve.constantMax = _emVal * _spdMax;

            /// visual particles
            if (!_visualParticles.isPlaying) _visualParticles.Play();
            _main = _visualParticles.main;
            _main.startSpeed = _curve;

            _em = _visualParticles.emission;
            _em.rateOverTimeMultiplier = _emVal * _emMaxVis;

            /// collision particles
            if (!_collisionParticles.isPlaying) _collisionParticles.Play();

            _main = _collisionParticles.main;
            _main.startSpeed = _curve;

            _em = _collisionParticles.emission;
            _em.rateOverTimeMultiplier = _emVal * _emMaxCol;
        }

        void OnParticleCollision(GameObject other)
        {
            if (!_en) return;

           
            if (other.GetComponent<Plant>() != null)
            {
                other.GetComponent<Plant>().Water();
            }

        }
    }
}