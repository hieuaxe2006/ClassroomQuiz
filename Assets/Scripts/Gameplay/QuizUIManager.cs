using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
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
    public Button btnHand;
    public TMP_Text txtStatus;
    [Header("Ranking UI")]
    public TMP_Text txtRankingScores;
    private TMP_Text txtA, txtB, txtC, txtD;
    private bool iLostThisRound = false;
    private int lastQuestionNumber = -1;
    private void Start()
    {
        btnHand.onClick.AddListener(OnClickHand);
        btnAnswerA.onClick.AddListener(() => OnClickAnswer(0));
        btnAnswerB.onClick.AddListener(() => OnClickAnswer(1));
        btnAnswerC.onClick.AddListener(() => OnClickAnswer(2));
        btnAnswerD.onClick.AddListener(() => OnClickAnswer(3));
        txtA = btnAnswerA.GetComponentInChildren<TMP_Text>();
        txtB = btnAnswerB.GetComponentInChildren<TMP_Text>();
        txtC = btnAnswerC.GetComponentInChildren<TMP_Text>();
        txtD = btnAnswerD.GetComponentInChildren<TMP_Text>();
        if (QuizGameManager.Instance != null && QuizGameManager.Instance.PlayerScores != null)
        {
            QuizGameManager.Instance.PlayerScores.OnListChanged += OnScoresChanged;
            Debug.Log("[QuizUI] Da dang ky OnListChanged thanh cong!");
        }
        else
        {
            Debug.LogWarning("[QuizUI] QuizGameManager.Instance hoac PlayerScores la NULL trong Start()! Se thu lai trong Update().");
        }
    }
    private bool registered = false;
    private void Update()
    {
        if (QuizGameManager.Instance == null) return;
        if (!registered && QuizGameManager.Instance.PlayerScores != null)
        {
            QuizGameManager.Instance.PlayerScores.OnListChanged += OnScoresChanged;
            registered = true;
            Debug.Log("[QuizUI] Da dang ky OnListChanged (tu Update)!");
            RefreshRankingText(); 
        }
        int currentQ = QuizGameManager.Instance.CurrentQuestionNumber.Value;
        if (currentQ != lastQuestionNumber)
        {
            lastQuestionNumber = currentQ;
            iLostThisRound = false;
        }
        UpdateTimerUI();
        UpdateQuestionUI();
        UpdateButtonsState();
        RefreshRankingText();
    }
    private void OnScoresChanged(NetworkListEvent<PlayerScoreInfo> changeEvent)
    {
        Debug.Log($"[QuizUI] Score thay doi! Type={changeEvent.Type}, Index={changeEvent.Index}, Score={changeEvent.Value.Score}");
        RefreshRankingText();
    }
    private void RefreshRankingText()
    {
        if (txtRankingScores == null) return;
        txtRankingScores.text = QuizGameManager.LatestRankingText;
    }
    private void UpdateTimerUI()
    {
        var gm = QuizGameManager.Instance;
        if (gm.AnsweringClientId.Value != 9999)
        {
            float ansTime = gm.AnswerTimeRemaining.Value;
            txtTimeRemaining.text = "Tra loi: " + Mathf.CeilToInt(Mathf.Max(0, ansTime)) + "s";
            txtTimeRemaining.color = Color.yellow;
        }
        else
        {
            float time = gm.TimeRemaining.Value;
            txtTimeRemaining.text = "Thoi gian: " + Mathf.CeilToInt(Mathf.Max(0, time)) + "s";
            txtTimeRemaining.color = (time <= 3f) ? Color.red : Color.white;
        }
    }
    private void UpdateQuestionUI()
    {
        var q = QuizGameManager.Instance.CurrentQuestion.Value;
        txtQuestionNumber.text = "Cau " + QuizGameManager.Instance.CurrentQuestionNumber.Value + "/10";
        txtQuestion.text = q.QuestionText.ToString();
        if (txtA != null) txtA.text = "A. " + q.AnswerA.ToString();
        if (txtB != null) txtB.text = "B. " + q.AnswerB.ToString();
        if (txtC != null) txtC.text = "C. " + q.AnswerC.ToString();
        if (txtD != null) txtD.text = "D. " + q.AnswerD.ToString();
    }
    private void UpdateButtonsState()
    {
        ulong answeringId = QuizGameManager.Instance.AnsweringClientId.Value;
        ulong myId = NetworkManager.Singleton.LocalClientId;
        if (answeringId == 9999)
        {
            if (iLostThisRound)
            {
                txtStatus.text = "Ban da mat luot cau nay!";
                btnHand.interactable = false;
            }
            else
            {
                txtStatus.text = "Hay nhanh tay bam HAND!";
                btnHand.interactable = true;
            }
            SetAnswersInteractable(false);
        }
        else if (answeringId == myId)
        {
            float t = QuizGameManager.Instance.AnswerTimeRemaining.Value;
            txtStatus.text = "Ban dang tra loi! Con " + Mathf.CeilToInt(Mathf.Max(0, t)) + "s";
            btnHand.interactable = false;
            SetAnswersInteractable(true);
        }
        else
        {
            txtStatus.text = "Doi thu dang tra loi...";
            btnHand.interactable = false;
            SetAnswersInteractable(false);
        }
    }
    private void SetAnswersInteractable(bool state)
    {
        btnAnswerA.interactable = state;
        btnAnswerB.interactable = state;
        btnAnswerC.interactable = state;
        btnAnswerD.interactable = state;
    }
    private void OnClickHand()
    {
        QuizGameManager.Instance.SubmitHandServerRpc();
    }
    private void OnClickAnswer(int index)
    {
        QuizGameManager.Instance.SubmitAnswerServerRpc(index);
        SetAnswersInteractable(false);
        iLostThisRound = true;
    }
    private void OnDestroy()
    {
        if (QuizGameManager.Instance != null && QuizGameManager.Instance.PlayerScores != null)
        {
            QuizGameManager.Instance.PlayerScores.OnListChanged -= OnScoresChanged;
        }
    }
}
