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
        private const int LeaderboardEntryLimit = 25;
        private static readonly string[] DefaultChallengePatternNames = { "Star", "Duck", "Bolt", "Crown", "Leaf", "Rocket", "Moon" };

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
        [SerializeField] private Button leaderboardButton;
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
        [SerializeField] private string hintPackageAmountLabel = "5 Hints";
        [SerializeField] private string hintPackagePriceLabel = "$0.10";
        [SerializeField] private string livesPackageAmountLabel = "3 Revives";
        [SerializeField] private string livesPackagePriceLabel = "$1.99";
        [SerializeField] private int hintPackageAmount = 5;
        [SerializeField] private int livesPackageAmount = 3;

        [Header("Purchase Gate")]
        [SerializeField] private GameObject purchaseGatePanel;
        [SerializeField] private Button purchaseGatePayButton;
        [SerializeField] private TextMeshProUGUI purchaseGateTitleText;
        [SerializeField] private TextMeshProUGUI purchaseGateBodyText;
        [SerializeField] private TextMeshProUGUI purchaseGatePriceText;
        [SerializeField] private TextMeshProUGUI purchaseGateStatusText;
        [SerializeField] private string purchaseGateTitleLabel = "Enjoying Arrow Out?";
        [SerializeField] private string purchaseGateBodyLabel = "Unlock the full game with a one-time purchase.Unlimited play 10000+ Levels Leaderboard Rewards";
        [SerializeField] private string purchaseGatePriceLabel = "$0.50";
        [SerializeField] private string purchaseGateIdleStatusLabel = "Complete the payment to continue into the main game.";
        [SerializeField] private float bridgeStartupWaitSeconds = 5f;

        [Header("Purchase Success")]
        [SerializeField] private GameObject purchaseSuccessPanel;
        [SerializeField] private TextMeshProUGUI purchaseSuccessTitleText;
        [SerializeField] private TextMeshProUGUI purchaseSuccessBodyText;
        [SerializeField] private TMP_InputField purchaseSuccessNameInputField;
        [SerializeField] private Button purchaseSuccessOkButton;
        [SerializeField] private TextMeshProUGUI purchaseSuccessStatusText;
        [SerializeField] private string purchaseSuccessTitleLabel = "Payment Successful";
        [SerializeField] private string purchaseSuccessBodyLabel = "Welcome to Arrow Out. Enter your player name to continue.";
        [SerializeField] private string purchaseSuccessStatusLabel = "Your name will be saved to MiniPay.";
        [SerializeField] private int freeUnlockHintRewardCount =3;

        [Header("Unlock Reward")]
        [SerializeField] private GameObject hintRewardPanel;
        [SerializeField] private TextMeshProUGUI hintRewardTitleText;
        [SerializeField] private TextMeshProUGUI hintRewardBodyText;
        [SerializeField] private TextMeshProUGUI hintRewardStatusText;
        [SerializeField] private Button hintRewardOkButton;
        [SerializeField] private string hintRewardTitleLabel = "Free Hints Unlocked";
        [SerializeField] private string hintRewardBodyLabel = "Congrats! You received 3 free hints with your unlock purchase.";
        [SerializeField] private string hintRewardStatusLabel = "These hints are saved to your MiniPay account.";
        [SerializeField] private string hintPurchaseSuccessTitleLabel = "Hints Added";
        [SerializeField] private string hintPurchaseSuccessBodyLabel = "Your hint purchase was successful.";
        [SerializeField] private string hintPurchaseSuccessStatusLabel = "The hints were added to your MiniPay game data.";

        [Header("Payment Failed")]
        [SerializeField] private GameObject paymentFailedPanel;
        [SerializeField] private TextMeshProUGUI paymentFailedTitleText;
        [SerializeField] private TextMeshProUGUI paymentFailedBodyText;
        [SerializeField] private Button paymentFailedRetryButton;
        [SerializeField] private string paymentFailedTitleLabel = "Payment Failed";
        [SerializeField] private string paymentFailedDefaultBodyLabel = "Payment failed. Please try again.";

        [Header("Settings")]
        [SerializeField] private TMP_InputField userNameInputField;
        [SerializeField] private Button userNameEditButton;
        [SerializeField] private GameObject userNameEditPanel;
        [SerializeField] private TMP_InputField userNameEditInputField;
        [SerializeField] private Button userNameEditSaveButton;
        [SerializeField] private Button userNameEditCancelButton;
        [SerializeField] private TextMeshProUGUI userNameEditTitleText;
        [SerializeField] private TextMeshProUGUI userNameEditStatusText;
        [SerializeField] private string userNameEditTitleLabel = "Change Your Name";
        [SerializeField] private string userNameEditStatusLabel = "Enter a new name and save it to your MiniPay profile.";
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
        [SerializeField] private string challengeTitlePrefix = "WEEKLY CHALLANGE";
        [SerializeField] private string[] challengePatternNames = {  };
        [SerializeField] private TextMeshProUGUI challengeTitleText;
        [SerializeField] private TextMeshProUGUI challengePatternText;
        [SerializeField] private TextMeshProUGUI challengeCycleTimerText;
        [SerializeField] private TextMeshProUGUI challengeChanceText;
        [SerializeField] private TextMeshProUGUI challengeNextChanceTimerText;
        [SerializeField] private TextMeshProUGUI challengeStatusText;

        [Header("Menu Leaderboard")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private Button closeLeaderboardButton;
        [SerializeField] private TextMeshProUGUI leaderboardTitleText;
        [SerializeField] private TextMeshProUGUI leaderboardPlayerBestText;
        [SerializeField] private ChallengeLeaderboardEntryView[] leaderboardEntryViews;

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
        private bool entryFlowResolved;
        private bool shouldShowUnlockHintReward;
        private bool showingHintPurchaseSuccess;
        private bool isHomeTabSelected;
        private bool isCollectionTabSelected;
        private bool isSettingsTabSelected;
        private bool isHandlingGameDataChange;
        private bool hasAppliedThemeState;
        private bool lastAppliedDarkModeEnabled;

        private void OnValidate()
        {
            TryAssignSettingsReferencesFromPanel();
            TryAssignShopReferences();
            TryAssignPurchaseGateReferences();
            TryAssignPurchaseSuccessReferences();
            TryAssignHintRewardReferences();
            TryAssignPaymentFailedReferences();
            TryAssignLeaderboardReferences();
        }

        private void Awake()
        {
            TryAssignSettingsReferencesFromPanel();
            TryAssignShopReferences();
            TryAssignPurchaseGateReferences();
            TryAssignPurchaseSuccessReferences();
            TryAssignHintRewardReferences();
            TryAssignPaymentFailedReferences();
            TryAssignLeaderboardReferences();
            WireButtons();
            WireSettingsControls();
            CloseShopPanel();
            ClosePurchaseGatePanel();
            ClosePurchaseSuccessPanel();
            CloseHintRewardPanel();
            ClosePaymentFailedPanel();
            CloseLeaderboardPanel();
            RefreshLevelLabel();
            RefreshSettingsUi();
            RefreshShopUi();
            RefreshPurchaseGateUi();
            RefreshPurchaseSuccessUi();
            RefreshHintRewardUi();
            RefreshPaymentFailedUi();
            RefreshLeaderboardUi();
            SetTabState(false, false, false);
        }

        private void OnEnable()
        {
            TryAssignSettingsReferencesFromPanel();
            TryAssignShopReferences();
            TryAssignPurchaseGateReferences();
            TryAssignPurchaseSuccessReferences();
            TryAssignHintRewardReferences();
            TryAssignPaymentFailedReferences();
            TryAssignLeaderboardReferences();
            WireSettingsControls();
            SubscribeMiniPayEvents();
            nextChallengeUiRefreshTime = 0f;
            RefreshLevelLabel();
            RefreshChallengeUi();
            RefreshSettingsUi();
            RefreshShopUi();
            RefreshPurchaseGateUi();
            RefreshPurchaseSuccessUi();
            RefreshHintRewardUi();
            RefreshPaymentFailedUi();
            RefreshLeaderboardUi();
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
            if (purchaseSuccessNameInputField != null)
                purchaseSuccessNameInputField.onValueChanged.RemoveListener(HandlePurchaseSuccessNameChanged);

            if (userNameEditInputField != null)
                userNameEditInputField.onValueChanged.RemoveListener(HandleUserNameEditChanged);

            if (entryFlowCoroutine != null)
                StopCoroutine(entryFlowCoroutine);
        }

        public void ShowHome()
        {
            if (!CanInteractWithMenu())
                return;

            CloseUserNameEditPanel();
            SetTabState(true, false, false);
        }

        public void ShowCollection()
        {
            if (!CanInteractWithMenu())
                return;

            CloseShopPanel();
            CloseLeaderboardPanel();
            CloseUserNameEditPanel();
            SetTabState(false, true, false);
        }

        public void ShowSettings()
        {
            if (!CanInteractWithMenu())
                return;

            CloseShopPanel();
            CloseLeaderboardPanel();
            CloseUserNameEditPanel();
            RefreshSettingsUi();
            SetTabState(false, false, true);
        }

        public void PlayGame()
        {
            if (!entryFlowResolved)
                return;

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
            if (!entryFlowResolved)
                return;

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
            if (!CanInteractWithMenu() || ShouldShowPurchaseGate())
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

        public void OpenLeaderboardPanel()
        {
            if (!CanInteractWithMenu() || ShouldShowPurchaseGate())
            {
                OpenPurchaseGatePanel();
                return;
            }

            RefreshLeaderboardUi();
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(true);

            NormalizeLeaderboardPanelLayout();
            EnsureLeaderboardRequested(false);
        }

        public void CloseLeaderboardPanel()
        {
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
        }

        public void BuyHintsPackage()
        {
            ClosePaymentFailedPanel();
            purchaseGateStatusOverride = "Opening MiniPay for your hint purchase...";
            RefreshPurchaseGateUi();
            MiniPayBridge.Instance.BuyHints(hintPackageAmount);
        }

        public void PayForGame()
        {
            ClosePaymentFailedPanel();
            purchaseGateBusy = true;
            purchaseGateStatusOverride = "Waiting for MiniPay confirmation...";
            RefreshPurchaseGateUi();
            MiniPayBridge.Instance.PurchaseGame();
        }

        public void ConfirmPurchaseSuccessName()
        {
            if (purchaseSuccessNameInputField == null)
                return;

            string playerName = purchaseSuccessNameInputField.text?.Trim();
            if (string.IsNullOrWhiteSpace(playerName))
                return;

            GameDataStore.PlayerName = playerName;
            ClosePurchaseSuccessPanel();

            if (shouldShowUnlockHintReward)
            {
                shouldShowUnlockHintReward = false;
                OpenHintRewardPanel();
                return;
            }

            ShowHomeAfterEntryResolved();
        }

        public void ConfirmHintReward()
        {
            showingHintPurchaseSuccess = false;
            CloseHintRewardPanel();
            ShowHomeAfterEntryResolved();
        }

        public void RetryFailedPayment()
        {
            ClosePaymentFailedPanel();
            OpenPurchaseGatePanel();
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
            ButtonBindingUtility.Bind(leaderboardButton, OpenLeaderboardPanel);
            ButtonBindingUtility.Bind(challengePlayButton, PlayChallenge);
            ButtonBindingUtility.Bind(closeShopButton, CloseShopPanel);
            ButtonBindingUtility.Bind(hintBuyButton, BuyHintsPackage);
            ButtonBindingUtility.Bind(purchaseGatePayButton, PayForGame);
            ButtonBindingUtility.Bind(paymentFailedRetryButton, RetryFailedPayment);
            ButtonBindingUtility.Bind(closeLeaderboardButton, CloseLeaderboardPanel);
            ButtonBindingUtility.Bind(purchaseSuccessOkButton, ConfirmPurchaseSuccessName);
            ButtonBindingUtility.Bind(hintRewardOkButton, ConfirmHintReward);
            ButtonBindingUtility.Bind(userNameEditButton, OpenUserNameEditPanel);
            ButtonBindingUtility.Bind(userNameEditSaveButton, SaveUserNameEdit);
            ButtonBindingUtility.Bind(userNameEditCancelButton, CloseUserNameEditPanel);
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
                userNameInputField.readOnly = true;

            if (purchaseSuccessNameInputField != null)
            {
                purchaseSuccessNameInputField.onValueChanged.RemoveListener(HandlePurchaseSuccessNameChanged);
                purchaseSuccessNameInputField.onValueChanged.AddListener(HandlePurchaseSuccessNameChanged);
            }

            if (userNameEditInputField != null)
            {
                userNameEditInputField.onValueChanged.RemoveListener(HandleUserNameEditChanged);
                userNameEditInputField.onValueChanged.AddListener(HandleUserNameEditChanged);
            }
        }

        private void TryAssignSettingsReferencesFromPanel()
        {
            if (settingsPanel == null)
                return;

            userNameInputField = FindFirstInputField(settingsPanel, userNameInputField);
            userNameEditButton = FindNamedButton(settingsPanel.transform, "Username Edit Button", userNameEditButton);

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

            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : null;
            userNameEditPanel = FindNamedObject(canvasTransform, "Username Edit Panel", userNameEditPanel);
            userNameEditInputField = FindNamedInputField(canvasTransform, "Username Edit Input Field", userNameEditInputField);
            userNameEditSaveButton = FindNamedButton(canvasTransform, "Username Edit Save Button", userNameEditSaveButton);
            userNameEditCancelButton = FindNamedButton(canvasTransform, "Username Edit Cancel Button", userNameEditCancelButton);
            userNameEditTitleText = FindNamedText(canvasTransform, "Username Edit Title", userNameEditTitleText);
            userNameEditStatusText = FindNamedText(canvasTransform, "Username Edit Status", userNameEditStatusText);

            themeSurfaceImages = BuildSurfaceThemeImages();
            themeAccentImages = BuildAccentThemeImages();
            themePrimaryTexts = settingsPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        }

        private void TryAssignShopReferences()
        {
            if (homePanel == null)
                return;

            shopButton = FindNamedButton(homePanel.transform, "Shop Button", shopButton);
            leaderboardButton = FindNamedButton(homePanel.transform, "Leaderboard Button", leaderboardButton);

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

        private void TryAssignPurchaseSuccessReferences()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : null;
            purchaseSuccessPanel = FindNamedObject(canvasTransform, "Purchase Success Panel", purchaseSuccessPanel);
            purchaseSuccessTitleText = FindNamedText(canvasTransform, "Purchase Success Title", purchaseSuccessTitleText);
            purchaseSuccessBodyText = FindNamedText(canvasTransform, "Purchase Success Body", purchaseSuccessBodyText);
            purchaseSuccessNameInputField = FindNamedInputField(canvasTransform, "Purchase Success Name Input", purchaseSuccessNameInputField);
            purchaseSuccessOkButton = FindNamedButton(canvasTransform, "Purchase Success OK Button", purchaseSuccessOkButton);
            purchaseSuccessStatusText = FindNamedText(canvasTransform, "Purchase Success Status Text", purchaseSuccessStatusText);
        }

        private void TryAssignHintRewardReferences()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : null;
            hintRewardPanel = FindNamedObject(canvasTransform, "Hint Reward Panel", hintRewardPanel);
            hintRewardTitleText = FindNamedText(canvasTransform, "Hint Reward Title", hintRewardTitleText);
            hintRewardBodyText = FindNamedText(canvasTransform, "Hint Reward Body", hintRewardBodyText);
            hintRewardStatusText = FindNamedText(canvasTransform, "Hint Reward Status Text", hintRewardStatusText);
            hintRewardOkButton = FindNamedButton(canvasTransform, "Hint Reward OK Button", hintRewardOkButton);
        }

        private void TryAssignPaymentFailedReferences()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : null;
            paymentFailedPanel = FindNamedObject(canvasTransform, "Payment Failed Panel", paymentFailedPanel);
            paymentFailedTitleText = FindNamedText(canvasTransform, "Payment Failed Title", paymentFailedTitleText);
            paymentFailedBodyText = FindNamedText(canvasTransform, "Payment Failed Body", paymentFailedBodyText);
            paymentFailedRetryButton = FindNamedButton(canvasTransform, "Payment Failed Retry Button", paymentFailedRetryButton);
        }

        private void TryAssignLeaderboardReferences()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform canvasTransform = canvas != null ? canvas.transform : null;
            leaderboardPanel = FindNamedObject(canvasTransform, "Menu Leaderboard Panel", leaderboardPanel);
            closeLeaderboardButton = FindNamedButton(canvasTransform, "Menu Leaderboard Close Button", closeLeaderboardButton);
            leaderboardTitleText = FindNamedText(canvasTransform, "Menu Leaderboard Title", leaderboardTitleText);
            leaderboardPlayerBestText = FindNamedText(canvasTransform, "Menu Leaderboard Best Text", leaderboardPlayerBestText);

            if (leaderboardEntryViews == null || leaderboardEntryViews.Length < LeaderboardEntryLimit)
            {
                Transform listRoot = FindDescendantByName(canvasTransform, "Menu Leaderboard List");
                if (listRoot != null)
                    leaderboardEntryViews = listRoot.GetComponentsInChildren<ChallengeLeaderboardEntryView>(true);
            }

            NormalizeLeaderboardPanelLayout();
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
            if (hintBuyButton != null)
                hintBuyButton.gameObject.SetActive(true);
            if (livesOfferBackground != null)
                livesOfferBackground.gameObject.SetActive(false);

            if (livesBuyButton != null)
                livesBuyButton.gameObject.SetActive(false);

            if (livesAmountText != null)
                livesAmountText.gameObject.SetActive(false);

            if (livesPriceText != null)
                livesPriceText.gameObject.SetActive(false);
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

        private void RefreshPurchaseSuccessUi()
        {
            if (purchaseSuccessTitleText != null)
                purchaseSuccessTitleText.text = purchaseSuccessTitleLabel;

            if (purchaseSuccessBodyText != null)
                purchaseSuccessBodyText.text = purchaseSuccessBodyLabel;

            if (purchaseSuccessStatusText != null)
                purchaseSuccessStatusText.text = purchaseSuccessStatusLabel;

            if (purchaseSuccessOkButton != null)
            {
                string currentValue = purchaseSuccessNameInputField != null
                    ? purchaseSuccessNameInputField.text
                    : string.Empty;

                purchaseSuccessOkButton.interactable = !string.IsNullOrWhiteSpace(currentValue);
            }
        }

        private void RefreshHintRewardUi()
        {
            if (hintRewardTitleText != null)
                hintRewardTitleText.text = showingHintPurchaseSuccess ? hintPurchaseSuccessTitleLabel : hintRewardTitleLabel;

            if (hintRewardBodyText != null)
                hintRewardBodyText.text = showingHintPurchaseSuccess
                    ? hintPurchaseSuccessBodyLabel.Replace("5", hintPackageAmount.ToString())
                    : hintRewardBodyLabel.Replace("5", freeUnlockHintRewardCount.ToString());

            if (hintRewardStatusText != null)
                hintRewardStatusText.text = showingHintPurchaseSuccess
                    ? $"{hintPurchaseSuccessStatusLabel} You now have {GameDataStore.HintCount} hints ready to use."
                    : $"{hintRewardStatusLabel} You now have {GameDataStore.HintCount} hints ready to use.";
        }

        private void RefreshPaymentFailedUi()
        {
            if (paymentFailedTitleText != null)
                paymentFailedTitleText.text = paymentFailedTitleLabel;

            if (paymentFailedBodyText != null && string.IsNullOrWhiteSpace(paymentFailedBodyText.text))
                paymentFailedBodyText.text = paymentFailedDefaultBodyLabel;
        }

        private void RefreshLeaderboardUi()
        {
            DateTime nowUtc = DateTime.UtcNow;
            int cycleIndex = GameDataStore.GetDisplayedChallengeLeaderboardCycleIndex(nowUtc);
            string patternName = GameDataStore.GetDisplayedChallengeLeaderboardPatternName(nowUtc, GetResolvedChallengePatternNames());
            float playerBestTime = GameDataStore.GetChallengeBestTimeSeconds(nowUtc);

            if (leaderboardTitleText != null)
                leaderboardTitleText.text = $"{challengeTitlePrefix} #{cycleIndex + 1} - {patternName}";

            if (leaderboardPlayerBestText != null)
            {
                leaderboardPlayerBestText.text = playerBestTime > 0f
                    ? $"Your Best: {FormatRunTime(playerBestTime)}"
                    : "Your Best: Not set yet";
            }

            int leaderboardViewCount = leaderboardEntryViews != null ? leaderboardEntryViews.Length : 0;
            List<ChallengeLeaderboardEntryData> entries = GameDataStore.GetChallengeLeaderboardEntries(nowUtc, patternName, Mathf.Max(leaderboardViewCount, LeaderboardEntryLimit));
            Debug.Log($"Menu leaderboard rendering {entries.Count} entries across {leaderboardViewCount} row views for cycle {cycleIndex}, pattern '{patternName}'.");
            for (int i = 0; i < leaderboardViewCount; i++)
            {
                ChallengeLeaderboardEntryData entryData = i < entries.Count ? entries[i] : null;
                if (leaderboardEntryViews[i] != null)
                    leaderboardEntryViews[i].Bind(entryData);
            }

            NormalizeLeaderboardPanelLayout();
        }

        private void EnsureLeaderboardRequested(bool forceRefresh)
        {
            DateTime nowUtc = DateTime.UtcNow;
            string patternName = GetCurrentPatternName(nowUtc);
            if (!forceRefresh && GameDataStore.HasChallengeLeaderboardSnapshot(nowUtc, patternName))
                return;

            MiniPayBridge.Instance.RequestChallengeLeaderboard(patternName, LeaderboardEntryLimit);
        }

        private void SetTabState(bool showHome, bool showCollection, bool showSettings)
        {
            isHomeTabSelected = showHome;
            isCollectionTabSelected = showCollection;
            isSettingsTabSelected = showSettings;

            if (homePanel != null)
                homePanel.SetActive(showHome);
            if (collectionPanel != null)
                collectionPanel.SetActive(showCollection);
            if (settingsPanel != null)
                settingsPanel.SetActive(showSettings);

            ApplyCurrentTabVisualState();
        }

        private void ApplyCurrentTabVisualState()
        {
            SetTabVisual(homeTabBackground, homeTabLabel, isHomeTabSelected);
            SetTabVisual(collectionTabBackground, collectionTabLabel, isCollectionTabSelected);
            SetTabVisual(settingsTabBackground, settingsTabLabel, isSettingsTabSelected);
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

        private void OpenUserNameEditPanel()
        {
            if (userNameEditPanel != null)
                userNameEditPanel.SetActive(true);

            if (userNameEditInputField != null)
            {
                string currentName = GameDataStore.PlayerName;
                userNameEditInputField.SetTextWithoutNotify(string.Equals(currentName, "You", StringComparison.OrdinalIgnoreCase) ? string.Empty : currentName);
                userNameEditInputField.ActivateInputField();
            }

            if (userNameEditTitleText != null)
                userNameEditTitleText.text = userNameEditTitleLabel;

            if (userNameEditStatusText != null)
                userNameEditStatusText.text = userNameEditStatusLabel;

            RefreshSettingsUi();
        }

        private void SaveUserNameEdit()
        {
            if (userNameEditInputField == null)
                return;

            string sanitizedName = userNameEditInputField.text?.Trim();
            if (string.IsNullOrWhiteSpace(sanitizedName))
                return;

            GameDataStore.PlayerName = sanitizedName;
            CloseUserNameEditPanel();
            RefreshSettingsUi();
        }

        private void CloseUserNameEditPanel()
        {
            if (userNameEditPanel != null)
                userNameEditPanel.SetActive(false);
        }

        private void HandleUserNameEditChanged(string value)
        {
            if (userNameEditSaveButton != null)
                userNameEditSaveButton.interactable = !string.IsNullOrWhiteSpace(value);
        }

        private void RefreshSettingsUi()
        {
            if (userNameInputField != null)
                userNameInputField.SetTextWithoutNotify(GameDataStore.PlayerName);

            if (userNameEditTitleText != null)
                userNameEditTitleText.text = userNameEditTitleLabel;

            if (userNameEditStatusText != null)
                userNameEditStatusText.text = userNameEditStatusLabel;

            if (userNameEditSaveButton != null)
            {
                string currentValue = userNameEditInputField != null ? userNameEditInputField.text : string.Empty;
                userNameEditSaveButton.interactable = !string.IsNullOrWhiteSpace(currentValue);
            }

            bool isVibrationEnabled = GameDataStore.IsVibrationEnabled;
            bool isSoundEnabled = GameDataStore.IsSoundEnabled;
            bool isDarkModeEnabled = ThemeManager.IsDarkModeEnabled;

            if (!hasAppliedThemeState || lastAppliedDarkModeEnabled != isDarkModeEnabled)
            {
                ApplyDarkModeState(isDarkModeEnabled);
                hasAppliedThemeState = true;
                lastAppliedDarkModeEnabled = isDarkModeEnabled;
            }

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
            Color challengeInfoHeaderColor = palette.IsDarkMode
                ? new Color(0.24f, 0.28f, 0.36f, 1f)
                : new Color(0.25f, 0.25f, 0.27f, 1f);
            Color challengeInfoBodyColor = palette.IsDarkMode
                ? new Color(0.19f, 0.23f, 0.3f, 1f)
                : new Color(0.19f, 0.2f, 0.2f, 1f);
            Color challengeInfoTitleColor = palette.IsDarkMode
                ? new Color(0.88f, 0.92f, 1f, 1f)
                : new Color(0.35f, 0.43f, 0.98f, 1f);
            Color challengeInfoPrimaryTextColor = palette.IsDarkMode
                ? new Color(0.96f, 0.98f, 1f, 1f)
                : Color.black;
            Color challengeInfoSecondaryTextColor = palette.IsDarkMode
                ? new Color(0.77f, 0.84f, 0.95f, 1f)
                : new Color(0.37f, 0.39f, 0.51f, 1f);

            SetImageColor(settingsPanel != null ? settingsPanel.GetComponent<Image>() : null, settingsCardColor);
            SetImageColor(FindAncestorImage(vibrationToggleButton), settingsCardColor);
            SetImageColor(FindAncestorImage(privacyButton), settingsCardColor);
            SetImageColor(FindAncestorImage(challengeTitleText), menuCardColor);

            Transform challengeCardTransform = challengeTitleText != null ? challengeTitleText.transform.parent : null;
            Image challengeInfoRoot = FindNamedImage(challengeCardTransform, "User Info Chances", null);
            Image challengeInfoHeader = FindNamedImage(challengeCardTransform, "Header", null);
            Image challengeInfoBody = FindNamedImage(challengeCardTransform, "CONTANT", null);
            TextMeshProUGUI challengeInfoHeaderText = FindNamedText(challengeCardTransform, "Label", null);
            SetImageColor(challengeInfoRoot, challengeInfoBodyColor);
            SetImageColor(challengeInfoHeader, challengeInfoHeaderColor);
            SetImageColor(challengeInfoBody, challengeInfoBodyColor);
            if (challengeInfoHeaderText != null)
                challengeInfoHeaderText.color = challengeInfoTitleColor;
            if (challengeChanceText != null)
                challengeChanceText.color = challengeInfoPrimaryTextColor;
            if (challengeNextChanceTimerText != null)
                challengeNextChanceTimerText.color = challengeInfoSecondaryTextColor;
            if (challengeStatusText != null)
                challengeStatusText.color = challengeInfoSecondaryTextColor;

            ThemeManager.ApplyButtonTheme(primaryPlayButton, primaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(cardPlayButton, primaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(shopButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(leaderboardButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(challengePlayButton, primaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
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

            Color purchaseButtonColor = palette.IsDarkMode
                ? new Color(0.98f, 0.74f, 0.22f, 1f)
                : new Color(0.95f, 0.68f, 0.15f, 1f);

            ThemeManager.ApplyButtonTheme(closeShopButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(hintBuyButton, shopBuyButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);
            ThemeManager.ApplyButtonTheme(livesBuyButton, shopBuyButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);

            if (purchaseGatePanel != null)
                SetImageColor(purchaseGatePanel.GetComponent<Image>(), popupOverlayColor);

            Image purchaseGateCard = FindNamedImage(purchaseGatePanel != null ? purchaseGatePanel.transform : null, "Purchase Gate Card", null);
            Image purchasePriceBadge = FindNamedImage(purchaseGatePanel != null ? purchaseGatePanel.transform : null, "Purchase Gate Price Badge", null);
            SetImageColor(purchaseGateCard, popupCardColor);
            SetImageColor(purchasePriceBadge, shopHeaderColor);
            ThemeManager.ApplyButtonTheme(purchaseGatePayButton, purchaseButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);

            if (purchaseSuccessPanel != null)
                SetImageColor(purchaseSuccessPanel.GetComponent<Image>(), popupOverlayColor);

            Image purchaseSuccessCard = FindNamedImage(purchaseSuccessPanel != null ? purchaseSuccessPanel.transform : null, "Purchase Success Card", null);
            SetImageColor(purchaseSuccessCard, popupCardColor);
            ThemeManager.ApplyButtonTheme(purchaseSuccessOkButton, purchaseButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);

            if (hintRewardPanel != null)
                SetImageColor(hintRewardPanel.GetComponent<Image>(), popupOverlayColor);

            Image hintRewardCard = FindNamedImage(hintRewardPanel != null ? hintRewardPanel.transform : null, "Hint Reward Card", null);
            SetImageColor(hintRewardCard, popupCardColor);
            ThemeManager.ApplyButtonTheme(hintRewardOkButton, purchaseButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);

            if (paymentFailedPanel != null)
                SetImageColor(paymentFailedPanel.GetComponent<Image>(), popupOverlayColor);

            Image paymentFailedCard = FindNamedImage(paymentFailedPanel != null ? paymentFailedPanel.transform : null, "Payment Failed Card", null);
            SetImageColor(paymentFailedCard, popupCardColor);
            ThemeManager.ApplyButtonTheme(paymentFailedRetryButton, purchaseButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);

            if (leaderboardPanel != null)
                SetImageColor(leaderboardPanel.GetComponent<Image>(), popupOverlayColor);

            Image leaderboardCard = FindNamedImage(leaderboardPanel != null ? leaderboardPanel.transform : null, "Menu Leaderboard Card", null);
            SetImageColor(leaderboardCard, menuCardColor);
            ThemeManager.ApplyButtonTheme(closeLeaderboardButton, secondaryButtonColor, buttonTextColor, disabledButtonColor, disabledTextColor);

            ApplyCurrentTabVisualState();
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

        private static TMP_InputField FindNamedInputField(Transform root, string name, TMP_InputField fallback)
        {
            Transform child = FindDescendantByName(root, name);
            return child != null ? child.GetComponent<TMP_InputField>() : fallback;
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
            int cycleIndex = GameDataStore.GetCurrentChallengeCycleIndex(nowUtc);
            string patternName = GetCurrentPatternName(nowUtc);

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

            ApplyMenuThemeOverrides();
        }

        private void BeginEntryFlowResolution()
        {
            if (entryFlowCoroutine != null)
                StopCoroutine(entryFlowCoroutine);

            ApplyPreResolvedEntryState();
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
            entryFlowResolved = true;
            RefreshLevelLabel();
            RefreshChallengeUi();
            RefreshSettingsUi();
            RefreshShopUi();
            RefreshPurchaseGateUi();
            RefreshPurchaseSuccessUi();
            RefreshHintRewardUi();
            ClosePaymentFailedPanel();
            RefreshLeaderboardUi();

            if (GameDataStore.HasPurchasedGame)
            {
                ClosePurchaseGatePanel();
                ClosePurchaseSuccessPanel();
                CloseHintRewardPanel();
                ClosePaymentFailedPanel();
                ShowHomeAfterEntryResolved();
                return;
            }

            if (!GameDataStore.HasCompletedTutorial)
            {
                SceneManager.LoadScene(TutorialSceneName);
                return;
            }

            ShowHomeAfterEntryResolved();
            OpenPurchaseGatePanel();
        }

        private void ApplyPreResolvedEntryState()
        {
            entryFlowResolved = false;

            if (!GameDataStore.HasCompletedTutorial)
            {
                SetTabState(false, false, false);
                return;
            }

            ShowHomeAfterEntryResolved();

            if (!GameDataStore.HasPurchasedGame)
                OpenPurchaseGatePanel();
        }

        private bool ShouldShowPurchaseGate()
        {
            return GameDataStore.HasCompletedTutorial && !GameDataStore.HasPurchasedGame;
        }

        private bool CanInteractWithMenu()
        {
            return entryFlowResolved &&
                   !ShouldShowPurchaseGate() &&
                   (purchaseSuccessPanel == null || !purchaseSuccessPanel.activeSelf) &&
                   (hintRewardPanel == null || !hintRewardPanel.activeSelf) &&
                   (userNameEditPanel == null || !userNameEditPanel.activeSelf);
        }

        private void ShowHomeAfterEntryResolved()
        {
            SetTabState(true, false, false);
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

        private void OpenPurchaseSuccessPanel()
        {
            if (purchaseSuccessPanel != null)
                purchaseSuccessPanel.SetActive(true);

            if (purchaseSuccessNameInputField != null)
            {
                string currentName = GameDataStore.PlayerName;
                purchaseSuccessNameInputField.SetTextWithoutNotify(string.Equals(currentName, "You", StringComparison.OrdinalIgnoreCase) ? string.Empty : currentName);
            }

            RefreshPurchaseSuccessUi();
        }

        private void ClosePurchaseSuccessPanel()
        {
            if (purchaseSuccessPanel != null)
                purchaseSuccessPanel.SetActive(false);
        }

        private void OpenHintRewardPanel()
        {
            RefreshHintRewardUi();
            if (hintRewardPanel != null)
                hintRewardPanel.SetActive(true);
        }

        private void CloseHintRewardPanel()
        {
            if (hintRewardPanel != null)
                hintRewardPanel.SetActive(false);
        }

        private void OpenPaymentFailedPanel(string message)
        {
            if (paymentFailedBodyText != null)
                paymentFailedBodyText.text = string.IsNullOrWhiteSpace(message) ? paymentFailedDefaultBodyLabel : message;

            if (paymentFailedPanel != null)
                paymentFailedPanel.SetActive(true);
        }

        private void ClosePaymentFailedPanel()
        {
            if (paymentFailedPanel != null)
                paymentFailedPanel.SetActive(false);
        }

        private void SubscribeMiniPayEvents()
        {
            GameDataStore.DataChanged -= HandleGameDataChanged;
            GameDataStore.DataChanged += HandleGameDataChanged;
            GameDataStore.ChallengeLeaderboardChanged -= HandleChallengeLeaderboardChanged;
            GameDataStore.ChallengeLeaderboardChanged += HandleChallengeLeaderboardChanged;
            MiniPayBridge.InitialStateResolved -= HandleInitialStateResolved;
            MiniPayBridge.InitialStateResolved += HandleInitialStateResolved;
            MiniPayBridge.GamePurchaseStatusReceived -= HandleGamePurchaseStatusReceived;
            MiniPayBridge.GamePurchaseStatusReceived += HandleGamePurchaseStatusReceived;
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
            GameDataStore.ChallengeLeaderboardChanged -= HandleChallengeLeaderboardChanged;
            MiniPayBridge.InitialStateResolved -= HandleInitialStateResolved;
            MiniPayBridge.GamePurchaseStatusReceived -= HandleGamePurchaseStatusReceived;
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
            if (isHandlingGameDataChange)
                return;

            isHandlingGameDataChange = true;
            try
            {
            RefreshLevelLabel();
            RefreshChallengeUi();
            RefreshSettingsUi();
            RefreshPurchaseGateUi();
            RefreshPurchaseSuccessUi();
            RefreshHintRewardUi();
            RefreshLeaderboardUi();

            if (GameDataStore.HasPurchasedGame)
            {
                ClosePurchaseGatePanel();
                ClosePaymentFailedPanel();
            }
            }
            finally
            {
                isHandlingGameDataChange = false;
            }
        }

        private void HandleGamePurchaseSucceeded()
        {
            purchaseGateBusy = false;
            showingHintPurchaseSuccess = false;
            shouldShowUnlockHintReward = true;
            purchaseGateStatusOverride = "Purchase complete. Welcome to Arrow Out.";
            RefreshPurchaseGateUi();
            ClosePaymentFailedPanel();
            ClosePurchaseGatePanel();
            OpenPurchaseSuccessPanel();
        }

        private void HandleGamePurchaseStatusReceived(string message)
        {
            purchaseGateBusy = false;
            purchaseGateStatusOverride = string.IsNullOrWhiteSpace(message)
                ? purchaseGateIdleStatusLabel
                : message;
            RefreshPurchaseGateUi();
            OpenPurchaseGatePanel();
        }

        private void HandleGamePurchaseFailed(string errorMessage)
        {
            purchaseGateBusy = false;
            string message = string.IsNullOrWhiteSpace(errorMessage) ? paymentFailedDefaultBodyLabel : errorMessage;
            purchaseGateStatusOverride = message;
            RefreshPurchaseGateUi();
            OpenPurchaseGatePanel();
            OpenPaymentFailedPanel(message);
        }

        private void HandleConsumablePurchaseSucceeded()
        {
            purchaseGateStatusOverride = "Purchase complete.";
            RefreshPurchaseGateUi();
            showingHintPurchaseSuccess = true;
            ClosePaymentFailedPanel();
            OpenHintRewardPanel();
        }

        private void HandleConsumablePurchaseFailed(string errorMessage)
        {
            purchaseGateStatusOverride = string.IsNullOrWhiteSpace(errorMessage) ? "Purchase failed. Please try again." : errorMessage;
            RefreshPurchaseGateUi();
            OpenPaymentFailedPanel(purchaseGateStatusOverride);
        }

        private void HandleChallengeLeaderboardChanged()
        {
            RefreshLeaderboardUi();
        }

        private void HandlePurchaseSuccessNameChanged(string value)
        {
            RefreshPurchaseSuccessUi();
        }

        private static string FormatCountdown(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.Zero)
                timeSpan = TimeSpan.Zero;

            int totalDays = Mathf.Max(0, timeSpan.Days);
            return $"{totalDays:00}d {timeSpan.Hours:00}h {timeSpan.Minutes:00}m {timeSpan.Seconds:00}s";
        }

        private string GetCurrentPatternName(DateTime nowUtc)
        {
            return GameDataStore.GetCurrentChallengePatternName(nowUtc, GetResolvedChallengePatternNames());
        }

        private string[] GetResolvedChallengePatternNames()
        {
            return challengePatternNames != null && challengePatternNames.Length > 0
                ? challengePatternNames
                : DefaultChallengePatternNames;
        }

        private void NormalizeLeaderboardPanelLayout()
        {
            if (leaderboardPanel == null)
                return;

            Transform scrollRoot = FindDescendantByName(leaderboardPanel.transform, "Menu Leaderboard Scroll View");
            if (scrollRoot != null)
            {
                LayoutElement scrollLayout = scrollRoot.GetComponent<LayoutElement>() ?? scrollRoot.gameObject.AddComponent<LayoutElement>();
                scrollLayout.flexibleHeight = 1f;
                scrollLayout.preferredHeight = Mathf.Max(scrollLayout.preferredHeight, 720f);
            }

            Transform spacer = FindDescendantByName(leaderboardPanel.transform, "Menu Leaderboard Bottom Spacer");
            if (spacer == null)
            {
                Transform card = FindDescendantByName(leaderboardPanel.transform, "Menu Leaderboard Card");
                if (card != null)
                {
                    GameObject spacerObject = new("Menu Leaderboard Bottom Spacer", typeof(RectTransform), typeof(LayoutElement));
                    RectTransform spacerRect = spacerObject.GetComponent<RectTransform>();
                    spacerRect.SetParent(card, false);
                    spacerRect.SetSiblingIndex(Mathf.Max(0, card.childCount - 1));
                    spacer = spacerRect;
                }
            }

            if (spacer != null)
            {
                LayoutElement spacerLayout = spacer.GetComponent<LayoutElement>() ?? spacer.gameObject.AddComponent<LayoutElement>();
                spacerLayout.flexibleHeight = 1f;
                spacerLayout.minHeight = 24f;
                spacerLayout.preferredHeight = Mathf.Max(spacerLayout.preferredHeight, 24f);
                if (closeLeaderboardButton != null)
                    spacer.SetSiblingIndex(Mathf.Max(0, closeLeaderboardButton.transform.GetSiblingIndex()));
            }

            if (closeLeaderboardButton != null)
            {
                LayoutElement closeLayout = closeLeaderboardButton.GetComponent<LayoutElement>() ?? closeLeaderboardButton.gameObject.AddComponent<LayoutElement>();
                closeLayout.preferredHeight = Mathf.Max(closeLayout.preferredHeight, 80f);
                closeLayout.flexibleHeight = 0f;
                closeLeaderboardButton.transform.SetAsLastSibling();
            }
        }

        private static string FormatRunTime(float seconds)
        {
            int totalMilliseconds = Mathf.RoundToInt(seconds * 1000f);
            int minutes = totalMilliseconds / 60000;
            int remainingMilliseconds = totalMilliseconds % 60000;
            int wholeSeconds = remainingMilliseconds / 1000;
            int milliseconds = remainingMilliseconds % 1000;
            return $"{minutes:00}:{wholeSeconds:00}.{milliseconds:000}";
        }
    }

}
