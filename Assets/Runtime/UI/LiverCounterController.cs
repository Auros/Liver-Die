using LiverDie.Gremlin.Health;
using LiverDie.Runtime.Intermediate;
using TMPro;
using UnityEngine;

namespace LiverDie.UI
{
    public class LiverCounterController : MonoBehaviour
    {
        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        [SerializeField]
        private TextMeshProUGUI _counter = null!;

        private int _livers = 0;

        private void Start()
        {
            _dialogueEventIntermediate.OnNpcDelivered += DialogueEventIntermediate_OnNpcDelivered;
        }

        private void DialogueEventIntermediate_OnNpcDelivered(Runtime.Dialogue.NpcDeliveredEvent obj)
        {
            _counter.text = (++_livers).ToString();
        }

        private void OnDestroy()
        {
            _dialogueEventIntermediate.OnNpcDelivered -= DialogueEventIntermediate_OnNpcDelivered;
        }
    }
}
