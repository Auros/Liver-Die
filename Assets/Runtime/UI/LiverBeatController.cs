using System;
using AuraTween;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiverDie
{
    public class LiverBeatController : MonoBehaviour
    {
        [SerializeField]
        private TweenManager _tweenManager = null!;

        [SerializeField]
        private float _normalBeatScale = 1.1f;

        [SerializeField]
        private float _bigBeatScale = 1.25f;

        [SerializeField]
        private float _beatDuration = 0.25f;

        [SerializeField]
        private float _beatDelay = 0.25f;

        private async UniTask Start()
        {
            var delay = TimeSpan.FromSeconds(_beatDelay);
            var normalBeatSize = _normalBeatScale * Vector3.one;
            var bigBeatSize = _bigBeatScale * Vector3.one;

            while (true)
            {
                for (var i = 0; i < 4; i++)
                {
                    var beatScale = i == 0 ? _bigBeatScale : _normalBeatScale;

                    await _tweenManager.Run(beatScale, 1f, _beatDuration, LiverBeat, Easer.Linear);
                    await UniTask.Delay(delay);
                }
            }
        }

        private void LiverBeat(float scale) => transform.localScale = scale * Vector3.one;
    }
}
