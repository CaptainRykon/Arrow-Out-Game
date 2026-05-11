using UnityEngine;
using ArrowGame.Data;

namespace ArrowGame
{
    public class SoundManager : MonoBehaviour
    {
        [System.Serializable]
        private sealed class SoundGroup
        {
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(-0.25f, 0.25f)] public float pitchJitter = 0.05f;
        }

        public static SoundManager Instance { get; private set; }

        [Header("Output")]
        [SerializeField] private AudioSource sfxSource;

        [Header("UI")]
        [SerializeField] private SoundGroup buttonClick = new();

        [Header("Game")]
        [SerializeField] private SoundGroup win = new();
        [SerializeField] private SoundGroup lose = new();
        [SerializeField] private SoundGroup arrowEscapeSuccess = new();
        [SerializeField] private SoundGroup arrowEscapeFail = new();
        [SerializeField] private SoundGroup wrongArrowClick = new();
        [SerializeField] private SoundGroup rightArrowClick = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioSource();
            ApplySoundEnabled(GameDataStore.IsSoundEnabled);
        }

        public static void PlayButtonClick() => Instance?.Play(Instance.buttonClick);
        public static void PlayWin() => Instance?.Play(Instance.win);
        public static void PlayLose() => Instance?.Play(Instance.lose);
        public static void PlayArrowEscapeSuccess() => Instance?.Play(Instance.arrowEscapeSuccess);
        public static void PlayArrowEscapeFail() => Instance?.Play(Instance.arrowEscapeFail);
        public static void PlayWrongArrowClick() => Instance?.Play(Instance.wrongArrowClick);
        public static void PlayRightArrowClick() => Instance?.Play(Instance.rightArrowClick);

        public static void ApplySoundEnabled(bool isEnabled)
        {
            if (Instance?.sfxSource == null)
                return;

            Instance.sfxSource.mute = !isEnabled;
        }

        private void EnsureAudioSource()
        {
            if (sfxSource == null)
                sfxSource = GetComponent<AudioSource>();

            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();

            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
        }

        private void Play(SoundGroup group)
        {
            if (!GameDataStore.IsSoundEnabled || group == null || group.clips == null || group.clips.Length == 0 || sfxSource == null)
                return;

            AudioClip clip = group.clips[Random.Range(0, group.clips.Length)];
            if (clip == null)
                return;

            float originalPitch = sfxSource.pitch;
            sfxSource.pitch = 1f + Random.Range(-group.pitchJitter, group.pitchJitter);
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(group.volume));
            sfxSource.pitch = originalPitch;
        }
    }

    public static class HapticManager
    {
        private const long LightTapDurationMs = 25L;
        private const int LightTapAmplitude = 80;
        private const long SuccessDurationMs = 45L;
        private const int SuccessAmplitude = 140;
        private const long HeavyImpactDurationMs = 90L;
        private const int HeavyImpactAmplitude = 255;

        public static void PlayButtonTap()
        {
            if (!GameDataStore.IsVibrationEnabled)
                return;

            VibrateOneShot(LightTapDurationMs, LightTapAmplitude);
        }

        public static void PlaySuccess()
        {
            if (!GameDataStore.IsVibrationEnabled)
                return;

            VibrateOneShot(SuccessDurationMs, SuccessAmplitude);
        }

        public static void PlayFailure()
        {
            if (!GameDataStore.IsVibrationEnabled)
                return;

            VibrateOneShot(HeavyImpactDurationMs, HeavyImpactAmplitude);
        }

        public static void PlayToggleEnabledPreview()
        {
            VibrateOneShot(SuccessDurationMs, SuccessAmplitude);
        }

        private static void VibrateOneShot(long durationMs, int amplitude)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject vibrator = currentActivity?.Call<AndroidJavaObject>("getSystemService", "vibrator");

            if (vibrator == null)
                return;

            if (!vibrator.Call<bool>("hasVibrator"))
                return;

            using AndroidJavaClass version = new("android.os.Build$VERSION");
            int sdkInt = version.GetStatic<int>("SDK_INT");

            if (sdkInt >= 26)
            {
                using AndroidJavaClass vibrationEffectClass = new("android.os.VibrationEffect");
                using AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", durationMs, amplitude);
                vibrator.Call("vibrate", effect);
                return;
            }

            vibrator.Call("vibrate", durationMs);
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
