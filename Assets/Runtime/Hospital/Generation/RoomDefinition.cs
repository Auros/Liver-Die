using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LiverDie.Hospital.Data;
using LiverDie.NPC;
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

        [SerializeField]
        private List<RendererInfo> _rendererInfos = new();

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

        private void OnEnable()
        {
            foreach (var npc in GetComponentsInChildren<NpcDefinition>(true))
            {
                npc.gameObject.SetActive(true);
            }
        }

        public List<Material> GetMaterials()
        {
            return _rendererInfos.SelectMany(x => x.Materials).ToList();
        }

        public void SetMaterials(List<Material> materials)
        {
            // might not perform great but its probably fine!!
            foreach(var material in materials)
            {
                foreach (var rendererInfo in _rendererInfos)
                {
                    for (int i = 0; i < rendererInfo.Materials.Count; i++)
                    {
                        var rendererMaterial = rendererInfo.Materials[i];
                        if (material.name.StartsWith(rendererMaterial.name))
                        {
                            // replace
                            rendererInfo.Renderer.sharedMaterials[i] = material;
                        }
                    }
                }
            }
        }
    }
}
