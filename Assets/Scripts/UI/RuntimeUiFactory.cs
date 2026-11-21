using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClassroomQuiz.UI
{
    internal static class RuntimeUiFactory
    {
        public static readonly Color Background = ClassroomQuizDesignSystem.Background;
        public static readonly Color Surface = ClassroomQuizDesignSystem.Surface;
        public static readonly Color Navy = ClassroomQuizDesignSystem.BoardGreen;
        public static readonly Color Muted = ClassroomQuizDesignSystem.Muted;
        public static readonly Color Blue = ClassroomQuizDesignSystem.Blue;
        public static readonly Color Cyan = ClassroomQuizDesignSystem.Cyan;
        public static readonly Color Purple = ClassroomQuizDesignSystem.Purple;
        public static readonly Color Green = ClassroomQuizDesignSystem.Green;
        public static readonly Color Orange = ClassroomQuizDesignSystem.Orange;
        public static readonly Color Red = ClassroomQuizDesignSystem.Red;
        public static readonly Color Border = ClassroomQuizDesignSystem.Border;
        public static readonly Color Gold = ClassroomQuizDesignSystem.BadgeGold;

        private static TMP_FontAsset _font;

        public static Canvas CreateCanvas(string name)
        {
            EnsureEventSystem();
            var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(ClassroomQuizAuthoredCanvas));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.sortingOrder = 20;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 position, Vector2? anchor = null)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            var rect = item.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var effectiveAnchor = anchor ?? new Vector2(0.5f, 0.5f);
            rect.anchorMin = effectiveAnchor;
            rect.anchorMax = effectiveAnchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        public static RectTransform Stretch(Transform parent, string name, Vector2 insetMin = default, Vector2 insetMax = default)
        {
            var rect = Rect(parent, name, Vector2.zero, Vector2.zero);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = insetMin;
            rect.offsetMax = -insetMax;
            return rect;
        }

        public static Image Panel(Transform parent, string name, Vector2 size, Vector2 position, Color color, Vector2? anchor = null)
        {
            var rect = Rect(parent, name, size, position, anchor ?? new Vector2(0.5f, 0.5f));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        public static TMP_Text Text(Transform parent, string name, string value, float size, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, bool bold = false)
        {
            var rect = Rect(parent, name, Vector2.zero, Vector2.zero);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8, 4);
            rect.offsetMax = new Vector2(-8, -4);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (_font == null)
            {
                _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (_font == null && TMP_Settings.defaultFontAsset != null)
                {
                    _font = TMP_Settings.defaultFontAsset;
                }
            }
            if (_font != null)
            {
                text.font = _font;
            }
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
        }

        public static TMP_Text Label(Transform parent, string name, string value, Vector2 size, Vector2 position,
            float fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, bool bold = false)
        {
            var text = Text(parent, name, value, fontSize, color, alignment, bold);
            text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.sizeDelta = size;
            text.rectTransform.anchoredPosition = position;
            return text;
        }

        public static Button Button(Transform parent, string name, string label, Vector2 size, Vector2 position,
            Color color, UnityEngine.Events.UnityAction onClick, Vector2? anchor = null, float fontSize = 26)
        {
            var rect = Rect(parent, name, size, position, anchor ?? new Vector2(0.5f, 0.5f));
            var image = rect.gameObject.AddComponent<Image>();
            
            var spriteName = color == Red ? "ClassroomButtonDanger"
                : (color == Green || color == Navy) ? "ChalkButton"
                : color == Blue ? "ClassroomButtonBlue"
                : color == Purple ? "ClassroomButtonPurple"
                : color == Cyan ? "ClassroomButtonTeal"
                : color == Orange ? "ClassroomButtonWood"
                : "ClassroomButton";
            var sprite = Resources.Load<Sprite>($"ClassroomQuiz/Sprites/{spriteName}")
                ?? Resources.Load<Sprite>("ClassroomQuiz/Sprites/ClassroomButton");

            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
            }
            else
            {
                image.color = color;
            }

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = sprite != null
                ? new ColorBlock
                {
                    normalColor = Color.white,
                    highlightedColor = new Color(1f, 1f, 0.88f, 1f),
                    pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f),
                    selectedColor = Color.white,
                    disabledColor = new Color(1f, 1f, 1f, 0.45f),
                    colorMultiplier = 1,
                    fadeDuration = 0.12f
                }
                : new ColorBlock
                {
                    normalColor = color,
                    highlightedColor = Color.Lerp(color, Color.white, 0.12f),
                    pressedColor = Color.Lerp(color, Color.black, 0.13f),
                    selectedColor = color,
                    disabledColor = new Color(color.r, color.g, color.b, 0.45f),
                    colorMultiplier = 1,
                    fadeDuration = 0.12f
                };
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            var labelText = Text(rect, "Label", label, fontSize, Color.white, TextAlignmentOptions.Center, true);
            labelText.margin = new Vector4(12, 4, 12, 4);
            return button;
        }

        public static TMP_InputField Input(Transform parent, string name, string placeholder, Vector2 size,
            Vector2 position, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard,
            bool multiline = false, Vector2? anchor = null)
        {
            var rect = Rect(parent, name, size, position, anchor ?? new Vector2(0.5f, 0.5f));
            var background = rect.gameObject.AddComponent<Image>();
            background.color = Surface;

            var viewportRect = Rect(rect, "Text Area", Vector2.zero, Vector2.zero);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(16, 8);
            viewportRect.offsetMax = new Vector2(-16, -8);
            viewportRect.gameObject.AddComponent<RectMask2D>();

            var value = Text(viewportRect, "Text", string.Empty, 22, Navy, TextAlignmentOptions.MidlineLeft);
            var placeholderText = Text(viewportRect, "Placeholder", placeholder, 22, Muted, TextAlignmentOptions.MidlineLeft);
            placeholderText.fontStyle = FontStyles.Italic;

            var input = rect.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = viewportRect;
            input.textComponent = value;
            input.placeholder = placeholderText;
            input.targetGraphic = background;
            input.contentType = contentType;
            input.lineType = multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            input.richText = false;
            input.characterLimit = contentType == TMP_InputField.ContentType.Password ? 128 : 160;
            return input;
        }

        public static TMP_Dropdown Dropdown(Transform parent, string name, IReadOnlyList<string> options,
            Vector2 size, Vector2 position, int selectedIndex = 0, Vector2? anchor = null)
        {
            var rect = Rect(parent, name, size, position, anchor ?? new Vector2(0.5f, 0.5f));
            var background = rect.gameObject.AddComponent<Image>();
            background.color = Surface;
            var dropdown = rect.gameObject.AddComponent<TMP_Dropdown>();
            dropdown.targetGraphic = background;
            dropdown.transition = Selectable.Transition.ColorTint;
            dropdown.colors = new ColorBlock
            {
                normalColor = Surface,
                highlightedColor = Hex("F4FAFF"),
                pressedColor = Hex("E7F3FC"),
                selectedColor = Surface,
                disabledColor = Hex("D8E0E7"),
                colorMultiplier = 1,
                fadeDuration = 0.12f
            };

            var caption = Text(rect, "Caption", string.Empty, 21, Navy, TextAlignmentOptions.MidlineLeft);
            caption.rectTransform.offsetMin = new Vector2(18, 3);
            caption.rectTransform.offsetMax = new Vector2(-44, -3);

            var arrow = Text(rect, "Arrow", "▾", 28, Blue, TextAlignmentOptions.Center, true);
            arrow.rectTransform.anchorMin = new Vector2(1, 0);
            arrow.rectTransform.anchorMax = new Vector2(1, 1);
            arrow.rectTransform.pivot = new Vector2(1, 0.5f);
            arrow.rectTransform.sizeDelta = new Vector2(42, 0);
            arrow.rectTransform.anchoredPosition = Vector2.zero;

            var templateRect = Rect(rect, "Template", new Vector2(0, 220), new Vector2(0, -3));
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.offsetMin = new Vector2(0, -220);
            templateRect.offsetMax = Vector2.zero;
            var templateBackground = templateRect.gameObject.AddComponent<Image>();
            templateBackground.color = Surface;
            var scroll = templateRect.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            var viewportRect = Stretch(templateRect, "Viewport");
            viewportRect.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            viewportRect.gameObject.AddComponent<RectMask2D>();
            var contentRect = Stretch(viewportRect, "Content");
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            var layout = contentRect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var sizeFitter = contentRect.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var itemRect = Rect(contentRect, "Item", new Vector2(0, 42), Vector2.zero);
            itemRect.anchorMin = new Vector2(0, 1);
            itemRect.anchorMax = new Vector2(1, 1);
            itemRect.pivot = new Vector2(0.5f, 1);
            itemRect.sizeDelta = new Vector2(0, 42);
            itemRect.gameObject.AddComponent<LayoutElement>().preferredHeight = 42;
            var itemBackground = itemRect.gameObject.AddComponent<Image>();
            itemBackground.color = Surface;
            var toggle = itemRect.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = itemBackground;
            toggle.transition = Selectable.Transition.ColorTint;
            toggle.colors = new ColorBlock
            {
                normalColor = Surface,
                highlightedColor = Hex("EAF5FE"),
                pressedColor = Hex("D8EBFB"),
                selectedColor = Hex("EAF5FE"),
                disabledColor = Hex("D8E0E7"),
                colorMultiplier = 1,
                fadeDuration = 0.08f
            };
            var itemText = Text(itemRect, "Item Label", string.Empty, 20, Navy, TextAlignmentOptions.MidlineLeft);
            itemText.rectTransform.offsetMin = new Vector2(14, 2);
            itemText.rectTransform.offsetMax = new Vector2(-10, -2);

            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            dropdown.captionText = caption;
            dropdown.template = templateRect;
            dropdown.itemText = itemText;
            dropdown.itemImage = itemBackground;
            dropdown.options = new List<TMP_Dropdown.OptionData>();
            if (options != null)
            {
                for (var i = 0; i < options.Count; i++)
                {
                    dropdown.options.Add(new TMP_Dropdown.OptionData(options[i]));
                }
            }
            dropdown.value = options == null || options.Count == 0 ? 0 : Mathf.Clamp(selectedIndex, 0, options.Count - 1);
            dropdown.RefreshShownValue();
            templateRect.gameObject.SetActive(false);
            return dropdown;
        }

        public static Slider Slider(Transform parent, string name, Vector2 size, Vector2 position,
            float minimum, float maximum, float value, Vector2? anchor = null)
        {
            var rect = Rect(parent, name, size, position, anchor ?? new Vector2(0.5f, 0.5f));
            var background = rect.gameObject.AddComponent<Image>();
            background.color = Border;

            var fillArea = Rect(rect, "Fill Area", Vector2.zero, Vector2.zero);
            fillArea.anchorMin = new Vector2(0, 0.25f);
            fillArea.anchorMax = new Vector2(1, 0.75f);
            fillArea.offsetMin = new Vector2(5, 0);
            fillArea.offsetMax = new Vector2(-5, 0);
            var fillRect = Rect(fillArea, "Fill", Vector2.zero, Vector2.zero);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fill = fillRect.gameObject.AddComponent<Image>();
            fill.color = Blue;

            var handleArea = Rect(rect, "Handle Slide Area", Vector2.zero, Vector2.zero);
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(12, 0);
            handleArea.offsetMax = new Vector2(-12, 0);
            var handleRect = Rect(handleArea, "Handle", new Vector2(24, 24), Vector2.zero);
            var handle = handleRect.gameObject.AddComponent<Image>();
            handle.color = Surface;

            var slider = rect.gameObject.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.minValue = minimum;
            slider.maxValue = maximum;
            slider.value = Mathf.Clamp(value, minimum, maximum);
            return slider;
        }

        public static Image ProgressBar(Transform parent, string name, Vector2 size, Vector2 position, Color fillColor,
            Vector2? anchor = null)
        {
            var rect = Rect(parent, name, size, position, anchor ?? new Vector2(0.5f, 0.5f));
            rect.gameObject.AddComponent<Image>().color = Border;
            var fillRect = Rect(rect, "Fill", Vector2.zero, Vector2.zero);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = Vector2.zero;
            var fill = fillRect.gameObject.AddComponent<Image>();
            fill.color = fillColor;
            return fill;
        }

        public static RectTransform ScrollContent(Transform parent, string name, Vector2 size, Vector2 position, out ScrollRect scroll)
        {
            var rect = Rect(parent, name, size, position);
            var background = rect.gameObject.AddComponent<Image>();
            background.color = new Color(1, 1, 1, 0.55f);
            scroll = rect.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            var viewport = Stretch(rect, "Viewport", new Vector2(6, 6), new Vector2(6, 6));
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect(viewport, "Content", Vector2.zero, Vector2.zero);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 0);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        public static Color Hex(string value)
        {
            return ColorUtility.TryParseHtmlString("#" + value, out var color) ? color : Color.white;
        }

        public static void SetVisible(GameObject target, bool visible)
        {
            if (target != null && target.activeSelf != visible)
            {
                target.SetActive(visible);
            }
        }
    }
}
