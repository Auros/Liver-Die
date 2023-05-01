using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LiverDie
{
    public class SliderContainer : MonoBehaviour
    {
        [SerializeField]
        private Slider _slider = null!;

        [SerializeField]
        private TextMeshProUGUI _textMeshPro = null!;

        private float _lastValue;

        public float SliderValue
        {
            get => _slider.value;
            set => _slider.value = value;
        }

        private void Update()
        {
            if (Math.Abs(_lastValue - SliderValue) < 0.01f)
                return;

            _lastValue = SliderValue;
            _textMeshPro.text = $"{Mathf.RoundToInt(_slider.value * 100)}%";
        }
    }
}
