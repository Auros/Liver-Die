using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LiverDie
{
    public class RunPopupTrigger : MonoBehaviour
    {
        [SerializeField]
        private ShiftPopup _shiftPopup;
        private void OnTriggerEnter(Collider other)
        {
            _shiftPopup.DoPopup();
        }
    }
}
