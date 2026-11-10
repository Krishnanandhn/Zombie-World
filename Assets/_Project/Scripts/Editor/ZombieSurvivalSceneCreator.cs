using System.Collections.Generic;
using System.IO;
using Project.Data;
using Project.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.EditorTools
{
    // Creates the ZombieSurvival scene and the default content assets once. The game itself is built at runtime by the bootstrap.
    [InitializeOnLoad]
    public static class ZombieSurvivalSceneCreator
    {
        const string ScenePath = "Assets/_Project/Scenes/" + ZombieSurvivalBootstrap.SceneName + ".unity";
        const string DataRoot = "Assets/_Project/Data";
        const string ContentFolder = "Assets/_Project/Resources/ZombieSurvival";
        const string ContentPath = ContentFolder + "/GameContent.asset";

        static readonly string[] RequiredShaders =
        {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Unlit",
            "Universal Render Pipeline/Particles/Unlit"
        };

        static ZombieSurvivalSceneCreator()
        {
            EditorApplication.delayCall += AutoCreate;
        }

        static void AutoCreate()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Retry once the editor is back in edit mode.
                EditorApplication.playModeStateChanged += RetryAfterPlayMode;
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AutoCreate;
                return;
            }

            CreateAll(true);
            Debug.Log($"[ZombieSurvival] Created {ScenePath} and default content assets.");
        }

        static void RetryAfterPlayMode(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.playModeStateChanged -= RetryAfterPlayMode;
            EditorApplication.delayCall += AutoCreate;
        }

        [MenuItem("Tools/Zombie Survival/Create Scene And Content")]
        static void CreateFromMenu() => CreateAll(true);

        [MenuItem("Tools/Zombie Survival/Open Scene")]
        static void OpenFromMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) CreateAll(false);
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        static void CreateAll(bool openAfter)
        {
            CreateContentAssets();
            CreateScene();
            AddToBuildSettings();
            EnsureShadersIncluded();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (openAfter && !HasDirtyScene()) EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        static void CreateScene()
        {
            EnsureFolder(Path.GetDirectoryName(ScenePath).Replace('\\', '/'));
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
        }

        static void CreateContentAssets()
        {
            if (AssetDatabase.LoadAssetAtPath<GameContent>(ContentPath) != null) return;

            EnsureFolder(ContentFolder);
            GameContent content = DefaultContent.Create();

            Save(content.Player, DataRoot + "/Player");
            Save(content.Level, DataRoot + "/Levels");
            foreach (WeaponDefinition w in content.Weapons) Save(w, DataRoot + "/Weapons");
            foreach (EnemyDefinition e in content.Enemies) Save(e, DataRoot + "/Enemies");
            foreach (UpgradeDefinition u in content.Upgrades) Save(u, DataRoot + "/Upgrades");

            AssetDatabase.CreateAsset(content, ContentPath);
        }

        static void Save(Object asset, string folder)
        {
            EnsureFolder(folder);
            AssetDatabase.CreateAsset(asset, $"{folder}/{asset.name}.asset");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
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

        // Materials are built from shader names at runtime, so builds must not strip these shaders.
        static void EnsureShadersIncluded()
        {
            var graphics = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset");
            if (graphics == null) return;

            var serialized = new SerializedObject(graphics);
            SerializedProperty list = serialized.FindProperty("m_AlwaysIncludedShaders");
            if (list == null) return;

            foreach (string name in RequiredShaders)
            {
                Shader shader = Shader.Find(name);
                if (shader == null || Contains(list, shader)) continue;

                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = shader;
            }

            serialized.ApplyModifiedProperties();
        }

        static bool Contains(SerializedProperty list, Shader shader)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == shader) return true;
            }

            return false;
        }
    }
}
