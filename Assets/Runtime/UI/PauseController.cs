using System;
using AuraTween;
using LiverDie.Gremlin;
using LiverDie.Runtime.Intermediate;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace LiverDie.UI
{
    public class PauseController : MonoBehaviour, LiverInput.IPauseActions
    {
        [SerializeField]
        private SettingsManager _settingsManager = null!;
        [SerializeField]
        private GremlinController _gremlinController = null!;
        [SerializeField]
        private TweenManager _tweenManager = null!;
        [SerializeField]
        private CanvasGroup _canvasGroup = null!;
        [SerializeField]
        private DialogueEventIntermediate _dialogueEvent = null!;
        [SerializeField]
        private RectTransform _pauseHud = null!;
        [SerializeField]
        private float _settingsOffset = 1920;
        [SerializeField]
        private float _settingAnimationTime = 1.2f;
        [SerializeField]
        private AudioMixer _mixer = null!;
        [SerializeField]
        private float _lowpassAmount = 377;
        [SerializeField]
        private float _normalLowpassAmount = 22000;
        [SerializeField]
        private PostProcessingController _postProcessingController = null!;

        private LiverInput _liverInput = null!;
        private bool _isPaused = false;
        private bool _pauseBlocked = false;

        private void Start()
        {
            _liverInput = new LiverInput();
            _liverInput.Pause.AddCallbacks(this);
            _liverInput.Enable();
            _dialogueEvent.OnDialogueFocusChanged += DialogueEvent_OnDialogueFocusChanged;
        }

        private void DialogueEvent_OnDialogueFocusChanged(Runtime.Dialogue.DialogueFocusChangedEvent obj) => _pauseBlocked = obj.Focused;

        public void Pause()
        {
            if (_pauseBlocked) return;
            Time.timeScale = 0f;
            _gremlinController.IsFocused = false;
            _postProcessingController.Blur(0f);
            _isPaused = true;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _mixer.SetFloat("LowPass", _lowpassAmount);
        }

        public void Unpause()
        {
            Time.timeScale = 1f;
            _gremlinController.IsFocused = true;
            _postProcessingController.UnBlur(0f);
            _isPaused = false;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _mixer.SetFloat("LowPass", _normalLowpassAmount);
        }
        public void SettingsMenu()
        {
            Vector2 vec = new Vector2(0, 0);
            Vector2 vec2 = new Vector2(_settingsOffset, 0);
            _tweenManager.Run(vec, vec2, _settingAnimationTime, x => _pauseHud.localPosition = x, Easer.OutExpo);
            _settingsManager.LoadSettings();
        }
        public void HowToPlay() => Application.OpenURL("https://en.wikipedia.org/wiki/Acute_liver_failure");

        public void Quit() => Application.Quit();

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (_isPaused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }

        private void OnDestroy()
        {
            _liverInput.Dispose();
            _dialogueEvent.OnDialogueFocusChanged -= DialogueEvent_OnDialogueFocusChanged;
        }

        internal void BackToPause()
        {
            Vector2 vec = new Vector2(0, 0);
            Vector2 vec2 = new Vector2(_settingsOffset, 0);
            //_pauseHud.localPosition = vec;

            _tweenManager.Run(vec2, vec, _settingAnimationTime, x => _pauseHud.localPosition = x, Easer.OutExpo);
        }
    }
}
