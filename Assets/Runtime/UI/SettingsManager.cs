using System.Collections;
using System.Collections.Generic;
using LiverDie.Gremlin;
using LiverDie.UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace LiverDie
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField]
        private float _defaultSensitivity;
        [SerializeField]
        private float _maxVol;
        [SerializeField]
        private float _maxVolMusic;
        [SerializeField]
        private float _maxVolSfx;
        [SerializeField]
        private AudioMixer _audioMixer;
        [SerializeField]
        private SliderContainer _masterSlider;
        [SerializeField]
        private SliderContainer _musicSlider;
        [SerializeField]
        private SliderContainer _sfxSlider;
        [SerializeField]
        private SliderContainer _mouseSensitivitySlider;
        [SerializeField]
        private PauseController _pauseController;
        [SerializeField]
        private GremlinController _gremlinController = null!;
        private bool _doSettings;
        // Start is called before the first frame update
        void Start()
        {
            _mouseSensitivitySlider.SliderValue = _defaultSensitivity/100;
            _gremlinController.HorizontalSensitivity = _defaultSensitivity;
            _gremlinController.VerticalSensitivity = _defaultSensitivity;
        }
        public void LoadSettings()
        {
            _doSettings = true;
        }
        // Update is called once per frame
        void Update()
        {
            if(_doSettings )
            {
                _audioMixer.SetFloat("MasterVolume", SliderToVolume(_masterSlider.SliderValue, _maxVol));
                _audioMixer.SetFloat("SFXVolume", SliderToVolume(_sfxSlider.SliderValue, _maxVolSfx));
                _audioMixer.SetFloat("MusicVolume", SliderToVolume(_musicSlider.SliderValue, _maxVolMusic));
                _gremlinController.HorizontalSensitivity = Mathf.Lerp(0, 100, _mouseSensitivitySlider.SliderValue);
                _gremlinController.VerticalSensitivity = Mathf.Lerp(0, 100, _mouseSensitivitySlider.SliderValue);
            }
        }
        public void GoBack()
        {
            _doSettings = false;
            _pauseController.BackToPause();
        }
        private float SliderToVolume(float slide, float max)
        {
            return Mathf.Lerp(-80, max, slide);
        }
    }
}
