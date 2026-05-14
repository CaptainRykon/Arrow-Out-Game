using System;
using System.Collections;
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
        [SerializeField] private Button shopButton;
        [SerializeField] private Button challengePlayButton;
        [SerializeField] private TextMeshProUGUI currentLevelLabel;

        [Header("Shop UI")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button closeShopButton;
        [SerializeField] private Button hintBuyButton;
        [SerializeField] private Button livesBuyButton;
        [SerializeField] private Image shopCardBackground;
        [SerializeField] private Image shopHeaderBackground;
        [SerializeField] private Image hintOfferBackground;
        [SerializeField] private Image livesOfferBackground;
        [SerializeField] private Image hintIconBackground;
        [SerializeField] private Image livesIconBackground;
        [SerializeField] private TextMeshProUGUI shopTitleText;
        [SerializeField] private TextMeshProUGUI hintAmountText;
        [SerializeField] private TextMeshProUGUI hintPriceText;
        [SerializeField] private TextMeshProUGUI livesAmountText;
        [SerializeField] private TextMeshProUGUI livesPriceText;
        [SerializeField] private string hintPackageAmountLabel = "10 Hints";
        [SerializeField] private string hintPackagePriceLabel = "$0.99";
        [SerializeField] private string livesPackageAmountLabel = "3 Lives";
        [SerializeField] private string livesPackagePriceLabel = "$1.99";
        [SerializeField] private int hintPackageAmount = 10;
        [SerializeField] private int livesPackageAmount = 3;

        [Header("Purchase Gate")]
        [SerializeField] private GameObject purchaseGatePanel;
        [SerializeField] private Button purchaseGatePayButton;
        [SerializeField] private TextMeshProUGUI purchaseGateTitleText;
        [SerializeField] private TextMeshProUGUI purchaseGateBodyText;
        [SerializeField] private TextMeshProUGUI purchaseGatePriceText;
        [SerializeField] private TextMeshProUGUI purchaseGateStatusText;
        [SerializeField] private string purchaseGateTitleLabel = "Unlock Arrow Out";
        [SerializeField] private string purchaseGateBodyLabel = "Finish the tutorial, then pay once in MiniPay to unlock the full game.";
        [SerializeField] private string purchaseGatePriceLabel = "$0.50";
        [SerializeField] private string purchaseGateIdleStatusLabel = "Complete the payment to continue into the main game.";
        [SerializeField] private float bridgeStartupWaitSeconds = 5f;

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
        private Coroutine entryFlowCoroutine;
        private bool purchaseGateBusy;
        private string purchaseGateStatusOverride;
        private static Sprite runtimeSprite;

        private void OnValidate()
        {
            TryAssignSettingsReferencesFromPanel();
            TryAssignShopReferences();
            TryAssignPurchaseGateReferences();
        }

        private void Awake()
        {
            EnsurePurchaseGateUi();
            TryAssignSettingsReferencesFromPanel();
            TryAssignShopReferences();
            TryAssignPurchaseGateReferences();
            WireButtons();
            WireSettingsControls();
            CloseStreakPanel();
            CloseShopPanel();
            ClosePurchaseGatePanel();
            RefreshLevelLabel();
            RefreshSettingsUi();
            RefreshShopUi();
            RefreshPurchaseGateUi();
            ShowHome();
        }

        private void OnEnable()
        {
            EnsurePurchaseGateUi();
            TryAssignSettingsReferencesFromPanel();
            TryAssignShopReferences();
            TryAssignPurchaseGateReferences();
            WireSettingsControls();
            SubscribeMiniPayEvents();
            nextChallengeUiRefreshTime = 0f;
            RefreshLevelLabel();
            RefreshChallengeUi();
            RefreshSettingsUi();
            RefreshShopUi();
            RefreshPurchaseGateUi();
        }

        private void Start()
        {
            BeginEntryFlowResolution();
        }

        private void OnDisable()
        {
            UnsubscribeMiniPayEvents();
        }

        private void OnDestroy()
        {
            if (userNameInputField != null)
                userNameInputField.onEndEdit.RemoveListener(HandleUserNameChanged);

            if (entryFlowCoroutine != null)
                StopCoroutine(entryFlowCoroutine);
        }

        public void ShowHome()
        {
            SetTabState(true, false, false);
        }

        public void ShowCollection()
        {
            CloseStreakPanel();
            CloseShopPanel();
            SetTabState(false, true, false);
        }

        public void ShowSettings()
        {
            CloseStreakPanel();
            CloseShopPanel();
            RefreshSettingsUi();
            SetTabState(false, false, true);
        }

        public void PlayGame()
        {
            SoundManager.PlayButtonClick();
            HapticManager.PlayButtonTap();

            if (ShouldShowPurchaseGate())
            {
                OpenPurchaseGatePanel();
                return;
            }

            SceneManager.LoadScene(GameDataStore.HasCompletedTutorial ? GameSceneName : TutorialSceneName);
        }

        public void PlayChallenge()
        {
            SoundManager.PlayButtonClick();
            HapticManager.PlayButtonTap();

            if (ShouldShowPurchaseGate())
            {
                OpenPurchaseGatePanel();
                return;
            }

            if (!GameDataStore.CanPlayChallengeToday(DateTime.UtcNow))
            {
                RefreshChallengeUi();
                return;
            }

            SceneManager.LoadScene(ChallengeSceneName);
        }

        public void OpenShopPanel()
        {
            if (ShouldShowPurchaseGate())
            {
                OpenPurchaseGatePanel();
                return;
            }

            RefreshShopUi();
            if (shopPanel != null)
                shopPanel.SetActive(true);
        }

        public void CloseShopPanel()
        {
            if (shopPanel != null)
                shopPanel.SetActive(false);
        }

        public void BuyHintsPackage()
        {
            purchaseGateStatusOverride = "Opening MiniPay for your hint purchase...";
            RefreshPurchaseGateUi();
            MiniPayBridge.Instance.BuyHints(hintPackageAmount);
        }

        public void BuyLivesPackage()
        {
            purchaseGateStatusOverride = "Opening MiniPay for your lives purchase...";
            RefreshPurchaseGateUi();
            MiniPayBridge.Instance.BuyLives(livesPackageAmount);
        }

        public void PayForGame()
        {
            purchaseGateBusy = true;
            purchaseGateStatusOverride = "Waiting for MiniPay confirmation...";
            RefreshPurchaseGateUi();
            MiniPayBridge.Instance.PurchaseGame();
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
            ButtonBindingUtility.Bind(homeTabButton, ShowHome);
            ButtonBindingUtility.Bind(collectionTabButton, ShowCollection);
            ButtonBindingUtility.Bind(settingsTabButton, ShowSettings);
            ButtonBindingUtility.Bind(primaryPlayButton, PlayGame);
            ButtonBindingUtility.Bind(cardPlayButton, PlayGame);
            ButtonBindingUtility.Bind(shopButton, OpenShopPanel);
            ButtonBindingUtility.Bind(challengePlayButton, PlayChallenge);
            ButtonBindingUtility.Bind(streakButton, OpenStreakPanel);
            ButtonBindingUtility.Bind(closeStreakButton, CloseStreakPanel);
            ButtonBindingUtility.Bind(closeShopButton, CloseShopPanel);
            ButtonBindingUtility.Bind(hintBuyButton, BuyHintsPackage);
            ButtonBindingUtility.Bind(livesBuyButton, BuyLivesPackage);
            ButtonBindingUtility.Bind(purchaseGatePayButton, PayForGame);
        }

        private void WireSettingsControls()
        {
            TryAssignSettingsReferencesFromPanel();
            ButtonBindingUtility.Bind(vibrationToggleButton, ToggleVibration);
            ButtonBindingUtility.Bind(soundToggleButton, ToggleSound);
            ButtonBindingUtility.Bind(darkModeToggleButton, ToggleDarkMode);
            ButtonBindingUtility.Bind(privacyButton, OpenPrivacy);
            ButtonBindingUtility.Bind(termsButton, OpenTerms);
            ButtonBindingUtility.Bind(faqButton, OpenFaq);
            ButtonBindingUtility.Bind(telegramButton, OpenTelegram);
            ButtonBindingUtility.Bind(twitterButton, OpenTwitter);

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

        private void TryAssignShopReferences()
        {
            if (homePanel == null)
                return;

            shopButton = FindNamedButton(homePanel.transform, "Shop Button", shopButton);

            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : null;
            shopPanel = FindNamedObject(canvasTransform, "Shop Panel", shopPanel);
            closeShopButton = FindNamedButton(canvasTransform, "Shop Close Button", closeShopButton);
            hintBuyButton = FindNamedButton(canvasTransform, "Hint Buy Button", hintBuyButton);
            livesBuyButton = FindNamedButton(canvasTransform, "Lives Buy Button", livesBuyButton);
            shopCardBackground = FindNamedImage(canvasTransform, "Shop Card", shopCardBackground);
            shopHeaderBackground = FindNamedImage(canvasTransform, "Shop Header", shopHeaderBackground);
            hintOfferBackground = FindNamedImage(canvasTransform, "Hint Offer Card", hintOfferBackground);
            livesOfferBackground = FindNamedImage(canvasTransform, "Lives Offer Card", livesOfferBackground);
            hintIconBackground = FindNamedImage(canvasTransform, "Hint Icon", hintIconBackground);
            livesIconBackground = FindNamedImage(canvasTransform, "Lives Icon", livesIconBackground);
            shopTitleText = FindNamedText(canvasTransform, "Shop Title", shopTitleText);
            hintAmountText = FindNamedText(canvasTransform, "Hint Amount Text", hintAmountText);
            hintPriceText = FindNamedText(canvasTransform, "Hint Price Text", hintPriceText);
            livesAmountText = FindNamedText(canvasTransform, "Lives Amount Text", livesAmountText);
            livesPriceText = FindNamedText(canvasTransform, "Lives Price Text", livesPriceText);
        }

        private void TryAssignPurchaseGateReferences()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : null;
            purchaseGatePanel = FindNamedObject(canvasTransform, "Purchase Gate Panel", purchaseGatePanel);
            purchaseGatePayButton = FindNamedButton(canvasTransform, "Purchase Gate Pay Button", purchaseGatePayButton);
            purchaseGateTitleText = FindNamedText(canvasTransform, "Purchase Gate Title", purchaseGateTitleText);
            purchaseGateBodyText = FindNamedText(canvasTransform, "Purchase Gate Body", purchaseGateBodyText);
            purchaseGatePriceText = FindNamedText(canvasTransform, "Purchase Gate Price Text", purchaseGatePriceText);
            purchaseGateStatusText = FindNamedText(canvasTransform, "Purchase Gate Status Text", purchaseGateStatusText);
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

        private void RefreshShopUi()
        {
            SetTextIfBlank(shopTitleText, "SHOP");
            SetTextIfBlank(hintAmountText, hintPackageAmountLabel);
            SetTextIfBlank(hintPriceText, hintPackagePriceLabel);
            SetTextIfBlank(livesAmountText, livesPackageAmountLabel);
            SetTextIfBlank(livesPriceText, livesPackagePriceLabel);
        }

        private void RefreshPurchaseGateUi()
        {
            if (purchaseGateTitleText != null)
                purchaseGateTitleText.text = purchaseGateTitleLabel;

            if (purchaseGateBodyText != null)
                purchaseGateBodyText.text = purchaseGateBodyLabel;

            if (purchaseGatePriceText != null)
                purchaseGatePriceText.text = purchaseGatePriceLabel;

            if (purchaseGateStatusText != null)
            {
                if (GameDataStore.HasPurchasedGame)
                    purchaseGateStatusText.text = "Purchase complete. Welcome back.";
                else if (!string.IsNullOrWhiteSpace(purchaseGateStatusOverride))
                    purchaseGateStatusText.text = purchaseGateStatusOverride;
                else
                    purchaseGateStatusText.text = purchaseGateIdleStatusLabel;
            }

            if (purchaseGatePayButton != null)
                purchaseGatePayButton.interactable = !purchaseGateBusy && !GameDataStore.HasPurchasedGame;
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
            ThemeManager.ApplyButtonTheme(shopButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(challengePlayButton, primaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(streakButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(closeStreakButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(privacyButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(termsButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(faqButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(telegramButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(twitterButton, linkButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);

            Color shopCardColor = palette.IsDarkMode
                ? new Color(0.18f, 0.2f, 0.26f, 0.98f)
                : new Color(0.98f, 0.91f, 0.69f, 1f);
            Color shopHeaderColor = palette.IsDarkMode
                ? new Color(0.78f, 0.54f, 0.18f, 1f)
                : new Color(0.95f, 0.68f, 0.15f, 1f);
            Color shopOfferColor = palette.IsDarkMode
                ? new Color(0.25f, 0.28f, 0.34f, 0.98f)
                : new Color(1f, 0.96f, 0.8f, 1f);
            Color shopIconColor = palette.IsDarkMode
                ? new Color(0.23f, 0.69f, 0.84f, 1f)
                : new Color(0.36f, 0.86f, 0.92f, 1f);
            Color shopBuyButtonColor = palette.IsDarkMode
                ? new Color(0.9f, 0.3f, 0.58f, 1f)
                : new Color(0.95f, 0.23f, 0.61f, 1f);

            SetImageColor(shopPanel != null ? shopPanel.GetComponent<Image>() : null, popupOverlayColor);
            SetImageColor(shopCardBackground, shopCardColor);
            SetImageColor(shopHeaderBackground, shopHeaderColor);
            SetImageColor(hintOfferBackground, shopOfferColor);
            SetImageColor(livesOfferBackground, shopOfferColor);
            SetImageColor(hintIconBackground, shopIconColor);
            SetImageColor(livesIconBackground, shopIconColor);

            ThemeManager.ApplyButtonTheme(closeShopButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(hintBuyButton, shopBuyButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(livesBuyButton, shopBuyButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);

            if (purchaseGatePanel != null)
                SetImageColor(purchaseGatePanel.GetComponent<Image>(), popupOverlayColor);

            Image purchaseGateCard = FindNamedImage(purchaseGatePanel != null ? purchaseGatePanel.transform : null, "Purchase Gate Card", null);
            Image purchasePriceBadge = FindNamedImage(purchaseGatePanel != null ? purchaseGatePanel.transform : null, "Purchase Gate Price Badge", null);
            SetImageColor(purchaseGateCard, popupCardColor);
            SetImageColor(purchasePriceBadge, shopHeaderColor);
            ThemeManager.ApplyButtonTheme(purchaseGatePayButton, primaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
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

        private static void SetTextIfBlank(TMP_Text textComponent, string fallbackText)
        {
            if (textComponent == null || !string.IsNullOrWhiteSpace(textComponent.text))
                return;

            textComponent.text = fallbackText;
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

        private static Button FindNamedButton(Transform root, string name, Button fallback)
        {
            Transform child = FindDescendantByName(root, name);
            return child != null ? child.GetComponent<Button>() : fallback;
        }

        private static Image FindNamedImage(Transform root, string name, Image fallback)
        {
            Transform child = FindDescendantByName(root, name);
            return child != null ? child.GetComponent<Image>() : fallback;
        }

        private static TextMeshProUGUI FindNamedText(Transform root, string name, TextMeshProUGUI fallback)
        {
            Transform child = FindDescendantByName(root, name);
            return child != null ? child.GetComponent<TextMeshProUGUI>() : fallback;
        }

        private static GameObject FindNamedObject(Transform root, string name, GameObject fallback)
        {
            Transform child = FindDescendantByName(root, name);
            return child != null ? child.gameObject : fallback;
        }

        private static Transform FindDescendantByName(Transform root, string name)
        {
            if (root == null || string.IsNullOrWhiteSpace(name))
                return null;

            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDescendantByName(root.GetChild(i), name);
                if (match != null)
                    return match;
            }

            return null;
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

        private void BeginEntryFlowResolution()
        {
            if (entryFlowCoroutine != null)
                StopCoroutine(entryFlowCoroutine);

            entryFlowCoroutine = StartCoroutine(ResolveEntryFlowCO());
        }

        private IEnumerator ResolveEntryFlowCO()
        {
            MiniPayBridge bridge = MiniPayBridge.Instance;
            float timeoutAt = Time.realtimeSinceStartup + Mathf.Max(0.5f, bridgeStartupWaitSeconds);
            while (bridge != null && !bridge.HasResolvedInitialState && Time.realtimeSinceStartup < timeoutAt)
                yield return null;

            entryFlowCoroutine = null;
            RefreshEntryFlowState();
        }

        private void RefreshEntryFlowState()
        {
            RefreshLevelLabel();
            RefreshChallengeUi();
            RefreshSettingsUi();
            RefreshShopUi();
            RefreshPurchaseGateUi();

            if (GameDataStore.HasPurchasedGame)
            {
                ClosePurchaseGatePanel();
                return;
            }

            if (!GameDataStore.HasCompletedTutorial)
            {
                SceneManager.LoadScene(TutorialSceneName);
                return;
            }

            OpenPurchaseGatePanel();
        }

        private bool ShouldShowPurchaseGate()
        {
            return GameDataStore.HasCompletedTutorial && !GameDataStore.HasPurchasedGame;
        }

        private void OpenPurchaseGatePanel()
        {
            purchaseGateStatusOverride = purchaseGateBusy
                ? purchaseGateStatusOverride
                : string.IsNullOrWhiteSpace(purchaseGateStatusOverride) ? null : purchaseGateStatusOverride;
            RefreshPurchaseGateUi();
            if (purchaseGatePanel != null)
                purchaseGatePanel.SetActive(true);
        }

        private void ClosePurchaseGatePanel()
        {
            if (purchaseGatePanel != null)
                purchaseGatePanel.SetActive(false);
        }

        private void SubscribeMiniPayEvents()
        {
            GameDataStore.DataChanged -= HandleGameDataChanged;
            GameDataStore.DataChanged += HandleGameDataChanged;
            MiniPayBridge.InitialStateResolved -= HandleInitialStateResolved;
            MiniPayBridge.InitialStateResolved += HandleInitialStateResolved;
            MiniPayBridge.GamePurchaseSucceeded -= HandleGamePurchaseSucceeded;
            MiniPayBridge.GamePurchaseSucceeded += HandleGamePurchaseSucceeded;
            MiniPayBridge.GamePurchaseFailed -= HandleGamePurchaseFailed;
            MiniPayBridge.GamePurchaseFailed += HandleGamePurchaseFailed;
            MiniPayBridge.HintPurchaseSucceeded -= HandleConsumablePurchaseSucceeded;
            MiniPayBridge.HintPurchaseSucceeded += HandleConsumablePurchaseSucceeded;
            MiniPayBridge.LivesPurchaseSucceeded -= HandleConsumablePurchaseSucceeded;
            MiniPayBridge.LivesPurchaseSucceeded += HandleConsumablePurchaseSucceeded;
            MiniPayBridge.HintPurchaseFailed -= HandleConsumablePurchaseFailed;
            MiniPayBridge.HintPurchaseFailed += HandleConsumablePurchaseFailed;
            MiniPayBridge.LivesPurchaseFailed -= HandleConsumablePurchaseFailed;
            MiniPayBridge.LivesPurchaseFailed += HandleConsumablePurchaseFailed;
        }

        private void UnsubscribeMiniPayEvents()
        {
            GameDataStore.DataChanged -= HandleGameDataChanged;
            MiniPayBridge.InitialStateResolved -= HandleInitialStateResolved;
            MiniPayBridge.GamePurchaseSucceeded -= HandleGamePurchaseSucceeded;
            MiniPayBridge.GamePurchaseFailed -= HandleGamePurchaseFailed;
            MiniPayBridge.HintPurchaseSucceeded -= HandleConsumablePurchaseSucceeded;
            MiniPayBridge.LivesPurchaseSucceeded -= HandleConsumablePurchaseSucceeded;
            MiniPayBridge.HintPurchaseFailed -= HandleConsumablePurchaseFailed;
            MiniPayBridge.LivesPurchaseFailed -= HandleConsumablePurchaseFailed;
        }

        private void HandleInitialStateResolved()
        {
            RefreshEntryFlowState();
        }

        private void HandleGameDataChanged()
        {
            RefreshLevelLabel();
            RefreshChallengeUi();
            RefreshSettingsUi();
            RefreshPurchaseGateUi();

            if (GameDataStore.HasPurchasedGame)
                ClosePurchaseGatePanel();
        }

        private void HandleGamePurchaseSucceeded()
        {
            purchaseGateBusy = false;
            purchaseGateStatusOverride = "Purchase complete. Welcome to Arrow Out.";
            RefreshPurchaseGateUi();
            ClosePurchaseGatePanel();
        }

        private void HandleGamePurchaseFailed(string errorMessage)
        {
            purchaseGateBusy = false;
            purchaseGateStatusOverride = string.IsNullOrWhiteSpace(errorMessage) ? "Payment failed. Please try again." : errorMessage;
            RefreshPurchaseGateUi();
            OpenPurchaseGatePanel();
        }

        private void HandleConsumablePurchaseSucceeded()
        {
            purchaseGateStatusOverride = "Purchase complete.";
            RefreshPurchaseGateUi();
        }

        private void HandleConsumablePurchaseFailed(string errorMessage)
        {
            purchaseGateStatusOverride = string.IsNullOrWhiteSpace(errorMessage) ? "Purchase failed. Please try again." : errorMessage;
            RefreshPurchaseGateUi();
        }

        private void EnsurePurchaseGateUi()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            if (FindDescendantByName(canvas.transform, "Purchase Gate Panel") != null)
                return;

            RectTransform overlay = CreateRect("Purchase Gate Panel", canvas.transform);
            StretchRect(overlay);
            EnsureImage(overlay.gameObject, new Color(0.04f, 0.05f, 0.08f, 0.8f));
            overlay.gameObject.SetActive(false);

            RectTransform card = CreateRect("Purchase Gate Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(720f, 620f);
            card.anchoredPosition = Vector2.zero;
            EnsureImage(card.gameObject, Color.white);

            RectTransform titleRect = CreateRect("Purchase Gate Title", card);
            titleRect.anchorMin = new Vector2(0.1f, 1f);
            titleRect.anchorMax = new Vector2(0.9f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 96f);
            titleRect.anchoredPosition = new Vector2(0f, -42f);
            CreateRectLabel(titleRect, purchaseGateTitleLabel, 46f, new Color(0.16f, 0.18f, 0.3f, 1f), TextAlignmentOptions.Center);

            RectTransform bodyRect = CreateRect("Purchase Gate Body", card);
            bodyRect.anchorMin = new Vector2(0.12f, 0.46f);
            bodyRect.anchorMax = new Vector2(0.88f, 0.74f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;
            CreateRectLabel(bodyRect, purchaseGateBodyLabel, 29f, new Color(0.28f, 0.31f, 0.48f, 1f), TextAlignmentOptions.Center);

            RectTransform priceBadge = CreateRect("Purchase Gate Price Badge", card);
            priceBadge.anchorMin = new Vector2(0.5f, 0.5f);
            priceBadge.anchorMax = new Vector2(0.5f, 0.5f);
            priceBadge.pivot = new Vector2(0.5f, 0.5f);
            priceBadge.sizeDelta = new Vector2(260f, 100f);
            priceBadge.anchoredPosition = new Vector2(0f, 16f);
            EnsureImage(priceBadge.gameObject, new Color(0.95f, 0.68f, 0.15f, 1f));

            RectTransform priceTextRect = CreateRect("Purchase Gate Price Text", priceBadge);
            StretchRect(priceTextRect);
            CreateRectLabel(priceTextRect, purchaseGatePriceLabel, 42f, Color.white, TextAlignmentOptions.Center);

            RectTransform statusRect = CreateRect("Purchase Gate Status Text", card);
            statusRect.anchorMin = new Vector2(0.12f, 0.22f);
            statusRect.anchorMax = new Vector2(0.88f, 0.34f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            CreateRectLabel(statusRect, purchaseGateIdleStatusLabel, 24f, new Color(0.35f, 0.38f, 0.53f, 1f), TextAlignmentOptions.Center);

            RectTransform buttonRect = CreateRect("Purchase Gate Pay Button", card);
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.sizeDelta = new Vector2(340f, 96f);
            buttonRect.anchoredPosition = new Vector2(0f, 42f);
            Image buttonImage = EnsureImage(buttonRect.gameObject, new Color(0.35f, 0.43f, 0.98f, 1f));
            Button payButton = buttonRect.gameObject.AddComponent<Button>();
            payButton.targetGraphic = buttonImage;
            RectTransform buttonLabelRect = CreateRect("Label", buttonRect);
            StretchRect(buttonLabelRect);
            CreateRectLabel(buttonLabelRect, "Pay", 34f, Color.white, TextAlignmentOptions.Center);
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Image EnsureImage(GameObject gameObject, Color color)
        {
            Image image = gameObject.GetComponent<Image>();
            if (image == null)
                image = gameObject.AddComponent<Image>();

            image.color = color;
            if (image.sprite == null)
                image.sprite = GetRuntimeSprite();
            return image;
        }

        private static TextMeshProUGUI CreateRectLabel(RectTransform rect, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;
            return label;
        }

        private static Sprite GetRuntimeSprite()
        {
            if (runtimeSprite != null)
                return runtimeSprite;

            Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp;
            runtimeSprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
            runtimeSprite.name = "MenuSceneRuntimeSprite";
            return runtimeSprite;
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
