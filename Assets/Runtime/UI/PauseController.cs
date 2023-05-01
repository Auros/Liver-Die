using System;
using AuraTween;
using LiverDie.Gremlin;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LiverDie.UI
{
    public class PauseController : MonoBehaviour, LiverInput.IPauseActions
    {
        [SerializeField]
        private SettingsManager _settingsManager;
        [SerializeField]
        private GremlinController _gremlinController = null!;
        [SerializeField]
        private TweenManager _tweenManager = null!;
        [SerializeField]
        private CanvasGroup _canvasGroup = null!;
        [SerializeField]
        private RectTransform _pauseHud = null!;
        [SerializeField]
        private float _settingsOffset = 1920;
        [SerializeField]
        private float _settingAnimationTime = 1.2f;
        private LiverInput _liverInput = null!;
        private bool _isPaused = false;

        private void Start()
        {
            _liverInput = new LiverInput();
            _liverInput.Pause.AddCallbacks(this);
            _liverInput.Enable();
            
        }

        public void Pause()
        {
            Time.timeScale = 0f;
            _gremlinController.IsFocused = false;
            _isPaused = true;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Unpause()
        {
            Time.timeScale = 1f;
            _gremlinController.IsFocused = true;
            _isPaused = false;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
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
