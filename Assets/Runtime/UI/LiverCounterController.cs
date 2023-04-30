using LiverDie.Gremlin.Health;
using TMPro;
using UnityEngine;

namespace LiverDie.UI
{
    public class LiverCounterController : MonoBehaviour
    {
        [SerializeField, Space]
        private GremlinLiverController _liverController = null!;

        [SerializeField]
        private TextMeshProUGUI _counter = null!;

        private int _livers = 0;

        private void Start()
        {
            _liverController.OnLiverUpdate += LiverController_OnLiverUpdate;
        }

        private void LiverController_OnLiverUpdate(LiverUpdateEvent obj)
        {
            // Kinda a bad way to tell if a liver was obtained but......
            if (obj.LiverChange < 0) return;

            _counter.text = (++_livers).ToString();
        }

        private void OnDestroy()
        {
            _liverController.OnLiverUpdate -= LiverController_OnLiverUpdate;
        }
    }
}
