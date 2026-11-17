using System;
using System.Collections;
using System.Text;
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
        public string password = string.Empty;
        public string token = string.Empty;
        public long roomId;
    }

    [Serializable]
    public sealed class ClassroomQuizApiResponse
    {
        public bool success;
        public string error = string.Empty;
        public long accountId;
        public string email = string.Empty;
        public string displayName = string.Empty;
        public string token = string.Empty;
        [NonSerialized] public string rawJson = string.Empty;
    }

    public static class PhpApiClient
    {
        public const string DefaultUrl = "http://127.0.0.1/ClassroomQuizApi/api.php";
        public const string ApiUrlPreferenceKey = "ClassroomQuiz.ApiUrl";
        public const string AccountIdPreferenceKey = "ClassroomQuiz.AccountId";
        public const string AccountTokenPreferenceKey = "ClassroomQuiz.AccountToken";
        public const string AccountEmailPreferenceKey = "ClassroomQuiz.AccountEmail";

        public static string ConfiguredUrl => PlayerPrefs.GetString(ApiUrlPreferenceKey, DefaultUrl).Trim();

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
