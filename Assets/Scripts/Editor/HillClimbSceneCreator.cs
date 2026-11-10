using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace PeakRover.EditorTools
{
    // Creates Assets/Scenes/HillClimb.unity once; the game itself is built at runtime by HillBootstrap.
    [InitializeOnLoad]
    public static class HillClimbSceneCreator
    {
        const string ScenePath = "Assets/Scenes/HillClimb.unity";

        static HillClimbSceneCreator()
        {
            EditorApplication.delayCall += AutoCreate;
        }

        static void AutoCreate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return;
            CreateScene(true);
        }

        [MenuItem("Tools/Peak Rover/Create Scene")]
        static void CreateFromMenu()
        {
            CreateScene(true);
        }

        [MenuItem("Tools/Peak Rover/Open Scene")]
        static void OpenFromMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) CreateScene(false);
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        static void CreateScene(bool openAfter)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            AddToBuildSettings();
            AssetDatabase.Refresh();

            if (openAfter && !HasDirtyScene())
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        static bool HasDirtyScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isDirty) return true;
            }
            return false;
        }

        static void AddToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path) == null);
            if (scenes.Exists(s => s.path == ScenePath)) return;

            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
