using System;
using UnityEngine;

namespace LiverDie.Hospital
{
    public class ReceptionController : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] _receptionVisuals = Array.Empty<GameObject>();

        public void ShowReception()
        {
            foreach (var visual in _receptionVisuals)
                visual.SetActive(true);
        }

        public void HideReception()
        {
            foreach (var visual in _receptionVisuals)
                visual.SetActive(false);
        }
    }
}
