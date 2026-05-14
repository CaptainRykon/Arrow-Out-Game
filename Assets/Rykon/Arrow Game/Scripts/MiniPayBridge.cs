using System;
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
        private const string HintPackageId = "arrow_out_hints_10";
        private const string LivesPackageId = "arrow_out_lives_3";
        private const int DefaultLeaderboardRequestLimit = 10;

        private static MiniPayBridge instance;

        public static event Action InitialStateResolved;
        public static event Action<string> GamePurchaseFailed;
        public static event Action GamePurchaseSucceeded;
        public static event Action<string> HintPurchaseFailed;
        public static event Action HintPurchaseSucceeded;
        public static event Action<string> LivesPurchaseFailed;
        public static event Action LivesPurchaseSucceeded;
        public static event Action<string> BridgeLogReceived;

        public bool HasResolvedInitialState { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void MiniPayBridge_RequestBootstrap();
        [DllImport("__Internal")] private static extern void MiniPayBridge_SyncUserState(string payloadJson);
        [DllImport("__Internal")] private static extern void MiniPayBridge_PurchaseGame();
        [DllImport("__Internal")] private static extern void MiniPayBridge_BuyHints(string payloadJson);
        [DllImport("__Internal")] private static extern void MiniPayBridge_BuyLives(string payloadJson);
        [DllImport("__Internal")] private static extern void MiniPayBridge_SubmitChallengeResult(string payloadJson);
        [DllImport("__Internal")] private static extern void MiniPayBridge_RequestChallengeLeaderboard(string payloadJson);
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
        }

        public void RequestBootstrap()
        {
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

            Instance.SyncCurrentState(reason);
        }

        public void SyncCurrentState(string reason = "")
        {
            string payloadJson = GameDataStore.BuildBridgeSnapshotJson(DateTime.UtcNow);

#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_SyncUserState(payloadJson);
#else
            Debug.Log($"MiniPayBridge.SyncCurrentState editor fallback ({reason})");
#endif
        }

        public void PurchaseGame()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_PurchaseGame();
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
            MiniPayBridge_BuyHints(JsonUtility.ToJson(payload));
#else
            if (amount > 0)
                GameDataStore.AddHints(amount);
            ResolveInitialState();
            HintPurchaseSucceeded?.Invoke();
            Debug.Log("MiniPayBridge.BuyHints editor fallback");
#endif
        }

        public void BuyLives(int amount)
        {
            PurchaseRequestPayload payload = new()
            {
                productId = LivesPackageId,
                amount = Mathf.Max(0, amount),
                walletAddress = GameDataStore.WalletAddress
            };

#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_BuyLives(JsonUtility.ToJson(payload));
#else
            if (amount > 0)
                GameDataStore.AddLives(amount);
            ResolveInitialState();
            LivesPurchaseSucceeded?.Invoke();
            Debug.Log("MiniPayBridge.BuyLives editor fallback");
#endif
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
            MiniPayBridge_SubmitChallengeResult(JsonUtility.ToJson(payload));
#else
            Debug.Log("MiniPayBridge.SubmitChallengeResult editor fallback");
#endif
        }

        public void RequestChallengeLeaderboard(string patternName, int limit = DefaultLeaderboardRequestLimit)
        {
            ChallengeLeaderboardRequestPayload payload = new()
            {
                cycleIndex = GameDataStore.GetCurrentChallengeCycleIndex(DateTime.UtcNow),
                patternName = patternName,
                limit = Mathf.Max(1, limit),
                walletAddress = GameDataStore.WalletAddress
            };

#if UNITY_WEBGL && !UNITY_EDITOR
            MiniPayBridge_RequestChallengeLeaderboard(JsonUtility.ToJson(payload));
#else
            Debug.Log("MiniPayBridge.RequestChallengeLeaderboard editor fallback");
#endif
        }

        public void OnBootstrapDataReceived(string json)
        {
            TryApplyBridgeSnapshot(json);
            ResolveInitialState();
        }

        public void OnUserStateSynced(string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
                TryApplyBridgeSnapshot(json);
        }

        public void OnWalletAddressResolved(string walletAddress)
        {
            if (!string.IsNullOrWhiteSpace(walletAddress))
                GameDataStore.WalletAddress = walletAddress.Trim();

            ResolveInitialState();
        }

        public void OnGamePurchaseSuccess(string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
                TryApplyBridgeSnapshot(json);
            else
                GameDataStore.HasPurchasedGame = true;

            ResolveInitialState();
            GamePurchaseSucceeded?.Invoke();
        }

        public void OnGamePurchaseFailed(string errorMessage)
        {
            ResolveInitialState();
            GamePurchaseFailed?.Invoke(string.IsNullOrWhiteSpace(errorMessage) ? "Purchase failed." : errorMessage);
        }

        public void OnHintPurchaseSuccess(string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
                TryApplyBridgeSnapshot(json);

            ResolveInitialState();
            HintPurchaseSucceeded?.Invoke();
        }

        public void OnHintPurchaseFailed(string errorMessage)
        {
            HintPurchaseFailed?.Invoke(string.IsNullOrWhiteSpace(errorMessage) ? "Hint purchase failed." : errorMessage);
        }

        public void OnLivesPurchaseSuccess(string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
                TryApplyBridgeSnapshot(json);

            ResolveInitialState();
            LivesPurchaseSucceeded?.Invoke();
        }

        public void OnLivesPurchaseFailed(string errorMessage)
        {
            LivesPurchaseFailed?.Invoke(string.IsNullOrWhiteSpace(errorMessage) ? "Lives purchase failed." : errorMessage);
        }

        public void OnChallengeLeaderboardReceived(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;

            ChallengeLeaderboardBridgeEntry[] bridgeEntries = TryParseLeaderboardEntries(json);
            if (bridgeEntries == null || bridgeEntries.Length == 0)
                return;

            List<ChallengeLeaderboardEntryData> entries = new(bridgeEntries.Length);
            string walletAddress = GameDataStore.WalletAddress;
            for (int i = 0; i < bridgeEntries.Length; i++)
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

            GameDataStore.ApplyChallengeLeaderboard(entries);
        }

        public void OnBridgeLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            Debug.Log($"MiniPayBridge: {message}");
            BridgeLogReceived?.Invoke(message);
        }

        private void TryApplyBridgeSnapshot(string json)
        {
            try
            {
                MiniPayUserSnapshotData snapshot = JsonUtility.FromJson<MiniPayUserSnapshotData>(json);
                if (snapshot != null)
                    GameDataStore.ApplyBridgeSnapshot(snapshot);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"MiniPayBridge failed to parse snapshot: {exception.Message}");
            }
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
