using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArrowGame
{
    public class LineGenerator : MonoBehaviour
    {
        public bool AutoGenerateOnEnable = true;

        [Header("Grid")]
        public int width = 10;
        public int height = 10;
        public int maxLineLength = 10;

        [Header("Line Visuals")]
        public float renderCellSpacing = 1.7f;
        public float lineWidth = 0.2f;
        public float arrowSize = 0.5f;
        public GameObject arrowPrefab;
        public float dotSize = 0.2f;
        public Color dotColor = new(0f, 0.14f, 1f, 0.39f);
        public float guideLineWidth = 0.1f;
        public Color guideLineColor = new(0f, 0.14f, 1f, 0.52f);
        public float segmentEndpointInset = 0.08f;
        public float headEndpointInset = 0.34f;
        public float tailEndpointInset = 0.26f;
        public float arrowScaleMultiplier = 1.2f;

        [Header("Segment Aggregation")]
        public int minSegmentEdges = 3;
        public int maxSegmentEdges = 9;
        [Range(0f, 1f)] public float branchPassThroughChance = 0.85f;

        [Header("Arrow Spacing")]
        [Range(0.2f, 0.8f)] public float targetArrowDensity = 0.48f;
        [Range(0.02f, 0.2f)] public float densityTolerance = 0.08f;
        public int minArrowGapCells = 1;
        [Range(0f, 1f)] public float longerSegmentPriority = 0.7f;
        public int dotSortingOrder = 8;
        public int guideLineSortingOrder = 10;
        public int mainLineSortingOrder = 5;
        public int arrowSortingOrder = 6;

        [Header("Dot Win Animation")]
        public float dotBrightenDuration = 0.28f;
        public Color dotWinHighlightColor = new(0.56f, 0.65f, 0.98f, 0.92f);
        public float dotBrightenScale = 1.22f;
        public float challengeDotScaleMultiplier = 0.35f;
        public float challengeDotAlphaMultiplier = 0.1f;
        public float dotClearDuration = 0.45f;
        public float dotRippleSpreadDuration = 0.75f;
        public float dotDropDistance = 0.6f;
        public float dotEndScale = 0.2f;
        public float introSpawnOffsetPadding = 4f;

        public bool HasGeneratedBoard { get; private set; }

        private const float StraightWeight = 4.8f;
        private const int MinimumRunLength = 3;
        private const int GenerationAttempts = 24;
        private const int SubsetBuildAttempts = 12;
        private const int AttemptPlateauLimit = 4;
        private const float IntersectionEpsilon = 0.05f;
        private const int SparseStepSize = 2;

        private bool[,] playableMask;
        private readonly List<DotVisual> dotVisuals = new();
        private Material runtimeLineMaterial;
        private Sprite runtimeDotSprite;
        private Sprite runtimeArrowSprite;
        private Vector2 boardMin;
        private Vector2 boardMax;
        private float boardEscapeLimit = 50f;

        private void OnEnable()
        {
            if (!Application.isPlaying || !AutoGenerateOnEnable || HasGeneratedBoard)
                return;

            ArrowGameManager manager = ArrowGameManager.Instance ?? FindFirstObjectByType<ArrowGameManager>();
            if (manager != null && manager.IsChallengeMode)
                return;

            GenerateBoard();
        }

        public void SetPlayableMask(bool[,] mask)
        {
            playableMask = CloneMask(mask);
            HasGeneratedBoard = false;

            if (playableMask != null)
            {
                width = playableMask.GetLength(0);
                height = playableMask.GetLength(1);
            }
        }

        public void ClearPlayableMask()
        {
            playableMask = null;
            HasGeneratedBoard = false;
        }

        public void GenerateBoard()
        {
            ArrowGameManager manager = ArrowGameManager.Instance ?? FindFirstObjectByType<ArrowGameManager>();
            if (manager != null)
                manager.ResetBoardStateForGeneration();

            ClearGeneratedBoard();
            EnsureRuntimeAssets();

            bool[,] activeMask = BuildWorkingMask();
            if (!HasAnyActiveCell(activeMask))
            {
                HasGeneratedBoard = false;
                GetFallbackBounds(out boardMin, out boardMax);
                return;
            }

            activeMask = KeepLargestConnectedRegion(activeMask);
            ComputeBounds(activeMask, out boardMin, out boardMax);
            boardEscapeLimit = Mathf.Max(
                Mathf.Abs(boardMin.x),
                Mathf.Abs(boardMin.y),
                Mathf.Abs(boardMax.x),
                Mathf.Abs(boardMax.y)) + 8f;

            CreateDots(activeMask);

            List<PathCandidate> candidates;
            if (playableMask != null)
            {
                if (!TryBuildPuzzle(activeMask, out candidates) &&
                    !TryBuildSeparatedPuzzle(activeMask, out candidates) &&
                    !TryBuildSparsePuzzle(activeMask, out candidates))
                {
                    candidates = BuildConservativePuzzle(activeMask);
                }
            }
            else if (!TryBuildPuzzle(activeMask, out candidates))
            {
                candidates = BuildConservativePuzzle(activeMask);
            }

            foreach (PathCandidate candidate in candidates)
                SpawnLine(candidate);

            HasGeneratedBoard = true;
        }

        public void GetBoardBounds(out Vector2 minBounds, out Vector2 maxBounds)
        {
            if (HasGeneratedBoard)
            {
                minBounds = boardMin;
                maxBounds = boardMax;
                return;
            }

            bool[,] workingMask = BuildWorkingMask();
            if (HasAnyActiveCell(workingMask))
            {
                ComputeBounds(workingMask, out minBounds, out maxBounds);
                return;
            }

            GetFallbackBounds(out minBounds, out maxBounds);
        }

        public IEnumerator PlayDotClearAnimation()
        {
            if (dotVisuals.Count == 0)
                yield break;

            float brightenElapsed = 0f;
            while (brightenElapsed < dotBrightenDuration)
            {
                brightenElapsed += Time.deltaTime;
                float t = dotBrightenDuration <= 0f ? 1f : Mathf.Clamp01(brightenElapsed / dotBrightenDuration);
                for (int i = 0; i < dotVisuals.Count; i++)
                {
                    DotVisual dot = dotVisuals[i];
                    if (dot == null || dot.Renderer == null)
                        continue;

                    dot.Renderer.color = Color.Lerp(dotColor, dotWinHighlightColor, t);
                    dot.Transform.localScale = Vector3.one * Mathf.Lerp(dot.BaseScale, dot.BaseScale * dotBrightenScale, t);
                }

                yield return null;
            }

            Vector2 center = (boardMin + boardMax) * 0.5f;
            float maxDistance = 0.001f;
            for (int i = 0; i < dotVisuals.Count; i++)
                maxDistance = Mathf.Max(maxDistance, Vector2.Distance(dotVisuals[i].StartPosition, center));

            float totalDuration = dotRippleSpreadDuration + dotClearDuration;
            float clearElapsed = 0f;
            while (clearElapsed < totalDuration)
            {
                clearElapsed += Time.deltaTime;

                for (int i = 0; i < dotVisuals.Count; i++)
                {
                    DotVisual dot = dotVisuals[i];
                    if (dot == null || dot.Renderer == null)
                        continue;

                    float normalizedDistance = Vector2.Distance(dot.StartPosition, center) / maxDistance;
                    float delay = normalizedDistance * dotRippleSpreadDuration;
                    float t = Mathf.Clamp01((clearElapsed - delay) / Mathf.Max(dotClearDuration, 0.0001f));
                    if (t <= 0f)
                        continue;

                    Color fadedColor = dotWinHighlightColor;
                    fadedColor.a = Mathf.Lerp(dotWinHighlightColor.a, 0f, t);
                    dot.Renderer.color = fadedColor;
                    dot.Transform.localPosition = dot.StartPosition + Vector2.down * dotDropDistance * t;
                    dot.Transform.localScale = Vector3.one * Mathf.Lerp(dot.BaseScale * dotBrightenScale, dot.BaseScale * dotEndScale, t);
                }

                yield return null;
            }

            for (int i = 0; i < dotVisuals.Count; i++)
            {
                DotVisual dot = dotVisuals[i];
                if (dot == null || dot.Transform == null)
                    continue;

                dot.Transform.gameObject.SetActive(false);
            }
        }

        private bool TryBuildPuzzle(bool[,] activeMask, out List<PathCandidate> candidates)
        {
            candidates = null;
            List<Vector2Int> cells = GetActiveCells(activeMask);
            if (cells.Count <= 1)
            {
                candidates = new List<PathCandidate>();
                return true;
            }

            Vector2Int root = ChooseRootCell(cells);
            List<PathCandidate> best = new();
            int bestOccupiedCount = -1;
            int stagnantAttempts = 0;

            for (int attempt = 0; attempt < GenerationAttempts; attempt++)
            {
                TreeData tree = BuildTree(activeMask, root);
                if (tree == null || tree.Parent.Count <= 1)
                    continue;

                List<PathCandidate> extracted = ExtractCandidates(tree, root);
                if (extracted.Count == 0)
                    continue;

                List<PathCandidate> picked = BuildBestEffortSubset(extracted, activeMask, false);
                int occupiedCount = CountOccupiedCells(picked);
                if (IsBetterSubset(picked, occupiedCount, best, bestOccupiedCount))
                {
                    best = picked;
                    bestOccupiedCount = occupiedCount;
                    stagnantAttempts = 0;
                    if (bestOccupiedCount >= cells.Count - 2)
                        break;
                }
                else
                {
                    stagnantAttempts++;
                    if (best.Count > 0 && stagnantAttempts >= AttemptPlateauLimit)
                        break;
                }
            }

            if (best.Count == 0)
                return false;

            candidates = best;
            return true;
        }

        private bool TryBuildSparsePuzzle(bool[,] activeMask, out List<PathCandidate> candidates)
        {
            candidates = null;
            HashSet<Vector2Int> sparseNodes = BuildSparseNodeSet(activeMask);
            if (sparseNodes.Count <= 1)
                return false;

            List<Vector2Int> nodeList = sparseNodes.ToList();
            Vector2Int root = ChooseRootCell(nodeList);
            List<PathCandidate> best = new();
            int bestOccupiedCount = -1;
            int stagnantAttempts = 0;

            for (int attempt = 0; attempt < GenerationAttempts; attempt++)
            {
                TreeData tree = BuildSparseTree(activeMask, sparseNodes, root);
                if (tree == null || tree.Parent.Count <= 1)
                    continue;

                List<PathCandidate> extracted = ExtractSparseCandidates(tree, root);
                if (extracted.Count == 0)
                    continue;

                List<PathCandidate> picked = BuildBestEffortSubset(extracted, activeMask, false);
                int occupiedCount = CountOccupiedCells(picked);
                if (IsBetterSubset(picked, occupiedCount, best, bestOccupiedCount))
                {
                    best = picked;
                    bestOccupiedCount = occupiedCount;
                    stagnantAttempts = 0;
                    if (bestOccupiedCount >= sparseNodes.Count * SparseStepSize)
                        break;
                }
                else
                {
                    stagnantAttempts++;
                    if (best.Count > 0 && stagnantAttempts >= AttemptPlateauLimit)
                        break;
                }
            }

            if (best.Count == 0)
                return false;

            candidates = best;
            return true;
        }

        private bool TryBuildSeparatedPuzzle(bool[,] activeMask, out List<PathCandidate> candidates)
        {
            candidates = null;
            HashSet<Vector2Int> sparseNodes = BuildSparseNodeSet(activeMask);
            if (sparseNodes.Count <= 1)
                return false;

            List<Vector2Int> nodeList = sparseNodes.ToList();
            Vector2Int root = ChooseRootCell(nodeList);
            List<PathCandidate> best = new();
            int bestOccupiedCount = -1;
            int stagnantAttempts = 0;
            int targetOccupiedCells = Mathf.RoundToInt(GetActiveCells(activeMask).Count * targetArrowDensity);

            for (int attempt = 0; attempt < GenerationAttempts * 2; attempt++)
            {
                TreeData tree = BuildSparseTree(activeMask, sparseNodes, root);
                if (tree == null || tree.Parent.Count <= 1)
                    continue;

                List<PathCandidate> extracted = ExtractSparseCandidates(tree, root);
                if (extracted.Count == 0)
                    continue;

                if (!TrySelectSpacedCandidates(extracted, activeMask, out List<PathCandidate> selected))
                    continue;

                int occupiedCount = CountOccupiedCells(selected);
                if (IsBetterSubset(selected, occupiedCount, best, bestOccupiedCount))
                {
                    best = selected;
                    bestOccupiedCount = occupiedCount;
                    stagnantAttempts = 0;
                    if (bestOccupiedCount >= targetOccupiedCells)
                        break;
                }
                else
                {
                    stagnantAttempts++;
                    if (best.Count > 0 && stagnantAttempts >= AttemptPlateauLimit)
                        break;
                }
            }

            if (best.Count == 0)
                return false;

            candidates = best;
            return true;
        }

        private List<PathCandidate> BuildFallbackPuzzle(bool[,] activeMask)
        {
            List<Vector2Int> cells = GetActiveCells(activeMask);
            if (cells.Count <= 1)
                return new List<PathCandidate>();

            Vector2Int root = ChooseRootCell(cells);
            TreeData tree = BuildTree(activeMask, root);
            List<PathCandidate> aggregated = ExtractCandidates(tree, root);
            if (aggregated.Count > 0)
                return aggregated;

            List<PathCandidate> edgeCandidates = new();

            foreach (KeyValuePair<Vector2Int, Vector2Int> pair in tree.Parent)
            {
                if (pair.Key == root)
                    continue;

                List<Vector2Int> edgeCells = new() { pair.Key, pair.Value };
                edgeCandidates.Add(new PathCandidate(edgeCells, BuildWorldPath(edgeCells), GetDepth(tree, pair.Key)));
            }

            return edgeCandidates;
        }

        private List<PathCandidate> BuildConservativePuzzle(bool[,] activeMask)
        {
            List<PathCandidate> baseCandidates = BuildFallbackPuzzle(activeMask);
            if (baseCandidates.Count == 0)
                return baseCandidates;

            List<PathCandidate> picked = BuildBestEffortSubset(baseCandidates, activeMask, false);
            if (picked.Count > 0)
                return picked;

            return new List<PathCandidate>();
        }

        private TreeData BuildTree(bool[,] activeMask, Vector2Int root)
        {
            TreeData tree = new();
            tree.Parent[root] = root;
            tree.Depth[root] = 0;
            tree.Children[root] = new List<Vector2Int>();

            HashSet<Vector2Int> visited = new() { root };
            Stack<DfsFrame> stack = new();
            stack.Push(new DfsFrame(root, Vector2Int.zero, 0));

            while (stack.Count > 0)
            {
                DfsFrame frame = stack.Peek();
                List<Vector2Int> availableDirections = GetAvailableDirections(frame.Cell, activeMask, visited);
                if (availableDirections.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                Vector2Int chosenDirection = ChooseDirection(frame, availableDirections, activeMask);
                Vector2Int next = frame.Cell + chosenDirection;
                visited.Add(next);

                tree.Parent[next] = frame.Cell;
                tree.Depth[next] = tree.Depth[frame.Cell] + 1;
                if (!tree.Children.TryGetValue(frame.Cell, out List<Vector2Int> children))
                {
                    children = new List<Vector2Int>();
                    tree.Children[frame.Cell] = children;
                }

                children.Add(next);
                if (!tree.Children.ContainsKey(next))
                    tree.Children[next] = new List<Vector2Int>();

                int runLength = chosenDirection == frame.Direction ? frame.RunLength + 1 : 1;
                stack.Push(new DfsFrame(next, chosenDirection, runLength));
            }

            return tree;
        }

        private TreeData BuildSparseTree(bool[,] activeMask, HashSet<Vector2Int> nodeSet, Vector2Int root)
        {
            TreeData tree = new();
            tree.Parent[root] = root;
            tree.Depth[root] = 0;
            tree.Children[root] = new List<Vector2Int>();

            HashSet<Vector2Int> visited = new() { root };
            Stack<DfsFrame> stack = new();
            stack.Push(new DfsFrame(root, Vector2Int.zero, 0));

            while (stack.Count > 0)
            {
                DfsFrame frame = stack.Peek();
                List<Vector2Int> availableDirections = GetAvailableSparseDirections(frame.Cell, activeMask, nodeSet, visited);
                if (availableDirections.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                Vector2Int chosenDirection = ChooseSparseDirection(frame, availableDirections, activeMask);
                Vector2Int next = frame.Cell + chosenDirection * SparseStepSize;
                visited.Add(next);

                tree.Parent[next] = frame.Cell;
                tree.Depth[next] = tree.Depth[frame.Cell] + 1;
                if (!tree.Children.TryGetValue(frame.Cell, out List<Vector2Int> children))
                {
                    children = new List<Vector2Int>();
                    tree.Children[frame.Cell] = children;
                }

                children.Add(next);
                if (!tree.Children.ContainsKey(next))
                    tree.Children[next] = new List<Vector2Int>();

                int runLength = chosenDirection == frame.Direction ? frame.RunLength + 1 : 1;
                stack.Push(new DfsFrame(next, chosenDirection, runLength));
            }

            return tree;
        }

        private List<PathCandidate> ExtractCandidates(TreeData tree, Vector2Int root)
        {
            List<Vector2Int> orderedNodes = tree.Depth
                .Where(entry => entry.Key != root)
                .OrderByDescending(entry => entry.Value)
                .Select(entry => entry.Key)
                .ToList();

            HashSet<EdgeKey> claimedEdges = new();
            List<PathCandidate> candidates = new();

            for (int i = 0; i < orderedNodes.Count; i++)
            {
                Vector2Int start = orderedNodes[i];
                Vector2Int parent = tree.Parent[start];
                EdgeKey startEdge = new(start, parent);
                if (claimedEdges.Contains(startEdge))
                    continue;

                int targetEdges = PickTargetSegmentEdgeCount();
                List<Vector2Int> cells = new() { start };
                Vector2Int current = start;
                int traversedEdges = 0;

                while (current != root)
                {
                    Vector2Int next = tree.Parent[current];
                    EdgeKey edge = new(current, next);
                    if (claimedEdges.Contains(edge))
                        break;

                    claimedEdges.Add(edge);
                    cells.Add(next);
                    traversedEdges++;
                    current = next;

                    if (current == root)
                        break;

                    if (traversedEdges >= targetEdges)
                        break;

                    if (ShouldStopAtNode(tree, current, root, traversedEdges, targetEdges))
                        break;
                }

                if (cells.Count >= 2)
                    candidates.Add(new PathCandidate(cells, BuildWorldPath(cells), GetDepth(tree, start)));
            }

            return candidates;
        }

        private List<PathCandidate> ExtractSparseCandidates(TreeData tree, Vector2Int root)
        {
            List<Vector2Int> orderedNodes = tree.Depth
                .Where(entry => entry.Key != root)
                .OrderByDescending(entry => entry.Value)
                .Select(entry => entry.Key)
                .ToList();

            HashSet<EdgeKey> claimedEdges = new();
            List<PathCandidate> candidates = new();

            for (int i = 0; i < orderedNodes.Count; i++)
            {
                Vector2Int start = orderedNodes[i];
                Vector2Int parent = tree.Parent[start];
                EdgeKey startEdge = new(start, parent);
                if (claimedEdges.Contains(startEdge))
                    continue;

                int targetEdges = PickTargetSegmentEdgeCount();
                List<Vector2Int> nodes = new() { start };
                Vector2Int current = start;
                int traversedEdges = 0;

                while (current != root)
                {
                    Vector2Int next = tree.Parent[current];
                    EdgeKey edge = new(current, next);
                    if (claimedEdges.Contains(edge))
                        break;

                    claimedEdges.Add(edge);
                    nodes.Add(next);
                    traversedEdges++;
                    current = next;

                    if (current == root)
                        break;

                    if (traversedEdges >= targetEdges)
                        break;

                    if (ShouldStopAtNode(tree, current, root, traversedEdges, targetEdges))
                        break;
                }

                if (nodes.Count >= 2)
                    candidates.Add(new PathCandidate(nodes, BuildSparseWorldPath(nodes), GetDepth(tree, start)));
            }

            return candidates;
        }

        private bool ValidatePuzzle(List<PathCandidate> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return true;

            List<PathCandidate> alive = new(candidates);
            bool removedAny = true;
            int localWidth = Mathf.Max(width, 1);
            int localHeight = Mathf.Max(height, 1);

            while (removedAny && alive.Count > 0)
            {
                removedAny = false;
                List<int> removable = new();
                bool[,] occupiedGrid = BuildOccupiedGrid(alive, localWidth, localHeight);

                for (int i = 0; i < alive.Count; i++)
                {
                    if (CanCandidateEscape(alive[i], occupiedGrid, localWidth, localHeight))
                        removable.Add(i);
                }

                if (removable.Count == 0)
                    break;

                for (int i = removable.Count - 1; i >= 0; i--)
                {
                    alive.RemoveAt(removable[i]);
                    removedAny = true;
                }
            }

            return alive.Count == 0;
        }

        private bool CanCandidateEscape(PathCandidate source, List<PathCandidate> blockers, int selfIndex = -1)
        {
            if (source == null || source.Cells == null || source.Cells.Count < 2)
                return false;

            Vector2Int outward = source.GetHeadOutwardDirection();
            if (outward == Vector2Int.zero)
                return false;

            HashSet<Vector2Int> blockedCells = new();
            for (int cellIndex = 1; cellIndex < source.OccupiedCells.Count; cellIndex++)
                blockedCells.Add(source.OccupiedCells[cellIndex]);

            if (blockers != null)
            {
                for (int i = 0; i < blockers.Count; i++)
                {
                    PathCandidate blocker = blockers[i];
                    if (blocker == null)
                        continue;

                    for (int cellIndex = 0; cellIndex < blocker.OccupiedCells.Count; cellIndex++)
                    {
                        Vector2Int occupiedCell = blocker.OccupiedCells[cellIndex];
                        if (i == selfIndex && occupiedCell == source.Cells[0])
                            continue;

                        blockedCells.Add(occupiedCell);
                    }
                }
            }

            Vector2Int cursor = source.Cells[0] + outward;
            while (IsInside(cursor, Mathf.Max(width, 1), Mathf.Max(height, 1)))
            {
                if (blockedCells.Contains(cursor))
                    return false;

                cursor += outward;
            }

            return true;
        }

        private bool CanCandidateEscape(PathCandidate source, bool[,] occupiedGrid, int localWidth, int localHeight)
        {
            if (source == null || source.Cells == null || source.Cells.Count < 2)
                return false;

            Vector2Int outward = source.GetHeadOutwardDirection();
            if (outward == Vector2Int.zero)
                return false;

            Vector2Int cursor = source.Cells[0] + outward;
            while (IsInside(cursor, localWidth, localHeight))
            {
                if (source.ContainsOccupiedCell(cursor))
                    return false;

                if (occupiedGrid != null && occupiedGrid[cursor.x, cursor.y] && cursor != source.Cells[0])
                    return false;

                cursor += outward;
            }

            return true;
        }

        private bool[,] BuildOccupiedGrid(List<PathCandidate> candidates, int localWidth, int localHeight)
        {
            bool[,] occupied = new bool[localWidth, localHeight];
            if (candidates == null)
                return occupied;

            for (int i = 0; i < candidates.Count; i++)
            {
                PathCandidate candidate = candidates[i];
                if (candidate == null)
                    continue;

                MarkCandidateOccupied(candidate, occupied);
            }

            return occupied;
        }

        private float GetIntersectionDistance(Vector2 rayStart, Vector2 direction, List<Vector2> points)
        {
            float nearestDistance = float.PositiveInfinity;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[i + 1];
                float distance = IntersectRayWithSegment(rayStart, direction, a, b);
                if (distance >= 0f)
                    nearestDistance = Mathf.Min(nearestDistance, distance);
            }

            return float.IsPositiveInfinity(nearestDistance) ? -1f : nearestDistance;
        }

        private float IntersectRayWithSegment(Vector2 rayStart, Vector2 direction, Vector2 a, Vector2 b)
        {
            bool horizontalRay = Mathf.Abs(direction.x) > 0.5f;

            if (horizontalRay)
            {
                float y = rayStart.y;
                if (Mathf.Approximately(a.x, b.x))
                {
                    float minY = Mathf.Min(a.y, b.y) - IntersectionEpsilon;
                    float maxY = Mathf.Max(a.y, b.y) + IntersectionEpsilon;
                    if (y < minY || y > maxY)
                        return -1f;

                    float x = a.x;
                    float distance = (x - rayStart.x) / direction.x;
                    return distance > IntersectionEpsilon ? distance : -1f;
                }

                if (Mathf.Abs(a.y - y) > IntersectionEpsilon || Mathf.Abs(b.y - y) > IntersectionEpsilon)
                    return -1f;

                float minX = Mathf.Min(a.x, b.x);
                float maxX = Mathf.Max(a.x, b.x);
                if (direction.x > 0f)
                {
                    if (maxX < rayStart.x + IntersectionEpsilon)
                        return -1f;

                    float x = Mathf.Max(minX, rayStart.x + IntersectionEpsilon);
                    return x - rayStart.x;
                }

                if (minX > rayStart.x - IntersectionEpsilon)
                    return -1f;

                float hitX = Mathf.Min(maxX, rayStart.x - IntersectionEpsilon);
                return rayStart.x - hitX;
            }

            float xRay = rayStart.x;
            if (Mathf.Approximately(a.y, b.y))
            {
                float minX = Mathf.Min(a.x, b.x) - IntersectionEpsilon;
                float maxX = Mathf.Max(a.x, b.x) + IntersectionEpsilon;
                if (xRay < minX || xRay > maxX)
                    return -1f;

                float y = a.y;
                float distance = (y - rayStart.y) / direction.y;
                return distance > IntersectionEpsilon ? distance : -1f;
            }

            if (Mathf.Abs(a.x - xRay) > IntersectionEpsilon || Mathf.Abs(b.x - xRay) > IntersectionEpsilon)
                return -1f;

            float minYSegment = Mathf.Min(a.y, b.y);
            float maxYSegment = Mathf.Max(a.y, b.y);
            if (direction.y > 0f)
            {
                if (maxYSegment < rayStart.y + IntersectionEpsilon)
                    return -1f;

                float yHit = Mathf.Max(minYSegment, rayStart.y + IntersectionEpsilon);
                return yHit - rayStart.y;
            }

            if (minYSegment > rayStart.y - IntersectionEpsilon)
                return -1f;

            float hitY = Mathf.Min(maxYSegment, rayStart.y - IntersectionEpsilon);
            return rayStart.y - hitY;
        }

        private void SpawnLine(PathCandidate candidate)
        {
            GameObject lineObject = new($"Line_{candidate.Depth}");
            lineObject.transform.SetParent(transform, false);

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = runtimeLineMaterial;
            lineRenderer.useWorldSpace = false;
            lineRenderer.alignment = LineAlignment.TransformZ;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            float effectiveLineWidth = GetEffectiveLineWidth();
            lineRenderer.startWidth = effectiveLineWidth;
            lineRenderer.endWidth = effectiveLineWidth;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.numCapVertices = 4;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.sortingOrder = mainLineSortingOrder;

            LineController controller = lineObject.AddComponent<LineController>();
            controller.points = new List<Vector2>(candidate.WorldPoints);
            controller.moveSpeed = 30f;
            controller.board = boardEscapeLimit;

            Transform arrowTransform = CreateArrow(lineObject.transform, candidate.WorldPoints);
            controller.arrow = arrowTransform;
            controller.ConfigureBoardBounds(boardMin, boardMax);
            controller.ConfigureVisualSpacing(
                GetEffectiveSegmentEndpointInset(),
                GetEffectiveHeadEndpointInset(),
                GetEffectiveTailEndpointInset(),
                arrowScaleMultiplier);
            controller.ConfigureGuideLine(runtimeLineMaterial, guideLineWidth, guideLineColor, guideLineSortingOrder);
            controller.ConfigureIntroOffset(BuildIntroOffset(candidate.WorldPoints[0]));
            controller.Init();
            controller.UpdateLineRender();
        }

        private Transform CreateArrow(Transform parent, List<Vector2> points)
        {
            GameObject arrowObject = arrowPrefab != null
                ? Instantiate(arrowPrefab, parent)
                : new GameObject("Arrow", typeof(SpriteRenderer));

            arrowObject.name = "Arrow";
            arrowObject.transform.SetParent(parent, false);
            arrowObject.transform.localPosition = points[0];
            arrowObject.transform.localScale = Vector3.one * arrowSize;

            Vector2 direction = (points[0] - points[1]).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowObject.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

            SpriteRenderer spriteRenderer = arrowObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = arrowObject.AddComponent<SpriteRenderer>();

            if (spriteRenderer.sprite == null)
                spriteRenderer.sprite = runtimeArrowSprite;

            spriteRenderer.color = Color.black;
            spriteRenderer.sortingOrder = arrowSortingOrder;
            return arrowObject.transform;
        }

        private Vector2 BuildIntroOffset(Vector2 headPosition)
        {
            Vector2 center = (boardMin + boardMax) * 0.5f;
            Vector2 direction = headPosition - center;
            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector2.up;

            return direction.normalized * introSpawnOffsetPadding;
        }

        private List<Vector2> BuildWorldPath(List<Vector2Int> cells)
        {
            List<Vector2> rawPoints = new();
            for (int i = 0; i < cells.Count; i++)
                rawPoints.Add(GetCellCenter(cells[i]));

            if (rawPoints.Count <= 2)
                return rawPoints;

            List<Vector2> simplified = new();
            simplified.Add(rawPoints[0]);
            for (int i = 1; i < rawPoints.Count - 1; i++)
            {
                Vector2 previousDirection = (rawPoints[i] - rawPoints[i - 1]).normalized;
                Vector2 nextDirection = (rawPoints[i + 1] - rawPoints[i]).normalized;
                if (previousDirection != nextDirection)
                    simplified.Add(rawPoints[i]);
            }

            simplified.Add(rawPoints[rawPoints.Count - 1]);
            return simplified;
        }

        private List<Vector2> BuildSparseWorldPath(List<Vector2Int> nodes)
        {
            List<Vector2> rawPoints = new();
            if (nodes.Count == 0)
                return rawPoints;

            rawPoints.Add(GetCellCenter(nodes[0]));
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Vector2Int current = nodes[i];
                Vector2Int target = nodes[i + 1];
                Vector2Int step = new(
                    target.x == current.x ? 0 : (target.x > current.x ? 1 : -1),
                    target.y == current.y ? 0 : (target.y > current.y ? 1 : -1));

                while (current != target)
                {
                    current += step;
                    rawPoints.Add(GetCellCenter(current));
                }
            }

            if (rawPoints.Count <= 2)
                return rawPoints;

            List<Vector2> simplified = new();
            simplified.Add(rawPoints[0]);
            for (int i = 1; i < rawPoints.Count - 1; i++)
            {
                Vector2 previousDirection = (rawPoints[i] - rawPoints[i - 1]).normalized;
                Vector2 nextDirection = (rawPoints[i + 1] - rawPoints[i]).normalized;
                if (previousDirection != nextDirection)
                    simplified.Add(rawPoints[i]);
            }

            simplified.Add(rawPoints[rawPoints.Count - 1]);
            return simplified;
        }

        private void CreateDots(bool[,] activeMask)
        {
            List<Vector2Int> cells = GetActiveCells(activeMask);
            float effectiveDotSize = playableMask != null ? dotSize * challengeDotScaleMultiplier : dotSize;
            Color effectiveDotColor = dotColor;
            if (playableMask != null)
                effectiveDotColor.a *= challengeDotAlphaMultiplier;

            for (int i = 0; i < cells.Count; i++)
            {
                Vector2 position = GetCellCenter(cells[i]);
                GameObject dotObject = new($"Dot_{cells[i].x}_{cells[i].y}");
                dotObject.transform.SetParent(transform, false);
                dotObject.transform.localPosition = position;
                dotObject.transform.localScale = Vector3.one * effectiveDotSize;

                SpriteRenderer renderer = dotObject.AddComponent<SpriteRenderer>();
                renderer.sprite = runtimeDotSprite;
                renderer.color = effectiveDotColor;
                renderer.sortingOrder = dotSortingOrder;

                dotVisuals.Add(new DotVisual(dotObject.transform, renderer, position, effectiveDotSize));
            }
        }

        private void ClearGeneratedBoard()
        {
            dotVisuals.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        private void EnsureRuntimeAssets()
        {
            if (runtimeLineMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                runtimeLineMaterial = new Material(shader);
            }

            if (runtimeDotSprite == null)
                runtimeDotSprite = BuildCircleSprite("RuntimeDotSprite");

            if (runtimeArrowSprite == null)
                runtimeArrowSprite = BuildArrowSprite("RuntimeArrowSprite");
        }

        private Sprite BuildCircleSprite(string spriteName)
        {
            const int size = 32;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            Vector2 center = new(size * 0.5f, size * 0.5f);
            float radius = size * 0.42f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = distance <= radius ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = spriteName;
            return sprite;
        }

        private Sprite BuildArrowSprite(string spriteName)
        {
            const int size = 64;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            Vector2 top = new(size * 0.5f, size * 0.88f);
            Vector2 left = new(size * 0.18f, size * 0.18f);
            Vector2 right = new(size * 0.82f, size * 0.18f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new(x + 0.5f, y + 0.5f);
                    float alpha = IsPointInsideTriangle(point, top, left, right) ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = spriteName;
            return sprite;
        }

        private static bool IsPointInsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            float area = Cross(b - a, c - a);
            float area1 = Cross(a - point, b - point);
            float area2 = Cross(b - point, c - point);
            float area3 = Cross(c - point, a - point);

            bool hasNegative = area1 < 0f || area2 < 0f || area3 < 0f;
            bool hasPositive = area1 > 0f || area2 > 0f || area3 > 0f;

            return Mathf.Abs(area) > 0.0001f && !(hasNegative && hasPositive);
        }

        private static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private bool[,] BuildWorkingMask()
        {
            if (playableMask != null)
                return CloneMask(playableMask);

            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            bool[,] mask = new bool[safeWidth, safeHeight];
            for (int x = 0; x < safeWidth; x++)
            {
                for (int y = 0; y < safeHeight; y++)
                    mask[x, y] = true;
            }

            return mask;
        }

        private void ComputeBounds(bool[,] activeMask, out Vector2 minBounds, out Vector2 maxBounds)
        {
            bool foundAny = false;
            minBounds = Vector2.zero;
            maxBounds = Vector2.zero;

            int localWidth = activeMask.GetLength(0);
            int localHeight = activeMask.GetLength(1);

            for (int x = 0; x < localWidth; x++)
            {
                for (int y = 0; y < localHeight; y++)
                {
                    if (!activeMask[x, y])
                        continue;

                    Vector2 cellCenter = GetCellCenter(new Vector2Int(x, y));
                    if (!foundAny)
                    {
                        minBounds = cellCenter;
                        maxBounds = cellCenter;
                        foundAny = true;
                        continue;
                    }

                    minBounds = Vector2.Min(minBounds, cellCenter);
                    maxBounds = Vector2.Max(maxBounds, cellCenter);
                }
            }

            if (!foundAny)
                GetFallbackBounds(out minBounds, out maxBounds);
        }

        private void GetFallbackBounds(out Vector2 minBounds, out Vector2 maxBounds)
        {
            float halfWidth = Mathf.Max(0f, (Mathf.Max(width, 1) - 1) * renderCellSpacing * 0.5f);
            float halfHeight = Mathf.Max(0f, (Mathf.Max(height, 1) - 1) * renderCellSpacing * 0.5f);
            minBounds = new Vector2(-halfWidth, -halfHeight);
            maxBounds = new Vector2(halfWidth, halfHeight);
        }

        private Vector2 GetCellCenter(Vector2Int cell)
        {
            float xOffset = (Mathf.Max(width, 1) - 1) * renderCellSpacing * 0.5f;
            float yOffset = (Mathf.Max(height, 1) - 1) * renderCellSpacing * 0.5f;
            return new Vector2(cell.x * renderCellSpacing - xOffset, cell.y * renderCellSpacing - yOffset);
        }

        private float GetEffectiveLineWidth()
        {
            float maxWidthFromSpacing = renderCellSpacing * 0.07f;
            return Mathf.Min(lineWidth, maxWidthFromSpacing);
        }

        private float GetEffectiveSegmentEndpointInset()
        {
            return Mathf.Max(segmentEndpointInset, renderCellSpacing * 0.24f);
        }

        private float GetEffectiveHeadEndpointInset()
        {
            return Mathf.Max(headEndpointInset, renderCellSpacing * 0.42f);
        }

        private float GetEffectiveTailEndpointInset()
        {
            return Mathf.Max(tailEndpointInset, renderCellSpacing * 0.34f);
        }

        private static bool[,] CloneMask(bool[,] source)
        {
            if (source == null)
                return null;

            int sourceWidth = source.GetLength(0);
            int sourceHeight = source.GetLength(1);
            bool[,] clone = new bool[sourceWidth, sourceHeight];
            for (int x = 0; x < sourceWidth; x++)
            {
                for (int y = 0; y < sourceHeight; y++)
                    clone[x, y] = source[x, y];
            }

            return clone;
        }

        private static bool HasAnyActiveCell(bool[,] mask)
        {
            if (mask == null)
                return false;

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

        private static List<Vector2Int> GetActiveCells(bool[,] mask)
        {
            List<Vector2Int> cells = new();
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                for (int y = 0; y < mask.GetLength(1); y++)
                {
                    if (mask[x, y])
                        cells.Add(new Vector2Int(x, y));
                }
            }

            return cells;
        }

        private bool[,] KeepLargestConnectedRegion(bool[,] mask)
        {
            int localWidth = mask.GetLength(0);
            int localHeight = mask.GetLength(1);
            bool[,] visited = new bool[localWidth, localHeight];
            List<Vector2Int> largestRegion = null;

            for (int x = 0; x < localWidth; x++)
            {
                for (int y = 0; y < localHeight; y++)
                {
                    if (!mask[x, y] || visited[x, y])
                        continue;

                    List<Vector2Int> region = new();
                    Queue<Vector2Int> queue = new();
                    queue.Enqueue(new Vector2Int(x, y));
                    visited[x, y] = true;

                    while (queue.Count > 0)
                    {
                        Vector2Int cell = queue.Dequeue();
                        region.Add(cell);

                        for (int i = 0; i < Directions.Length; i++)
                        {
                            Vector2Int next = cell + Directions[i];
                            if (!IsInside(next, localWidth, localHeight) || visited[next.x, next.y] || !mask[next.x, next.y])
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

            bool[,] result = new bool[localWidth, localHeight];
            for (int i = 0; i < largestRegion.Count; i++)
            {
                Vector2Int cell = largestRegion[i];
                result[cell.x, cell.y] = true;
            }

            return result;
        }

        private Vector2Int ChooseRootCell(List<Vector2Int> cells)
        {
            Vector2 centroid = Vector2.zero;
            for (int i = 0; i < cells.Count; i++)
                centroid += (Vector2)cells[i];
            centroid /= Mathf.Max(cells.Count, 1);

            Vector2Int bestCell = cells[0];
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < cells.Count; i++)
            {
                float distance = Vector2.SqrMagnitude((Vector2)cells[i] - centroid);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCell = cells[i];
                }
            }

            return bestCell;
        }

        private HashSet<Vector2Int> BuildSparseNodeSet(bool[,] activeMask)
        {
            HashSet<Vector2Int> bestRegion = new();
            for (int parity = 0; parity < 2; parity++)
            {
                HashSet<Vector2Int> candidates = new();
                for (int x = 0; x < activeMask.GetLength(0); x++)
                {
                    for (int y = 0; y < activeMask.GetLength(1); y++)
                    {
                        if (!activeMask[x, y] || ((x + y) & 1) != parity)
                            continue;

                        Vector2Int cell = new(x, y);
                        if (HasSparseConnection(cell, activeMask))
                            candidates.Add(cell);
                    }
                }

                HashSet<Vector2Int> region = KeepLargestSparseRegion(candidates, activeMask);
                if (region.Count > bestRegion.Count)
                    bestRegion = region;
            }

            return bestRegion;
        }

        private List<Vector2Int> GetAvailableDirections(Vector2Int cell, bool[,] mask, HashSet<Vector2Int> visited)
        {
            List<Vector2Int> directions = new();
            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int next = cell + Directions[i];
                if (!IsInside(next, mask.GetLength(0), mask.GetLength(1)))
                    continue;
                if (!mask[next.x, next.y] || visited.Contains(next))
                    continue;

                directions.Add(Directions[i]);
            }

            return directions;
        }

        private List<Vector2Int> GetAvailableSparseDirections(Vector2Int cell, bool[,] mask, HashSet<Vector2Int> nodeSet, HashSet<Vector2Int> visited)
        {
            List<Vector2Int> directions = new();
            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int step = Directions[i];
                Vector2Int middle = cell + step;
                Vector2Int target = cell + step * SparseStepSize;
                if (!IsInside(middle, mask.GetLength(0), mask.GetLength(1)) ||
                    !IsInside(target, mask.GetLength(0), mask.GetLength(1)))
                    continue;
                if (!mask[middle.x, middle.y] || !mask[target.x, target.y])
                    continue;
                if (!nodeSet.Contains(target) || visited.Contains(target))
                    continue;

                directions.Add(step);
            }

            return directions;
        }

        private Vector2Int ChooseDirection(DfsFrame frame, List<Vector2Int> availableDirections, bool[,] mask)
        {
            if (frame.Direction != Vector2Int.zero && frame.RunLength < MinimumRunLength)
            {
                for (int i = 0; i < availableDirections.Count; i++)
                {
                    if (availableDirections[i] == frame.Direction)
                        return availableDirections[i];
                }
            }

            float totalWeight = 0f;
            float[] weights = new float[availableDirections.Count];
            for (int i = 0; i < availableDirections.Count; i++)
            {
                Vector2Int direction = availableDirections[i];
                Vector2Int next = frame.Cell + direction;
                float weight = direction == frame.Direction ? StraightWeight : 1f;
                weight += GetForwardRoomScore(next, direction, mask) * 0.45f;
                weights[i] = weight;
                totalWeight += weight;
            }

            float roll = Random.value * totalWeight;
            for (int i = 0; i < availableDirections.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0f)
                    return availableDirections[i];
            }

            return availableDirections[availableDirections.Count - 1];
        }

        private Vector2Int ChooseSparseDirection(DfsFrame frame, List<Vector2Int> availableDirections, bool[,] mask)
        {
            if (frame.Direction != Vector2Int.zero && frame.RunLength < MinimumRunLength)
            {
                for (int i = 0; i < availableDirections.Count; i++)
                {
                    if (availableDirections[i] == frame.Direction)
                        return availableDirections[i];
                }
            }

            float totalWeight = 0f;
            float[] weights = new float[availableDirections.Count];
            for (int i = 0; i < availableDirections.Count; i++)
            {
                Vector2Int direction = availableDirections[i];
                Vector2Int next = frame.Cell + direction * SparseStepSize;
                float weight = direction == frame.Direction ? StraightWeight : 1f;
                weight += GetForwardRoomScoreSparse(next, direction, mask) * 0.6f;
                weights[i] = weight;
                totalWeight += weight;
            }

            float roll = Random.value * totalWeight;
            for (int i = 0; i < availableDirections.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0f)
                    return availableDirections[i];
            }

            return availableDirections[availableDirections.Count - 1];
        }

        private float GetForwardRoomScore(Vector2Int start, Vector2Int direction, bool[,] mask)
        {
            float score = 0f;
            Vector2Int current = start;
            for (int i = 0; i < 4; i++)
            {
                if (!IsInside(current, mask.GetLength(0), mask.GetLength(1)) || !mask[current.x, current.y])
                    break;

                score += 1f;
                current += direction;
            }

            return score;
        }

        private float GetForwardRoomScoreSparse(Vector2Int start, Vector2Int direction, bool[,] mask)
        {
            float score = 0f;
            Vector2Int current = start;
            for (int i = 0; i < 4; i++)
            {
                Vector2Int middle = current - direction;
                if (!IsInside(current, mask.GetLength(0), mask.GetLength(1)) ||
                    !IsInside(middle, mask.GetLength(0), mask.GetLength(1)) ||
                    !mask[current.x, current.y] ||
                    !mask[middle.x, middle.y])
                    break;

                score += 1f;
                current += direction * SparseStepSize;
            }

            return score;
        }

        private int PickTargetSegmentEdgeCount()
        {
            int minEdges = Mathf.Clamp(minSegmentEdges, 1, Mathf.Max(1, maxSegmentEdges));
            int maxEdges = Mathf.Clamp(maxSegmentEdges, minEdges, Mathf.Max(minEdges, maxLineLength));
            if (minEdges == maxEdges)
                return minEdges;

            int shortMax = Mathf.Clamp(minEdges + 1, minEdges, maxEdges);
            int mediumMin = Mathf.Clamp(minEdges + 2, minEdges, maxEdges);
            int mediumMax = Mathf.Clamp(minEdges + 4, mediumMin, maxEdges);
            int longMin = Mathf.Clamp(maxEdges - 2, minEdges, maxEdges);

            float roll = Random.value;
            if (roll < 0.28f)
                return Random.Range(minEdges, shortMax + 1);
            if (roll < 0.72f)
                return Random.Range(mediumMin, mediumMax + 1);
            return Random.Range(longMin, maxEdges + 1);
        }

        private bool ShouldStopAtNode(TreeData tree, Vector2Int cell, Vector2Int root, int traversedEdges, int targetEdges)
        {
            if (cell == root)
                return true;

            if (traversedEdges >= targetEdges)
                return true;

            int degree = GetDegree(tree, cell, root);
            if (degree <= 2)
                return false;

            if (traversedEdges < minSegmentEdges)
                return false;

            if (traversedEdges >= targetEdges - 1)
                return true;

            return Random.value > branchPassThroughChance;
        }

        private bool TrySelectSpacedCandidates(List<PathCandidate> candidates, bool[,] activeMask, out List<PathCandidate> selected)
        {
            selected = null;
            int totalPlayableCells = GetActiveCells(activeMask).Count;
            if (totalPlayableCells <= 0)
                return false;

            int targetOccupiedCells = Mathf.RoundToInt(totalPlayableCells * targetArrowDensity);
            int minimumOccupiedCells = Mathf.RoundToInt(totalPlayableCells * Mathf.Max(0.1f, targetArrowDensity - densityTolerance));
            int maximumOccupiedCells = Mathf.RoundToInt(totalPlayableCells * Mathf.Min(0.95f, targetArrowDensity + densityTolerance));

            List<PathCandidate> best = new();
            int bestOccupiedCount = -1;

            for (int attempt = 0; attempt < SubsetBuildAttempts; attempt++)
            {
                List<PathCandidate> picked = BuildSubsetAttempt(
                    candidates,
                    activeMask,
                    enforceSpacing: true,
                    maximumOccupiedCells,
                    minimumOccupiedCells,
                    targetOccupiedCells);

                int occupiedCount = CountOccupiedCells(picked);
                if (IsBetterSubset(picked, occupiedCount, best, bestOccupiedCount))
                {
                    best = picked;
                    bestOccupiedCount = occupiedCount;
                }

                if (occupiedCount >= targetOccupiedCells && picked.Count >= 6)
                    break;
            }

            if (bestOccupiedCount < minimumOccupiedCells || best.Count < 4)
                return false;

            selected = best;
            return true;
        }

        private List<PathCandidate> BuildBestEffortSubset(List<PathCandidate> candidates, bool[,] activeMask, bool enforceSpacing)
        {
            if (candidates == null || candidates.Count == 0)
                return new List<PathCandidate>();

            List<PathCandidate> best = new();
            int bestOccupiedCount = -1;
            int maxOccupiedCells = activeMask.GetLength(0) * activeMask.GetLength(1);
            int stagnantAttempts = 0;

            for (int attempt = 0; attempt < SubsetBuildAttempts; attempt++)
            {
                List<PathCandidate> picked = BuildSubsetAttempt(
                    candidates,
                    activeMask,
                    enforceSpacing,
                    maxOccupiedCells,
                    0,
                    maxOccupiedCells);

                int occupiedCount = CountOccupiedCells(picked);
                if (IsBetterSubset(picked, occupiedCount, best, bestOccupiedCount))
                {
                    best = picked;
                    bestOccupiedCount = occupiedCount;
                    stagnantAttempts = 0;
                    if (bestOccupiedCount >= maxOccupiedCells - 2)
                        break;
                }
                else
                {
                    stagnantAttempts++;
                    if (best.Count > 0 && stagnantAttempts >= AttemptPlateauLimit)
                        break;
                }
            }

            return best;
        }

        private List<PathCandidate> BuildSubsetAttempt(
            List<PathCandidate> candidates,
            bool[,] activeMask,
            bool enforceSpacing,
            int maximumOccupiedCells,
            int minimumOccupiedCells,
            int targetOccupiedCells)
        {
            HashSet<int> remaining = new();
            for (int i = 0; i < candidates.Count; i++)
                remaining.Add(i);

            int localWidth = activeMask.GetLength(0);
            int localHeight = activeMask.GetLength(1);
            bool[,] occupied = enforceSpacing ? new bool[activeMask.GetLength(0), activeMask.GetLength(1)] : null;
            bool[,] blockerOccupied = new bool[localWidth, localHeight];
            List<PathCandidate> picked = new();
            int occupiedCount = 0;

            while (remaining.Count > 0)
            {
                List<CandidatePlacement> options = BuildPlacementOptions(
                    candidates,
                    remaining,
                    blockerOccupied,
                    occupied,
                    activeMask,
                    maximumOccupiedCells - occupiedCount);

                if (options.Count == 0)
                    break;

                bool shouldForceCoverage = occupiedCount < minimumOccupiedCells;
                CandidatePlacement chosen = ChoosePlacementOption(options, shouldForceCoverage);
                PathCandidate candidate = chosen.Candidate;
                picked.Add(candidate);
                remaining.Remove(chosen.SourceIndex);
                occupiedCount += candidate.OccupiedCells.Count;

                MarkCandidateOccupied(candidate, blockerOccupied);
                if (occupied != null)
                    MarkCandidateOccupied(candidate, occupied);

                if (occupiedCount >= targetOccupiedCells)
                    break;
            }

            return picked;
        }

        private int CountOccupiedCells(List<PathCandidate> candidates)
        {
            int total = 0;
            if (candidates == null)
                return total;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] != null)
                    total += candidates[i].OccupiedCells.Count;
            }

            return total;
        }

        private bool IsBetterSubset(List<PathCandidate> candidateSubset, int occupiedCount, List<PathCandidate> bestSubset, int bestOccupiedCount)
        {
            if (candidateSubset == null || candidateSubset.Count == 0)
                return false;

            if (bestSubset == null || bestSubset.Count == 0)
                return true;

            if (occupiedCount != bestOccupiedCount)
                return occupiedCount > bestOccupiedCount;

            return candidateSubset.Count > bestSubset.Count;
        }

        private List<CandidatePlacement> BuildPlacementOptions(
            List<PathCandidate> candidates,
            HashSet<int> remaining,
            bool[,] blockerOccupied,
            bool[,] occupied,
            bool[,] activeMask,
            int remainingCapacity)
        {
            List<CandidatePlacement> options = new();
            List<int> orderedIndices = remaining.ToList();
            for (int i = 0; i < orderedIndices.Count; i++)
            {
                int swapIndex = Random.Range(i, orderedIndices.Count);
                (orderedIndices[i], orderedIndices[swapIndex]) = (orderedIndices[swapIndex], orderedIndices[i]);
            }

            for (int i = 0; i < orderedIndices.Count; i++)
            {
                int sourceIndex = orderedIndices[i];
                PathCandidate candidate = candidates[sourceIndex];
                if (candidate == null || candidate.OccupiedCells.Count == 0)
                    continue;

                if (candidate.OccupiedCells.Count > remainingCapacity)
                    continue;

                AddPlacementOption(candidate, sourceIndex, blockerOccupied, occupied, activeMask, options);
            }

            return options;
        }

        private void AddPlacementOption(
            PathCandidate candidate,
            int sourceIndex,
            bool[,] blockerOccupied,
            bool[,] occupied,
            bool[,] activeMask,
            List<CandidatePlacement> options)
        {
            if (candidate == null)
                return;

            if (occupied != null && !CanPlaceCandidateWithSpacing(candidate, occupied, activeMask))
                return;

            if (CanCandidateEscape(candidate, blockerOccupied, activeMask.GetLength(0), activeMask.GetLength(1)))
                options.Add(new CandidatePlacement(sourceIndex, candidate, GetPlacementScore(candidate)));

            PathCandidate reversed = candidate.CreateReversed();
            if (reversed != null && CanCandidateEscape(reversed, blockerOccupied, activeMask.GetLength(0), activeMask.GetLength(1)))
                options.Add(new CandidatePlacement(sourceIndex, reversed, GetPlacementScore(reversed) - 0.05f));
        }

        private float GetPlacementScore(PathCandidate candidate)
        {
            return candidate.OccupiedCells.Count * (1f + longerSegmentPriority) +
                   candidate.Depth * 0.15f +
                   Random.value * 0.75f;
        }

        private CandidatePlacement ChoosePlacementOption(List<CandidatePlacement> options, bool prioritizeCoverage)
        {
            CandidatePlacement best = options[0];
            float bestScore = best.Score;

            for (int i = 1; i < options.Count; i++)
            {
                if (options[i].Score > bestScore)
                {
                    best = options[i];
                    bestScore = options[i].Score;
                }
            }

            float scoreWindow = prioritizeCoverage ? 0.2f : 0.75f;
            List<CandidatePlacement> finalists = new();
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Score >= bestScore - scoreWindow)
                    finalists.Add(options[i]);
            }

            return finalists[Random.Range(0, finalists.Count)];
        }

        private List<PathCandidate> OrderCandidatesForSpacing(List<PathCandidate> candidates)
        {
            List<PathCandidate> ordered = new(candidates);
            for (int i = 0; i < ordered.Count; i++)
            {
                int swapIndex = Random.Range(i, ordered.Count);
                (ordered[i], ordered[swapIndex]) = (ordered[swapIndex], ordered[i]);
            }

            return ordered;
        }

        private bool CanPlaceCandidateWithSpacing(PathCandidate candidate, bool[,] occupied, bool[,] activeMask)
        {
            for (int i = 0; i < candidate.OccupiedCells.Count; i++)
            {
                Vector2Int cell = candidate.OccupiedCells[i];
                if (!IsInside(cell, occupied.GetLength(0), occupied.GetLength(1)) || occupied[cell.x, cell.y])
                    return false;

                for (int offsetX = -minArrowGapCells; offsetX <= minArrowGapCells; offsetX++)
                {
                    for (int offsetY = -minArrowGapCells; offsetY <= minArrowGapCells; offsetY++)
                    {
                        if (offsetX == 0 && offsetY == 0)
                            continue;

                        int sampleX = cell.x + offsetX;
                        int sampleY = cell.y + offsetY;
                        if (!IsInside(new Vector2Int(sampleX, sampleY), occupied.GetLength(0), occupied.GetLength(1)))
                            continue;

                        if (occupied[sampleX, sampleY])
                            return false;
                    }
                }
            }

            return true;
        }

        private void MarkCandidateOccupied(PathCandidate candidate, bool[,] occupied)
        {
            for (int i = 0; i < candidate.OccupiedCells.Count; i++)
            {
                Vector2Int cell = candidate.OccupiedCells[i];
                if (IsInside(cell, occupied.GetLength(0), occupied.GetLength(1)))
                    occupied[cell.x, cell.y] = true;
            }
        }

        private bool HasSparseConnection(Vector2Int cell, bool[,] activeMask)
        {
            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int middle = cell + Directions[i];
                Vector2Int target = cell + Directions[i] * SparseStepSize;
                if (!IsInside(middle, activeMask.GetLength(0), activeMask.GetLength(1)) ||
                    !IsInside(target, activeMask.GetLength(0), activeMask.GetLength(1)))
                    continue;
                if (activeMask[middle.x, middle.y] && activeMask[target.x, target.y])
                    return true;
            }

            return false;
        }

        private HashSet<Vector2Int> KeepLargestSparseRegion(HashSet<Vector2Int> nodes, bool[,] activeMask)
        {
            HashSet<Vector2Int> bestRegion = new();
            HashSet<Vector2Int> visited = new();

            foreach (Vector2Int seed in nodes)
            {
                if (visited.Contains(seed))
                    continue;

                HashSet<Vector2Int> region = new();
                Queue<Vector2Int> queue = new();
                queue.Enqueue(seed);
                visited.Add(seed);

                while (queue.Count > 0)
                {
                    Vector2Int cell = queue.Dequeue();
                    region.Add(cell);

                    for (int i = 0; i < Directions.Length; i++)
                    {
                        Vector2Int middle = cell + Directions[i];
                        Vector2Int next = cell + Directions[i] * SparseStepSize;
                        if (!nodes.Contains(next) ||
                            visited.Contains(next) ||
                            !IsInside(middle, activeMask.GetLength(0), activeMask.GetLength(1)) ||
                            !activeMask[middle.x, middle.y])
                            continue;

                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }

                if (region.Count > bestRegion.Count)
                    bestRegion = region;
            }

            return bestRegion;
        }

        private int GetDegree(TreeData tree, Vector2Int cell, Vector2Int root)
        {
            int childCount = tree.Children.TryGetValue(cell, out List<Vector2Int> children) ? children.Count : 0;
            return cell == root ? childCount : childCount + 1;
        }

        private static int GetDepth(TreeData tree, Vector2Int cell)
        {
            return tree.Depth.TryGetValue(cell, out int depth) ? depth : 0;
        }

        private static bool IsInside(Vector2Int cell, int localWidth, int localHeight)
        {
            return cell.x >= 0 && cell.x < localWidth && cell.y >= 0 && cell.y < localHeight;
        }

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        private sealed class TreeData
        {
            public readonly Dictionary<Vector2Int, Vector2Int> Parent = new();
            public readonly Dictionary<Vector2Int, List<Vector2Int>> Children = new();
            public readonly Dictionary<Vector2Int, int> Depth = new();
        }

        private readonly struct DfsFrame
        {
            public readonly Vector2Int Cell;
            public readonly Vector2Int Direction;
            public readonly int RunLength;

            public DfsFrame(Vector2Int cell, Vector2Int direction, int runLength)
            {
                Cell = cell;
                Direction = direction;
                RunLength = runLength;
            }
        }

        private readonly struct EdgeKey
        {
            private readonly Vector2Int a;
            private readonly Vector2Int b;

            public EdgeKey(Vector2Int first, Vector2Int second)
            {
                if (first.x < second.x || (first.x == second.x && first.y <= second.y))
                {
                    a = first;
                    b = second;
                }
                else
                {
                    a = second;
                    b = first;
                }
            }
        }

        private readonly struct CandidatePlacement
        {
            public readonly int SourceIndex;
            public readonly PathCandidate Candidate;
            public readonly float Score;

            public CandidatePlacement(int sourceIndex, PathCandidate candidate, float score)
            {
                SourceIndex = sourceIndex;
                Candidate = candidate;
                Score = score;
            }
        }

        private sealed class PathCandidate
        {
            public readonly List<Vector2Int> Cells;
            public readonly List<Vector2Int> OccupiedCells;
            public readonly List<Vector2> WorldPoints;
            public readonly int Depth;
            private readonly HashSet<Vector2Int> occupiedLookup;

            public PathCandidate(List<Vector2Int> cells, List<Vector2> worldPoints, int depth)
            {
                Cells = cells;
                OccupiedCells = ExpandOccupiedCells(cells);
                Depth = depth;
                WorldPoints = worldPoints;
                occupiedLookup = new HashSet<Vector2Int>(OccupiedCells);
            }

            public PathCandidate CreateReversed()
            {
                if (Cells == null || WorldPoints == null || Cells.Count < 2 || WorldPoints.Count < 2)
                    return null;

                List<Vector2Int> reversedCells = new(Cells);
                reversedCells.Reverse();

                List<Vector2> reversedPoints = new(WorldPoints);
                reversedPoints.Reverse();

                return new PathCandidate(reversedCells, reversedPoints, Depth);
            }

            public Vector2Int GetHeadOutwardDirection()
            {
                if (Cells == null || Cells.Count < 2)
                    return Vector2Int.zero;

                Vector2Int head = Cells[0];
                Vector2Int neck = Cells[1];
                return new Vector2Int(
                    Mathf.Clamp(head.x - neck.x, -1, 1),
                    Mathf.Clamp(head.y - neck.y, -1, 1));
            }

            public bool ContainsOccupiedCell(Vector2Int cell)
            {
                return occupiedLookup.Contains(cell);
            }

            private static List<Vector2Int> ExpandOccupiedCells(List<Vector2Int> cells)
            {
                List<Vector2Int> occupied = new();
                if (cells == null || cells.Count == 0)
                    return occupied;

                occupied.Add(cells[0]);
                for (int i = 0; i < cells.Count - 1; i++)
                {
                    Vector2Int current = cells[i];
                    Vector2Int target = cells[i + 1];
                    Vector2Int step = new(
                        target.x == current.x ? 0 : (target.x > current.x ? 1 : -1),
                        target.y == current.y ? 0 : (target.y > current.y ? 1 : -1));

                    while (current != target)
                    {
                        current += step;
                        if (occupied.Count == 0 || occupied[occupied.Count - 1] != current)
                            occupied.Add(current);
                    }
                }

                return occupied;
            }
        }

        private sealed class DotVisual
        {
            public readonly Transform Transform;
            public readonly SpriteRenderer Renderer;
            public readonly Vector2 StartPosition;
            public readonly float BaseScale;

            public DotVisual(Transform transform, SpriteRenderer renderer, Vector2 startPosition, float baseScale)
            {
                Transform = transform;
                Renderer = renderer;
                StartPosition = startPosition;
                BaseScale = baseScale;
            }
        }
    }
}
