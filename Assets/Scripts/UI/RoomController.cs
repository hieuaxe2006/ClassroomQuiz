using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomController : NetworkBehaviour
{
    [Header("UI Elements")]
    public TMP_Text txtNotification;
    public Button btnPlay;
    public string gameplaySceneName = "GamePlay";

    private async void Start()
    {
        // Tắt nút Play ban đầu
        if (btnPlay != null)
        {
            btnPlay.gameObject.SetActive(false);
            btnPlay.onClick.AddListener(StartGame);
        }

        // Đợi Unity Services
        while (UnityServices.State != ServicesInitializationState.Initialized || !AuthenticationService.Instance.IsSignedIn)
        {
            await System.Threading.Tasks.Task.Delay(100);
            if (this == null) return;
        }

        // Đổi tên thành IP
        string myIp = GetLocalIPAddress();
        try { await AuthenticationService.Instance.UpdatePlayerNameAsync(myIp); } catch { }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UpdateNotificationClientRpc($"Host (IP: {GetLocalIPAddress()}) đã tạo phòng!");
            InvokeRepeating(nameof(CheckPlayersAndEnablePlayButton), 0.5f, 0.2f);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer && clientId != NetworkManager.Singleton.LocalClientId)
        {
            UpdateNotificationClientRpc($"Một Client vừa tham gia phòng!");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            UpdateNotificationClientRpc($"Một Client đã rời phòng!");
        }
    }

    // Hàm này sẽ tự động chạy liên tục trên máy Host để đếm người
    private void CheckPlayersAndEnablePlayButton()
    {
        if (!IsServer || btnPlay == null) return;

        int players = NetworkManager.Singleton.ConnectedClientsIds.Count;
        btnPlay.gameObject.SetActive(players >= 2);
    }

    [ClientRpc]
    private void UpdateNotificationClientRpc(string message)
    {
        if (txtNotification != null)
            txtNotification.text = message;
    }

    private void StartGame()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();
        }
        return "Unknown IP";
    }
}
