using System;
using System.Collections;
using System.Collections.Generic;
using AuraTween;
using Cysharp.Threading.Tasks;
using LiverDie.Audio;
using LiverDie.Gremlin;
using LiverDie.Gremlin.Health;
using LiverDie.NPC;
using LiverDie.Runtime.Dialogue;
using LiverDie.Runtime.Intermediate;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LiverDie.UI
{
    public class DialogueUIController : MonoBehaviour, LiverInput.IDialogueActions
    {
        private LiverInput _liverInput = null!;

        [SerializeField]
        private TweenManager _tweenManager = null!;
        // for getting camera based raycasts
        [SerializeField]
        private DialogueEventIntermediate _dialogueEventIntermediate = null!;

        [SerializeField]
        private GameObject _interactPrompt = null!;
        [SerializeField]
        private GameObject _deliverPrompt = null!;

        [SerializeField]
        private GameObject _dialogueBox = null!;
        [SerializeField]
        private TextMeshProUGUI _dialogueNameText = null!;
        [SerializeField]
        private TextMeshProUGUI _dialogueText = null!;

        [SerializeField]
        private DialogLibrarySO _dialogLibrary = null!;

        [SerializeField]
        private AudioPool _audioPool = null!;

        [SerializeField]
        private float _charactersPerMinute = 300;

        [SerializeField]
        private float _deliverPromptDelay = 2;
        [SerializeField]
        private float _deliverPromptIn = 3;
        [SerializeField]
        private float _deliverPromptScaleAmount = 0.75f;
        

        // Start is called before the first frame update
        private NpcDefinition? _npcDefinition = null;

        private bool _talking;
        private bool _skipRequested; // skips to end of dialogue, if already done then goes to next page
        private bool _finishRequested;

        void Start()
        {
            (_liverInput = new LiverInput()).Dialogue.AddCallbacks(this);
            _liverInput.Dialogue.Enable();

            _dialogueEventIntermediate.OnNpcSelected += OnNpcSelected;
            _dialogueEventIntermediate.OnNpcDeselected += OnNpcDeselected;
            _dialogueEventIntermediate.OnNpcDelivered += OnNpcDelivered;

            _interactPrompt.SetActive(false);
            _deliverPrompt.SetActive(false);
            _dialogueBox.SetActive(false);
        }

        private void OnNpcDeselected(NpcDeselectedEvent ctx)
        {
            if (_talking) return;

            _interactPrompt.SetActive(false);
            _npcDefinition = null;
        }

        private void OnNpcSelected(NpcSelectedEvent ctx)
        {
            if (_talking || !ctx.Npc || !ctx.Npc.HasDialogue)
                return;

            _interactPrompt.SetActive(true);
            _npcDefinition = ctx.Npc;
        }

        // Update is called once per frame

        public async void OnTalk(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (_talking || _npcDefinition == null) return; // (?)

            _talking = true;
            _dialogueEventIntermediate.ChangeDialogueFocus(true);
            await HandleDialogue();
        }

        private void OnNpcDelivered(NpcDeliveredEvent ctx)
        {
            if(!_talking && _interactPrompt.activeSelf) _interactPrompt.SetActive(false);
            if (!_talking && _npcDefinition != null)
            {
                _npcDefinition = null;
            }
            if (!_talking || _npcDefinition == null) return;

            _finishRequested = true;
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (!_talking || _npcDefinition == null) return;

            _skipRequested = true;
        }

        // eeeeeh
        private async UniTask HandleDialogue()
        {
            if (_npcDefinition == null)
                return;

            var characterDelay = TimeSpan.FromSeconds(1 / (_charactersPerMinute / 60));

            _interactPrompt.SetActive(false);
            _dialogueBox.SetActive(true);
            _dialogueNameText.SetText(_npcDefinition.Name);
            _dialogueText.SetText("");

            for (var i = 0; i < _npcDefinition.Dialogue.Length; i++)
            {
                // do each thing of dialogue individually
                for (var j = 0; j < _npcDefinition.Dialogue[i].Length; j++)
                {
                    if(j == 0) _dialogueText.SetText("");

                    if (_skipRequested || _finishRequested)
                    {
                        _dialogueText.SetText(_npcDefinition.Dialogue[i]);
                        _skipRequested = false;
                        break;
                    }

                    var character = _npcDefinition.Dialogue[i][j];

                    _dialogueText.SetText(_dialogueText.text + character);

                    if (char.IsLetter(character) && _dialogLibrary.TryGetClipForCharacter(character, out var clip))
                    {
                        _audioPool.Play(clip);
                    }

                    await UniTask.Delay(characterDelay);
                }

                // TODO: add "waiting" visuals here
                await UniTask.WaitUntil(() => _skipRequested || _finishRequested);

                if (_skipRequested && i < _npcDefinition.Dialogue.Length - 1)
                {
                    _skipRequested = false;
                    continue;
                }

                // TODO: make "press e to deliver" prompt slowly get bigger
                await UniTask.Delay(TimeSpan.FromSeconds(_deliverPromptDelay));
                Image deliverImage = _deliverPrompt.GetComponent<Image>();
                TextMeshProUGUI deliverText = _deliverPrompt.GetComponentInChildren<TextMeshProUGUI>();
                _deliverPrompt.SetActive(true);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _tweenManager.Run(0, 1, _deliverPromptIn, x =>
                {
                    Color col = deliverImage.color;
                    col.a = x;
                    deliverImage.color = col;

                    col = deliverText.color;
                    col.a = x;
                    deliverText.color = col;
                }, Easer.FastLinear);
                Vector2 size = new Vector2(_deliverPromptScaleAmount, _deliverPromptScaleAmount);
                _tweenManager.Run(size, Vector2.one, _deliverPromptIn, x =>
                {
                    _deliverPrompt.transform.localScale = x;
                }, Easer.FastLinear);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                await UniTask.WaitUntil(() => _finishRequested);

                // clean up
                FinishDialogue();
                break;
            }
        }

        private void FinishDialogue()
        {
            _dialogueEventIntermediate.ChangeDialogueFocus(false);

            _talking = false;
            _skipRequested = false;
            _finishRequested = false;

            _dialogueBox.SetActive(false);
            _interactPrompt.SetActive(false);
            _deliverPrompt.SetActive(false);
            _npcDefinition = null;
        }
    }
}
