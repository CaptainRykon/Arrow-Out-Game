using UnityEngine;

namespace ArrowGame
{
    public class ChallengePatternLibrary : MonoBehaviour
    {
        [Header("Source Images")]
        [SerializeField] private Sprite[] sourceImages;

        [Header("Target Grid")]
        [SerializeField] private int challengeGridWidth = 30;
        [SerializeField] private int challengeGridHeight = 40;
        [SerializeField] private float shapeFillThreshold = 0.45f;

        public Sprite[] SourceImages => sourceImages;
        public int ChallengeGridWidth => challengeGridWidth;
        public int ChallengeGridHeight => challengeGridHeight;
        public float ShapeFillThreshold => shapeFillThreshold;

        public Sprite GetCurrentWeeklyImage()
        {
            if (sourceImages == null || sourceImages.Length == 0)
                return null;

            int patternIndex = Data.GameDataStore.GetCurrentChallengePatternIndex(System.DateTime.UtcNow, sourceImages.Length);
            return sourceImages[Mathf.Clamp(patternIndex, 0, sourceImages.Length - 1)];
        }
    }
}
