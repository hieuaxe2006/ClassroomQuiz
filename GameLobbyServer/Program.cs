// Program.cs
using Fleck;
using System.Text.Json;

var rooms = new Dictionary<string, Room>();
var connectionToPlayer = new Dictionary<IWebSocketConnection, PlayerInfo>();

var server = new WebSocketServer("ws://0.0.0.0:8181");

server.Start(socket =>
{
    socket.OnOpen = () => Console.WriteLine("Client connected");

    socket.OnMessage = message =>
    {
        var msg = JsonSerializer.Deserialize<ClientMessage>(message);
        switch (msg?.Type)
        {
            case "CREATE_ROOM":
                HandleCreateRoom(socket, msg.PlayerName);
                break;
            case "JOIN_ROOM":
                HandleJoinRoom(socket, msg.RoomId, msg.PlayerName);
                break;
            case "LEAVE_ROOM":
                HandleLeaveRoom(socket);
                break;
        }
    };

    socket.OnClose = () => HandleLeaveRoom(socket);
});

Console.WriteLine("Server chạy tại ws://localhost:8181");
Console.ReadLine();

// ─── Handlers ───

void HandleCreateRoom(IWebSocketConnection socket, string playerName)
{
    var roomId = Guid.NewGuid().ToString()[..6].ToUpper();
    var player = new PlayerInfo { Name = playerName, IsHost = true };
    var room = new Room
    {
        RoomId = roomId,
        Host = socket,
        Players = new() { { socket, player } }
    };
    rooms[roomId] = room;
    connectionToPlayer[socket] = player;

    BroadcastRoomState(room);
}

void HandleJoinRoom(IWebSocketConnection socket, string roomId, string playerName)
{
    if (!rooms.TryGetValue(roomId, out var room))
    {
        socket.Send(JsonSerializer.Serialize(new
        {
            Type = "ERROR",
            Message = "Phòng không tồn tại!"
        }));
        return;
    }

    if (room.Players.Count >= 4)
    {
        socket.Send(JsonSerializer.Serialize(new
        {
            Type = "ERROR",
            Message = "Phòng đã đầy!"
        }));
        return;
    }

    var player = new PlayerInfo { Name = playerName, IsHost = false };
    room.Players[socket] = player;
    connectionToPlayer[socket] = player;

    BroadcastRoomState(room);
}

void HandleLeaveRoom(IWebSocketConnection socket)
{
    foreach (var (roomId, room) in rooms)
    {
        if (room.Players.Remove(socket))
        {
            connectionToPlayer.Remove(socket);
            if (room.Players.Count == 0)
            {
                rooms.Remove(roomId);
            }
            else
            {
                // Chuyển host nếu host rời
                if (room.Host == socket)
                {
                    var newHost = room.Players.First();
                    room.Host = newHost.Key;
                    newHost.Value.IsHost = true;
                }
                BroadcastRoomState(room);
            }
            break;
        }
    }
}

void BroadcastRoomState(Room room)
{
    var state = new
    {
        Type = "ROOM_STATE",
        RoomId = room.RoomId,
        Players = room.Players.Values.Select(p => new
        {
            p.Name,
            p.IsHost,
            Status = "Ready"
        }).ToArray(),
        MaxPlayers = 4
    };

    var json = JsonSerializer.Serialize(state);
    foreach (var conn in room.Players.Keys)
    {
        conn.Send(json);
    }
}

// ─── Models ───

class Room
{
    public string RoomId { get; set; } = "";
    public IWebSocketConnection Host { get; set; } = null!;
    public Dictionary<IWebSocketConnection, PlayerInfo> Players { get; set; } = new();
}

class PlayerInfo
{
    public string Name { get; set; } = "";
    public bool IsHost { get; set; }
}

class ClientMessage
{
    public string Type { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public string RoomId { get; set; } = "";
}