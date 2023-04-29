using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LiverDie.Audio;
using UnityEngine;

namespace LiverDie.Audio
{
    public class AnimaleseController : MonoBehaviour
    {
        [SerializeField]
        private DialogLibrarySO _dialogLibrary = null!;

        [SerializeField]
        private AudioPool _audioPool = null!;

        [SerializeField]
        private float _charactersPerMinute = 1200;

        public void SpeakLine(string line)
        {
            SpeakLineAsync(line).Forget();
        }

        private async UniTask SpeakLineAsync(string line)
        {
            var characterDelay = TimeSpan.FromSeconds(1 / (_charactersPerMinute / 60));

            for (var i = 0; i < line.Length; i++)
            {
                var character = line[i];

                if (char.IsLetter(character) && _dialogLibrary.TryGetClipForCharacter(character, out var clip))
                {
                    _audioPool.Play(clip);
                }

                await UniTask.Delay(characterDelay);
            }
        }
    }
}
