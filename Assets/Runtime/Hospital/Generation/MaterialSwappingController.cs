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

            Debug.Log("HUH");
            foreach (var rendererInfo in room.RendererInfos)
            {
                Debug.Log("RENDDD");
                for (int i = 0; i < rendererInfo.Materials.Count; i++)
                {
                    var mat = rendererInfo.Materials[i];
                    if (mat == null) continue;

                    Color.RGBToHSV(mat.GetColor("_BaseColor"), out float H, out float S, out float V);
                    Color newColor = Color.HSVToRGB(_currentHueDelta, S, V);
                    /* Material? swappedMaterial = null;

                    if (swappedMaterial == null)
                    {
                        swappedMaterial = Instantiate(mat);
                        // do color
                        Color.RGBToHSV(swappedMaterial.GetColor("_BaseColor"), out float H, out float S, out float V);

                        newColor = Color.HSVToRGB(_currentHueDelta, S, V);
                        swappedMaterial.SetColor("_BaseColor", newColor.Value);
                        swappedMaterial.SetColor("_Color", newColor.Value);
                        materialInfo.Materials.Add(swappedMaterial);
                    }
                    rendererInfo.Renderer.materials[i] = swappedMaterial; */
                    rendererInfo.Renderer.materials[i].color = newColor;
                    rendererInfo.Renderer.materials[i].SetColor("_BaseColor", newColor);
                }
            }
        }
    }
}
