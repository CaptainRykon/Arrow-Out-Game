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
            Challenge = 1
        }

        public static ArrowGameManager Instance;
        private const string MenuSceneName = "MenuScene";

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
        [SerializeField] private float editorScrollZoomSpeed = 0.75f;
        [SerializeField] private float dragThresholdPixels = 12f;
        [SerializeField] private float winCameraResetDuration = 0.45f;
        [SerializeField] private float winLeadInDelay = 0.35f;
        [SerializeField] private float winLastArrowSettleDuration = 0.35f;
        [SerializeField] private float winPanelDelay = 0.2f;
        [SerializeField] private float levelIntroDelay = 0.12f;
        [SerializeField] private float levelIntroDuration = 0.85f;

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

        public event UnityAction ChallengeCompleted;
        public event UnityAction ChallengeFailed;

        public bool CanProcessTouchTap => Input.touchCount == 1 && !hasDraggedCurrentTouch;
        public bool HasDraggedCurrentTouch => hasDraggedCurrentTouch;
        public bool IsInputLocked => externalInputLocked || isTransitioningToWin || isLevelIntroPlaying || isQuitPanelVisible || loseUI.activeSelf || winUI.activeSelf;
        public GameplayMode CurrentGameplayMode => gameplayMode;
        public bool IsChallengeMode => gameplayMode == GameplayMode.Challenge;

        private void Awake()
        {
            Instance = this;
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

            InitializeGuideToggleButton();
            InitializeGameplayButtons();

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
            SetDamageOverlayVisible(0f);
            SetWinMessageAlpha(0f);
            ConfigureModeSpecificUi();

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
            HapticManager.PlayFailure();
            SoundManager.PlayArrowEscapeFail();
            heart--;
            if (heart < 0)
                heart = 0;

            hearts[heart].color = Color.black;
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

        public void OpenQuitConfirmation()
        {
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
                ChallengeFailed?.Invoke();
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
            if (quitConfirmationPanelRect == null)
                return;

            quitPanelVisibleAnchoredPosition = quitConfirmationPanelRect.anchoredPosition;
            quitPanelHiddenAnchoredPosition = quitPanelVisibleAnchoredPosition + Vector2.down * quitPanelHiddenOffsetY;
            quitConfirmationPanelRect.anchoredPosition = quitPanelHiddenAnchoredPosition;
            quitConfirmationPanelRect.gameObject.SetActive(false);
            isQuitPanelVisible = false;
        }

        private void RefreshGuideButtonState()
        {
            if (guideToggleButtonImage == null)
                return;

            guideToggleButtonImage.color = guideLinesVisible ? guideButtonOnColor : guideButtonOffColor;
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
            if (restartButton != null && IsChallengeMode)
                restartButton.gameObject.SetActive(false);
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
            minAllowedCameraSize = Mathf.Max(minimumCameraSize * 0.6f, fittedCameraSize * minZoomRatio);
            maxAllowedCameraSize = fittedCameraSize * maxZoomRatio;

            Vector2 boardCenter = (boardWorldMin + boardWorldMax) * 0.5f;
            fittedCameraPosition = new Vector3(boardCenter.x, boardCenter.y, mainCamera.transform.position.z);
            mainCamera.transform.position = fittedCameraPosition;
            mainCamera.orthographicSize = fittedCameraSize;
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





