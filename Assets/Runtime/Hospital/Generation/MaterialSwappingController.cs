using System;
using System.Collections.Generic;
using System.Linq;
using LiverDie.Gremlin.Health;
using LiverDie.Hospital.Data;
using LiverDie.Hospital.Generation;
using LiverDie.Runtime.Dialogue;
using LiverDie.Runtime.Intermediate;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LiverDie.Runtime.Hospital.Generation
{
    public class MaterialSwappingController : MonoBehaviour
    {
        private List<ColoredMaterialInfo> _coloredMaterials = new();

        private float _currentHueDelta = 0;
        private int _liverCount = 0;

        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        [SerializeField]
        private int _roomColorChangeEveryXLivers = 5;

        public void Start()
        {
            _dialogueEventIntermediate.OnNpcDelivered += OnNpcDelivered;
        }

        private void OnNpcDelivered(NpcDeliveredEvent obj)
        {
            _liverCount++;

            if (_liverCount % _roomColorChangeEveryXLivers == 0)
            {
                _currentHueDelta = Random.value;
            }
        }

        private void OnLiverUpdate(LiverUpdateEvent ctx)
        {
        }

        public void SetColorOfRoom(RoomDefinition room)
        {
            if (_currentHueDelta == 0) return;

            ColoredMaterialInfo? materialInfo = _coloredMaterials.FirstOrDefault(x => x.HueDelta == _currentHueDelta);
            if (materialInfo == null)
            {
                materialInfo = new ColoredMaterialInfo();
                _coloredMaterials.Add(materialInfo);
            }

            var roomMats = room.GetMaterials();

            for (int i = 0; i < roomMats.Count; i++)
            {
                var mat = roomMats[i];

                Material? swappedMaterial = materialInfo.Materials.FirstOrDefault(x => x.name.Contains(mat.name));
                if (swappedMaterial == null)
                {
                    swappedMaterial = Instantiate(mat);
                    // do color
                    Color.RGBToHSV(swappedMaterial.GetColor("_BaseColor"), out float H, out float S, out float V);

                    swappedMaterial.SetColor("_BaseColor", Color.HSVToRGB(_currentHueDelta, S, V));
                    materialInfo.Materials.Add(swappedMaterial);
                }

                roomMats[i] = swappedMaterial;
            }

            room.SetMaterials(roomMats);
        }
    }
}
