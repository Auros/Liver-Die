using System.Collections;
using System.Collections.Generic;
using AuraTween;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiverDie
{
    public class PostProcessingController : MonoBehaviour
    {
        [SerializeField]
        private Volume _blurVolume;
        [SerializeField]
        private TweenManager _tweenManager;
        // Start is called before the first frame update
        public void Blur(float time)
        {
            _tweenManager.Run(0, 1, time, (x) => { _blurVolume.weight = x; }, Easer.InOutSine);
        }
        public void UnBlur(float time)
        {
            _tweenManager.Run(1, 0, time, (x) => { _blurVolume.weight = x; }, Easer.InOutSine);
        }
    }
}
