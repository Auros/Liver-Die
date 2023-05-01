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

        public int TotalLivers
        {
            get => _totalLivers;
        }
        /// <summary>
        /// Toggles on/off liver decay.
        /// </summary>
        public bool LiverDecay { get; set; } = true;

        [SerializeField, Tooltip("Time before liver decay starts")]
        private float _liverDecayOffset = 5f;

        [SerializeField, Tooltip("Time (in seconds) of complete liver decay (1 to 0)")]
        private float _liverDecayTime = 20f;

        [SerializeField, Tooltip("De-livering health gain")]
        private float _deliverGain = 0.4f;

        private float _liver = 1f;
        private float _lastLiverGainTime;
        private float _totalTimer = 0;
        private int _totalLivers = 0;
        private bool _timerOn = false;
        /// <summary>
        /// Adds the given <paramref name="liverAmount"/> and resets the decay timer.
        /// </summary>
        /// <param name="liverAmount">Amount of liver to give to/take from the gremlin</param>
        public void ChangeLiver(float liverAmount = 1)
        {
            LiverAmount = Mathf.Clamp01(LiverAmount + liverAmount);
            _lastLiverGainTime = Time.time;
            if (liverAmount > 0) _totalLivers++;
        }

        private void Start()
        {
            _lastLiverGainTime = Time.time;
            _dialogueEventIntermediate.OnDialogueFocusChanged += OnDialogueFocusChanged;
            _dialogueEventIntermediate.OnNpcDelivered += OnNpcDelivered;
            _totalLivers = 0;
        }

        private void OnNpcDelivered(NpcDeliveredEvent ctx) => ChangeLiver(_deliverGain);

        private void OnDialogueFocusChanged(DialogueFocusChangedEvent ctx) => LiverDecay = !ctx.Focused;

        private void Update()
        {
            if(_timerOn) _totalTimer += Time.deltaTime;
            if (Time.time < _lastLiverGainTime + _liverDecayOffset) return;

            if (!LiverDecay) return;

            if (LiverAmount == 0) return;

            if (Time.timeScale == 0) return;

            LiverAmount = Mathf.Clamp01(LiverAmount - 1f / _liverDecayTime * Time.deltaTime);
        }
        public void StartTimer()
        {
            _totalTimer = 0;
            _timerOn = true;
        }

        public float StopTimer()
        {
            _timerOn = false;
            return _totalTimer;
        }
    }
}
