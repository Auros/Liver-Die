using UnityEngine;

namespace LiverDie.NPC
{
    public class RagdollDefinition : MonoBehaviour
    {
        [SerializeField]
        private SkinnedMeshRenderer _renderer = null!;

        // main rigidbody
        [SerializeField]

        private Rigidbody _ragdollRigidbody = null!;

        [SerializeField]
        private Animator _animator = null!;

        [SerializeField]
        private GameObject _liver = null!;

        [SerializeField]
        private ParticleSystem _deathParticles = null!;

        [SerializeField]
        private float _deathParticleMinForce = 1f;

        [SerializeField]
        private float _deathParticleMaxForce = 2f;

        [SerializeField]
        private float _deathExplosionForce = 1f;

        [SerializeField]
        private float _deathExplosionRadius = 5f;

        public void Ragdoll(Vector3 position, Vector3 velocity, Color shirtColor, Color pantsColor, Color shoesColor, RuntimeAnimatorController controller)
        {
            // initialize colors
            _renderer.materials[1].color = shirtColor;
            _renderer.materials[2].color = pantsColor;
            _renderer.materials[3].color = shoesColor;

            // disable liver visuals & disable interactivity
            // TODO: Animate better
            _liver.SetActive(false);

            _animator.runtimeAnimatorController = controller;
            _animator.enabled = false;
            _animator.Update(0);

            var rigidbodies = transform.GetComponentsInChildren<Rigidbody>();
            foreach(var rigidbody in rigidbodies)
            {
                rigidbody.AddExplosionForce(_deathExplosionForce, position, _deathExplosionRadius);
                rigidbody.AddForce(velocity);
            }

            var deathParticlesMain = _deathParticles.main;
            var deathParticleStartSpeed = deathParticlesMain.startSpeed;
            deathParticleStartSpeed.constantMin = _deathParticleMinForce;
            deathParticleStartSpeed.constantMax = _deathParticleMaxForce;
            deathParticleStartSpeed.mode = ParticleSystemCurveMode.TwoConstants;

            var relativePosition = _deathParticles.transform.position - position;
            var awayPosition = _deathParticles.transform.position - relativePosition;
            _deathParticles.transform.rotation = Quaternion.LookRotation(awayPosition, _deathParticles.transform.up);

            _deathParticles.Play();
        }

        void OnDisable()
        {
            Destroy(gameObject);
        }
    }
}
