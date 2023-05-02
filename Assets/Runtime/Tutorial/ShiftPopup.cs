using System.Collections;
using System.Collections.Generic;
using AuraTween;
using UnityEngine;

namespace LiverDie
{
    public class ShiftPopup : MonoBehaviour
    {
        [SerializeField]
        private GameObject _popupContainer;
        [SerializeField]
        private TweenManager _tweenManager;
        public static bool HasPressedShift = false;
        private bool _popupIsUp = false;
        Vector2 _pos1 = new Vector2(-491.8f, 0);
        Vector2 _pos2 = new Vector2(61, 0);
        // Start is called before the first frame update
        public void DoPopup()
        {
            if (!HasPressedShift)
            {
                _popupContainer.SetActive(true);

                _tweenManager.Run(_pos1, _pos2, 1, (x) => { _popupContainer.transform.localPosition = x; }, Easer.OutExpo).SetOnComplete(() => { _popupIsUp = true; });
            }
        }
        void Update()
        {
            if(_popupIsUp && HasPressedShift)
            {
                _tweenManager.Run(_pos2, _pos1, 0.5f, (x) => { _popupContainer.transform.localPosition = x; }, Easer.OutExpo).SetOnComplete(() => { _popupIsUp = false; _popupContainer.SetActive(false); });

            }
        }
    }
}
