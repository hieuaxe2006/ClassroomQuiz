// RoomData.cs — CẢ 2 NGƯỜI DÙNG CHUNG
using System;
using System.Collections.Generic;

[Serializable]
public class RoomData
{
    public string roomId;
    public string roomName;
    public string hostId;
    public List<PlayerData> players;
    public int maxPlayers = 5;
    public string status;       // "WAITING", "PLAYING", "FINISHED"
}
