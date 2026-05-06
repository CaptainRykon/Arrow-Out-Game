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
        private LineRenderer guideRenderer;
        private Vector2 boardMin;
        private Vector2 boardMax;
        private bool guideVisible;
        private bool isBlockedAnimating;
        private Vector3 introStartLocalPosition;
        private bool hasIntroState;
        private bool hasPlayedEscapeSuccessSound;

        public void Init()
        {
            Color = Color.black;
            moveSpeed = 30f;

            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.startColor = lineRenderer.endColor = Color;
            arrow.GetComponent<SpriteRenderer>().color = Color;

            edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>();
            edgeCollider2D.points = points.ToArray();

            head = gameObject.AddComponent<BoxCollider2D>();
            rear = gameObject.AddComponent<BoxCollider2D>();
            head.size = rear.size = Vector2.one * .1f;
            head.offset = points[0];
            rear.offset = points[^1];

            ArrowGameManager.Instance.AddLine(this);
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
            lineRenderer.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                lineRenderer.SetPosition(i, points[i]);

            edgeCollider2D.points = points.ToArray();
            head.offset = points[0];
            rear.offset = points[^1];

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
            arrow.position = originalHead;
            UpdateLineRender();

            lineRenderer.startColor = lineRenderer.endColor = Color;
            arrow.GetComponent<SpriteRenderer>().color = Color;

            ArrowGameManager.Instance.OnCollide();
            isBlockedAnimating = false;
        }

        private IEnumerator ShowHintCO()
        {
            lineRenderer.startColor = lineRenderer.endColor = Color.yellow;
            arrow.GetComponent<SpriteRenderer>().color = Color.yellow;
            yield return new WaitForSeconds(1.5f);
            lineRenderer.startColor = lineRenderer.endColor = Color;
            arrow.GetComponent<SpriteRenderer>().color = Color;
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
            arrow.position = points[0];

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
                arrow.position = to;
                UpdateLineRender();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                points[0] = Vector2.Lerp(from, to, t);
                arrow.position = points[0];
                UpdateLineRender();
                yield return null;
            }

            points[0] = to;
            arrow.position = to;
            UpdateLineRender();
        }

        private void SetBlockedHighlight(Color color)
        {
            lineRenderer.startColor = lineRenderer.endColor = color;
            arrow.GetComponent<SpriteRenderer>().color = color;
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
    }
}
