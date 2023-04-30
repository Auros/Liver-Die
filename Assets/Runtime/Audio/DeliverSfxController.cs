using LiverDie.Runtime.Dialogue;
using LiverDie.Runtime.Intermediate;
using UnityEngine;

namespace LiverDie.Audio
{
    public class DeliverSfxController : MonoBehaviour
    {
        [SerializeField]
        private AudioClip[] _deliverSfx = null!;

        [SerializeField]
        private AudioPool _audioPool = null!;

        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        void Start()
        {
            _dialogueEventIntermediate.OnNpcDelivered += OnNpcDelivered;
        }

        private void OnNpcDelivered(NpcDeliveredEvent ctx)
        {
            var deliverIdx = UnityEngine.Random.Range(0, _deliverSfx.Length);
            _audioPool.Play(_deliverSfx[deliverIdx]);
        }
    }
}
