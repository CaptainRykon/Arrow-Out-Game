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

        [Header("Menu Panels")]
        [SerializeField] private GameObject challengeMenuPanel;
        [SerializeField] private GameObject streakPanel;
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private GameObject challengeHudPanel;
        [SerializeField] private GameObject leaderboardPanel;

        [Header("Menu UI")]
        [SerializeField] private Button challengePlayButton;
        [SerializeField] private Button openStreakButton;
        [SerializeField] private Button closeStreakButton;
        [SerializeField] private TextMeshProUGUI challengeTitleText;
        [SerializeField] private TextMeshProUGUI challengePatternText;
        [SerializeField] private TextMeshProUGUI cycleTimerText;
        [SerializeField] private TextMeshProUGUI nextChanceTimerText;
        [SerializeField] private TextMeshProUGUI challengeStatusText;
        [SerializeField] private TextMeshProUGUI streakSummaryText;

        [Header("Countdown")]
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private float countdownStepDuration = 0.8f;

        [Header("Run HUD")]
        [SerializeField] private TextMeshProUGUI runTimerText;

        [Header("Streak")]
        [SerializeField] private ChallengeStreakDayView[] streakDayViews;

        [Header("Leaderboard")]
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

        private void Awake()
        {
            if (arrowGameManager == null)
                arrowGameManager = FindFirstObjectByType<ArrowGameManager>();

            WireButtons();
            PrepareInitialPanels();
            RefreshAllUi();
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
            RefreshDynamicTimerUi();

            if (!runTimerActive)
                return;

            float elapsedSeconds = Time.realtimeSinceStartup - runTimerStartRealtime;
            if (runTimerText != null)
                runTimerText.text = FormatTime(elapsedSeconds);
        }

        public void StartChallengeFlow()
        {
            if (!GameDataStore.CanPlayChallengeToday(DateTime.UtcNow))
            {
                RefreshAllUi();
                return;
            }

            StartCoroutine(StartChallengeFlowCO());
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
            GameDataStore.MarkChallengeAttemptUsed(nowUtc);
            RefreshAllUi();

            if (challengeMenuPanel != null)
                challengeMenuPanel.SetActive(false);
            if (streakPanel != null)
                streakPanel.SetActive(false);
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
            if (challengeHudPanel != null)
                challengeHudPanel.SetActive(true);

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

            runTimerStartRealtime = Time.realtimeSinceStartup;
            runTimerActive = true;
        }

        private void HandleChallengeCompleted()
        {
            runTimerActive = false;
            pendingCompletionSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - runTimerStartRealtime);
            hasPendingScoreSubmission = true;
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
            RefreshAllUi();
        }

        private void WireButtons()
        {
            RegisterButton(challengePlayButton, StartChallengeFlow);
            RegisterButton(openStreakButton, OpenStreakPanel);
            RegisterButton(closeStreakButton, CloseStreakPanel);
            RegisterButton(submitScoreButton, SubmitPendingScore);
            RegisterButton(leaderboardMainMenuButton, ReturnToMenu);
        }

        private void PrepareInitialPanels()
        {
            if (countdownPanel != null)
                countdownPanel.SetActive(false);
            if (streakPanel != null)
                streakPanel.SetActive(false);
            if (challengeHudPanel != null)
                challengeHudPanel.SetActive(false);
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
            if (challengeMenuPanel != null)
                challengeMenuPanel.SetActive(true);
            if (submitScoreButton != null)
                submitScoreButton.interactable = false;
        }

        private void RefreshAllUi()
        {
            DateTime nowUtc = DateTime.UtcNow;
            int cycleIndex = GameDataStore.GetCurrentChallengeCycleIndex(nowUtc);
            int patternIndex = GameDataStore.GetCurrentChallengePatternIndex(nowUtc, challengePatternNames.Length);
            string patternName = challengePatternNames.Length > 0 ? challengePatternNames[patternIndex] : $"Pattern {cycleIndex + 1}";

            if (challengeTitleText != null)
                challengeTitleText.text = $"{challengeTitlePrefix} #{cycleIndex + 1}";
            if (challengePatternText != null)
                challengePatternText.text = patternName;

            RefreshDynamicTimerUi();
            RefreshStreakUi(nowUtc);
            RefreshLeaderboardUi();
        }

        private void RefreshDynamicTimerUi()
        {
            DateTime nowUtc = DateTime.UtcNow;
            bool canPlayToday = GameDataStore.CanPlayChallengeToday(nowUtc);

            if (cycleTimerText != null)
                cycleTimerText.text = FormatCountdown(GameDataStore.GetCurrentChallengeTimeRemaining(nowUtc));

            if (nextChanceTimerText != null)
            {
                nextChanceTimerText.text = canPlayToday
                    ? "Chance Ready"
                    : FormatCountdown(GameDataStore.GetTimeUntilNextChallengeChance(nowUtc));
            }

            if (challengeStatusText != null)
            {
                challengeStatusText.text = canPlayToday
                    ? "You have 1 chance available today."
                    : "Today’s challenge chance is used. Come back when the timer resets.";
            }

            if (challengePlayButton != null)
                challengePlayButton.interactable = canPlayToday;
        }

        private void RefreshStreakUi(DateTime nowUtc)
        {
            int currentDayIndex = GameDataStore.GetCurrentChallengeDayIndex(nowUtc);
            int streakMask = GameDataStore.GetChallengeStreakMask(nowUtc);
            int playedDayCount = 0;

            int streakViewCount = streakDayViews != null ? streakDayViews.Length : 0;
            for (int i = 0; i < streakViewCount; i++)
            {
                bool isPlayed = (streakMask & (1 << i)) != 0;
                bool isCurrentDay = i == currentDayIndex;
                bool isMissed = i < currentDayIndex && !isPlayed;
                if (isPlayed)
                    playedDayCount++;

                if (streakDayViews[i] != null)
                    streakDayViews[i].Bind(i + 1, isPlayed, isCurrentDay, isMissed);
            }

            if (streakSummaryText != null)
                streakSummaryText.text = $"{playedDayCount}/{Mathf.Max(streakViewCount, 7)} days played this week";
        }

        private void RefreshLeaderboardUi()
        {
            DateTime nowUtc = DateTime.UtcNow;
            int leaderboardViewCount = leaderboardEntryViews != null ? leaderboardEntryViews.Length : 0;
            List<ChallengeLeaderboardEntryData> entries = GameDataStore.BuildLocalChallengeLeaderboard(nowUtc, Mathf.Max(leaderboardViewCount, 6));
            float playerBestTime = GameDataStore.GetChallengeBestTimeSeconds(nowUtc);

            if (leaderboardTitleText != null)
                leaderboardTitleText.text = $"Cycle #{GameDataStore.GetCurrentChallengeCycleIndex(nowUtc) + 1} Leaderboard";

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

        private static string FormatCountdown(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.Zero)
                timeSpan = TimeSpan.Zero;

            int totalDays = Mathf.Max(0, timeSpan.Days);
            return $"{totalDays:00}d {timeSpan.Hours:00}h {timeSpan.Minutes:00}m {timeSpan.Seconds:00}s";
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

        private static void RegisterButton(Button button, Action action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action.Invoke());
        }
    }
}

