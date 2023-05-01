using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LiverDie.Hospital.Items
{
    public class PickRandomObjectPrefab : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> _gameObjects = new();

        [SerializeField]
        private bool _instantiate = false;

        [SerializeField]
        private float _spawnChance = 1f;

        void Start()
        {
            if (_gameObjects == null || _gameObjects.Count == 0) return;

            Debug.Log("SPAWN CHANGE");
            Debug.Log(_spawnChance);
            var toSpawn = Random.value <= _spawnChance;
            if (!toSpawn)
            {
                foreach (var otherGameObject in _gameObjects)
                {
                    otherGameObject.SetActive(false);
                }
                return;
            }
            
            var randomIndex = Random.Range(0, _gameObjects.Count);
            if (_instantiate)
            {
                var instantiated = Instantiate(_gameObjects[randomIndex], transform, false);
                instantiated.transform.localPosition = Vector3.zero;
                instantiated.transform.localRotation = Quaternion.identity;
            }
            else
            {
                var selectedGameObject = _gameObjects[randomIndex];
                foreach (var otherGameObject in _gameObjects)
                {
                    if(otherGameObject != selectedGameObject) otherGameObject.SetActive(false);
                }
                selectedGameObject.SetActive(true);
            }
        }
    }
}
