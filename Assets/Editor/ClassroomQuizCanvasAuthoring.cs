using System;
using System.IO;
using ClassroomQuiz.Gameplay;
using ClassroomQuiz.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClassroomQuiz.Editor
{
    public static class ClassroomQuizCanvasAuthoring
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/LoginScene.unity",
            "Assets/Scenes/StartScene.unity",
            "Assets/Scenes/GamePlay.unity",
            "Assets/Scenes/EndScene.unity"
        };

        [MenuItem("ClassroomQuiz/Generate Editable Canvas UI")]
        public static void GenerateEditableCanvasUi()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var originalScenePath = SceneManager.GetActiveScene().path;
            var generatedCount = 0;
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            var backupDirectory = Path.Combine(projectRoot ?? Application.dataPath, "Library", "ClassroomQuizSceneBackups", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            try
            {
                foreach (var scenePath in ScenePaths)
                {
                    if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath))
                    {
                        Debug.LogError($"ClassroomQuiz Canvas authoring: scene not found: {scenePath}");
                        continue;
                    }

                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    if (HasAuthoredCanvas(scene))
                    {
                        Debug.Log($"ClassroomQuiz Canvas authoring: already created in {scene.name}.");
                        continue;
                    }

                    Directory.CreateDirectory(backupDirectory);
                    var sceneFile = Path.Combine(projectRoot ?? Application.dataPath, scenePath.Replace('/', Path.DirectorySeparatorChar));
                    File.Copy(sceneFile, Path.Combine(backupDirectory, Path.GetFileName(scenePath)), true);

                    switch (scene.name)
                    {
                        case "LoginScene":
                        {
                            var authoringRoot = new GameObject("Login UI Authoring");
                            var controller = authoringRoot.AddComponent<LoginSceneUIController>();
                            controller.BuildInterface();
                            UnityEngine.Object.DestroyImmediate(authoringRoot);
                            ClassroomQuizSpriteApplier.ApplyToLoginScene();
                            break;
                        }
                        case "StartScene":
                            StartSceneRuntimeUi.Build(null);
                            ClassroomQuizSpriteApplier.ApplyToStartScene();
                            break;
                        case "GamePlay":
                            GameplayRuntimeUi.Build();
                            ClassroomQuizSpriteApplier.ApplyToGameplayScene();
                            break;
                        case "EndScene":
                            EndSceneUIController.BuildUi();
                            ClassroomQuizSpriteApplier.ApplyToEndScene();
                            break;
                    }

                    DisableLegacyCanvasObjects(scene);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    generatedCount++;
                }
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(originalScenePath) && AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScenePath))
                {
                    EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"ClassroomQuiz Canvas authoring: saved editable uGUI canvases in {generatedCount} scenes.");
        }

        private static bool HasAuthoredCanvas(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<ClassroomQuizAuthoredCanvas>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static void DisableLegacyCanvasObjects(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (canvas != null && canvas.GetComponent<ClassroomQuizAuthoredCanvas>() == null)
                    {
                        canvas.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
