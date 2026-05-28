using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ArrowGame.Data;
using UnityEngine;

namespace ArrowGame
{
    public class MiniPayBridge : MonoBehaviour
    {
        [Serializable]
        private sealed class PurchaseRequestPayload
        {
            public string productId;
            public int amount;
            public string walletAddress;
        }

        [Serializable]
        private sealed class ChallengeResultPayload
        {
            public string walletAddress;
            public string playerName;
            public int cycleIndex;
            public string patternName;
            public float completionSeconds;
        }

        [Serializable]
        private sealed class ChallengeLeaderboardRequestPayload
        {
            public int cycleIndex;
            public string patternName;
            public int limit;
            public string walletAddress;
        }

        [Serializable]
        private sealed class ChallengeLeaderboardPayload
        {
            public int cycleIndex;
            public int leaderboardCycleIndex;
            public string patternName;
            public string leaderboardPatternName;
            public ChallengeLeaderboardBridgeEntry[] entries;
        }

        [Serializable]
        private sealed class ChallengeLeaderboardBridgeEntry
        {
            public int rank;
            public string playerName;
            public string walletAddress;
            public float completionSeconds;
        }

        private const string BridgeObjectName = "MiniPayBridge";
        private const string HintPackageId = "arrow_out_hints_5";
        private const string RevivePackageId = "arrow_out_revive_1";
        private const int DefaultLeaderboardRequestLimit = 25;
        private const float UserStateSyncDebounceSeconds = 0.75f;
        private const float BootstrapRetryDelaySeconds = 0.6f;
        private const int MaxBootstrapRetryCount = 6;

        private static MiniPayBridge instance;
        private int lastRequestedLeaderboardCycleIndex = -1;
        private string lastRequestedLeaderboardPatternName = string.Empty;
        private Coroutine queuedUserStateSyncCoroutine;
        private Coroutine bootstrapRetryCoroutine;
        private string pendingUserStateSyncReason = string.Empty;
        private string lastSyncedSnapshotJson = string.Empty;
        private int bootstrapRetryCount;

        public static event Action InitialStateResolved;
        public static event Action<string> GamePurchaseStatusReceived;
        public static event Action<string> GamePurchaseFailed;
        public static event Action GamePurchaseSucceeded;
        public static event Action<string> HintPurchaseFailed;
        public static event Action HintPurchaseSucceeded;
        public static event Action<string> RevivePurchaseFailed;
        public static event Action RevivePurchaseSucceeded;
        public static event Action<string> LivesPurchaseFailed;
        public static event Action LivesPurchaseSucceeded;
        public static event Action ChallengeLeaderboardSubmitted;
        public static event Action<string> ChallengeLeaderboardSubmitFailed;
        public static event Action<string> BridgeLogReceived;

        public bool HasResolvedInitialState { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
[DllImport("__Internal")]
private static extern void MiniPayBridge_RequestBootstrap();

[DllImport("__Internal")]
private static extern void MiniPayBridge_Initialize();

[DllImport("__Internal")]
private static extern void MiniPayBridge_SyncUserState(string snapshotJson);

[DllImport("__Internal")]
private static extern void MiniPayBridge_PurchaseGame(string token);

[DllImport("__Internal")]
private static extern void MiniPayBridge_BuyHints(int amount, string token);

[DllImport("__Internal")]
private static extern void MiniPayBridge_BuyRevive(int amount, string token, string mode);

[DllImport("__Internal")]
private static extern void MiniPayBridge_SubmitChallengeScore(string payloadJson);

[DllImport("__Internal")]
private static extern void MiniPayBridge_RequestLeaderboard(string payloadJson);
#endif

        public static MiniPayBridge Instance => EnsureInstance();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInstance();
        }

        private static MiniPayBridge EnsureInstance()
        {
            if (instance != null)
                return instance;

            instance = FindAnyObjectByType<MiniPayBridge>();
            if (instance != null)
            {
                instance.EnsureSingletonState();
                return instance;
            }

            GameObject bridgeObject = new(BridgeObjectName);
            instance = bridgeObject.AddComponent<MiniPayBridge>();
            instance.EnsureSingletonState();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            EnsureSingletonState();
        }

        private void Start()
        {
            RequestBootstrap();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                RequestBootstrap();
        }

        private void EnsureSingletonState()
        {
            gameObject.name = BridgeObjectName;
            DontDestroyOnLoad(gameObject);
            InitializeInterop();
        }

        private static void InitializeInterop()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_Initialize();
#endif
        }

        public void RequestBootstrap()
        {
            if (bootstrapRetryCoroutine != null)
            {
                StopCoroutine(bootstrapRetryCoroutine);
                bootstrapRetryCoroutine = null;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_RequestBootstrap();
#else
            ResolveInitialState();
            Debug.Log("MiniPayBridge.RequestBootstrap editor fallback");
#endif
        }

        public static void RequestUserStateSync(string reason = "")
        {
            if (!Application.isPlaying)
                return;

            Instance.QueueUserStateSync(reason);
        }

        public void SyncCurrentState(string reason = "")
        {
            pendingUserStateSyncReason = string.Empty;
            if (queuedUserStateSyncCoroutine != null)
            {
                StopCoroutine(queuedUserStateSyncCoroutine);
                queuedUserStateSyncCoroutine = null;
            }

            SendCurrentState(reason);
        }

        private void QueueUserStateSync(string reason = "")
        {
            if (!string.IsNullOrWhiteSpace(reason))
            {
                pendingUserStateSyncReason = string.IsNullOrWhiteSpace(pendingUserStateSyncReason)
                    ? reason.Trim()
                    : $"{pendingUserStateSyncReason}, {reason.Trim()}";
            }

            if (queuedUserStateSyncCoroutine != null)
                return;

            queuedUserStateSyncCoroutine = StartCoroutine(QueueUserStateSyncCO());
        }

        private IEnumerator QueueUserStateSyncCO()
        {
            yield return new WaitForSecondsRealtime(UserStateSyncDebounceSeconds);
            queuedUserStateSyncCoroutine = null;

            string reason = pendingUserStateSyncReason;
            pendingUserStateSyncReason = string.Empty;
            SendCurrentState(reason);
        }

        private void SendCurrentState(string reason = "")
        {
            string payloadJson = GameDataStore.BuildBridgeSnapshotJson(DateTime.UtcNow);
            if (string.Equals(payloadJson, lastSyncedSnapshotJson, StringComparison.Ordinal))
                return;

#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_SyncUserState(payloadJson);
            lastSyncedSnapshotJson = payloadJson;
#else
            Debug.Log($"MiniPayBridge.SyncCurrentState editor fallback ({reason})");
            lastSyncedSnapshotJson = payloadJson;
#endif
        }

        public void PurchaseGame()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_PurchaseGame("USDT");
#else
            GameDataStore.HasPurchasedGame = true;
            ResolveInitialState();
            GamePurchaseSucceeded?.Invoke();
            Debug.Log("MiniPayBridge.PurchaseGame editor fallback");
#endif
        }

        public void BuyHints(int amount)
        {
            PurchaseRequestPayload payload = new()
            {
                productId = HintPackageId,
                amount = Mathf.Max(0, amount),
                walletAddress = GameDataStore.WalletAddress
            };

#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_BuyHints(
    payload.amount,
    "USDT"
);
#else
            if (amount > 0)
                GameDataStore.AddHints(amount);
            ResolveInitialState();
            HintPurchaseSucceeded?.Invoke();
            Debug.Log("MiniPayBridge.BuyHints editor fallback");
#endif
        }

        public void BuyRevive(int amount = 1, string mode = "classic")
        {
            PurchaseRequestPayload payload = new()
            {
                productId = RevivePackageId,
                amount = Mathf.Max(0, amount),
                walletAddress = GameDataStore.WalletAddress
            };

            string purchaseMode = string.Equals(mode, "challenge", StringComparison.OrdinalIgnoreCase)
                ? "challenge"
                : "classic";

#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_BuyRevive(
    payload.amount,
    "USDT",
    purchaseMode
);
#else
            ResolveInitialState();
            RevivePurchaseSucceeded?.Invoke();
            LivesPurchaseSucceeded?.Invoke();
            Debug.Log("MiniPayBridge.BuyRevive editor fallback");
#endif
        }

        public void BuyLives(int amount)
        {
            BuyRevive(amount);
        }

        public void SubmitChallengeResult(float completionSeconds, string patternName)
        {
            ChallengeResultPayload payload = new()
            {
                walletAddress = GameDataStore.WalletAddress,
                playerName = GameDataStore.ChallengePlayerName,
                cycleIndex = GameDataStore.GetCurrentChallengeCycleIndex(DateTime.UtcNow),
                patternName = patternName,
                completionSeconds = Mathf.Max(0f, completionSeconds)
            };

#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_SubmitChallengeScore(
    JsonUtility.ToJson(payload)
);
#else
            Debug.Log("MiniPayBridge.SubmitChallengeResult editor fallback");
#endif
        }

        public void RequestChallengeLeaderboard(string patternName, int limit = DefaultLeaderboardRequestLimit)
        {
            lastRequestedLeaderboardPatternName = patternName?.Trim() ?? string.Empty;
            lastRequestedLeaderboardCycleIndex = GameDataStore.GetCurrentChallengeCycleIndex(DateTime.UtcNow);

            ChallengeLeaderboardRequestPayload payload = new()
            {
                cycleIndex = lastRequestedLeaderboardCycleIndex,
                patternName = lastRequestedLeaderboardPatternName,
                limit = Mathf.Max(1, limit),
                walletAddress = GameDataStore.WalletAddress
            };

#if UNITY_WEBGL && !UNITY_EDITOR
           MiniPayBridge_RequestLeaderboard(
    JsonUtility.ToJson(payload)
);
#else
            Debug.Log("MiniPayBridge.RequestChallengeLeaderboard editor fallback");
#endif
        }

        public void OnBootstrapDataReceived(string json)
        {
            if (TryApplyBridgeSnapshot(json))
            {
                bootstrapRetryCount = 0;
                ResolveInitialState();
                return;
            }

            if (Application.isPlaying && bootstrapRetryCount < MaxBootstrapRetryCount)
            {
                bootstrapRetryCount++;
                if (bootstrapRetryCoroutine != null)
                    StopCoroutine(bootstrapRetryCoroutine);
                bootstrapRetryCoroutine = StartCoroutine(RetryBootstrapAfterDelay());
                Debug.LogWarning($"MiniPayBridge received empty bootstrap snapshot. Retrying bootstrap ({bootstrapRetryCount}/{MaxBootstrapRetryCount}).");
                return;
            }

            Debug.LogWarning("MiniPayBridge bootstrap snapshot was empty after retries. Waiting for a usable remote snapshot before resolving initial state.");
        }

        public void OnUserStateSynced(string json)
        {
            if (TryApplyBridgeSnapshot(json))
            {
                bootstrapRetryCount = 0;
                ResolveInitialState();
            }
        }

        public void OnWalletAddressResolved(string walletAddress)
        {
            if (!string.IsNullOrWhiteSpace(walletAddress))
                GameDataStore.WalletAddress = walletAddress.Trim();
        }

        public void OnGamePurchaseSuccess(string json)
        {
            if (TryApplyBridgeSnapshot(json))
            {
                bootstrapRetryCount = 0;
            }
            else
            {
                GameDataStore.HasPurchasedGame = true;
            }

            ResolveInitialState();
            GamePurchaseSucceeded?.Invoke();
        }

        public void OnGamePurchaseStatus(string message)
        {
            ResolveInitialState();
            GamePurchaseStatusReceived?.Invoke(string.IsNullOrWhiteSpace(message) ? "Waiting for MiniPay confirmation..." : message);
        }

        public void OnGamePurchaseFailed(string errorMessage)
        {
            ResolveInitialState();
            GamePurchaseFailed?.Invoke(string.IsNullOrWhiteSpace(errorMessage) ? "Purchase failed." : errorMessage);
        }

        public void OnHintPurchaseSuccess(string json)
        {
            if (TryApplyBridgeSnapshot(json))
                bootstrapRetryCount = 0;

            ResolveInitialState();
            HintPurchaseSucceeded?.Invoke();
        }

        public void OnHintPurchaseFailed(string errorMessage)
        {
            HintPurchaseFailed?.Invoke(string.IsNullOrWhiteSpace(errorMessage) ? "Hint purchase failed." : errorMessage);
        }

        public void OnRevivePurchaseSuccess(string json)
        {
            if (TryApplyBridgeSnapshot(json))
                bootstrapRetryCount = 0;

            ResolveInitialState();
            RevivePurchaseSucceeded?.Invoke();
            LivesPurchaseSucceeded?.Invoke();
        }

        public void OnRevivePurchaseFailed(string errorMessage)
        {
            string message = string.IsNullOrWhiteSpace(errorMessage) ? "Revive purchase failed." : errorMessage;
            RevivePurchaseFailed?.Invoke(message);
            LivesPurchaseFailed?.Invoke(message);
        }

        public void OnLivesPurchaseSuccess(string json)
        {
            OnRevivePurchaseSuccess(json);
        }

        public void OnLivesPurchaseFailed(string errorMessage)
        {
            OnRevivePurchaseFailed(errorMessage);
        }

        public void OnChallengeLeaderboardReceived(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("MiniPayBridge received empty leaderboard payload.");
                GameDataStore.ApplyChallengeLeaderboard(
                    new List<ChallengeLeaderboardEntryData>(),
                    Mathf.Max(0, lastRequestedLeaderboardCycleIndex),
                    lastRequestedLeaderboardPatternName);
                return;
            }

            int resolvedCycleIndex = lastRequestedLeaderboardCycleIndex;
            string resolvedPatternName = lastRequestedLeaderboardPatternName;
            ChallengeLeaderboardBridgeEntry[] bridgeEntries = TryParseLeaderboardEntries(json);
            TryParseLeaderboardMetadata(json, ref resolvedCycleIndex, ref resolvedPatternName);
            Debug.Log($"MiniPayBridge leaderboard parsed {bridgeEntries?.Length ?? 0} entries for cycle {resolvedCycleIndex}, pattern '{resolvedPatternName}'.");
            List<ChallengeLeaderboardEntryData> entries = new(bridgeEntries != null ? bridgeEntries.Length : 0);
            string walletAddress = GameDataStore.WalletAddress;
            for (int i = 0; bridgeEntries != null && i < bridgeEntries.Length; i++)
            {
                ChallengeLeaderboardBridgeEntry entry = bridgeEntries[i];
                entries.Add(new ChallengeLeaderboardEntryData
                {
                    rank = entry.rank > 0 ? entry.rank : i + 1,
                    playerName = string.IsNullOrWhiteSpace(entry.playerName) ? "Player" : entry.playerName,
                    completionSeconds = Mathf.Max(0f, entry.completionSeconds),
                    isPlayer = !string.IsNullOrWhiteSpace(walletAddress) &&
                               string.Equals(entry.walletAddress, walletAddress, StringComparison.OrdinalIgnoreCase)
                });
            }

            GameDataStore.ApplyChallengeLeaderboard(entries, resolvedCycleIndex, resolvedPatternName);
        }

        public void OnLeaderboardSubmitted(string _)
        {
            ChallengeLeaderboardSubmitted?.Invoke();
        }

        public void OnLeaderboardSubmitFailed(string errorMessage)
        {
            ChallengeLeaderboardSubmitFailed?.Invoke(string.IsNullOrWhiteSpace(errorMessage) ? "Leaderboard submit failed." : errorMessage);
        }

        public void OnBridgeLogReceived(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            Debug.Log($"MiniPayBridge: {message}");
            BridgeLogReceived?.Invoke(message);
        }

        private IEnumerator RetryBootstrapAfterDelay()
        {
            yield return new WaitForSecondsRealtime(BootstrapRetryDelaySeconds);
            bootstrapRetryCoroutine = null;
            RequestBootstrap();
        }

        private bool TryApplyBridgeSnapshot(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                MiniPayUserSnapshotData snapshot = JsonUtility.FromJson<MiniPayUserSnapshotData>(json);
                if (!IsUsableSnapshot(snapshot))
                    return false;

                GameDataStore.ApplyBridgeSnapshot(snapshot);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"MiniPayBridge failed to parse snapshot: {exception.Message}");
            }

            return false;
        }

        private static bool IsUsableSnapshot(MiniPayUserSnapshotData snapshot)
        {
            if (snapshot == null)
                return false;

            return !string.IsNullOrWhiteSpace(snapshot.walletAddress) ||
                   !string.IsNullOrWhiteSpace(snapshot.username) ||
                   snapshot.hasPurchasedGame ||
                   snapshot.tutorialCompleted ||
                   snapshot.hints > 0 ||
                   snapshot.revives >= 0 ||
                   snapshot.lives >= 0 ||
                   snapshot.classic != null ||
                   snapshot.challenge != null ||
                   snapshot.universal != null;
        }

        private void ResolveInitialState()
        {
            if (HasResolvedInitialState)
                return;

            HasResolvedInitialState = true;
            GameDataStore.MarkBridgeBootstrapResolved();
            InitialStateResolved?.Invoke();
        }

        private static ChallengeLeaderboardBridgeEntry[] TryParseLeaderboardEntries(string json)
        {
            try
            {
                ChallengeLeaderboardPayload payload = JsonUtility.FromJson<ChallengeLeaderboardPayload>(json);
                if (payload != null && payload.entries != null && payload.entries.Length > 0)
                    return payload.entries;
            }
            catch
            {
            }

            try
            {
                return JsonArrayUtility.FromJson<ChallengeLeaderboardBridgeEntry>(json);
            }
            catch
            {
                return Array.Empty<ChallengeLeaderboardBridgeEntry>();
            }
        }

        private static void TryParseLeaderboardMetadata(string json, ref int cycleIndex, ref string patternName)
        {
            try
            {
                ChallengeLeaderboardPayload payload = JsonUtility.FromJson<ChallengeLeaderboardPayload>(json);
                if (payload == null)
                    return;

                int payloadCycleIndex = payload.cycleIndex >= 0 ? payload.cycleIndex : payload.leaderboardCycleIndex;
                if (payloadCycleIndex >= 0)
                    cycleIndex = payloadCycleIndex;

                string payloadPatternName = !string.IsNullOrWhiteSpace(payload.patternName)
                    ? payload.patternName
                    : payload.leaderboardPatternName;
                if (!string.IsNullOrWhiteSpace(payloadPatternName))
                    patternName = payloadPatternName.Trim();
            }
            catch
            {
            }
        }

        private static class JsonArrayUtility
        {
            [Serializable]
            private sealed class Wrapper<T>
            {
                public T[] items;
            }

            public static T[] FromJson<T>(string json)
            {
                if (string.IsNullOrWhiteSpace(json))
                    return Array.Empty<T>();

                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>("{\"items\":" + json + "}");
                return wrapper?.items ?? Array.Empty<T>();
            }
        }
    }
}
