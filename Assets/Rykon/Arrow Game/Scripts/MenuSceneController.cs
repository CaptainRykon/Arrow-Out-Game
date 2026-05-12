using System;
using System.Collections.Generic;
using ArrowGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArrowGame
{
    public class MenuSceneController : MonoBehaviour
    {
        private const string GameSceneName = "ArrowGame";
        private const string ChallengeSceneName = "Challange";
        private const string TutorialSceneName = "TutorialScene";

        [Header("Panels")]
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject collectionPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Navigation")]
        [SerializeField] private Button homeTabButton;
        [SerializeField] private Button collectionTabButton;
        [SerializeField] private Button settingsTabButton;
        [SerializeField] private Image homeTabBackground;
        [SerializeField] private Image collectionTabBackground;
        [SerializeField] private Image settingsTabBackground;
        [SerializeField] private TextMeshProUGUI homeTabLabel;
        [SerializeField] private TextMeshProUGUI collectionTabLabel;
        [SerializeField] private TextMeshProUGUI settingsTabLabel;

        [Header("Actions")]
        [SerializeField] private Button primaryPlayButton;
        [SerializeField] private Button cardPlayButton;
        [SerializeField] private Button challengePlayButton;
        [SerializeField] private TextMeshProUGUI currentLevelLabel;

        [Header("Settings")]
        [SerializeField] private TMP_InputField userNameInputField;
        [SerializeField] private Button vibrationToggleButton;
        [SerializeField] private Image vibrationToggleBackground;
        [SerializeField] private RectTransform vibrationToggleKnob;
        [SerializeField] private Button soundToggleButton;
        [SerializeField] private Image soundToggleBackground;
        [SerializeField] private RectTransform soundToggleKnob;
        [SerializeField] private Button darkModeToggleButton;
        [SerializeField] private Image darkModeToggleBackground;
        [SerializeField] private RectTransform darkModeToggleKnob;
        [SerializeField] private Button privacyButton;
        [SerializeField] private Button termsButton;
        [SerializeField] private Button faqButton;
        [SerializeField] private Button telegramButton;
        [SerializeField] private Button twitterButton;
        [SerializeField] private string privacyUrl;
        [SerializeField] private string termsUrl;
        [SerializeField] private string faqUrl;
        [SerializeField] private string telegramUrl;
        [SerializeField] private string twitterUrl;

        [Header("Settings Theme")]
        [SerializeField] private Image[] themeSurfaceImages;
        [SerializeField] private Image[] themeAccentImages;
        [SerializeField] private TextMeshProUGUI[] themePrimaryTexts;
        [SerializeField] private TextMeshProUGUI[] themeSecondaryTexts;
        [SerializeField] private Color lightSurfaceColor = new(0.25f, 0.41f, 0.59f, 1f);
        [SerializeField] private Color darkSurfaceColor = new(0.14f, 0.18f, 0.24f, 1f);
        [SerializeField] private Color lightAccentColor = new(1f, 0.82f, 0.29f, 1f);
        [SerializeField] private Color darkAccentColor = new(0.45f, 0.67f, 1f, 1f);
        [SerializeField] private Color lightPrimaryTextColor = new(0.95f, 0.96f, 1f, 1f);
        [SerializeField] private Color darkPrimaryTextColor = new(0.9f, 0.95f, 1f, 1f);
        [SerializeField] private Color lightSecondaryTextColor = new(0.73f, 0.84f, 1f, 1f);
        [SerializeField] private Color darkSecondaryTextColor = new(0.77f, 0.84f, 0.92f, 1f);
        [SerializeField] private Color toggleEnabledColor = new(1f, 0.82f, 0.29f, 1f);
        [SerializeField] private Color toggleDisabledColor = new(0.45f, 0.52f, 0.64f, 1f);
        [SerializeField] private Vector2 toggleKnobOnPosition = new(26f, 0f);
        [SerializeField] private Vector2 toggleKnobOffPosition = new(-26f, 0f);

        [Header("Challenge UI")]
        [SerializeField] private string challengeTitlePrefix = "Weekly Challenge";
        [SerializeField] private string[] challengePatternNames = { "Star", "Duck", "Bolt", "Crown", "Leaf", "Rocket", "Moon" };
        [SerializeField] private TextMeshProUGUI challengeTitleText;
        [SerializeField] private TextMeshProUGUI challengePatternText;
        [SerializeField] private TextMeshProUGUI challengeCycleTimerText;
        [SerializeField] private TextMeshProUGUI challengeChanceText;
        [SerializeField] private TextMeshProUGUI challengeNextChanceTimerText;
        [SerializeField] private TextMeshProUGUI challengeStatusText;
        [SerializeField] private Button streakButton;
        [SerializeField] private GameObject streakPanel;
        [SerializeField] private Button closeStreakButton;
        [SerializeField] private TextMeshProUGUI streakHeadlineText;
        [SerializeField] private TextMeshProUGUI streakSummaryText;
        [SerializeField] private ChallengeStreakDayView[] streakDayViews;

        [Header("Colors")]
        [SerializeField] private Color selectedTabColor = new(0.35f, 0.43f, 0.98f, 0.18f);
        [SerializeField] private Color unselectedTabColor = Color.clear;
        [SerializeField] private Color selectedLabelColor = new(0.95f, 0.96f, 1f, 1f);
        [SerializeField] private Color unselectedLabelColor = new(0.67f, 0.7f, 0.84f, 1f);

        private const float ChallengeUiRefreshInterval = 0.25f;
        private float nextChallengeUiRefreshTime;

        private void OnValidate()
        {
            TryAssignSettingsReferencesFromPanel();
        }

        private void Awake()
        {
            TryAssignSettingsReferencesFromPanel();
            WireButtons();
            WireSettingsControls();
            CloseStreakPanel();
            RefreshLevelLabel();
            RefreshSettingsUi();
            ShowHome();
        }

        private void OnEnable()
        {
            TryAssignSettingsReferencesFromPanel();
            WireSettingsControls();
            nextChallengeUiRefreshTime = 0f;
            RefreshLevelLabel();
            RefreshChallengeUi();
            RefreshSettingsUi();
        }

        private void Start()
        {
            if (!GameDataStore.HasCompletedTutorial)
                SceneManager.LoadScene(TutorialSceneName);
        }

        private void OnDestroy()
        {
            if (userNameInputField != null)
                userNameInputField.onEndEdit.RemoveListener(HandleUserNameChanged);
        }

        public void ShowHome()
        {
            SetTabState(true, false, false);
        }

        public void ShowCollection()
        {
            CloseStreakPanel();
            SetTabState(false, true, false);
        }

        public void ShowSettings()
        {
            CloseStreakPanel();
            RefreshSettingsUi();
            SetTabState(false, false, true);
        }

        public void PlayGame()
        {
            SoundManager.PlayButtonClick();
            HapticManager.PlayButtonTap();
            SceneManager.LoadScene(GameDataStore.HasCompletedTutorial ? GameSceneName : TutorialSceneName);
        }

        public void PlayChallenge()
        {
            SoundManager.PlayButtonClick();
            HapticManager.PlayButtonTap();
            if (!GameDataStore.CanPlayChallengeToday(DateTime.UtcNow))
            {
                RefreshChallengeUi();
                return;
            }

            SceneManager.LoadScene(ChallengeSceneName);
        }

        public void OpenStreakPanel()
        {
            if (streakPanel != null)
                streakPanel.SetActive(true);
        }

        public void CloseStreakPanel()
        {
            if (streakPanel != null)
                streakPanel.SetActive(false);
        }

        public void RefreshLevelLabel()
        {
            if (currentLevelLabel != null)
                currentLevelLabel.text = $"Level {GameDataStore.Level}";
        }

        private void WireButtons()
        {
            RegisterButton(homeTabButton, ShowHome);
            RegisterButton(collectionTabButton, ShowCollection);
            RegisterButton(settingsTabButton, ShowSettings);
            RegisterButton(primaryPlayButton, PlayGame);
            RegisterButton(cardPlayButton, PlayGame);
            RegisterButton(challengePlayButton, PlayChallenge);
            RegisterButton(streakButton, OpenStreakPanel);
            RegisterButton(closeStreakButton, CloseStreakPanel);
        }

        private void WireSettingsControls()
        {
            TryAssignSettingsReferencesFromPanel();
            RegisterButton(vibrationToggleButton, ToggleVibration);
            RegisterButton(soundToggleButton, ToggleSound);
            RegisterButton(darkModeToggleButton, ToggleDarkMode);
            RegisterButton(privacyButton, OpenPrivacy);
            RegisterButton(termsButton, OpenTerms);
            RegisterButton(faqButton, OpenFaq);
            RegisterButton(telegramButton, OpenTelegram);
            RegisterButton(twitterButton, OpenTwitter);

            if (userNameInputField != null)
            {
                userNameInputField.onEndEdit.RemoveListener(HandleUserNameChanged);
                userNameInputField.onEndEdit.AddListener(HandleUserNameChanged);
            }
        }

        private void TryAssignSettingsReferencesFromPanel()
        {
            if (settingsPanel == null)
                return;

            userNameInputField = FindFirstInputField(settingsPanel, userNameInputField);

            vibrationToggleButton = FindButtonByKeywords(settingsPanel, vibrationToggleButton, "vibration", "vibrations");
            vibrationToggleBackground = FindButtonBackground(vibrationToggleButton, vibrationToggleBackground);
            vibrationToggleKnob = FindToggleKnob(vibrationToggleButton, vibrationToggleKnob);

            soundToggleButton = FindButtonByKeywords(settingsPanel, soundToggleButton, "sound", "sounds");
            soundToggleBackground = FindButtonBackground(soundToggleButton, soundToggleBackground);
            soundToggleKnob = FindToggleKnob(soundToggleButton, soundToggleKnob);

            darkModeToggleButton = FindButtonByKeywords(settingsPanel, darkModeToggleButton, "dark");
            darkModeToggleBackground = FindButtonBackground(darkModeToggleButton, darkModeToggleBackground);
            darkModeToggleKnob = FindToggleKnob(darkModeToggleButton, darkModeToggleKnob);

            privacyButton = FindButtonByKeywords(settingsPanel, privacyButton, "privacy");
            termsButton = FindButtonByKeywords(settingsPanel, termsButton, "terms");
            faqButton = FindButtonByKeywords(settingsPanel, faqButton, "faq");
            telegramButton = FindButtonByKeywords(settingsPanel, telegramButton, "telegram");
            twitterButton = FindButtonByKeywords(settingsPanel, twitterButton, "twitter");

            themeSurfaceImages = BuildSurfaceThemeImages();
            themeAccentImages = BuildAccentThemeImages();
            themePrimaryTexts = settingsPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        }

        private Image[] BuildSurfaceThemeImages()
        {
            List<Image> images = new();
            AddIfNotNull(images, settingsPanel != null ? settingsPanel.GetComponent<Image>() : null);
            AddIfNotNull(images, userNameInputField != null ? userNameInputField.GetComponent<Image>() : null);
            return images.ToArray();
        }

        private Image[] BuildAccentThemeImages()
        {
            List<Image> images = new();
            AddIfNotNull(images, FindButtonBackground(privacyButton, null));
            AddIfNotNull(images, FindButtonBackground(termsButton, null));
            AddIfNotNull(images, FindButtonBackground(faqButton, null));
            AddIfNotNull(images, FindButtonBackground(telegramButton, null));
            AddIfNotNull(images, FindButtonBackground(twitterButton, null));
            return images.ToArray();
        }

        private static void RegisterButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private void SetTabState(bool showHome, bool showCollection, bool showSettings)
        {
            if (homePanel != null)
                homePanel.SetActive(showHome);
            if (collectionPanel != null)
                collectionPanel.SetActive(showCollection);
            if (settingsPanel != null)
                settingsPanel.SetActive(showSettings);

            SetTabVisual(homeTabBackground, homeTabLabel, showHome);
            SetTabVisual(collectionTabBackground, collectionTabLabel, showCollection);
            SetTabVisual(settingsTabBackground, settingsTabLabel, showSettings);
        }

        private void SetTabVisual(Image background, TextMeshProUGUI label, bool isSelected)
        {
            ThemeManager.ThemePalette palette = ThemeManager.CurrentPalette;

            if (background != null)
                background.color = isSelected ? palette.SelectedTabColor : palette.UnselectedTabColor;

            if (label != null)
                label.color = isSelected ? palette.SelectedTabLabelColor : palette.UnselectedTabLabelColor;
        }

        private void ToggleVibration()
        {
            bool isEnabled = !GameDataStore.IsVibrationEnabled;
            GameDataStore.IsVibrationEnabled = isEnabled;
            RefreshSettingsUi();

            if (isEnabled)
                HapticManager.PlayToggleEnabledPreview();
        }

        private void ToggleSound()
        {
            bool isEnabled = !GameDataStore.IsSoundEnabled;
            GameDataStore.IsSoundEnabled = isEnabled;
            SoundManager.ApplySoundEnabled(isEnabled);
            RefreshSettingsUi();
        }

        private void ToggleDarkMode()
        {
            ThemeManager.SetDarkModeEnabled(!ThemeManager.IsDarkModeEnabled);
            RefreshSettingsUi();
        }

        private void OpenPrivacy() => OpenExternalUrl(privacyUrl);
        private void OpenTerms() => OpenExternalUrl(termsUrl);
        private void OpenFaq() => OpenExternalUrl(faqUrl);
        private void OpenTelegram() => OpenExternalUrl(telegramUrl);
        private void OpenTwitter() => OpenExternalUrl(twitterUrl);

        private void OpenExternalUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            SoundManager.PlayButtonClick();
            HapticManager.PlayButtonTap();
            Application.OpenURL(url);
        }

        private void HandleUserNameChanged(string value)
        {
            GameDataStore.PlayerName = value;
            RefreshSettingsUi();
        }

        private void RefreshSettingsUi()
        {
            if (userNameInputField != null)
                userNameInputField.SetTextWithoutNotify(GameDataStore.PlayerName);

            bool isVibrationEnabled = GameDataStore.IsVibrationEnabled;
            bool isSoundEnabled = GameDataStore.IsSoundEnabled;
            bool isDarkModeEnabled = ThemeManager.IsDarkModeEnabled;

            ApplyDarkModeState(isDarkModeEnabled);
            UpdateToggleVisual(vibrationToggleBackground, vibrationToggleKnob, isVibrationEnabled);
            UpdateToggleVisual(soundToggleBackground, soundToggleKnob, isSoundEnabled);
            UpdateToggleVisual(darkModeToggleBackground, darkModeToggleKnob, isDarkModeEnabled);

            SetLinkState(privacyButton, privacyUrl);
            SetLinkState(termsButton, termsUrl);
            SetLinkState(faqButton, faqUrl);
            SetLinkState(telegramButton, telegramUrl);
            SetLinkState(twitterButton, twitterUrl);

            ApplyMenuThemeOverrides();
        }

        private void UpdateToggleVisual(Image background, RectTransform knob, bool isEnabled)
        {
            ThemeManager.ThemePalette palette = ThemeManager.CurrentPalette;
            Color enabledColor = palette.IsDarkMode ? palette.AccentColor : toggleEnabledColor;
            Color disabledColor = palette.IsDarkMode ? new Color(0.3f, 0.37f, 0.47f, 1f) : toggleDisabledColor;

            if (background != null)
                background.color = isEnabled ? enabledColor : disabledColor;

            if (knob != null)
            {
                knob.anchoredPosition = isEnabled ? toggleKnobOnPosition : toggleKnobOffPosition;
                Image knobImage = knob.GetComponent<Image>();
                if (knobImage != null)
                    knobImage.color = Color.white;
            }
        }

        private void ApplyDarkModeState(bool isDarkModeEnabled)
        {
            ThemeManager.ApplyThemeToScene(gameObject.scene);
        }

        private void ApplyMenuThemeOverrides()
        {
            ThemeManager.ThemePalette palette = ThemeManager.CurrentPalette;
            if (!palette.IsDarkMode)
                return;

            Color menuCardColor = palette.IsDarkMode
                ? new Color(0.14f, 0.18f, 0.24f, 0.98f)
                : new Color(0.31f, 0.33f, 0.48f, 0.96f);
            Color settingsCardColor = palette.IsDarkMode
                ? new Color(0.14f, 0.18f, 0.24f, 0.98f)
                : new Color(0.2f, 0.23f, 0.41f, 0.96f);
            Color popupCardColor = palette.IsDarkMode
                ? new Color(0.16f, 0.2f, 0.27f, 0.98f)
                : Color.white;
            Color popupOverlayColor = palette.IsDarkMode
                ? new Color(0.03f, 0.05f, 0.08f, 0.82f)
                : new Color(0.04f, 0.05f, 0.08f, 0.72f);
            Color primaryButtonColor = palette.IsDarkMode
                ? new Color(0.38f, 0.56f, 1f, 1f)
                : selectedTabColor;
            Color secondaryButtonColor = palette.IsDarkMode
                ? new Color(0.33f, 0.48f, 0.92f, 1f)
                : new Color(0.35f, 0.43f, 0.98f, 1f);
            Color linkButtonColor = palette.IsDarkMode
                ? new Color(0.2f, 0.25f, 0.33f, 1f)
                : new Color(0.25f, 0.31f, 0.49f, 1f);
            Color disabledButtonColor = palette.IsDarkMode
                ? new Color(0.16f, 0.2f, 0.28f, 1f)
                : new Color(0.28f, 0.32f, 0.52f, 0.96f);
            Color buttonTextColor = Color.white;
            Color disabledTextColor = palette.IsDarkMode
                ? new Color(0.77f, 0.83f, 0.92f, 1f)
                : new Color(0.93f, 0.95f, 1f, 1f);

            SetImageColor(settingsPanel != null ? settingsPanel.GetComponent<Image>() : null, settingsCardColor);
            SetImageColor(FindAncestorImage(vibrationToggleButton), settingsCardColor);
            SetImageColor(FindAncestorImage(privacyButton), settingsCardColor);
            SetImageColor(FindAncestorImage(challengeTitleText), menuCardColor);
            SetImageColor(FindAncestorImage(streakHeadlineText), popupCardColor);
            SetImageColor(streakPanel != null ? streakPanel.GetComponent<Image>() : null, popupOverlayColor);

            ThemeManager.ApplyButtonTheme(primaryPlayButton, primaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(cardPlayButton, primaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(challengePlayButton, primaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(streakButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(closeStreakButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(privacyButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(termsButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(faqButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(telegramButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(twitterButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
        }

        private static void SetImageColor(Image image, Color color)
        {
            if (image != null)
                image.color = color;
        }

        private static Image FindAncestorImage(Component component)
        {
            if (component == null)
                return null;

            Transform current = component.transform.parent;
            while (current != null)
            {
                Image image = current.GetComponent<Image>();
                if (image != null)
                    return image;

                current = current.parent;
            }

            return null;
        }

        private static void ApplyImageColors(Image[] images, Color color)
        {
            if (images == null)
                return;

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null)
                    images[i].color = color;
            }
        }

        private static void ApplyTextColors(TextMeshProUGUI[] texts, Color color)
        {
            if (texts == null)
                return;

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                    texts[i].color = color;
            }
        }

        private static void SetLinkState(Button button, string url)
        {
            if (button != null)
                button.interactable = !string.IsNullOrWhiteSpace(url);
        }

        private static TMP_InputField FindFirstInputField(GameObject root, TMP_InputField current)
        {
            if (current != null)
                return current;

            return root != null ? root.GetComponentInChildren<TMP_InputField>(true) : null;
        }

        private static Button FindButtonByKeywords(GameObject root, Button current, params string[] keywords)
        {
            if (root == null)
                return current;

            if (ButtonMatches(current, keywords))
                return current;

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (ButtonMatches(buttons[i], keywords))
                    return buttons[i];
            }

            return current;
        }

        private static bool ButtonMatches(Button button, params string[] keywords)
        {
            if (button == null || keywords == null || keywords.Length == 0)
                return false;

            string searchText = BuildSearchText(button);
            for (int i = 0; i < keywords.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(keywords[i]) && searchText.Contains(keywords[i].ToLowerInvariant()))
                    return true;
            }

            return false;
        }

        private static string BuildSearchText(Component component)
        {
            if (component == null)
                return string.Empty;

            string searchText = component.name.ToLowerInvariant();
            TextMeshProUGUI[] labels = component.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] != null && !string.IsNullOrWhiteSpace(labels[i].text))
                    searchText += " " + labels[i].text.ToLowerInvariant();
            }

            return searchText;
        }

        private static Image FindButtonBackground(Button button, Image fallback)
        {
            if (button == null)
                return fallback;

            if (button.targetGraphic is Image targetImage)
                return targetImage;

            Image image = button.GetComponent<Image>();
            return image != null ? image : fallback;
        }

        private static RectTransform FindToggleKnob(Button button, RectTransform fallback)
        {
            if (button == null)
                return fallback;

            Image[] images = button.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].transform != button.transform)
                    return images[i].rectTransform;
            }

            return fallback;
        }

        private static void AddIfNotNull<T>(List<T> list, T value) where T : class
        {
            if (value != null && !list.Contains(value))
                list.Add(value);
        }

        private void Update()
        {
            if (Time.unscaledTime < nextChallengeUiRefreshTime)
                return;

            nextChallengeUiRefreshTime = Time.unscaledTime + ChallengeUiRefreshInterval;
            RefreshChallengeUi();
        }

        private void RefreshChallengeUi()
        {
            DateTime nowUtc = DateTime.UtcNow;
            int chancesRemaining = GameDataStore.GetChallengeChancesRemainingToday(nowUtc);
            int playedDayCount = GameDataStore.GetPlayedChallengeDayCount(nowUtc);
            int currentDayIndex = GameDataStore.GetCurrentChallengeDayIndex(nowUtc);
            int streakMask = GameDataStore.GetChallengeStreakMask(nowUtc);
            int cycleIndex = GameDataStore.GetCurrentChallengeCycleIndex(nowUtc);
            int patternIndex = GameDataStore.GetCurrentChallengePatternIndex(nowUtc, challengePatternNames.Length);
            string patternName = challengePatternNames != null && challengePatternNames.Length > 0
                ? challengePatternNames[Mathf.Clamp(patternIndex, 0, challengePatternNames.Length - 1)]
                : $"Pattern {cycleIndex + 1}";

            if (challengeTitleText != null)
                challengeTitleText.text = $"{challengeTitlePrefix} #{cycleIndex + 1}";

            if (challengePatternText != null)
                challengePatternText.text = patternName;

            if (challengeCycleTimerText != null)
                challengeCycleTimerText.text = FormatCountdown(GameDataStore.GetCurrentChallengeTimeRemaining(nowUtc));

            if (challengeChanceText != null)
                challengeChanceText.text = chancesRemaining > 0 ? $"{chancesRemaining} chance left" : "0 chances left";

            if (challengeNextChanceTimerText != null)
            {
                challengeNextChanceTimerText.text = chancesRemaining > 0
                    ? "Chance Ready"
                    : FormatCountdown(GameDataStore.GetTimeUntilNextChallengeChance(nowUtc));
            }

            if (challengeStatusText != null)
            {
                challengeStatusText.text = chancesRemaining > 0
                    ? "You have 1 chance available today."
                    : "Today's challenge chance is used. Come back when the timer resets.";
            }

            if (challengePlayButton != null)
                challengePlayButton.interactable = chancesRemaining > 0;

            if (streakHeadlineText != null)
                streakHeadlineText.text = $"{playedDayCount} day streak";

            if (streakSummaryText != null)
            {
                streakSummaryText.text = playedDayCount > 0
                    ? "Keep clearing the challenge every day to grow your streak!"
                    : "Win a level to start your streak!";
            }

            if (streakDayViews == null)
                return;

            for (int i = 0; i < streakDayViews.Length; i++)
            {
                bool isPlayed = (streakMask & (1 << i)) != 0;
                bool isCurrentDay = i == currentDayIndex;
                bool isMissed = i < currentDayIndex && !isPlayed;

                if (streakDayViews[i] != null)
                    streakDayViews[i].Bind(i + 1, isPlayed, isCurrentDay, isMissed);
            }

            ApplyMenuThemeOverrides();
        }

        private static string FormatCountdown(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.Zero)
                timeSpan = TimeSpan.Zero;

            int totalDays = Mathf.Max(0, timeSpan.Days);
            return $"{totalDays:00}d {timeSpan.Hours:00}h {timeSpan.Minutes:00}m {timeSpan.Seconds:00}s";
        }
    }
}
