using AuraTween;
using LiverDie.Audio;
using LiverDie.Gremlin.Health;
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
            _liverController.LiverDecay = true;
            _tweenManagerIHardlyKnowHer.Run(0f, 1, 2.5f, UpdateAlpha, Easer.InOutSine);
            _tweenManagerIHardlyKnowHer.Run(2.326909f, -6.764f, 5f, UpdateWallPos, Easer.InOutSine);
            _dialogueEventIntermediate.OnNpcDelivered -= DialogueEventIntermediate_OnNpcDelivered;
            _musicController.DisableOverride();
        }

        private void UpdateAlpha(float alpha)
        {
            foreach (var group in _disabledHudElements) group.alpha = alpha;
        }

        private void UpdateWallPos(float x)
            => _wallTransform.localPosition = _wallTransform.localPosition.WithX(x);
    }
}
