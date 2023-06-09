﻿using System.Collections.Generic;
using LiverDie.Gremlin;
using LiverDie.UI;
using UnityEngine;
using UnityEngine.Audio;

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
        private float _defaultVolMusic;
        [SerializeField]
        private float _maxVolSfx;
        [SerializeField]
        private AudioMixer _audioMixer = null!;
        [SerializeField]
        private SliderContainer _masterSlider = null!;
        [SerializeField]
        private SliderContainer _musicSlider = null!;
        [SerializeField]
        private SliderContainer _sfxSlider = null!;
        [SerializeField]
        private SliderContainer _mouseSensitivitySlider = null!;
        [SerializeField]
        private SliderContainer _liverSlider = null!;
        [SerializeField]
        private PauseController _pauseController = null!;
        [SerializeField]
        private GremlinController _gremlinController = null!;

        private static List<float>? _sliderValues;
        private bool _doSettings;

        private void Start()
        {
            _sliderValues ??= new List<float>
            {
                100,
                _defaultVolMusic,
                100,
                _defaultSensitivity / 100,
                100
            };

            _masterSlider.SliderValue = _sliderValues[0];
            _musicSlider.SliderValue = _sliderValues[1];
            _sfxSlider.SliderValue = _sliderValues[2];
            _mouseSensitivitySlider.SliderValue = _sliderValues[3];
            _liverSlider.SliderValue = _sliderValues[4]; // deserialized for the bit

            _gremlinController.HorizontalSensitivity = Mathf.Lerp(0, 100, _mouseSensitivitySlider.SliderValue);
            _gremlinController.VerticalSensitivity = Mathf.Lerp(0, 100, _mouseSensitivitySlider.SliderValue);

            _doSettings = true;
        }

        public void LoadSettings()
        {
            _doSettings = true;
        }

        private void Update()
        {
            if (!_doSettings || _sliderValues is null)
                return;

            _audioMixer.SetFloat("MasterVolume", SliderToVolume(_masterSlider.SliderValue, _maxVol));
            _audioMixer.SetFloat("SFXVolume", SliderToVolume(_sfxSlider.SliderValue, _maxVolSfx));
            _audioMixer.SetFloat("MusicVolume", SliderToVolume(_musicSlider.SliderValue, _maxVolMusic));
            _gremlinController.HorizontalSensitivity = Mathf.Lerp(0, 100, _mouseSensitivitySlider.SliderValue);
            _gremlinController.VerticalSensitivity = Mathf.Lerp(0, 100, _mouseSensitivitySlider.SliderValue);

            _sliderValues[0] = _masterSlider.SliderValue;
            _sliderValues[1] = _musicSlider.SliderValue;
            _sliderValues[2] = _sfxSlider.SliderValue;
            _sliderValues[3] = _mouseSensitivitySlider.SliderValue;
            _sliderValues[4] = _liverSlider.SliderValue; // serialized for the bit
        }

        public void GoBack()
        {
            _doSettings = false;
            _pauseController.BackToPause();
        }

        private static float SliderToVolume(float slide, float max)
        {
            return Mathf.Lerp(-80, max, slide);
        }
    }
}
