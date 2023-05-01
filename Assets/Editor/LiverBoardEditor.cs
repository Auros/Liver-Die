using System.Collections;
using System.Collections.Generic;
using LiverDie.Audio;
using UnityEditor;
using UnityEngine;

namespace LiverDie
{
    [CustomEditor(typeof(LiverboardController))]
    public class LiverBoardEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if(GUILayout.Button("Flood with data"))
            {
                
                LiverboardController controller = (LiverboardController)target;
                controller.name = "FUCK";
                for (int i = 0; i < 200; i++)
                {
                    /*
                    controller.SendScore(
                        Mathf.RoundToInt(Random.value * 20 * 1000),
                        Mathf.RoundToInt((Random.value * 20)),
                        System.Guid.NewGuid().ToString(),
                        false
                        ); */
                }
            }
        }
    }
}
