using System;
using System.Collections.Generic;
using System.Linq;
using AuraTween;
using Cysharp.Threading.Tasks;
using LiverDie.Gremlin.Health;
using LiverDie.Runtime.Intermediate;
using UnityEngine;
using UnityEngine.Audio;

namespace LiverDie.Audio
{

    public class MusicController : MonoBehaviour
    {
        public enum Stem
        {
            Synth,
            Kicks,
            BreakSnares,
            BreakKicks,
            Noise

        }
        [System.Serializable]
        private class StemContainer
        {
            public AudioClip clip;
            public Stem StemType;
            public bool AlwaysPlaying;
            public bool IsSilly;
            public bool IsReplace;
            public bool IsFade = true;
            public bool IsPlaying { get; private set; }
            private AudioSource _audioSource;
            private TweenManager _tweenManager;
            private bool _hasTweenedOnce;
            private Tween _currentTween;
            private float _volume;
            private bool IsTweening { get { return _hasTweenedOnce && _currentTween.IsAlive; } }
            public void Setup(AudioSource audioSource, TweenManager tweenManager, float volume)
            {
                _volume = volume;
                _tweenManager = tweenManager;
                _audioSource = audioSource;
                _audioSource.clip = clip;
                _audioSource.loop = true;
                _audioSource.volume = 0;
                _audioSource.Play();
            }
            public void Play(float fadeTime)
            {
                Debug.Log($"Playing {Enum.GetName(typeof(Stem), StemType)}");
                if(IsTweening)
                {
                    _currentTween.Cancel();
                }
                _currentTween = _tweenManager.Run(_audioSource.volume, _volume, fadeTime, value => _audioSource.volume = value, Easer.FastLinear);
                IsPlaying = true;
                //_currentTween.SetOnComplete(() => { _isTweening = false; Debug.Log("huh wha"); });
            }
            public void Stop(float fadeTime)
            {
                if (AlwaysPlaying) return;
                Debug.Log($"Stopping {Enum.GetName(typeof(Stem), StemType)}");

                if (IsTweening)
                {
                    _currentTween.Cancel();
                }
                _currentTween = _tweenManager.Run(_audioSource.volume, 0f, fadeTime, value => _audioSource.volume = value, Easer.FastLinear);
                IsPlaying = false;
                //_currentTween.SetOnComplete(() => { _isTweening = false; Debug.Log("huh whuh"); });
            }
            public void PlayImmediate()
            {
                if(IsTweening)
                {
                    _currentTween.Cancel();
                }
                _audioSource.volume = _volume;
            }
            public void StopImmediate()
            {
                if (IsTweening)
                {
                    _currentTween.Cancel();
                }
                _audioSource.volume = 0f;
            }
        }
        [SerializeField]
        private float _minPercent = 0;
        [SerializeField]
        private float _maxPercent = 100;
        [SerializeField]
        private TweenManager _tweenManager = null!;
        [SerializeField]
        private GremlinLiverController _liverController;
        [SerializeField]
        private List<StemContainer> _stems = new List<StemContainer>();
        //[SerializeField]
        private List<StemContainer> _normalStems = new List<StemContainer>();
        private List<StemContainer> _sillyStems = new List<StemContainer>();
        private List<(bool, StemContainer)> _beatChangeQueue;
        // I dont know if we actually need this
        private List<AudioSource> _sources = null!;

        [SerializeField]
        private float _musicVolume = 0.5f;

        [SerializeField]
        private float _fadeDuration = 5;

        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;
        [SerializeField]
        private AudioMixerGroup _mixerGroup = null!;
        private int _activatedStems = 0;
        private int _activatedSillyStems = 0;
        private float _precentInterval;
        private float _sillyPercentInterval;
        private float _currentPercent;
        private float _overidePercent;
        private bool _isOverride;
        private void Start()
        {
            //make audio sources
            for (int i = 0; i < _stems.Count; i++)
            {
                StemContainer container = _stems[i];
                AudioSource source = this.gameObject.AddComponent<AudioSource>();

                source.outputAudioMixerGroup = _mixerGroup;
                container.Setup(source, _tweenManager, _musicVolume);
                if (container.IsSilly)
                {
                    _sillyStems.Add(container);
                } else
                {
                    _normalStems.Add(container);
                }
                if (container.AlwaysPlaying)
                {
                    container.PlayImmediate();
                }
            }
            _sillyPercentInterval = (1f / (float)_sillyStems.Count) * 50;
            _precentInterval = (1f / (float)_stems.Count) * 50;
            //_synthLine.volume = _musicVolume;
            //_dialogueEventIntermediate.OnNpcDelivered += DialogueEventIntermediate_OnNpcDelivered;
        }
        public void SetPercent(float percent)
        {
            _currentPercent = Mathf.Clamp(percent, _minPercent, _maxPercent);
            UpdateFromPercent();
        }
        public void IncrementPercent(float percent)
        {
            _currentPercent = Mathf.Clamp(_currentPercent + percent, _minPercent, _maxPercent);
            UpdateFromPercent();
        }
        public void DecrementPercent(float percent)
        {
            _currentPercent = Mathf.Clamp(_currentPercent - percent, _minPercent, _maxPercent);
            UpdateFromPercent();
        }
        public void OverridePercent(float percent)
        {
            _overidePercent = percent;
            _isOverride = true;
            UpdateFromPercent();
        }
        public void DisableOverride()
        {
            _isOverride = false;
            UpdateFromPercent();
        }
        private void UpdateFromPercent()
        {
            float percent = _currentPercent;
            Debug.Log($"calc with {_currentPercent} percent");
            if(_isOverride) percent = _overidePercent;
            if (percent > 50)
            {
                int activations = Mathf.RoundToInt(_sillyStems.Count*((percent-50) / 50));
                if (activations != _activatedSillyStems)
                {
                    _activatedSillyStems = activations;
                    for (int i = 0; i < _sillyStems.Count; i++)
                    {
                        if (activations >= i && !_sillyStems[i].IsPlaying)
                        {
                            if (_sillyStems[i].IsReplace)
                            {
                                foreach (StemContainer stem in _normalStems.Where(x => x.StemType == _sillyStems[i].StemType))
                                {
                                    stem.Stop(_fadeDuration);
                                }
                            }
                            _sillyStems[i].Play(_fadeDuration);
                        }else if (activations <= i && _sillyStems[i].IsPlaying)
                        {
                            if (_sillyStems[i].IsReplace)
                            {
                                foreach (StemContainer stem in _normalStems.Where(x => x.StemType == _sillyStems[i].StemType))
                                {
                                    stem.Play(_fadeDuration);
                                }
                            }
                            _sillyStems[i].Stop(_fadeDuration);
                        }
                    }
                }
            } else
            {
                if (_activatedSillyStems > 0)
                {
                    _activatedSillyStems = 0;
                    foreach (StemContainer stem in _sillyStems.Where(x => x.IsPlaying))
                    {
                        stem.Stop(_fadeDuration);
                    }
                }
                int activations = Mathf.RoundToInt(_normalStems.Count*(percent / 50f));
                Debug.Log($"activations = {activations}, {percent / 50f}");
                if (activations != _activatedStems)
                {
                    for (int i = 0; i < _normalStems.Count; i++)
                    {
                        if (activations >= i && !_normalStems[i].IsPlaying)
                        {
                            _normalStems[i].Play(_fadeDuration);
                        } else if (activations <= i && _normalStems[i].IsPlaying)
                        {
                            _normalStems[i].Stop(_fadeDuration);
                        }
                    }
                }
                _activatedStems = activations;
            }
        }
        private void DialogueEventIntermediate_OnNpcDelivered(Runtime.Dialogue.NpcDeliveredEvent obj)
        {
            //_drumLine.volume = _musicVolume;
            //_audioFadeTime = Time.unscaledTime + 10;
        }

        private void Update()
        {
            if(!_isOverride)
            {
                SetPercent(_liverController.LiverAmount * 100);
                //Debug.Log(_liverController.LiverAmount);
            }
        }

        private void OnDestroy()
        {
            //_dialogueEventIntermediate.OnNpcDelivered -= DialogueEventIntermediate_OnNpcDelivered;
        }
    }
}
