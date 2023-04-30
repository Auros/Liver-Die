using System.Collections;
using System.Collections.Generic;
using LiverDie.Gremlin;
using UnityEngine;

namespace LiverDie.Gremlin
{
    public class GremlinTappingController : MonoBehaviour
    {
        [SerializeField]
        private GremlinController _gremlinController = null!;

        [SerializeField]
        private AudioSource _audioSource;

        private bool _cachedState = true;

        private void Update()
        {
            var shouldStep = _gremlinController.IsGrounded && _gremlinController.IsMoving;

            if (_cachedState == shouldStep) return;

            _cachedState = shouldStep;

            if (shouldStep)
            {
                _audioSource.Play();
            }
            else
            {
                _audioSource.Stop();
            }
        }
    }
}
