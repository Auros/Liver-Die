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

        private void Start()
        {
            _dialogueEventIntermediate.OnNpcDelivered += DialogueEventIntermediate_OnNpcDelivered;
            _tweenManager.Run(1.2f * Vector3.one, Vector3.one, 0.25f, UpdateScale, Easer.OutSine);
        }

        private void DialogueEventIntermediate_OnNpcDelivered(Runtime.Dialogue.NpcDeliveredEvent obj)
        {
            var objectiveIdx = Random.Range(0, _objectiveTexts.Length);
            UpdateObjectiveAsync(_objectiveTexts[objectiveIdx]).Forget();
        }

        private async UniTask UpdateObjectiveAsync(string text)
        {
            _objectiveText.text = string.Empty;
            var sections = text.Split('|');

            foreach (var section in sections)
            {
                _objectiveText.text += section;

                _audioPool.Play(_thudClip);
                await _tweenManager.Run(1.5f * Vector3.one, Vector3.one, 0.25f, UpdateScale, Easer.OutSine);
                await UniTask.Delay(250);
            }
        }

        private void UpdateScale(Vector3 scale) => transform.localScale = scale;

        private void OnDestroy()
        {
            _dialogueEventIntermediate.OnNpcDelivered -= DialogueEventIntermediate_OnNpcDelivered;
        }
    }
}
