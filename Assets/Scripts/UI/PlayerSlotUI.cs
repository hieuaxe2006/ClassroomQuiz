using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI hiển thị thông tin 1 player trong danh sách phòng chờ.
/// Gắn vào Prefab PlayerSlot (item trong ScrollView).
/// Hỗ trợ hiển thị character sprite hoặc fallback avatar letter.
/// </summary>
public class PlayerSlotUI : MonoBehaviour
{
    [Header("═══ UI REFERENCES ═══")]
    [Tooltip("Hình nền avatar (dùng Image.color để đổi màu)")]
    [SerializeField] private Image imgAvatar;

    [Tooltip("Text hiển thị chữ cái đầu tên (avatar letter) — fallback khi không có sprite")]
    [SerializeField] private TMP_Text txtAvatarLetter;

    [Tooltip("Text hiển thị tên người chơi")]
    [SerializeField] private TMP_Text txtPlayerName;

    [Tooltip("Text hiển thị vai trò (Host / Player 1, 2...)")]
    [SerializeField] private TMP_Text txtRole;

    [Tooltip("Text hoặc Image hiển thị trạng thái (Sẵn sàng, Đang chờ...)")]
    [SerializeField] private TMP_Text txtStatus;

    [Tooltip("Icon vương miện cho Host (ẩn nếu không phải host)")]
    [SerializeField] private GameObject iconCrown;

    [Tooltip("Nút Kick player (chỉ hiện cho Host, ẩn với chính mình)")]
    [SerializeField] private Button btnKick;

    [Tooltip("Viền nổi bật khi là chính mình (optional)")]
    [SerializeField] private Image imgHighlightBorder;

    [Header("═══ CHARACTER SPRITE ═══")]
    [Tooltip("Image hiển thị character sprite (ẩn nếu không có sprite)")]
    [SerializeField] private Image imgCharacter;

    [Tooltip("Danh sách sprite 3 nhân vật (kéo từ Assets/Art/Sprites/Characters)")]
    [SerializeField] private Sprite[] characterSprites;

    // Dữ liệu player được gán
    private PlayerData playerData;
    private bool isLocalPlayer;

    // Bảng màu avatar
    private static readonly Color[] AvatarColors = new Color[]
    {
        new Color(0f, 0.83f, 1f),       // #00D4FF - Cyan
        new Color(1f, 0.42f, 0.42f),    // #FF6B6B - Red
        new Color(1f, 0.84f, 0f),       // #FFD700 - Gold
        new Color(0f, 0.9f, 0.46f),     // #00E676 - Green
        new Color(0.88f, 0.25f, 0.98f), // #E040FB - Purple
    };

    // ═══════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════

    /// <summary>
    /// Thiết lập thông tin player lên UI slot.
    /// </summary>
    /// <param name="data">Dữ liệu người chơi</param>
    /// <param name="slotIndex">Thứ tự trong danh sách (0-based)</param>
    /// <param name="isLocal">True nếu đây là người chơi local</param>
    /// <param name="isHostViewing">True nếu người xem là Host (để hiện nút Kick)</param>
    public void Setup(PlayerData data, int slotIndex, bool isLocal, bool isHostViewing)
    {
        playerData = data;
        isLocalPlayer = isLocal;

        // === Tên người chơi ===
        if (txtPlayerName != null)
        {
            string displayName = data.playerName;
            if (isLocal) displayName += " <color=#00D4FF><size=80%>(Bạn)</size></color>";
            txtPlayerName.text = displayName;
        }

        // === Character Sprite hoặc Avatar Letter ===
        SetupCharacterDisplay(data, slotIndex);

        // === Vai trò ===
        if (txtRole != null)
        {
            txtRole.text = data.isHost ? "👑 Host" : $"🎮 Player {slotIndex + 1}";
        }

        // === Trạng thái ===
        if (txtStatus != null)
        {
            txtStatus.text = "● Sẵn sàng";
            txtStatus.color = new Color(0f, 0.9f, 0.46f); // Green
        }

        // === Icon vương miện ===
        if (iconCrown != null)
        {
            iconCrown.SetActive(data.isHost);
        }

        // === Nút Kick (chỉ Host thấy, không kick chính mình) ===
        if (btnKick != null)
        {
            bool showKick = isHostViewing && !isLocal && !data.isHost;
            btnKick.gameObject.SetActive(showKick);

            if (showKick)
            {
                btnKick.onClick.RemoveAllListeners();
                btnKick.onClick.AddListener(() => OnClickKick(data));
            }
        }

        // === Viền nổi bật cho chính mình ===
        if (imgHighlightBorder != null)
        {
            imgHighlightBorder.enabled = isLocal;
            if (isLocal)
            {
                imgHighlightBorder.color = new Color(0f, 0.83f, 1f, 0.5f); // Cyan 50%
            }
        }
    }

    /// <summary>
    /// Thiết lập slot rỗng (Đang chờ người chơi...).
    /// </summary>
    /// <param name="slotIndex">Thứ tự slot</param>
    public void SetupEmpty(int slotIndex)
    {
        playerData = null;
        isLocalPlayer = false;

        if (txtPlayerName != null)
        {
            txtPlayerName.text = $"Slot {slotIndex + 1} — Đang chờ...";
            txtPlayerName.color = new Color(1f, 1f, 1f, 0.3f);
        }

        if (txtAvatarLetter != null) txtAvatarLetter.text = "?";
        if (imgAvatar != null) imgAvatar.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        if (imgCharacter != null) imgCharacter.gameObject.SetActive(false);
        if (txtRole != null) txtRole.text = "";
        if (txtStatus != null)
        {
            txtStatus.text = "⏳ Đợi";
            txtStatus.color = new Color(1f, 1f, 1f, 0.3f);
        }
        if (iconCrown != null) iconCrown.SetActive(false);
        if (btnKick != null) btnKick.gameObject.SetActive(false);
        if (imgHighlightBorder != null) imgHighlightBorder.enabled = false;
    }

    // ═══════════════════════════════════════════
    //  PRIVATE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Hiển thị character sprite nếu có, fallback sang avatar letter + màu nếu không.
    /// </summary>
    private void SetupCharacterDisplay(PlayerData data, int slotIndex)
    {
        bool hasCharacterSprite = imgCharacter != null
            && characterSprites != null
            && characterSprites.Length > 0
            && data.characterId >= 0
            && data.characterId < characterSprites.Length
            && characterSprites[data.characterId] != null;

        if (hasCharacterSprite)
        {
            // === Hiển thị Character Sprite ===
            imgCharacter.sprite = characterSprites[data.characterId];
            imgCharacter.gameObject.SetActive(true);
            imgCharacter.preserveAspect = true;

            // Ẩn avatar letter (không cần nữa)
            if (txtAvatarLetter != null) txtAvatarLetter.gameObject.SetActive(false);

            // Đổi màu nền avatar theo slot
            if (imgAvatar != null)
            {
                imgAvatar.color = AvatarColors[slotIndex % AvatarColors.Length];
            }
        }
        else
        {
            // === Fallback: Avatar Letter + Màu ===
            if (imgCharacter != null) imgCharacter.gameObject.SetActive(false);

            if (txtAvatarLetter != null)
            {
                txtAvatarLetter.gameObject.SetActive(true);
                if (!string.IsNullOrEmpty(data.playerName))
                {
                    txtAvatarLetter.text = data.playerName[0].ToString().ToUpper();
                }
            }

            if (imgAvatar != null)
            {
                imgAvatar.color = AvatarColors[slotIndex % AvatarColors.Length];
            }
        }
    }

    /// <summary>
    /// Xử lý khi Host nhấn nút Kick player.
    /// </summary>
    private void OnClickKick(PlayerData target)
    {
        Debug.Log($"[PlayerSlotUI] Kick player: {target.playerName}");
        // TODO: Gửi packet kick player lên server
        // PacketSender.SendKickPlayer(target.playerId);
    }
}
