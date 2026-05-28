using System;
using System.Collections.Generic;
using ArrowGame;
using UnityEngine;

namespace ArrowGame.Data
{
    [Serializable]
    public class ChallengeLeaderboardEntryData
    {
        public int rank;
        public string playerName;
        public float completionSeconds;
        public bool isPlayer;
    }

    [Serializable]
    public class MiniPayClassicProgressData
    {
        public int level = 1;
    }

    [Serializable]
    public class MiniPayChallengeProgressData
    {
        public int chances = 1;
        public long lastResetUnixMilliseconds;
        public float bestTimeSeconds = -1f;
    }

    [Serializable]
    public class MiniPayUniversalProgressData
    {
        public int weeklyChallengeCycleIndex = -1;
        public long weeklyChallengeEndUnixMilliseconds = -1;
        public string weeklyChallengePatternName = string.Empty;
    }

    [Serializable]
    public class MiniPayUserSnapshotData
    {
        public string walletAddress;
        public string username;
        public bool hasPurchasedGame;
        public int revives = -1;
        public int lives = -1;
        public int hints = 0;
        public bool tutorialCompleted;
        public MiniPayClassicProgressData classic = new();
        public MiniPayChallengeProgressData challenge = new();
        public MiniPayUniversalProgressData universal = new();
    }

    public static class GameDataStore
    {
        private const string LevelKey = "level";
        private const string HintCountKey = "hint_count";
        private const string ReviveCountKey = "revives_count";
        private const string LegacyLivesCountKey = "lives_count";
        private const string GamePurchasedKey = "game_purchased";
        private const string WalletAddressKey = "wallet_address";
        private const string PlayerNameKey = "player_name";
        private const string ChallengePlayerNameKey = "challenge_player_name";
        private const string ChallengeChanceCountKey = "challenge_chances";
        private const string ChallengeLastResetUnixMillisecondsKey = "challenge_last_reset_unix_ms";
        private const string ChallengeLastPlayedUtcDayKey = "challenge_last_played_utc_day";
        private const string ChallengeRetryUsedUtcDayKey = "challenge_retry_used_utc_day";
        private const string ChallengePendingRetryUtcDayKey = "challenge_pending_retry_utc_day";
        private const string UniversalChallengeCycleIndexKey = "universal_challenge_cycle_index";
        private const string UniversalChallengeEndUnixMillisecondsKey = "universal_challenge_end_unix_ms";
        private const string UniversalChallengePatternNameKey = "universal_challenge_pattern_name";
        private const string VibrationEnabledKey = "vibration_enabled";
        private const string SoundEnabledKey = "sound_enabled";
        private const string DarkModeEnabledKey = "dark_mode_enabled";
        private const string TutorialCompletedKey = "tutorial_completed";
        private const int DefaultLevel = 1;
        private const int DefaultHintCount = 0;
        private const int DefaultReviveCount = 3;
        private const int DefaultChallengeChanceCount = 1;
        private const bool DefaultVibrationEnabled = true;
        private const bool DefaultSoundEnabled = true;
        private const bool DefaultDarkModeEnabled = false;
        private const int ChallengeCycleLengthDays = 7;
        private static readonly DateTime UnixEpochUtc = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime ChallengeEpochUtc = new(2026, 4, 27, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan ChallengeChanceCooldown = TimeSpan.FromHours(24);
        private static bool hintCountLoaded;
        private static int cachedHintCount = DefaultHintCount;
        private static bool suppressBridgeSync;
        private static bool bridgeBootstrapResolved;
        private static bool isNotifyingDataChanged;
        private static bool hasPendingDataChangedNotification;
        private static bool pendingDataChangedRequiresBridgeSync;
        private static readonly List<ChallengeLeaderboardEntryData> remoteChallengeLeaderboard = new();
        private static bool hasRemoteChallengeLeaderboardSnapshot;
        private static int remoteChallengeLeaderboardCycleIndex = -1;
        private static string remoteChallengeLeaderboardPatternName = string.Empty;

        public static event Action DataChanged;
        public static event Action ChallengeLeaderboardChanged;

        public static bool HasResolvedBridgeBootstrap => bridgeBootstrapResolved;

        public static string WalletAddress
        {
            get => PlayerPrefs.GetString(WalletAddressKey, string.Empty);
            set => SaveString(WalletAddressKey, value?.Trim() ?? string.Empty, syncBridge: false);
        }

        public static string PlayerName
        {
            get
            {
                string fallbackName = PlayerPrefs.GetString(ChallengePlayerNameKey, "You");
                return PlayerPrefs.GetString(PlayerNameKey, fallbackName);
            }
            set
            {
                string sanitizedValue = string.IsNullOrWhiteSpace(value) ? "You" : value.Trim();
                SaveStringPair(PlayerNameKey, sanitizedValue, ChallengePlayerNameKey, sanitizedValue);
            }
        }

        public static string ChallengePlayerName
        {
            get => PlayerName;
            set => PlayerName = value;
        }

        public static int Level
        {
            get => PlayerPrefs.GetInt(LevelKey, DefaultLevel);
            set => SaveInt(LevelKey, Mathf.Max(DefaultLevel, value));
        }

        public static int HintCount
        {
            get
            {
                if (!hintCountLoaded)
                {
                    cachedHintCount = Mathf.Max(0, PlayerPrefs.GetInt(HintCountKey, DefaultHintCount));
                    hintCountLoaded = true;
                }

                return cachedHintCount;
            }
            set
            {
                cachedHintCount = Mathf.Max(0, value);
                hintCountLoaded = true;
                SaveInt(HintCountKey, cachedHintCount);
            }
        }

        public static int ReviveCount
        {
            get
            {
                if (PlayerPrefs.HasKey(ReviveCountKey))
                    return Mathf.Max(0, PlayerPrefs.GetInt(ReviveCountKey, DefaultReviveCount));

                return Mathf.Max(0, PlayerPrefs.GetInt(LegacyLivesCountKey, DefaultReviveCount));
            }
            set => SaveInt(ReviveCountKey, Mathf.Max(0, value));
        }

        public static int LivesCount
        {
            get => ReviveCount;
            set => ReviveCount = value;
        }

        public static bool HasPurchasedGame
        {
            get => GetBool(GamePurchasedKey);
            set => SaveBool(GamePurchasedKey, value);
        }

        public static bool IsVibrationEnabled
        {
            get => GetBool(VibrationEnabledKey, DefaultVibrationEnabled);
            set => SaveBool(VibrationEnabledKey, value, syncBridge: false);
        }

        public static bool IsSoundEnabled
        {
            get => GetBool(SoundEnabledKey, DefaultSoundEnabled);
            set => SaveBool(SoundEnabledKey, value, syncBridge: false);
        }

        public static bool IsDarkModeEnabled
        {
            get => GetBool(DarkModeEnabledKey, DefaultDarkModeEnabled);
            set => SaveBool(DarkModeEnabledKey, value, syncBridge: false);
        }

        public static bool HasCompletedTutorial
        {
            get => GetBool(TutorialCompletedKey);
            set => SaveBool(TutorialCompletedKey, value, syncBridge: false);
        }

        public static bool TryConsumeHint()
        {
            if (HintCount <= 0)
                return false;

            HintCount--;
            return true;
        }

        public static void AddHints(int amount)
        {
            if (amount <= 0)
                return;

            HintCount += amount;
        }

        public static void AddRevives(int amount)
        {
            if (amount <= 0)
                return;

            ReviveCount += amount;
        }

        public static void AddLives(int amount)
        {
            AddRevives(amount);
        }

        public static void MarkTutorialCompleted()
        {
            HasCompletedTutorial = true;
            NotifyDataChanged(syncBridge: false);
        }

        public static void MarkBridgeBootstrapResolved()
        {
            bridgeBootstrapResolved = true;
            NotifyDataChanged(syncBridge: false);
        }

        public static void ApplyBridgeSnapshot(MiniPayUserSnapshotData snapshot)
        {
            if (snapshot == null)
                return;

            suppressBridgeSync = true;
            try
            {
                if (!string.IsNullOrWhiteSpace(snapshot.walletAddress))
                    PlayerPrefs.SetString(WalletAddressKey, snapshot.walletAddress.Trim());

                if (!string.IsNullOrWhiteSpace(snapshot.username))
                {
                    PlayerPrefs.SetString(PlayerNameKey, snapshot.username.Trim());
                    PlayerPrefs.SetString(ChallengePlayerNameKey, snapshot.username.Trim());
                }

                PlayerPrefs.SetInt(GamePurchasedKey, snapshot.hasPurchasedGame ? 1 : 0);
                int reviveCount = snapshot.revives >= 0
                    ? snapshot.revives
                    : snapshot.lives >= 0
                        ? snapshot.lives
                        : DefaultReviveCount;

                PlayerPrefs.SetInt(ReviveCountKey, Mathf.Max(0, reviveCount));
                PlayerPrefs.SetInt(HintCountKey, Mathf.Max(0, snapshot.hints));
                hintCountLoaded = true;
                cachedHintCount = Mathf.Max(0, snapshot.hints);

                if (snapshot.classic != null)
                    PlayerPrefs.SetInt(LevelKey, Mathf.Max(DefaultLevel, snapshot.classic.level));

                if (snapshot.challenge != null)
                {
                    PlayerPrefs.SetInt(ChallengeChanceCountKey, Mathf.Max(0, snapshot.challenge.chances));
                    SaveLongRaw(ChallengeLastResetUnixMillisecondsKey, Math.Max(0L, snapshot.challenge.lastResetUnixMilliseconds));

                    if (snapshot.challenge.bestTimeSeconds > 0f)
                    {
                        int bestTimeCycleIndex = snapshot.universal != null && snapshot.universal.weeklyChallengeCycleIndex >= 0
                            ? snapshot.universal.weeklyChallengeCycleIndex
                            : GetCurrentChallengeCycleIndex(DateTime.UtcNow);
                        PlayerPrefs.SetFloat(GetChallengeBestTimeKey(bestTimeCycleIndex), snapshot.challenge.bestTimeSeconds);
                    }
                }

                if (snapshot.universal != null)
                {
                    if (snapshot.universal.weeklyChallengeCycleIndex >= 0)
                        PlayerPrefs.SetInt(UniversalChallengeCycleIndexKey, snapshot.universal.weeklyChallengeCycleIndex);

                    if (snapshot.universal.weeklyChallengeEndUnixMilliseconds > 0)
                        SaveLongRaw(UniversalChallengeEndUnixMillisecondsKey, snapshot.universal.weeklyChallengeEndUnixMilliseconds);

                    if (!string.IsNullOrWhiteSpace(snapshot.universal.weeklyChallengePatternName))
                        PlayerPrefs.SetString(UniversalChallengePatternNameKey, snapshot.universal.weeklyChallengePatternName.Trim());
                    else
                        PlayerPrefs.DeleteKey(UniversalChallengePatternNameKey);
                }

                if (snapshot.tutorialCompleted)
                    PlayerPrefs.SetInt(TutorialCompletedKey, 1);

                PlayerPrefs.Save();
            }
            finally
            {
                suppressBridgeSync = false;
            }

            bridgeBootstrapResolved = true;
            RefreshChallengeChances(DateTime.UtcNow, syncBridgeIfChanged: false);
            NotifyDataChanged(syncBridge: false);
        }

        public static string BuildBridgeSnapshotJson(DateTime utcNow)
        {
            return JsonUtility.ToJson(BuildBridgeSnapshot(utcNow));
        }

        public static MiniPayUserSnapshotData BuildBridgeSnapshot(DateTime utcNow)
        {
            DateTime normalizedUtcNow = utcNow.ToUniversalTime();
            int cycleIndex = GetCurrentChallengeCycleIndex(normalizedUtcNow);
            RefreshChallengeChances(normalizedUtcNow, syncBridgeIfChanged: false);

            return new MiniPayUserSnapshotData
            {
                walletAddress = WalletAddress,
                username = PlayerName,
                hasPurchasedGame = HasPurchasedGame,
                revives = ReviveCount,
                lives = ReviveCount,
                hints = HintCount,
                tutorialCompleted = HasCompletedTutorial,
                classic = new MiniPayClassicProgressData
                {
                    level = Level
                },
                challenge = new MiniPayChallengeProgressData
                {
                    chances = PlayerPrefs.GetInt(ChallengeChanceCountKey, DefaultChallengeChanceCount),
                    lastResetUnixMilliseconds = GetLong(ChallengeLastResetUnixMillisecondsKey, 0L),
                    bestTimeSeconds = GetChallengeBestTimeSeconds(normalizedUtcNow)
                },
                universal = new MiniPayUniversalProgressData
                {
                    weeklyChallengeCycleIndex = GetCurrentChallengeCycleIndex(normalizedUtcNow),
                    weeklyChallengeEndUnixMilliseconds = ToUnixMilliseconds(GetCurrentChallengeCycleEndUtc(normalizedUtcNow)),
                    weeklyChallengePatternName = GetSharedChallengePatternName()
                }
            };
        }

        public static int GetCurrentChallengeCycleIndex(DateTime utcNow)
        {
            if (TryGetUniversalChallengeWindow(out int remoteCycleIndex, out _))
                return Mathf.Max(0, remoteCycleIndex);

            int elapsedDays = Mathf.Max(0, GetUtcDayNumber(utcNow) - GetUtcDayNumber(ChallengeEpochUtc));
            return elapsedDays / ChallengeCycleLengthDays;
        }

        public static int GetCurrentChallengeDayIndex(DateTime utcNow)
        {
            if (TryGetUniversalChallengeWindow(out _, out DateTime remoteEndUtc))
            {
                DateTime remoteStartUtc = remoteEndUtc.AddDays(-ChallengeCycleLengthDays);
                int remoteDayIndex = Mathf.FloorToInt((float)(utcNow.ToUniversalTime().Date - remoteStartUtc.Date).TotalDays);
                return Mathf.Clamp(remoteDayIndex, 0, ChallengeCycleLengthDays - 1);
            }

            int elapsedDays = Mathf.Max(0, GetUtcDayNumber(utcNow) - GetUtcDayNumber(ChallengeEpochUtc));
            return elapsedDays % ChallengeCycleLengthDays;
        }

        public static DateTime GetCurrentChallengeCycleStartUtc(DateTime utcNow)
        {
            if (TryGetUniversalChallengeWindow(out _, out DateTime remoteEndUtc))
                return remoteEndUtc.AddDays(-ChallengeCycleLengthDays);

            return ChallengeEpochUtc.AddDays(GetCurrentChallengeCycleIndex(utcNow) * ChallengeCycleLengthDays);
        }

        public static DateTime GetCurrentChallengeCycleEndUtc(DateTime utcNow)
        {
            if (TryGetUniversalChallengeWindow(out _, out DateTime remoteEndUtc))
                return remoteEndUtc;

            return GetCurrentChallengeCycleStartUtc(utcNow).AddDays(ChallengeCycleLengthDays);
        }

        public static TimeSpan GetCurrentChallengeTimeRemaining(DateTime utcNow)
        {
            return GetCurrentChallengeCycleEndUtc(utcNow) - utcNow.ToUniversalTime();
        }

        public static bool HasPlayedChallengeToday(DateTime utcNow)
        {
            return PlayerPrefs.GetInt(ChallengeLastPlayedUtcDayKey, int.MinValue) == GetUtcDayNumber(utcNow);
        }

        public static bool CanPlayChallengeToday(DateTime utcNow)
        {
            return GetChallengeChancesRemainingToday(utcNow) > 0;
        }

        public static bool HasPendingChallengeRetry(DateTime utcNow)
        {
            return PlayerPrefs.GetInt(ChallengePendingRetryUtcDayKey, int.MinValue) == GetUtcDayNumber(utcNow);
        }

        public static bool CanEnterChallengeSession(DateTime utcNow)
        {
            return CanPlayChallengeToday(utcNow) || HasPendingChallengeRetry(utcNow);
        }

        public static bool CanUseChallengeRetry(DateTime utcNow)
        {
            int utcDayNumber = GetUtcDayNumber(utcNow);
            bool retryAlreadyUsedToday = PlayerPrefs.GetInt(ChallengeRetryUsedUtcDayKey, int.MinValue) == utcDayNumber;
            return HasPlayedChallengeToday(utcNow) && !HasPendingChallengeRetry(utcNow) && !retryAlreadyUsedToday;
        }

        public static int GetChallengeChancesRemainingToday(DateTime utcNow)
        {
            RefreshChallengeChances(utcNow);
            return Mathf.Max(0, PlayerPrefs.GetInt(ChallengeChanceCountKey, DefaultChallengeChanceCount));
        }

        public static TimeSpan GetTimeUntilNextChallengeChance(DateTime utcNow)
        {
            if (GetChallengeChancesRemainingToday(utcNow) > 0)
                return TimeSpan.Zero;

            long lastResetUnixMilliseconds = GetLong(ChallengeLastResetUnixMillisecondsKey, 0L);
            if (lastResetUnixMilliseconds <= 0)
                return TimeSpan.Zero;

            DateTime nextResetUtc = FromUnixMilliseconds(lastResetUnixMilliseconds).Add(ChallengeChanceCooldown);
            return nextResetUtc - utcNow.ToUniversalTime();
        }

        public static void MarkChallengeAttemptUsed(DateTime utcNow)
        {
            DateTime utcDateTime = utcNow.ToUniversalTime();
            RefreshChallengeChances(utcDateTime, syncBridgeIfChanged: false);

            int remainingChances = Mathf.Max(0, PlayerPrefs.GetInt(ChallengeChanceCountKey, DefaultChallengeChanceCount) - 1);
            PlayerPrefs.SetInt(ChallengeChanceCountKey, remainingChances);
            PlayerPrefs.SetInt(ChallengeLastPlayedUtcDayKey, GetUtcDayNumber(utcDateTime));

            if (remainingChances <= 0)
                SaveLongRaw(ChallengeLastResetUnixMillisecondsKey, ToUnixMilliseconds(utcDateTime));

            PlayerPrefs.Save();

            NotifyDataChanged();
        }

        public static void PrepareChallengeRetry(DateTime utcNow)
        {
            DateTime utcDateTime = utcNow.ToUniversalTime();
            int utcDayNumber = GetUtcDayNumber(utcDateTime);
            PlayerPrefs.SetInt(ChallengeRetryUsedUtcDayKey, utcDayNumber);
            PlayerPrefs.SetInt(ChallengePendingRetryUtcDayKey, utcDayNumber);
            PlayerPrefs.Save();
            NotifyDataChanged();
        }

        public static bool ConsumePendingChallengeRetry(DateTime utcNow)
        {
            if (!HasPendingChallengeRetry(utcNow))
                return false;

            PlayerPrefs.DeleteKey(ChallengePendingRetryUtcDayKey);
            PlayerPrefs.Save();
            NotifyDataChanged();
            return true;
        }

        public static float GetChallengeBestTimeSeconds(DateTime utcNow)
        {
            int cycleIndex = GetCurrentChallengeCycleIndex(utcNow);
            return PlayerPrefs.GetFloat(GetChallengeBestTimeKey(cycleIndex), -1f);
        }

        public static bool HasChallengeBestTime(DateTime utcNow)
        {
            int cycleIndex = GetCurrentChallengeCycleIndex(utcNow);
            return PlayerPrefs.HasKey(GetChallengeBestTimeKey(cycleIndex));
        }

        public static bool SubmitChallengeResult(float completionSeconds, DateTime utcNow)
        {
            int cycleIndex = GetCurrentChallengeCycleIndex(utcNow);
            string bestTimeKey = GetChallengeBestTimeKey(cycleIndex);
            float currentBestTime = PlayerPrefs.GetFloat(bestTimeKey, float.MaxValue);
            bool isNewBest = !PlayerPrefs.HasKey(bestTimeKey) || completionSeconds < currentBestTime;

            if (isNewBest)
            {
                PlayerPrefs.SetFloat(bestTimeKey, completionSeconds);
                PlayerPrefs.Save();
                NotifyDataChanged();
            }

            return isNewBest;
        }

        public static int GetCurrentChallengePatternIndex(DateTime utcNow, int patternCount)
        {
            int safePatternCount = Mathf.Max(patternCount, 1);
            return GetCurrentChallengeCycleIndex(utcNow) % safePatternCount;
        }

        public static string GetSharedChallengePatternName()
        {
            return PlayerPrefs.GetString(UniversalChallengePatternNameKey, string.Empty).Trim();
        }

        public static string GetCurrentChallengePatternName(DateTime utcNow, string[] fallbackPatternNames)
        {
            string sharedPatternName = GetSharedChallengePatternName();
            if (!string.IsNullOrWhiteSpace(sharedPatternName))
                return sharedPatternName;

            string[] safePatternNames = fallbackPatternNames ?? Array.Empty<string>();
            int cycleIndex = GetCurrentChallengeCycleIndex(utcNow);
            int patternIndex = GetCurrentChallengePatternIndex(utcNow, safePatternNames.Length);
            return safePatternNames.Length > 0
                ? safePatternNames[Mathf.Clamp(patternIndex, 0, safePatternNames.Length - 1)]
                : $"Pattern {cycleIndex + 1}";
        }

        public static bool HasRemoteChallengeLeaderboardSnapshot()
        {
            return hasRemoteChallengeLeaderboardSnapshot;
        }

        public static int GetDisplayedChallengeLeaderboardCycleIndex(DateTime utcNow)
        {
            return hasRemoteChallengeLeaderboardSnapshot
                ? Mathf.Max(0, remoteChallengeLeaderboardCycleIndex)
                : GetCurrentChallengeCycleIndex(utcNow);
        }

        public static string GetDisplayedChallengeLeaderboardPatternName(DateTime utcNow, string[] fallbackPatternNames)
        {
            if (hasRemoteChallengeLeaderboardSnapshot && !string.IsNullOrWhiteSpace(remoteChallengeLeaderboardPatternName))
                return remoteChallengeLeaderboardPatternName;

            return GetCurrentChallengePatternName(utcNow, fallbackPatternNames);
        }

        public static int GetCurrentChallengeSeed(DateTime utcNow, int seedOffset = 0)
        {
            unchecked
            {
                return 7919 + GetCurrentChallengeCycleIndex(utcNow) * 104729 + seedOffset * 17;
            }
        }

        public static void ApplyChallengeLeaderboard(List<ChallengeLeaderboardEntryData> entries, DateTime utcNow, string patternName)
        {
            ApplyChallengeLeaderboard(entries, GetCurrentChallengeCycleIndex(utcNow), patternName);
        }

        public static void ApplyChallengeLeaderboard(List<ChallengeLeaderboardEntryData> entries, int cycleIndex, string patternName)
        {
            hasRemoteChallengeLeaderboardSnapshot = true;
            remoteChallengeLeaderboardCycleIndex = Mathf.Max(0, cycleIndex);
            remoteChallengeLeaderboardPatternName = patternName?.Trim() ?? string.Empty;
            remoteChallengeLeaderboard.Clear();
            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i] == null)
                        continue;

                    remoteChallengeLeaderboard.Add(new ChallengeLeaderboardEntryData
                    {
                        rank = entries[i].rank,
                        playerName = entries[i].playerName,
                        completionSeconds = entries[i].completionSeconds,
                        isPlayer = entries[i].isPlayer
                    });
                }
            }

            ChallengeLeaderboardChanged?.Invoke();
        }

        public static bool HasChallengeLeaderboardSnapshot(DateTime utcNow, string patternName)
        {
            return hasRemoteChallengeLeaderboardSnapshot;
        }

        public static List<ChallengeLeaderboardEntryData> GetChallengeLeaderboardEntries(DateTime utcNow, string patternName, int entryCount)
        {
            if (remoteChallengeLeaderboard.Count == 0)
                return BuildLocalChallengeLeaderboard(utcNow, entryCount);

            int safeEntryCount = Mathf.Max(entryCount, 1);
            List<ChallengeLeaderboardEntryData> entries = new(Mathf.Min(safeEntryCount, remoteChallengeLeaderboard.Count));
            for (int i = 0; i < remoteChallengeLeaderboard.Count && entries.Count < safeEntryCount; i++)
            {
                ChallengeLeaderboardEntryData source = remoteChallengeLeaderboard[i];
                entries.Add(new ChallengeLeaderboardEntryData
                {
                    rank = source.rank > 0 ? source.rank : i + 1,
                    playerName = source.playerName,
                    completionSeconds = source.completionSeconds,
                    isPlayer = source.isPlayer
                });
            }

            return entries;
        }

        public static List<ChallengeLeaderboardEntryData> BuildLocalChallengeLeaderboard(DateTime utcNow, int entryCount)
        {
            int safeEntryCount = Mathf.Max(entryCount, 1);
            List<ChallengeLeaderboardEntryData> entries = new();
            float playerBestTime = GetChallengeBestTimeSeconds(utcNow);

            if (playerBestTime > 0f)
            {
                entries.Add(new ChallengeLeaderboardEntryData
                {
                    rank = 1,
                    playerName = ChallengePlayerName,
                    completionSeconds = playerBestTime,
                    isPlayer = true
                });
            }
            return entries.Count > safeEntryCount
                ? entries.GetRange(0, safeEntryCount)
                : entries;
        }

        private static void RefreshChallengeChances(DateTime utcNow, bool syncBridgeIfChanged = true)
        {
            int storedChanceCount = Mathf.Max(0, PlayerPrefs.GetInt(ChallengeChanceCountKey, DefaultChallengeChanceCount));
            long lastResetUnixMilliseconds = GetLong(ChallengeLastResetUnixMillisecondsKey, 0L);
            if (storedChanceCount > 0 || lastResetUnixMilliseconds <= 0)
                return;

            DateTime nextResetUtc = FromUnixMilliseconds(lastResetUnixMilliseconds).Add(ChallengeChanceCooldown);
            if (utcNow.ToUniversalTime() < nextResetUtc)
                return;

            PlayerPrefs.SetInt(ChallengeChanceCountKey, DefaultChallengeChanceCount);
            SaveLongRaw(ChallengeLastResetUnixMillisecondsKey, 0L);
            PlayerPrefs.Save();
            NotifyDataChanged(syncBridge: syncBridgeIfChanged);
        }

        private static bool TryGetUniversalChallengeWindow(out int cycleIndex, out DateTime endUtc)
        {
            cycleIndex = PlayerPrefs.GetInt(UniversalChallengeCycleIndexKey, -1);
            long endUnixMilliseconds = GetLong(UniversalChallengeEndUnixMillisecondsKey, -1L);
            if (cycleIndex < 0 || endUnixMilliseconds <= 0)
            {
                endUtc = default;
                return false;
            }

            endUtc = FromUnixMilliseconds(endUnixMilliseconds);
            return true;
        }

        private static bool GetBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        private static void SaveBool(string key, bool value, bool syncBridge = true)
        {
            SaveInt(key, value ? 1 : 0, syncBridge);
        }

        private static void SaveInt(string key, int value, bool syncBridge = true)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            NotifyDataChanged(syncBridge);
        }

        private static void SaveString(string key, string value, bool syncBridge = true)
        {
            PlayerPrefs.SetString(key, value ?? string.Empty);
            PlayerPrefs.Save();
            NotifyDataChanged(syncBridge);
        }

        private static void SaveStringPair(string firstKey, string firstValue, string secondKey, string secondValue)
        {
            PlayerPrefs.SetString(firstKey, firstValue ?? string.Empty);
            PlayerPrefs.SetString(secondKey, secondValue ?? string.Empty);
            PlayerPrefs.Save();
            NotifyDataChanged();
        }

        private static long GetLong(string key, long defaultValue)
        {
            string rawValue = PlayerPrefs.GetString(key, string.Empty);
            return long.TryParse(rawValue, out long parsedValue) ? parsedValue : defaultValue;
        }

        private static void SaveLongRaw(string key, long value)
        {
            PlayerPrefs.SetString(key, value.ToString());
        }

        private static int GetUtcDayNumber(DateTime utcNow)
        {
            return Mathf.FloorToInt((float)(utcNow.ToUniversalTime().Date - UnixEpochUtc).TotalDays);
        }

        private static DateTime FromUnixMilliseconds(long unixMilliseconds)
        {
            return UnixEpochUtc.AddMilliseconds(unixMilliseconds);
        }

        private static long ToUnixMilliseconds(DateTime utcDateTime)
        {
            return Convert.ToInt64((utcDateTime.ToUniversalTime() - UnixEpochUtc).TotalMilliseconds);
        }

        private static string GetChallengeBestTimeKey(int cycleIndex)
        {
            return $"challenge_cycle_{cycleIndex}_best_time";
        }

        private static void NotifyDataChanged(bool syncBridge = true)
        {
            if (isNotifyingDataChanged)
            {
                hasPendingDataChangedNotification = true;
                pendingDataChangedRequiresBridgeSync |= syncBridge;
                return;
            }

            bool shouldSyncBridge = syncBridge;
            do
            {
                hasPendingDataChangedNotification = false;
                pendingDataChangedRequiresBridgeSync = false;
                isNotifyingDataChanged = true;
                try
                {
                    DataChanged?.Invoke();
                }
                finally
                {
                    isNotifyingDataChanged = false;
                }

                shouldSyncBridge |= pendingDataChangedRequiresBridgeSync;
            }
            while (hasPendingDataChangedNotification);

            if (suppressBridgeSync || !shouldSyncBridge || !bridgeBootstrapResolved)
                return;

            MiniPayBridge.RequestUserStateSync();
        }
    }
}
