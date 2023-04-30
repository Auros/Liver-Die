using AuraTween;
using Cysharp.Threading.Tasks;
using LiverDie.Runtime.Intermediate;
using UnityEngine;

namespace LiverDie.Audio
{
    public class MusicController : MonoBehaviour
    {
        // I dont know if we actually need this
        [SerializeField]
        private AudioSource _synthLine = null!;

        [SerializeField]
        private AudioSource _drumLine = null!;

        [SerializeField]
        private float _musicVolume = 0.5f;

        [SerializeField]
        private float _fadeDuration = 5;

        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        [SerializeField]
        private TweenManager _tweenManager = null!;

        private float _audioFadeTime = float.MaxValue;

        private void Start()
        {
            _synthLine.volume = _musicVolume;
            _dialogueEventIntermediate.OnNpcDelivered += DialogueEventIntermediate_OnNpcDelivered;
        }

        private void DialogueEventIntermediate_OnNpcDelivered(Runtime.Dialogue.NpcDeliveredEvent obj)
        {
            _drumLine.volume = _musicVolume;
            _audioFadeTime = Time.unscaledTime + 10;
        }

        private void Update()
        {
            if (Time.unscaledTime < _audioFadeTime) return;

            var t = Mathf.Clamp01((_audioFadeTime + _fadeDuration - Time.unscaledTime) / _fadeDuration);
            _drumLine.volume = Mathf.SmoothStep(_musicVolume, 0, 1 - t);
        }

        private void OnDestroy()
        {
            _dialogueEventIntermediate.OnNpcDelivered -= DialogueEventIntermediate_OnNpcDelivered;
        }
    }
}
