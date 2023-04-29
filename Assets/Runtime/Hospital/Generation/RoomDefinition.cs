using JetBrains.Annotations;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public class RoomDefinition : MonoBehaviour
    {
        [SerializeField, UsedImplicitly]
        private BoxCollider _roomBounds = null!;

        [SerializeField, UsedImplicitly]
        private EntranceDefinition _entranceDefinition = null!;
    }
}
