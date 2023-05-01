using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LiverDie.Hospital.Items
{
    public class XRayController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _xrayGameObject = null!;

        [SerializeField]
        private AudioSource _audioSource = null!;

        private CancellationTokenSource _cts = new();

        private void OnEnable() => _xrayGameObject.SetActive(false);

        private void OnTriggerEnter(Collider other)
        {
            if (_xrayGameObject.activeSelf) return;

            XRayAsync(_cts.Token).Forget();
        }

        // this works fuck you
        private void OnTriggerExit(Collider other)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new();
            _audioSource.Stop();
        }

        private async UniTask XRayAsync(CancellationToken cancelToken = default)
        {
            await UniTask.Delay(2000, cancellationToken: cancelToken).SuppressCancellationThrow();

            _xrayGameObject.SetActive(true);
            _audioSource.Play();

            await UniTask.Delay(1000, cancellationToken: cancelToken).SuppressCancellationThrow();
            _audioSource.Stop();
        }
    }
}
