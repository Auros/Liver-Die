using System;
using System.Threading;
using AuraTween;
using Cysharp.Threading.Tasks;
using LiverDie.Gremlin.Health;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiverDie.UI
{
    public class LiverBeatController : MonoBehaviour
    {
        [SerializeField]
        private TweenManager _tweenManager = null!;

        [SerializeField]
        private GremlinLiverController _liverController = null!;

        [SerializeField]
        private float _normalBeatScale = 1.1f;

        [SerializeField]
        private float _bigBeatScale = 1.25f;

        [SerializeField]
        private float _beatDuration = 0.25f;

        [SerializeField]
        private float _beatDelay = 0.25f;

        private CancellationTokenSource _beatCancelSource = new();

        private Tween? _activeTween;

        private void Start()
        {
            _liverController.OnLiverDecayed += OnLiverDecayed;

            BeatLoop(_beatCancelSource.Token).Forget();
        }

        // we are dead
        private void OnLiverDecayed()
        {
            _activeTween?.Cancel();
            _beatCancelSource.Cancel();

            _tweenManager.Run(_normalBeatScale, 0.85f, _beatDuration, LiverBeat, Easer.OutSine);
        }

        // i get that technically you're supposed to throw on a cancelled token
        private async UniTask BeatLoop(CancellationToken token = default)
        {
            var delay = TimeSpan.FromSeconds(_beatDelay);
            var normalBeatSize = _normalBeatScale * Vector3.one;
            var bigBeatSize = _bigBeatScale * Vector3.one;

            while (true)
            {
                for (var i = 0; i < 4; i++)
                {
                    if (token.IsCancellationRequested) return;

                    var beatScale = i == 0 ? _bigBeatScale : _normalBeatScale;

                    _activeTween = _tweenManager.Run(beatScale, 1f, _beatDuration, LiverBeat, Easer.OutSine);
                    await _activeTween.Value;
                    _activeTween = null;

                    if (token.IsCancellationRequested) return;

                    await UniTask.Delay(delay);
                }
            }
        }

        private void LiverBeat(float scale) => transform.localScale = scale * Vector3.one;

        private void OnDestroy()
        {
            _beatCancelSource.Dispose();
            _liverController.OnLiverDecayed -= OnLiverDecayed;
        }
    }
}
