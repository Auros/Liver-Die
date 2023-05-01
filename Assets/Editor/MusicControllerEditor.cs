using System.Collections;
using System.Collections.Generic;
using LiverDie.Audio;
using UnityEditor;
using UnityEngine;

namespace LiverDie
{
    [CustomEditor(typeof(MusicController))]
    public class MusicControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if(EditorApplication.isPlaying)
            {
                if(GUILayout.Button("Up by 5"))
                {
                    MusicController controller = (MusicController)target;
                    controller.IncrementPercent(5);
                }
                if (GUILayout.Button("Down by 5"))
                {
                    MusicController controller = (MusicController)target;
                    controller.DecrementPercent(5);
                }

            }
        }
    }
}
