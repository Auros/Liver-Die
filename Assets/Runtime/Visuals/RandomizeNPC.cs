using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LiverDie
{
    public class RandomizeNPC : MonoBehaviour
    {
        private Color _baseShirtColor = new Color(231/255f, 63/255f, 87/255f);
        private Color _basePantsColor = new Color(71/255f, 55/255f, 46/255f);
        private Color _baseShoeColor = new Color(42/255f, 34/255f, 34/255f);

        [SerializeField]
        private SkinnedMeshRenderer _renderer = null!;

        void Start()
        {
            MaterialPropertyBlock props = new MaterialPropertyBlock();

            Color.RGBToHSV(_baseShirtColor, out float shirtH, out float shirtS, out float shirtV);
            Color.RGBToHSV(_basePantsColor, out float pantsH, out float pantsS, out float pantsV);
            Color.RGBToHSV(_baseShoeColor, out float shoeH, out float shoeS, out float shoeV);

            _renderer.materials[1].color = Color.HSVToRGB(Random.value, shirtS, shirtV);
            _renderer.materials[2].color = Color.HSVToRGB(Random.value, pantsS, pantsV);
            _renderer.materials[3].color = Color.HSVToRGB(Random.value, shoeS, shoeV);
        }
    }
}
