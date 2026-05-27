using System.Collections.Generic;
using ArrowGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArrowGame
{
    public static class ThemeManager
    {
        public readonly struct ThemePalette
        {
            public readonly bool IsDarkMode;
            public readonly Color CameraBackgroundColor;
            public readonly Color SurfaceColor;
            public readonly Color SurfaceSoftColor;
            public readonly Color AccentColor;
            public readonly Color AccentSecondaryColor;
            public readonly Color TextPrimaryColor;
            public readonly Color TextSecondaryColor;
            public readonly Color DisabledColor;
            public readonly Color ToggleEnabledColor;
            public readonly Color ToggleDisabledColor;
            public readonly Color ArrowColor;
            public readonly Color ArrowHintColor;
            public readonly Color ArrowBlockedColor;
            public readonly Color GuideLineColor;
            public readonly Color DotColor;
            public readonly Color DotWinHighlightColor;
            public readonly Color HeartFilledColor;
            public readonly Color HeartEmptyColor;
            public readonly Color GuideButtonOnColor;
            public readonly Color GuideButtonOffColor;
            public readonly Color SelectedTabColor;
            public readonly Color UnselectedTabColor;
            public readonly Color SelectedTabLabelColor;
            public readonly Color UnselectedTabLabelColor;
            public readonly Color ChallengePlayedColor;
            public readonly Color ChallengeCurrentColor;
            public readonly Color ChallengeMissedColor;
            public readonly Color ChallengePendingColor;
            public readonly Color LeaderboardDefaultBackgroundColor;
            public readonly Color LeaderboardFirstPlaceBackgroundColor;
            public readonly Color LeaderboardSecondPlaceBackgroundColor;
            public readonly Color LeaderboardThirdPlaceBackgroundColor;
            public readonly Color LeaderboardPlayerBackgroundColor;
            public readonly Color LeaderboardHighlightedTextColor;
            public readonly Color LeaderboardDefaultTextColor;

            public ThemePalette(
                bool isDarkMode,
                Color cameraBackgroundColor,
                Color surfaceColor,
                Color surfaceSoftColor,
                Color accentColor,
                Color accentSecondaryColor,
                Color textPrimaryColor,
                Color textSecondaryColor,
                Color disabledColor,
                Color toggleEnabledColor,
                Color toggleDisabledColor,
                Color arrowColor,
                Color arrowHintColor,
                Color arrowBlockedColor,
                Color guideLineColor,
                Color dotColor,
                Color dotWinHighlightColor,
                Color heartFilledColor,
                Color heartEmptyColor,
                Color guideButtonOnColor,
                Color guideButtonOffColor,
                Color selectedTabColor,
                Color unselectedTabColor,
                Color selectedTabLabelColor,
                Color unselectedTabLabelColor,
                Color challengePlayedColor,
                Color challengeCurrentColor,
                Color challengeMissedColor,
                Color challengePendingColor,
                Color leaderboardDefaultBackgroundColor,
                Color leaderboardFirstPlaceBackgroundColor,
                Color leaderboardSecondPlaceBackgroundColor,
                Color leaderboardThirdPlaceBackgroundColor,
                Color leaderboardPlayerBackgroundColor,
                Color leaderboardHighlightedTextColor,
                Color leaderboardDefaultTextColor)
            {
                IsDarkMode = isDarkMode;
                CameraBackgroundColor = cameraBackgroundColor;
                SurfaceColor = surfaceColor;
                SurfaceSoftColor = surfaceSoftColor;
                AccentColor = accentColor;
                AccentSecondaryColor = accentSecondaryColor;
                TextPrimaryColor = textPrimaryColor;
                TextSecondaryColor = textSecondaryColor;
                DisabledColor = disabledColor;
                ToggleEnabledColor = toggleEnabledColor;
                ToggleDisabledColor = toggleDisabledColor;
                ArrowColor = arrowColor;
                ArrowHintColor = arrowHintColor;
                ArrowBlockedColor = arrowBlockedColor;
                GuideLineColor = guideLineColor;
                DotColor = dotColor;
                DotWinHighlightColor = dotWinHighlightColor;
                HeartFilledColor = heartFilledColor;
                HeartEmptyColor = heartEmptyColor;
                GuideButtonOnColor = guideButtonOnColor;
                GuideButtonOffColor = guideButtonOffColor;
                SelectedTabColor = selectedTabColor;
                UnselectedTabColor = unselectedTabColor;
                SelectedTabLabelColor = selectedTabLabelColor;
                UnselectedTabLabelColor = unselectedTabLabelColor;
                ChallengePlayedColor = challengePlayedColor;
                ChallengeCurrentColor = challengeCurrentColor;
                ChallengeMissedColor = challengeMissedColor;
                ChallengePendingColor = challengePendingColor;
                LeaderboardDefaultBackgroundColor = leaderboardDefaultBackgroundColor;
                LeaderboardFirstPlaceBackgroundColor = leaderboardFirstPlaceBackgroundColor;
                LeaderboardSecondPlaceBackgroundColor = leaderboardSecondPlaceBackgroundColor;
                LeaderboardThirdPlaceBackgroundColor = leaderboardThirdPlaceBackgroundColor;
                LeaderboardPlayerBackgroundColor = leaderboardPlayerBackgroundColor;
                LeaderboardHighlightedTextColor = leaderboardHighlightedTextColor;
                LeaderboardDefaultTextColor = leaderboardDefaultTextColor;
            }
        }

        private enum VisualRole
        {
            Surface,
            SurfaceSoft,
            Accent,
            TextPrimary,
            TextSecondary,
            Disabled,
            PreserveHue
        }

        private static readonly ThemePalette LightPalette = new(
            false,
            new Color(0.92f, 0.95f, 1f, 1f),
            new Color(0.94f, 0.96f, 1f, 1f),
            new Color(0.83f, 0.88f, 0.96f, 1f),
            new Color(1f, 0.82f, 0.29f, 1f),
            new Color(0.35f, 0.43f, 0.98f, 1f),
            new Color(0.15f, 0.18f, 0.28f, 1f),
            new Color(0.38f, 0.44f, 0.56f, 1f),
            new Color(0.66f, 0.71f, 0.8f, 1f),
            new Color(1f, 0.82f, 0.29f, 1f),
            new Color(0.45f, 0.52f, 0.64f, 1f),
            Color.black,
            new Color(1f, 0.83f, 0.2f, 1f),
            new Color(0.9f, 0.24f, 0.24f, 1f),
            new Color(0f, 0.14f, 1f, 0.52f),
            new Color(0f, 0.14f, 1f, 0.39f),
            new Color(0.56f, 0.65f, 0.98f, 0.92f),
            new Color(0.98f, 0.3f, 0.38f, 1f),
            new Color(0.22f, 0.24f, 0.3f, 1f),
            new Color(0.54f, 0.63f, 1f, 0.96f),
            new Color(0.33f, 0.35f, 0.51f, 0.92f),
            new Color(0.35f, 0.43f, 0.98f, 0.18f),
            Color.clear,
            new Color(0.15f, 0.18f, 0.28f, 1f),
            new Color(0.38f, 0.44f, 0.56f, 1f),
            new Color(0.34f, 0.7f, 0.43f, 0.95f),
            new Color(0.35f, 0.43f, 0.98f, 0.95f),
            new Color(0.43f, 0.2f, 0.23f, 0.92f),
            new Color(0.82f, 0.87f, 0.94f, 0.95f),
            new Color(0.14f, 0.16f, 0.25f, 0.92f),
            new Color(0.95f, 0.76f, 0.23f, 0.95f),
            new Color(0.73f, 0.79f, 0.88f, 0.92f),
            new Color(0.88f, 0.58f, 0.35f, 0.92f),
            new Color(0.35f, 0.43f, 0.98f, 0.9f),
            Color.white,
            new Color(0.92f, 0.94f, 1f, 1f));

        private static readonly ThemePalette DarkPalette = new(
            true,
            new Color(0.06f, 0.08f, 0.11f, 1f),
            new Color(0.15f, 0.19f, 0.25f, 1f),
            new Color(0.2f, 0.25f, 0.33f, 1f),
            new Color(0.49f, 0.66f, 1f, 1f),
            new Color(0.97f, 0.78f, 0.28f, 1f),
            new Color(0.94f, 0.97f, 1f, 1f),
            new Color(0.74f, 0.81f, 0.9f, 1f),
            new Color(0.5f, 0.57f, 0.67f, 1f),
            new Color(0.49f, 0.66f, 1f, 1f),
            new Color(0.32f, 0.38f, 0.48f, 1f),
            new Color(0.94f, 0.97f, 1f, 1f),
            new Color(1f, 0.86f, 0.3f, 1f),
            new Color(1f, 0.45f, 0.45f, 1f),
            new Color(0.49f, 0.66f, 1f, 0.72f),
            new Color(0.49f, 0.66f, 1f, 0.28f),
            new Color(0.92f, 0.96f, 1f, 0.92f),
            new Color(1f, 0.49f, 0.58f, 1f),
            new Color(0.24f, 0.28f, 0.35f, 1f),
            new Color(0.49f, 0.66f, 1f, 0.96f),
            new Color(0.26f, 0.31f, 0.4f, 0.92f),
            new Color(0.49f, 0.66f, 1f, 0.22f),
            Color.clear,
            new Color(0.94f, 0.97f, 1f, 1f),
            new Color(0.67f, 0.76f, 0.88f, 1f),
            new Color(0.31f, 0.7f, 0.47f, 0.95f),
            new Color(0.49f, 0.66f, 1f, 0.95f),
            new Color(0.63f, 0.26f, 0.31f, 0.92f),
            new Color(0.2f, 0.24f, 0.31f, 0.95f),
            new Color(0.16f, 0.19f, 0.26f, 0.92f),
            new Color(0.95f, 0.76f, 0.23f, 0.95f),
            new Color(0.62f, 0.7f, 0.86f, 0.92f),
            new Color(0.78f, 0.52f, 0.34f, 0.92f),
            new Color(0.49f, 0.66f, 1f, 0.9f),
            new Color(0.97f, 0.98f, 1f, 1f),
            new Color(0.9f, 0.94f, 1f, 1f));

        private static readonly Dictionary<int, Color> GraphicBaseColors = new();
        private static readonly Dictionary<int, Color> TextBaseColors = new();
        private static readonly Dictionary<int, Color> SpriteBaseColors = new();
        private static readonly Dictionary<int, Color> CameraBaseColors = new();
        private static bool isApplyingThemeToScene;

        public static bool IsDarkModeEnabled => GameDataStore.IsDarkModeEnabled;
        public static ThemePalette CurrentPalette => IsDarkModeEnabled ? DarkPalette : LightPalette;

        public static void SetDarkModeEnabled(bool isEnabled)
        {
            if (GameDataStore.IsDarkModeEnabled == isEnabled)
                return;

            GameDataStore.IsDarkModeEnabled = isEnabled;
        }

        public static void ApplyThemeToScene(Scene scene)
        {
            if (!scene.IsValid())
                return;

            if (isApplyingThemeToScene)
                return;

            ThemePalette palette = CurrentPalette;
            isApplyingThemeToScene = true;
            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    ApplyThemeToRoot(roots[i], palette);
                }
            }
            finally
            {
                isApplyingThemeToScene = false;
            }
        }

        public static void ApplyButtonTheme(
            Button button,
            Color backgroundColor,
            Color foregroundColor,
            Color disabledBackgroundColor,
            Color disabledForegroundColor,
            bool tintChildImages = false)
        {
            if (button == null)
                return;

            if (button.targetGraphic is Image backgroundImage)
                backgroundImage.color = button.interactable ? backgroundColor : disabledBackgroundColor;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.96f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = Color.white;
            button.colors = colors;

            Graphic[] graphics = button.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == null || graphic == button.targetGraphic || graphic is TMP_SubMeshUI)
                    continue;

                if (graphic is TextMeshProUGUI text)
                {
                    text.color = button.interactable ? foregroundColor : disabledForegroundColor;
                    continue;
                }

                if (tintChildImages && graphic is Image image)
                    image.color = button.interactable ? foregroundColor : disabledForegroundColor;
            }
        }

        private static void ApplyThemeToRoot(GameObject root, ThemePalette palette)
        {
            if (root == null)
                return;

            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == null || graphic is TMP_Text || graphic is TMP_SubMeshUI)
                    continue;

                ApplyGraphicTheme(graphic, palette);
            }

            TextMeshProUGUI[] texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                    ApplyTextTheme(texts[i], palette);
            }

            SpriteRenderer[] sprites = root.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null)
                    ApplySpriteTheme(sprites[i], palette);
            }

            Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null)
                    ApplyCameraTheme(cameras[i], palette);
            }
        }

        private static void ApplyGraphicTheme(Graphic graphic, ThemePalette palette)
        {
            int id = graphic.GetInstanceID();
            if (!GraphicBaseColors.ContainsKey(id))
                GraphicBaseColors[id] = graphic.color;

            Color baseColor = GraphicBaseColors[id];
            graphic.color = palette.IsDarkMode
                ? ResolveGraphicDarkColor(graphic, baseColor, palette)
                : baseColor;
        }

        private static void ApplyTextTheme(TextMeshProUGUI text, ThemePalette palette)
        {
            int id = text.GetInstanceID();
            if (!TextBaseColors.ContainsKey(id))
                TextBaseColors[id] = text.color;

            Color baseColor = TextBaseColors[id];
            text.color = palette.IsDarkMode
                ? ResolveTextDarkColor(text, baseColor, palette)
                : baseColor;
        }

        private static void ApplySpriteTheme(SpriteRenderer sprite, ThemePalette palette)
        {
            if (sprite.GetComponentInParent<LineController>() != null || sprite.GetComponentInParent<LineGenerator>() != null)
                return;

            int id = sprite.GetInstanceID();
            if (!SpriteBaseColors.ContainsKey(id))
                SpriteBaseColors[id] = sprite.color;

            Color baseColor = SpriteBaseColors[id];
            sprite.color = palette.IsDarkMode
                ? ResolveSpriteDarkColor(sprite, baseColor, palette)
                : baseColor;
        }

        private static void ApplyCameraTheme(Camera camera, ThemePalette palette)
        {
            int id = camera.GetInstanceID();
            if (!CameraBaseColors.ContainsKey(id))
                CameraBaseColors[id] = camera.backgroundColor;

            camera.backgroundColor = palette.IsDarkMode
                ? palette.CameraBackgroundColor
                : CameraBaseColors[id];
        }

        private static Color ResolveGraphicDarkColor(Graphic graphic, Color baseColor, ThemePalette palette)
        {
            if (baseColor.a <= 0.001f)
                return baseColor;

            VisualRole role = DetectGraphicRole(graphic, baseColor);
            return ApplyRole(baseColor, palette, role);
        }

        private static Color ResolveTextDarkColor(TextMeshProUGUI text, Color baseColor, ThemePalette palette)
        {
            if (baseColor.a <= 0.001f)
                return baseColor;

            string searchText = GetSearchText(text);
            bool isSecondary = ContainsAny(searchText, "secondary", "summary", "status", "chance", "timer", "placeholder", "sub", "caption");
            return CopyAlpha(isSecondary ? palette.TextSecondaryColor : palette.TextPrimaryColor, baseColor.a);
        }

        private static Color ResolveSpriteDarkColor(SpriteRenderer sprite, Color baseColor, ThemePalette palette)
        {
            if (baseColor.a <= 0.001f)
                return baseColor;

            string searchText = GetSearchText(sprite);
            if (IsBackgroundSprite(sprite, searchText))
                return CopyAlpha(palette.CameraBackgroundColor, baseColor.a);

            if (ContainsAny(searchText, "icon", "badge", "marker"))
                return CopyAlpha(palette.AccentColor, baseColor.a);

            return ApplyRole(baseColor, palette, DetectColorRole(baseColor));
        }

        private static bool IsBackgroundSprite(SpriteRenderer sprite, string searchText)
        {
            if (sprite == null)
                return false;

            string spriteName = sprite.name.ToLowerInvariant();
            return spriteName == "bg" ||
                   spriteName == "background" ||
                   ContainsAny(searchText, " background", " bg ");
        }

        private static VisualRole DetectGraphicRole(Graphic graphic, Color baseColor)
        {
            string searchText = GetSearchText(graphic);
            if (ContainsAny(searchText, "knob", "icon"))
                return VisualRole.TextPrimary;

            if (ContainsAny(searchText, "background", "panel", "card", "surface", "container", "viewport", "overlay", "track"))
                return baseColor.a < 0.85f ? VisualRole.SurfaceSoft : VisualRole.Surface;

            if (ContainsAny(searchText, "play button", "play challenge button", "streak button", "hint button", "submit score button"))
                return VisualRole.Accent;

            if (ContainsAny(searchText, "home button", "retry button", "close button", "line trace button", "leaderboard main menu button", "toggle button"))
                return VisualRole.SurfaceSoft;

            if (ContainsAny(searchText, "fill", "progress", "button", "toggle", "accent", "badge", "marker", "play", "flame"))
                return VisualRole.Accent;

            if (ContainsAny(searchText, "shadow", "outline", "disabled"))
                return VisualRole.Disabled;

            return DetectColorRole(baseColor);
        }

        private static VisualRole DetectColorRole(Color color)
        {
            float luminance = GetLuminance(color);
            float saturation = GetSaturation(color);

            if (saturation >= 0.2f)
                return VisualRole.PreserveHue;
            if (luminance >= 0.8f)
                return VisualRole.TextPrimary;
            if (luminance >= 0.5f)
                return VisualRole.TextSecondary;
            if (luminance >= 0.25f)
                return VisualRole.SurfaceSoft;

            return VisualRole.Surface;
        }

        private static Color ApplyRole(Color sourceColor, ThemePalette palette, VisualRole role)
        {
            return role switch
            {
                VisualRole.Surface => CopyAlpha(palette.SurfaceColor, sourceColor.a),
                VisualRole.SurfaceSoft => CopyAlpha(palette.SurfaceSoftColor, sourceColor.a),
                VisualRole.Accent => CopyAlpha(BlendAccentColor(sourceColor, palette), sourceColor.a),
                VisualRole.TextPrimary => CopyAlpha(palette.TextPrimaryColor, sourceColor.a),
                VisualRole.TextSecondary => CopyAlpha(palette.TextSecondaryColor, sourceColor.a),
                VisualRole.Disabled => CopyAlpha(palette.DisabledColor, sourceColor.a),
                _ => CopyAlpha(BlendAccentColor(sourceColor, palette), sourceColor.a)
            };
        }

        private static Color BlendAccentColor(Color sourceColor, ThemePalette palette)
        {
            Color.RGBToHSV(sourceColor, out float hue, out float saturation, out float value);
            float themedSaturation = Mathf.Clamp01(Mathf.Lerp(0.4f, 0.85f, saturation));
            float themedValue = Mathf.Clamp01(Mathf.Lerp(0.5f, 0.88f, value));
            Color huePreserved = Color.HSVToRGB(hue, themedSaturation, themedValue);
            return Color.Lerp(palette.AccentColor, huePreserved, 0.45f);
        }

        private static string GetSearchText(Component component)
        {
            if (component == null)
                return string.Empty;

            string text = component.name.ToLowerInvariant();
            Transform current = component.transform.parent;
            int depth = 0;
            while (current != null && depth < 3)
            {
                text += " " + current.name.ToLowerInvariant();
                current = current.parent;
                depth++;
            }

            return text;
        }

        private static bool ContainsAny(string source, params string[] keywords)
        {
            if (string.IsNullOrWhiteSpace(source) || keywords == null)
                return false;

            for (int i = 0; i < keywords.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(keywords[i]) && source.Contains(keywords[i]))
                    return true;
            }

            return false;
        }

        private static float GetLuminance(Color color)
        {
            return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
        }

        private static float GetSaturation(Color color)
        {
            Color.RGBToHSV(color, out _, out float saturation, out _);
            return saturation;
        }

        private static Color CopyAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
