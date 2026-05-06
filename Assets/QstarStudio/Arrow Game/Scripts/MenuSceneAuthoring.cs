using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ArrowGame
{
    [ExecuteAlways]
    public class MenuSceneAuthoring : MonoBehaviour
    {
        private sealed class ChallengeMenuReferences
        {
            public TextMeshProUGUI titleText;
            public TextMeshProUGUI patternNameText;
            public TextMeshProUGUI cycleTimerText;
            public TextMeshProUGUI chanceText;
            public TextMeshProUGUI nextChanceTimerText;
            public TextMeshProUGUI statusText;
            public Button streakButton;
            public Button challengePlayButton;
            public GameObject streakPanel;
            public Button closeStreakButton;
            public TextMeshProUGUI streakHeadlineText;
            public TextMeshProUGUI streakSummaryText;
            public ChallengeStreakDayView[] streakDayViews;
        }

        private static Sprite runtimeSprite;

        private readonly Color cardColor = new(0.18f, 0.2f, 0.31f, 1f);
        private readonly Color accentColor = new(0.35f, 0.43f, 0.98f, 1f);
        private readonly Color textPrimaryColor = new(0.95f, 0.96f, 1f, 1f);
        private readonly Color textSecondaryColor = new(0.67f, 0.7f, 0.84f, 1f);

        private void OnEnable()
        {
            if (Application.isPlaying)
                return;

            EnsureChallengeUi();
        }

        [ContextMenu("Rebuild Menu Hierarchy")]
        public void RebuildMenuHierarchy()
        {
            EnsureChallengeUi(forceRebuild: true);
        }

        private void EnsureChallengeUi(bool forceRebuild = false)
        {
            MenuSceneController controller = GetComponent<MenuSceneController>();
            if (controller == null)
                return;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            EnsureEventSystem();

            Transform challengePanel = FindDeepChild(canvas.transform, "Challenge Panel") ?? FindDeepChild(canvas.transform, "Collection Panel");
            if (challengePanel == null)
                return;

            Transform challengeMenuPanel = challengePanel.Find("Challenge Menu Panel");
            if (challengeMenuPanel == null)
            {
                Transform existingPanel = FindDeepChild(canvas.transform, "Challenge Menu Panel");
                if (existingPanel != null)
                {
                    existingPanel.SetParent(challengePanel, false);
                    challengeMenuPanel = existingPanel;
                }
            }

            if (forceRebuild || NeedsChallengeMenuRebuild(challengeMenuPanel))
            {
                if (challengeMenuPanel != null)
                    DestroyEditorSafe(challengeMenuPanel.gameObject);

                BuildChallengeMenuPanel(challengePanel);
            }

            Transform streakPanel = FindDeepChild(canvas.transform, "Challenge Streak Panel");
            if (forceRebuild || streakPanel == null || FindDeepChild(streakPanel, "Day 7") == null)
            {
                if (streakPanel != null)
                    DestroyEditorSafe(streakPanel.gameObject);

                BuildStreakPanel(canvas.transform);
            }

            ChallengeMenuReferences refs = CollectChallengeReferences(challengePanel, canvas.transform);
            if (!HasRequiredChallengeReferences(refs))
                return;

            AssignChallengeReferences(controller, refs);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(controller);
            if (controller.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
#endif
        }

        private bool NeedsChallengeMenuRebuild(Transform challengeMenuPanel)
        {
            return challengeMenuPanel == null ||
                   FindDeepChild(challengeMenuPanel, "Challenge Title") == null ||
                   FindDeepChild(challengeMenuPanel, "Pattern Name") == null ||
                   FindDeepChild(challengeMenuPanel, "Cycle Timer") == null ||
                   FindDeepChild(challengeMenuPanel, "Challenge Text") == null ||
                   FindDeepChild(challengeMenuPanel, "Next Chance Timer") == null ||
                   FindDeepChild(challengeMenuPanel, "Status Text") == null ||
                   FindDeepChild(challengeMenuPanel, "Streak Button") == null ||
                   FindDeepChild(challengeMenuPanel, "Play Challenge Button") == null;
        }

        private void BuildChallengeMenuPanel(Transform challengePanel)
        {
            RectTransform panel = CreateRect("Challenge Menu Panel", challengePanel);
            StretchRect(panel);

            VerticalLayoutGroup sectionLayout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            sectionLayout.padding = new RectOffset(0, 0, 36, 36);
            sectionLayout.spacing = 24f;
            sectionLayout.childAlignment = TextAnchor.UpperCenter;
            sectionLayout.childControlWidth = true;
            sectionLayout.childControlHeight = false;
            sectionLayout.childForceExpandWidth = true;
            sectionLayout.childForceExpandHeight = false;

            RectTransform card = CreateRect("Challenge Card", panel);
            card.sizeDelta = new Vector2(0f, 620f);
            LayoutElement cardLayout = card.gameObject.AddComponent<LayoutElement>();
            cardLayout.flexibleWidth = 1f;

            Image cardImage = card.gameObject.AddComponent<Image>();
            cardImage.sprite = GetRuntimeSprite();
            cardImage.color = cardColor;
            cardImage.type = Image.Type.Sliced;

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 32, 32);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateNamedLabel(card, "Challenge Title", "Weekly Challenge #1", 48f, textPrimaryColor);
            CreateNamedLabel(card, "Pattern Name", "Pattern", 34f, accentColor);
            CreateNamedLabel(card, "Cycle Timer", "07d 00h 00m 00s", 34f, accentColor);
            CreateNamedLabel(card, "Challenge Text", "1 chance left", 28f, textPrimaryColor);
            CreateNamedLabel(card, "Next Chance Timer", "Chance Ready", 24f, textSecondaryColor);
            CreateNamedLabel(card, "Status Text", "You have 1 chance available today.", 24f, textSecondaryColor);
            CreateButton(card, "Streak", new Vector2(0f, 76f));
            CreateButton(card, "Play Challenge", new Vector2(0f, 88f));
        }

        private void BuildStreakPanel(Transform canvasRoot)
        {
            RectTransform overlay = CreateRect("Challenge Streak Panel", canvasRoot);
            StretchRect(overlay);
            Image overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.sprite = GetRuntimeSprite();
            overlayImage.color = new Color(0.04f, 0.05f, 0.08f, 0.72f);

            RectTransform card = CreateRect("Streak Popup Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(760f, 1040f);
            card.anchoredPosition = Vector2.zero;

            Image cardImage = card.gameObject.AddComponent<Image>();
            cardImage.sprite = GetRuntimeSprite();
            cardImage.color = Color.white;
            cardImage.type = Image.Type.Sliced;

            VerticalLayoutGroup cardLayout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(56, 56, 72, 72);
            cardLayout.spacing = 22f;
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = false;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;

            RectTransform flame = CreateRect("Flame", card);
            flame.sizeDelta = new Vector2(210f, 210f);
            Image flameImage = flame.gameObject.AddComponent<Image>();
            flameImage.sprite = GetRuntimeSprite();
            flameImage.color = new Color(1f, 0.63f, 0.12f, 1f);
            flameImage.type = Image.Type.Sliced;

            CreateNamedLabel(card, "Streak Headline", "0 day streak", 52f, new Color(0.2f, 0.24f, 0.42f, 1f));

            RectTransform dayRow = CreateRect("Streak Day Row", card);
            dayRow.sizeDelta = new Vector2(0f, 160f);
            HorizontalLayoutGroup dayRowLayout = dayRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            dayRowLayout.spacing = 10f;
            dayRowLayout.childAlignment = TextAnchor.MiddleCenter;
            dayRowLayout.childControlWidth = true;
            dayRowLayout.childControlHeight = true;
            dayRowLayout.childForceExpandWidth = true;
            dayRowLayout.childForceExpandHeight = false;

            for (int i = 0; i < 7; i++)
            {
                RectTransform dayItem = CreateRect($"Day {i + 1}", dayRow);
                LayoutElement dayLayout = dayItem.gameObject.AddComponent<LayoutElement>();
                dayLayout.preferredWidth = 90f;
                dayLayout.preferredHeight = 130f;

                VerticalLayoutGroup dayLayoutGroup = dayItem.gameObject.AddComponent<VerticalLayoutGroup>();
                dayLayoutGroup.spacing = 10f;
                dayLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                dayLayoutGroup.childControlWidth = true;
                dayLayoutGroup.childControlHeight = false;
                dayLayoutGroup.childForceExpandWidth = true;
                dayLayoutGroup.childForceExpandHeight = false;

                string dayShortName = i switch { 0 => "Mo", 1 => "Tu", 2 => "We", 3 => "Th", 4 => "Fr", 5 => "Sa", _ => "Su" };
                TextMeshProUGUI dayLabel = CreateNamedLabel(dayItem, "Day Label", dayShortName, 22f, textSecondaryColor);

                RectTransform dayBubble = CreateRect("Day Bubble", dayItem);
                dayBubble.sizeDelta = new Vector2(72f, 72f);
                Image dayBubbleImage = dayBubble.gameObject.AddComponent<Image>();
                dayBubbleImage.sprite = GetRuntimeSprite();
                dayBubbleImage.color = new Color(0.93f, 0.93f, 0.98f, 1f);
                dayBubbleImage.type = Image.Type.Sliced;

                TextMeshProUGUI stateLabel = CreateNamedLabel(dayBubble, "State Label", "Today", 16f, new Color(0.28f, 0.31f, 0.48f, 1f));
                GameObject playedMarker = CreateMarker(dayBubble, "Played Marker", "OK", 20f, Color.white);
                GameObject currentMarker = CreateMarker(dayBubble, "Current Marker", "Now", 16f, Color.white);
                playedMarker.SetActive(false);
                currentMarker.SetActive(false);

                ChallengeStreakDayView view = dayItem.gameObject.AddComponent<ChallengeStreakDayView>();
                AssignStreakDayView(view, dayBubbleImage, dayLabel, stateLabel, playedMarker, currentMarker);
            }

            CreateNamedLabel(card, "Streak Summary", "Win a level to start your streak!", 26f, new Color(0.28f, 0.31f, 0.48f, 1f));
            RectTransform spacer = CreateRect("Bottom Spacer", card);
            LayoutElement spacerLayout = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleHeight = 1f;
            spacerLayout.minHeight = 40f;
            CreateButton(card, "Close", new Vector2(0f, 82f));
        }

        private ChallengeMenuReferences CollectChallengeReferences(Transform challengePanel, Transform canvasRoot)
        {
            ChallengeMenuReferences refs = new();
            Transform menuPanel = challengePanel.Find("Challenge Menu Panel") ?? FindDeepChild(challengePanel, "Challenge Menu Panel");
            Transform card = menuPanel != null ? FindDeepChild(menuPanel, "Challenge Card") : null;
            Transform streakPanel = FindDeepChild(canvasRoot, "Challenge Streak Panel");
            Transform streakCard = streakPanel != null ? FindDeepChild(streakPanel, "Streak Popup Card") : null;
            Transform dayRow = streakCard != null ? FindDeepChild(streakCard, "Streak Day Row") : null;

            refs.titleText = GetText(card, "Challenge Title");
            refs.patternNameText = GetText(card, "Pattern Name");
            refs.cycleTimerText = GetText(card, "Cycle Timer");
            refs.chanceText = GetText(card, "Challenge Text");
            refs.nextChanceTimerText = GetText(card, "Next Chance Timer");
            refs.statusText = GetText(card, "Status Text");
            refs.streakButton = GetButton(card, "Streak Button");
            refs.challengePlayButton = GetButton(card, "Play Challenge Button");
            refs.streakPanel = streakPanel != null ? streakPanel.gameObject : null;
            refs.closeStreakButton = GetButton(streakCard, "Close Button");
            refs.streakHeadlineText = GetText(streakCard, "Streak Headline");
            refs.streakSummaryText = GetText(streakCard, "Streak Summary");
            refs.streakDayViews = new ChallengeStreakDayView[7];

            for (int i = 0; i < refs.streakDayViews.Length; i++)
            {
                Transform dayItem = dayRow != null ? FindDeepChild(dayRow, $"Day {i + 1}") : null;
                refs.streakDayViews[i] = dayItem != null ? dayItem.GetComponent<ChallengeStreakDayView>() : null;
            }

            return refs;
        }

        private static bool HasRequiredChallengeReferences(ChallengeMenuReferences refs)
        {
            if (refs == null || refs.titleText == null || refs.patternNameText == null || refs.cycleTimerText == null || refs.chanceText == null || refs.nextChanceTimerText == null || refs.statusText == null || refs.streakButton == null || refs.challengePlayButton == null || refs.streakPanel == null || refs.closeStreakButton == null || refs.streakHeadlineText == null || refs.streakSummaryText == null || refs.streakDayViews == null || refs.streakDayViews.Length < 7)
                return false;

            for (int i = 0; i < 7; i++)
            {
                if (refs.streakDayViews[i] == null)
                    return false;
            }

            return true;
        }

#if UNITY_EDITOR
        private static void AssignChallengeReferences(MenuSceneController controller, ChallengeMenuReferences refs)
        {
            UnityEditor.SerializedObject so = new(controller);
            Assign(so, "challengeTitleText", refs.titleText);
            Assign(so, "challengePatternText", refs.patternNameText);
            Assign(so, "challengeCycleTimerText", refs.cycleTimerText);
            Assign(so, "challengeChanceText", refs.chanceText);
            Assign(so, "challengeNextChanceTimerText", refs.nextChanceTimerText);
            Assign(so, "challengeStatusText", refs.statusText);
            Assign(so, "streakButton", refs.streakButton);
            Assign(so, "challengePlayButton", refs.challengePlayButton);
            Assign(so, "streakPanel", refs.streakPanel);
            Assign(so, "closeStreakButton", refs.closeStreakButton);
            Assign(so, "streakHeadlineText", refs.streakHeadlineText);
            Assign(so, "streakSummaryText", refs.streakSummaryText);
            AssignArray(so, "streakDayViews", refs.streakDayViews);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Assign(UnityEditor.SerializedObject so, string propertyName, Object value)
        {
            UnityEditor.SerializedProperty property = so.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
        }

        private static void AssignArray(UnityEditor.SerializedObject so, string propertyName, Object[] values)
        {
            UnityEditor.SerializedProperty property = so.FindProperty(propertyName);
            if (property == null || !property.isArray)
                return;

            property.arraySize = values != null ? values.Length : 0;
            for (int i = 0; i < property.arraySize; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
#endif

        private Button CreateButton(Transform parent, string label, Vector2 size)
        {
            RectTransform rect = CreateRect($"{label} Button", parent);
            rect.sizeDelta = size;
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = GetRuntimeSprite();
            image.color = accentColor;
            image.type = Image.Type.Sliced;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CreateNamedLabel(rect, "Label", label, 34f, textPrimaryColor);
            return button;
        }

        private static TextMeshProUGUI CreateNamedLabel(Transform parent, string objectName, string text, float fontSize, Color color)
        {
            RectTransform rect = CreateRect(objectName, parent);
            StretchRect(rect);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;
            ContentSizeFitter fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return label;
        }

        private static GameObject CreateMarker(Transform parent, string name, string text, float fontSize, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            StretchRect(rect);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;
            return rect.gameObject;
        }

        private static void AssignStreakDayView(ChallengeStreakDayView view, Image background, TextMeshProUGUI dayLabel, TextMeshProUGUI stateLabel, GameObject playedMarker, GameObject currentMarker)
        {
#if UNITY_EDITOR
            UnityEditor.SerializedObject so = new(view);
            Assign(so, "background", background);
            Assign(so, "dayLabel", dayLabel);
            Assign(so, "stateLabel", stateLabel);
            Assign(so, "playedMarker", playedMarker);
            Assign(so, "currentMarker", currentMarker);
            so.ApplyModifiedPropertiesWithoutUndo();
#endif
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI GetText(Transform parent, string name)
        {
            Transform child = FindDeepChild(parent, name);
            return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        }

        private static Button GetButton(Transform parent, string name)
        {
            Transform child = FindDeepChild(parent, name);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            if (parent == null)
                return null;

            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                Transform nested = FindDeepChild(child, childName);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static void DestroyEditorSafe(GameObject gameObject)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.DestroyObjectImmediate(gameObject);
                return;
            }
#endif
            DestroyImmediate(gameObject);
        }

        private static Sprite GetRuntimeSprite()
        {
            if (runtimeSprite != null)
                return runtimeSprite;

            Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp;
            runtimeSprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
            runtimeSprite.name = "MenuRuntimeSprite";
            return runtimeSprite;
        }
    }
}
