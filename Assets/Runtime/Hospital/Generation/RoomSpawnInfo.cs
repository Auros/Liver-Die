using LiverDie.Hospital.Data;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    [System.Serializable]
    public class RoomSpawnInfo
    {
        [SerializeField]
        private int _weight = 1;

        [SerializeField]
        private RoomScriptableObject _room = null!;

        public int Weight => _weight;

        public RoomScriptableObject Room => _room;
    }
}
