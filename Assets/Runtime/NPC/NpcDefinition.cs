using System.Collections;
using System.Collections.Generic;
using LiverDie.Dialogue.Data;
using UnityEngine;

namespace LiverDie.NPC
{
    // handles visuals, liver, whatever
    public class NpcDefinition : MonoBehaviour
    {
        private Color _baseShirtColor = new Color(231/255f, 63/255f, 87/255f);
        private Color _basePantsColor = new Color(71/255f, 55/255f, 46/255f);
        private Color _baseShoeColor = new Color(42/255f, 34/255f, 34/255f);

        private bool _active = true;

        [SerializeField]
        private SkinnedMeshRenderer _renderer = null!;

        [SerializeField]
        private GameObject _liver = null!;

        [SerializeField]
        private DialogueScriptableObject _dialogue = null!;

        [SerializeField]
        private Rigidbody _ragdollRigidbody = null!;

        [SerializeField]
        private ParticleSystem _deathParticles = null!;

        [SerializeField]
        private float _deathParticleMinForce = 1f;
        private float _deathParticleMaxForce = 2f;

        [SerializeField]
        private float _deathExplosionForce = 1f;

        [SerializeField]
        private float _deathExplosionRadius = 5f;

        public string Name => _dialogue.Name;
        public string[] Dialogue => _dialogue.Dialogue;

        public bool Interactable => _active;

        void Start()
        {
            MaterialPropertyBlock props = new MaterialPropertyBlock();

            Color.RGBToHSV(_baseShirtColor, out float shirtH, out float shirtS, out float shirtV);
            Color.RGBToHSV(_basePantsColor, out float pantsH, out float pantsS, out float pantsV);
            Color.RGBToHSV(_baseShoeColor, out float shoeH, out float shoeS, out float shoeV);

            _renderer.materials[1].color = Color.HSVToRGB(Random.value, shirtS, shirtV);
            _renderer.materials[2].color = Color.HSVToRGB(Random.value, pantsS, pantsV);
            _renderer.materials[3].color = Color.HSVToRGB(Random.value, shoeS, shoeV);

            _liver.SetActive(true);
        }

        public void Deliver(Vector3 position, Vector3 velocity)
        {
            // disable liver visuals & disable interactivity
            // TODO: Animate better
            _liver.SetActive(false);
            _active = false;

            var animator = gameObject.GetComponent<Animator>();
            animator.enabled = false;

            _ragdollRigidbody.AddExplosionForce(_deathExplosionForce, position, _deathExplosionRadius);
            _ragdollRigidbody.AddForce(velocity);

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
    }
}
