using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArrowGame
{
    public class LineController : MonoBehaviour
    {
        public List<Vector2> points = new();
        public float moveSpeed = 1f;
        public Transform arrow;
        public Color Color;
        public float board = 50f;

        public bool isClicked;

        private EdgeCollider2D edgeCollider2D;
        private BoxCollider2D head;
        private BoxCollider2D rear;
        private LineRenderer lineRenderer;
        private readonly List<LineRenderer> segmentRenderers = new();
        private LineRenderer guideRenderer;
        private Vector2 boardMin;
        private Vector2 boardMax;
        private bool guideVisible;
        private bool isBlockedAnimating;
        private Vector3 introStartLocalPosition;
        private bool hasIntroState;
        private bool hasPlayedEscapeSuccessSound;
        private float segmentEndpointInset = 0.14f;
        private float headEndpointInset = 0.28f;
        private float tailEndpointInset = 0.22f;
        private float arrowScaleMultiplier = 1.15f;
        private float baseArrowScale = 1f;
        private float cachedLineWidth = 0.2f;
        private Color currentVisualColor = Color.black;
        private float arrowHeadJoinOverlap = 0.08f;

        public void Init()
        {
            Color = Color.black;
            moveSpeed = 30f;

            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            cachedLineWidth = lineRenderer.startWidth;
            lineRenderer.enabled = false;
            if (arrow != null)
                baseArrowScale = Mathf.Max(arrow.localScale.x, 0.0001f);
            SetVisualColor(Color);
            UpdateArrowScale();

            edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>();
            edgeCollider2D.points = points.ToArray();

            head = gameObject.AddComponent<BoxCollider2D>();
            rear = gameObject.AddComponent<BoxCollider2D>();
            float capColliderSize = Mathf.Max(0.08f, cachedLineWidth * 0.8f);
            head.size = rear.size = Vector2.one * capColliderSize;
            head.offset = points[0];
            rear.offset = points[^1];

            ArrowGameManager manager = ArrowGameManager.Instance ?? FindFirstObjectByType<ArrowGameManager>();
            if (manager != null)
            {
                ArrowGameManager.Instance = manager;
                manager.AddLine(this);
            }
            else
            {
                Debug.LogWarning("LineController could not find ArrowGameManager during Init.");
            }
        }

        public void ConfigureVisualSpacing(float segmentInset, float headInset, float tailInset, float arrowScaleBoost)
        {
            segmentEndpointInset = Mathf.Max(0f, segmentInset);
            headEndpointInset = Mathf.Max(segmentEndpointInset, headInset);
            tailEndpointInset = Mathf.Max(segmentEndpointInset, tailInset);
            arrowScaleMultiplier = Mathf.Max(0.5f, arrowScaleBoost);
            if (arrow != null)
                baseArrowScale = Mathf.Max(arrow.localScale.x, 0.0001f);
            UpdateArrowScale();
        }

        public IEnumerator PlayIntroAnimation(float duration)
        {
            if (!hasIntroState || duration <= 0f)
            {
                transform.localPosition = Vector3.zero;
                yield break;
            }

            float elapsed = 0f;
            transform.localPosition = introStartLocalPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                transform.localPosition = Vector3.Lerp(introStartLocalPosition, Vector3.zero, easedT);
                yield return null;
            }

            transform.localPosition = Vector3.zero;
        }

        public void ConfigureBoardBounds(Vector2 min, Vector2 max)
        {
            boardMin = min;
            boardMax = max;
        }

        public void ConfigureIntroOffset(Vector2 offset)
        {
            introStartLocalPosition = new Vector3(offset.x, offset.y, 0f);
            hasIntroState = true;
            transform.localPosition = introStartLocalPosition;
        }

        public void ConfigureGuideLine(Material material, float width, Color color, int sortingOrder)
        {
            GameObject guideObject = new("GuideLine");
            guideObject.transform.SetParent(transform, false);

            guideRenderer = guideObject.AddComponent<LineRenderer>();
            guideRenderer.material = material;
            guideRenderer.useWorldSpace = true;
            guideRenderer.positionCount = 2;
            guideRenderer.startWidth = width;
            guideRenderer.endWidth = width;
            guideRenderer.widthMultiplier = 1f;
            guideRenderer.numCapVertices = 4;
            guideRenderer.startColor = color;
            guideRenderer.endColor = color;
            guideRenderer.sortingOrder = sortingOrder;
            guideRenderer.enabled = false;

            UpdateGuideLineGeometry();
        }

        public void SetGuideVisible(bool visible)
        {
            guideVisible = visible && !isClicked && guideRenderer != null;

            if (guideRenderer == null)
                return;

            if (guideVisible)
                UpdateGuideLineGeometry();

            guideRenderer.enabled = guideVisible;
        }

        public void UpdateLineRender()
        {
            edgeCollider2D.points = points.ToArray();
            head.offset = points[0];
            rear.offset = points[^1];
            RebuildSegmentRenderers();

            if (guideVisible)
                UpdateGuideLineGeometry();
        }

        public bool CanBeRemoved()
        {
            return !TryGetBlockingHit(out _);
        }

        public void ShowHint()
        {
            StartCoroutine(ShowHintCO());
        }

        public void CompleteForWin()
        {
            gameObject.SetActive(false);
        }

        private void CheckLineClick(Vector2 screenPosition)
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            float minDistance = CheckDistanceToLine(worldPosition);

            if (minDistance < 0.5f)
            {
                if (TryGetBlockingHit(out RaycastHit2D blockingHit))
                {
                    if (!isBlockedAnimating)
                    {
                        SoundManager.PlayWrongArrowClick();
                        StartCoroutine(OnBlockedHit(blockingHit));
                    }
                    return;
                }

                OnLineClicked();
            }
        }

        private void OnLineClicked()
        {
            isClicked = true;
            hasPlayedEscapeSuccessSound = false;
            SoundManager.PlayRightArrowClick();
            SetGuideVisible(false);
            gameObject.layer = 2;

            ArrowGameManager.Instance.LineRemoved(this);
        }

        private IEnumerator OnBlockedHit(RaycastHit2D blockingHit)
        {
            isBlockedAnimating = true;
            SetBlockedHighlight(Color.red);

            Vector2 originalHead = points[0];
            Vector2 direction = GetHeadDirection();
            Vector2 blockingPoint = blockingHit.point;
            Vector2 targetHead = blockingPoint - direction * 0.18f;
            float outwardDuration = 0.12f;
            float returnDuration = 0.18f;

            yield return AnimateHeadPosition(originalHead, targetHead, outwardDuration);
            yield return AnimateHeadPosition(targetHead, originalHead, returnDuration);

            points[0] = originalHead;
            SetArrowLocalPosition(originalHead);
            UpdateLineRender();

            SetVisualColor(Color);

            ArrowGameManager.Instance.OnCollide();
            isBlockedAnimating = false;
        }

        private IEnumerator ShowHintCO()
        {
            SetVisualColor(Color.yellow);
            yield return new WaitForSeconds(1.5f);
            SetVisualColor(Color);
        }

        private float CheckDistanceToLine(Vector2 point)
        {
            float minDistance = float.MaxValue;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 worldPointA = transform.TransformPoint(points[i]);
                Vector2 worldPointB = transform.TransformPoint(points[i + 1]);
                float distance = PointToLineDistance(point, worldPointA, worldPointB);
                minDistance = Mathf.Min(minDistance, distance);
            }

            return minDistance;
        }

        private float PointToLineDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 lineDir = (lineEnd - lineStart).normalized;
            Vector2 pointDir = point - lineStart;

            float dot = Vector2.Dot(pointDir, lineDir);
            dot = Mathf.Clamp(dot, 0f, Vector2.Distance(lineStart, lineEnd));

            Vector2 nearestPoint = lineStart + lineDir * dot;
            return Vector2.Distance(point, nearestPoint);
        }

        private Vector2 GetHeadDirection()
        {
            return (points[0] - points[1]).normalized;
        }

        private bool TryGetBlockingHit(out RaycastHit2D hit)
        {
            Vector2 dir = GetHeadDirection();
            hit = Physics2D.Raycast(points[0] + dir * .2f, dir);
            return hit.collider != null;
        }

        private void UpdateGuideLineGeometry()
        {
            if (guideRenderer == null || points.Count < 2)
                return;

            Vector2 headPosition = points[0];
            Vector2 direction = GetHeadDirection();
            Vector2 exitPoint = CalculateGuideEndPoint(headPosition, direction);

            Vector3 worldStart = transform.TransformPoint(new Vector3(headPosition.x, headPosition.y, 0f));
            Vector3 worldEnd = transform.TransformPoint(new Vector3(exitPoint.x, exitPoint.y, 0f));

            guideRenderer.SetPosition(0, worldStart);
            guideRenderer.SetPosition(1, worldEnd);
        }

        private Vector2 CalculateGuideEndPoint(Vector2 start, Vector2 direction)
        {
            float distanceToVerticalEdge = float.PositiveInfinity;
            float distanceToHorizontalEdge = float.PositiveInfinity;

            if (!Mathf.Approximately(direction.x, 0f))
            {
                float targetX = direction.x > 0f ? boardMax.x : boardMin.x;
                distanceToVerticalEdge = (targetX - start.x) / direction.x;
            }

            if (!Mathf.Approximately(direction.y, 0f))
            {
                float targetY = direction.y > 0f ? boardMax.y : boardMin.y;
                distanceToHorizontalEdge = (targetY - start.y) / direction.y;
            }

            float distance = Mathf.Min(distanceToVerticalEdge, distanceToHorizontalEdge);
            distance = Mathf.Max(distance, 0f);
            return start + direction * distance;
        }

        private void UpdateLineMovement()
        {
            float moveDist = moveSpeed * Time.deltaTime;
            Vector2 dir1 = points[0] - points[1];
            points[0] += dir1.normalized * moveDist;
            SetArrowLocalPosition(points[0]);

            while (points.Count >= 2)
            {
                int len = points.Count;
                Vector2 dir2 = (points[len - 2] - points[len - 1]).normalized;
                float dist = Vector2.Distance(points[len - 2], points[len - 1]);

                if (dist < moveDist && points.Count > 2)
                {
                    moveDist -= dist;
                    points.RemoveAt(len - 1);
                    continue;
                }

                points[^1] += dir2 * moveDist;
                break;
            }

            UpdateLineRender();
        }

        private IEnumerator AnimateHeadPosition(Vector2 from, Vector2 to, float duration)
        {
            if (duration <= 0f)
            {
                points[0] = to;
                SetArrowLocalPosition(to);
                UpdateLineRender();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                points[0] = Vector2.Lerp(from, to, t);
                SetArrowLocalPosition(points[0]);
                UpdateLineRender();
                yield return null;
            }

            points[0] = to;
            SetArrowLocalPosition(to);
            UpdateLineRender();
        }

        private void SetBlockedHighlight(Color color)
        {
            SetVisualColor(color);
        }

        private void Update()
        {
            bool isInputLocked = ArrowGameManager.Instance != null && ArrowGameManager.Instance.IsInputLocked;

            if (!isInputLocked && Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                bool hasDraggedTouch = ArrowGameManager.Instance != null && ArrowGameManager.Instance.HasDraggedCurrentTouch;
                bool touchEndedWithoutDrag = touch.phase == TouchPhase.Ended && !hasDraggedTouch;
                bool touchOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);

                if (touchEndedWithoutDrag && !touchOverUI)
                    CheckLineClick(touch.position);
            }
            else if (!isInputLocked && Input.touchCount == 0 && Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                CheckLineClick(Input.mousePosition);
            }

            if (isClicked)
            {
                UpdateLineMovement();

                Vector2 lastPt = points[^1];
                if (lastPt.x < -board || lastPt.x > board || lastPt.y < -board || lastPt.y > board)
                {
                    if (!hasPlayedEscapeSuccessSound)
                    {
                        hasPlayedEscapeSuccessSound = true;
                        HapticManager.PlaySuccess();
                        SoundManager.PlayArrowEscapeSuccess();
                    }

                    gameObject.SetActive(false);
                }
            }
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void RebuildSegmentRenderers()
        {
            int segmentCount = Mathf.Max(0, points.Count - 1);
            while (segmentRenderers.Count < segmentCount)
                segmentRenderers.Add(CreateSegmentRenderer(segmentRenderers.Count));

            for (int i = 0; i < segmentRenderers.Count; i++)
                segmentRenderers[i].enabled = i < segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                LineRenderer segmentRenderer = segmentRenderers[i];
                Vector2 a = points[i];
                Vector2 b = points[i + 1];
                Vector2 direction = b - a;
                float length = direction.magnitude;
                if (length <= 0.0001f)
                {
                    segmentRenderer.enabled = false;
                    continue;
                }

                Vector2 unit = direction / length;
                float startInset = 0f;
                float endInset = i == segmentCount - 1 ? tailEndpointInset : 0f;
                float maxInset = Mathf.Max(0f, length * 0.5f - 0.01f);
                startInset = Mathf.Min(startInset, maxInset);
                endInset = Mathf.Min(endInset, maxInset);

                Vector2 renderStart = a + unit * startInset;
                Vector2 renderEnd = b - unit * endInset;
                if ((renderEnd - renderStart).sqrMagnitude <= 0.0004f)
                {
                    segmentRenderer.enabled = false;
                    continue;
                }

                segmentRenderer.enabled = true;
                segmentRenderer.positionCount = 2;
                segmentRenderer.SetPosition(0, renderStart);
                segmentRenderer.SetPosition(1, renderEnd);
                segmentRenderer.startColor = currentVisualColor;
                segmentRenderer.endColor = currentVisualColor;
            }
        }

        private LineRenderer CreateSegmentRenderer(int index)
        {
            GameObject segmentObject = new($"Segment_{index}");
            segmentObject.transform.SetParent(transform, false);

            LineRenderer segmentRenderer = segmentObject.AddComponent<LineRenderer>();
            segmentRenderer.material = lineRenderer.material;
            segmentRenderer.useWorldSpace = false;
            segmentRenderer.alignment = lineRenderer.alignment;
            segmentRenderer.textureMode = lineRenderer.textureMode;
            segmentRenderer.widthMultiplier = 1f;
            segmentRenderer.startWidth = cachedLineWidth;
            segmentRenderer.endWidth = cachedLineWidth;
            segmentRenderer.numCapVertices = 8;
            segmentRenderer.numCornerVertices = 0;
            segmentRenderer.sortingOrder = lineRenderer.sortingOrder;
            segmentRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            segmentRenderer.receiveShadows = false;
            segmentRenderer.enabled = false;
            return segmentRenderer;
        }

        private void SetVisualColor(Color color)
        {
            currentVisualColor = color;

            if (arrow != null)
            {
                SpriteRenderer arrowRenderer = arrow.GetComponent<SpriteRenderer>();
                if (arrowRenderer != null)
                    arrowRenderer.color = color;
            }

            for (int i = 0; i < segmentRenderers.Count; i++)
            {
                segmentRenderers[i].startColor = color;
                segmentRenderers[i].endColor = color;
            }
        }

        private void UpdateArrowScale()
        {
            if (arrow == null)
                return;

            arrow.localScale = Vector3.one * baseArrowScale * arrowScaleMultiplier;
            arrowHeadJoinOverlap = Mathf.Max(cachedLineWidth * 0.9f, arrow.localScale.x * 0.08f);
        }

        private void SetArrowLocalPosition(Vector2 position)
        {
            if (arrow != null)
                arrow.localPosition = position;
        }
    }
}
