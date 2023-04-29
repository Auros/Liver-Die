using System.Collections;
using System.Collections.Generic;
using LiverDie.Gremlin.Health;
using UnityEngine;
using UnityEngine.UI;

namespace LiverDie.UI
{
    public class LiverUIController : MonoBehaviour
    {
        [SerializeField]
        private GremlinLiverController _liverController = null!;

        [SerializeField]
        private Image _liverFillImage = null!;

        private void Start()
        {
            _liverController.OnLiverUpdate += _liverController_OnLiverUpdate;
        }

        private void _liverController_OnLiverUpdate(LiverUpdateEvent obj)
        {
            _liverFillImage.fillAmount = obj.NewLiver;
        }

        private void OnDestroy()
        {
            _liverController.OnLiverUpdate -= _liverController_OnLiverUpdate;
        }
    }
}
