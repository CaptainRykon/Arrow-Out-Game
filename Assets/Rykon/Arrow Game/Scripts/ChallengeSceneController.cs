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
    public class ChallengeSceneController : MonoBehaviour
    {
        private const string MenuSceneName = "MenuScene";
        private const int LeaderboardEntryLimit = 25;
        private const float MintStatusDisplaySeconds = 6f;

        [Header("Core")]
        [SerializeField] private ArrowGameManager arrowGameManager;
        [SerializeField] private string challengeTitlePrefix = "Weekly Challenge";
        [SerializeField] private string[] challengePatternNames = { "Star", "Duck", "Bolt", "Crown", "Leaf", "Rocket", "Moon" };

        [Header("Countdown")]
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private float countdownStepDuration = 0.8f;

        [Header("Loading")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI loadingStatusText;
        [SerializeField] private Image loadingProgressFill;
        [SerializeField] private float loadingScreenMinimumSeconds = 0.2f;

        [Header("Run HUD")]
        [SerializeField] private GameObject challengeHudPanel;
        [SerializeField] private TextMeshProUGUI runTimerText;

        [Header("Leaderboard")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private TextMeshProUGUI leaderboardTitleText;
        [SerializeField] private TextMeshProUGUI leaderboardPlayerBestText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private ChallengeLeaderboardEntryView[] leaderboardEntryViews;
        [SerializeField] private Button submitScoreButton;
        [SerializeField] private Button leaderboardMainMenuButton;
        [SerializeField] private GameObject mintStatusPanel;
        [SerializeField] private TextMeshProUGUI mintStatusText;

        private bool runTimerActive;
        private float runTimerStartRealtime;
        private bool hasPendingScoreSubmission;
        private float pendingCompletionSeconds;
        private float pausedRunTimerSeconds;
        private bool challengeStarted;
        private bool isSubmittingScore;
        private Coroutine mintStatusRoutine;

        private void Awake()
        {
            if (arrowGameManager == null)
                arrowGameManager = FindFirstObjectByType<ArrowGameManager>();

            WireButtons();
            PrepareInitialPanels();
            RefreshLeaderboardUi();
            ConfigureChallengeWinUi();
        }

        private void Start()
        {
            StartChallengeFlow();
            EnsureLeaderboardRequested(false);
        }

        private void OnEnable()
        {
            if (arrowGameManager != null)
            {
                arrowGameManager.ChallengeCompleted += HandleChallengeCompleted;
                arrowGameManager.ChallengeFailed += HandleChallengeFailed;
                arrowGameManager.SetExternalInputLock(true);
            }

            GameDataStore.ChallengeLeaderboardChanged += HandleChallengeLeaderboardChanged;
            MiniPayBridge.ChallengeLeaderboardSubmitted += HandleLeaderboardSubmitted;
            MiniPayBridge.ChallengeLeaderboardSubmitFailed += HandleLeaderboardSubmitFailed;
        }

        private void OnDisable()
        {
            if (arrowGameManager != null)
            {
                arrowGameManager.ChallengeCompleted -= HandleChallengeCompleted;
                arrowGameManager.ChallengeFailed -= HandleChallengeFailed;
            }

            GameDataStore.ChallengeLeaderboardChanged -= HandleChallengeLeaderboardChanged;
            MiniPayBridge.ChallengeLeaderboardSubmitted -= HandleLeaderboardSubmitted;
            MiniPayBridge.ChallengeLeaderboardSubmitFailed -= HandleLeaderboardSubmitFailed;
        }

        private void Update()
        {
            if (!runTimerActive)
                return;

            float elapsedSeconds = Time.realtimeSinceStartup - runTimerStartRealtime;
            if (runTimerText != null)
                runTimerText.text = FormatTime(elapsedSeconds);
        }

        public void StartChallengeFlow()
        {
            if (challengeStarted)
                return;

            if (!GameDataStore.CanEnterChallengeSession(DateTime.UtcNow))
            {
                ReturnToMenu();
                return;
            }

            challengeStarted = true;
            StartCoroutine(StartChallengeFlowCO());
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        public void SubmitPendingScore()
        {
            if (!hasPendingScoreSubmission || isSubmittingScore)
                return;

            isSubmittingScore = true;
            GameDataStore.SubmitChallengeResult(pendingCompletionSeconds, DateTime.UtcNow);
            MiniPayBridge.Instance.SubmitChallengeResult(pendingCompletionSeconds, GetCurrentPatternName(DateTime.UtcNow));

            if (submitScoreButton != null)
                submitScoreButton.interactable = false;

            ShowMintStatus("Minting your score...", false);
            RefreshLeaderboardUi();
        }

        private IEnumerator StartChallengeFlowCO()
        {
            DateTime nowUtc = DateTime.UtcNow;
            bool startedFromRetry = GameDataStore.ConsumePendingChallengeRetry(nowUtc);
            if (!startedFromRetry)
                GameDataStore.MarkChallengeAttemptUsed(nowUtc);

            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
            if (challengeHudPanel != null)
                challengeHudPanel.SetActive(false);
            SetLoadingState(true, 0.08f, "Preparing challenge...");
            yield return null;
            yield return new WaitForEndOfFrame();

            float loadingStartTime = Time.realtimeSinceStartup;
            if (arrowGameManager != null && arrowGameManager.LineGenerator != null && !arrowGameManager.LineGenerator.HasGeneratedBoard)
            {
                SetLoadingState(true, 0.22f, "Reading challenge image...");
                yield return null;

                SetLoadingState(true, 0.45f, "Building arrow puzzle...");
                arrowGameManager.LineGenerator.GenerateBoard();
                arrowGameManager.RefreshBoardAfterGeneration();

                SetLoadingState(true, 0.88f, "Finalizing board...");
                float loadingElapsed = Time.realtimeSinceStartup - loadingStartTime;
                if (loadingElapsed < loadingScreenMinimumSeconds)
                    yield return new WaitForSecondsRealtime(loadingScreenMinimumSeconds - loadingElapsed);
            }

            SetLoadingState(true, 1f, "Challenge ready");
            yield return null;
            SetLoadingState(false, 0f, string.Empty);

            if (countdownPanel != null)
                countdownPanel.SetActive(true);

            for (int count = 3; count >= 1; count--)
            {
                if (countdownText != null)
                    countdownText.text = count.ToString();
                yield return new WaitForSeconds(countdownStepDuration);
            }

            if (countdownPanel != null)
                countdownPanel.SetActive(false);

            if (arrowGameManager != null)
                yield return arrowGameManager.BeginGameplayIntro();

            if (challengeHudPanel != null)
                challengeHudPanel.SetActive(true);

            runTimerStartRealtime = Time.realtimeSinceStartup;
            runTimerActive = true;
        }

        private void HandleChallengeCompleted()
        {
            runTimerActive = false;
            pendingCompletionSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - runTimerStartRealtime);
            hasPendingScoreSubmission = true;

            if (challengeHudPanel != null)
                challengeHudPanel.SetActive(false);
            if (leaderboardPanel == null && arrowGameManager != null)
                leaderboardPanel = arrowGameManager.winUI;
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(true);

            if (submitScoreButton != null)
                submitScoreButton.interactable = true;
            if (finalScoreText != null)
                finalScoreText.text = FormatTime(pendingCompletionSeconds);

            ConfigureChallengeWinUi();
            RefreshLeaderboardUi();
            RebuildLeaderboardLayout();
            EnsureLeaderboardRequested(false);
        }

        private void HandleChallengeFailed()
        {
            pausedRunTimerSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - runTimerStartRealtime);
            runTimerActive = false;
            if (arrowGameManager != null)
            {
                arrowGameManager.ConfigureChallengeRetryUi(GameDataStore.CanUseChallengeRetry(DateTime.UtcNow));
                arrowGameManager.SetExternalInputLock(true);
            }
        }

        public void ResumeRunTimerAfterRevive()
        {
            if (!challengeStarted || hasPendingScoreSubmission)
                return;

            runTimerStartRealtime = Time.realtimeSinceStartup - pausedRunTimerSeconds;
            runTimerActive = true;

            if (challengeHudPanel != null)
                challengeHudPanel.SetActive(true);
        }

        public bool TryUseChallengeRetry()
        {
            DateTime nowUtc = DateTime.UtcNow;
            if (!GameDataStore.CanUseChallengeRetry(nowUtc))
            {
                if (arrowGameManager != null)
                    arrowGameManager.ConfigureChallengeRetryUi(false);
                return false;
            }

            runTimerActive = false;
            GameDataStore.PrepareChallengeRetry(nowUtc);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return true;
        }

        private void WireButtons()
        {
            ButtonBindingUtility.BindAction(submitScoreButton, SubmitPendingScore);
            ButtonBindingUtility.BindAction(leaderboardMainMenuButton, ReturnToMenu);
        }

        private void PrepareInitialPanels()
        {
            SetLoadingState(false, 0f, string.Empty);
            if (countdownPanel != null)
                countdownPanel.SetActive(false);
            if (challengeHudPanel != null)
                challengeHudPanel.SetActive(false);
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
            if (submitScoreButton != null)
                submitScoreButton.interactable = false;
            if (mintStatusPanel != null)
                mintStatusPanel.SetActive(false);

            ConfigureChallengeWinUi();
            ApplySceneTheme();
        }

        private void SetLoadingState(bool visible, float progress, string statusText)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(visible);

            if (loadingProgressFill != null)
                loadingProgressFill.fillAmount = Mathf.Clamp01(progress);

            if (loadingStatusText != null)
                loadingStatusText.text = string.IsNullOrEmpty(statusText) ? string.Empty : statusText;
        }

        private void RefreshLeaderboardUi()
        {
            ApplySceneTheme();

            DateTime nowUtc = DateTime.UtcNow;
            int leaderboardViewCount = leaderboardEntryViews != null ? leaderboardEntryViews.Length : 0;
            List<ChallengeLeaderboardEntryData> entries = GameDataStore.GetChallengeLeaderboardEntries(nowUtc, GetCurrentPatternName(nowUtc), Mathf.Max(leaderboardViewCount, LeaderboardEntryLimit));
            float playerBestTime = GameDataStore.GetChallengeBestTimeSeconds(nowUtc);
            int cycleIndex = GameDataStore.GetCurrentChallengeCycleIndex(nowUtc);
            string patternName = GetCurrentPatternName(nowUtc);

            if (leaderboardTitleText != null)
                leaderboardTitleText.text = $"{challengeTitlePrefix} #{cycleIndex + 1} - {patternName}";

            if (leaderboardPlayerBestText != null)
            {
                leaderboardPlayerBestText.text = playerBestTime > 0f
                    ? $"Your Best: {FormatTime(playerBestTime)}"
                    : "Your Best: Not set yet";
            }

            if (finalScoreText != null && !hasPendingScoreSubmission && playerBestTime > 0f)
                finalScoreText.text = FormatTime(playerBestTime);

            for (int i = 0; i < leaderboardViewCount; i++)
            {
                ChallengeLeaderboardEntryData entryData = i < entries.Count ? entries[i] : null;
                if (leaderboardEntryViews[i] != null)
                    leaderboardEntryViews[i].Bind(entryData);
            }

            RebuildLeaderboardLayout();
        }

        private void HandleChallengeLeaderboardChanged()
        {
            RefreshLeaderboardUi();
        }

        private void HandleLeaderboardSubmitted()
        {
            isSubmittingScore = false;
            hasPendingScoreSubmission = false;

            if (submitScoreButton != null)
                submitScoreButton.interactable = false;

            ShowMintStatus("Your score was successfully minted.", true);
            EnsureLeaderboardRequested(true);
            RefreshLeaderboardUi();
        }

        private void HandleLeaderboardSubmitFailed(string errorMessage)
        {
            isSubmittingScore = false;

            if (submitScoreButton != null)
                submitScoreButton.interactable = true;

            ShowMintStatus(string.IsNullOrWhiteSpace(errorMessage) ? "Score minting failed. Please try again." : errorMessage, true);
        }

        private void EnsureLeaderboardRequested(bool forceRefresh)
        {
            DateTime nowUtc = DateTime.UtcNow;
            string patternName = GetCurrentPatternName(nowUtc);
            if (!forceRefresh && GameDataStore.HasChallengeLeaderboardSnapshot(nowUtc, patternName))
                return;

            MiniPayBridge.Instance.RequestChallengeLeaderboard(patternName, LeaderboardEntryLimit);
        }

        private void ShowMintStatus(string message, bool autoHide)
        {
            if (mintStatusText != null)
                mintStatusText.text = message;

            if (mintStatusPanel != null)
                mintStatusPanel.SetActive(true);

            if (mintStatusRoutine != null)
                StopCoroutine(mintStatusRoutine);

            mintStatusRoutine = autoHide ? StartCoroutine(HideMintStatusAfterDelay()) : null;
        }

        private IEnumerator HideMintStatusAfterDelay()
        {
            yield return new WaitForSecondsRealtime(MintStatusDisplaySeconds);
            if (mintStatusPanel != null)
                mintStatusPanel.SetActive(false);
            mintStatusRoutine = null;
        }

        private string GetCurrentPatternName(DateTime nowUtc)
        {
            return GameDataStore.GetCurrentChallengePatternName(nowUtc, challengePatternNames);
        }

        private static string FormatTime(float seconds)
        {
            int totalMilliseconds = Mathf.RoundToInt(seconds * 1000f);
            int minutes = totalMilliseconds / 60000;
            int remainingMilliseconds = totalMilliseconds % 60000;
            int wholeSeconds = remainingMilliseconds / 1000;
            int milliseconds = remainingMilliseconds % 1000;
            return $"{minutes:00}:{wholeSeconds:00}.{milliseconds:000}";
        }

        private void ApplySceneTheme()
        {
            if (arrowGameManager != null)
            {
                arrowGameManager.RefreshTheme();
                return;
            }

            ThemeManager.ApplyThemeToScene(gameObject.scene);
        }

        private void ConfigureChallengeWinUi()
        {
            if (arrowGameManager == null || arrowGameManager.winUI == null)
                return;

            Button continueButton = FindButtonByLabel(arrowGameManager.winUI.transform, "continue");
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);
        }

        private void RebuildLeaderboardLayout()
        {
            Canvas.ForceUpdateCanvases();

            if (leaderboardPanel != null && leaderboardPanel.transform is RectTransform leaderboardRect)
                LayoutRebuilder.ForceRebuildLayoutImmediate(leaderboardRect);

            RectTransform card = FindDescendantRect(leaderboardPanel != null ? leaderboardPanel.transform : null, "Challenge Leaderboard Card");
            if (card != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(card);

            Canvas.ForceUpdateCanvases();
        }

        private static RectTransform FindDescendantRect(Transform root, string name)
        {
            if (root == null || string.IsNullOrWhiteSpace(name))
                return null;

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == name)
                    return children[i] as RectTransform;
            }

            return null;
        }

        private static Button FindButtonByLabel(Transform root, string labelText)
        {
            if (root == null || string.IsNullOrWhiteSpace(labelText))
                return null;

            string search = labelText.Trim().ToLowerInvariant();
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                TMP_Text label = buttons[i] != null ? buttons[i].GetComponentInChildren<TMP_Text>(true) : null;
                if (label != null && !string.IsNullOrWhiteSpace(label.text) && label.text.Trim().ToLowerInvariant() == search)
                    return buttons[i];
            }

            return null;
        }

    }
}

