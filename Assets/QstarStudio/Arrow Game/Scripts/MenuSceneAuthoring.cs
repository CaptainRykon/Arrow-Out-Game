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
            public Button challengePlayButton;
            public Button streakButton;
            public GameObject streakPanel;
            public Button closeStreakButton;
            public TextMeshProUGUI cycleTimerText;
            public TextMeshProUGUI chanceText;
            public TextMeshProUGUI nextChanceTimerText;
            public TextMeshProUGUI streakHeadlineText;
            public TextMeshProUGUI streakSummaryText;
            public ChallengeStreakDayView[] streakDayViews;
        }

        private static Sprite runtimeSprite;

        private readonly Color backgroundColor = new(0.12f, 0.13f, 0.22f, 1f);
        private readonly Color panelColor = new(0.18f, 0.2f, 0.31f, 1f);
        private readonly Color panelSecondaryColor = new(0.16f, 0.17f, 0.27f, 1f);
        private readonly Color accentColor = new(0.35f, 0.43f, 0.98f, 1f);
        private readonly Color accentSoftColor = new(0.35f, 0.43f, 0.98f, 0.18f);
        private readonly Color textPrimaryColor = new(0.95f, 0.96f, 1f, 1f);
        private readonly Color textSecondaryColor = new(0.67f, 0.7f, 0.84f, 1f);

        [SerializeField] private bool menuGenerated;

        private void OnEnable()
        {
            if (Application.isPlaying)
                return;

            EnsureChallengeUi();
        }

        [ContextMenu("Rebuild Menu Hierarchy")]
        public void RebuildMenuHierarchy()
        {
            menuGenerated = false;
            EnsureHierarchy(true);
        }

        private void EnsureChallengeUi()
        {
            MenuSceneController controller = GetComponent<MenuSceneController>();
            if (controller == null)
                return;

            Canvas canvas = EnsureCanvas();
            Transform challengePanel = FindDeepChild(canvas.transform, "Challenge Panel");
            if (challengePanel == null)
                challengePanel = FindDeepChild(canvas.transform, "Collection Panel");

            if (challengePanel == null)
                return;

            Transform challengeMenuPanel = FindDeepChild(challengePanel, "Challenge Menu Panel");
            if (challengeMenuPanel == null)
                BuildChallengeSection(challengePanel, canvas.transform, out _);

            ChallengeMenuReferences challengeRefs = CollectChallengeReferences(challengePanel, canvas.transform);
            if (!HasRequiredChallengeReferences(challengeRefs))
                return;

            AssignChallengeReferences(controller, challengeRefs);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
            if (gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        private void EnsureHierarchy(bool force = false)
        {
            if (!force && menuGenerated && transform.childCount > 0)
                return;

            ClearChildren();
            BuildMenuHierarchy();
            menuGenerated = true;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
            if (gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        private void ClearChildren()
        {
            while (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(child.gameObject);
#else
                DestroyImmediate(child.gameObject);
#endif
            }
        }

        private void BuildMenuHierarchy()
        {
            MenuSceneController controller = GetComponent<MenuSceneController>();
            if (controller == null)
                controller = gameObject.AddComponent<MenuSceneController>();

            Canvas canvas = EnsureCanvas();
            EnsureEventSystem();
            ConfigureCamera();

            RectTransform background = CreateRect("Background", canvas.transform);
            StretchRect(background);
            Image backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.sprite = GetRuntimeSprite();
            backgroundImage.color = backgroundColor;

            RectTransform contentRoot = CreateRect("Content Root", canvas.transform);
            StretchRect(contentRoot);
            contentRoot.offsetMin = new Vector2(36f, 196f);
            contentRoot.offsetMax = new Vector2(-36f, -48f);

            RectTransform navRoot = CreateRect("Bottom Navigation", canvas.transform);
            navRoot.anchorMin = new Vector2(0f, 0f);
            navRoot.anchorMax = new Vector2(1f, 0f);
            navRoot.pivot = new Vector2(0.5f, 0f);
            navRoot.offsetMin = new Vector2(0f, 0f);
            navRoot.offsetMax = new Vector2(0f, 156f);
            Image navBackground = navRoot.gameObject.AddComponent<Image>();
            navBackground.sprite = GetRuntimeSprite();
            navBackground.color = panelSecondaryColor;

            HorizontalLayoutGroup navLayout = navRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            navLayout.padding = new RectOffset(30, 30, 18, 24);
            navLayout.spacing = 20f;
            navLayout.childAlignment = TextAnchor.MiddleCenter;
            navLayout.childControlWidth = true;
            navLayout.childControlHeight = true;
            navLayout.childForceExpandWidth = true;
            navLayout.childForceExpandHeight = true;

            BuildHomePanel(contentRoot, out GameObject homePanel, out Button cardPlayButton, out Button primaryPlayButton, out TextMeshProUGUI levelLabel);
            BuildChallengePanel(contentRoot, canvas.transform, out GameObject challengePanel, out ChallengeMenuReferences challengeRefs);
            BuildPlaceholderPanel(contentRoot, "Settings", "Settings will be added here.", out GameObject settingsPanel);

            BuildBottomTab(navRoot, "Home", true, out Button homeButton, out Image homeBackground, out TextMeshProUGUI homeLabel);
            BuildBottomTab(navRoot, "Collection", false, out Button collectionButton, out Image collectionBackground, out TextMeshProUGUI collectionLabel);
            BuildBottomTab(navRoot, "Settings", false, out Button settingsButton, out Image settingsBackground, out TextMeshProUGUI settingsLabel);

            AssignControllerReferences(
                controller,
                homePanel,
                challengePanel,
                settingsPanel,
                homeButton,
                collectionButton,
                settingsButton,
                homeBackground,
                collectionBackground,
                settingsBackground,
                homeLabel,
                collectionLabel,
                settingsLabel,
                primaryPlayButton,
                cardPlayButton,
                levelLabel,
                challengeRefs);
        }

        private void BuildHomePanel(Transform parent, out GameObject panelObject, out Button cardPlayButton, out Button primaryPlayButton, out TextMeshProUGUI levelLabel)
        {
            RectTransform panel = CreateRect("Home Panel", parent);
            StretchRect(panel);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 20, 20);
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            RectTransform badge = CreateRect("Top Badge", panel);
            badge.sizeDelta = new Vector2(122f, 58f);
            Image badgeImage = badge.gameObject.AddComponent<Image>();
            badgeImage.sprite = GetRuntimeSprite();
            badgeImage.color = panelSecondaryColor;
            badgeImage.type = Image.Type.Sliced;
            CreateLabel(badge, "1", 28f, textPrimaryColor, TextAlignmentOptions.Center);

            RectTransform featureCard = CreateRect("Feature Card", panel);
            featureCard.sizeDelta = new Vector2(360f, 420f);
            Image featureCardImage = featureCard.gameObject.AddComponent<Image>();
            featureCardImage.sprite = GetRuntimeSprite();
            featureCardImage.color = panelColor;
            featureCardImage.type = Image.Type.Sliced;

            VerticalLayoutGroup featureLayout = featureCard.gameObject.AddComponent<VerticalLayoutGroup>();
            featureLayout.padding = new RectOffset(28, 28, 26, 26);
            featureLayout.spacing = 18f;
            featureLayout.childAlignment = TextAnchor.UpperCenter;
            featureLayout.childControlWidth = true;
            featureLayout.childControlHeight = false;
            featureLayout.childForceExpandWidth = true;
            featureLayout.childForceExpandHeight = false;

            CreateLabel(featureCard, "Leagues", 40f, textPrimaryColor, TextAlignmentOptions.Center);

            RectTransform badgeHolder = CreateRect("Shield Placeholder", featureCard);
            badgeHolder.sizeDelta = new Vector2(160f, 160f);
            Image badgeHolderImage = badgeHolder.gameObject.AddComponent<Image>();
            badgeHolderImage.sprite = GetRuntimeSprite();
            badgeHolderImage.color = new Color(0.83f, 0.59f, 0.49f, 1f);
            badgeHolderImage.type = Image.Type.Sliced;
            CreateLabel(badgeHolder, "Badge", 26f, textPrimaryColor, TextAlignmentOptions.Center);

            cardPlayButton = CreateButton(featureCard, "Play", new Vector2(220f, 62f));

            RectTransform titleBlock = CreateRect("Title Block", panel);
            titleBlock.sizeDelta = new Vector2(0f, 120f);
            VerticalLayoutGroup titleLayout = titleBlock.gameObject.AddComponent<VerticalLayoutGroup>();
            titleLayout.spacing = 10f;
            titleLayout.childAlignment = TextAnchor.MiddleCenter;
            titleLayout.childControlWidth = true;
            titleLayout.childControlHeight = false;
            titleLayout.childForceExpandWidth = true;
            titleLayout.childForceExpandHeight = false;

            CreateLabel(titleBlock, "Arrows", 70f, textPrimaryColor, TextAlignmentOptions.Center);
            levelLabel = CreateLabel(titleBlock, "Level 1", 44f, accentColor, TextAlignmentOptions.Center);

            RectTransform spacer = CreateRect("Spacer", panel);
            LayoutElement spacerLayout = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleHeight = 1f;
            spacerLayout.minHeight = 120f;

            primaryPlayButton = CreateButton(panel, "Play", new Vector2(0f, 112f));
            LayoutElement playLayout = primaryPlayButton.gameObject.AddComponent<LayoutElement>();
            playLayout.flexibleWidth = 1f;

            panelObject = panel.gameObject;
        }

        private void BuildChallengePanel(Transform parent, Transform canvasRoot, out GameObject panelObject, out ChallengeMenuReferences challengeRefs)
        {
            RectTransform panel = CreateRect("Challenge Panel", parent);
            StretchRect(panel);
            BuildChallengeSection(panel, canvasRoot, out challengeRefs);
            panelObject = panel.gameObject;
        }

        private void BuildChallengeSection(Transform parent, Transform canvasRoot, out ChallengeMenuReferences challengeRefs)
        {
            challengeRefs = new ChallengeMenuReferences();

            RectTransform challengeMenuPanel = CreateRect("Challenge Menu Panel", parent);
            StretchRect(challengeMenuPanel);

            VerticalLayoutGroup sectionLayout = challengeMenuPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            sectionLayout.padding = new RectOffset(0, 0, 36, 36);
            sectionLayout.spacing = 24f;
            sectionLayout.childAlignment = TextAnchor.UpperCenter;
            sectionLayout.childControlWidth = true;
            sectionLayout.childControlHeight = false;
            sectionLayout.childForceExpandWidth = true;
            sectionLayout.childForceExpandHeight = false;

            RectTransform challengeCard = CreateRect("Challenge Card", challengeMenuPanel);
            challengeCard.sizeDelta = new Vector2(0f, 520f);
            LayoutElement cardLayout = challengeCard.gameObject.AddComponent<LayoutElement>();
            cardLayout.flexibleWidth = 1f;

            Image cardImage = challengeCard.gameObject.AddComponent<Image>();
            cardImage.sprite = GetRuntimeSprite();
            cardImage.color = panelColor;
            cardImage.type = Image.Type.Sliced;

            VerticalLayoutGroup layout = challengeCard.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 28, 28);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateLabel(challengeCard, "Challenge", 48f, textPrimaryColor, TextAlignmentOptions.Center);
            challengeRefs.cycleTimerText = CreateLabel(challengeCard, "07d 00h 00m 00s", 34f, accentColor, TextAlignmentOptions.Center);
            challengeRefs.chanceText = CreateLabel(challengeCard, "1 chance left", 28f, textPrimaryColor, TextAlignmentOptions.Center);
            challengeRefs.nextChanceTimerText = CreateLabel(challengeCard, "Chance Ready", 24f, textSecondaryColor, TextAlignmentOptions.Center);

            challengeRefs.streakButton = CreateButton(challengeCard, "Streak", new Vector2(0f, 76f));
            LayoutElement streakButtonLayout = challengeRefs.streakButton.gameObject.AddComponent<LayoutElement>();
            streakButtonLayout.flexibleWidth = 1f;

            challengeRefs.challengePlayButton = CreateButton(challengeCard, "Play Challenge", new Vector2(0f, 88f));
            LayoutElement challengePlayLayout = challengeRefs.challengePlayButton.gameObject.AddComponent<LayoutElement>();
            challengePlayLayout.flexibleWidth = 1f;

            BuildStreakPanel(canvasRoot, challengeRefs);
        }

        private ChallengeMenuReferences CollectChallengeReferences(Transform challengePanel, Transform canvasRoot)
        {
            ChallengeMenuReferences refs = new();

            Transform challengeMenuPanel = FindDeepChild(challengePanel, "Challenge Menu Panel");
            Transform challengeCard = challengeMenuPanel != null
                ? FindDeepChild(challengeMenuPanel, "Challenge Card")
                : FindDeepChild(challengePanel, "Challenge Card");
            Transform streakPanel = FindDeepChild(canvasRoot, "Challenge Streak Panel");
            Transform streakPopupCard = streakPanel != null ? FindDeepChild(streakPanel, "Streak Popup Card") : null;
            Transform streakDayRow = streakPopupCard != null ? FindDeepChild(streakPopupCard, "Streak Day Row") : null;

            refs.challengePlayButton = FindComponentInChildren<Button>(challengeCard, "Play Challenge Button");
            refs.streakButton = FindComponentInChildren<Button>(challengeCard, "Streak Button");
            refs.cycleTimerText = FindComponentInChildren<TextMeshProUGUI>(challengeCard, "07d 00h 00m 00s Label");
            refs.chanceText = FindComponentInChildren<TextMeshProUGUI>(challengeCard, "1 chance left Label");
            refs.nextChanceTimerText = FindComponentInChildren<TextMeshProUGUI>(challengeCard, "Chance Ready Label");
            refs.streakPanel = streakPanel != null ? streakPanel.gameObject : null;
            refs.closeStreakButton = FindComponentInChildren<Button>(streakPopupCard, "Close Button");
            refs.streakHeadlineText = FindComponentInChildren<TextMeshProUGUI>(streakPopupCard, "0 day streak Label");
            refs.streakSummaryText = FindComponentInChildren<TextMeshProUGUI>(streakPopupCard, "Win a level to start your streak! Label");

            if (streakDayRow != null)
            {
                refs.streakDayViews = new ChallengeStreakDayView[7];
                for (int i = 0; i < refs.streakDayViews.Length; i++)
                {
                    Transform dayItem = FindDeepChild(streakDayRow, $"Day {i + 1}");
                    refs.streakDayViews[i] = dayItem != null ? dayItem.GetComponent<ChallengeStreakDayView>() : null;
                }
            }

            return refs;
        }

        private static bool HasRequiredChallengeReferences(ChallengeMenuReferences challengeRefs)
        {
            if (challengeRefs == null ||
                challengeRefs.challengePlayButton == null ||
                challengeRefs.streakButton == null ||
                challengeRefs.streakPanel == null ||
                challengeRefs.closeStreakButton == null ||
                challengeRefs.cycleTimerText == null ||
                challengeRefs.chanceText == null ||
                challengeRefs.nextChanceTimerText == null ||
                challengeRefs.streakHeadlineText == null ||
                challengeRefs.streakSummaryText == null ||
                challengeRefs.streakDayViews == null ||
                challengeRefs.streakDayViews.Length < 7)
            {
                return false;
            }

            for (int i = 0; i < 7; i++)
            {
                if (challengeRefs.streakDayViews[i] == null)
                    return false;
            }

            return true;
        }

        private void BuildStreakPanel(Transform canvasRoot, ChallengeMenuReferences challengeRefs)
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

            challengeRefs.streakHeadlineText = CreateLabel(card, "0 day streak", 52f, new Color(0.2f, 0.24f, 0.42f, 1f), TextAlignmentOptions.Center);

            RectTransform dayRow = CreateRect("Streak Day Row", card);
            dayRow.sizeDelta = new Vector2(0f, 160f);
            HorizontalLayoutGroup dayRowLayout = dayRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            dayRowLayout.spacing = 10f;
            dayRowLayout.childAlignment = TextAnchor.MiddleCenter;
            dayRowLayout.childControlWidth = true;
            dayRowLayout.childControlHeight = true;
            dayRowLayout.childForceExpandWidth = true;
            dayRowLayout.childForceExpandHeight = false;

            challengeRefs.streakDayViews = new ChallengeStreakDayView[7];
            for (int i = 0; i < challengeRefs.streakDayViews.Length; i++)
            {
                RectTransform dayItem = CreateRect($"Day {i + 1}", dayRow);
                LayoutElement dayLayout = dayItem.gameObject.AddComponent<LayoutElement>();
                dayLayout.preferredWidth = 90f;
                dayLayout.preferredHeight = 130f;

                VerticalLayoutGroup dayItemLayout = dayItem.gameObject.AddComponent<VerticalLayoutGroup>();
                dayItemLayout.spacing = 10f;
                dayItemLayout.childAlignment = TextAnchor.MiddleCenter;
                dayItemLayout.childControlWidth = true;
                dayItemLayout.childControlHeight = false;
                dayItemLayout.childForceExpandWidth = true;
                dayItemLayout.childForceExpandHeight = false;

                string dayShortName = i switch
                {
                    0 => "Mo",
                    1 => "Tu",
                    2 => "We",
                    3 => "Th",
                    4 => "Fr",
                    5 => "Sa",
                    _ => "Su"
                };

                TextMeshProUGUI dayLabel = CreateLabel(dayItem, dayShortName, 22f, textSecondaryColor, TextAlignmentOptions.Center);

                RectTransform dayBubble = CreateRect("Day Bubble", dayItem);
                dayBubble.sizeDelta = new Vector2(72f, 72f);
                Image dayBubbleImage = dayBubble.gameObject.AddComponent<Image>();
                dayBubbleImage.sprite = GetRuntimeSprite();
                dayBubbleImage.color = new Color(0.93f, 0.93f, 0.98f, 1f);
                dayBubbleImage.type = Image.Type.Sliced;

                TextMeshProUGUI stateLabel = CreateLabel(dayBubble, "Today", 16f, new Color(0.28f, 0.31f, 0.48f, 1f), TextAlignmentOptions.Center);
                GameObject playedMarker = CreateMarker(dayBubble, "Played Marker", "OK", 20f, Color.white);
                playedMarker.SetActive(false);
                GameObject currentMarker = CreateMarker(dayBubble, "Current Marker", "Now", 16f, Color.white);
                currentMarker.SetActive(false);

                ChallengeStreakDayView streakDayView = dayItem.gameObject.AddComponent<ChallengeStreakDayView>();
                AssignStreakDayViewReferences(streakDayView, dayBubbleImage, dayLabel, stateLabel, playedMarker, currentMarker);
                challengeRefs.streakDayViews[i] = streakDayView;
            }

            challengeRefs.streakSummaryText = CreateLabel(card, "Win a level to start your streak!", 26f, new Color(0.28f, 0.31f, 0.48f, 1f), TextAlignmentOptions.Center);

            RectTransform spacer = CreateRect("Bottom Spacer", card);
            LayoutElement spacerLayout = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleHeight = 1f;
            spacerLayout.minHeight = 40f;

            challengeRefs.closeStreakButton = CreateButton(card, "Close", new Vector2(0f, 82f));
            LayoutElement closeButtonLayout = challengeRefs.closeStreakButton.gameObject.AddComponent<LayoutElement>();
            closeButtonLayout.flexibleWidth = 1f;

            challengeRefs.streakPanel = overlay.gameObject;
        }

        private void BuildPlaceholderPanel(Transform parent, string title, string description, out GameObject panelObject)
        {
            RectTransform panel = CreateRect($"{title} Panel", parent);
            StretchRect(panel);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 120, 120);
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            RectTransform card = CreateRect($"{title} Card", panel);
            card.sizeDelta = new Vector2(0f, 420f);
            LayoutElement cardLayout = card.gameObject.AddComponent<LayoutElement>();
            cardLayout.flexibleWidth = 1f;
            Image cardImage = card.gameObject.AddComponent<Image>();
            cardImage.sprite = GetRuntimeSprite();
            cardImage.color = panelColor;
            cardImage.type = Image.Type.Sliced;

            VerticalLayoutGroup cardInnerLayout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            cardInnerLayout.padding = new RectOffset(36, 36, 40, 40);
            cardInnerLayout.spacing = 20f;
            cardInnerLayout.childAlignment = TextAnchor.MiddleCenter;
            cardInnerLayout.childControlWidth = true;
            cardInnerLayout.childControlHeight = false;
            cardInnerLayout.childForceExpandWidth = true;
            cardInnerLayout.childForceExpandHeight = false;

            CreateLabel(card, title, 52f, textPrimaryColor, TextAlignmentOptions.Center);
            CreateLabel(card, description, 30f, textSecondaryColor, TextAlignmentOptions.Center);

            panelObject = panel.gameObject;
        }

        private void BuildBottomTab(Transform parent, string label, bool isSelected, out Button button, out Image background, out TextMeshProUGUI textLabel)
        {
            RectTransform tabRoot = CreateRect($"{label} Tab", parent);
            LayoutElement layout = tabRoot.gameObject.AddComponent<LayoutElement>();
            layout.flexibleWidth = 1f;
            layout.minHeight = 88f;

            background = tabRoot.gameObject.AddComponent<Image>();
            background.sprite = GetRuntimeSprite();
            background.color = isSelected ? accentSoftColor : Color.clear;
            background.type = Image.Type.Sliced;

            button = tabRoot.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            textLabel = CreateLabel(tabRoot, label, 24f, isSelected ? textPrimaryColor : textSecondaryColor, TextAlignmentOptions.Center);
        }

        private void AssignControllerReferences(
            MenuSceneController controller,
            GameObject homePanel,
            GameObject collectionPanel,
            GameObject settingsPanel,
            Button homeButton,
            Button collectionButton,
            Button settingsButton,
            Image homeBackground,
            Image collectionBackground,
            Image settingsBackground,
            TextMeshProUGUI homeLabel,
            TextMeshProUGUI collectionLabel,
            TextMeshProUGUI settingsLabel,
            Button primaryPlayButton,
            Button cardPlayButton,
            TextMeshProUGUI levelLabel,
            ChallengeMenuReferences challengeRefs)
        {
#if UNITY_EDITOR
            UnityEditor.SerializedObject serializedObject = new(controller);
            Assign(serializedObject, "homePanel", homePanel);
            Assign(serializedObject, "collectionPanel", collectionPanel);
            Assign(serializedObject, "settingsPanel", settingsPanel);
            Assign(serializedObject, "homeTabButton", homeButton);
            Assign(serializedObject, "collectionTabButton", collectionButton);
            Assign(serializedObject, "settingsTabButton", settingsButton);
            Assign(serializedObject, "homeTabBackground", homeBackground);
            Assign(serializedObject, "collectionTabBackground", collectionBackground);
            Assign(serializedObject, "settingsTabBackground", settingsBackground);
            Assign(serializedObject, "homeTabLabel", homeLabel);
            Assign(serializedObject, "collectionTabLabel", collectionLabel);
            Assign(serializedObject, "settingsTabLabel", settingsLabel);
            Assign(serializedObject, "primaryPlayButton", primaryPlayButton);
            Assign(serializedObject, "cardPlayButton", cardPlayButton);
            Assign(serializedObject, "currentLevelLabel", levelLabel);
            AssignChallengeReferences(serializedObject, challengeRefs);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
#endif
        }

        private static void AssignChallengeReferences(MenuSceneController controller, ChallengeMenuReferences challengeRefs)
        {
#if UNITY_EDITOR
            UnityEditor.SerializedObject serializedObject = new(controller);
            AssignChallengeReferences(serializedObject, challengeRefs);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
#endif
        }

#if UNITY_EDITOR
        private static void AssignChallengeReferences(UnityEditor.SerializedObject serializedObject, ChallengeMenuReferences challengeRefs)
        {
            Assign(serializedObject, "challengePlayButton", challengeRefs.challengePlayButton);
            Assign(serializedObject, "challengeCycleTimerText", challengeRefs.cycleTimerText);
            Assign(serializedObject, "challengeChanceText", challengeRefs.chanceText);
            Assign(serializedObject, "challengeNextChanceTimerText", challengeRefs.nextChanceTimerText);
            Assign(serializedObject, "streakButton", challengeRefs.streakButton);
            Assign(serializedObject, "streakPanel", challengeRefs.streakPanel);
            Assign(serializedObject, "closeStreakButton", challengeRefs.closeStreakButton);
            Assign(serializedObject, "streakHeadlineText", challengeRefs.streakHeadlineText);
            Assign(serializedObject, "streakSummaryText", challengeRefs.streakSummaryText);
            AssignArray(serializedObject, "streakDayViews", challengeRefs.streakDayViews);
        }
#endif

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

        private void ConfigureCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = backgroundColor;
        }

        private static Canvas EnsureCanvas()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
                return canvas;

            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        private Button CreateButton(Transform parent, string label, Vector2 sizeDelta)
        {
            RectTransform buttonRect = CreateRect($"{label} Button", parent);
            buttonRect.sizeDelta = sizeDelta;

            Image image = buttonRect.gameObject.AddComponent<Image>();
            image.sprite = GetRuntimeSprite();
            image.color = accentColor;
            image.type = Image.Type.Sliced;

            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            CreateLabel(buttonRect, label, 34f, textPrimaryColor, TextAlignmentOptions.Center);
            return button;
        }

        private static GameObject CreateMarker(Transform parent, string name, string text, float fontSize, Color color)
        {
            RectTransform markerRect = CreateRect(name, parent);
            StretchRect(markerRect);
            TextMeshProUGUI marker = markerRect.gameObject.AddComponent<TextMeshProUGUI>();
            marker.text = text;
            marker.fontSize = fontSize;
            marker.color = color;
            marker.alignment = TextAlignmentOptions.Center;
            marker.raycastTarget = false;

            if (TMP_Settings.defaultFontAsset != null)
                marker.font = TMP_Settings.defaultFontAsset;

            return marker.gameObject;
        }

        private static void AssignStreakDayViewReferences(
            ChallengeStreakDayView streakDayView,
            Image background,
            TextMeshProUGUI dayLabel,
            TextMeshProUGUI stateLabel,
            GameObject playedMarker,
            GameObject currentMarker)
        {
#if UNITY_EDITOR
            UnityEditor.SerializedObject serializedObject = new(streakDayView);
            Assign(serializedObject, "background", background);
            Assign(serializedObject, "dayLabel", dayLabel);
            Assign(serializedObject, "stateLabel", stateLabel);
            Assign(serializedObject, "playedMarker", playedMarker);
            Assign(serializedObject, "currentMarker", currentMarker);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
#endif
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            RectTransform labelRect = CreateRect($"{text} Label", parent);
            StretchRect(labelRect);

            TextMeshProUGUI label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;

            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;

            ContentSizeFitter fitter = labelRect.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return label;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
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

        private static T FindComponentInChildren<T>(Transform parent, string childName) where T : Component
        {
            Transform child = FindDeepChild(parent, childName);
            return child != null ? child.GetComponent<T>() : null;
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
            texture.SetPixels(new[]
            {
                Color.white, Color.white,
                Color.white, Color.white
            });
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp;

            runtimeSprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
            runtimeSprite.name = "MenuRuntimeSprite";
            return runtimeSprite;
        }
    }
}
