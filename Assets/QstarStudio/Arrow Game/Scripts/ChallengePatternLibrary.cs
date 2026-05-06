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
        [SerializeField, Range(0f, 1f)] private float shapeFillThreshold = 0.45f;
        [SerializeField, Range(0f, 1f)] private float minimumAlpha = 0.1f;
        [SerializeField] private int shapePadding = 1;

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

        public bool TryBuildCurrentWeeklyMask(out bool[,] mask)
        {
            Sprite currentImage = GetCurrentWeeklyImage();
            return TryBuildMask(currentImage, out mask);
        }

        public bool TryBuildMask(Sprite sprite, out bool[,] mask)
        {
            mask = null;
            if (sprite == null || challengeGridWidth <= 0 || challengeGridHeight <= 0)
                return false;

            Texture2D texture = sprite.texture;
            if (texture == null)
                return false;

            Color[] pixels;
            Rect rect = sprite.rect;

            try
            {
                pixels = texture.GetPixels(
                    Mathf.RoundToInt(rect.x),
                    Mathf.RoundToInt(rect.y),
                    Mathf.RoundToInt(rect.width),
                    Mathf.RoundToInt(rect.height));
            }
            catch (UnityException)
            {
                Debug.LogWarning("ChallengePatternLibrary could not read sprite pixels. Enable Read/Write on the challenge source texture.");
                return false;
            }

            int sourceWidth = Mathf.RoundToInt(rect.width);
            int sourceHeight = Mathf.RoundToInt(rect.height);
            if (sourceWidth <= 0 || sourceHeight <= 0)
                return false;

            mask = new bool[challengeGridWidth, challengeGridHeight];

            int contentWidth = Mathf.Max(1, challengeGridWidth - shapePadding * 2);
            int contentHeight = Mathf.Max(1, challengeGridHeight - shapePadding * 2);

            float scale = Mathf.Min((float)contentWidth / sourceWidth, (float)contentHeight / sourceHeight);
            float placedWidth = sourceWidth * scale;
            float placedHeight = sourceHeight * scale;
            float offsetX = (challengeGridWidth - placedWidth) * 0.5f;
            float offsetY = (challengeGridHeight - placedHeight) * 0.5f;

            for (int x = 0; x < challengeGridWidth; x++)
            {
                for (int y = 0; y < challengeGridHeight; y++)
                {
                    float localX = (x + 0.5f - offsetX) / placedWidth;
                    float localY = (y + 0.5f - offsetY) / placedHeight;
                    if (localX < 0f || localX > 1f || localY < 0f || localY > 1f)
                        continue;

                    int sampleX = Mathf.Clamp(Mathf.FloorToInt(localX * sourceWidth), 0, sourceWidth - 1);
                    int sampleY = Mathf.Clamp(Mathf.FloorToInt(localY * sourceHeight), 0, sourceHeight - 1);
                    Color sample = pixels[sampleY * sourceWidth + sampleX];
                    if (IsFilledPixel(sample))
                        mask[x, y] = true;
                }
            }

            CleanupIsolatedCells(mask);
            return HasAnyFilledCell(mask);
        }

        private bool IsFilledPixel(Color sample)
        {
            if (sample.a < minimumAlpha)
                return false;

            float luminance = (sample.r + sample.g + sample.b) / 3f;
            float darkness = 1f - luminance;
            return darkness >= shapeFillThreshold;
        }

        private static void CleanupIsolatedCells(bool[,] mask)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            bool[,] snapshot = (bool[,])mask.Clone();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!snapshot[x, y])
                        continue;

                    int neighborCount = 0;
                    if (x > 0 && snapshot[x - 1, y]) neighborCount++;
                    if (x < width - 1 && snapshot[x + 1, y]) neighborCount++;
                    if (y > 0 && snapshot[x, y - 1]) neighborCount++;
                    if (y < height - 1 && snapshot[x, y + 1]) neighborCount++;

                    if (neighborCount == 0)
                        mask[x, y] = false;
                }
            }
        }

        private static bool HasAnyFilledCell(bool[,] mask)
        {
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                for (int y = 0; y < mask.GetLength(1); y++)
                {
                    if (mask[x, y])
                        return true;
                }
            }

            return false;
        }
    }
}
