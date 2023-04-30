using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public class RailPairDefinition : MonoBehaviour
    {
        [SerializeField]
        private Transform _leftRail = null!;

        [SerializeField]
        private Transform _rightRail = null!;

        public Transform LeftRail => _leftRail;

        public Transform RightRail => _rightRail;
    }
}
