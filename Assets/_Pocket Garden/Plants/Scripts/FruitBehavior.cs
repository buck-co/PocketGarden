using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buck.PocketGarden
{
    public class FruitBehavior : MonoBehaviour
    {
        [SerializeField] GameObject _harvestFXPrefab;
        private Plant _plant;
        private float _harvestThreshold,
                      _harvestProgress;

        public void Init(Plant plant)
        {
            _plant = plant;
            _plant.WasShaken.AddListener(OnPlantWasShaken);

            _harvestThreshold = UnityEngine.Random.value;
        }

        private void OnPlantWasShaken(float amount)
        {
            _harvestProgress += amount;

            if (_harvestProgress >= _harvestThreshold)
                Harvest();
        }

        private void Harvest()
        {
            if (_harvestFXPrefab != null)
                Instantiate(_harvestFXPrefab, transform.position, transform.rotation);

            _plant.HarvestFruit(this.transform);

            Destroy(gameObject);
        }
    }
}
