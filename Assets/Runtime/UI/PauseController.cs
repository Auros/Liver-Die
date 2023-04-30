using LiverDie.Gremlin;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LiverDie.UI
{
    public class PauseController : MonoBehaviour, LiverInput.IPauseActions
    {
        [SerializeField]
        private GremlinController _gremlinController = null!;

        [SerializeField]
        private CanvasGroup _canvasGroup = null!;

        private LiverInput _liverInput = null!;

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

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
        }

        public void Unpause()
        {
            Time.timeScale = 1f;
            _gremlinController.IsFocused = true;

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
        }

        public void HowToPlay() => Application.OpenURL("https://en.wikipedia.org/wiki/Acute_liver_failure");

        public void Quit() => Application.Quit();

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            Pause();
        }

        private void OnDestroy()
        {
            _liverInput.Dispose();
        }
    }
}
