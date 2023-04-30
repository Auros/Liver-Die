using System;
using LiverDie.Runtime.Dialogue;
using LiverDie.Runtime.Intermediate;
using UnityEditor;
using UnityEngine;

namespace LiverDie.Gremlin.Health
{
    /// <summary>
    /// Also known as the health controller lmao
    /// </summary>
    // TODO: Events for change of liver and liver hits 0 (de-live)
    public class GremlinLiverController : MonoBehaviour
    {
        public event Action? OnLiverDecayed;
        public event Action<LiverUpdateEvent>? OnLiverUpdate;

        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        /// <summary>
        /// Current liver/health of gremlin
        /// </summary>
        public float LiverAmount
        {
            get => _liver;
            private set
            {
                var oldLiver = _liver;
                _liver = value;
                OnLiverUpdate?.Invoke(new LiverUpdateEvent(value, value - oldLiver));

                if (LiverAmount == 0) OnLiverDecayed?.Invoke();
            }
        }

        /// <summary>
        /// Toggles on/off liver decay.
        /// </summary>
        public bool LiverDecay { get; set; } = true;

        [SerializeField, Tooltip("Time before liver decay starts")]
        private float _liverDecayOffset = 5f;

        [SerializeField, Tooltip("Time (in seconds) of complete liver decay (1 to 0)")]
        private float _liverDecayTime = 20f;


        private float _liver = 1f;
        private float _lastLiverGainTime;

        /// <summary>
        /// Adds the given <paramref name="liverAmount"/> and resets the decay timer.
        /// </summary>
        /// <param name="liverAmount">Amount of liver to give to/take from the gremlin</param>
        public void ChangeLiver(float liverAmount = 1)
        {
            LiverAmount = Mathf.Clamp01(LiverAmount + liverAmount);
            _lastLiverGainTime = Time.time;
        }

        private void Start()
        {
            _lastLiverGainTime = Time.time;
            _dialogueEventIntermediate.OnDialogueFocusChanged += OnDialogueFocusChanged;
            _dialogueEventIntermediate.OnNpcDelivered += OnNpcDelivered;
        }

        private void OnNpcDelivered(NpcDeliveredEvent ctx)
        {
            ChangeLiver(0.4f); // might break when npc is null but it's probably fine
        }

        private void OnDialogueFocusChanged(DialogueFocusChangedEvent ctx)
        {
            if (ctx.Focused)
            {
                LiverDecay = false;
            }
            else
            {
                LiverDecay = true;
            }
        }

        private void Update()
        {
            if (Time.time < _lastLiverGainTime + _liverDecayOffset) return;

            if (!LiverDecay) return;

            if (LiverAmount == 0) return;

            if (Time.timeScale == 0) return;

            LiverAmount = Mathf.Clamp01(LiverAmount - 1f / _liverDecayTime * Time.deltaTime);
        }
    }
}
