// PlayerData.cs — CẢ 2 NGƯỜI DÙNG CHUNG
using System;

[Serializable]
public class PlayerData
{
    public string playerId;
    public string playerName;
    public int characterId;     // 0, 1, 2
    public int currentScore;
    public float totalWins;     // 0.5 khi hòa
    public bool isHost;
}
