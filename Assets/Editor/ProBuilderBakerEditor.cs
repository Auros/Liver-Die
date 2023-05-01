using System;
using System.IO;
using System.Reflection;
using LiverDie.Utilities;
using UnityEditor;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace LiverDie.Editor
{
    [CustomEditor(typeof(ProBuilderBaker))]
    public class ProBuilderBakerEditor : UnityEditor.Editor
    {
        private static readonly MethodInfo _stripMethod;

        static ProBuilderBakerEditor()
        {
            _stripMethod = typeof(CutToolToggle).Assembly
                .GetType("UnityEditor.ProBuilder.Actions.StripProBuilderScripts", true, false)
                .GetMethod("Strip", BindingFlags.Static | BindingFlags.Public);
        }

        public override void OnInspectorGUI()
        {
            var baker = (ProBuilderBaker)target;

            GUI.enabled = !baker.IsBaked;
            if (GUILayout.Button("Bake"))
            {
                var meshes = baker.GetComponentsInChildren<ProBuilderMesh>();
                if (meshes.Length is not 0)
                {
                    var copy = Instantiate(baker.gameObject);
                    DestroyImmediate(copy.GetComponent<ProBuilderBaker>());
                    StageUtility.PlaceGameObjectInCurrentStage(copy);
                    copy.name = $"{baker.name} [Baked]";
                    copy.gameObject.SetActive(true);

                    var copyTransform = copy.transform;
                    var sourceTransform = baker.transform;

                    // ReSharper disable once Unity.InstantiateWithoutParent (In different scenes)
                    copyTransform.SetParent(sourceTransform.parent);
                    copyTransform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation);
                    copyTransform.SetSiblingIndex(sourceTransform.GetSiblingIndex() + 1);

                    meshes = copy.GetComponentsInChildren<ProBuilderMesh>();
                    _stripMethod.Invoke(null, new object[] { meshes });
                    baker.gameObject.SetActive(false);
                    baker.SetBakeTarget(copy);

                    var stagePath = StageUtility.GetStage(baker.gameObject).assetPath;
                    var targetId = AssetDatabase.GUIDFromAssetPath(stagePath);

                    var transformPath = AnimationUtility.CalculateTransformPath(sourceTransform, sourceTransform.root);
                    var relativePath = Path.Combine($"Assets/Models/Baked/${targetId}", transformPath);
                    var dir = Directory.CreateDirectory(relativePath);
                    dir.Delete(true);
                    dir.Create();

                    foreach (var filter in copy.GetComponentsInChildren<MeshFilter>())
                    {
                        AssetDatabase.CreateAsset(filter.sharedMesh, $"{relativePath}.{Guid.NewGuid()}.asset");
                    }
                    AssetDatabase.SaveAssets();
                }
            }

            GUI.enabled = baker.IsBaked;
            if (GUILayout.Button("Restore If Possible"))
            {
                baker.DeleteBakeTarget();
            }

            GUI.enabled = true;
        }
    }
}
