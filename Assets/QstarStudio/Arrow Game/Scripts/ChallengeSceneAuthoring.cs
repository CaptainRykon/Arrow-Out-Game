using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowGame
{
    [ExecuteAlways]
    public class ChallengeSceneAuthoring : MonoBehaviour
    {
        private static Sprite runtimeSprite;

        private readonly Color overlayColor = new(0.05f, 0.06f, 0.1f, 0.82f);
        private readonly Color panelColor = new(0.12f, 0.13f, 0.22f, 0.96f);
        private readonly Color panelSoftColor = new(0.17f, 0.18f, 0.29f, 0.94f);
        private readonly Color accentColor = new(0.35f, 0.43f, 0.98f, 1f);
        private readonly Color textPrimaryColor = new(0.95f, 0.96f, 1f, 1f);
        private readonly Color textSecondaryColor = new(0.67f, 0.7f, 0.84f, 1f);

        private void OnEnable()
        {
            if (Application.isPlaying)
                return;

            EnsureChallengeSetup();
        }

        private void EnsureChallengeSetup()
        {
            ArrowGameManager gameManager = FindFirstObjectByType<ArrowGameManager>();
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (gameManager == null || canvas == null)
                return;

            ChallengeSceneController controller = gameManager.GetComponent<ChallengeSceneController>();
            if (controller == null)
                controller = gameManager.gameObject.AddComponent<ChallengeSceneController>();

            ChallengePatternLibrary patternLibrary = gameManager.GetComponent<ChallengePatternLibrary>();
            if (patternLibrary == null)
                patternLibrary = gameManager.gameObject.AddComponent<ChallengePatternLibrary>();

            ChallengeUiRefs refs = BuildOrFindUi(canvas.transform, gameManager);
            AssignControllerReferences(controller, gameManager, refs);
        }

        private ChallengeUiRefs BuildOrFindUi(Transform canvasRoot, ArrowGameManager gameManager)
        {
            ChallengeUiRefs refs = new();

            refs.challengeMenuPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge Menu Panel", true).gameObject;
            refs.challengeHudPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge HUD Panel", false).gameObject;
            refs.countdownPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge Countdown Panel", false).gameObject;
            refs.streakPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge Streak Panel", false).gameObject;
            refs.leaderboardPanel = gameManager.winUI != null ? gameManager.winUI : FindOrCreateFullScreenPanel(canvasRoot, "Challenge Leaderboard Panel", false).gameObject;

            BuildChallengeMenu(refs.challengeMenuPanel.transform, refs);
            BuildCountdownPanel(refs.countdownPanel.transform, refs);
            BuildHudPanel(refs.challengeHudPanel.transform, refs);
            BuildStreakPanel(refs.streakPanel.transform, refs);
            BuildLeaderboardPanel(refs.leaderboardPanel.transform, refs);

            return refs;
        }

        private void BuildChallengeMenu(Transform panelRoot, ChallengeUiRefs refs)
        {
            RectTransform card = FindOrCreateCard(panelRoot, "Challenge Menu Card", new Vector2(820f, 1100f), panelColor);
            VerticalLayoutGroup layout = EnsureVerticalLayout(card.gameObject, new RectOffset(44, 44, 52, 52), 22f);

            CreateOrUpdateLabel(card, "Challenge Title", "Weekly Challenge #1", 52f, textPrimaryColor, out refs.challengeTitleText);
            CreateOrUpdateLabel(card, "Pattern Name", "Pattern", 34f, accentColor, out refs.challengePatternText);
            CreateOrUpdateLabel(card, "Cycle Timer", "07d 00h 00m 00s", 30f, textPrimaryColor, out refs.cycleTimerText);
            CreateOrUpdateLabel(card, "Chance Text", "1 chance left", 28f, textPrimaryColor, out refs.challengeChanceText);
            CreateOrUpdateLabel(card, "Next Chance Timer", "Chance Ready", 24f, textSecondaryColor, out refs.nextChanceTimerText);
            CreateOrUpdateLabel(card, "Status Text", "You have 1 chance available today.", 24f, textSecondaryColor, out refs.challengeStatusText);

            refs.streakButton = FindOrCreateButton(card, "Streak Button", "Streak", new Vector2(0f, 80f));
            refs.challengePlayButton = FindOrCreateButton(card, "Challenge Play Button", "Play Challenge", new Vector2(0f, 92f));
        }

        private void BuildCountdownPanel(Transform panelRoot, ChallengeUiRefs refs)
        {
            RectTransform center = FindOrCreateCard(panelRoot, "Countdown Card", new Vector2(360f, 360f), panelSoftColor);
            CreateOrUpdateLabel(center, "Countdown Text", "3", 160f, textPrimaryColor, out refs.countdownText);
        }

        private void BuildHudPanel(Transform panelRoot, ChallengeUiRefs refs)
        {
            RectTransform timerHolder = FindOrCreateCard(panelRoot, "Run Timer Holder", new Vector2(360f, 120f), panelColor);
            timerHolder.anchorMin = new Vector2(0.5f, 1f);
            timerHolder.anchorMax = new Vector2(0.5f, 1f);
            timerHolder.pivot = new Vector2(0.5f, 1f);
            timerHolder.anchoredPosition = new Vector2(0f, -40f);
            CreateOrUpdateLabel(timerHolder, "Run Timer", "00:00.000", 58f, textPrimaryColor, out refs.runTimerText);
        }

        private void BuildStreakPanel(Transform panelRoot, ChallengeUiRefs refs)
        {
            RectTransform card = FindOrCreateCard(panelRoot, "Streak Popup Card", new Vector2(760f, 1040f), Color.white);
            VerticalLayoutGroup layout = EnsureVerticalLayout(card.gameObject, new RectOffset(56, 56, 72, 72), 22f);

            RectTransform flame = FindOrCreateCard(card, "Flame", new Vector2(220f, 220f), new Color(1f, 0.63f, 0.12f, 1f));
            LayoutElement flameLayout = flame.gameObject.GetComponent<LayoutElement>() ?? flame.gameObject.AddComponent<LayoutElement>();
            flameLayout.preferredWidth = 220f;
            flameLayout.preferredHeight = 220f;

            CreateOrUpdateLabel(card, "Streak Headline", "0 day streak", 52f, new Color(0.2f, 0.24f, 0.42f, 1f), out refs.streakHeadlineText);

            RectTransform dayRow = FindOrCreateRect(card, "Streak Day Row");
            LayoutElement rowLayout = dayRow.gameObject.GetComponent<LayoutElement>() ?? dayRow.gameObject.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 150f;
            HorizontalLayoutGroup rowGroup = dayRow.gameObject.GetComponent<HorizontalLayoutGroup>() ?? dayRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowGroup.spacing = 10f;
            rowGroup.childAlignment = TextAnchor.MiddleCenter;
            rowGroup.childControlWidth = true;
            rowGroup.childControlHeight = true;
            rowGroup.childForceExpandWidth = true;
            rowGroup.childForceExpandHeight = false;

            refs.streakDayViews = new ChallengeStreakDayView[7];
            for (int i = 0; i < refs.streakDayViews.Length; i++)
            {
                refs.streakDayViews[i] = FindOrCreateStreakDayView(dayRow, i);
            }

            CreateOrUpdateLabel(card, "Streak Summary", "Win a level to start your streak!", 26f, new Color(0.28f, 0.31f, 0.48f, 1f), out refs.streakSummaryText);

            RectTransform spacer = FindOrCreateRect(card, "Bottom Spacer");
            LayoutElement spacerLayout = spacer.gameObject.GetComponent<LayoutElement>() ?? spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleHeight = 1f;
            spacerLayout.minHeight = 32f;

            refs.closeStreakButton = FindOrCreateButton(card, "Close Streak Button", "Close", new Vector2(0f, 84f));
        }

        private void BuildLeaderboardPanel(Transform panelRoot, ChallengeUiRefs refs)
        {
            RectTransform card = FindOrCreateCard(panelRoot, "Challenge Leaderboard Card", new Vector2(860f, 1200f), panelColor);
            VerticalLayoutGroup layout = EnsureVerticalLayout(card.gameObject, new RectOffset(44, 44, 44, 44), 18f);

            CreateOrUpdateLabel(card, "Leaderboard Title", "Cycle #1 Leaderboard", 46f, textPrimaryColor, out refs.leaderboardTitleText);
            CreateOrUpdateLabel(card, "Final Score", "00:00.000", 40f, accentColor, out refs.finalScoreText);
            CreateOrUpdateLabel(card, "Player Best", "Your Best: Not set yet", 28f, textSecondaryColor, out refs.leaderboardPlayerBestText);

            refs.submitScoreButton = FindOrCreateButton(card, "Submit Score Button", "Assign Your Score", new Vector2(0f, 80f));
            refs.leaderboardMainMenuButton = FindOrCreateButton(card, "Leaderboard Main Menu Button", "Main Menu", new Vector2(0f, 80f));

            RectTransform listRoot = FindOrCreateRect(card, "Leaderboard List");
            VerticalLayoutGroup listLayout = EnsureVerticalLayout(listRoot.gameObject, new RectOffset(0, 0, 0, 0), 12f);
            LayoutElement listRootLayout = listRoot.gameObject.GetComponent<LayoutElement>() ?? listRoot.gameObject.AddComponent<LayoutElement>();
            listRootLayout.flexibleHeight = 1f;

            refs.leaderboardEntryViews = new ChallengeLeaderboardEntryView[6];
            for (int i = 0; i < refs.leaderboardEntryViews.Length; i++)
            {
                refs.leaderboardEntryViews[i] = FindOrCreateLeaderboardEntry(listRoot, i);
            }
        }

        private ChallengeStreakDayView FindOrCreateStreakDayView(Transform parent, int index)
        {
            RectTransform root = FindOrCreateRect(parent, $"Day {index + 1}");
            LayoutElement rootLayout = root.gameObject.GetComponent<LayoutElement>() ?? root.gameObject.AddComponent<LayoutElement>();
            rootLayout.preferredWidth = 90f;
            rootLayout.preferredHeight = 130f;

            VerticalLayoutGroup layout = root.gameObject.GetComponent<VerticalLayoutGroup>() ?? root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            string dayShortName = index switch
            {
                0 => "Mo",
                1 => "Tu",
                2 => "We",
                3 => "Th",
                4 => "Fr",
                5 => "Sa",
                _ => "Su"
            };

            CreateOrUpdateLabel(root, "Day Label", dayShortName, 22f, textSecondaryColor, out TextMeshProUGUI dayLabel);

            RectTransform bubble = FindOrCreateCard(root, "Day Bubble", new Vector2(72f, 72f), new Color(0.93f, 0.93f, 0.98f, 1f));
            CreateOrUpdateLabel(bubble, "State Label", "Today", 16f, new Color(0.28f, 0.31f, 0.48f, 1f), out TextMeshProUGUI stateLabel);
            GameObject playedMarker = FindOrCreateMarker(bubble, "Played Marker", "OK", 20f, Color.white);
            GameObject currentMarker = FindOrCreateMarker(bubble, "Current Marker", "Now", 16f, Color.white);
            playedMarker.SetActive(false);
            currentMarker.SetActive(false);

            ChallengeStreakDayView view = root.GetComponent<ChallengeStreakDayView>();
            if (view == null)
                view = root.gameObject.AddComponent<ChallengeStreakDayView>();

            AssignStreakDayView(view, bubble.GetComponent<Image>(), dayLabel, stateLabel, playedMarker, currentMarker);
            return view;
        }

        private ChallengeLeaderboardEntryView FindOrCreateLeaderboardEntry(Transform parent, int index)
        {
            RectTransform root = FindOrCreateCard(parent, $"Leaderboard Entry {index + 1}", new Vector2(0f, 120f), panelSoftColor);
            LayoutElement rootLayout = root.gameObject.GetComponent<LayoutElement>() ?? root.gameObject.AddComponent<LayoutElement>();
            rootLayout.preferredHeight = 120f;

            HorizontalLayoutGroup layout = root.gameObject.GetComponent<HorizontalLayoutGroup>() ?? root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 18, 18);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            CreateOrUpdateLabel(root, "Rank Text", $"{index + 1}", 34f, textPrimaryColor, out TextMeshProUGUI rankText, new Vector2(90f, 84f));
            CreateOrUpdateLabel(root, "Name Text", $"Player {index + 1}", 30f, textPrimaryColor, out TextMeshProUGUI nameText, new Vector2(320f, 84f));
            CreateOrUpdateLabel(root, "Time Text", "00:00.000", 30f, textPrimaryColor, out TextMeshProUGUI timeText, new Vector2(220f, 84f));

            GameObject firstBadge = FindOrCreateMarker(root, "First Place Badge", "1", 28f, Color.white);
            GameObject secondBadge = FindOrCreateMarker(root, "Second Place Badge", "2", 28f, Color.white);
            GameObject thirdBadge = FindOrCreateMarker(root, "Third Place Badge", "3", 28f, Color.white);
            firstBadge.SetActive(false);
            secondBadge.SetActive(false);
            thirdBadge.SetActive(false);

            ChallengeLeaderboardEntryView view = root.GetComponent<ChallengeLeaderboardEntryView>();
            if (view == null)
                view = root.gameObject.AddComponent<ChallengeLeaderboardEntryView>();

            AssignLeaderboardEntryView(view, root.gameObject, root.GetComponent<Image>(), rankText, nameText, timeText, firstBadge, secondBadge, thirdBadge);
            return view;
        }

        private static void AssignControllerReferences(ChallengeSceneController controller, ArrowGameManager gameManager, ChallengeUiRefs refs)
        {
#if UNITY_EDITOR
            UnityEditor.SerializedObject serializedObject = new(controller);
            Assign(serializedObject, "arrowGameManager", gameManager);
            Assign(serializedObject, "challengeMenuPanel", refs.challengeMenuPanel);
            Assign(serializedObject, "streakPanel", refs.streakPanel);
            Assign(serializedObject, "countdownPanel", refs.countdownPanel);
            Assign(serializedObject, "challengeHudPanel", refs.challengeHudPanel);
            Assign(serializedObject, "leaderboardPanel", refs.leaderboardPanel);
            Assign(serializedObject, "challengePlayButton", refs.challengePlayButton);
            Assign(serializedObject, "openStreakButton", refs.streakButton);
            Assign(serializedObject, "closeStreakButton", refs.closeStreakButton);
            Assign(serializedObject, "challengeTitleText", refs.challengeTitleText);
            Assign(serializedObject, "challengePatternText", refs.challengePatternText);
            Assign(serializedObject, "cycleTimerText", refs.cycleTimerText);
            Assign(serializedObject, "nextChanceTimerText", refs.nextChanceTimerText);
            Assign(serializedObject, "challengeStatusText", refs.challengeStatusText);
            Assign(serializedObject, "streakSummaryText", refs.streakSummaryText);
            Assign(serializedObject, "countdownText", refs.countdownText);
            Assign(serializedObject, "runTimerText", refs.runTimerText);
            Assign(serializedObject, "streakHeadlineText", refs.streakHeadlineText);
            Assign(serializedObject, "leaderboardTitleText", refs.leaderboardTitleText);
            Assign(serializedObject, "leaderboardPlayerBestText", refs.leaderboardPlayerBestText);
            Assign(serializedObject, "finalScoreText", refs.finalScoreText);
            Assign(serializedObject, "submitScoreButton", refs.submitScoreButton);
            Assign(serializedObject, "leaderboardMainMenuButton", refs.leaderboardMainMenuButton);
            AssignArray(serializedObject, "streakDayViews", refs.streakDayViews);
            AssignArray(serializedObject, "leaderboardEntryViews", refs.leaderboardEntryViews);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
#endif
        }

#if UNITY_EDITOR
        private static void Assign(UnityEditor.SerializedObject serializedObject, string propertyName, Object value)
        {
            UnityEditor.SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
        }

        private static void AssignArray(UnityEditor.SerializedObject serializedObject, string propertyName, Object[] values)
        {
            UnityEditor.SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
                return;

            property.arraySize = values != null ? values.Length : 0;
            for (int i = 0; i < property.arraySize; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
#endif

#if UNITY_EDITOR
        private static void AssignStreakDayView(
            ChallengeStreakDayView view,
            Image background,
            TextMeshProUGUI dayLabel,
            TextMeshProUGUI stateLabel,
            GameObject playedMarker,
            GameObject currentMarker)
        {
            UnityEditor.SerializedObject serializedObject = new(view);
            Assign(serializedObject, "background", background);
            Assign(serializedObject, "dayLabel", dayLabel);
            Assign(serializedObject, "stateLabel", stateLabel);
            Assign(serializedObject, "playedMarker", playedMarker);
            Assign(serializedObject, "currentMarker", currentMarker);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignLeaderboardEntryView(
            ChallengeLeaderboardEntryView view,
            GameObject contentRoot,
            Image background,
            TextMeshProUGUI rankText,
            TextMeshProUGUI nameText,
            TextMeshProUGUI timeText,
            GameObject firstBadge,
            GameObject secondBadge,
            GameObject thirdBadge)
        {
            UnityEditor.SerializedObject serializedObject = new(view);
            Assign(serializedObject, "contentRoot", contentRoot);
            Assign(serializedObject, "background", background);
            Assign(serializedObject, "rankText", rankText);
            Assign(serializedObject, "nameText", nameText);
            Assign(serializedObject, "timeText", timeText);
            Assign(serializedObject, "firstPlaceBadge", firstBadge);
            Assign(serializedObject, "secondPlaceBadge", secondBadge);
            Assign(serializedObject, "thirdPlaceBadge", thirdBadge);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
#endif

        
        private static void RemoveGeneratedChallengeMenuPanel(Transform canvasRoot)
        {
            RectTransform menuPanel = FindChildRect(canvasRoot, "Challenge Menu Panel");
            if (menuPanel == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.DestroyObjectImmediate(menuPanel.gameObject);
                return;
            }
#endif

            DestroyImmediate(menuPanel.gameObject);
        }
        private RectTransform FindOrCreateFullScreenPanel(Transform parent, string name, bool active)
        {
            RectTransform rect = FindChildRect(parent, name);
            if (rect == null)
                rect = CreateRect(name, parent);

            StretchRect(rect);
            Image image = rect.GetComponent<Image>();
            if (image == null)
                image = rect.gameObject.AddComponent<Image>();
            image.sprite = GetRuntimeSprite();
            image.color = overlayColor;
            rect.gameObject.SetActive(active);
            return rect;
        }

        private RectTransform FindOrCreateCard(Transform parent, string name, Vector2 size, Color color)
        {
            RectTransform rect = FindChildRect(parent, name);
            if (rect == null)
                rect = CreateRect(name, parent);

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Image image = rect.GetComponent<Image>();
            if (image == null)
                image = rect.gameObject.AddComponent<Image>();
            image.sprite = GetRuntimeSprite();
            image.color = color;
            image.type = Image.Type.Sliced;
            return rect;
        }

        private static RectTransform FindOrCreateRect(Transform parent, string name)
        {
            RectTransform rect = FindChildRect(parent, name);
            return rect ?? CreateRect(name, parent);
        }

        private static RectTransform FindChildRect(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            return child != null ? child as RectTransform : null;
        }

        private Button FindOrCreateButton(Transform parent, string name, string label, Vector2 size)
        {
            RectTransform rect = FindOrCreateRect(parent, name);
            LayoutElement layout = rect.gameObject.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = size.y;

            rect.sizeDelta = size;
            Image image = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.sprite = GetRuntimeSprite();
            image.color = accentColor;
            image.type = Image.Type.Sliced;

            Button button = rect.GetComponent<Button>() ?? rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            CreateOrUpdateLabel(rect, "Label", label, 30f, textPrimaryColor, out _);
            return button;
        }

        private static VerticalLayoutGroup EnsureVerticalLayout(GameObject gameObject, RectOffset padding, float spacing)
        {
            VerticalLayoutGroup layout = gameObject.GetComponent<VerticalLayoutGroup>() ?? gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        private void CreateOrUpdateLabel(Transform parent, string name, string text, float fontSize, Color color, out TextMeshProUGUI label, Vector2? preferredSize = null)
        {
            RectTransform rect = FindChildRect(parent, name);
            if (rect == null)
                rect = CreateRect(name, parent);

            if (preferredSize.HasValue)
            {
                LayoutElement layout = rect.gameObject.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
                layout.preferredWidth = preferredSize.Value.x;
                layout.preferredHeight = preferredSize.Value.y;
            }

            StretchRect(rect);
            label = rect.GetComponent<TextMeshProUGUI>();
            if (label == null)
                label = rect.gameObject.AddComponent<TextMeshProUGUI>();

            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;

            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;

            ContentSizeFitter fitter = rect.gameObject.GetComponent<ContentSizeFitter>() ?? rect.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static GameObject FindOrCreateMarker(Transform parent, string name, string text, float fontSize, Color color)
        {
            RectTransform rect = FindChildRect(parent, name);
            if (rect == null)
                rect = CreateRect(name, parent);

            StretchRect(rect);
            TextMeshProUGUI label = rect.GetComponent<TextMeshProUGUI>();
            if (label == null)
                label = rect.gameObject.AddComponent<TextMeshProUGUI>();

            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;

            return rect.gameObject;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static void StretchRect(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
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
            runtimeSprite.name = "ChallengeRuntimeSprite";
            return runtimeSprite;
        }

        private sealed class ChallengeUiRefs
        {
            public GameObject challengeMenuPanel;
            public GameObject streakPanel;
            public GameObject countdownPanel;
            public GameObject challengeHudPanel;
            public GameObject leaderboardPanel;
            public Button challengePlayButton;
            public Button streakButton;
            public Button closeStreakButton;
            public TextMeshProUGUI challengeTitleText;
            public TextMeshProUGUI challengePatternText;
            public TextMeshProUGUI cycleTimerText;
            public TextMeshProUGUI challengeChanceText;
            public TextMeshProUGUI nextChanceTimerText;
            public TextMeshProUGUI challengeStatusText;
            public TextMeshProUGUI countdownText;
            public TextMeshProUGUI runTimerText;
            public TextMeshProUGUI streakHeadlineText;
            public TextMeshProUGUI streakSummaryText;
            public TextMeshProUGUI leaderboardTitleText;
            public TextMeshProUGUI leaderboardPlayerBestText;
            public TextMeshProUGUI finalScoreText;
            public Button submitScoreButton;
            public Button leaderboardMainMenuButton;
            public ChallengeStreakDayView[] streakDayViews;
            public ChallengeLeaderboardEntryView[] leaderboardEntryViews;
        }
    }
}

