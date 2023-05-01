using LiverDie.Audio;
using UnityEditor;
using UnityEngine;

namespace LiverDie.Editor
{
    [CustomEditor(typeof(MusicController))]
    public class MusicControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Up by 5"))
                {
                    var controller = (MusicController)target;
                    controller.IncrementPercent(5);
                }
                if (GUILayout.Button("Down by 5"))
                {
                    var controller = (MusicController)target;
                    controller.DecrementPercent(5);
                }

            }
        }
    }
}
