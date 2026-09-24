using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Linq;
public class EndSceneUIManager : MonoBehaviour
{
    public TMP_Text txtRankings;
    public Button btnReplay;
    public Button btnMainMenu;
    public string gameplaySceneName = "GamePlay";
    public string mainMenuSceneName = "MainMenu";
    private void Start()
    {
        btnReplay.onClick.AddListener(OnClickReplay);
        btnMainMenu.onClick.AddListener(OnClickMainMenu);
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            btnReplay.gameObject.SetActive(true);
        }
        else
        {
            btnReplay.gameObject.SetActive(false);
        }
        DisplayRankings();
    }
    private void DisplayRankings()
    {
        if (QuizGameManager.FinalScores == null || QuizGameManager.FinalScores.Count == 0)
        {
            txtRankings.text = "Không có dữ liệu điểm số.";
            return;
        }
        var sortedScores = QuizGameManager.FinalScores.OrderByDescending(x => x.Score).ToList();
        string rankText = "<b>BẢNG XẾP HẠNG CHUNG CUỘC</b>\n\n";
        for (int i = 0; i < sortedScores.Count; i++)
        {
            var s = sortedScores[i];
            string myMarker = "";
            if (NetworkManager.Singleton != null && s.ClientId == NetworkManager.Singleton.LocalClientId)
            {
                myMarker = " (Bạn)";
            }
            string displayName = s.PlayerIP.ToString();
            if (string.IsNullOrEmpty(displayName)) displayName = "Player " + s.ClientId;
            rankText += $"#{i + 1} - {displayName}{myMarker}: {s.Score} điểm\n";
        }
        txtRankings.text = rankText;
    }
    private void OnClickReplay()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
    private void OnClickMainMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
}
