using System;
using System.IO;
using ClassroomQuiz.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ClassroomQuiz.Editor
{
    [InitializeOnLoad]
    public static class ClassroomQuizSpriteApplier
    {
        private const string ResDir = "Assets/Resources/ClassroomQuiz/Sprites";
        private const string UiDir = "Assets/Art/Sprites/ClassroomUI";
        private const string RanKey = "ClassroomQuiz_Sprites_Applied_V4";

        static ClassroomQuizSpriteApplier()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool(RanKey, false))
                {
                    SessionState.SetBool(RanKey, true);
                    ApplyClassroomSpritesToAllScenes();
                }
            };
        }

        [MenuItem("ClassroomQuiz/Apply Classroom Sprites to All Scenes")]
        public static void ApplyClassroomSpritesToAllScenes()
        {
            try
            {
                ConfigureAllTextureImporters();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                ApplyToLoginScene();
                ApplyToStartScene();
                ApplyToGameplayScene();
                ApplyToEndScene();

                AssetDatabase.SaveAssets();
                Debug.Log("<color=green><b>[ClassroomQuiz]</b> All classroom sprites successfully applied to all scenes!</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ClassroomQuiz] Error applying sprites: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void ConfigureAllTextureImporters()
        {
            var directories = new[] { ResDir, UiDir, "Assets/Art/Sprites/Backgrounds" };
            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir)) continue;
                var files = Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var assetPath = file.Replace('\\', '/');
                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer == null) continue;

                    var changed = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.alphaIsTransparency = true;
                        importer.mipmapEnabled = false;
                        changed = true;
                    }

                    var fileName = Path.GetFileNameWithoutExtension(assetPath);
                    var border = GetBorderForSprite(fileName);
                    if (importer.spriteBorder != border)
                    {
                        importer.spriteBorder = border;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        private static Vector4 GetBorderForSprite(string name)
        {
            switch (name)
            {
                case "BoardFrame":
                    return new Vector4(42, 42, 42, 42);
                case "QuestionCard":
                case "SchoolPaperCard":
                    return new Vector4(32, 32, 32, 32);
                case "ClassroomButton":
                case "ChalkButton":
                case "ClassroomButtonBlue":
                case "ClassroomButtonDanger":
                case "ClassroomButtonTeal":
                case "ClassroomButtonPurple":
                case "ClassroomButtonWood":
                    return new Vector4(28, 28, 28, 28);
                case "PodiumCardGold":
                case "PodiumCardSilver":
                case "PodiumCardBronze":
                    return new Vector4(26, 26, 26, 26);
                default:
                    return Vector4.zero;
            }
        }

        private static Sprite LoadSprite(string name)
        {
            // Try Resources first
            var sprite = Resources.Load<Sprite>($"ClassroomQuiz/Sprites/{name}");
            if (sprite != null) return sprite;

            // Try direct AssetDatabase paths
            var path = $"{ResDir}/{name}.png";
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            path = $"{UiDir}/{name}.png";
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            path = $"Assets/Art/Sprites/Backgrounds/{name}.png";
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        public static void ApplyToLoginScene()
        {
            var scenePath = "Assets/Scenes/LoginScene.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            var bgSprite = LoadSprite("BackgroundLogin");
            var boardSprite = LoadSprite("BoardFrame");
            var cardSprite = LoadSprite("SchoolPaperCard") ?? LoadSprite("QuestionCard");
            var btnGold = LoadSprite("ClassroomButton");
            var btnBlue = LoadSprite("ClassroomButtonBlue");
            var btnChalk = LoadSprite("ChalkButton");
            var buntingSprite = LoadSprite("BuntingBanner");

            // 1. Background
            var bgGo = GameObject.Find("Background");
            if (bgGo != null)
            {
                var img = bgGo.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = bgSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Simple;
                }
            }

            // 2. Login Card
            var cardGo = GameObject.Find("Login Card");
            if (cardGo != null)
            {
                var img = cardGo.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = boardSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Sliced;
                }

                // Add or update decorative Bunting Banner at top of card
                var buntingGo = cardGo.transform.Find("Bunting Decoration");
                if (buntingGo == null && buntingSprite != null)
                {
                    var buntingObj = new GameObject("Bunting Decoration", typeof(RectTransform), typeof(Image));
                    buntingObj.transform.SetParent(cardGo.transform, false);
                    var rect = buntingObj.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(0.5f, 1);
                    rect.sizeDelta = new Vector2(0, 60);
                    rect.anchoredPosition = new Vector2(0, 0);
                    var bImg = buntingObj.GetComponent<Image>();
                    bImg.sprite = buntingSprite;
                    bImg.color = Color.white;
                    bImg.raycastTarget = false;
                }
            }

            // 3. Status Plate
            var statusGo = GameObject.Find("Status Plate");
            if (statusGo != null)
            {
                var img = statusGo.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = cardSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Sliced;
                }
            }

            // 4. Buttons
            ApplyButtonSprite("Login Button", btnGold);
            ApplyButtonSprite("Register Button", btnBlue ?? btnGold);
            ApplyButtonSprite("Register Tab", btnGold);
            ApplyButtonSprite("Back To Login", btnChalk);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ClassroomQuiz] LoginScene updated with classroom sprites.");
        }

        public static void ApplyToStartScene()
        {
            var scenePath = "Assets/Scenes/StartScene.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            var bgSprite = LoadSprite("BackgroundLobby") ?? LoadSprite("BackgroundStartScene");
            var boardSprite = LoadSprite("BoardFrame");
            var cardSprite = LoadSprite("SchoolPaperCard") ?? LoadSprite("QuestionCard");
            var btnGold = LoadSprite("ClassroomButton");
            var btnChalk = LoadSprite("ChalkButton");
            var btnBlue = LoadSprite("ClassroomButtonBlue");
            var btnTeal = LoadSprite("ClassroomButtonTeal") ?? btnChalk;
            var btnPurple = LoadSprite("ClassroomButtonPurple") ?? btnBlue;
            var btnWood = LoadSprite("ClassroomButtonWood") ?? btnGold;
            var btnDanger = LoadSprite("ClassroomButtonDanger");
            var buntingSprite = LoadSprite("BuntingBanner");
            var trophySprite = LoadSprite("TrophyGold");
            var studentSprite = LoadSprite("StudentAvatar");

            // 1. Apply background to all root screens/panels
            var screenNames = new[]
            {
                "Main Menu", "Main Panel", "Character Screen", "Character Panel",
                "Played Rooms", "Leaderboard Panel", "History Detail Panel",
                "Study Library", "Study Panel", "Settings Screen", "Settings Panel",
                "Room Hub", "Room Hub Panel", "Create Room", "Create Room Panel",
                "Join Room", "Join Room Panel", "Waiting Room", "Waiting Room Panel",
                "Connecting Overlay", "Connecting Panel", "Question Editor", "Question Editor Panel",
                "Room History Detail"
            };

            foreach (var sName in screenNames)
            {
                var sGo = GameObject.Find(sName);
                if (sGo != null)
                {
                    var img = sGo.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = bgSprite;
                        img.color = Color.white;
                        img.type = Image.Type.Simple;
                    }
                }
            }

            // 2. Apply BoardFrame to all Cards
            var allImages = UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var img in allImages)
            {
                if (img.gameObject.name.IndexOf("Card", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    img.gameObject.name.IndexOf("Frame", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    img.sprite = boardSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Sliced;
                }
            }

            // 3. Specifically style "Main Card" and its header texts
            var mainCard = GameObject.Find("Main Card");
            if (mainCard != null)
            {
                var img = mainCard.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = boardSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Sliced;
                }

                // Add decorative Bunting Banner across the top of Main Card
                var buntingGo = mainCard.transform.Find("Bunting Decoration");
                if (buntingGo == null && buntingSprite != null)
                {
                    var buntingObj = new GameObject("Bunting Decoration", typeof(RectTransform), typeof(Image));
                    buntingObj.transform.SetParent(mainCard.transform, false);
                    var rect = buntingObj.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(0.5f, 1);
                    rect.sizeDelta = new Vector2(0, 52);
                    rect.anchoredPosition = new Vector2(0, 0);
                    var bImg = buntingObj.GetComponent<Image>();
                    bImg.sprite = buntingSprite;
                    bImg.color = Color.white;
                    bImg.raycastTarget = false;
                }
            }

            // Style Main Menu texts for high contrast on the chalkboard card
            var mainMenu = GameObject.Find("Main Menu");
            if (mainMenu != null)
            {
                SetTextColorByName(mainMenu.transform, "Brand", new Color(0.95f, 0.77f, 0.06f, 1f));       // Gold #F1C40F
                SetTextColorByName(mainMenu.transform, "Title", new Color(0.97f, 0.98f, 0.98f, 1f));       // Chalk White #F8F9FA
                SetTextColorByName(mainMenu.transform, "Subtitle", new Color(0.64f, 0.85f, 0.81f, 1f));    // Pastel Cyan #A2D9CE
                SetTextColorByName(mainMenu.transform, "Welcome", new Color(0.98f, 0.91f, 0.62f, 1f));     // Soft Gold #F9E79F
            }

            // 4. Main Menu Buttons (exact GameObject names!)
            ApplyButtonSprite("Play", btnGold);
            ApplyButtonSprite("Play Button", btnGold);
            ApplyButtonSprite("Study", btnTeal);
            ApplyButtonSprite("Study Button", btnTeal);
            ApplyButtonSprite("Character", btnBlue);
            ApplyButtonSprite("Character Button", btnBlue);
            ApplyButtonSprite("Played Rooms", btnPurple);
            ApplyButtonSprite("Leaderboard Button", btnPurple);
            ApplyButtonSprite("Settings", btnWood);
            ApplyButtonSprite("Settings Button", btnWood);
            ApplyButtonSprite("Exit", btnDanger);
            ApplyButtonSprite("Exit Button", btnDanger);

            // Sub-screen Buttons
            ApplyButtonSprite("Create", btnGold);
            ApplyButtonSprite("Create Room", btnGold);
            ApplyButtonSprite("Create Room Button", btnGold);
            ApplyButtonSprite("Submit Create Room Button", btnGold);
            ApplyButtonSprite("Create Room Hub Button", btnGold);
            ApplyButtonSprite("Find Room", btnGold);
            ApplyButtonSprite("Find Room Hub Button", btnGold);
            ApplyButtonSprite("Join", btnGold);
            ApplyButtonSprite("Join Room", btnGold);
            ApplyButtonSprite("Join Room Button", btnGold);
            ApplyButtonSprite("Submit Join Room Button", btnGold);
            ApplyButtonSprite("Start Game", btnGold);
            ApplyButtonSprite("Start Game Button", btnGold);
            ApplyButtonSprite("Retry", btnGold);
            ApplyButtonSprite("Connecting Retry Button", btnGold);
            ApplyButtonSprite("Save Settings", btnGold);
            ApplyButtonSprite("Save Settings Button", btnGold);
            ApplyButtonSprite("Save Questions", btnGold);
            ApplyButtonSprite("Question Editor Save Button", btnGold);
            ApplyButtonSprite("Confirm Avatar", btnGold);
            ApplyButtonSprite("Character Confirm Button", btnGold);
            ApplyButtonSprite("Add Question", btnGold);
            ApplyButtonSprite("Question Editor Add Button", btnGold);

            ApplyButtonSprite("Leave Room", btnDanger);
            ApplyButtonSprite("Waiting Leave Button", btnDanger);
            ApplyButtonSprite("Cancel Questions", btnDanger);
            ApplyButtonSprite("Question Editor Cancel Button", btnDanger);

            ApplyButtonSprite("Back", btnChalk);
            ApplyButtonSprite("Back To Menu", btnChalk);
            ApplyButtonSprite("Character Back Button", btnChalk);
            ApplyButtonSprite("Leaderboard Back Button", btnChalk);
            ApplyButtonSprite("Settings Back Button", btnChalk);
            ApplyButtonSprite("Room Hub Back Button", btnChalk);
            ApplyButtonSprite("Create Back Button", btnChalk);
            ApplyButtonSprite("Join Back Button", btnChalk);
            ApplyButtonSprite("Study Back Button", btnChalk);
            ApplyButtonSprite("Connecting Back Button", btnChalk);

            ApplyButtonSprite("Copy Code", btnBlue);
            ApplyButtonSprite("Copy Room Code Button", btnBlue);
            ApplyButtonSprite("Edit Questions", btnBlue);
            ApplyButtonSprite("Edit Questions Button", btnBlue);

            ApplyButtonSprite("Avatar A", btnPurple);
            ApplyButtonSprite("Avatar B", btnPurple);
            ApplyButtonSprite("Avatar C", btnPurple);
            ApplyButtonSprite("Avatar D", btnPurple);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("<color=green>[ClassroomQuiz] StartScene updated with colorful classroom buttons & chalkboard frame.</color>");
        }

        private static void SetTextColorByName(Transform parent, string name, Color color)
        {
            if (parent == null) return;
            var target = parent.Find(name);
            if (target != null)
            {
                var txt = target.GetComponent<TMPro.TMP_Text>();
                if (txt != null) txt.color = color;
            }
        }

        public static void ApplyToGameplayScene()
        {
            var scenePath = "Assets/Scenes/GamePlay.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            var bgLobby = LoadSprite("BackgroundLobby");
            var bgClassroom = LoadSprite("BackgroundGameplayClassroom");
            var bgQuiz = LoadSprite("BackgroundGameplayQuiz");
            var boardSprite = LoadSprite("BoardFrame");
            var cardSprite = LoadSprite("SchoolPaperCard") ?? LoadSprite("QuestionCard");
            var btnChalk = LoadSprite("ChalkButton");
            var btnGold = LoadSprite("ClassroomButton");
            var trophySprite = LoadSprite("TrophyGold");

            // 1. Gameplay Background
            var bgGo = GameObject.Find("Gameplay Background");
            if (bgGo != null)
            {
                var img = bgGo.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = bgLobby;
                    img.color = Color.white;
                    img.type = Image.Type.Simple;
                }
            }

            // 2. Classroom Half (Left side)
            var classroomGo = GameObject.Find("Classroom Half");
            if (classroomGo != null)
            {
                var img = classroomGo.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = bgClassroom;
                    img.color = Color.white;
                    img.type = Image.Type.Simple;
                }

                // Make placeholder wall/floor transparent so the full illustrated classroom art shows through!
                var rearWall = classroomGo.transform.Find("Rear Wall");
                if (rearWall != null)
                {
                    var rImg = rearWall.GetComponent<Image>();
                    if (rImg != null) rImg.color = new Color(1, 1, 1, 0);
                }
                var floor = classroomGo.transform.Find("Floor");
                if (floor != null)
                {
                    var fImg = floor.GetComponent<Image>();
                    if (fImg != null) fImg.color = new Color(1, 1, 1, 0);
                }
                var boardFrameGo = classroomGo.transform.Find("Front Board Frame");
                if (boardFrameGo != null)
                {
                    var bfImg = boardFrameGo.GetComponent<Image>();
                    if (bfImg != null) bfImg.color = new Color(1, 1, 1, 0);
                }
                var boardGo = classroomGo.transform.Find("Classroom Board");
                if (boardGo != null)
                {
                    var bImg = boardGo.GetComponent<Image>();
                    if (bImg != null) bImg.color = new Color(1, 1, 1, 0);
                }
                var boardTitle = classroomGo.transform.Find("Board Title");
                if (boardTitle != null)
                {
                    // Keep board title clean or hide text since it's already illustrated on the board
                    var txt = boardTitle.GetComponent<TMPro.TMP_Text>();
                    if (txt != null) txt.color = new Color(1, 1, 1, 0);
                }
                var leftWindow = classroomGo.transform.Find("Left Window");
                if (leftWindow != null)
                {
                    var lwImg = leftWindow.GetComponent<Image>();
                    if (lwImg != null) lwImg.color = new Color(1, 1, 1, 0);
                }
            }

            // 3. Question Half (Right side)
            var questionHalfGo = GameObject.Find("Question Half");
            if (questionHalfGo != null)
            {
                var img = questionHalfGo.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = bgQuiz;
                    img.color = Color.white;
                    img.type = Image.Type.Simple;
                }
            }

            // 4. Prompt Card (Question prompt)
            var promptGo = GameObject.Find("Prompt");
            if (promptGo != null && promptGo.transform.parent != null)
            {
                // The parent or prompt card itself
                var parentImg = promptGo.transform.parent.GetComponent<Image>();
                if (parentImg != null && parentImg.gameObject.name.IndexOf("Prompt", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    parentImg.sprite = cardSprite;
                    parentImg.color = Color.white;
                    parentImg.type = Image.Type.Sliced;
                }
            }

            // 5. Answer Buttons (Answer A, Answer B, Answer C, Answer D)
            for (var i = 1; i <= 4; i++)
            {
                ApplyButtonSprite($"Answer {i}", btnChalk);
                ApplyButtonSprite($"Answer_{i}", btnChalk);
                ApplyButtonSprite($"Answer{i}", btnChalk);
            }

            // 6. Overlays
            var feedbackGo = GameObject.Find("Feedback Panel");
            if (feedbackGo != null)
            {
                var img = feedbackGo.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = boardSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Sliced;
                }
            }

            var winnerGo = GameObject.Find("Winner Overlay");
            if (winnerGo != null)
            {
                var trophyImgGo = winnerGo.transform.Find("Trophy");
                if (trophyImgGo != null)
                {
                    var tImg = trophyImgGo.GetComponent<Image>();
                    if (tImg != null)
                    {
                        tImg.sprite = trophySprite;
                        tImg.color = Color.white;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ClassroomQuiz] GamePlay scene updated with classroom sprites.");
        }

        public static void ApplyToEndScene()
        {
            var scenePath = "Assets/Scenes/EndScene.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            var bgSprite = LoadSprite("BackgroundEndScene");
            var boardSprite = LoadSprite("BoardFrame");
            var cardGold = LoadSprite("PodiumCardGold");
            var cardSilver = LoadSprite("PodiumCardSilver");
            var cardBronze = LoadSprite("PodiumCardBronze");
            var btnGold = LoadSprite("ClassroomButton");
            var btnChalk = LoadSprite("ChalkButton");
            var btnDanger = LoadSprite("ClassroomButtonDanger");
            var medalGold = LoadSprite("MedalGold");
            var medalSilver = LoadSprite("MedalSilver");
            var medalBronze = LoadSprite("MedalBronze");

            // 1. Background
            var bgGo = GameObject.Find("Background");
            if (bgGo != null)
            {
                var img = bgGo.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = bgSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Simple;
                }
            }

            // 2. Card
            var cardGo = GameObject.Find("Card");
            if (cardGo != null)
            {
                var img = cardGo.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = boardSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Sliced;
                }
            }

            // 3. Podium Cards
            var p1 = GameObject.Find("Podium Card 1");
            if (p1 != null)
            {
                var img = p1.GetComponent<Image>();
                if (img != null) { img.sprite = cardGold; img.color = Color.white; img.type = Image.Type.Sliced; }
                SetMedalImage(p1.transform, medalGold);
            }
            var p2 = GameObject.Find("Podium Card 2");
            if (p2 != null)
            {
                var img = p2.GetComponent<Image>();
                if (img != null) { img.sprite = cardSilver; img.color = Color.white; img.type = Image.Type.Sliced; }
                SetMedalImage(p2.transform, medalSilver);
            }
            var p3 = GameObject.Find("Podium Card 3");
            if (p3 != null)
            {
                var img = p3.GetComponent<Image>();
                if (img != null) { img.sprite = cardBronze; img.color = Color.white; img.type = Image.Type.Sliced; }
                SetMedalImage(p3.transform, medalBronze);
            }

            // 4. Buttons
            ApplyButtonSprite("Restart Room", btnGold);
            ApplyButtonSprite("End Session", btnDanger ?? btnChalk);
            ApplyButtonSprite("Leave", btnChalk);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ClassroomQuiz] EndScene updated with classroom sprites.");
        }

        private static void SetMedalImage(Transform parent, Sprite medalSprite)
        {
            if (parent == null || medalSprite == null) return;
            var medalObj = parent.Find("Medal Sprite");
            if (medalObj != null)
            {
                var img = medalObj.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = medalSprite;
                    img.color = Color.white;
                    img.preserveAspect = true;
                }
            }
        }

        private static void ApplyButtonSprite(string buttonName, Sprite sprite)
        {
            if (sprite == null) return;
            var allButtons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name.Equals(buttonName, StringComparison.OrdinalIgnoreCase))
                {
                    var img = btn.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = sprite;
                        img.color = Color.white;
                        img.type = Image.Type.Sliced;
                    }
                    btn.transition = Selectable.Transition.ColorTint;
                    btn.colors = new ColorBlock
                    {
                        normalColor = Color.white,
                        highlightedColor = new Color(1f, 1f, 0.88f, 1f),
                        pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f),
                        selectedColor = Color.white,
                        disabledColor = new Color(1f, 1f, 1f, 0.45f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.1f
                    };

                    var label = btn.GetComponentInChildren<TMPro.TMP_Text>(true);
                    if (label != null)
                    {
                        label.color = Color.white;
                    }
                }
            }
        }
    }
}
