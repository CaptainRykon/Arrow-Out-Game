using System;
using System.Collections.Generic;
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

    public static class GameDataStore
    {
        private const string LevelKey = "level";
        private const string HintCountKey = "hint_count";
        private const string PlayerNameKey = "player_name";
        private const string ChallengePlayerNameKey = "challenge_player_name";
        private const string ChallengeLastPlayedUtcDayKey = "challenge_last_played_utc_day";
        private const string VibrationEnabledKey = "vibration_enabled";
        private const string SoundEnabledKey = "sound_enabled";
        private const string DarkModeEnabledKey = "dark_mode_enabled";
        private const int DefaultLevel = 1;
        private const int DefaultHintCount = 10;
        private const bool DefaultVibrationEnabled = true;
        private const bool DefaultSoundEnabled = true;
        private const bool DefaultDarkModeEnabled = false;
        private const int ChallengeCycleLengthDays = 7;
        private static readonly DateTime UnixEpochUtc = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime ChallengeEpochUtc = new(2026, 4, 27, 0, 0, 0, DateTimeKind.Utc);
        private static readonly string[] ChallengeRivalNames =
        {
            "Ava", "Noah", "Mira", "Theo", "Ivy", "Owen", "Luna", "Aria",
            "Kai", "Zara", "Nina", "Ezra", "Milo", "Skye", "Nova", "Jude"
        };

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
                PlayerPrefs.SetString(PlayerNameKey, sanitizedValue);
                PlayerPrefs.SetString(ChallengePlayerNameKey, sanitizedValue);
                PlayerPrefs.Save();
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
            set
            {
                PlayerPrefs.SetInt(LevelKey, Mathf.Max(DefaultLevel, value));
                PlayerPrefs.Save();
            }
        }

        public static int HintCount
        {
            get => PlayerPrefs.GetInt(HintCountKey, DefaultHintCount);
            set
            {
                PlayerPrefs.SetInt(HintCountKey, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static bool IsVibrationEnabled
        {
            get => PlayerPrefs.GetInt(VibrationEnabledKey, DefaultVibrationEnabled ? 1 : 0) != 0;
            set
            {
                PlayerPrefs.SetInt(VibrationEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool IsSoundEnabled
        {
            get => PlayerPrefs.GetInt(SoundEnabledKey, DefaultSoundEnabled ? 1 : 0) != 0;
            set
            {
                PlayerPrefs.SetInt(SoundEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool IsDarkModeEnabled
        {
            get => PlayerPrefs.GetInt(DarkModeEnabledKey, DefaultDarkModeEnabled ? 1 : 0) != 0;
            set
            {
                PlayerPrefs.SetInt(DarkModeEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool TryConsumeHint()
        {
            if (HintCount <= 0)
                return false;

            HintCount--;
            return true;
        }

        public static int GetCurrentChallengeCycleIndex(DateTime utcNow)
        {
            int elapsedDays = Mathf.Max(0, GetUtcDayNumber(utcNow) - GetUtcDayNumber(ChallengeEpochUtc));
            return elapsedDays / ChallengeCycleLengthDays;
        }

        public static int GetCurrentChallengeDayIndex(DateTime utcNow)
        {
            int elapsedDays = Mathf.Max(0, GetUtcDayNumber(utcNow) - GetUtcDayNumber(ChallengeEpochUtc));
            return elapsedDays % ChallengeCycleLengthDays;
        }

        public static DateTime GetCurrentChallengeCycleStartUtc(DateTime utcNow)
        {
            return ChallengeEpochUtc.AddDays(GetCurrentChallengeCycleIndex(utcNow) * ChallengeCycleLengthDays);
        }

        public static DateTime GetCurrentChallengeCycleEndUtc(DateTime utcNow)
        {
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
            return !HasPlayedChallengeToday(utcNow);
        }

        public static int GetChallengeChancesRemainingToday(DateTime utcNow)
        {
            return CanPlayChallengeToday(utcNow) ? 1 : 0;
        }

        public static TimeSpan GetTimeUntilNextChallengeChance(DateTime utcNow)
        {
            DateTime utcDateTime = utcNow.ToUniversalTime();
            DateTime nextDay = utcDateTime.Date.AddDays(1);
            return nextDay - utcDateTime;
        }

        public static void MarkChallengeAttemptUsed(DateTime utcNow)
        {
            DateTime utcDateTime = utcNow.ToUniversalTime();
            PlayerPrefs.SetInt(ChallengeLastPlayedUtcDayKey, GetUtcDayNumber(utcDateTime));

            int cycleIndex = GetCurrentChallengeCycleIndex(utcDateTime);
            int dayIndex = GetCurrentChallengeDayIndex(utcDateTime);
            int streakMask = PlayerPrefs.GetInt(GetChallengeStreakMaskKey(cycleIndex), 0);
            streakMask |= 1 << dayIndex;
            PlayerPrefs.SetInt(GetChallengeStreakMaskKey(cycleIndex), streakMask);
            PlayerPrefs.Save();
        }

        public static int GetChallengeStreakMask(DateTime utcNow)
        {
            int cycleIndex = GetCurrentChallengeCycleIndex(utcNow);
            return PlayerPrefs.GetInt(GetChallengeStreakMaskKey(cycleIndex), 0);
        }

        public static int GetPlayedChallengeDayCount(DateTime utcNow)
        {
            int streakMask = GetChallengeStreakMask(utcNow);
            int playedCount = 0;
            for (int i = 0; i < ChallengeCycleLengthDays; i++)
            {
                if ((streakMask & (1 << i)) != 0)
                    playedCount++;
            }

            return playedCount;
        }

        public static bool HasPlayedChallengeDay(DateTime utcNow, int dayIndex)
        {
            int streakMask = GetChallengeStreakMask(utcNow);
            return (streakMask & (1 << dayIndex)) != 0;
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
            }

            return isNewBest;
        }

        public static int GetCurrentChallengePatternIndex(DateTime utcNow, int patternCount)
        {
            int safePatternCount = Mathf.Max(patternCount, 1);
            return GetCurrentChallengeCycleIndex(utcNow) % safePatternCount;
        }

        public static int GetCurrentChallengeSeed(DateTime utcNow, int seedOffset = 0)
        {
            unchecked
            {
                return 7919 + GetCurrentChallengeCycleIndex(utcNow) * 104729 + seedOffset * 17;
            }
        }

        public static List<ChallengeLeaderboardEntryData> BuildLocalChallengeLeaderboard(DateTime utcNow, int entryCount)
        {
            int safeEntryCount = Mathf.Max(entryCount, 1);
            List<ChallengeLeaderboardEntryData> entries = new();
            int cycleIndex = GetCurrentChallengeCycleIndex(utcNow);
            float playerBestTime = GetChallengeBestTimeSeconds(utcNow);

            if (playerBestTime > 0f)
            {
                entries.Add(new ChallengeLeaderboardEntryData
                {
                    playerName = ChallengePlayerName,
                    completionSeconds = playerBestTime,
                    isPlayer = true
                });
            }

            System.Random random = new(cycleIndex * 3571 + 91);
            float baselineSeconds = playerBestTime > 0f ? Mathf.Clamp(playerBestTime, 18f, 360f) : 72f + cycleIndex * 1.7f;
            int rivalCount = Mathf.Max(safeEntryCount + 3, 8);
            for (int i = 0; i < rivalCount; i++)
            {
                float variation = (float)(random.NextDouble() * 28d - 10d);
                float rivalTime = Mathf.Max(12f, baselineSeconds + variation + i * 0.85f);
                entries.Add(new ChallengeLeaderboardEntryData
                {
                    playerName = $"{ChallengeRivalNames[i % ChallengeRivalNames.Length]} {random.Next(1, 90)}",
                    completionSeconds = rivalTime,
                    isPlayer = false
                });
            }

            entries.Sort((left, right) => left.completionSeconds.CompareTo(right.completionSeconds));

            List<ChallengeLeaderboardEntryData> rankedEntries = new();
            for (int i = 0; i < entries.Count && rankedEntries.Count < safeEntryCount; i++)
            {
                ChallengeLeaderboardEntryData entry = entries[i];
                entry.rank = i + 1;
                rankedEntries.Add(entry);
            }

            if (playerBestTime > 0f && rankedEntries.TrueForAll(entry => !entry.isPlayer))
            {
                int playerRank = entries.FindIndex(entry => entry.isPlayer) + 1;
                rankedEntries.Add(new ChallengeLeaderboardEntryData
                {
                    rank = playerRank,
                    playerName = ChallengePlayerName,
                    completionSeconds = playerBestTime,
                    isPlayer = true
                });
            }

            return rankedEntries;
        }

        private static int GetUtcDayNumber(DateTime utcNow)
        {
            return Mathf.FloorToInt((float)(utcNow.ToUniversalTime().Date - UnixEpochUtc).TotalDays);
        }

        private static string GetChallengeStreakMaskKey(int cycleIndex)
        {
            return $"challenge_cycle_{cycleIndex}_streak_mask";
        }

        private static string GetChallengeBestTimeKey(int cycleIndex)
        {
            return $"challenge_cycle_{cycleIndex}_best_time";
        }
    }
}
