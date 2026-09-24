using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor tool chạy 1 lần để tự động tạo PlayerSlot Prefab và LobbyHost scene hierarchy.
/// Menu: ClassroomQuiz > Setup Lobby UI
/// Sau khi chạy xong có thể xóa file này.
/// </summary>
public class LobbyUISetupTool : EditorWindow
{
    [MenuItem("ClassroomQuiz/1. Tạo PlayerSlot Prefab")]
    public static void CreatePlayerSlotPrefab()
    {
        // ══════════════════════════════════════════
        //  TẠO PLAYERSLOT PREFAB
        // ══════════════════════════════════════════

        // Root
        GameObject root = new GameObject("PlayerSlot");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(500, 100);

        Image rootBg = root.AddComponent<Image>();
        rootBg.color = new Color(0.12f, 0.14f, 0.2f, 0.9f);

        HorizontalLayoutGroup hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12;
        hlg.padding = new RectOffset(10, 10, 8, 8);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // --- ImgAvatar (nền tròn) ---
        GameObject avatarObj = CreateChild(root, "ImgAvatar", new Vector2(80, 80));
        Image imgAvatar = avatarObj.AddComponent<Image>();
        imgAvatar.color = new Color(0f, 0.83f, 1f);
        // Làm tròn bằng cách dùng sprite mặc định (sẽ set trong Inspector)

        // --- TxtAvatarLetter (chữ cái đầu, con của ImgAvatar) ---
        GameObject letterObj = CreateChild(avatarObj, "TxtAvatarLetter", new Vector2(80, 80));
        TextMeshProUGUI txtLetter = letterObj.AddComponent<TextMeshProUGUI>();
        txtLetter.text = "?";
        txtLetter.fontSize = 36;
        txtLetter.fontStyle = FontStyles.Bold;
        txtLetter.alignment = TextAlignmentOptions.Center;
        txtLetter.color = Color.white;
        RectTransform letterRT = letterObj.GetComponent<RectTransform>();
        letterRT.anchorMin = Vector2.zero;
        letterRT.anchorMax = Vector2.one;
        letterRT.offsetMin = Vector2.zero;
        letterRT.offsetMax = Vector2.zero;

        // --- ImgCharacter (character sprite, con của ImgAvatar, ẩn mặc định) ---
        GameObject charObj = CreateChild(avatarObj, "ImgCharacter", new Vector2(70, 70));
        Image imgChar = charObj.AddComponent<Image>();
        imgChar.preserveAspect = true;
        imgChar.color = Color.white;
        charObj.SetActive(false);
        RectTransform charRT = charObj.GetComponent<RectTransform>();
        charRT.anchorMin = new Vector2(0.5f, 0.5f);
        charRT.anchorMax = new Vector2(0.5f, 0.5f);
        charRT.anchoredPosition = Vector2.zero;

        // --- Info Panel (tên + role + status, bên phải avatar) ---
        GameObject infoPanel = CreateChild(root, "InfoPanel", new Vector2(280, 80));
        VerticalLayoutGroup vlg = infoPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.MiddleLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // TxtPlayerName
        GameObject nameObj = CreateChild(infoPanel, "TxtPlayerName", new Vector2(280, 30));
        TextMeshProUGUI txtName = nameObj.AddComponent<TextMeshProUGUI>();
        txtName.text = "Player Name";
        txtName.fontSize = 22;
        txtName.fontStyle = FontStyles.Bold;
        txtName.color = Color.white;
        LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
        nameLE.preferredHeight = 30;

        // TxtRole
        GameObject roleObj = CreateChild(infoPanel, "TxtRole", new Vector2(280, 22));
        TextMeshProUGUI txtRole = roleObj.AddComponent<TextMeshProUGUI>();
        txtRole.text = "🎮 Player 1";
        txtRole.fontSize = 16;
        txtRole.color = new Color(0.7f, 0.7f, 0.8f);
        LayoutElement roleLE = roleObj.AddComponent<LayoutElement>();
        roleLE.preferredHeight = 22;

        // TxtStatus
        GameObject statusObj = CreateChild(infoPanel, "TxtStatus", new Vector2(280, 20));
        TextMeshProUGUI txtStatus = statusObj.AddComponent<TextMeshProUGUI>();
        txtStatus.text = "● Sẵn sàng";
        txtStatus.fontSize = 14;
        txtStatus.color = new Color(0f, 0.9f, 0.46f);
        LayoutElement statusLE = statusObj.AddComponent<LayoutElement>();
        statusLE.preferredHeight = 20;

        // --- IconCrown (góc trên phải avatar, ẩn mặc định) ---
        GameObject crownObj = CreateChild(avatarObj, "IconCrown", new Vector2(28, 28));
        Image imgCrown = crownObj.AddComponent<Image>();
        imgCrown.color = new Color(1f, 0.84f, 0f); // Gold
        RectTransform crownRT = crownObj.GetComponent<RectTransform>();
        crownRT.anchorMin = new Vector2(1f, 1f);
        crownRT.anchorMax = new Vector2(1f, 1f);
        crownRT.anchoredPosition = new Vector2(5, 5);
        crownObj.SetActive(false);

        // --- BtnKick (bên phải, ẩn mặc định) ---
        GameObject kickObj = CreateChild(root, "BtnKick", new Vector2(70, 36));
        Image kickBg = kickObj.AddComponent<Image>();
        kickBg.color = new Color(1f, 0.3f, 0.3f);
        Button btnKick = kickObj.AddComponent<Button>();
        ColorBlock cb = btnKick.colors;
        cb.normalColor = new Color(1f, 0.3f, 0.3f);
        cb.highlightedColor = new Color(1f, 0.5f, 0.5f);
        btnKick.colors = cb;

        GameObject kickText = CreateChild(kickObj, "Text", new Vector2(70, 36));
        TextMeshProUGUI txtKick = kickText.AddComponent<TextMeshProUGUI>();
        txtKick.text = "Kick";
        txtKick.fontSize = 14;
        txtKick.alignment = TextAlignmentOptions.Center;
        txtKick.color = Color.white;
        RectTransform kickTRT = kickText.GetComponent<RectTransform>();
        kickTRT.anchorMin = Vector2.zero;
        kickTRT.anchorMax = Vector2.one;
        kickTRT.offsetMin = Vector2.zero;
        kickTRT.offsetMax = Vector2.zero;
        kickObj.SetActive(false);

        // --- ImgHighlightBorder (viền nổi bật, toàn slot) ---
        GameObject borderObj = CreateChild(root, "ImgHighlightBorder", Vector2.zero);
        Image imgBorder = borderObj.AddComponent<Image>();
        imgBorder.color = new Color(0f, 0.83f, 1f, 0.5f);
        imgBorder.raycastTarget = false;
        imgBorder.enabled = false;
        RectTransform borderRT = borderObj.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-3, -3);
        borderRT.offsetMax = new Vector2(3, 3);

        // ══════ GẮN COMPONENT PlayerSlotUI ══════
        PlayerSlotUI slotUI = root.AddComponent<PlayerSlotUI>();

        // Gán references qua SerializedObject
        SerializedObject so = new SerializedObject(slotUI);
        so.FindProperty("imgAvatar").objectReferenceValue = imgAvatar;
        so.FindProperty("txtAvatarLetter").objectReferenceValue = txtLetter;
        so.FindProperty("txtPlayerName").objectReferenceValue = txtName;
        so.FindProperty("txtRole").objectReferenceValue = txtRole;
        so.FindProperty("txtStatus").objectReferenceValue = txtStatus;
        so.FindProperty("iconCrown").objectReferenceValue = crownObj;
        so.FindProperty("btnKick").objectReferenceValue = btnKick;
        so.FindProperty("imgHighlightBorder").objectReferenceValue = imgBorder;
        so.FindProperty("imgCharacter").objectReferenceValue = imgChar;

        // Load và gán character sprites
        Sprite[] charSprites = LoadCharacterSprites();
        if (charSprites != null && charSprites.Length > 0)
        {
            SerializedProperty spriteProp = so.FindProperty("characterSprites");
            spriteProp.arraySize = charSprites.Length;
            for (int i = 0; i < charSprites.Length; i++)
            {
                spriteProp.GetArrayElementAtIndex(i).objectReferenceValue = charSprites[i];
            }
        }

        // Load crown sprite
        Sprite crownSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/IconCrown.jpg");
        if (crownSprite != null)
        {
            imgCrown.sprite = crownSprite;
        }

        so.ApplyModifiedProperties();

        // ══════ LƯU PREFAB ══════
        string prefabPath = "Assets/Prefabs/PlayerSlot.prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            prefabPath = "Assets/Prefabs/PlayerSlotNew.prefab";
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        DestroyImmediate(root);

        Debug.Log($"✅ PlayerSlot Prefab đã tạo tại: {prefabPath}");
        EditorUtility.DisplayDialog("Thành công!", $"PlayerSlot Prefab đã tạo tại:\n{prefabPath}\n\nHãy kéo prefab này vào ô 'Player Slot Prefab' của LobbyHostUI.", "OK");
    }

    [MenuItem("ClassroomQuiz/2. Tạo LobbyHost UI trong Scene hiện tại")]
    public static void CreateLobbyHostUI()
    {
        // ══════════════════════════════════════════
        //  TẠO CANVAS + LOBBYHOST UI
        // ══════════════════════════════════════════

        // Tìm hoặc tạo Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject canvasObj;

        if (canvas != null)
        {
            canvasObj = canvas.gameObject;
        }
        else
        {
            canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // ── Panel chính (LobbyHostPanel) ──
        GameObject panel = CreateUIChild(canvasObj, "LobbyHostPanel");
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        StretchFull(panelRT);
        Image panelBg = panel.AddComponent<Image>();

        // Load background
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/LobbyPanelBG.jpg");
        if (bgSprite != null)
        {
            panelBg.sprite = bgSprite;
            panelBg.type = Image.Type.Simple;
            panelBg.color = Color.white;
        }
        else
        {
            panelBg.color = new Color(0.08f, 0.1f, 0.18f, 0.95f);
        }

        VerticalLayoutGroup panelVLG = panel.AddComponent<VerticalLayoutGroup>();
        panelVLG.spacing = 15;
        panelVLG.padding = new RectOffset(40, 40, 30, 30);
        panelVLG.childAlignment = TextAnchor.UpperCenter;
        panelVLG.childControlWidth = true;
        panelVLG.childControlHeight = false;
        panelVLG.childForceExpandWidth = true;
        panelVLG.childForceExpandHeight = false;

        // ── Header: Room Code ──
        GameObject headerObj = CreateUIChild(panel, "TxtRoomCode");
        TextMeshProUGUI txtRoomCode = headerObj.AddComponent<TextMeshProUGUI>();
        txtRoomCode.text = "Mã phòng: ------";
        txtRoomCode.fontSize = 32;
        txtRoomCode.fontStyle = FontStyles.Bold;
        txtRoomCode.alignment = TextAlignmentOptions.Center;
        txtRoomCode.color = new Color(0f, 0.83f, 1f);
        LayoutElement headerLE = headerObj.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 50;

        // ── Sub header: Player Count + Status ──
        GameObject subHeader = CreateUIChild(panel, "SubHeader");
        HorizontalLayoutGroup subHLG = subHeader.AddComponent<HorizontalLayoutGroup>();
        subHLG.spacing = 20;
        subHLG.childAlignment = TextAnchor.MiddleCenter;
        subHLG.childControlWidth = true;
        subHLG.childControlHeight = true;
        subHLG.childForceExpandWidth = true;
        LayoutElement subLE = subHeader.AddComponent<LayoutElement>();
        subLE.preferredHeight = 35;

        GameObject countObj = CreateUIChild(subHeader, "TxtPlayerCount");
        TextMeshProUGUI txtCount = countObj.AddComponent<TextMeshProUGUI>();
        txtCount.text = "0 / 5";
        txtCount.fontSize = 22;
        txtCount.alignment = TextAlignmentOptions.Center;
        txtCount.color = Color.white;

        GameObject statusObj = CreateUIChild(subHeader, "TxtRoomStatus");
        TextMeshProUGUI txtStatus = statusObj.AddComponent<TextMeshProUGUI>();
        txtStatus.text = "WAITING";
        txtStatus.fontSize = 22;
        txtStatus.alignment = TextAlignmentOptions.Center;
        txtStatus.color = new Color(1f, 0.84f, 0f);

        // ── Server IP ──
        GameObject ipObj = CreateUIChild(panel, "TxtServerIP");
        TextMeshProUGUI txtIP = ipObj.AddComponent<TextMeshProUGUI>();
        txtIP.text = "Server: ---";
        txtIP.fontSize = 16;
        txtIP.alignment = TextAlignmentOptions.Center;
        txtIP.color = new Color(0.6f, 0.6f, 0.7f);
        LayoutElement ipLE = ipObj.AddComponent<LayoutElement>();
        ipLE.preferredHeight = 25;

        // ── ScrollView (danh sách player) ──
        GameObject scrollView = CreateScrollView(panel, "PlayerListScrollView");
        LayoutElement scrollLE = scrollView.AddComponent<LayoutElement>();
        scrollLE.preferredHeight = 400;
        scrollLE.flexibleHeight = 1;

        Transform contentTransform = scrollView.transform
            .Find("Viewport/Content");

        // ── Buttons Row ──
        GameObject btnRow = CreateUIChild(panel, "ButtonRow");
        HorizontalLayoutGroup btnHLG = btnRow.AddComponent<HorizontalLayoutGroup>();
        btnHLG.spacing = 20;
        btnHLG.childAlignment = TextAnchor.MiddleCenter;
        btnHLG.childControlWidth = false;
        btnHLG.childControlHeight = false;
        btnHLG.childForceExpandWidth = false;
        LayoutElement btnRowLE = btnRow.AddComponent<LayoutElement>();
        btnRowLE.preferredHeight = 55;

        // BtnStart
        GameObject startObj = CreateButton(btnRow, "BtnStartGame", "🚀 Bắt Đầu",
            new Vector2(200, 50), new Color(0f, 0.7f, 0.3f));
        Button btnStart = startObj.GetComponent<Button>();
        TextMeshProUGUI txtStartBtn = startObj.GetComponentInChildren<TextMeshProUGUI>();

        // BtnCopyCode
        GameObject copyObj = CreateButton(btnRow, "BtnCopyCode", "📋 Copy Mã",
            new Vector2(160, 50), new Color(0.3f, 0.4f, 0.7f));
        Button btnCopy = copyObj.GetComponent<Button>();

        // BtnLeave
        GameObject leaveObj = CreateButton(btnRow, "BtnLeaveRoom", "🚪 Rời Phòng",
            new Vector2(180, 50), new Color(0.8f, 0.2f, 0.2f));
        Button btnLeave = leaveObj.GetComponent<Button>();

        // ── Copy Status Text ──
        GameObject copyStatusObj = CreateUIChild(panel, "TxtCopyStatus");
        TextMeshProUGUI txtCopyStatus = copyStatusObj.AddComponent<TextMeshProUGUI>();
        txtCopyStatus.text = "";
        txtCopyStatus.fontSize = 14;
        txtCopyStatus.alignment = TextAlignmentOptions.Center;
        txtCopyStatus.color = Color.green;
        LayoutElement csLE = copyStatusObj.AddComponent<LayoutElement>();
        csLE.preferredHeight = 20;

        // ── Notification Panel ──
        GameObject notifPanel = CreateUIChild(panel, "PanelNotification");
        Image notifBg = notifPanel.AddComponent<Image>();
        notifBg.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);
        LayoutElement notifLE = notifPanel.AddComponent<LayoutElement>();
        notifLE.preferredHeight = 40;
        notifPanel.SetActive(false);

        GameObject notifText = CreateUIChild(notifPanel, "TxtNotification");
        TextMeshProUGUI txtNotif = notifText.AddComponent<TextMeshProUGUI>();
        txtNotif.text = "";
        txtNotif.fontSize = 16;
        txtNotif.alignment = TextAlignmentOptions.Center;
        txtNotif.color = Color.white;
        RectTransform notifTRT = notifText.GetComponent<RectTransform>();
        StretchFull(notifTRT);

        // ══════ GẮN COMPONENT LobbyHostUI ══════
        LobbyHostUI lobbyUI = panel.AddComponent<LobbyHostUI>();

        SerializedObject so = new SerializedObject(lobbyUI);
        so.FindProperty("txtRoomCode").objectReferenceValue = txtRoomCode;
        so.FindProperty("btnCopyCode").objectReferenceValue = btnCopy;
        so.FindProperty("txtCopyStatus").objectReferenceValue = txtCopyStatus;
        so.FindProperty("txtPlayerCount").objectReferenceValue = txtCount;
        so.FindProperty("txtRoomStatus").objectReferenceValue = txtStatus;
        so.FindProperty("playerListParent").objectReferenceValue = contentTransform;
        so.FindProperty("btnStartGame").objectReferenceValue = btnStart;
        so.FindProperty("txtStartButton").objectReferenceValue = txtStartBtn;
        so.FindProperty("btnLeaveRoom").objectReferenceValue = btnLeave;
        so.FindProperty("panelNotification").objectReferenceValue = notifPanel;
        so.FindProperty("txtNotification").objectReferenceValue = txtNotif;

        // Load PlayerSlot prefab
        GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerSlotNew.prefab");
        if (slotPrefab == null)
            slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerSlot.prefab");

        if (slotPrefab != null)
        {
            so.FindProperty("playerSlotPrefab").objectReferenceValue = slotPrefab;
        }

        so.ApplyModifiedProperties();

        // Tạo EventSystem nếu chưa có
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Selection.activeGameObject = panel;
        Debug.Log("✅ LobbyHost UI đã tạo xong trong scene hiện tại!");
        EditorUtility.DisplayDialog("Thành công!", "LobbyHost UI đã tạo xong!\n\nCác reference đã được gán tự động.\nHãy lưu scene thành 'LobbyHost'.", "OK");
    }

    // ══════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════

    private static Sprite[] LoadCharacterSprites()
    {
        string[] paths = new string[]
        {
            "Assets/Art/Sprites/Characters/Character_0_Blue.jpg",
            "Assets/Art/Sprites/Characters/Character_1_Red.jpg",
            "Assets/Art/Sprites/Characters/Character_2_Green.jpg"
        };

        Sprite[] sprites = new Sprite[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]);
        }
        return sprites;
    }

    private static GameObject CreateChild(GameObject parent, string name, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        return obj;
    }

    private static GameObject CreateUIChild(GameObject parent, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static GameObject CreateScrollView(GameObject parent, string name)
    {
        GameObject scrollView = new GameObject(name);
        scrollView.transform.SetParent(parent.transform, false);
        RectTransform svRT = scrollView.AddComponent<RectTransform>();
        svRT.sizeDelta = new Vector2(0, 400);

        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        Image svBg = scrollView.AddComponent<Image>();
        svBg.color = new Color(0.05f, 0.07f, 0.12f, 0.6f);

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform vpRT = viewport.AddComponent<RectTransform>();
        StretchFull(vpRT);
        viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Link
        scrollRect.viewport = vpRT;
        scrollRect.content = contentRT;

        return scrollView;
    }

    private static GameObject CreateButton(GameObject parent, string name, string text,
        Vector2 size, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);
        RectTransform btnRT = btnObj.AddComponent<RectTransform>();
        btnRT.sizeDelta = size;

        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = bgColor * 1.2f;
        colors.pressedColor = bgColor * 0.8f;
        btn.colors = colors;

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y;

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        StretchFull(textRT);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btnObj;
    }
}
