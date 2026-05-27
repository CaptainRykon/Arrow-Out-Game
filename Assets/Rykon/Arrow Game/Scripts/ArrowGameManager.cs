using System.Collections;
using System.Collections.Generic;
using DateTime = System.DateTime;
using DateTimeOffset = System.DateTimeOffset;
using TimeSpan = System.TimeSpan;
using ArrowGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ArrowGame
{
    public class ArrowGameManager : MonoBehaviour
    {
        public enum GameplayMode
        {
            Levels = 0,
            Challenge = 1,
            Tutorial = 2
        }

        public static ArrowGameManager Instance;
        private const string MenuSceneName = "MenuScene";
        private const string TutorialSceneName = "TutorialScene";
        private static readonly Vector2Int[] TutorialBoardSizes =
        {
            new(8, 8),
            new(10, 12),
            new(12, 16)
        };
        private static readonly int[] TutorialSeeds = { 137, 821, 4099 };

        [Header("Gameplay UI")]
        public GameObject winUI;
        public GameObject loseUI;
        public Image[] hearts;
        public TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI winMessageText;
        [SerializeField] private RectTransform hintButtonRect;
        [SerializeField] private TextMeshProUGUI hintAmountText;
        [SerializeField] private RectTransform noHintsPanelRect;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button reviveButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private RectTransform quitConfirmationPanelRect;
        [SerializeField] private Button quitConfirmYesButton;
        [SerializeField] private Button quitConfirmNoButton;
        [SerializeField] private RectTransform gameplayPurchasePopupRect;
        [SerializeField] private Button gameplayPurchasePrimaryButton;
        [SerializeField] private Button gameplayPurchaseSecondaryButton;
        [SerializeField] private TextMeshProUGUI gameplayPurchaseTitleText;
        [SerializeField] private TextMeshProUGUI gameplayPurchaseBodyText;
        [SerializeField] private TextMeshProUGUI gameplayPurchaseStatusText;
        [SerializeField] private Image damageOverlay;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private float damageFlashDuration = 0.35f;
        [SerializeField] private float damageFlashAlpha = 0.5f;
        [SerializeField] private float hintRevealDelay = 6f;
        [SerializeField] private float hintSlideDuration = 0.35f;
        [SerializeField] private float hintHiddenOffsetX = -220f;
        [SerializeField] private float noHintsPanelSlideDuration = 0.3f;
        [SerializeField] private float noHintsPanelDisplayDuration = 1.8f;
        [SerializeField] private float noHintsPanelHiddenOffsetY = -240f;
        [SerializeField] private float winMessageFadeDuration = 0.35f;
        [SerializeField] private float quitPanelSlideDuration = 0.3f;
        [SerializeField] private float quitPanelHiddenOffsetY = -320f;
        [SerializeField] private Vector2 quitPanelSize = new(620f, 320f);
        [SerializeField] private Vector2 quitPanelButtonSize = new(220f, 76f);
        [SerializeField] private Vector2 gameplayPurchasePopupSize = new(700f, 420f);
        [SerializeField] private int gameplayHintPurchaseAmount = 5;
        [SerializeField] private string gameplayHintPriceLabel = "$0.10";
        [SerializeField] private string gameplayRevivePriceLabel = "$0.05";

        [Header("Board")]
        public LineGenerator LineGenerator;
        [SerializeField] private GameplayMode gameplayMode = GameplayMode.Levels;
        [SerializeField] private int tutorialBoardSize = 10;
        [SerializeField] private int secondBoardSize = 15;
        [SerializeField] private int baseBoardWidth = 18;
        [SerializeField] private int baseBoardHeight = 26;
        [SerializeField] private int challengeBoardWidth = 18;
        [SerializeField] private int challengeBoardHeight = 26;
        [SerializeField] private ChallengePatternLibrary challengePatternLibrary;
        [SerializeField] private int challengeSeedOffset;
        [SerializeField] private int boardGrowthStartLevel = 60;
        [SerializeField] private int boardGrowthInterval = 20;
        [SerializeField] private int boardGrowthStep = 2;
        [SerializeField] private float cameraBoardPadding = 3.5f;
        [SerializeField] private float minimumCameraSize = 18f;
        [SerializeField] private float pinchZoomSpeed = 0.032f;
        [SerializeField] private float pinchZoomResponse = 1.45f;
        [SerializeField] private float minZoomRatio = 0.7f;
        [SerializeField] private float maxZoomRatio = 1.35f;
        [SerializeField] private int maxZoomVisibleGridRows = 20;
        [SerializeField] private int maxZoomVisibleGridColumns = 15;
        [SerializeField] private float zoomInCellPadding = 1.5f;
        [SerializeField] private float absoluteMinimumZoomSize = 2.5f;
        [SerializeField] private float editorScrollZoomSpeed = 1.8f;
        [SerializeField] private float dragThresholdPixels = 12f;
        [SerializeField] private float winCameraResetDuration = 0.45f;
        [SerializeField] private float winLeadInDelay = 0.35f;
        [SerializeField] private float winLastArrowSettleDuration = 0.35f;
        [SerializeField] private float winPanelDelay = 0.2f;
        [SerializeField] private float levelIntroDelay = 0.12f;
        [SerializeField] private float levelIntroDuration = 0.85f;
        [SerializeField] private Vector2 challengeLoseRetryButtonPosition = new(-120f, -250f);
        [SerializeField] private Vector2 challengeLoseReviveButtonPosition = new(0f, -360f);
        [SerializeField] private Vector2 challengeLoseMainMenuButtonPosition = new(120f, -250f);
        [SerializeField] private Vector2 challengeLoseButtonSize = new(260f, 84f);
        [SerializeField] private Vector2 standardLoseReviveButtonPosition = new(0f, -360f);
        [SerializeField] private string reviveButtonLabel = "Revive $0.05";
        [SerializeField] private float tutorialAdvanceDelay = 0.4f;
        [SerializeField] private float tutorialHeaderFontSize = 28f;
        [SerializeField] private float pinchTapBlockDuration = 0.18f;
        [SerializeField] private int initialVisibleGridRows = 25;
        [SerializeField] private int initialVisibleGridColumns = 25;
        [SerializeField] private float initialGridZoomCellPadding = 1.2f;
        [SerializeField] private float initialGridZoomDuration = 0.4f;
        [SerializeField] private int hintFocusVisibleGridRows = 10;
        [SerializeField] private int hintFocusVisibleGridColumns = 8;
        [SerializeField] private float hintFocusCellPadding = 1.1f;
        [SerializeField] private float hintFocusDuration = 0.28f;
        [SerializeField] private float panSensitivity = 1.15f;
        [SerializeField] private float panSmoothTime = 0.08f;
        [SerializeField] private float zoomSmoothTime = 0.05f;

        [Header("Guide Toggle")]
        [SerializeField] private Button guideToggleButton;
        [SerializeField] private Color guideButtonOffColor = new(0.33f, 0.35f, 0.51f, 0.92f);
        [SerializeField] private Color guideButtonOnColor = new(0.54f, 0.63f, 1f, 0.96f);

        public int heart = 3;

        private readonly List<LineController> lines = new();
        private readonly List<int> levelSeed = new();
        private readonly string[] winMessages = { "Awesome", "Wonderful", "Great", "Brilliant", "Superb", "Excellent" };
        private Image guideToggleButtonImage;
        private bool guideLinesVisible;
        private int totalLineCount;
        private Coroutine damageFlashCoroutine;
        private float fittedCameraSize;
        private float minAllowedCameraSize;
        private float maxAllowedCameraSize;
        private Vector2 boardWorldMin;
        private Vector2 boardWorldMax;
        private Vector3 fittedCameraPosition;
        private Vector2 singleTouchStartScreenPosition;
        private Vector3 singleTouchStartCameraPosition;
        private bool hasDraggedCurrentTouch;
        private bool isTransitioningToWin;
        private bool hintVisible;
        private float idleSinceLastRemoval;
        private Vector2 hintVisibleAnchoredPosition;
        private Vector2 hintHiddenAnchoredPosition;
        private Coroutine hintSlideCoroutine;
        private Vector2 noHintsPanelVisibleAnchoredPosition;
        private Vector2 noHintsPanelHiddenAnchoredPosition;
        private Coroutine noHintsPanelCoroutine;
        private Vector2 quitPanelVisibleAnchoredPosition;
        private Vector2 quitPanelHiddenAnchoredPosition;
        private Coroutine quitPanelCoroutine;
        private Coroutine winMessageCoroutine;
        private bool isQuitPanelVisible;
        private bool isLevelIntroPlaying;
        private bool externalInputLocked;
        private bool hasStartedGameplayIntro;
        private bool hasChallengeSceneController;
        private int currentTutorialStep;
        private float touchTapBlockedUntilTime;
        private Vector3 targetCameraPanPosition;
        private Vector3 cameraPanVelocity;
        private bool hasTargetCameraPanPosition;
        private float targetCameraZoomSize;
        private float cameraZoomVelocity;
        private bool hasTargetCameraZoomSize;
        private bool challengeReviveUsedThisRun;
        private bool revivePurchasePending;
        private bool hintPurchasePending;
        private LineController activeHintLine;
        private Coroutine hintFocusCoroutine;

        private const string ChallengeReviveCooldownUnixMillisecondsKey = "challenge_revive_cooldown_until_unix_ms";
        private static readonly TimeSpan ChallengeReviveCooldown = TimeSpan.FromHours(12);

        private enum GameplayPurchasePopupMode
        {
            None = 0,
            HintOffer = 1,
            Success = 2,
            Failed = 3
        }

        private enum GameplayPurchaseContext
        {
            None = 0,
            Hint = 1,
            Revive = 2
        }

        private GameplayPurchasePopupMode gameplayPurchasePopupMode;
        private GameplayPurchaseContext gameplayPurchaseContext;
        private string gameplayPopupSuccessTitle;
        private string gameplayPopupSuccessBody;
        private string gameplayPopupSuccessStatus;
        private string gameplayPopupFailureMessage;

        public event UnityAction ChallengeCompleted;
        public event UnityAction ChallengeFailed;

        public bool CanProcessTouchTap => Input.touchCount == 1 && !hasDraggedCurrentTouch && !IsTouchTapBlocked();
        public bool HasDraggedCurrentTouch => hasDraggedCurrentTouch;
        public bool IsInputLocked => externalInputLocked || isTransitioningToWin || isLevelIntroPlaying || isQuitPanelVisible || loseUI.activeSelf || winUI.activeSelf;
        public GameplayMode CurrentGameplayMode => gameplayMode;
        public bool IsChallengeMode => gameplayMode == GameplayMode.Challenge;
        public bool IsTutorialMode => gameplayMode == GameplayMode.Tutorial || gameObject.scene.name == TutorialSceneName;

        private void Awake()
        {
            Instance = this;
            EnsureReviveButton();
            EnsureMainMenuLoseButton();
            EnsureExitButton();
            EnsureQuitConfirmationUi();
            EnsureGameplayPurchasePopup();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            EnsureReviveButton();
            EnsureMainMenuLoseButton();
            EnsureExitButton();
            EnsureQuitConfirmationUi();
            EnsureGameplayPurchasePopup();
        }

        public void ResetBoardStateForGeneration()
        {
            lines.Clear();
            totalLineCount = 0;
            RefreshProgress();
        }

        private void Start()
        {
            int l = Mathf.Max(GameDataStore.Level, 1);
            Application.targetFrameRate = 60;

            EnsureQuitConfirmationUi();
            EnsureGameplayPurchasePopup();
            EnsureReviveButton();
            InitializeGuideToggleButton();
            InitializeGameplayButtons();
            ApplyTheme();

            Random.InitState(0);
            for (int i = 0; i < 1000; i++)
                levelSeed.Add(Random.Range(0, 10000));

            ConfigureSeedAndBoard(l);
            FitBoardToCameraFromCurrentBoard();

            // Challenge boards are generated by the challenge loading flow so the loading panel can stay visible.
            LineGenerator.AutoGenerateOnEnable = !IsChallengeMode;

            // Use LineGenerator to generate the current level.
            LineGenerator.enabled = true;
            InitializeHintButton();
            InitializeNoHintsPanel();
            InitializeQuitConfirmationPanel();
            InitializeGameplayPurchasePopup();
            RefreshHintAmountText();
            RefreshProgress();
            RefreshHeartVisuals();
            SetDamageOverlayVisible(0f);
            SetWinMessageAlpha(0f);
            ConfigureModeSpecificUi();
            ApplyTheme();
            RefreshHeartVisuals();
            RefreshGuideButtonState();

            hasChallengeSceneController = FindFirstObjectByType<ChallengeSceneController>() != null;

            if (!IsChallengeMode)
                StartCoroutine(BeginGameplayIntroCO());
            else if (hasChallengeSceneController)
                SetExternalInputLock(true);
            else
                StartCoroutine(BeginGameplayIntroCO());
        }

        private void OnEnable()
        {
            GameDataStore.DataChanged -= HandleSharedGameDataChanged;
            GameDataStore.DataChanged += HandleSharedGameDataChanged;
            MiniPayBridge.HintPurchaseSucceeded -= HandleHintPurchaseSucceeded;
            MiniPayBridge.HintPurchaseSucceeded += HandleHintPurchaseSucceeded;
            MiniPayBridge.HintPurchaseFailed -= HandleHintPurchaseFailed;
            MiniPayBridge.HintPurchaseFailed += HandleHintPurchaseFailed;
            MiniPayBridge.RevivePurchaseSucceeded -= HandleRevivePurchaseSucceeded;
            MiniPayBridge.RevivePurchaseSucceeded += HandleRevivePurchaseSucceeded;
            MiniPayBridge.RevivePurchaseFailed -= HandleRevivePurchaseFailed;
            MiniPayBridge.RevivePurchaseFailed += HandleRevivePurchaseFailed;
        }

        private void OnDisable()
        {
            GameDataStore.DataChanged -= HandleSharedGameDataChanged;
            MiniPayBridge.HintPurchaseSucceeded -= HandleHintPurchaseSucceeded;
            MiniPayBridge.HintPurchaseFailed -= HandleHintPurchaseFailed;
            MiniPayBridge.RevivePurchaseSucceeded -= HandleRevivePurchaseSucceeded;
            MiniPayBridge.RevivePurchaseFailed -= HandleRevivePurchaseFailed;
        }

        public void ShowHint()
        {
            ResetHintIdleTimer();

            LineController removableLine = null;
            foreach (LineController line in lines)
            {
                if (line.CanBeRemoved())
                {
                    removableLine = line;
                    break;
                }
            }

            if (removableLine == null)
                return;

            if (!GameDataStore.TryConsumeHint())
            {
                OpenHintPurchaseOffer();
                return;
            }

            RefreshHintAmountText();
            SetHintVisible(false);
            if (activeHintLine != null && activeHintLine != removableLine)
                activeHintLine.ClearHint();
            removableLine.ShowHint();
            activeHintLine = removableLine;
            FocusCameraOnHintLine(removableLine);
        }

        private void HandleSharedGameDataChanged()
        {
            RefreshHintAmountText();
        }

        public void AddLine(LineController line)
        {
            PruneNullLineReferences();
            lines.Add(line);
            totalLineCount++;
            line.SetGuideVisible(guideLinesVisible);
            RefreshProgress();
        }

        public void OnCollide()
        {
            if (IsTutorialMode)
                return;

            SoundManager.PlayArrowEscapeFail();
            heart--;
            if (heart < 0)
                heart = 0;

            RefreshHeartVisuals();
            PlayDamageFlash();
            if (heart <= 0)
                GameOver();
        }

        public void LineRemoved(LineController line)
        {
            PruneNullLineReferences();
            lines.Remove(line);
            if (activeHintLine == line)
                activeHintLine = null;
            ResetHintIdleTimer();
            SetHintVisible(false);
            RefreshProgress();
            if (lines.Count == 0)
                GameWin(line);
        }

        public void ToggleGuideLines()
        {
            SetGuideLinesVisible(!guideLinesVisible);
        }

        public void SetGuideLinesVisible(bool isVisible)
        {
            PruneNullLineReferences();
            guideLinesVisible = isVisible;
            RefreshGuideButtonState();

            foreach (LineController line in lines)
            {
                line.SetGuideVisible(guideLinesVisible);
            }
        }

        public void Retry()
        {
            if (IsChallengeMode && loseUI != null && loseUI.activeSelf)
            {
                ChallengeSceneController challengeSceneController = FindFirstObjectByType<ChallengeSceneController>();
                if (challengeSceneController != null && challengeSceneController.TryUseChallengeRetry())
                    return;
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void PurchaseRevive()
        {
            if (!CanShowReviveButton())
                return;

            revivePurchasePending = true;
            gameplayPurchaseContext = GameplayPurchaseContext.Revive;
            if (reviveButton != null)
                reviveButton.interactable = false;

            MiniPayBridge.Instance.BuyRevive(1, IsChallengeMode ? "challenge" : "classic");
        }

        public void PurchaseHintsInGameplay()
        {
            if (hintPurchasePending)
                return;

            hintPurchasePending = true;
            gameplayPurchaseContext = GameplayPurchaseContext.Hint;
            OpenGameplayPurchasePopup(
                GameplayPurchasePopupMode.HintOffer,
                "Buying hints...",
                $"Opening MiniPay to buy {gameplayHintPurchaseAmount} hints for {gameplayHintPriceLabel}.",
                "Complete the payment in MiniPay to add hints instantly."
            );
            MiniPayBridge.Instance.BuyHints(gameplayHintPurchaseAmount);
        }

        public void SetExternalInputLock(bool isLocked)
        {
            externalInputLocked = isLocked;
        }

        public IEnumerator BeginGameplayIntro()
        {
            yield return BeginGameplayIntroCO();
        }

        public void RefreshBoardAfterGeneration()
        {
            FitBoardToCameraFromCurrentBoard();
            RefreshProgress();
        }

        public void ConfigureChallengeRetryUi(bool retryAvailable)
        {
            if (!IsChallengeMode)
                return;

            ConfigureChallengeLoseButtons();

            if (mainMenuButton != null)
            {
                mainMenuButton.gameObject.SetActive(true);
                mainMenuButton.interactable = true;
            }

            ConfigureReviveButton();

            ApplyGameplayThemeOverrides();
        }

        public void OpenQuitConfirmation()
        {
            if ((loseUI != null && loseUI.activeSelf) || (winUI != null && winUI.activeSelf))
            {
                ConfirmQuitToMenu();
                return;
            }

            if (quitConfirmationPanelRect == null)
                return;

            HideHintUi();
            ShowQuitPanel(true);
        }

        public void ConfirmQuitToMenu()
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        public void CancelQuitToMenu()
        {
            ShowQuitPanel(false);
        }

        private void GameOver()
        {
            HideTransientGameplayUi();
            SoundManager.PlayLose();
            loseUI.SetActive(true);
            ConfigureReviveButton();

            if (IsChallengeMode)
            {
                ConfigureChallengeLoseButtons();
                ChallengeFailed?.Invoke();
            }
        }

        private void ConfigureChallengeLoseButtons()
        {
            if (!IsChallengeMode)
                return;

            if (restartButton != null)
                restartButton.gameObject.SetActive(false);

            ConfigureChallengeLoseButton(mainMenuButton);
            ConfigureReviveButton();
        }

        private void ConfigureChallengeLoseButton(Button button)
        {
            if (button == null)
                return;

            button.gameObject.SetActive(true);
            button.interactable = true;
        }

        private void ConfigureReviveButton()
        {
            if (reviveButton == null)
                return;

            bool shouldShowRevive = CanShowReviveButton();
            bool canPurchaseRevive = CanOfferPaidRevive();
            reviveButton.gameObject.SetActive(loseUI != null && loseUI.activeSelf && shouldShowRevive);
            reviveButton.interactable = shouldShowRevive && canPurchaseRevive && !revivePurchasePending;
        }

        private bool CanOfferPaidRevive()
        {
            if (revivePurchasePending)
                return false;

            if (IsChallengeMode && IsChallengeReviveOnCooldown())
                return false;

            if (!IsChallengeMode)
                return true;

            return !challengeReviveUsedThisRun;
        }

        private bool CanShowReviveButton()
        {
            if (!IsChallengeMode)
                return true;

            return !challengeReviveUsedThisRun;
        }

        private void HandleRevivePurchaseSucceeded()
        {
            revivePurchasePending = false;
            if (IsChallengeMode)
            {
                challengeReviveUsedThisRun = true;
                SetChallengeReviveCooldownUntil(DateTime.UtcNow + ChallengeReviveCooldown);
            }

            heart = Mathf.Max(3, heart);
            RefreshHeartVisuals();
            SetDamageOverlayVisible(0f);

            if (loseUI != null)
                loseUI.SetActive(false);

            OpenGameplaySuccessPopup(
                GameplayPurchaseContext.Revive,
                "Revive Successful",
                $"Your 3 hearts are back. You can continue from where you lost.",
                $"Revive payment {gameplayRevivePriceLabel} completed."
            );
            ConfigureReviveButton();
            ApplyGameplayThemeOverrides();
        }

        private void HandleRevivePurchaseFailed(string message)
        {
            revivePurchasePending = false;
            OpenGameplayFailurePopup(
                GameplayPurchaseContext.Revive,
                string.IsNullOrWhiteSpace(message) ? "Revive purchase failed." : message
            );
            ConfigureReviveButton();
            ApplyGameplayThemeOverrides();
        }

        private void HandleHintPurchaseSucceeded()
        {
            hintPurchasePending = false;
            RefreshHintAmountText();
            OpenGameplaySuccessPopup(
                GameplayPurchaseContext.Hint,
                "Hints Added",
                $"{gameplayHintPurchaseAmount} hints were added successfully.",
                $"You now have {GameDataStore.HintCount} hints ready to use."
            );
        }

        private void HandleHintPurchaseFailed(string message)
        {
            hintPurchasePending = false;
            OpenGameplayFailurePopup(
                GameplayPurchaseContext.Hint,
                string.IsNullOrWhiteSpace(message) ? "Hint purchase failed." : message
            );
        }

        private void OpenHintPurchaseOffer()
        {
            gameplayPurchaseContext = GameplayPurchaseContext.Hint;
            OpenGameplayPurchasePopup(
                GameplayPurchasePopupMode.HintOffer,
                "Out of Hints",
                $"Buy {gameplayHintPurchaseAmount} hints for {gameplayHintPriceLabel}.",
                "Your hints are saved to your account and work in both Classic and Challenge."
            );
        }

        private void OpenGameplaySuccessPopup(GameplayPurchaseContext context, string title, string body, string status)
        {
            gameplayPurchaseContext = context;
            gameplayPopupSuccessTitle = title;
            gameplayPopupSuccessBody = body;
            gameplayPopupSuccessStatus = status;
            OpenGameplayPurchasePopup(GameplayPurchasePopupMode.Success, title, body, status);
        }

        private void OpenGameplayFailurePopup(GameplayPurchaseContext context, string message)
        {
            gameplayPurchaseContext = context;
            gameplayPopupFailureMessage = message;
            OpenGameplayPurchasePopup(GameplayPurchasePopupMode.Failed, "Payment Failed", message, "Retry the payment or close this panel.");
        }

        private void OpenGameplayPurchasePopup(GameplayPurchasePopupMode mode, string title, string body, string status)
        {
            EnsureGameplayPurchasePopup();
            gameplayPurchasePopupMode = mode;
            SetExternalInputLock(true);

            if (gameplayPurchasePopupRect != null)
                gameplayPurchasePopupRect.gameObject.SetActive(true);

            if (gameplayPurchaseTitleText != null)
                gameplayPurchaseTitleText.text = title;

            if (gameplayPurchaseBodyText != null)
                gameplayPurchaseBodyText.text = body;

            if (gameplayPurchaseStatusText != null)
                gameplayPurchaseStatusText.text = status;

            ConfigureGameplayPurchasePopupButtons();
            ApplyGameplayThemeOverrides();
        }

        private void CloseGameplayPurchasePopup(bool resumeGameplay)
        {
            if (gameplayPurchasePopupRect != null)
                gameplayPurchasePopupRect.gameObject.SetActive(false);

            gameplayPurchasePopupMode = GameplayPurchasePopupMode.None;

            if (resumeGameplay)
                SetExternalInputLock(false);

            ApplyGameplayThemeOverrides();
        }

        private void ConfigureGameplayPurchasePopupButtons()
        {
            if (gameplayPurchasePrimaryButton == null || gameplayPurchaseSecondaryButton == null)
                return;

            TextMeshProUGUI primaryLabel = gameplayPurchasePrimaryButton.GetComponentInChildren<TextMeshProUGUI>(true);
            TextMeshProUGUI secondaryLabel = gameplayPurchaseSecondaryButton.GetComponentInChildren<TextMeshProUGUI>(true);

            switch (gameplayPurchasePopupMode)
            {
                case GameplayPurchasePopupMode.HintOffer:
                    gameplayPurchasePrimaryButton.gameObject.SetActive(true);
                    gameplayPurchaseSecondaryButton.gameObject.SetActive(true);
                    gameplayPurchasePrimaryButton.interactable = !hintPurchasePending;
                    gameplayPurchaseSecondaryButton.interactable = true;
                    if (primaryLabel != null)
                        primaryLabel.text = gameplayHintPriceLabel;
                    if (secondaryLabel != null)
                        secondaryLabel.text = "Close";
                    break;
                case GameplayPurchasePopupMode.Success:
                    gameplayPurchasePrimaryButton.gameObject.SetActive(true);
                    gameplayPurchaseSecondaryButton.gameObject.SetActive(false);
                    gameplayPurchasePrimaryButton.interactable = true;
                    if (primaryLabel != null)
                        primaryLabel.text = "OK";
                    break;
                case GameplayPurchasePopupMode.Failed:
                    gameplayPurchasePrimaryButton.gameObject.SetActive(true);
                    gameplayPurchaseSecondaryButton.gameObject.SetActive(true);
                    gameplayPurchasePrimaryButton.interactable = true;
                    gameplayPurchaseSecondaryButton.interactable = true;
                    if (primaryLabel != null)
                        primaryLabel.text = "Retry";
                    if (secondaryLabel != null)
                        secondaryLabel.text = "Close";
                    break;
                default:
                    gameplayPurchasePrimaryButton.gameObject.SetActive(false);
                    gameplayPurchaseSecondaryButton.gameObject.SetActive(false);
                    break;
            }
        }

        public void HandleGameplayPurchasePrimary()
        {
            switch (gameplayPurchasePopupMode)
            {
                case GameplayPurchasePopupMode.HintOffer:
                    PurchaseHintsInGameplay();
                    break;
                case GameplayPurchasePopupMode.Success:
                    if (gameplayPurchaseContext == GameplayPurchaseContext.Revive && IsChallengeMode)
                    {
                        ChallengeSceneController challengeSceneController = FindFirstObjectByType<ChallengeSceneController>();
                        if (challengeSceneController != null)
                            challengeSceneController.ResumeRunTimerAfterRevive();
                    }
                    CloseGameplayPurchasePopup(true);
                    break;
                case GameplayPurchasePopupMode.Failed:
                    if (gameplayPurchaseContext == GameplayPurchaseContext.Revive)
                        PurchaseRevive();
                    else if (gameplayPurchaseContext == GameplayPurchaseContext.Hint)
                        PurchaseHintsInGameplay();
                    break;
            }
        }

        public void HandleGameplayPurchaseSecondary()
        {
            bool resumeGameplay = gameplayPurchaseContext != GameplayPurchaseContext.Revive || (loseUI == null || !loseUI.activeSelf);
            CloseGameplayPurchasePopup(resumeGameplay);
        }

        private bool IsChallengeReviveOnCooldown()
        {
            return GetChallengeReviveCooldownUntilUtc() > DateTime.UtcNow;
        }

        private DateTime GetChallengeReviveCooldownUntilUtc()
        {
            string storedValue = PlayerPrefs.GetString(ChallengeReviveCooldownUnixMillisecondsKey, string.Empty);
            if (!long.TryParse(storedValue, out long unixMilliseconds) || unixMilliseconds <= 0L)
                return DateTime.MinValue;

            return DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;
        }

        private void SetChallengeReviveCooldownUntil(DateTime utcTime)
        {
            long unixMilliseconds = new DateTimeOffset(utcTime.ToUniversalTime()).ToUnixTimeMilliseconds();
            PlayerPrefs.SetString(ChallengeReviveCooldownUnixMillisecondsKey, unixMilliseconds.ToString());
            PlayerPrefs.Save();
        }

        private void GameWin(LineController lastRemovedLine)
        {
            if (isTransitioningToWin)
                return;

            SoundManager.PlayWin();
            StartCoroutine(WinCO(lastRemovedLine));
        }

        private IEnumerator WinCO(LineController lastRemovedLine)
        {
            isTransitioningToWin = true;
            SetGuideLinesVisible(false);
            HideTransientGameplayUi();
            hasDraggedCurrentTouch = false;

            yield return new WaitForSeconds(winLeadInDelay);
            if (lastRemovedLine != null)
                yield return new WaitForSeconds(winLastArrowSettleDuration);

            foreach (LineController line in FindObjectsByType<LineController>(FindObjectsSortMode.None))
            {
                line.CompleteForWin();
            }

            yield return AnimateCameraToFittedView();
            yield return LineGenerator.PlayDotClearAnimation();
            yield return new WaitForSeconds(winPanelDelay);

            if (IsTutorialMode)
            {
                yield return HandleTutorialBoardCompleted();
                yield break;
            }

            if (!IsChallengeMode)
                GameDataStore.Level++;

            winUI.SetActive(true);
            PlayWinMessageAnimation();

            if (IsChallengeMode)
                ChallengeCompleted?.Invoke();
        }

        private void EnsureReviveButton()
        {
            if (TryAssignExistingReviveButton())
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                EnsureEditorReviveButton();

            TryAssignExistingReviveButton();
#endif
        }

        private bool TryAssignExistingReviveButton()
        {
            if (reviveButton != null)
                return true;

            if (loseUI == null)
                return false;

            Transform parent = restartButton != null ? restartButton.transform.parent : loseUI.transform;
            if (parent == null)
                return false;

            reviveButton = FindButtonByName(parent, "Revive Button");
            if (reviveButton == null && loseUI.transform != parent)
                reviveButton = FindButtonByName(loseUI.transform, "Revive Button");

            return reviveButton != null;
        }

        private void EnsureMainMenuLoseButton()
        {
            if (TryAssignExistingMainMenuLoseButton())
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                EnsureEditorMainMenuLoseButton();

            TryAssignExistingMainMenuLoseButton();
#endif
        }

        private void EnsureExitButton()
        {
            if (TryAssignExistingExitButton())
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                PersistEditorGameplayButtonReference("exitButton", exitButton);
#endif
        }

        private bool TryAssignExistingMainMenuLoseButton()
        {
            if (loseUI == null)
                return false;

            if (mainMenuButton != null && mainMenuButton.transform.IsChildOf(loseUI.transform))
                return true;

            Transform parent = restartButton != null ? restartButton.transform.parent : loseUI.transform;
            if (parent == null)
                return false;

            Button sceneButton = FindButtonByName(parent, "Main Menu Button");
            if (sceneButton == null && loseUI.transform != parent)
                sceneButton = FindButtonByName(loseUI.transform, "Main Menu Button");

            if (sceneButton == null)
                return false;

            mainMenuButton = sceneButton;
            return true;
        }

        private bool TryAssignExistingExitButton()
        {
            if (exitButton != null)
                return true;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return false;

            string[] candidateNames =
            {
                "Exit Button",
                "Home Button ",
                "Home Button",
                "Back Button"
            };

            foreach (string candidateName in candidateNames)
            {
                Button candidate = FindButtonByName(canvas.transform, candidateName);
                if (candidate == null)
                    continue;

                exitButton = candidate;
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        private void EnsureEditorReviveButton()
        {
            if (loseUI == null || restartButton == null)
                return;

            Transform parent = restartButton.transform.parent != null ? restartButton.transform.parent : loseUI.transform;
            if (parent == null)
                return;

            Button existingButton = FindButtonByName(parent, "Revive Button");
            if (existingButton != null)
            {
                reviveButton = existingButton;
                PersistEditorReviveButtonReference();
                return;
            }

            GameObject reviveObject = new("Revive Button", typeof(RectTransform), typeof(Image), typeof(Button));
            UnityEditor.Undo.RegisterCreatedObjectUndo(reviveObject, "Create Revive Button");
            reviveObject.transform.SetParent(parent, false);

            Image image = reviveObject.GetComponent<Image>();
            Button button = reviveObject.GetComponent<Button>();
            button.targetGraphic = image;

            if (restartButton != null)
            {
                Image restartImage = restartButton.GetComponent<Image>();
                if (restartImage != null)
                {
                    image.sprite = restartImage.sprite;
                    image.type = restartImage.type;
                    image.color = restartImage.color;
                    image.material = restartImage.material;
                }
            }

            button.colors = restartButton.colors;
            button.transition = restartButton.transition;
            button.navigation = restartButton.navigation;

            RectTransform reviveRect = reviveObject.GetComponent<RectTransform>();
            RectTransform restartRect = restartButton.transform as RectTransform;
            if (reviveRect != null && restartRect != null)
            {
                reviveRect.anchorMin = restartRect.anchorMin;
                reviveRect.anchorMax = restartRect.anchorMax;
                reviveRect.pivot = restartRect.pivot;
                reviveRect.sizeDelta = restartRect.sizeDelta;
                reviveRect.localScale = restartRect.localScale;
                reviveRect.anchoredPosition = standardLoseReviveButtonPosition;
            }

            RectTransform labelRect = new GameObject("Label", typeof(RectTransform)).GetComponent<RectTransform>();
            UnityEditor.Undo.RegisterCreatedObjectUndo(labelRect.gameObject, "Create Revive Button Label");
            labelRect.SetParent(reviveObject.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = reviveButtonLabel;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.fontSize = 28f;
            label.color = Color.white;

            if (restartButton != null)
            {
                TextMeshProUGUI restartLabel = restartButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (restartLabel != null)
                {
                    label.font = restartLabel.font;
                    label.fontSharedMaterial = restartLabel.fontSharedMaterial;
                    label.fontSize = restartLabel.fontSize;
                    label.color = restartLabel.color;
                }
            }

            reviveButton = button;
            reviveButton.gameObject.SetActive(true);
            PersistEditorReviveButtonReference();
        }

        private void EnsureEditorMainMenuLoseButton()
        {
            if (loseUI == null)
                return;

            Transform parent = restartButton != null && restartButton.transform.parent != null
                ? restartButton.transform.parent
                : loseUI.transform;

            if (parent == null)
                return;

            Button existingButton = FindButtonByName(parent, "Main Menu Button");
            if (existingButton != null)
            {
                mainMenuButton = existingButton;
                PersistEditorGameplayButtonReference("mainMenuButton", mainMenuButton);
                return;
            }

            Button templateButton = restartButton != null ? restartButton : reviveButton;
            GameObject buttonObject = new("Main Menu Button", typeof(RectTransform), typeof(Image), typeof(Button));
            UnityEditor.Undo.RegisterCreatedObjectUndo(buttonObject, "Create Main Menu Button");
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            if (templateButton != null)
            {
                Image templateImage = templateButton.GetComponent<Image>();
                if (templateImage != null)
                {
                    image.sprite = templateImage.sprite;
                    image.type = templateImage.type;
                    image.color = templateImage.color;
                    image.material = templateImage.material;
                }

                button.colors = templateButton.colors;
                button.transition = templateButton.transition;
                button.navigation = templateButton.navigation;
            }

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            RectTransform templateRect = templateButton != null ? templateButton.transform as RectTransform : null;
            if (buttonRect != null && templateRect != null)
            {
                buttonRect.anchorMin = templateRect.anchorMin;
                buttonRect.anchorMax = templateRect.anchorMax;
                buttonRect.pivot = templateRect.pivot;
                buttonRect.sizeDelta = templateRect.sizeDelta;
                buttonRect.localScale = templateRect.localScale;

                Vector2 basePosition = reviveButton != null && reviveButton.transform is RectTransform reviveRect
                    ? reviveRect.anchoredPosition
                    : templateRect.anchoredPosition;
                buttonRect.anchoredPosition = basePosition + new Vector2(0f, -110f);
            }

            RectTransform labelRect = new GameObject("Label", typeof(RectTransform)).GetComponent<RectTransform>();
            UnityEditor.Undo.RegisterCreatedObjectUndo(labelRect.gameObject, "Create Main Menu Button Label");
            labelRect.SetParent(buttonObject.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = "Main Menu";
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.fontSize = 28f;
            label.color = Color.white;

            if (templateButton != null)
            {
                TextMeshProUGUI templateLabel = templateButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (templateLabel != null)
                {
                    label.font = templateLabel.font;
                    label.fontSharedMaterial = templateLabel.fontSharedMaterial;
                    label.fontSize = templateLabel.fontSize;
                    label.color = templateLabel.color;
                }
            }

            mainMenuButton = button;
            mainMenuButton.gameObject.SetActive(true);
            PersistEditorGameplayButtonReference("mainMenuButton", mainMenuButton);
        }

        private void PersistEditorReviveButtonReference()
        {
            PersistEditorGameplayButtonReference("reviveButton", reviveButton);
        }

        private void PersistEditorGameplayButtonReference(string propertyName, Object value)
        {
            UnityEditor.SerializedObject serializedObject = new(this);
            AssignEditorReference(serializedObject, propertyName, value);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            UnityEditor.EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif

        private void InitializeGuideToggleButton()
        {
            if (guideToggleButton == null)
            {
                Debug.LogWarning("ArrowGameManager is missing the guide toggle button reference.");
                return;
            }

            guideToggleButton.onClick.RemoveListener(ToggleGuideLines);
            guideToggleButton.onClick.AddListener(ToggleGuideLines);
            guideToggleButtonImage = guideToggleButton.targetGraphic as Image;

            RefreshGuideButtonState();
        }

        private void InitializeGameplayButtons()
        {
            EnsureReviveButton();
            EnsureExitButton();
            EnsureGameplayPurchasePopup();
            ButtonBindingUtility.Bind(restartButton, Retry);
            ButtonBindingUtility.Bind(reviveButton, PurchaseRevive);
            ButtonBindingUtility.Bind(mainMenuButton, OpenQuitConfirmation);
            ButtonBindingUtility.Bind(exitButton, OpenQuitConfirmation);
            ButtonBindingUtility.Bind(quitConfirmYesButton, ConfirmQuitToMenu);
            ButtonBindingUtility.Bind(quitConfirmNoButton, CancelQuitToMenu);
            ButtonBindingUtility.Bind(gameplayPurchasePrimaryButton, HandleGameplayPurchasePrimary);
            ButtonBindingUtility.Bind(gameplayPurchaseSecondaryButton, HandleGameplayPurchaseSecondary);
        }

        private void InitializeHintButton()
        {
            if (hintButtonRect == null)
                return;

            hintVisibleAnchoredPosition = hintButtonRect.anchoredPosition;
            hintHiddenAnchoredPosition = hintVisibleAnchoredPosition + Vector2.right * hintHiddenOffsetX;
            idleSinceLastRemoval = 0f;
            hintVisible = false;
            hintButtonRect.anchoredPosition = hintHiddenAnchoredPosition;
        }

        private void InitializeNoHintsPanel()
        {
            if (noHintsPanelRect == null)
                return;

            noHintsPanelVisibleAnchoredPosition = noHintsPanelRect.anchoredPosition;
            noHintsPanelHiddenAnchoredPosition = noHintsPanelVisibleAnchoredPosition + Vector2.down * noHintsPanelHiddenOffsetY;
            noHintsPanelRect.anchoredPosition = noHintsPanelHiddenAnchoredPosition;
            noHintsPanelRect.gameObject.SetActive(false);
        }

        private void InitializeQuitConfirmationPanel()
        {
            EnsureQuitConfirmationUi();

            if (quitConfirmationPanelRect == null)
                return;

            quitPanelVisibleAnchoredPosition = quitConfirmationPanelRect.anchoredPosition;
            quitPanelHiddenAnchoredPosition = quitPanelVisibleAnchoredPosition + Vector2.down * quitPanelHiddenOffsetY;
            quitConfirmationPanelRect.anchoredPosition = quitPanelHiddenAnchoredPosition;
            quitConfirmationPanelRect.gameObject.SetActive(false);
            isQuitPanelVisible = false;
        }

        private void InitializeGameplayPurchasePopup()
        {
            EnsureGameplayPurchasePopup();

            if (gameplayPurchasePopupRect == null)
                return;

            gameplayPurchasePopupRect.gameObject.SetActive(false);
            gameplayPurchasePopupMode = GameplayPurchasePopupMode.None;
        }

        private void EnsureQuitConfirmationUi()
        {
            if (quitConfirmationPanelRect != null && quitConfirmYesButton != null && quitConfirmNoButton != null)
                return;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            RectTransform panel = quitConfirmationPanelRect != null
                ? quitConfirmationPanelRect
                : FindChildRect(canvas.transform, "Quit Confirmation Panel");

            if (panel == null)
                panel = CreateQuitConfirmationPanel(canvas.transform);

            quitConfirmationPanelRect = panel;
            quitConfirmYesButton = FindButtonByName(panel, "Quit Confirm Yes Button");
            quitConfirmNoButton = FindButtonByName(panel, "Quit Confirm No Button");

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.SerializedObject serializedObject = new(this);
                AssignEditorReference(serializedObject, "quitConfirmationPanelRect", quitConfirmationPanelRect);
                AssignEditorReference(serializedObject, "quitConfirmYesButton", quitConfirmYesButton);
                AssignEditorReference(serializedObject, "quitConfirmNoButton", quitConfirmNoButton);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();

                UnityEditor.EditorUtility.SetDirty(this);
                if (gameObject.scene.IsValid())
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif
        }

        private void EnsureGameplayPurchasePopup()
        {
            if (TryAssignExistingGameplayPurchasePopup())
                return;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EnsureEditorGameplayPurchasePopup(canvas);
                TryAssignExistingGameplayPurchasePopup();
                return;
            }
#endif

            RectTransform panel = CreateGameplayPurchasePopup(GetGameplayPopupParent(canvas));
            AssignGameplayPurchasePopupReferences(panel);
        }

        private bool TryAssignExistingGameplayPurchasePopup()
        {
            if (gameplayPurchasePopupRect != null &&
                gameplayPurchasePrimaryButton != null &&
                gameplayPurchaseSecondaryButton != null &&
                gameplayPurchaseTitleText != null &&
                gameplayPurchaseBodyText != null &&
                gameplayPurchaseStatusText != null)
            {
                return true;
            }

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return false;

            RectTransform panel = gameplayPurchasePopupRect != null
                ? gameplayPurchasePopupRect
                : FindChildRect(canvas.transform, "Gameplay Purchase Popup");

            if (panel == null && loseUI != null)
                panel = FindChildRect(loseUI.transform, "Gameplay Purchase Popup");

            if (panel == null)
                return false;

            AssignGameplayPurchasePopupReferences(panel);
            return gameplayPurchasePopupRect != null &&
                   gameplayPurchasePrimaryButton != null &&
                   gameplayPurchaseSecondaryButton != null;
        }

        private void AssignGameplayPurchasePopupReferences(RectTransform panel)
        {
            gameplayPurchasePopupRect = panel;
            gameplayPurchasePrimaryButton = FindButtonByName(panel, "Gameplay Purchase Primary Button");
            gameplayPurchaseSecondaryButton = FindButtonByName(panel, "Gameplay Purchase Secondary Button");
            gameplayPurchaseTitleText = FindLabelByName(panel, "Gameplay Purchase Title");
            gameplayPurchaseBodyText = FindLabelByName(panel, "Gameplay Purchase Body");
            gameplayPurchaseStatusText = FindLabelByName(panel, "Gameplay Purchase Status");
        }

        private Transform GetGameplayPopupParent(Canvas canvas)
        {
            if (loseUI != null && loseUI.transform.parent != null)
                return loseUI.transform.parent;

            if (winUI != null && winUI.transform.parent != null)
                return winUI.transform.parent;

            return canvas.transform;
        }

        private RectTransform CreateQuitConfirmationPanel(Transform parent)
        {
            RectTransform panel = CreateUiRect("Quit Confirmation Panel", parent);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = quitPanelSize;
            panel.anchoredPosition = Vector2.zero;

            Image panelImage = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
            ApplyOpaquePanelImage(panelImage);

            CreateUiLabel(panel, "Quit Title", "Quit Level?", 42f, new Vector2(0f, 88f), new Vector2(420f, 56f));
            CreateUiLabel(panel, "Quit Message", "Are you sure you want to go back to the main menu?", 24f, new Vector2(0f, 24f), new Vector2(520f, 72f));

            CreateUiButton(panel, "Quit Confirm Yes Button", "Yes", new Vector2(-120f, -92f), quitPanelButtonSize);
            CreateUiButton(panel, "Quit Confirm No Button", "No", new Vector2(120f, -92f), quitPanelButtonSize);
            panel.gameObject.SetActive(false);
            return panel;
        }

        private RectTransform CreateGameplayPurchasePopup(Transform parent)
        {
            RectTransform panel = CreateUiRect("Gameplay Purchase Popup", parent);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = gameplayPurchasePopupSize;
            panel.anchoredPosition = Vector2.zero;

            Image panelImage = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
            ApplyOpaquePanelImage(panelImage);

            CreateUiLabel(panel, "Gameplay Purchase Title", "Store", 40f, new Vector2(0f, 116f), new Vector2(520f, 56f));
            CreateUiLabel(panel, "Gameplay Purchase Body", "Body", 24f, new Vector2(0f, 28f), new Vector2(560f, 110f));
            CreateUiLabel(panel, "Gameplay Purchase Status", "Status", 20f, new Vector2(0f, -46f), new Vector2(560f, 70f));

            CreateUiButton(panel, "Gameplay Purchase Primary Button", gameplayHintPriceLabel, new Vector2(-120f, -136f), quitPanelButtonSize);
            CreateUiButton(panel, "Gameplay Purchase Secondary Button", "Close", new Vector2(120f, -136f), quitPanelButtonSize);
            panel.gameObject.SetActive(false);
            return panel;
        }

#if UNITY_EDITOR
        private void EnsureEditorGameplayPurchasePopup(Canvas canvas)
        {
            Transform parent = GetGameplayPopupParent(canvas);
            if (parent == null)
                return;

            RectTransform existingPanel = FindChildRect(parent, "Gameplay Purchase Popup");
            if (existingPanel != null)
            {
                AssignGameplayPurchasePopupReferences(existingPanel);
                PersistEditorGameplayPurchasePopupReferences();
                return;
            }

            RectTransform panel = CreateGameplayPurchasePopup(parent);
            UnityEditor.Undo.RegisterCreatedObjectUndo(panel.gameObject, "Create Gameplay Purchase Popup");
            AssignGameplayPurchasePopupReferences(panel);
            PersistEditorGameplayPurchasePopupReferences();
        }

        private void PersistEditorGameplayPurchasePopupReferences()
        {
            UnityEditor.SerializedObject serializedObject = new(this);
            AssignEditorReference(serializedObject, "gameplayPurchasePopupRect", gameplayPurchasePopupRect);
            AssignEditorReference(serializedObject, "gameplayPurchasePrimaryButton", gameplayPurchasePrimaryButton);
            AssignEditorReference(serializedObject, "gameplayPurchaseSecondaryButton", gameplayPurchaseSecondaryButton);
            AssignEditorReference(serializedObject, "gameplayPurchaseTitleText", gameplayPurchaseTitleText);
            AssignEditorReference(serializedObject, "gameplayPurchaseBodyText", gameplayPurchaseBodyText);
            AssignEditorReference(serializedObject, "gameplayPurchaseStatusText", gameplayPurchaseStatusText);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            UnityEditor.EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif

        private Button CreateUiButton(Transform parent, string name, string labelText, Vector2 anchoredPosition, Vector2 size)
        {
            RectTransform rect = FindChildRect(parent, name) ?? CreateUiRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            if (image.sprite == null)
                image.sprite = GetDefaultUiSprite();
            image.type = Image.Type.Sliced;
            image.color = new Color(0.31f, 0.35f, 0.51f, 0.94f);

            Button button = rect.GetComponent<Button>() ?? rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            CreateUiLabel(rect, "Label", labelText, 30f, Vector2.zero, size - new Vector2(24f, 20f));
            return button;
        }

        private TextMeshProUGUI CreateUiLabel(Transform parent, string name, string text, float fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            RectTransform rect = FindChildRect(parent, name) ?? CreateUiRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI label = rect.GetComponent<TextMeshProUGUI>() ?? rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.12f, 0.16f, 0.25f, 1f);
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;
            return label;
        }

        private static RectTransform CreateUiRect(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static RectTransform FindChildRect(Transform parent, string name)
        {
            if (parent == null)
                return null;

            Transform child = FindDeepChild(parent, name);
            return child as RectTransform;
        }

        private static Button FindButtonByName(Transform parent, string name)
        {
            Transform child = FindDeepChild(parent, name);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private static TextMeshProUGUI FindLabelByName(Transform parent, string name)
        {
            Transform child = FindDeepChild(parent, name);
            return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            if (parent == null)
                return null;

            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                Transform nested = FindDeepChild(child, childName);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private static Sprite GetDefaultUiSprite()
        {
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static void ApplyOpaquePanelImage(Image image)
        {
            if (image == null)
                return;

            float alpha = image.color.a;
            image.sprite = GetDefaultUiSprite();
            image.type = Image.Type.Sliced;
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        private static void ApplyPanelThemeColor(Image image, Color panelColor)
        {
            if (image == null)
                return;

            float alpha = image.color.a;
            ApplyOpaquePanelImage(image);
            image.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
        }

#if UNITY_EDITOR
        private static void AssignEditorReference(UnityEditor.SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            UnityEditor.SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
        }
#endif

        private void RefreshGuideButtonState()
        {
            if (guideToggleButton == null)
                return;

            ThemeManager.ThemePalette palette = ThemeManager.CurrentPalette;
            Color buttonColor = guideLinesVisible ? palette.GuideButtonOnColor : palette.GuideButtonOffColor;
            ThemeManager.ApplyButtonTheme(guideToggleButton, buttonColor, palette.TextPrimaryColor, buttonColor, palette.TextPrimaryColor, true);
            guideToggleButtonImage = guideToggleButton.targetGraphic as Image;
        }

        public void RefreshTheme()
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            ThemeManager.ThemePalette palette = ThemeManager.CurrentPalette;

            if (LineGenerator != null)
                LineGenerator.ApplyThemePalette(palette);

            ThemeManager.ApplyThemeToScene(gameObject.scene);
            ApplyGameplayThemeOverrides();
            RefreshGuideButtonState();
            RefreshHeartVisuals();
        }

        private void ApplyGameplayThemeOverrides()
        {
            ThemeManager.ThemePalette palette = ThemeManager.CurrentPalette;
            if (!palette.IsDarkMode)
            {
                RefreshGuideButtonState();
                RefreshHeartVisuals();
                return;
            }

            Color iconButtonColor = palette.IsDarkMode
                ? new Color(0.3f, 0.39f, 0.56f, 0.98f)
                : new Color(0.84f, 0.86f, 0.9f, 1f);
            Color hintButtonColor = palette.IsDarkMode
                ? new Color(0.19f, 0.72f, 0.97f, 1f)
                : new Color(0.12f, 0.74f, 0.97f, 1f);
            Color neutralButtonColor = palette.IsDarkMode
                ? new Color(0.24f, 0.31f, 0.45f, 0.98f)
                : new Color(0.31f, 0.35f, 0.51f, 0.94f);
            Color disabledButtonColor = palette.IsDarkMode
                ? new Color(0.16f, 0.2f, 0.27f, 1f)
                : new Color(0.72f, 0.76f, 0.83f, 1f);
            Color panelColor = palette.IsDarkMode
                ? new Color(0.12f, 0.16f, 0.22f, 1f)
                : new Color(1f, 1f, 1f, 1f);

            ThemeManager.ApplyButtonTheme(quitConfirmYesButton, palette.AccentColor, Color.white, disabledButtonColor, palette.TextSecondaryColor);
            ThemeManager.ApplyButtonTheme(quitConfirmNoButton, neutralButtonColor, Color.white, disabledButtonColor, palette.TextSecondaryColor);

            if (hintButtonRect != null)
            {
                Button hintButton = hintButtonRect.GetComponent<Button>();
                ThemeManager.ApplyButtonTheme(hintButton, hintButtonColor, Color.white, hintButtonColor, Color.white);
            }

            if (noHintsPanelRect != null && noHintsPanelRect.TryGetComponent(out Image noHintsImage))
            {
                ApplyPanelThemeColor(noHintsImage, panelColor);
            }

            if (quitConfirmationPanelRect != null && quitConfirmationPanelRect.TryGetComponent(out Image quitPanelImage))
            {
                ApplyPanelThemeColor(quitPanelImage, panelColor);
            }

            if (gameplayPurchasePopupRect != null && gameplayPurchasePopupRect.TryGetComponent(out Image gameplayPurchasePanelImage))
            {
                ApplyPanelThemeColor(gameplayPurchasePanelImage, panelColor);
            }

            ThemeManager.ApplyButtonTheme(gameplayPurchasePrimaryButton, palette.AccentColor, Color.white, disabledButtonColor, palette.TextSecondaryColor);
            ThemeManager.ApplyButtonTheme(gameplayPurchaseSecondaryButton, neutralButtonColor, Color.white, disabledButtonColor, palette.TextSecondaryColor);

            ApplyProgressTheme(palette);
        }

        private void ApplyProgressTheme(ThemeManager.ThemePalette palette)
        {
            if (progressSlider == null)
                return;

            Image fillImage = progressSlider.fillRect != null ? progressSlider.fillRect.GetComponent<Image>() : null;
            Image[] sliderImages = progressSlider.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < sliderImages.Length; i++)
            {
                if (sliderImages[i] == null)
                    continue;

                if (sliderImages[i] == fillImage)
                {
                    sliderImages[i].color = palette.IsDarkMode
                        ? new Color(0.38f, 0.56f, 1f, 1f)
                        : Color.white;
                    continue;
                }

                sliderImages[i].color = palette.IsDarkMode
                    ? new Color(0.16f, 0.19f, 0.26f, 1f)
                    : new Color(0.92f, 0.94f, 1f, 1f);
            }
        }

        private void RefreshHeartVisuals()
        {
            if (hearts == null)
                return;

            ThemeManager.ThemePalette palette = ThemeManager.CurrentPalette;
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null)
                    continue;

                hearts[i].color = i < heart ? palette.HeartFilledColor : palette.HeartEmptyColor;
            }
        }

        private void ConfigureBoardForLevel(int level)
        {
            switch (level)
            {
                case 1:
                    LineGenerator.width = tutorialBoardSize;
                    LineGenerator.height = tutorialBoardSize;
                    break;
                case 2:
                    LineGenerator.width = secondBoardSize;
                    LineGenerator.height = secondBoardSize;
                    break;
                default:
                    int growthTier = GetBoardGrowthTier(level);
                    LineGenerator.width = baseBoardWidth + growthTier * boardGrowthStep;
                    LineGenerator.height = baseBoardHeight + growthTier * boardGrowthStep;
                    break;
            }
        }

        private int GetBoardGrowthTier(int level)
        {
            if (level < boardGrowthStartLevel)
                return 0;

            return ((level - boardGrowthStartLevel) / boardGrowthInterval) + 1;
        }

        private void ConfigureSeedAndBoard(int level)
        {
            if (IsTutorialMode)
            {
                ConfigureTutorialBoard();
                return;
            }

            if (IsChallengeMode)
            {
                if (levelText != null)
                    levelText.text = string.Empty;

                Random.InitState(GameDataStore.GetCurrentChallengeSeed(System.DateTime.UtcNow, challengeSeedOffset));

                if (challengePatternLibrary == null)
                    challengePatternLibrary = GetComponent<ChallengePatternLibrary>();

                if (challengePatternLibrary != null && challengePatternLibrary.TryBuildCurrentWeeklyMask(out bool[,] challengeMask))
                {
                    LineGenerator.SetPlayableMask(challengeMask);
                }
                else
                {
                    LineGenerator.ClearPlayableMask();
                    LineGenerator.width = challengeBoardWidth;
                    LineGenerator.height = challengeBoardHeight;
                }
                return;
            }

            LineGenerator.ClearPlayableMask();

            if (levelText != null)
                levelText.text = "Level " + level;
            Random.InitState(levelSeed[(level - 1) % levelSeed.Count]);
            ConfigureBoardForLevel(level);
        }

        private void ConfigureModeSpecificUi()
        {
            if (IsTutorialMode)
            {
                ApplyTutorialUiState();
                return;
            }

            if (restartButton != null && IsChallengeMode)
                restartButton.gameObject.SetActive(true);
        }

        private void FitBoardToCameraFromCurrentBoard()
        {
            LineGenerator.GetBoardBounds(out Vector2 minBounds, out Vector2 maxBounds);
            FitBoardToCamera(minBounds, maxBounds);
        }

        private void FitBoardToCamera(Vector2 minBounds, Vector2 maxBounds)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || !mainCamera.orthographic)
                return;

            boardWorldMin = minBounds;
            boardWorldMax = maxBounds;

            float boardWidth = boardWorldMax.x - boardWorldMin.x + 1f;
            float boardHeight = boardWorldMax.y - boardWorldMin.y + 1f;
            float halfHeightWithPadding = boardHeight * 0.5f + cameraBoardPadding;
            float halfWidthWithPadding = (boardWidth * 0.5f + cameraBoardPadding) / mainCamera.aspect;
            fittedCameraSize = Mathf.Max(halfHeightWithPadding, halfWidthWithPadding, minimumCameraSize);
            minAllowedCameraSize = Mathf.Min(fittedCameraSize, GetMinimumZoomInCameraSize(mainCamera));
            maxAllowedCameraSize = fittedCameraSize * maxZoomRatio;

            Vector2 boardCenter = (boardWorldMin + boardWorldMax) * 0.5f;
            fittedCameraPosition = new Vector3(boardCenter.x, boardCenter.y, mainCamera.transform.position.z);
            mainCamera.transform.position = fittedCameraPosition;
            mainCamera.orthographicSize = fittedCameraSize;
            SyncCameraPanState(fittedCameraPosition);
            SyncCameraZoomState(fittedCameraSize);
        }

        private float GetMinimumZoomInCameraSize(Camera mainCamera)
        {
            float aspect = Mathf.Max(0.01f, mainCamera.aspect);
            float cellSpacing = Mathf.Max(0.01f, GetEffectiveBoardCellSpacing());
            float visibleRows = Mathf.Max(2, maxZoomVisibleGridRows) - 1 + zoomInCellPadding;
            float visibleColumns = Mathf.Max(2, maxZoomVisibleGridColumns) - 1 + zoomInCellPadding;

            float sizeFromRows = visibleRows * cellSpacing * 0.5f;
            float sizeFromColumns = visibleColumns * cellSpacing * 0.5f / aspect;
            float desiredZoomSize = Mathf.Min(sizeFromRows, sizeFromColumns);
            float ratioClampedSize = fittedCameraSize * minZoomRatio;

            return Mathf.Max(absoluteMinimumZoomSize, Mathf.Min(ratioClampedSize, desiredZoomSize));
        }

        private float GetEffectiveBoardCellSpacing()
        {
            if (LineGenerator == null)
                return 1f;

            return Mathf.Max(0.01f, LineGenerator.CurrentRenderCellSpacing);
        }

        private float GetMaximumCameraSizeForVisibleGrid(Camera mainCamera, int visibleRows, int visibleColumns, float extraPadding)
        {
            float aspect = Mathf.Max(0.01f, mainCamera.aspect);
            float cellSpacing = Mathf.Max(0.01f, GetEffectiveBoardCellSpacing());
            float rows = Mathf.Max(2, visibleRows) - 1 + extraPadding;
            float columns = Mathf.Max(2, visibleColumns) - 1 + extraPadding;

            float sizeFromRows = rows * cellSpacing * 0.5f;
            float sizeFromColumns = columns * cellSpacing * 0.5f / aspect;
            return Mathf.Max(absoluteMinimumZoomSize, Mathf.Min(sizeFromRows, sizeFromColumns));
        }

        private bool TryGetInitialPlayableCameraSize(Camera mainCamera, out float targetSize)
        {
            targetSize = fittedCameraSize;
            if (mainCamera == null || !mainCamera.orthographic)
                return false;

            float boardWidth = boardWorldMax.x - boardWorldMin.x + 1f;
            float boardHeight = boardWorldMax.y - boardWorldMin.y + 1f;
            if (boardWidth <= initialVisibleGridColumns && boardHeight <= initialVisibleGridRows)
                return false;

            float desiredSize = GetMaximumCameraSizeForVisibleGrid(mainCamera, initialVisibleGridRows, initialVisibleGridColumns, initialGridZoomCellPadding);
            targetSize = Mathf.Clamp(desiredSize, minAllowedCameraSize, fittedCameraSize);
            return targetSize < fittedCameraSize - 0.01f;
        }

        private void FocusCameraOnHintLine(LineController line)
        {
            if (line == null)
                return;

            Camera mainCamera = Camera.main;
            if (mainCamera == null || !mainCamera.orthographic)
                return;

            Bounds hintBounds = line.GetWorldBounds();
            float aspect = Mathf.Max(0.01f, mainCamera.aspect);
            float cellSpacing = Mathf.Max(0.01f, GetEffectiveBoardCellSpacing());
            float gridTargetSize = GetMaximumCameraSizeForVisibleGrid(mainCamera, hintFocusVisibleGridRows, hintFocusVisibleGridColumns, hintFocusCellPadding);
            float sizeFromBoundsHeight = hintBounds.extents.y + cellSpacing * hintFocusCellPadding;
            float sizeFromBoundsWidth = (hintBounds.extents.x + cellSpacing * hintFocusCellPadding) / aspect;
            float targetSize = Mathf.Max(sizeFromBoundsHeight, sizeFromBoundsWidth, gridTargetSize);
            targetSize = Mathf.Clamp(targetSize, minAllowedCameraSize, fittedCameraSize);

            Vector3 targetPosition = new(hintBounds.center.x, hintBounds.center.y, fittedCameraPosition.z);
            if (hintFocusCoroutine != null)
                StopCoroutine(hintFocusCoroutine);
            hintFocusCoroutine = StartCoroutine(FocusCameraOnHintLineCO(targetPosition, targetSize));
        }

        private IEnumerator FocusCameraOnHintLineCO(Vector3 targetPosition, float targetSize)
        {
            yield return AnimateCameraToPositionAndSize(targetPosition, targetSize, hintFocusDuration);
            hintFocusCoroutine = null;
        }

        private void SyncCameraPanState(Vector3 position)
        {
            targetCameraPanPosition = new Vector3(position.x, position.y, fittedCameraPosition.z);
            cameraPanVelocity = Vector3.zero;
            hasTargetCameraPanPosition = true;
        }

        private void SyncCameraZoomState(float orthographicSize)
        {
            targetCameraZoomSize = orthographicSize;
            cameraZoomVelocity = 0f;
            hasTargetCameraZoomSize = true;
        }

        private void SmoothCameraTowardTarget(Camera mainCamera)
        {
            if (mainCamera == null)
                return;

            Vector3 clampedTarget = ClampCameraPosition(mainCamera, targetCameraPanPosition);
            if (panSmoothTime <= 0f)
            {
                mainCamera.transform.position = clampedTarget;
                cameraPanVelocity = Vector3.zero;
            }
            else
            {
                Vector3 smoothedPosition = Vector3.SmoothDamp(mainCamera.transform.position, clampedTarget, ref cameraPanVelocity, panSmoothTime);
                smoothedPosition = ClampCameraPosition(mainCamera, smoothedPosition);
                mainCamera.transform.position = smoothedPosition;

                if ((mainCamera.transform.position - clampedTarget).sqrMagnitude <= 0.0001f)
                {
                    mainCamera.transform.position = clampedTarget;
                    cameraPanVelocity = Vector3.zero;
                }
            }

            if (!hasTargetCameraZoomSize)
                SyncCameraZoomState(mainCamera.orthographicSize);

            float clampedZoomTarget = Mathf.Clamp(targetCameraZoomSize, minAllowedCameraSize, maxAllowedCameraSize);
            if (zoomSmoothTime <= 0f)
            {
                mainCamera.orthographicSize = clampedZoomTarget;
                cameraZoomVelocity = 0f;
            }
            else
            {
                float smoothedZoom = Mathf.SmoothDamp(mainCamera.orthographicSize, clampedZoomTarget, ref cameraZoomVelocity, zoomSmoothTime);
                mainCamera.orthographicSize = Mathf.Clamp(smoothedZoom, minAllowedCameraSize, maxAllowedCameraSize);

                if (Mathf.Abs(mainCamera.orthographicSize - clampedZoomTarget) <= 0.001f)
                {
                    mainCamera.orthographicSize = clampedZoomTarget;
                    cameraZoomVelocity = 0f;
                }
            }

            mainCamera.transform.position = ClampCameraPosition(mainCamera, mainCamera.transform.position);
        }

        private void RefreshProgress()
        {
            if (progressSlider == null)
                return;

            PruneNullLineReferences();

            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;

            if (totalLineCount <= 0)
            {
                progressSlider.value = 0f;
                return;
            }

            int removedLines = totalLineCount - lines.Count;
            progressSlider.value = Mathf.Clamp01((float)removedLines / totalLineCount);
        }

        private void PruneNullLineReferences()
        {
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (lines[i] == null)
                    lines.RemoveAt(i);
            }
        }

        private void PlayDamageFlash()
        {
            if (damageOverlay == null)
                return;

            if (damageFlashCoroutine != null)
                StopCoroutine(damageFlashCoroutine);

            damageFlashCoroutine = StartCoroutine(DamageFlashCO());
        }

        private IEnumerator DamageFlashCO()
        {
            float elapsed = 0f;
            while (elapsed < damageFlashDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / damageFlashDuration);
                float alpha = Mathf.Lerp(damageFlashAlpha, 0f, normalizedTime);
                SetDamageOverlayVisible(alpha);
                yield return null;
            }

            SetDamageOverlayVisible(0f);
            damageFlashCoroutine = null;
        }

        private void SetDamageOverlayVisible(float alpha)
        {
            if (damageOverlay == null)
                return;

            Color color = damageOverlay.color;
            color.a = alpha;
            damageOverlay.color = color;
            damageOverlay.raycastTarget = false;
        }

        private void PlayWinMessageAnimation()
        {
            if (winMessageText == null)
                return;

            if (winMessageCoroutine != null)
                StopCoroutine(winMessageCoroutine);

            winMessageText.text = winMessages[Random.Range(0, winMessages.Length)];
            SetWinMessageAlpha(0f);
            winMessageCoroutine = StartCoroutine(AnimateWinMessageFadeIn());
        }

        private IEnumerator AnimateWinMessageFadeIn()
        {
            if (winMessageText == null)
                yield break;

            if (winMessageFadeDuration <= 0f)
            {
                SetWinMessageAlpha(1f);
                winMessageCoroutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < winMessageFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / winMessageFadeDuration);
                SetWinMessageAlpha(t);
                yield return null;
            }

            SetWinMessageAlpha(1f);
            winMessageCoroutine = null;
        }

        private void SetWinMessageAlpha(float alpha)
        {
            if (winMessageText == null)
                return;

            Color color = winMessageText.color;
            color.a = alpha;
            winMessageText.color = color;
        }

        private void RefreshHintAmountText()
        {
            if (hintAmountText != null)
                hintAmountText.text = GameDataStore.HintCount.ToString();
        }

        private void ConfigureTutorialBoard()
        {
            LineGenerator.ClearPlayableMask();

            int tutorialIndex = Mathf.Clamp(currentTutorialStep, 0, TutorialBoardSizes.Length - 1);
            Vector2Int boardSize = TutorialBoardSizes[tutorialIndex];
            LineGenerator.width = boardSize.x;
            LineGenerator.height = boardSize.y;

            Random.InitState(TutorialSeeds[tutorialIndex]);
            ApplyTutorialUiState();
        }

        private void ApplyTutorialUiState()
        {
            SetGameObjectActive(hearts != null && hearts.Length > 0 && hearts[0] != null ? hearts[0].transform.parent.gameObject : null, false);
            SetGameObjectActive(hintButtonRect != null ? hintButtonRect.gameObject : null, false);
            SetGameObjectActive(noHintsPanelRect != null ? noHintsPanelRect.gameObject : null, false);
            SetGameObjectActive(restartButton != null ? restartButton.gameObject : null, false);
            SetGameObjectActive(reviveButton != null ? reviveButton.gameObject : null, false);
            SetGameObjectActive(mainMenuButton != null ? mainMenuButton.gameObject : null, false);
            SetGameObjectActive(guideToggleButton != null ? guideToggleButton.gameObject : null, false);
            SetGameObjectActive(quitConfirmationPanelRect != null ? quitConfirmationPanelRect.gameObject : null, false);
            SetGameObjectActive(gameplayPurchasePopupRect != null ? gameplayPurchasePopupRect.gameObject : null, false);
            SetGameObjectActive(winMessageText != null ? winMessageText.gameObject : null, false);
            ShowQuitPanel(false, false);

            if (levelText != null)
            {
                levelText.fontSize = tutorialHeaderFontSize;
                levelText.alignment = TextAlignmentOptions.Center;
                levelText.text = $"TRIAL {currentTutorialStep + 1}/{TutorialBoardSizes.Length}\nComplete the trail\nEscape the arrow";
            }
        }

        private IEnumerator HandleTutorialBoardCompleted()
        {
            yield return new WaitForSeconds(tutorialAdvanceDelay);

            if (currentTutorialStep < TutorialBoardSizes.Length - 1)
            {
                currentTutorialStep++;
                LoadTutorialBoard();
                yield break;
            }

            GameDataStore.MarkTutorialCompleted();
            SceneManager.LoadScene(MenuSceneName);
        }

        private void LoadTutorialBoard()
        {
            hasDraggedCurrentTouch = false;
            isTransitioningToWin = false;
            isLevelIntroPlaying = false;
            hasStartedGameplayIntro = false;
            externalInputLocked = true;
            guideLinesVisible = false;

            if (winUI != null)
                winUI.SetActive(false);

            if (loseUI != null)
                loseUI.SetActive(false);

            HideTransientGameplayUi();
            SetDamageOverlayVisible(0f);

            ConfigureSeedAndBoard(currentTutorialStep + 1);
            LineGenerator.GenerateBoard();
            RefreshBoardAfterGeneration();
            ApplyTheme();
            StartCoroutine(BeginGameplayIntroCO());
        }

        private static void SetGameObjectActive(GameObject target, bool isActive)
        {
            if (target != null)
                target.SetActive(isActive);
        }

        private void ResetHintIdleTimer()
        {
            idleSinceLastRemoval = 0f;
        }

        private void HideHintUi(bool animateHint = true)
        {
            SetHintVisible(false, animateHint);
            HideNoHintsPanel(false);
        }

        private void HideTransientGameplayUi(bool animateHint = false, bool animateQuitPanel = false)
        {
            HideHintUi(animateHint);
            ShowQuitPanel(false, animateQuitPanel);
            if (gameplayPurchasePopupRect != null)
                gameplayPurchasePopupRect.gameObject.SetActive(false);
        }

        private void BlockTouchTapAfterGesture()
        {
            touchTapBlockedUntilTime = Mathf.Max(touchTapBlockedUntilTime, Time.unscaledTime + pinchTapBlockDuration);
        }

        private bool IsTouchTapBlocked()
        {
            return Time.unscaledTime < touchTapBlockedUntilTime;
        }

        private void UpdateHintButtonVisibility()
        {
            if (hintButtonRect == null || lines.Count == 0)
                return;

            idleSinceLastRemoval += Time.deltaTime;
            bool shouldBeVisible = idleSinceLastRemoval >= hintRevealDelay;
            SetHintVisible(shouldBeVisible);
        }

        private void SetHintVisible(bool visible, bool animate = true)
        {
            if (hintButtonRect == null || hintVisible == visible)
                return;

            hintVisible = visible;

            if (hintSlideCoroutine != null)
                StopCoroutine(hintSlideCoroutine);

            Vector2 targetPosition = visible ? hintVisibleAnchoredPosition : hintHiddenAnchoredPosition;
            if (!animate || hintSlideDuration <= 0f)
            {
                hintButtonRect.anchoredPosition = targetPosition;
                hintSlideCoroutine = null;
                return;
            }

            hintSlideCoroutine = StartCoroutine(AnimateHintButton(targetPosition));
        }

        private IEnumerator AnimateHintButton(Vector2 targetPosition)
        {
            Vector2 startPosition = hintButtonRect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < hintSlideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / hintSlideDuration);
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                hintButtonRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);
                yield return null;
            }

            hintButtonRect.anchoredPosition = targetPosition;
            hintSlideCoroutine = null;
        }

        private void ShowNoHintsPanel()
        {
            if (noHintsPanelRect == null)
                return;

            if (noHintsPanelCoroutine != null)
                StopCoroutine(noHintsPanelCoroutine);

            noHintsPanelCoroutine = StartCoroutine(ShowNoHintsPanelCO());
        }

        private void ShowQuitPanel(bool visible, bool animate = true)
        {
            if (quitConfirmationPanelRect == null || isQuitPanelVisible == visible)
                return;

            isQuitPanelVisible = visible;

            if (quitPanelCoroutine != null)
                StopCoroutine(quitPanelCoroutine);

            if (visible)
            {
                quitPanelCoroutine = StartCoroutine(ShowQuitPanelCO(animate));
                return;
            }

            quitPanelCoroutine = StartCoroutine(HideQuitPanelCO(animate));
        }

        private void HideNoHintsPanel(bool animate = true)
        {
            if (noHintsPanelRect == null)
                return;

            if (noHintsPanelCoroutine != null)
                StopCoroutine(noHintsPanelCoroutine);

            if (!animate || noHintsPanelSlideDuration <= 0f)
            {
                noHintsPanelRect.anchoredPosition = noHintsPanelHiddenAnchoredPosition;
                noHintsPanelRect.gameObject.SetActive(false);
                noHintsPanelCoroutine = null;
                return;
            }

            noHintsPanelCoroutine = StartCoroutine(HideNoHintsPanelCO());
        }

        private IEnumerator ShowNoHintsPanelCO()
        {
            noHintsPanelRect.gameObject.SetActive(true);
            yield return AnimateRectTransform(noHintsPanelRect, noHintsPanelVisibleAnchoredPosition, noHintsPanelSlideDuration);
            yield return new WaitForSeconds(noHintsPanelDisplayDuration);
            yield return HideNoHintsPanelCO();
            noHintsPanelCoroutine = null;
        }

        private IEnumerator HideNoHintsPanelCO()
        {
            yield return AnimateRectTransform(noHintsPanelRect, noHintsPanelHiddenAnchoredPosition, noHintsPanelSlideDuration);
            noHintsPanelRect.gameObject.SetActive(false);
            noHintsPanelCoroutine = null;
        }

        private IEnumerator ShowQuitPanelCO(bool animate)
        {
            quitConfirmationPanelRect.gameObject.SetActive(true);
            ApplyGameplayThemeOverrides();
            if (animate)
                yield return AnimateRectTransform(quitConfirmationPanelRect, quitPanelVisibleAnchoredPosition, quitPanelSlideDuration);
            else
                quitConfirmationPanelRect.anchoredPosition = quitPanelVisibleAnchoredPosition;

            quitPanelCoroutine = null;
        }

        private IEnumerator HideQuitPanelCO(bool animate)
        {
            if (animate)
                yield return AnimateRectTransform(quitConfirmationPanelRect, quitPanelHiddenAnchoredPosition, quitPanelSlideDuration);
            else
                quitConfirmationPanelRect.anchoredPosition = quitPanelHiddenAnchoredPosition;

            quitConfirmationPanelRect.gameObject.SetActive(false);
            quitPanelCoroutine = null;
        }

        private static IEnumerator AnimateRectTransform(RectTransform rectTransform, Vector2 targetPosition, float duration)
        {
            if (rectTransform == null)
                yield break;

            if (duration <= 0f)
            {
                rectTransform.anchoredPosition = targetPosition;
                yield break;
            }

            Vector2 startPosition = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);
                yield return null;
            }

            rectTransform.anchoredPosition = targetPosition;
        }

        private IEnumerator BeginGameplayIntroCO()
        {
            if (hasStartedGameplayIntro)
                yield break;

            hasStartedGameplayIntro = true;
            isLevelIntroPlaying = true;
            yield return new WaitForSeconds(levelIntroDelay);

            LineController[] introLines = FindObjectsByType<LineController>(FindObjectsSortMode.None);
            foreach (LineController line in introLines)
            {
                StartCoroutine(line.PlayIntroAnimation(levelIntroDuration));
            }

            yield return new WaitForSeconds(levelIntroDuration);
            Camera mainCamera = Camera.main;
            if (TryGetInitialPlayableCameraSize(mainCamera, out float initialPlayableCameraSize))
                yield return AnimateCameraToPositionAndSize(fittedCameraPosition, initialPlayableCameraSize, initialGridZoomDuration);

            isLevelIntroPlaying = false;
            externalInputLocked = false;
        }

        private void HandlePinchZoom()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || !mainCamera.orthographic)
                return;

            if (Input.touchCount == 2)
            {
                hasDraggedCurrentTouch = true;
                BlockTouchTapAfterGesture();

                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 previousTouchZeroPosition = touchZero.position - touchZero.deltaPosition;
                Vector2 previousTouchOnePosition = touchOne.position - touchOne.deltaPosition;

                float previousTouchDistance = Vector2.Distance(previousTouchZeroPosition, previousTouchOnePosition);
                float currentTouchDistance = Vector2.Distance(touchZero.position, touchOne.position);
                float safePreviousDistance = Mathf.Max(1f, previousTouchDistance);
                float safeCurrentDistance = Mathf.Max(1f, currentTouchDistance);
                float distanceDelta = safeCurrentDistance - safePreviousDistance;

                if (!Mathf.Approximately(distanceDelta, 0f))
                {
                    float zoomBaseSize = hasTargetCameraZoomSize ? targetCameraZoomSize : mainCamera.orthographicSize;
                    float linearTargetSize = zoomBaseSize - distanceDelta * pinchZoomSpeed;
                    float scaleRatio = safePreviousDistance / safeCurrentDistance;
                    float scaledTargetSize = zoomBaseSize * Mathf.Pow(scaleRatio, Mathf.Max(0.01f, pinchZoomResponse));
                    float targetSize = Mathf.Lerp(linearTargetSize, scaledTargetSize, 0.85f);
                    SyncCameraZoomState(Mathf.Clamp(targetSize, minAllowedCameraSize, maxAllowedCameraSize));
                    Vector3 clampedPosition = ClampCameraPosition(mainCamera, mainCamera.transform.position);
                    SyncCameraPanState(clampedPosition);
                    SmoothCameraTowardTarget(mainCamera);
                }

                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            float scrollDelta = Input.mouseScrollDelta.y;
            if (!Mathf.Approximately(scrollDelta, 0f))
            {
                float zoomBaseSize = hasTargetCameraZoomSize ? targetCameraZoomSize : mainCamera.orthographicSize;
                float scrollTargetSize = zoomBaseSize * Mathf.Exp(-scrollDelta * editorScrollZoomSpeed * 0.12f);
                SyncCameraZoomState(Mathf.Clamp(scrollTargetSize, minAllowedCameraSize, maxAllowedCameraSize));
                Vector3 clampedPosition = ClampCameraPosition(mainCamera, mainCamera.transform.position);
                SyncCameraPanState(clampedPosition);
                SmoothCameraTowardTarget(mainCamera);
            }
#endif
        }

        private void HandleCameraPan()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || !mainCamera.orthographic)
                return;

            if (!hasTargetCameraPanPosition)
                SyncCameraPanState(mainCamera.transform.position);

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        singleTouchStartScreenPosition = touch.position;
                        singleTouchStartCameraPosition = hasTargetCameraPanPosition ? targetCameraPanPosition : mainCamera.transform.position;
                        hasDraggedCurrentTouch = false;
                        cameraPanVelocity = Vector3.zero;
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (!CanPan(mainCamera))
                            break;

                        Vector2 screenDelta = touch.position - singleTouchStartScreenPosition;
                        if (!hasDraggedCurrentTouch && screenDelta.sqrMagnitude >= dragThresholdPixels * dragThresholdPixels)
                            hasDraggedCurrentTouch = true;

                        if (!hasDraggedCurrentTouch)
                            break;

                        Vector3 worldStart = mainCamera.ScreenToWorldPoint(new Vector3(singleTouchStartScreenPosition.x, singleTouchStartScreenPosition.y, -mainCamera.transform.position.z));
                        Vector3 worldCurrent = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, -mainCamera.transform.position.z));
                        Vector3 worldDelta = (worldStart - worldCurrent) * Mathf.Max(0.01f, panSensitivity);
                        Vector3 targetPosition = singleTouchStartCameraPosition + new Vector3(worldDelta.x, worldDelta.y, 0f);
                        targetCameraPanPosition = ClampCameraPosition(mainCamera, targetPosition);
                        SmoothCameraTowardTarget(mainCamera);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        break;
                }

                return;
            }

            if (CanPan(mainCamera))
                SmoothCameraTowardTarget(mainCamera);
            else
            {
                SyncCameraPanState(fittedCameraPosition);
                SyncCameraZoomState(fittedCameraSize);
                mainCamera.transform.position = fittedCameraPosition;
                mainCamera.orthographicSize = fittedCameraSize;
            }

            if (Input.touchCount == 0)
                hasDraggedCurrentTouch = false;
        }

        private bool CanPan(Camera mainCamera)
        {
            return mainCamera.orthographicSize < fittedCameraSize - 0.01f;
        }

        private Vector3 ClampCameraPosition(Camera mainCamera, Vector3 targetPosition)
        {
            float minX = boardWorldMin.x;
            float maxX = boardWorldMax.x;
            float minY = boardWorldMin.y;
            float maxY = boardWorldMax.y;

            float clampedX = minX > maxX ? fittedCameraPosition.x : Mathf.Clamp(targetPosition.x, minX, maxX);
            float clampedY = minY > maxY ? fittedCameraPosition.y : Mathf.Clamp(targetPosition.y, minY, maxY);

            return new Vector3(clampedX, clampedY, fittedCameraPosition.z);
        }

        private void Update()
        {
            if (IsInputLocked)
                return;

            HandlePinchZoom();
            HandleCameraPan();
            UpdateHintButtonVisibility();

            // debug code, you can add your own
#if DEBUG
            if (Input.GetKeyDown(KeyCode.R))
            {
                Retry();
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                GameDataStore.Level++;
                Retry();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                GameDataStore.Level--;
                Retry();
            }
#endif
        }

        private IEnumerator AnimateCameraToFittedView()
        {
            yield return AnimateCameraToPositionAndSize(fittedCameraPosition, fittedCameraSize, winCameraResetDuration);
        }

        private IEnumerator AnimateCameraToPositionAndSize(Vector3 targetPosition, float targetSize, float duration)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || !mainCamera.orthographic)
                yield break;

            Vector3 startPosition = mainCamera.transform.position;
            float startSize = mainCamera.orthographicSize;
            Vector3 clampedTargetPosition = ClampCameraPosition(mainCamera, targetPosition);

            if (duration <= 0f)
            {
                mainCamera.transform.position = clampedTargetPosition;
                mainCamera.orthographicSize = targetSize;
                SyncCameraPanState(clampedTargetPosition);
                SyncCameraZoomState(targetSize);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                mainCamera.transform.position = Vector3.Lerp(startPosition, clampedTargetPosition, easedT);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, easedT);
                yield return null;
            }

            mainCamera.transform.position = clampedTargetPosition;
            mainCamera.orthographicSize = targetSize;
            SyncCameraPanState(clampedTargetPosition);
            SyncCameraZoomState(targetSize);
        }
    }
}





