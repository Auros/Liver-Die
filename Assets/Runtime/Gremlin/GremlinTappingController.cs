using System.Linq;
using UnityEngine;

namespace LiverDie.Gremlin
{
    public class GremlinTappingController : MonoBehaviour
    {
        [SerializeField]
        private GremlinController _gremlinController = null!;

        [SerializeField]
        private Rigidbody _gremlinRigidbody = null!;

        [SerializeField]
        private float _stepsPerMeter = 3;

        private bool _cachedState = true;
        private AudioSource[] _audioSources;
        private float _timeSinceLastStep = 0f;

        private void Start()
        {
            _audioSources = GetComponentsInChildren<AudioSource>();
        }

        private void Update()
        {
            var shouldStep = _gremlinController.IsGrounded && _gremlinController.IsMoving;

            _timeSinceLastStep += Time.deltaTime;

            // speed * stepspermeter = stepspersecond, 1/stepspersecond = time between steps
            if (shouldStep && (_timeSinceLastStep > 1 / (_gremlinRigidbody.velocity.magnitude * _stepsPerMeter)))
            {
                PlayRandomStep();
                _timeSinceLastStep = 0;
            }
        }

        private void PlayRandomStep()
        {
            var availableSources = _audioSources.Where(x => !x.isPlaying).ToArray();
            if (!availableSources.Any())
            {
                Debug.Log("no step audio sources available to step");
                return;
            }
            var step = Random.Range(0, availableSources.Length - 1);
            availableSources[step].Play();
        }
    }
}
