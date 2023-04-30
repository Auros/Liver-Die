using System;
using JetBrains.Annotations;
using LiverDie.Hospital.Data;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public class RoomDefinition : MonoBehaviour
    {

        [SerializeField]
        private Transform _roomRoot = null!;

        [SerializeField]
        private BoxCollider _roomBounds = null!;

        [SerializeField]
        private EntranceDefinition _entranceDefinition = null!;

        [PublicAPI]
        public Vector3 Size => _roomBounds.size;

        public float Length => Size.x;

        public float Depth => Size.z;

        public float EntranceOffsetLength => Vector3.Distance(_entranceDefinition.Location.position, _roomBounds.transform.TransformPoint(_roomBounds.center + new Vector3(-Size.x, -Size.y, -Size.z) * 0.5f));

        public RoomScriptableObject Template { get; set; } = null!;

        /// <summary>
        /// Moves this door to the target pose based on its entrance location.
        /// </summary>
        /// <param name="pose">The target pose to move to.</param>
        public void MoveTo(Pose pose)
        {
            var roomRoot = GetRoomRoot();
            var entranceTransform = _entranceDefinition.transform;
            entranceTransform.GetLocalPositionAndRotation(out var oldPos, out var oldRot);

            var oldParent = roomRoot.parent;
            if (oldParent == roomRoot)
                oldParent = null;

            // :Aware:
            entranceTransform.SetParent(null, true);
            roomRoot.SetParent(entranceTransform, true);
            entranceTransform.SetPositionAndRotation(pose.position, pose.rotation);
            entranceTransform.SetParent(null, true);
            roomRoot.SetParent(oldParent, true);
            entranceTransform.SetParent(roomRoot);
            entranceTransform.SetLocalPositionAndRotation(oldPos, oldRot);
        }

        private Transform GetRoomRoot() => _roomRoot ? _roomRoot : transform;
    }
}
