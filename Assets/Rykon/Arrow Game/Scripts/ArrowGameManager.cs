using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private RectTransform quitConfirmationPanelRect;
        [SerializeField] private Button quitConfirmYesButton;
        [SerializeField] private Button quitConfirmNoButton;
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
        [SerializeField] private float pinchZoomSpeed = 0.02f;
        [SerializeField] private float minZoomRatio = 0.7f;
        [SerializeField] private float maxZoomRatio = 1.35f;
        [SerializeField] private int maxZoomVisibleGridRows = 20;
        [SerializeField] private int maxZoomVisibleGridColumns = 15;
        [SerializeField] private float zoomInCellPadding = 1.5f;
        [SerializeField] private float absoluteMinimumZoomSize = 2.5f;
        [SerializeField] private float editorScrollZoomSpeed = 0.75f;
        [SerializeField] private float dragThresholdPixels = 12f;
        [SerializeField] private float winCameraResetDuration = 0.45f;
        [SerializeField] private float winLeadInDelay = 0.35f;
        [SerializeField] private float winLastArrowSettleDuration = 0.35f;
        [SerializeField] private float winPanelDelay = 0.2f;
        [SerializeField] private float levelIntroDelay = 0.12f;
        [SerializeField] private float levelIntroDuration = 0.85f;
        [SerializeField] private Vector2 challengeLoseRetryButtonPosition = new(-120f, -250f);
        [SerializeField] private Vector2 challengeLoseMainMenuButtonPosition = new(120f, -250f);
        [SerializeField] private Vector2 challengeLoseButtonSize = new(260f, 84f);
        [SerializeField] private float tutorialAdvanceDelay = 0.4f;
        [SerializeField] private float tutorialHeaderFontSize = 28f;

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

        public event UnityAction ChallengeCompleted;
        public event UnityAction ChallengeFailed;

        public bool CanProcessTouchTap => Input.touchCount == 1 && !hasDraggedCurrentTouch;
        public bool HasDraggedCurrentTouch => hasDraggedCurrentTouch;
        public bool IsInputLocked => externalInputLocked || isTransitioningToWin || isLevelIntroPlaying || isQuitPanelVisible || loseUI.activeSelf || winUI.activeSelf;
        public GameplayMode CurrentGameplayMode => gameplayMode;
        public bool IsChallengeMode => gameplayMode == GameplayMode.Challenge;
        public bool IsTutorialMode => gameplayMode == GameplayMode.Tutorial || gameObject.scene.name == TutorialSceneName;

        private void Awake()
        {
            Instance = this;
            EnsureQuitConfirmationUi();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            EnsureQuitConfirmationUi();
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
                ShowNoHintsPanel();
                return;
            }

            RefreshHintAmountText();
            SetHintVisible(false);
            removableLine.ShowHint();
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

            HapticManager.PlayFailure();
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

            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(true);
                restartButton.interactable = retryAvailable;
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.gameObject.SetActive(true);
                mainMenuButton.interactable = true;
            }

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

            HideNoHintsPanel(false);
            SetHintVisible(false);
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
            SetHintVisible(false, false);
            HideNoHintsPanel(false);
            ShowQuitPanel(false, false);
            SoundManager.PlayLose();
            loseUI.SetActive(true);

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

            ConfigureChallengeLoseButton(restartButton, challengeLoseRetryButtonPosition, "Retry");
            ConfigureChallengeLoseButton(mainMenuButton, challengeLoseMainMenuButtonPosition, "Main Menu");
        }

        private void ConfigureChallengeLoseButton(Button button, Vector2 anchoredPosition, string labelText)
        {
            if (button == null)
                return;

            button.gameObject.SetActive(true);
            button.transform.SetAsLastSibling();

            if (button.transform is RectTransform rectTransform)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.sizeDelta = challengeLoseButtonSize;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null && !string.IsNullOrWhiteSpace(labelText))
                label.text = labelText;
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
            SetHintVisible(false, false);
            HideNoHintsPanel(false);
            ShowQuitPanel(false, false);
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
            RegisterButton(restartButton, Retry);
            RegisterButton(mainMenuButton, OpenQuitConfirmation);
            RegisterButton(quitConfirmYesButton, ConfirmQuitToMenu);
            RegisterButton(quitConfirmNoButton, CancelQuitToMenu);
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

        private RectTransform CreateQuitConfirmationPanel(Transform parent)
        {
            RectTransform panel = CreateUiRect("Quit Confirmation Panel", parent);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = quitPanelSize;
            panel.anchoredPosition = Vector2.zero;

            Image panelImage = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
            if (panelImage.sprite == null)
                panelImage.sprite = GetDefaultUiSprite();
            panelImage.type = Image.Type.Sliced;
            panelImage.color = new Color(1f, 1f, 1f, 0.96f);

            CreateUiLabel(panel, "Quit Title", "Quit Level?", 42f, new Vector2(0f, 88f), new Vector2(420f, 56f));
            CreateUiLabel(panel, "Quit Message", "Are you sure you want to go back to the main menu?", 24f, new Vector2(0f, 24f), new Vector2(520f, 72f));

            CreateUiButton(panel, "Quit Confirm Yes Button", "Yes", new Vector2(-120f, -92f), quitPanelButtonSize);
            CreateUiButton(panel, "Quit Confirm No Button", "No", new Vector2(120f, -92f), quitPanelButtonSize);
            panel.gameObject.SetActive(false);
            return panel;
        }

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

#if UNITY_EDITOR
        private static void AssignEditorReference(UnityEditor.SerializedObject serializedObject, string propertyName, Object value)
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
                ? new Color(0.12f, 0.16f, 0.22f, 0.96f)
                : new Color(1f, 1f, 1f, 0.96f);

            ThemeManager.ApplyButtonTheme(restartButton, iconButtonColor, Color.white, disabledButtonColor, palette.TextSecondaryColor, true);
            ThemeManager.ApplyButtonTheme(mainMenuButton, iconButtonColor, Color.white, disabledButtonColor, palette.TextSecondaryColor, true);
            ThemeManager.ApplyButtonTheme(quitConfirmYesButton, palette.AccentColor, Color.white, disabledButtonColor, palette.TextSecondaryColor);
            ThemeManager.ApplyButtonTheme(quitConfirmNoButton, neutralButtonColor, Color.white, disabledButtonColor, palette.TextSecondaryColor);

            if (hintButtonRect != null)
            {
                Button hintButton = hintButtonRect.GetComponent<Button>();
                ThemeManager.ApplyButtonTheme(hintButton, hintButtonColor, Color.white, hintButtonColor, Color.white);
            }

            if (noHintsPanelRect != null && noHintsPanelRect.TryGetComponent(out Image noHintsImage))
                noHintsImage.color = panelColor;

            if (quitConfirmationPanelRect != null && quitConfirmationPanelRect.TryGetComponent(out Image quitPanelImage))
                quitPanelImage.color = panelColor;

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
            SetGameObjectActive(mainMenuButton != null ? mainMenuButton.gameObject : null, false);
            SetGameObjectActive(guideToggleButton != null ? guideToggleButton.gameObject : null, false);
            SetGameObjectActive(quitConfirmationPanelRect != null ? quitConfirmationPanelRect.gameObject : null, false);
            SetGameObjectActive(winMessageText != null ? winMessageText.gameObject : null, false);
            ShowQuitPanel(false, false);

            if (levelText != null)
            {
                levelText.fontSize = tutorialHeaderFontSize;
                levelText.alignment = TextAlignmentOptions.Center;
                levelText.text = $"TUTORIAL {currentTutorialStep + 1}/{TutorialBoardSizes.Length}\nComplete the tutorial\nEscape the arrow";
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

            SetHintVisible(false, false);
            HideNoHintsPanel(false);
            ShowQuitPanel(false, false);
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
            isLevelIntroPlaying = false;
            externalInputLocked = false;
        }

        private static void RegisterButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private void HandlePinchZoom()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || !mainCamera.orthographic)
                return;

            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 previousTouchZeroPosition = touchZero.position - touchZero.deltaPosition;
                Vector2 previousTouchOnePosition = touchOne.position - touchOne.deltaPosition;

                float previousTouchDistance = Vector2.Distance(previousTouchZeroPosition, previousTouchOnePosition);
                float currentTouchDistance = Vector2.Distance(touchZero.position, touchOne.position);
                float distanceDelta = currentTouchDistance - previousTouchDistance;

                if (!Mathf.Approximately(distanceDelta, 0f))
                {
                    float targetSize = mainCamera.orthographicSize - distanceDelta * pinchZoomSpeed;
                    mainCamera.orthographicSize = Mathf.Clamp(targetSize, minAllowedCameraSize, maxAllowedCameraSize);
                    mainCamera.transform.position = ClampCameraPosition(mainCamera, mainCamera.transform.position);
                }

                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            float scrollDelta = Input.mouseScrollDelta.y;
            if (!Mathf.Approximately(scrollDelta, 0f))
            {
                float scrollTargetSize = mainCamera.orthographicSize - scrollDelta * editorScrollZoomSpeed;
                mainCamera.orthographicSize = Mathf.Clamp(scrollTargetSize, minAllowedCameraSize, maxAllowedCameraSize);
                mainCamera.transform.position = ClampCameraPosition(mainCamera, mainCamera.transform.position);
            }
#endif
        }

        private void HandleCameraPan()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || !mainCamera.orthographic)
                return;

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        singleTouchStartScreenPosition = touch.position;
                        singleTouchStartCameraPosition = mainCamera.transform.position;
                        hasDraggedCurrentTouch = false;
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
                        Vector3 worldDelta = worldStart - worldCurrent;
                        Vector3 targetPosition = singleTouchStartCameraPosition + new Vector3(worldDelta.x, worldDelta.y, 0f);
                        mainCamera.transform.position = ClampCameraPosition(mainCamera, targetPosition);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        break;
                }

                return;
            }

            hasDraggedCurrentTouch = false;
        }

        private bool CanPan(Camera mainCamera)
        {
            return mainCamera.orthographicSize < fittedCameraSize - 0.01f;
        }

        private Vector3 ClampCameraPosition(Camera mainCamera, Vector3 targetPosition)
        {
            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = mainCamera.aspect * halfHeight;

            float minX = boardWorldMin.x + halfWidth;
            float maxX = boardWorldMax.x - halfWidth;
            float minY = boardWorldMin.y + halfHeight;
            float maxY = boardWorldMax.y - halfHeight;

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
            Camera mainCamera = Camera.main;
            if (mainCamera == null || !mainCamera.orthographic)
                yield break;

            Vector3 startPosition = mainCamera.transform.position;
            float startSize = mainCamera.orthographicSize;
            float elapsed = 0f;

            while (elapsed < winCameraResetDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / winCameraResetDuration);
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                mainCamera.transform.position = Vector3.Lerp(startPosition, fittedCameraPosition, easedT);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, fittedCameraSize, easedT);
                yield return null;
            }

            mainCamera.transform.position = fittedCameraPosition;
            mainCamera.orthographicSize = fittedCameraSize;
        }
    }
}





