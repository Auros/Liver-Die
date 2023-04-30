using System;
using UnityEngine;

namespace LiverDie.Audio
{
    [CreateAssetMenu(fileName = "quebec", menuName = "Royal/Liver Die/Dialog Library")]
    public class DialogLibrarySO : ScriptableObject
    {
        [SerializeField]
        private AudioClip[] _alphabetDialog = Array.Empty<AudioClip>();

        public bool TryGetClipForCharacter(char character, out AudioClip clip)
        {
            clip = null!;
            character = char.ToLower(character);

            var idx = character - 'a';
            if (idx < 0 || idx >= _alphabetDialog.Length) return false;

            clip = _alphabetDialog[idx];
            return true;
        }
    }
}
