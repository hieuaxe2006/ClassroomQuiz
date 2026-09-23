# ClassroomQuiz 🎓

Game quiz trong lớp học — multiplayer qua TCP. Giáo viên tạo phòng, học sinh tham gia trả lời câu hỏi theo kiểu "giơ tay nhanh".

## 📋 Yêu cầu

| Tool | Version |
|------|---------|
| **Unity** | `6000.3.5f1` (Unity 6) |
| **.NET SDK** | 8.0+ (để chạy Server) |
| **Git LFS** | Bắt buộc (repo dùng LFS cho file binary) |

## 🚀 Bắt đầu

### 1. Clone repo

```bash
git lfs install
git clone https://github.com/hieuaxe2006/ClassroomQuiz.git
```

### 2. Mở Unity project

- Mở **Unity Hub** → **Add** → chọn thư mục `ClassroomQuiz/`
- Đảm bảo dùng đúng phiên bản Unity `6000.3.5f1`
- Unity sẽ tự tạo `Library/`, `Temp/`, v.v.

### 3. Chạy Server

```bash
cd Server
dotnet run
```

Server sẽ lắng nghe kết nối TCP từ game client.

## 📁 Cấu trúc thư mục

```
ClassroomQuiz/
├── Assets/                     # Unity Assets chính
│   ├── Art/                    # Sprites, Animations, UI art
│   │   ├── Animations/
│   │   ├── Sprites/
│   │   └── UI/
│   ├── Audio/                  # Âm thanh
│   │   ├── BGM/                # Nhạc nền
│   │   └── SFX/                # Hiệu ứng âm thanh
│   ├── Prefabs/                # Prefabs
│   ├── Resources/              # Resources (load at runtime)
│   ├── Scenes/                 # Các scene
│   │   ├── MainMenu.unity      # Menu chính
│   │   ├── GamePlay.unity      # Gameplay
│   │   └── EndScene.unity      # Kết thúc
│   ├── Scripts/                # Code C#
│   │   ├── Data/               # Data models (PlayerData, QuestionData, RoomData)
│   │   ├── Gameplay/           # Logic gameplay (PlayerAvatar, TeacherNPC)
│   │   ├── Managers/           # Managers (GameManager)
│   │   ├── Network/            # Networking (Client, PacketHandler, Serializer)
│   │   └── UI/                 # UI scripts
│   └── Settings/               # URP render settings
├── Server/                     # TCP Server (.NET Console App)
│   └── TCPServer.cs
├── Packages/                   # Unity Package Manager
├── ProjectSettings/            # Cấu hình project Unity
├── .gitignore
├── .gitattributes
├── .editorconfig
└── README.md
```

## 🤝 Quy ước làm việc nhóm

### Branching

- `main` — branch ổn định, luôn chạy được
- `dev` — branch phát triển chính, merge feature vào đây
- `feature/<tên>` — branch cho từng tính năng (VD: `feature/lobby-ui`)
- `fix/<tên>` — branch sửa lỗi

### Quy trình

1. Tạo branch mới từ `dev`:
   ```bash
   git checkout dev
   git pull origin dev
   git checkout -b feature/ten-tinh-nang
   ```
2. Code & commit thường xuyên
3. Push branch lên remote:
   ```bash
   git push origin feature/ten-tinh-nang
   ```
4. Tạo **Pull Request** vào `dev` trên GitHub
5. Ít nhất 1 người review trước khi merge

### Commit message

Dùng format: `<loại>: <mô tả ngắn>`

```
feat: thêm màn hình lobby
fix: sửa lỗi mất kết nối khi đổi scene
refactor: tách PacketHandler thành partial class
docs: cập nhật README
```

### ⚠️ Lưu ý quan trọng

- **KHÔNG** commit thư mục `Library/`, `Temp/`, `Logs/`, `UserSettings/` (đã có trong `.gitignore`)
- **KHÔNG** chỉnh sửa trực tiếp trên branch `main`
- Khi xung đột file `.unity` (scene) hoặc `.prefab`, hãy **trao đổi với nhau** thay vì tự resolve
- Luôn chạy `git lfs install` trước khi clone lần đầu

## 📝 License

Private project — VKU classroom assignment.
