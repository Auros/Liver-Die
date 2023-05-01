using UnityEngine;

namespace LiverDie.Utilities
{
    public class ProBuilderBaker : MonoBehaviour
    {
        [SerializeField]
        private GameObject? _bakedTarget;

        public bool IsBaked => _bakedTarget;

        public void SetBakeTarget(GameObject target)
        {
            _bakedTarget = target;
        }

        public void DeleteBakeTarget()
        {
            if (_bakedTarget == null)
                return;

            if (Application.isPlaying)
                Destroy(_bakedTarget);
            else
                DestroyImmediate(_bakedTarget);

            gameObject.SetActive(true);
            _bakedTarget = null;
        }
    }
}
