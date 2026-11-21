using System.IO;
using ClassroomQuiz.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ClassroomQuiz.Editor
{
    public static class ClassroomQuizSharedUiAssets
    {
        private const int TextureSize = 256;
        private const string SpriteDirectory = "Assets/Art/Sprites/ClassroomUI";
        private const string PrefabDirectory = "Assets/Prefabs/ClassroomQuizUI";

        [MenuItem("ClassroomQuiz/Create Shared Classroom UI Assets")]
        public static void CreateSharedAssets()
        {
            EnsureDirectory(SpriteDirectory);
            EnsureDirectory(PrefabDirectory);

            var board = CreateSprite("BoardFrame", BoardFrame, new Vector4(42, 42, 42, 42));
            var questionCard = CreateSprite("QuestionCard", QuestionCard, new Vector4(28, 28, 28, 28));
            var button = CreateSprite("ClassroomButton", ClassroomButton, new Vector4(28, 28, 28, 28));
            var student = CreateSprite("StudentAvatar", StudentAvatar, Vector4.zero);
            var teacher = CreateSprite("TeacherAvatar", TeacherAvatar, Vector4.zero);

            CreateImagePrefab("BoardFrame", board, null, Image.Type.Sliced);
            CreateImagePrefab("QuestionCard", questionCard, null, Image.Type.Sliced);
            CreateImagePrefab("ClassroomButton", button, true, Image.Type.Sliced);
            CreateImagePrefab("StudentAvatar", student, null, Image.Type.Simple);
            CreateImagePrefab("TeacherAvatar", teacher, null, Image.Type.Simple);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ClassroomQuiz shared classroom sprites and UI prefabs were created.");
        }

        private static Sprite CreateSprite(string name, System.Func<int, int, Color32> pixel,
            Vector4 border)
        {
            var path = SpriteDirectory + "/" + name + ".png";
            if (File.Exists(path))
            {
                var existingImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (existingImporter != null)
                {
                    existingImporter.textureType = TextureImporterType.Sprite;
                    existingImporter.spriteImportMode = SpriteImportMode.Single;
                    existingImporter.spritePixelsPerUnit = 100;
                    existingImporter.spriteBorder = border;
                    existingImporter.alphaIsTransparency = true;
                    existingImporter.mipmapEnabled = false;
                    existingImporter.SaveAndReimport();
                }
                var existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (existingSprite != null)
                {
                    return existingSprite;
                }
            }

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false, true);
            texture.name = name;
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[TextureSize * TextureSize];
            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    pixels[y * TextureSize + x] = pixel(x, y);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new IOException("Could not configure sprite importer for " + path);
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.spriteBorder = border;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new IOException("Unity did not import the generated sprite at " + path);
            }
            return sprite;
        }

        private static void CreateImagePrefab(string name, Sprite sprite, bool? addButton, Image.Type imageType)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            try
            {
                var image = root.GetComponent<Image>();
                image.sprite = sprite;
                image.type = imageType;
                image.color = Color.white;
                image.raycastTarget = addButton == true;

                if (addButton == true)
                {
                    var button = root.AddComponent<Button>();
                    button.targetGraphic = image;
                    button.transition = Selectable.Transition.ColorTint;
                    button.colors = new ColorBlock
                    {
                        normalColor = Color.white,
                        highlightedColor = Color.Lerp(Color.white, ClassroomQuizDesignSystem.BadgeGold, 0.12f),
                        pressedColor = Color.Lerp(Color.white, Color.black, 0.12f),
                        selectedColor = Color.white,
                        disabledColor = new Color(1f, 1f, 1f, 0.45f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.12f
                    };
                }

                var path = PrefabDirectory + "/" + name + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void EnsureDirectory(string assetPath)
        {
            var absolutePath = Path.GetFullPath(assetPath);
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }
        }

        private static Color32 BoardFrame(int x, int y)
        {
            if (!InsideRoundedRectangle(x, y, 8, 8, 248, 248, 30)) return Clear;
            if (!InsideRoundedRectangle(x, y, 28, 28, 228, 228, 20)) return Wood;
            if (!InsideRoundedRectangle(x, y, 42, 42, 214, 214, 12)) return Gold;
            return Board;
        }

        private static Color32 QuestionCard(int x, int y)
        {
            if (!InsideRoundedRectangle(x, y, 8, 8, 248, 248, 28)) return Clear;
            if (!InsideRoundedRectangle(x, y, 14, 14, 242, 242, 24)) return Wood;
            if (!InsideRoundedRectangle(x, y, 20, 20, 236, 236, 18)) return Chalk;
            if (x >= 20 && x < 34 && y >= 190 && y < 236) return Gold;
            return Chalk;
        }

        private static Color32 ClassroomButton(int x, int y)
        {
            if (!InsideRoundedRectangle(x, y, 8, 8, 248, 248, 34)) return Clear;
            if (!InsideRoundedRectangle(x, y, 14, 14, 242, 242, 30)) return Gold;
            return Board;
        }

        private static Color32 StudentAvatar(int x, int y)
        {
            var dx = x - 128;
            var dy = y - 128;
            var distance = dx * dx + dy * dy;
            if (distance > 124 * 124) return Clear;
            if (distance > 108 * 108) return Wood;
            if (distance > 96 * 96) return Gold;
            if (CircleContains(x, y, 114, 90, 5) || CircleContains(x, y, 142, 90, 5)) return Board;
            if (InRoundedBox(x, y, 113, 113, 143, 120, 4)) return Wood;
            if (CircleContains(x, y, 128, 88, 38) || InRoundedBox(x, y, 81, 147, 175, 220, 28)) return Board;
            return Chalk;
        }

        private static Color32 TeacherAvatar(int x, int y)
        {
            var dx = x - 128;
            var dy = y - 128;
            var distance = dx * dx + dy * dy;
            if (distance > 124 * 124) return Clear;
            if (distance > 108 * 108) return Board;
            if (distance > 96 * 96) return Gold;
            if (InRoundedBox(x, y, 86, 48, 170, 83, 16)) return Board;
            if (CircleContains(x, y, 114, 91, 5) || CircleContains(x, y, 142, 91, 5)) return Board;
            if (InRoundedBox(x, y, 113, 114, 143, 121, 4)) return Board;
            if (CircleContains(x, y, 128, 89, 37) || InRoundedBox(x, y, 78, 148, 178, 220, 28)) return Wood;
            return Chalk;
        }

        private static bool InsideRoundedRectangle(int x, int y, int left, int bottom,
            int right, int top, int radius)
        {
            if (x < left || x >= right || y < bottom || y >= top) return false;
            var cx = x < left + radius ? left + radius : x >= right - radius ? right - radius - 1 : x;
            var cy = y < bottom + radius ? bottom + radius : y >= top - radius ? top - radius - 1 : y;
            var dx = x - cx;
            var dy = y - cy;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static bool InRoundedBox(int x, int y, int left, int bottom, int right, int top, int radius)
        {
            return InsideRoundedRectangle(x, y, left, bottom, right, top, radius);
        }

        private static bool CircleContains(int x, int y, int centerX, int centerY, int radius)
        {
            var dx = x - centerX;
            var dy = y - centerY;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        private static readonly Color32 Board = ToColor32(ClassroomQuizDesignSystem.BoardGreen);
        private static readonly Color32 Wood = ToColor32(ClassroomQuizDesignSystem.DeskWood);
        private static readonly Color32 Gold = ToColor32(ClassroomQuizDesignSystem.BadgeGold);
        private static readonly Color32 Chalk = ToColor32(ClassroomQuizDesignSystem.ChalkWhite);

        private static Color32 ToColor32(Color value)
        {
            return value;
        }
    }
}
