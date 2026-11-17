using ClassroomQuiz.Gameplay;
using ClassroomQuiz.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClassroomQuiz.Managers
{
    internal static class ClassroomQuizSceneBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneListener()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "LoginScene" && scene.name != "StartScene" && scene.name != "GamePlay" && scene.name != "EndScene")
            {
                return;
            }

            Application.runInBackground = true;
            if (scene.name == "GamePlay" || scene.name == "EndScene")
            {
                DisableLegacyCanvases(scene);
                if (scene.name == "GamePlay")
                {
                    DisableLegacyNetworkObjects(scene);
                }
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "ClassroomQuizRuntimeUI")
                {
                    return;
                }
            }

            var runtimeRoot = new GameObject("ClassroomQuizRuntimeUI");
            switch (scene.name)
            {
                case "LoginScene":
                    runtimeRoot.AddComponent<LoginSceneUIController>();
                    break;
                case "StartScene":
                    runtimeRoot.AddComponent<StartSceneUIController>();
                    break;
                case "GamePlay":
                    runtimeRoot.AddComponent<TcpGameplayController>();
                    break;
                case "EndScene":
                    runtimeRoot.AddComponent<EndSceneUIController>();
                    break;
            }
        }

        private static void DisableLegacyNetworkObjects(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var oldManagers = root.GetComponentsInChildren<QuizGameManager>(true);
                foreach (var manager in oldManagers)
                {
                    if (manager != null)
                    {
                        manager.gameObject.SetActive(false);
                    }
                }

                var oldNetworkManagers = root.GetComponentsInChildren<Unity.Netcode.NetworkManager>(true);
                foreach (var manager in oldNetworkManagers)
                {
                    if (manager != null)
                    {
                        manager.gameObject.SetActive(false);
                    }
                }
            }
        }

        private static void DisableLegacyCanvases(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var canvases = root.GetComponentsInChildren<Canvas>(true);
                foreach (var canvas in canvases)
                {
                    if (canvas != null)
                    {
                        canvas.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
