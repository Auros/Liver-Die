using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LiverDie.Gremlin;
using LiverDie.NPC;
using LiverDie.Runtime.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LiverDie.UI
{
    public class DialogueUIController : MonoBehaviour, LiverInput.IDialogueActions
    {
        private LiverInput _liverInput = null!;

        // for getting camera based raycasts
        [SerializeField]
        private GremlinController _gremlinController = null!;

        [SerializeField]
        private GameObject _interactPrompt = null!;

        [SerializeField]
        private GameObject _dialogueBox = null!;
        [SerializeField]
        private TextMeshProUGUI _dialogueNameText = null!;
        [SerializeField]
        private TextMeshProUGUI _dialogueText = null!;

        [SerializeField]
        private float _charactersPerMinute = 300;

        // Start is called before the first frame update
        private NpcDefinition? _npcDefinition = null;

        private bool _talking;
        private bool _skipRequested; // skips to end of dialogue, if already done then goes to next page
        private bool _finishRequested;

        void Start()
        {
            (_liverInput = new LiverInput()).Dialogue.AddCallbacks(this);
            _liverInput.Dialogue.Enable();
            _gremlinController.OnNpcSelected += GremlinControllerOnNpcSelected;
            _gremlinController.OnNpcDeselected += GremlinControllerOnNpcDeselected;

            _interactPrompt.SetActive(false);
            _dialogueBox.SetActive(false);
        }

        private void GremlinControllerOnNpcDeselected(NpcDeselectedEvent ctx)
        {
            if (_talking) return;

            _interactPrompt.SetActive(false);
            _npcDefinition = null;
        }

        private void GremlinControllerOnNpcSelected(NpcSelectedEvent ctx)
        {
            if (_talking) return;

            _interactPrompt.SetActive(true);
            _npcDefinition = ctx.Npc;
        }

        // Update is called once per frame

        public async void OnTalk(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (_talking || _npcDefinition == null) return; // (?)

            _talking = true;
            await HandleDialogue();
        }

        public void OnDeliver(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
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
            if (_npcDefinition == null) return;

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

                    _dialogueText.SetText(_dialogueText.text + _npcDefinition.Dialogue[i][j]);
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
                await UniTask.WaitUntil(() => _finishRequested);

                // clean up
                FinishDialogue();
                break;
            }
        }

        private void FinishDialogue()
        {
            _talking = false;
            _skipRequested = false;
            _finishRequested = false;

            _dialogueBox.SetActive(false);
            _interactPrompt.SetActive(false);

            if(_npcDefinition != null) _npcDefinition.Deliver();
            _npcDefinition = null;
        }
    }
}
