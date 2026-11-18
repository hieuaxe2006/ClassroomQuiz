using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassroomQuiz.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace ClassroomQuiz.Network
{
    [Serializable]
    public sealed class ClassroomQuizApiRequest
    {
        public string action = string.Empty;
        public string email = string.Empty;
        public string displayName = string.Empty;
        public string avatarKey = string.Empty;
        public string title = string.Empty;
        public string summary = string.Empty;
        public string password = string.Empty;
        public string token = string.Empty;
        public long roomId;
        public int lessonId;
        public List<LessonQuestionData> questions = new List<LessonQuestionData>();
    }

    [Serializable]
    public sealed class ClassroomQuizApiResponse
    {
        public bool success;
        public string error = string.Empty;
        public long accountId;
        public string email = string.Empty;
        public string displayName = string.Empty;
        public string avatarKey = "A";
        public string token = string.Empty;
        [NonSerialized] public string rawJson = string.Empty;
    }

    public static class ClassroomQuizPreferences
    {
        private static readonly string Profile = ReadProfileArgument();

        public static string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(Key(key), defaultValue);
        public static int GetInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(Key(key), defaultValue);
        public static float GetFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(Key(key), defaultValue);
        public static void SetString(string key, string value) => PlayerPrefs.SetString(Key(key), value);
        public static void SetInt(string key, int value) => PlayerPrefs.SetInt(Key(key), value);
        public static void SetFloat(string key, float value) => PlayerPrefs.SetFloat(Key(key), value);
        public static void Save() => PlayerPrefs.Save();

        private static string Key(string key)
        {
            return string.IsNullOrEmpty(Profile) ? key : $"ClassroomQuiz.Profile.{Profile}.{key}";
        }

        private static string ReadProfileArgument()
        {
            const string prefix = "--cq-profile=";
            var argument = Environment.GetCommandLineArgs()
                .FirstOrDefault(value => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            if (argument == null)
            {
                return string.Empty;
            }

            var profile = new string(argument.Substring(prefix.Length)
                .Where(character => char.IsLetterOrDigit(character) || character == '-' || character == '_')
                .Take(32)
                .ToArray());
            return profile.ToLowerInvariant();
        }
    }

    public static class PhpApiClient
    {
        public const string DefaultUrl = "http://127.0.0.1/ClassroomQuizApi/api.php";
        public const string ApiUrlPreferenceKey = "ClassroomQuiz.ApiUrl";
        public const string AccountIdPreferenceKey = "ClassroomQuiz.AccountId";
        public const string AccountTokenPreferenceKey = "ClassroomQuiz.AccountToken";
        public const string AccountEmailPreferenceKey = "ClassroomQuiz.AccountEmail";

        public static string ConfiguredUrl => ClassroomQuizPreferences.GetString(ApiUrlPreferenceKey, DefaultUrl).Trim();

        public static IEnumerator Post(string apiUrl, ClassroomQuizApiRequest body, string bearerToken, Action<ClassroomQuizApiResponse> completed)
        {
            if (body == null)
            {
                completed?.Invoke(new ClassroomQuizApiResponse { error = "API request is empty." });
                yield break;
            }

            var json = JsonUtility.ToJson(body);
            using (var request = new UnityWebRequest(apiUrl, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = 12;
                request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                request.SetRequestHeader("Accept", "application/json");
                if (!string.IsNullOrWhiteSpace(bearerToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {bearerToken}");
                }

                yield return request.SendWebRequest();

                var response = JsonUtility.FromJson<ClassroomQuizApiResponse>(request.downloadHandler.text);
                if (response == null)
                {
                    response = new ClassroomQuizApiResponse
                    {
                        error = request.result == UnityWebRequest.Result.Success
                            ? "API trả về dữ liệu không đọc được."
                            : request.error
                    };
                }
                else if (request.result != UnityWebRequest.Result.Success && string.IsNullOrWhiteSpace(response.error))
                {
                    response.error = request.error;
                }

                response.rawJson = request.downloadHandler.text;
                completed?.Invoke(response);
            }
        }
    }
}
