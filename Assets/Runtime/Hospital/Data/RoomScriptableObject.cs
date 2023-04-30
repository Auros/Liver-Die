using LiverDie.Hospital.Generation;
using UnityEngine;

namespace LiverDie.Hospital.Data
{
    [CreateAssetMenu(fileName = "RoomSO", menuName = "Royal/Liver Die/RoomSO", order = 0)]
    public class RoomScriptableObject : ScriptableObject
    {
        [SerializeField]
        private string _roomName = string.Empty;

        [SerializeField]
        private RoomDefinition _definitionPrefab = null!;

        public RoomDefinition Prefab => _definitionPrefab;

        public string RoomName => _roomName;
    }
}
