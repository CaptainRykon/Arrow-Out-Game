using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowGame
{
    public class LineGenerator : MonoBehaviour
    {
        [Header("Generate settings")]
        // Width of the grid.
        public int width = 20;
        // Height of the grid.
        public int height = 20;
        public int maxLineLength = 20;
        public float lineWidth = .2f;
        public float arrowSize = .5f;
        public GameObject arrowPrefab;

        [Header("Board visuals")]
        public float dotSize = .32f;
        public Color dotColor = new(0.18f, 0.18f, 0.22f, 0.55f);
        public float guideLineWidth = .25f;
        public Color guideLineColor = new(0.2f, 0.6f, 1f, 0.95f);
        public int dotSortingOrder = 1;
        public int guideLineSortingOrder = 10;
        public int mainLineSortingOrder = 5;
        public int arrowSortingOrder = 6;
        public float dotBrightenDuration = 0.28f;
        public Color dotWinHighlightColor = new(0.56f, 0.65f, 0.98f, 0.92f);
        public float dotBrightenScale = 1.22f;
        public float dotClearDuration = 0.45f;
        public float dotRippleSpreadDuration = 0.75f;
        public float dotDropDistance = 0.6f;
        public float dotEndScale = 0.2f;
        public float introSpawnOffsetPadding = 4f;

        private bool[,] occupiedPoints;
        private Material lineMaterial;
        private Transform dotsRoot;
        private Transform linesRoot;
        private Vector2 boardOffset;
        private readonly List<SpriteRenderer> dotRenderers = new();

        private static Sprite runtimeDotSprite;

        private readonly Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        private void Start()
        {
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
            InitializeGrid();
            CreateVisualRoots();
            GenerateDotGrid();
            GenerateLines();
        }

        private void InitializeGrid()
        {
            occupiedPoints = new bool[width, height];
        }

        private void MarkHeadForwardBlocked(Vector2Int headPosition, Vector2Int headDirection)
        {
            Vector2Int checkPoint = headPosition + headDirection;
            while (IsPointValid(checkPoint))
            {
                occupiedPoints[checkPoint.x, checkPoint.y] = true;
                checkPoint += headDirection;
            }
        }

        private void ClearHeadForwardBlocked(Vector2Int headPosition, Vector2Int headDirection)
        {
            Vector2Int checkPoint = headPosition + headDirection;
            while (IsPointValid(checkPoint))
            {
                occupiedPoints[checkPoint.x, checkPoint.y] = false;
                checkPoint += headDirection;
            }
        }

        private void GenerateLines()
        {
            while (GenerateNewLine())
            {
            }
        }

        private bool GenerateNewLine()
        {
            int maxAttempts = 100;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                attempts++;
                Vector2Int? startPoint = FindAvailableStartPoint();
                if (!startPoint.HasValue)
                    return false;

                Vector2Int currentPos = startPoint.Value;
                Vector2Int? newDirection = FindAvailableDirection(currentPos);
                if (!newDirection.HasValue)
                    continue;

                List<Vector2Int> linePoints = new() { currentPos };

                occupiedPoints[currentPos.x, currentPos.y] = true;
                Vector2Int currentDirection = -newDirection.Value;
                Vector2Int arrowDirection = newDirection.Value;
                MarkHeadForwardBlocked(currentPos, newDirection.Value);

                int segmentLength = GetSegmentLength(currentPos, currentDirection);
                int segmentLeft = maxLineLength + 1 - linePoints.Count + 1;
                segmentLength = Random.Range(1, Mathf.Min(segmentLength, segmentLeft));

                while (segmentLeft > 0 && segmentLength > 0)
                {
                    for (int i = 1; i <= segmentLength; i++)
                    {
                        Vector2Int nextPoint = currentPos + currentDirection * i;
                        if (IsPointValid(nextPoint) && !occupiedPoints[nextPoint.x, nextPoint.y])
                        {
                            linePoints.Add(nextPoint);
                            occupiedPoints[nextPoint.x, nextPoint.y] = true;
                            segmentLeft--;
                        }
                        else
                        {
                            break;
                        }
                    }

                    currentPos = linePoints[^1];

                    newDirection = FindAvailableSegments(currentPos);
                    if (newDirection.HasValue && newDirection.Value != -currentDirection)
                    {
                        currentDirection = newDirection.Value;
                    }
                    else
                    {
                        break;
                    }

                    segmentLength = GetSegmentLength(currentPos, currentDirection);
                    segmentLength = Random.Range(1, Mathf.Min(segmentLength, segmentLeft));
                }

                ClearHeadForwardBlocked(startPoint.Value, arrowDirection);

                if (linePoints.Count > 1)
                {
                    CreateGeneratedLine(linePoints);
                    return true;
                }

                if (IsPointSurrounded(currentPos))
                    break;
            }

            return false;
        }

        private void CreateGeneratedLine(List<Vector2Int> linePoints)
        {
            Vector2 minBounds = GetBoardMin();
            Vector2 maxBounds = GetBoardMax();

            GameObject lineObj = new("Line");
            lineObj.transform.SetParent(linesRoot, false);

            LineController lineController = lineObj.AddComponent<LineController>();
            foreach (Vector2Int linePoint in linePoints)
            {
                lineController.points.Add(linePoint - boardOffset);
            }

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.positionCount = linePoints.Count;
            lr.useWorldSpace = false;
            lr.startWidth = lr.endWidth = lineWidth;
            lr.widthMultiplier = 1f;
            lr.sortingOrder = mainLineSortingOrder;

            for (int i = 0; i < lineController.points.Count; i++)
            {
                Vector2 point = lineController.points[i];
                lr.SetPosition(i, new Vector3(point.x, point.y, 0f));
            }

            Vector2 current = lineController.points[0];
            Vector2 next = lineController.points[1];
            Vector2 direction = -((next - current)).normalized;

            lineController.arrow = CreateArrow(current, direction, lineObj.transform);
            lineController.board = Mathf.Max(width, height) * 2f;
            lineController.ConfigureBoardBounds(minBounds, maxBounds);
            lineController.ConfigureIntroOffset(GetIntroOffset(direction));
            lineController.Init();
            lineController.ConfigureGuideLine(lineMaterial, guideLineWidth, guideLineColor, guideLineSortingOrder);
        }

        private Transform CreateArrow(Vector2 position, Vector2 direction, Transform parent)
        {
            GameObject arrow = Instantiate(arrowPrefab, parent);
            arrow.transform.position = new Vector3(position.x, position.y, 0f);
            arrow.transform.localScale = Vector3.one * arrowSize;

            SpriteRenderer arrowRenderer = arrow.GetComponent<SpriteRenderer>();
            if (arrowRenderer != null)
                arrowRenderer.sortingOrder = arrowSortingOrder;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            return arrow.transform;
        }

        private Vector2Int? FindAvailableStartPoint()
        {
            Vector2Int center = new(width / 2, height / 2);

            for (int radius = 0; radius <= Mathf.Max(width, height); radius++)
            {
                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    for (int y = center.y - radius; y <= center.y + radius; y++)
                    {
                        if (x >= 1 && x < width - 1 && y >= 1 && y < height - 1)
                        {
                            if (Mathf.Abs(x - center.x) == radius || Mathf.Abs(y - center.y) == radius)
                            {
                                Vector2Int point = new(x, y);
                                if (!occupiedPoints[x, y] && FindAvailableDirection(point).HasValue)
                                    return point;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private bool IsDirectionBlocked(Vector2Int point, Vector2Int direction)
        {
            Vector2Int checkPoint = point + direction;
            while (IsPointValid(checkPoint))
            {
                if (occupiedPoints[checkPoint.x, checkPoint.y])
                    return true;

                checkPoint += direction;
            }

            return false;
        }

        private Vector2Int? FindAvailableDirection(Vector2Int point)
        {
            List<Vector2Int> availableDirections = new();

            foreach (Vector2Int dir in directions)
            {
                if (!IsDirectionBlocked(point, dir) && GetSegmentLength(point, -dir) > 1)
                    availableDirections.Add(dir);
            }

            if (availableDirections.Count == 0)
                return null;

            return availableDirections[Random.Range(0, availableDirections.Count)];
        }

        private Vector2Int? FindAvailableSegments(Vector2Int point)
        {
            List<Vector2Int> availableDirections = new();

            foreach (Vector2Int dir in directions)
            {
                if (GetSegmentLength(point, dir) > 0)
                    availableDirections.Add(dir);
            }

            if (availableDirections.Count == 0)
                return null;

            return availableDirections[Random.Range(0, availableDirections.Count)];
        }

        private int GetSegmentLength(Vector2Int startPoint, Vector2Int direction)
        {
            int maxPossibleLength = 0;

            for (int i = 1; i <= maxLineLength; i++)
            {
                Vector2Int checkPoint = startPoint + direction * i;
                if (IsPointValid(checkPoint) && !occupiedPoints[checkPoint.x, checkPoint.y])
                {
                    maxPossibleLength = i;
                }
                else
                {
                    break;
                }
            }

            return maxPossibleLength;
        }

        private bool IsPointSurrounded(Vector2Int point)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = point + dir;
                if (IsPointValid(neighbor) && !occupiedPoints[neighbor.x, neighbor.y])
                    return false;
            }

            return true;
        }

        private bool IsPointValid(Vector2Int point)
        {
            return point.x >= 0 && point.x < width && point.y >= 0 && point.y < height;
        }

        private void CreateVisualRoots()
        {
            boardOffset = new Vector2(width / 2f, height / 2f);
            dotsRoot = CreateChildRoot("Dot Grid");
            linesRoot = CreateChildRoot("Generated Lines");
        }

        private Transform CreateChildRoot(string rootName)
        {
            GameObject root = new(rootName);
            root.transform.SetParent(transform, false);
            return root.transform;
        }

        private void GenerateDotGrid()
        {
            Sprite dotSprite = GetOrCreateRuntimeDotSprite();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GameObject dotObject = new($"Dot_{x}_{y}");
                    dotObject.transform.SetParent(dotsRoot, false);
                    dotObject.transform.localPosition = new Vector3(x - boardOffset.x, y - boardOffset.y, 0f);
                    dotObject.transform.localScale = Vector3.one * dotSize;

                    SpriteRenderer dotRenderer = dotObject.AddComponent<SpriteRenderer>();
                    dotRenderer.sprite = dotSprite;
                    dotRenderer.color = dotColor;
                    dotRenderer.sortingOrder = dotSortingOrder;
                    dotRenderers.Add(dotRenderer);
                }
            }
        }

        public IEnumerator PlayDotClearAnimation()
        {
            if (dotRenderers.Count == 0)
                yield break;

            List<Vector3> startPositions = new(dotRenderers.Count);
            List<Color> startColors = new(dotRenderers.Count);
            List<Vector3> startScales = new(dotRenderers.Count);
            List<float> startTimes = new(dotRenderers.Count);
            Vector2 boardCenter = (GetBoardMin() + GetBoardMax()) * 0.5f;
            float maxDistance = 0.0001f;

            foreach (SpriteRenderer dotRenderer in dotRenderers)
            {
                startPositions.Add(dotRenderer.transform.localPosition);
                startColors.Add(dotRenderer.color);
                startScales.Add(dotRenderer.transform.localScale);

                float distanceFromCenter = Vector2.Distance(dotRenderer.transform.localPosition, boardCenter);
                if (distanceFromCenter > maxDistance)
                    maxDistance = distanceFromCenter;
            }

            float brightenElapsed = 0f;
            while (brightenElapsed < dotBrightenDuration)
            {
                brightenElapsed += Time.deltaTime;
                float brightenT = Mathf.Clamp01(brightenElapsed / dotBrightenDuration);
                float easedBrightenT = 1f - Mathf.Pow(1f - brightenT, 2f);

                for (int i = 0; i < dotRenderers.Count; i++)
                {
                    SpriteRenderer dotRenderer = dotRenderers[i];
                    if (dotRenderer == null)
                        continue;

                    dotRenderer.color = Color.Lerp(startColors[i], dotWinHighlightColor, easedBrightenT);
                    dotRenderer.transform.localScale = Vector3.Lerp(startScales[i], startScales[i] * dotBrightenScale, easedBrightenT);
                }

                yield return null;
            }

            foreach (SpriteRenderer dotRenderer in dotRenderers)
            {
                float distanceFromCenter = Vector2.Distance(dotRenderer.transform.localPosition, boardCenter);
                float normalizedDistance = distanceFromCenter / maxDistance;
                startTimes.Add(normalizedDistance * dotRippleSpreadDuration);
            }

            float elapsed = 0f;
            float totalDuration = dotRippleSpreadDuration + dotClearDuration;
            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;

                for (int i = 0; i < dotRenderers.Count; i++)
                {
                    SpriteRenderer dotRenderer = dotRenderers[i];
                    if (dotRenderer == null)
                        continue;

                    float localElapsed = elapsed - startTimes[i];
                    float localT = Mathf.Clamp01(localElapsed / dotClearDuration);
                    if (localT <= 0f)
                    {
                        dotRenderer.color = dotWinHighlightColor;
                        dotRenderer.transform.localScale = startScales[i] * dotBrightenScale;
                        continue;
                    }

                    float easedT = 1f - Mathf.Pow(1f - localT, 3f);
                    Vector3 startPosition = startPositions[i];
                    dotRenderer.transform.localPosition = startPosition + Vector3.down * dotDropDistance * easedT;
                    dotRenderer.transform.localScale = Vector3.Lerp(startScales[i] * dotBrightenScale, startScales[i] * dotEndScale, easedT);

                    Color color = dotWinHighlightColor;
                    color.a = Mathf.Lerp(dotWinHighlightColor.a, 0f, easedT);
                    dotRenderer.color = color;
                }

                yield return null;
            }

            for (int i = 0; i < dotRenderers.Count; i++)
            {
                SpriteRenderer dotRenderer = dotRenderers[i];
                if (dotRenderer != null)
                {
                    dotRenderer.transform.localScale = startScales[i] * dotEndScale;
                    dotRenderer.enabled = false;
                }
            }
        }

        private Vector2 GetBoardMin()
        {
            return new Vector2(-boardOffset.x, -boardOffset.y);
        }

        private Vector2 GetBoardMax()
        {
            return new Vector2(width - 1 - boardOffset.x, height - 1 - boardOffset.y);
        }

        private Vector2 GetIntroOffset(Vector2 direction)
        {
            float horizontalOffset = width * 0.5f + introSpawnOffsetPadding;
            float verticalOffset = height * 0.5f + introSpawnOffsetPadding;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return Vector2.right * Mathf.Sign(direction.x) * horizontalOffset;

            return Vector2.up * Mathf.Sign(direction.y) * verticalOffset;
        }

        private static Sprite GetOrCreateRuntimeDotSprite()
        {
            if (runtimeDotSprite != null)
                return runtimeDotSprite;

            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            runtimeDotSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f), 1f);
            runtimeDotSprite.name = "RuntimeDotSprite";

            return runtimeDotSprite;
        }
    }
}
