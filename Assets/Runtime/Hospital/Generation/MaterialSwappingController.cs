using System;
using System.Collections.Generic;
using System.Linq;
using LiverDie.Gremlin.Health;
using LiverDie.Hospital.Data;
using LiverDie.Runtime.Dialogue;
using LiverDie.Runtime.Intermediate;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public class MaterialSwappingController : MonoBehaviour
    {
        private bool _locked;
        private int _liverCount;
        private float _currentHueDelta;
        private float _previousHueDelta;
        private readonly List<ColoredMaterialInfo> _coloredMaterials = new();
        private readonly int _baseColorProperty = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private int _roomColorChangeEveryXLivers = 5;

        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        public void Start()
        {
            _dialogueEventIntermediate.OnNpcDelivered += OnNpcDelivered;
        }

        private void OnNpcDelivered(NpcDeliveredEvent obj)
        {
            _liverCount++;

            if (_liverCount % _roomColorChangeEveryXLivers != 0 && !_locked)
                return;

            _previousHueDelta = _currentHueDelta;
            _currentHueDelta = (_currentHueDelta + 0.2f) % 1f;
            Lock();
        }

        private void OnLiverUpdate(LiverUpdateEvent ctx)
        {
        }

        private void Recolor(List<RendererInfo> rendererInfos, float progress = 1f)
        {
            if (_currentHueDelta == 0)
                return;

            var materialInfo = _coloredMaterials.FirstOrDefault(x => Math.Abs(x.HueDelta - _currentHueDelta) < 0.01f);
            if (materialInfo == null)
            {
                materialInfo = new ColoredMaterialInfo();
                _coloredMaterials.Add(materialInfo);
            }

            foreach (var rendererInfo in rendererInfos)
            {
                if (rendererInfo == null || !rendererInfo.Renderer.gameObject.activeSelf) continue;

                for (int i = 0; i < rendererInfo.Materials.Count; i++)
                {
                    var mat = rendererInfo.Materials[i];

                    if (mat == null)
                        continue;

                    Color.RGBToHSV(mat.GetColor(_baseColorProperty), out float _, out float s, out float v);
                    var first = _currentHueDelta > _previousHueDelta ? _currentHueDelta : _previousHueDelta;
                    var second = _currentHueDelta > _previousHueDelta ? _previousHueDelta : _currentHueDelta;
                    var newColor = Color.HSVToRGB(Mathf.Lerp(first, second, progress), s, v);

                    /*Material? swappedMaterial = null;

                    if (swappedMaterial == null)
                    {
                        swappedMaterial = Instantiate(mat);
                        // do color

                        swappedMaterial.color = newColor;
                        swappedMaterial.SetColor("_BaseColor", newColor);
                        materialInfo.Materials.Add(swappedMaterial);
                    }
                    rendererInfo.Renderer.materials[i] = swappedMaterial;*/

                    // this is NOT as efficent as it could be
                    rendererInfo.Renderer.materials[i].color = newColor;
                    rendererInfo.Renderer.materials[i].SetColor(_baseColorProperty, newColor);
                }
            }
        }

        public void SetColorOfCorridorSegment(CorridorSegmentDefinition corridorSegment, float transition)
        {
            Recolor(corridorSegment.RendererInfos, 1f - transition);
        }

        public void SetColorOfRoom(RoomDefinition room, float transition)
        {
            Recolor(room.RendererInfos, 1f - transition);
        }

        public void Lock()
        {
            _locked = true;
        }

        public void Unlock()
        {
            _locked = false;
        }
    }
}
