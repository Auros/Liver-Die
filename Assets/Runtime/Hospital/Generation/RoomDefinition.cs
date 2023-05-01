using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LiverDie.Hospital.Data;
using LiverDie.Hospital.Generation;
using LiverDie.NPC;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

        [SerializeField]
        public List<RendererInfo> RendererInfos = new();

        [SerializeField]
        private bool _banDoors;

        [PublicAPI]
        public Vector3 Size => _roomBounds.size;

        public float Length => Size.x;

        public float Depth => Size.z;

        public float EntranceOffsetLength => Vector3.Distance(_entranceDefinition.Location.position, _roomBounds.transform.TransformPoint(_roomBounds.center + new Vector3(-Size.x, -Size.y, -Size.z) * 0.5f));

        public RoomScriptableObject Template { get; set; } = null!;

        public bool BanDoors => _banDoors;

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

        private void OnEnable()
        {
            foreach (var npc in GetComponentsInChildren<NpcDefinition>(true))
            {
                npc.gameObject.SetActive(true);
            }
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(RoomDefinition))]
public class RoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var room = target as RoomDefinition;
        if (GUILayout.Button("SET RENDERERPROPERTIES (DESTRUCTIVE!!!!!)"))
        {
            foreach (var renderer in room!.RendererInfos)
            {
                renderer.Materials = renderer.Renderer.sharedMaterials.ToList();
                var indices = new List<int>();
                for (int i = 0; i < renderer.Materials.Count; i++)
                {
                    indices.Add(i);
                }

                renderer.MaterialIndices = indices;
            }
        }
    }
}
#endif

