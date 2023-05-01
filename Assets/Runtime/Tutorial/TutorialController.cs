using System;
using AuraTween;
using Cysharp.Threading.Tasks;
using LiverDie.Audio;
using LiverDie.Dialogue.Data;
using LiverDie.Gremlin.Health;
using LiverDie.NPC;
using LiverDie.Runtime.Intermediate;
using UnityEngine;

namespace LiverDie.Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup[] _disabledHudElements = null!;

        [SerializeField]
        private Transform _wallTransform = null!;

        [SerializeField]
        private GremlinLiverController _liverController = null!;
        [SerializeField]
        private MusicController _musicController = null!;

        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        [SerializeField]
        private TweenManager _tweenManagerIHardlyKnowHer = null!;

        [SerializeField]
        private NpcDefinition[] _receptionists = Array.Empty<NpcDefinition>();

        [SerializeField]
        private DialogueScriptableObject _receptionistSecondaryDialogue = null!;

        [SerializeField]
        private AudioSource _doorSfx = null!;

        [SerializeField]
        private TitleDropController _titleDropController = null!;

        private void Start()
        {
            _musicController.OverridePercent(15);
            foreach (var group in _disabledHudElements) group.alpha = 0;

            _liverController.LiverDecay = false;

            _dialogueEventIntermediate.OnNpcDelivered += DialogueEventIntermediate_OnNpcDelivered;
        }

        // only triggers on the first delivery
        private void DialogueEventIntermediate_OnNpcDelivered(Runtime.Dialogue.NpcDeliveredEvent obj)
        {
            TutorialFinishAsync().Forget();
        }

        private async UniTask TutorialFinishAsync()
        {
            await _titleDropController.TitleDropAsync();

            _dialogueEventIntermediate.OnNpcDelivered -= DialogueEventIntermediate_OnNpcDelivered;
            _musicController.DisableOverride();
            _doorSfx.Play();
            _ = _tweenManagerIHardlyKnowHer.Run(2.326909f, -6.764f, 10f, UpdateWallPos, Easer.InOutSine);

            foreach (var receptionist in _receptionists)
            {
                if (receptionist == null || !receptionist.Interactable) continue;
                receptionist.ChangeDialogue(_receptionistSecondaryDialogue);
            }

            await _tweenManagerIHardlyKnowHer.Run(0f, 1, 2.5f, UpdateAlpha, Easer.InOutSine);

            _liverController.LiverDecay = true;
            _liverController.StartTimer();
        }

        private void UpdateAlpha(float alpha)
        {
            foreach (var group in _disabledHudElements) group.alpha = alpha;
        }

        private void UpdateWallPos(float x)
            => _wallTransform.localPosition = _wallTransform.localPosition.WithX(x);
    }
}
