using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private Color _baseShirtColor = new Color(231/255f, 63/255f, 87/255f);
        private Color _basePantsColor = new Color(71/255f, 55/255f, 46/255f);
        private Color _baseShoeColor = new Color(42/255f, 34/255f, 34/255f);

        [SerializeField]
        private bool _forceShirtColor = false;

        [SerializeField]
        private Color _forcedShirtColor = new Color(42/255f, 34/255f, 34/255f);

        [SerializeField]
        private SkinnedMeshRenderer _renderer = null!;

        [SerializeField]
        private GameObject _liver = null!;

        [SerializeField]
        private DialogueScriptableObject? _dialogue;

        [SerializeField]
        [FormerlySerializedAs("Rigidbodies")]
        public Rigidbody[] _rigidbodies = Array.Empty<Rigidbody>();

        [SerializeField]
        private RagdollDefinition _ragdollPrefab = null!;

        [SerializeField]
        private bool _turnIntoRagdoll = true;

        public bool HasDialogue => _dialogue;
        public string Name => _dialogue ? _dialogue!.Name : string.Empty;
        public string[] Dialogue => _dialogue ? _dialogue!.Dialogue : Array.Empty<string>();

        public bool Interactable { get; private set; } = true;

        private void OnEnable()
        {
            Interactable = true;
            _liver.SetActive(true);

            Color.RGBToHSV(_baseShirtColor, out float shirtH, out float shirtS, out float shirtV);
            Color.RGBToHSV(_basePantsColor, out float pantsH, out float pantsS, out float pantsV);
            Color.RGBToHSV(_baseShoeColor, out float shoeH, out float shoeS, out float shoeV);

            if (_forceShirtColor)
            {
                _renderer.materials[1].color = _forcedShirtColor;
            }
            else
            {
                _renderer.materials[1].color = Color.HSVToRGB(Random.value, shirtS, shirtV);
            }
            _renderer.materials[2].color = Color.HSVToRGB(Random.value, pantsS, pantsV);
            _renderer.materials[3].color = Color.HSVToRGB(Random.value, shoeS, shoeV);

            _liver.SetActive(true);
        }

        public void Deliver(Vector3 position, Vector3 velocity)
        {
            this.Interactable = false;
            _liver.SetActive(false);

            if (_turnIntoRagdoll)
            {
                // should be property but im so tired rn somebody please fix this lmao
                var animator = gameObject.GetComponent<Animator>();

                var ragdoll = Instantiate(_ragdollPrefab.gameObject, transform.parent, false);
                ragdoll.GetComponent<RagdollDefinition>().Ragdoll(position, velocity, _renderer.materials[1].color, _renderer.materials[2].color, _renderer.materials[3].color, animator.runtimeAnimatorController);

                ragdoll.transform.localPosition = transform.localPosition;
                //ragdoll.transform.localScale = transform.localScale;
                ragdoll.transform.localRotation = transform.localRotation;

                gameObject.SetActive(false);
            }
        }

        public async void ChangeDialogue(DialogueScriptableObject dialogue)
        {
            // prevent crash lol
            await Task.Delay(150);
            _dialogue = dialogue;
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

        if (!GUILayout.Button("DESTROY RIGIDBODIES (DESTRUCTIVE!!!!!) THIS CANNOT BE UNDONE"))
            return;

        foreach (var rigidBody in npc!._rigidbodies)
        {
            var comp = rigidBody.gameObject.GetComponent<CharacterJoint>();
            DestroyImmediate(comp != null ? comp : rigidBody);
        }
    }
}
#endif
