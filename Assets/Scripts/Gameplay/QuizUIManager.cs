using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class QuizUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelRanking;
    public GameObject panelQuestion;
    [Header("Question UI")]
    public TMP_Text txtQuestionNumber;
    public TMP_Text txtTimeRemaining;
    public TMP_Text txtQuestion;
    public Button btnAnswerA;
    public Button btnAnswerB;
    public Button btnAnswerC;
    public Button btnAnswerD;
    public TMP_Text txtStatus;
    [Header("Ranking UI")]
    public TMP_Text txtRankingScores;

    private TMP_Text txtA;
    private TMP_Text txtB;
    private TMP_Text txtC;
    private TMP_Text txtD;
    private bool _answeredThisQuestion;
    private bool _registered;
    private int _lastQuestionNumber = -1;

    private void Start()
    {
        btnAnswerA.onClick.AddListener(() => OnClickAnswer(0));
        btnAnswerB.onClick.AddListener(() => OnClickAnswer(1));
        btnAnswerC.onClick.AddListener(() => OnClickAnswer(2));
        btnAnswerD.onClick.AddListener(() => OnClickAnswer(3));
        txtA = btnAnswerA.GetComponentInChildren<TMP_Text>();
        txtB = btnAnswerB.GetComponentInChildren<TMP_Text>();
        txtC = btnAnswerC.GetComponentInChildren<TMP_Text>();
        txtD = btnAnswerD.GetComponentInChildren<TMP_Text>();
        RegisterForScores();
    }

    private void Update()
    {
        var manager = QuizGameManager.Instance;
        if (manager == null) return;
        if (!_registered) RegisterForScores();

        var currentQuestion = manager.CurrentQuestionNumber.Value;
        if (currentQuestion != _lastQuestionNumber)
        {
            _lastQuestionNumber = currentQuestion;
            _answeredThisQuestion = false;
        }

        txtTimeRemaining.text = $"Thời gian: {Mathf.CeilToInt(Mathf.Max(0, manager.TimeRemaining.Value))}s";
        txtQuestionNumber.text = $"Câu {currentQuestion}/{manager.maxQuestions}";
        var question = manager.CurrentQuestion.Value;
        txtQuestion.text = question.QuestionText.ToString();
        if (txtA != null) txtA.text = "A. " + question.AnswerA.ToString();
        if (txtB != null) txtB.text = "B. " + question.AnswerB.ToString();
        if (txtC != null) txtC.text = "C. " + question.AnswerC.ToString();
        if (txtD != null) txtD.text = "D. " + question.AnswerD.ToString();

        var canAnswer = !_answeredThisQuestion && manager.TimeRemaining.Value > 0;
        SetAnswersInteractable(canAnswer);
        txtStatus.text = _answeredThisQuestion ? "Đã ghi nhận câu trả lời của bạn." : "Chọn một đáp án trước khi hết giờ.";
        txtRankingScores.text = QuizGameManager.LatestRankingText;
    }

    private void RegisterForScores()
    {
        if (QuizGameManager.Instance == null || QuizGameManager.Instance.PlayerScores == null) return;
        QuizGameManager.Instance.PlayerScores.OnListChanged -= OnScoresChanged;
        QuizGameManager.Instance.PlayerScores.OnListChanged += OnScoresChanged;
        _registered = true;
        RefreshRankingText();
    }

    private void OnScoresChanged(NetworkListEvent<PlayerScoreInfo> changeEvent)
    {
        RefreshRankingText();
    }

    private void RefreshRankingText()
    {
        if (txtRankingScores != null) txtRankingScores.text = QuizGameManager.LatestRankingText;
    }

    private void SetAnswersInteractable(bool enabled)
    {
        btnAnswerA.interactable = enabled;
        btnAnswerB.interactable = enabled;
        btnAnswerC.interactable = enabled;
        btnAnswerD.interactable = enabled;
    }

    private void OnClickAnswer(int index)
    {
        if (_answeredThisQuestion || QuizGameManager.Instance == null) return;
        _answeredThisQuestion = true;
        SetAnswersInteractable(false);
        QuizGameManager.Instance.SubmitAnswerServerRpc(index);
    }

    private void OnDestroy()
    {
        if (QuizGameManager.Instance != null && QuizGameManager.Instance.PlayerScores != null)
        {
            QuizGameManager.Instance.PlayerScores.OnListChanged -= OnScoresChanged;
        }
    }
}
