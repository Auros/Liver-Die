using System;
using System.Linq;
using LiverDie.Dialogue.Data;
using LiverDie.NPC;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace LiverDie.NPC
{
    // handles visuals, liver, whatever
    public class NpcDefinition : MonoBehaviour
    {
        private static readonly Color _baseShirtColor = new(231 / 255f, 63 / 255f, 87 / 255f);
        private static readonly Color _basePantsColor = new(71 / 255f, 55 / 255f, 46 / 255f);
        private static readonly Color _baseShoeColor = new(42 / 255f, 34 / 255f, 34 / 255f);

        [SerializeField]
        private SkinnedMeshRenderer _renderer = null!;

        [SerializeField]
        private GameObject _liver = null!;

        [SerializeField]
        private DialogueScriptableObject? _dialogue;

        // main rigidbody
        [SerializeField]
        private Rigidbody _ragdollRigidbody = null!;

        [SerializeField]
        [FormerlySerializedAs("Rigidbodies")]
        public Rigidbody[] _rigidbodies = Array.Empty<Rigidbody>();

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

        public bool HasDialogue => _dialogue;

        public string Name => _dialogue ? _dialogue!.Name : string.Empty;

        public string[] Dialogue => _dialogue ? _dialogue!.Dialogue : Array.Empty<string>();

        public bool Interactable { get; private set; } = true;

        private void Start()
        {
            foreach (var body in _rigidbodies)
            {
                body.detectCollisions = false;
                body.isKinematic = true;
            }

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
            Interactable = false;

            foreach (var body in _rigidbodies)
            {
                body.isKinematic = false;
                body.detectCollisions = true;
            }

            var animator = gameObject.GetComponent<Animator>();
            animator.enabled = false;

            _ragdollRigidbody.AddExplosionForce(_deathExplosionForce, position, _deathExplosionRadius);
            _ragdollRigidbody.AddForce(velocity);

            var deathParticlesMain = _deathParticles.main;
            var deathParticleStartSpeed = deathParticlesMain.startSpeed;
            deathParticleStartSpeed.constantMin = _deathParticleMinForce;
            deathParticleStartSpeed.constantMax = _deathParticleMaxForce;
            deathParticleStartSpeed.mode = ParticleSystemCurveMode.TwoConstants;

            var particleTransform = _deathParticles.transform;
            var particlePosition = particleTransform.position;
            var relativePosition = particlePosition - position;
            var awayPosition = particlePosition - relativePosition;
            _deathParticles.transform.rotation = Quaternion.LookRotation(awayPosition, particleTransform.up);

            _deathParticles.Play();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NpcDefinition))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var npc = target as NpcDefinition;
        if (GUILayout.Button("SET RIGIDBODIES (DESTRUCTIVE!!!!!)"))
        {
            npc!._rigidbodies = npc.GetComponentsInChildren<Rigidbody>().ToArray();
        }
    }
}
#endif
