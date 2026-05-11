using ArrowGame.Data;
using UnityEngine;

namespace ArrowGame
{
    public class ChallengePatternLibrary : MonoBehaviour
    {
        [SerializeField] private Texture2D[] sourceImages;
        [SerializeField] private int challengeGridWidth = 120;
        [SerializeField] private int challengeGridHeight = 180;
        [SerializeField, Range(0.05f, 1f)] private float shapeFillThreshold = 0.568f;
        [SerializeField, Range(0f, 1f)] private float minimumAlpha = 0.1f;
        [SerializeField, Min(0)] private int shapePadding = 1;

        public bool TryBuildCurrentWeeklyMask(out bool[,] challengeMask)
        {
            challengeMask = null;
            Texture2D sourceTexture = GetCurrentSourceTexture();
            if (sourceTexture == null)
                return false;

            Vector2Int targetSize = ResolveTargetSize();
            if (targetSize.x <= 0 || targetSize.y <= 0)
                return false;

            bool[,] highResolutionMask = RasterizeSource(sourceTexture, Mathf.Max(8, challengeGridWidth), Mathf.Max(8, challengeGridHeight));
            highResolutionMask = Dilate(highResolutionMask, 1);
            highResolutionMask = Erode(highResolutionMask, 1);
            highResolutionMask = Erode(highResolutionMask, 1);
            highResolutionMask = Dilate(highResolutionMask, 1);
            highResolutionMask = KeepLargestRegion(highResolutionMask);

            if (!HasAny(highResolutionMask))
                return false;

            challengeMask = DownsampleToBoard(highResolutionMask, targetSize.x, targetSize.y);
            challengeMask = FillSingleCellGaps(challengeMask);
            challengeMask = RemoveIsolatedCells(challengeMask);
            challengeMask = KeepLargestRegion(challengeMask);
            return HasAny(challengeMask);
        }

        private Texture2D GetCurrentSourceTexture()
        {
            if (sourceImages == null || sourceImages.Length == 0)
                return null;

            int cycleIndex = GameDataStore.GetCurrentChallengeCycleIndex(System.DateTime.UtcNow);
            int sourceIndex = Mathf.Abs(cycleIndex) % sourceImages.Length;
            return sourceImages[sourceIndex];
        }

        private Vector2Int ResolveTargetSize()
        {
            LineGenerator lineGenerator = GetComponent<LineGenerator>();
            if (lineGenerator != null)
                return new Vector2Int(Mathf.Max(1, lineGenerator.width), Mathf.Max(1, lineGenerator.height));

            return new Vector2Int(18, 26);
        }

        private bool[,] RasterizeSource(Texture2D sourceTexture, int targetWidth, int targetHeight)
        {
            bool[,] mask = new bool[targetWidth, targetHeight];
            for (int x = 0; x < targetWidth; x++)
            {
                for (int y = 0; y < targetHeight; y++)
                {
                    float u = (x + 0.5f) / targetWidth;
                    float v = (y + 0.5f) / targetHeight;
                    Color pixel = sourceTexture.GetPixelBilinear(u, v);
                    mask[x, y] = IsFilledPixel(pixel);
                }
            }

            return mask;
        }

        private bool IsFilledPixel(Color pixel)
        {
            if (pixel.a < minimumAlpha)
                return false;

            if (pixel.grayscale < 0.97f)
                return true;

            return pixel.a >= Mathf.Max(0.55f, minimumAlpha);
        }

        private bool[,] DownsampleToBoard(bool[,] mask, int boardWidth, int boardHeight)
        {
            GetOccupiedBounds(mask, out int minX, out int maxX, out int minY, out int maxY);

            minX = Mathf.Max(0, minX - shapePadding);
            minY = Mathf.Max(0, minY - shapePadding);
            maxX = Mathf.Min(mask.GetLength(0) - 1, maxX + shapePadding);
            maxY = Mathf.Min(mask.GetLength(1) - 1, maxY + shapePadding);

            int sourceWidth = Mathf.Max(1, maxX - minX + 1);
            int sourceHeight = Mathf.Max(1, maxY - minY + 1);
            bool[,] result = new bool[boardWidth, boardHeight];

            for (int x = 0; x < boardWidth; x++)
            {
                float startX = minX + sourceWidth * (x / (float)boardWidth);
                float endX = minX + sourceWidth * ((x + 1) / (float)boardWidth);
                int sampleMinX = Mathf.Clamp(Mathf.FloorToInt(startX), minX, maxX);
                int sampleMaxX = Mathf.Clamp(Mathf.CeilToInt(endX) - 1, minX, maxX);

                for (int y = 0; y < boardHeight; y++)
                {
                    float startY = minY + sourceHeight * (y / (float)boardHeight);
                    float endY = minY + sourceHeight * ((y + 1) / (float)boardHeight);
                    int sampleMinY = Mathf.Clamp(Mathf.FloorToInt(startY), minY, maxY);
                    int sampleMaxY = Mathf.Clamp(Mathf.CeilToInt(endY) - 1, minY, maxY);

                    int filledCount = 0;
                    int sampleCount = 0;
                    for (int sampleX = sampleMinX; sampleX <= sampleMaxX; sampleX++)
                    {
                        for (int sampleY = sampleMinY; sampleY <= sampleMaxY; sampleY++)
                        {
                            sampleCount++;
                            if (mask[sampleX, sampleY])
                                filledCount++;
                        }
                    }

                    float fillRatio = sampleCount <= 0 ? 0f : filledCount / (float)sampleCount;
                    result[x, y] = fillRatio >= shapeFillThreshold;
                }
            }

            return result;
        }

        private static bool[,] RemoveIsolatedCells(bool[,] mask)
        {
            bool[,] result = (bool[,])mask.Clone();
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                for (int y = 0; y < mask.GetLength(1); y++)
                {
                    if (!mask[x, y])
                        continue;

                    int neighborCount = CountOrthogonalNeighbors(mask, x, y);
                    if (neighborCount == 0)
                        result[x, y] = false;
                }
            }

            return result;
        }

        private static bool[,] FillSingleCellGaps(bool[,] mask)
        {
            bool[,] result = (bool[,])mask.Clone();
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                for (int y = 0; y < mask.GetLength(1); y++)
                {
                    if (mask[x, y])
                        continue;

                    int neighborCount = CountOrthogonalNeighbors(mask, x, y);
                    if (neighborCount >= 3)
                        result[x, y] = true;
                }
            }

            return result;
        }

        private static int CountOrthogonalNeighbors(bool[,] mask, int x, int y)
        {
            int count = 0;
            if (IsInside(mask, x + 1, y) && mask[x + 1, y])
                count++;
            if (IsInside(mask, x - 1, y) && mask[x - 1, y])
                count++;
            if (IsInside(mask, x, y + 1) && mask[x, y + 1])
                count++;
            if (IsInside(mask, x, y - 1) && mask[x, y - 1])
                count++;
            return count;
        }

        private static bool[,] Dilate(bool[,] mask, int radius)
        {
            bool[,] result = new bool[mask.GetLength(0), mask.GetLength(1)];
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                for (int y = 0; y < mask.GetLength(1); y++)
                {
                    bool any = false;
                    for (int dx = -radius; dx <= radius && !any; dx++)
                    {
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            int sampleX = x + dx;
                            int sampleY = y + dy;
                            if (IsInside(mask, sampleX, sampleY) && mask[sampleX, sampleY])
                            {
                                any = true;
                                break;
                            }
                        }
                    }

                    result[x, y] = any;
                }
            }

            return result;
        }

        private static bool[,] Erode(bool[,] mask, int radius)
        {
            bool[,] result = new bool[mask.GetLength(0), mask.GetLength(1)];
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                for (int y = 0; y < mask.GetLength(1); y++)
                {
                    bool all = true;
                    for (int dx = -radius; dx <= radius && all; dx++)
                    {
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            int sampleX = x + dx;
                            int sampleY = y + dy;
                            if (!IsInside(mask, sampleX, sampleY) || !mask[sampleX, sampleY])
                            {
                                all = false;
                                break;
                            }
                        }
                    }

                    result[x, y] = all;
                }
            }

            return result;
        }

        private static bool[,] KeepLargestRegion(bool[,] mask)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            bool[,] visited = new bool[width, height];
            System.Collections.Generic.List<Vector2Int> largestRegion = null;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!mask[x, y] || visited[x, y])
                        continue;

                    System.Collections.Generic.List<Vector2Int> region = new();
                    System.Collections.Generic.Queue<Vector2Int> queue = new();
                    queue.Enqueue(new Vector2Int(x, y));
                    visited[x, y] = true;

                    while (queue.Count > 0)
                    {
                        Vector2Int cell = queue.Dequeue();
                        region.Add(cell);

                        for (int i = 0; i < Directions.Length; i++)
                        {
                            Vector2Int next = cell + Directions[i];
                            if (!IsInside(mask, next.x, next.y) || visited[next.x, next.y] || !mask[next.x, next.y])
                                continue;

                            visited[next.x, next.y] = true;
                            queue.Enqueue(next);
                        }
                    }

                    if (largestRegion == null || region.Count > largestRegion.Count)
                        largestRegion = region;
                }
            }

            if (largestRegion == null)
                return mask;

            bool[,] result = new bool[width, height];
            for (int i = 0; i < largestRegion.Count; i++)
            {
                Vector2Int cell = largestRegion[i];
                result[cell.x, cell.y] = true;
            }

            return result;
        }

        private static void GetOccupiedBounds(bool[,] mask, out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = mask.GetLength(0) - 1;
            minY = mask.GetLength(1) - 1;
            maxX = 0;
            maxY = 0;
            bool found = false;

            for (int x = 0; x < mask.GetLength(0); x++)
            {
                for (int y = 0; y < mask.GetLength(1); y++)
                {
                    if (!mask[x, y])
                        continue;

                    found = true;
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            if (!found)
            {
                minX = 0;
                minY = 0;
                maxX = mask.GetLength(0) - 1;
                maxY = mask.GetLength(1) - 1;
            }
        }

        private static bool HasAny(bool[,] mask)
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

        private static bool IsInside(bool[,] mask, int x, int y)
        {
            return x >= 0 && x < mask.GetLength(0) && y >= 0 && y < mask.GetLength(1);
        }

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };
    }
}
