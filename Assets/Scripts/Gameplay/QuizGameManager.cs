using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
[Serializable]
public struct QuizQuestionData : INetworkSerializable
{
    public FixedString128Bytes QuestionText;
    public FixedString64Bytes AnswerA;
    public FixedString64Bytes AnswerB;
    public FixedString64Bytes AnswerC;
    public FixedString64Bytes AnswerD;
    public int CorrectIndex; 
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref QuestionText);
        serializer.SerializeValue(ref AnswerA);
        serializer.SerializeValue(ref AnswerB);
        serializer.SerializeValue(ref AnswerC);
        serializer.SerializeValue(ref AnswerD);
        serializer.SerializeValue(ref CorrectIndex);
    }
}
public struct PlayerScoreInfo : INetworkSerializable, IEquatable<PlayerScoreInfo>
{
    public ulong ClientId;
    public int Score;
    public FixedString32Bytes PlayerIP;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Score);
        serializer.SerializeValue(ref PlayerIP);
    }
    public bool Equals(PlayerScoreInfo other) => ClientId == other.ClientId;
}
public class QuizGameManager : NetworkBehaviour
{
    public static QuizGameManager Instance;
    [Header("Game Settings")]
    public float timePerQuestion = 10f;
    public float timeToAnswer = 3f;
    public int maxQuestions = 10;
    public string endSceneName = "EndScene";
    public NetworkVariable<QuizQuestionData> CurrentQuestion = new NetworkVariable<QuizQuestionData>();
    public NetworkVariable<float> TimeRemaining = new NetworkVariable<float>();
    public NetworkVariable<int> CurrentQuestionNumber = new NetworkVariable<int>(1);
    public NetworkVariable<ulong> AnsweringClientId = new NetworkVariable<ulong>(9999);
    public NetworkVariable<float> AnswerTimeRemaining = new NetworkVariable<float>();
    public NetworkList<PlayerScoreInfo> PlayerScores;
    private HashSet<ulong> wrongThisRound = new HashSet<ulong>();
    private Dictionary<ulong, string> playerIPs = new Dictionary<ulong, string>();
    public static List<PlayerScoreInfo> FinalScores = new List<PlayerScoreInfo>();
    private List<QuizQuestionData> questionPool = new List<QuizQuestionData>();
    private void Awake()
    {
        Instance = this;
        PlayerScores = new NetworkList<PlayerScoreInfo>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeQuestions();
            InitializePlayers();
            StartNextQuestion();
        }
        string myIp = RoomController.GetLocalIPAddress();
        RegisterIPServerRpc(myIp);
    }
    [ServerRpc(RequireOwnership = false)]
    private void RegisterIPServerRpc(string ip, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        playerIPs[clientId] = ip;
        int index = GetPlayerIndex(clientId);
        if (index >= 0)
        {
            var p = PlayerScores[index];
            p.PlayerIP = ip;
            PlayerScores[index] = p;
        }
        Debug.Log($"[QuizGame] Player {clientId} IP = {ip}");
        BroadcastScores(); 
    }
    public string GetPlayerName(ulong clientId)
    {
        if (playerIPs.ContainsKey(clientId))
            return playerIPs[clientId];
        return "Player " + clientId;
    }
    private void InitializeQuestions()
    {
        questionPool.Add(new QuizQuestionData { QuestionText = "1 + 1 = ?", AnswerA = "1", AnswerB = "2", AnswerC = "3", AnswerD = "4", CorrectIndex = 1 });
        questionPool.Add(new QuizQuestionData { QuestionText = "Thủ đô của Việt Nam là gì?", AnswerA = "Hà Nội", AnswerB = "TP.HCM", AnswerC = "Đà Nẵng", AnswerD = "Huế", CorrectIndex = 0 });
        questionPool.Add(new QuizQuestionData { QuestionText = "Mặt trời mọc hướng nào?", AnswerA = "Đông", AnswerB = "Tây", AnswerC = "Nam", AnswerD = "Bắc", CorrectIndex = 0 });
        questionPool.Add(new QuizQuestionData { QuestionText = "Con gì có 4 chân?", AnswerA = "Gà", AnswerB = "Vịt", AnswerC = "Chó", AnswerD = "Cá", CorrectIndex = 2 });
        questionPool.Add(new QuizQuestionData { QuestionText = "5 x 5 = ?", AnswerA = "10", AnswerB = "20", AnswerC = "25", AnswerD = "30", CorrectIndex = 2 });
        questionPool.Add(new QuizQuestionData { QuestionText = "Cái gì dùng để chặt cây?", AnswerA = "Búa", AnswerB = "Rìu", AnswerC = "Kéo", AnswerD = "Kiếm", CorrectIndex = 1 });
        questionPool.Add(new QuizQuestionData { QuestionText = "Quả nào có màu đỏ?", AnswerA = "Dưa hấu", AnswerB = "Chuối", AnswerC = "Táo", AnswerD = "Nho xanh", CorrectIndex = 2 });
        questionPool.Add(new QuizQuestionData { QuestionText = "Ai là người tìm ra châu Mỹ?", AnswerA = "Columbus", AnswerB = "Magellan", AnswerC = "Edison", AnswerD = "Newton", CorrectIndex = 0 });
        questionPool.Add(new QuizQuestionData { QuestionText = "10 / 2 = ?", AnswerA = "2", AnswerB = "4", AnswerC = "5", AnswerD = "10", CorrectIndex = 2 });
        questionPool.Add(new QuizQuestionData { QuestionText = "Con nào biết bay?", AnswerA = "Chó", AnswerB = "Mèo", AnswerC = "Chim", AnswerD = "Lợn", CorrectIndex = 2 });
    }
    private void InitializePlayers()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PlayerScores.Add(new PlayerScoreInfo { ClientId = clientId, Score = 0 });
            Debug.Log($"[QuizGame] Thêm Player {clientId} vào bảng điểm.");
        }
    }
    private void StartNextQuestion()
    {
        if (CurrentQuestionNumber.Value > maxQuestions || questionPool.Count == 0)
        {
            EndGame();
            return;
        }
        int randomIndex = UnityEngine.Random.Range(0, questionPool.Count);
        CurrentQuestion.Value = questionPool[randomIndex];
        questionPool.RemoveAt(randomIndex);
        TimeRemaining.Value = timePerQuestion;
        AnsweringClientId.Value = 9999;
        AnswerTimeRemaining.Value = 0;
        wrongThisRound.Clear(); 
        NewRoundClientRpc();
        Debug.Log($"[QuizGame] === Câu {CurrentQuestionNumber.Value}: {CurrentQuestion.Value.QuestionText} ===");
    }
    [ClientRpc]
    private void NewRoundClientRpc()
    {
    }
    private void Update()
    {
        if (!IsServer) return;
        if (AnsweringClientId.Value == 9999)
        {
            TimeRemaining.Value -= Time.deltaTime;
            if (TimeRemaining.Value <= 0)
            {
                CurrentQuestionNumber.Value++;
                StartNextQuestion();
            }
        }
        else
        {
            AnswerTimeRemaining.Value -= Time.deltaTime;
            if (AnswerTimeRemaining.Value <= 0)
            {
                Debug.Log($"[QuizGame] Player {AnsweringClientId.Value} hết 3s, coi như sai!");
                HandleWrongAnswer(AnsweringClientId.Value);
            }
        }
    }
    private void HandleWrongAnswer(ulong clientId)
    {
        wrongThisRound.Add(clientId);
        AnsweringClientId.Value = 9999;
        NotifyWrongClientRpc(clientId);
    }
    [ClientRpc]
    private void NotifyWrongClientRpc(ulong wrongClientId)
    {
    }
    [HideInInspector] public bool localPlayerWrongThisRound = false;
    [ServerRpc(RequireOwnership = false)]
    public void SubmitHandServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (AnsweringClientId.Value == 9999 && !wrongThisRound.Contains(clientId))
        {
            AnsweringClientId.Value = clientId;
            AnswerTimeRemaining.Value = timeToAnswer;
            Debug.Log($"[QuizGame] Player {clientId} giơ tay!");
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SubmitAnswerServerRpc(int answerIndex, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (AnsweringClientId.Value != clientId) return;
        int pIndex = GetPlayerIndex(clientId);
        if (pIndex < 0) return;
        if (answerIndex == CurrentQuestion.Value.CorrectIndex)
        {
            var p = PlayerScores[pIndex];
            p.Score += 1;
            PlayerScores[pIndex] = p;
            Debug.Log($"[QuizGame] Player {clientId} ĐÚNG! Score = {p.Score}");
            BroadcastScores();
            AnsweringClientId.Value = 9999;
            Invoke(nameof(GoToNextQuestion), 0.5f);
        }
        else
        {
            Debug.Log($"[QuizGame] Player {clientId} SAI!");
            HandleWrongAnswer(clientId);
        }
    }
    private void GoToNextQuestion()
    {
        CurrentQuestionNumber.Value++;
        StartNextQuestion();
    }
    private int GetPlayerIndex(ulong clientId)
    {
        for (int i = 0; i < PlayerScores.Count; i++)
        {
            if (PlayerScores[i].ClientId == clientId) return i;
        }
        return -1;
    }
    public static string LatestRankingText = "Dang cho...";
    public void BroadcastScores()
    {
        string text = "BANG DIEM\n";
        for (int i = 0; i < PlayerScores.Count; i++)
        {
            var s = PlayerScores[i];
            string name = GetPlayerName(s.ClientId);
            text += name + ": " + s.Score + " diem\n";
        }
        Debug.Log("[QuizGame] Broadcasting: " + text);
        UpdateScoresClientRpc(text);
    }
    [ClientRpc]
    private void UpdateScoresClientRpc(string rankingText)
    {
        LatestRankingText = rankingText;
        Debug.Log("[QuizGame CLIENT] Nhan ranking: " + rankingText);
    }
    private void EndGame()
    {
        FinalScores.Clear();
        foreach (var s in PlayerScores)
        {
            FinalScores.Add(s);
        }
        NetworkManager.Singleton.SceneManager.LoadScene(endSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
