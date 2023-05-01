using System;
using System.Threading;
using AuraTween;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LiverDie.Gremlin.Health;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiverDie.UI
{
    public class LiverBeatController : MonoBehaviour
    {
        [SerializeField]
        private TweenManager _tweenManager = null!;

        [SerializeField, UsedImplicitly]
        private GremlinLiverController _liverController = null!;

        [SerializeField]
        private float _normalBeatScale = 1.1f;

        [SerializeField]
        private float _bigBeatScale = 1.25f;

        [SerializeField]
        private float _beatsPerMinute = 100;

        private CancellationTokenSource _beatCancelSource = new();

        private float _beatLength;
        private Tween? _activeTween;

        private void Start()
        {
            _beatLength = 60 / _beatsPerMinute / 2;
            _liverController.OnLiverDecayed += OnLiverDecayed;
            UniTask.Void(BeatLoop);
        }

        // we are dead
        private void OnLiverDecayed()
        {
            _activeTween?.Cancel();
            _beatCancelSource.Cancel();
        }

        private async UniTaskVoid BeatLoop()
        {
            var token = gameObject.GetCancellationTokenOnDestroy();
            var delay = TimeSpan.FromSeconds(_beatLength);

            while (!token.IsCancellationRequested)
            {
                for (var i = 0; i < 4; i++)
                {
                    if (token.IsCancellationRequested)
                        return;

                    var beatScale = i == 0 ? _bigBeatScale : _normalBeatScale;
                    var tween = _tweenManager.Run(beatScale, 1f, _beatLength, LiverBeat, Easer.OutSine);

                    _activeTween = tween;
                    await tween;
                    _activeTween = null;

                    await UniTask.Delay(delay, true, cancellationToken: token);
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
