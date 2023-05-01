using System;
using System.Collections;
using System.Collections.Generic;
using AuraTween;
using Cysharp.Threading.Tasks;
using LiverDie.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace LiverDie
{
    public class TitleDropController : MonoBehaviour
    {
        [SerializeField]
        private TweenManager _tweenManager = null!;

        [SerializeField]
        private float _fadeDuration = 0.5f;

        [SerializeField]
        private float _dropDuration = 0.25f;

        [SerializeField]
        private AudioClip _thudClip = null!;

        [SerializeField]
        private AudioPool _audioPool = null!;

        [SerializeField]
        private Image[] _titleDropElements = null!;

        [SerializeField]
        private CanvasGroup[] _allGameElements = null!;

        public async UniTask TitleDropAsync()
        {
            var currentAlphas = new float[_allGameElements.Length];
            for (var i = 0; i < _allGameElements.Length; i++)
            {
                currentAlphas[i] = _allGameElements[i].alpha;
            }

            var fadeTimeSpan = TimeSpan.FromSeconds(_fadeDuration);

            // im sorry
            for (var i = 0; i < _allGameElements.Length; i++)
            {
                var element = _allGameElements[i];

                _ = _tweenManager.Run(currentAlphas[i], 0, _fadeDuration,
                    a => element.alpha = a, Easer.InOutSine);
            }

            await UniTask.Delay(fadeTimeSpan);

            for (var i = 0; i < _titleDropElements.Length; i++)
            {
                var element = _titleDropElements[i];

                _ = _tweenManager.Run(1.5f * Vector3.one, Vector3.one, _dropDuration,
                    (s) => element.transform.localScale = s, Easer.InExpo);

                await _tweenManager.Run(0, 1, _dropDuration,
                    (a) => element.color = element.color.WithA(a), Easer.InExpo);

                _audioPool.Play(_thudClip);
            }

            await UniTask.Delay(fadeTimeSpan);

            // im sorry part 2 electric boogaloo
            for (var i = 0; i < _allGameElements.Length; i++)
            {
                var element = _allGameElements[i];

                _ = _tweenManager.Run(0, currentAlphas[i], _fadeDuration,
                    a => element.alpha = a, Easer.InOutSine);
            }
            for (var i = 0; i < _titleDropElements.Length; i++)
            {
                var element = _titleDropElements[i];

                _ = _tweenManager.Run(Vector3.one, Vector3.zero, _fadeDuration,
                    (s) => element.transform.localScale = s, Easer.InExpo);

                _ = _tweenManager.Run(1f, 0f, _fadeDuration,
                    (a) => element.color = element.color.WithA(a), Easer.InExpo);
            }

            await UniTask.Delay(fadeTimeSpan);
        }
    }
}
