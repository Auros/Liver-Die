using LiverDie.Gremlin;
using LiverDie.Gremlin.Health;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace LiverDie.UI
{
    public class DeliveController : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup[] _disabledHudElements = null!;
        [SerializeField]
        private GremlinController _gremlinController = null!;

        [SerializeField]
        private GremlinLiverController _liverController = null!;
        [SerializeField]
        private LiverCounterController _counterController = null!;
        [SerializeField]
        private CanvasGroup _canvasGroup = null!;
        [SerializeField]
        private LiverboardController _boardController = null!;
        [SerializeField]
        private AudioMixer _mixer = null!;
        [SerializeField]
        private float _lowpassAmount = 377;
        [SerializeField]
        private float _normalLowpassAmount = 22000;

        public void Restart()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
            _mixer.SetFloat("LowPass", _normalLowpassAmount);
        }

        public void Quit() => Application.Quit();

        private void Start()
        {
            _liverController.OnLiverDecayed += LiverController_OnLiverDecayed;
        }

        private void LiverController_OnLiverDecayed()
        {
            _gremlinController.IsFocused = false;
            Time.timeScale = 0;
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            foreach (var group in _disabledHudElements) group.alpha = 0;
            _boardController.SendScore(Mathf.RoundToInt(_liverController.StopTimer()*1000), _counterController.Livers, LiverboardController.SessionID);
            _mixer.SetFloat("LowPass", _lowpassAmount);
        }

        private void OnDestroy()
        {
            _liverController.OnLiverDecayed -= LiverController_OnLiverDecayed;
        }
    }
}
