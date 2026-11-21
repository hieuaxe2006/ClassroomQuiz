using UnityEngine;

namespace ClassroomQuiz.UI
{
    /// <summary>
    /// Shared classroom theme colors used by runtime-created UI and authored UI assets.
    /// </summary>
    public static class ClassroomQuizDesignSystem
    {
        public static readonly Color BoardGreen = Parse("1E3F20");
        public static readonly Color DeskWood = Parse("8B5A2B");
        public static readonly Color BadgeGold = Parse("F1C40F");
        public static readonly Color ChalkWhite = Parse("F8F9FA");
        public static readonly Color Background = ChalkWhite;
        public static readonly Color Surface = Color.white;
        public static readonly Color Muted = Parse("63788F");
        public static readonly Color Green = BoardGreen;
        public static readonly Color Blue = Parse("3B7A57");
        public static readonly Color Cyan = Parse("6A9B83");
        public static readonly Color Purple = Parse("7655BD");
        public static readonly Color Orange = DeskWood;
        public static readonly Color Red = Parse("C84C51");
        public static readonly Color Border = Parse("D9D2C4");

        private static Color Parse(string html)
        {
            return ColorUtility.TryParseHtmlString("#" + html, out var color) ? color : Color.white;
        }
    }
}
