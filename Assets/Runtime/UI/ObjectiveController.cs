using System.Threading;
using AuraTween;
using Cysharp.Threading.Tasks;
using LiverDie.Audio;
using LiverDie.Runtime.Intermediate;
using TMPro;
using UnityEngine;

namespace LiverDie.UI
{
    public class ObjectiveController : MonoBehaviour
    {
        [SerializeField, Tooltip("Use | to split sections of text")]
        private string[] _objectiveTexts = null!;

        [SerializeField, Space]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        [SerializeField]
        private TweenManager _tweenManager = null!;

        [SerializeField]
        private TextMeshProUGUI _objectiveText = null!;

        [SerializeField]
        private AudioClip _thudClip = null!;

        [SerializeField]
        private AudioPool _audioPool = null!;

        [SerializeField]
        private Color _flashColor = Color.red;

        private CancellationTokenSource? _cts;

        private bool _shouldThud;

        private void Start()
        {
            _dialogueEventIntermediate.OnNpcDelivered += DialogueEventIntermediate_OnNpcDelivered;
            _tweenManager.Run(1.2f * Vector3.one, Vector3.one, 0.25f, UpdateScale, Easer.OutSine);
        }

        private void DialogueEventIntermediate_OnNpcDelivered(Runtime.Dialogue.NpcDeliveredEvent obj)
        {
            // this works fuck you
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            var objectiveIdx = Random.Range(0, _objectiveTexts.Length);
            UpdateObjectiveAsync(_objectiveTexts[objectiveIdx], _cts.Token).Forget();
        }

        private async UniTask UpdateObjectiveAsync(string text, CancellationToken token = default)
        {
            // this is bad but game jam moment
            if (!_shouldThud) await UniTask.Delay(1000);

            _objectiveText.text = string.Empty;
            var sections = text.Split('|');

            foreach (var section in sections)
            {
                if (token.IsCancellationRequested) return;

                _objectiveText.text += section;

                if (_shouldThud) _audioPool.Play(_thudClip);
                var tween = _tweenManager.Run(_flashColor, Color.white, 0.2f, UpdateTextColor, Easer.InQuint);
                tween.SetOnCancel(ResetTextColor);
                await _tweenManager.Run(1.5f * Vector3.one, Vector3.one, 0.25f, UpdateScale, Easer.OutSine);
                await UniTask.Delay(250);
            }

            _shouldThud = true;
        }

        private void UpdateScale(Vector3 scale) => transform.localScale = scale;

        private void UpdateTextColor(Color color) => _objectiveText.color = color;

        private void ResetTextColor() => _objectiveText.color = Color.white;

        private void OnDestroy()
        {
            _dialogueEventIntermediate.OnNpcDelivered -= DialogueEventIntermediate_OnNpcDelivered;
        }
    }
}
