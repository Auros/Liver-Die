using System;
using UnityEngine;

namespace LiverDie.Dialogue.Data
{
    [CreateAssetMenu(fileName = "DialogueSO", menuName = "Royal/Liver Die/DialogueSO", order = 0)]
    public class DialogueScriptableObject : ScriptableObject
    {
        [SerializeField]
        private string _name = string.Empty;

        [SerializeField, TextArea(3, 10)]
        private string[] _dialogue = Array.Empty<string>();

        public string Name => _name;

        public string[] Dialogue => _dialogue;
    }
}
