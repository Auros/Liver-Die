using LiverDie.Hospital.Data;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    [System.Serializable]
    public class RoomSpawnInfo
    {
        [SerializeField]
        private float _weight = 1;

        [SerializeField]
        private RoomScriptableObject _room = null!;

        public float Weight => _weight;

        public RoomScriptableObject Room => _room;
    }
}
