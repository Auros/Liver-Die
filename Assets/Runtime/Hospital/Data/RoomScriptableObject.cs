using LiverDie.Hospital.Generation;
using UnityEngine;

namespace LiverDie.Hospital.Data
{
    [CreateAssetMenu(fileName = "RoomSO", menuName = "Royal/Liver Die/RoomSO", order = 0)]
    public class RoomScriptableObject : ScriptableObject
    {
        [Min(0.01f), SerializeField]
        private float _length = 5f;

        [SerializeField]
        private RoomDefinition _definitionPrefab = null!;
    }
}
