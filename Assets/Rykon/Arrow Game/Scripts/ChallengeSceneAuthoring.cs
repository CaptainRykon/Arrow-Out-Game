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

            RemoveGeneratedChallengeMenuPanel(canvasRoot);
            refs.loadingPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge Loading Panel", false).gameObject;
            refs.countdownPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge Countdown Panel", false).gameObject;
            refs.challengeHudPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge HUD Panel", false).gameObject;
            refs.leaderboardPanel = gameManager.winUI != null ? gameManager.winUI : FindOrCreateFullScreenPanel(canvasRoot, "Challenge Leaderboard Panel", false).gameObject;

            BuildLoadingPanel(refs.loadingPanel.transform, refs);
            BuildCountdownPanel(refs.countdownPanel.transform, refs);
            BuildHudPanel(refs.challengeHudPanel.transform, refs);
            BuildLeaderboardPanel(refs.leaderboardPanel.transform, refs);

            return refs;
        }

        private void BuildLoadingPanel(Transform panelRoot, ChallengeUiRefs refs)
        {
            RectTransform card = FindOrCreateCard(panelRoot, "Loading Card", new Vector2(720f, 260f), panelSoftColor);
            VerticalLayoutGroup layout = EnsureVerticalLayout(card.gameObject, new RectOffset(42, 42, 40, 40), 18f);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandHeight = false;

            CreateOrUpdateLabel(card, "Loading Title", "Preparing Challenge", 44f, textPrimaryColor, out _);
            CreateOrUpdateLabel(card, "Loading Status", "Building arrow puzzle...", 28f, textSecondaryColor, out refs.loadingStatusText, new Vector2(0f, 58f));

            RectTransform barTrack = FindOrCreateRect(card, "Loading Bar Track");
            LayoutElement barTrackLayout = barTrack.gameObject.GetComponent<LayoutElement>() ?? barTrack.gameObject.AddComponent<LayoutElement>();
            barTrackLayout.preferredHeight = 34f;
            barTrackLayout.flexibleWidth = 1f;

            Image barTrackImage = EnsureImage(barTrack.gameObject, new Color(0.14f, 0.16f, 0.28f, 0.94f), Image.Type.Sliced);

            RectTransform fillRect = FindOrCreateRect(barTrack, "Loading Bar Fill");
            StretchRect(fillRect);
            Image fillImage = EnsureImage(fillRect.gameObject, accentColor, Image.Type.Filled);
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0f;
            refs.loadingProgressFill = fillImage;
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

        private void BuildLeaderboardPanel(Transform panelRoot, ChallengeUiRefs refs)
        {
            RectTransform card = FindOrCreateCard(panelRoot, "Challenge Leaderboard Card", new Vector2(860f, 1200f), panelColor);
            VerticalLayoutGroup layout = EnsureVerticalLayout(card.gameObject, new RectOffset(44, 44, 44, 44), 18f);

            CreateOrUpdateLabel(card, "Leaderboard Title", "Weekly Challenge #1", 46f, textPrimaryColor, out refs.leaderboardTitleText);
            CreateOrUpdateLabel(card, "Final Score", "00:00.000", 40f, accentColor, out refs.finalScoreText);
            CreateOrUpdateLabel(card, "Player Best", "Your Best: Not set yet", 28f, textSecondaryColor, out refs.leaderboardPlayerBestText);

            refs.submitScoreButton = FindOrCreateButton(card, "Submit Score Button", "Assign Your Score", new Vector2(0f, 80f));
            refs.leaderboardMainMenuButton = FindOrCreateButton(card, "Leaderboard Main Menu Button", "Main Menu", new Vector2(0f, 80f));

            RectTransform listRoot = FindOrCreateRect(card, "Leaderboard List");
            EnsureVerticalLayout(listRoot.gameObject, new RectOffset(0, 0, 0, 0), 12f);
            LayoutElement listRootLayout = listRoot.gameObject.GetComponent<LayoutElement>() ?? listRoot.gameObject.AddComponent<LayoutElement>();
            listRootLayout.flexibleHeight = 1f;

            refs.leaderboardEntryViews = new ChallengeLeaderboardEntryView[6];
            for (int i = 0; i < refs.leaderboardEntryViews.Length; i++)
                refs.leaderboardEntryViews[i] = FindOrCreateLeaderboardEntry(listRoot, i);
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

#if UNITY_EDITOR
            AssignLeaderboardEntryView(view, root.gameObject, root.GetComponent<Image>(), rankText, nameText, timeText, firstBadge, secondBadge, thirdBadge);
#endif
            return view;
        }

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

        private static void AssignControllerReferences(ChallengeSceneController controller, ArrowGameManager gameManager, ChallengeUiRefs refs)
        {
#if UNITY_EDITOR
            UnityEditor.SerializedObject serializedObject = new(controller);
            Assign(serializedObject, "arrowGameManager", gameManager);
            Assign(serializedObject, "loadingPanel", refs.loadingPanel);
            Assign(serializedObject, "loadingStatusText", refs.loadingStatusText);
            Assign(serializedObject, "loadingProgressFill", refs.loadingProgressFill);
            Assign(serializedObject, "countdownPanel", refs.countdownPanel);
            Assign(serializedObject, "countdownText", refs.countdownText);
            Assign(serializedObject, "challengeHudPanel", refs.challengeHudPanel);
            Assign(serializedObject, "runTimerText", refs.runTimerText);
            Assign(serializedObject, "leaderboardPanel", refs.leaderboardPanel);
            Assign(serializedObject, "leaderboardTitleText", refs.leaderboardTitleText);
            Assign(serializedObject, "leaderboardPlayerBestText", refs.leaderboardPlayerBestText);
            Assign(serializedObject, "finalScoreText", refs.finalScoreText);
            Assign(serializedObject, "submitScoreButton", refs.submitScoreButton);
            Assign(serializedObject, "leaderboardMainMenuButton", refs.leaderboardMainMenuButton);
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

        private RectTransform FindOrCreateFullScreenPanel(Transform parent, string name, bool active)
        {
            RectTransform rect = FindChildRect(parent, name);
            if (rect == null)
                rect = CreateRect(name, parent);

            StretchRect(rect);
            Image image = EnsureImage(rect.gameObject, overlayColor);
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

            Image image = EnsureImage(rect.gameObject, color, Image.Type.Sliced);
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
            Image image = EnsureImage(rect.gameObject, accentColor, Image.Type.Sliced);

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

        private static Image EnsureImage(GameObject gameObject, Color defaultColor, Image.Type defaultType = Image.Type.Simple)
        {
            Image image = gameObject.GetComponent<Image>();
            if (image == null)
            {
                image = gameObject.AddComponent<Image>();
                image.color = defaultColor;
                image.type = defaultType;
            }

            if (image.sprite == null)
                image.sprite = GetRuntimeSprite();

            return image;
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
            public GameObject loadingPanel;
            public GameObject countdownPanel;
            public GameObject challengeHudPanel;
            public GameObject leaderboardPanel;
            public Image loadingProgressFill;
            public TextMeshProUGUI loadingStatusText;
            public TextMeshProUGUI countdownText;
            public TextMeshProUGUI runTimerText;
            public TextMeshProUGUI leaderboardTitleText;
            public TextMeshProUGUI leaderboardPlayerBestText;
            public TextMeshProUGUI finalScoreText;
            public Button submitScoreButton;
            public Button leaderboardMainMenuButton;
            public ChallengeLeaderboardEntryView[] leaderboardEntryViews;
        }
    }
}
