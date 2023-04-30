using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LiverDie
{
    public class GremlinIdleSFXController : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _audioSource = null!;

        [SerializeField, Tooltip("Seconds")]
        private float _idleDelayMin = 60;

        [SerializeField, Tooltip("Seconds")]
        private float _idleDelayMax = 120;

        [SerializeField]
        private AudioClip[] _clips = null!;

        [SerializeField]
        private AudioClip[] _rareClips = null!;

        public void GoblinMode()
        {
            var clipArray = UnityEngine.Random.Range(0f, 1f) < 0.05
                ? _rareClips
                : _clips;

            var gremlinIdx = UnityEngine.Random.Range(0, clipArray.Length);
            _audioSource.clip = clipArray[gremlinIdx];
            _audioSource.Play();
        }

        private async UniTaskVoid Start()
        {
            var token = gameObject.GetCancellationTokenOnDestroy();
            while (!token.IsCancellationRequested)
            {
                var delay = TimeSpan.FromSeconds(UnityEngine.Random.Range(_idleDelayMin, _idleDelayMax));
                await UniTask.Delay(delay, cancellationToken: token);

                GoblinMode();
            }
        }
    }
}
