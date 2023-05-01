using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LiverDie
{
    public class SliderContainer : MonoBehaviour
    {
        [SerializeField]
        private Slider _slider;
        [SerializeField]
        private TextMeshProUGUI _textMeshPro;
        public float SliderValue { get { return _slider.value; } set { _slider.value = value; } }
        // Start is called before the first frame update
        void Start()
        {
            SliderValue = 1;
        }

        // Update is called once per frame
        void Update()
        {
            _textMeshPro.text = $"{Mathf.RoundToInt(_slider.value * 100)}%";
        }
    }
}
