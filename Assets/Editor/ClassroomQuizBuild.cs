using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ClassroomQuiz.Editor
{
    public static class ClassroomQuizBuild
    {
        private static readonly string[] BuildScenes =
        {
            "Assets/Scenes/LoginScene.unity",
            "Assets/Scenes/StartScene.unity",
            "Assets/Scenes/GamePlay.unity",
            "Assets/Scenes/EndScene.unity"
        };

        [MenuItem("ClassroomQuiz/Build Windows Client (Build/ClassroomQuiz.exe)")]
        public static void BuildWindowsClient()
        {
            string buildPath = "Build/ClassroomQuiz.exe";
            string buildDir = Path.GetDirectoryName(buildPath);
            if (!string.IsNullOrEmpty(buildDir) && !Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = BuildScenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[ClassroomQuiz] Starting Windows build to " + buildPath + "...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[ClassroomQuiz] Build succeeded! Size: {summary.totalSize / (1024 * 1024)} MB. Path: {buildPath}");
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError($"[ClassroomQuiz] Build failed with result: {summary.result}. Errors: {summary.totalErrors}");
            }
        }
    }
}
