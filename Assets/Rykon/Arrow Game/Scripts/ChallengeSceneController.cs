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

        private bool runTimerActive;
        private float runTimerStartRealtime;
        private bool hasPendingScoreSubmission;
        private float pendingCompletionSeconds;
        private bool challengeStarted;

        private void Awake()
        {
            if (arrowGameManager == null)
                arrowGameManager = FindFirstObjectByType<ArrowGameManager>();

            WireButtons();
            PrepareInitialPanels();
            RefreshLeaderboardUi();
        }

        private void Start()
        {
            StartChallengeFlow();
        }

        private void OnEnable()
        {
            if (arrowGameManager != null)
            {
                arrowGameManager.ChallengeCompleted += HandleChallengeCompleted;
                arrowGameManager.ChallengeFailed += HandleChallengeFailed;
                arrowGameManager.SetExternalInputLock(true);
            }
        }

        private void OnDisable()
        {
            if (arrowGameManager != null)
            {
                arrowGameManager.ChallengeCompleted -= HandleChallengeCompleted;
                arrowGameManager.ChallengeFailed -= HandleChallengeFailed;
            }
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
            if (!hasPendingScoreSubmission)
                return;

            GameDataStore.SubmitChallengeResult(pendingCompletionSeconds, DateTime.UtcNow);
            hasPendingScoreSubmission = false;

            if (submitScoreButton != null)
                submitScoreButton.interactable = false;

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

            RefreshLeaderboardUi();
        }

        private void HandleChallengeFailed()
        {
            runTimerActive = false;
            if (arrowGameManager != null)
            {
                arrowGameManager.ConfigureChallengeRetryUi(GameDataStore.CanUseChallengeRetry(DateTime.UtcNow));
                arrowGameManager.SetExternalInputLock(true);
            }
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
            RegisterButton(submitScoreButton, SubmitPendingScore);
            RegisterButton(leaderboardMainMenuButton, ReturnToMenu);
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
            List<ChallengeLeaderboardEntryData> entries = GameDataStore.BuildLocalChallengeLeaderboard(nowUtc, Mathf.Max(leaderboardViewCount, 6));
            float playerBestTime = GameDataStore.GetChallengeBestTimeSeconds(nowUtc);
            int cycleIndex = GameDataStore.GetCurrentChallengeCycleIndex(nowUtc);
            int patternIndex = GameDataStore.GetCurrentChallengePatternIndex(nowUtc, challengePatternNames.Length);
            string patternName = challengePatternNames.Length > 0
                ? challengePatternNames[Mathf.Clamp(patternIndex, 0, challengePatternNames.Length - 1)]
                : $"Pattern {cycleIndex + 1}";

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

        private static void RegisterButton(Button button, Action action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action.Invoke());
        }
    }
}

