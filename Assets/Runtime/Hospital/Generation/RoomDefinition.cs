using JetBrains.Annotations;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public class RoomDefinition : MonoBehaviour
    {
        [SerializeField]
        private Transform _roomRoot = null!;

        [SerializeField, UsedImplicitly]
        private BoxCollider _roomBounds = null!;

        [SerializeField, UsedImplicitly]
        private EntranceDefinition _entranceDefinition = null!;

        public Vector3 Size => _roomBounds.size;

        /// <summary>
        /// Moves this door to the target pose based on its entrance location.
        /// </summary>
        /// <param name="pose">The target pose to move to.</param>
        public void MoveTo(Pose pose)
        {
            var roomRoot = GetRoomRoot();
            var entranceLocation = _entranceDefinition.Location;

            var entranceOffsetPos = roomRoot.position - entranceLocation.position;
            var entranceOffsetRot = Quaternion.Inverse(roomRoot.rotation) * entranceLocation.rotation;
            roomRoot.SetPositionAndRotation(entranceOffsetPos + pose.position, entranceOffsetRot * pose.rotation);
            //roomRoot.SetPositionAndRotation(pose.position, pose.rotation);
        }

        private Transform GetRoomRoot() => _roomRoot ? _roomRoot : transform;
    }
}
