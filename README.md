# ClassroomQuiz 🎓

Game quiz giáo dục trong lớp học — multiplayer qua TCP. Giáo viên theo dõi bảng xếp hạng trực tiếp; học sinh trả lời câu hỏi theo chuỗi học tập của từng bài.

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

Sau khi Unity biên dịch xong, chọn **ClassroomQuiz → Generate Editable Canvas UI**. Lệnh này tạo và lưu Canvas uGUI vào các scene Login, Start, GamePlay và EndScene. Bạn có thể chọn các đối tượng trong Hierarchy để sửa TextMeshPro, nút, màu, kích thước và vị trí ngay trong Editor; các scene cũ được sao lưu trước khi tạo.

### 3. Thiết lập tài khoản và database XAMPP

- Bật **Apache** và **MySQL** trong XAMPP. XAMPP2 trên máy này dùng `D:\webserver` làm `DocumentRoot`.
- Chạy `install_xampp_api.bat` từ thư mục project để cài API vào `D:\webserver\ClassroomQuizApi`.
- Mở `http://127.0.0.1/ClassroomQuizApi/api.php`. API tự tạo database `classromquiz` và các bảng khi được gọi lần đầu.
- API tạo bảng `lesson_questions` và nạp 30 câu hỏi theo 6 bài học mẫu ở lần chạy đầu. Người dùng đã đăng nhập có thể sửa 2–5 câu trong phần chỉnh sửa bài; nội dung được lưu vào bảng này.
- Tài liệu cấu hình server, API và kết nối LAN nằm trong [Server/README.md](Server/README.md).

### 4. Chạy Server

```bash
build_server.bat
run_server.bat
```

Server sẽ lắng nghe kết nối TCP từ game client.

Hướng dẫn chi tiết clone project, trỏ API theo Apache DocumentRoot, build server và chạy thử 3 client nằm trong [HUONG_DAN_CLONE_VA_TEST_3_CLIENT.txt](HUONG_DAN_CLONE_VA_TEST_3_CLIENT.txt).

Sau khi build Windows vào thư mục `Build`, có thể mở ba cửa sổ với profile đăng nhập độc lập bằng `run_3_clients.bat`.

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
