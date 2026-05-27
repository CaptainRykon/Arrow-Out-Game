using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowGame
{
    [ExecuteAlways]
    public class ChallengeSceneAuthoring : MonoBehaviour
    {
        private static Sprite runtimeSprite;
        private static TMP_FontAsset leaderboardEntryFontAsset;

        private readonly Color overlayColor = new(0.05f, 0.06f, 0.1f, 1f);
        private readonly Color panelColor = new(0.12f, 0.13f, 0.22f, 1f);
        private readonly Color panelSoftColor = new(0.17f, 0.18f, 0.29f, 1f);
        private readonly Color accentColor = new(0.35f, 0.43f, 0.98f, 1f);
        private readonly Color textPrimaryColor = new(0.95f, 0.96f, 1f, 1f);
        private readonly Color textSecondaryColor = new(0.67f, 0.7f, 0.84f, 1f);

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif
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
            RemoveLegacyStreakUi(canvasRoot);
            refs.loadingPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge Loading Panel", false).gameObject;
            refs.countdownPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge Countdown Panel", false).gameObject;
            refs.challengeHudPanel = FindOrCreateFullScreenPanel(canvasRoot, "Challenge HUD Panel", false).gameObject;
            refs.leaderboardPanel = gameManager.winUI != null ? gameManager.winUI : FindOrCreateFullScreenPanel(canvasRoot, "Challenge Leaderboard Panel", false).gameObject;
            RemoveLegacyContinueButton(refs.leaderboardPanel.transform);

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
            bool timerHolderExists = FindChildRect(panelRoot, "Run Timer Holder") != null;
            RectTransform timerHolder = FindOrCreateCard(panelRoot, "Run Timer Holder", new Vector2(360f, 120f), panelColor);
            if (!timerHolderExists)
            {
                timerHolder.anchorMin = new Vector2(0.5f, 1f);
                timerHolder.anchorMax = new Vector2(0.5f, 1f);
                timerHolder.pivot = new Vector2(0.5f, 1f);
                timerHolder.anchoredPosition = new Vector2(0f, -40f);
            }
            CreateOrUpdateLabel(timerHolder, "Run Timer", "00:00.000", 58f, textPrimaryColor, out refs.runTimerText);
        }

        private void BuildLeaderboardPanel(Transform panelRoot, ChallengeUiRefs refs)
        {
            RectTransform card = FindOrCreateCard(panelRoot, "Challenge Leaderboard Card", new Vector2(860f, 1200f), panelColor);
            VerticalLayoutGroup layout = EnsureVerticalLayout(card.gameObject, new RectOffset(44, 44, 44, 44), 18f);

            CreateOrUpdateLabel(card, "Leaderboard Title", "Weekly Challenge #1", 46f, textPrimaryColor, out refs.leaderboardTitleText);
            CreateOrUpdateLabel(card, "Final Score", "00:00.000", 40f, accentColor, out refs.finalScoreText);
            CreateOrUpdateLabel(card, "Player Best", "Your Best: Not set yet", 28f, textSecondaryColor, out refs.leaderboardPlayerBestText);

            refs.submitScoreButton = FindOrCreateButton(card, "Submit Score Button", "Mint Your Score", new Vector2(0f, 80f));
            refs.leaderboardMainMenuButton = FindOrCreateButton(card, "Leaderboard Main Menu Button", "Main Menu", new Vector2(0f, 80f));

            RectTransform scrollRoot = FindOrCreateRect(card, "Leaderboard Scroll View");
            LayoutElement scrollLayout = scrollRoot.gameObject.GetComponent<LayoutElement>() ?? scrollRoot.gameObject.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1f;
            scrollLayout.preferredHeight = 720f;
            Image scrollBackground = scrollRoot.gameObject.GetComponent<Image>() ?? scrollRoot.gameObject.AddComponent<Image>();
            scrollBackground.color = new Color(1f, 1f, 1f, 0.02f);

            ScrollRect scrollRect = scrollRoot.gameObject.GetComponent<ScrollRect>() ?? scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 24f;

            RectTransform viewport = FindOrCreateRect(scrollRoot, "Leaderboard Viewport");
            StretchRect(viewport);
            Image viewportImage = viewport.gameObject.GetComponent<Image>() ?? viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            if (viewport.gameObject.GetComponent<RectMask2D>() == null)
                viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform listRoot = FindOrCreateRect(viewport, "Leaderboard List");
            listRoot.anchorMin = new Vector2(0f, 1f);
            listRoot.anchorMax = new Vector2(1f, 1f);
            listRoot.pivot = new Vector2(0.5f, 1f);
            listRoot.anchoredPosition = Vector2.zero;
            listRoot.sizeDelta = Vector2.zero;
            EnsureVerticalLayout(listRoot.gameObject, new RectOffset(0, 0, 0, 0), 12f);
            ContentSizeFitter listFitter = listRoot.gameObject.GetComponent<ContentSizeFitter>() ?? listRoot.gameObject.AddComponent<ContentSizeFitter>();
            listFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport;
            scrollRect.content = listRoot;

            refs.leaderboardEntryViews = new ChallengeLeaderboardEntryView[25];
            for (int i = 0; i < refs.leaderboardEntryViews.Length; i++)
                refs.leaderboardEntryViews[i] = FindOrCreateLeaderboardEntry(listRoot, i);

            RectTransform statusPanel = FindOrCreateCard(panelRoot, "Mint Status Panel", new Vector2(640f, 120f), accentColor);
            statusPanel.anchorMin = new Vector2(0.5f, 1f);
            statusPanel.anchorMax = new Vector2(0.5f, 1f);
            statusPanel.pivot = new Vector2(0.5f, 1f);
            statusPanel.anchoredPosition = new Vector2(0f, -36f);
            statusPanel.gameObject.SetActive(false);
            CreateOrUpdateLabel(statusPanel, "Mint Status Text", "Your score was successfully minted.", 28f, Color.white, out refs.mintStatusText);
            refs.mintStatusPanel = statusPanel.gameObject;
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
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            CreateOrUpdateLabel(root, "Rank Text", $"{index + 1}", 34f, textPrimaryColor, out TextMeshProUGUI rankText, new Vector2(225f, 84f));
            CreateOrUpdateLabel(root, "Name Text", $"Player {index + 1}", 30f, textPrimaryColor, out TextMeshProUGUI nameText, new Vector2(225f, 84f));
            CreateOrUpdateLabel(root, "Time Text", "00:00.000", 30f, textPrimaryColor, out TextMeshProUGUI timeText, new Vector2(225f, 84f));

            GameObject firstBadge = FindOrCreateMarker(root, "First Place Badge", "1", 28f, Color.white);
            GameObject secondBadge = FindOrCreateMarker(root, "Second Place Badge", "2", 28f, Color.white);
            GameObject thirdBadge = FindOrCreateMarker(root, "Third Place Badge", "3", 28f, Color.white);
            firstBadge.SetActive(false);
            secondBadge.SetActive(false);
            thirdBadge.SetActive(false);

            ApplyLeaderboardEntryFont(rankText);
            ApplyLeaderboardEntryFont(nameText);
            ApplyLeaderboardEntryFont(timeText);
            ApplyLeaderboardEntryFont(firstBadge != null ? firstBadge.GetComponent<TextMeshProUGUI>() : null);
            ApplyLeaderboardEntryFont(secondBadge != null ? secondBadge.GetComponent<TextMeshProUGUI>() : null);
            ApplyLeaderboardEntryFont(thirdBadge != null ? thirdBadge.GetComponent<TextMeshProUGUI>() : null);

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

        private static void RemoveLegacyStreakUi(Transform canvasRoot)
        {
            RectTransform streakPanel = FindChildRect(canvasRoot, "Challenge Streak Panel");
            if (streakPanel == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.DestroyObjectImmediate(streakPanel.gameObject);
                return;
            }
#endif

            DestroyImmediate(streakPanel.gameObject);
        }

        private static void RemoveLegacyContinueButton(Transform root)
        {
            Button continueButton = FindButtonByLabel(root, "continue");
            if (continueButton == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.DestroyObjectImmediate(continueButton.gameObject);
                return;
            }
#endif

            DestroyImmediate(continueButton.gameObject);
        }

        private static void AssignControllerReferences(ChallengeSceneController controller, ArrowGameManager gameManager, ChallengeUiRefs refs)
        {
#if UNITY_EDITOR
            UnityEditor.SerializedObject serializedObject = new(controller);
            SerializedReferenceUtility.Assign(serializedObject, "arrowGameManager", gameManager);
            SerializedReferenceUtility.Assign(serializedObject, "loadingPanel", refs.loadingPanel);
            SerializedReferenceUtility.Assign(serializedObject, "loadingStatusText", refs.loadingStatusText);
            SerializedReferenceUtility.Assign(serializedObject, "loadingProgressFill", refs.loadingProgressFill);
            SerializedReferenceUtility.Assign(serializedObject, "countdownPanel", refs.countdownPanel);
            SerializedReferenceUtility.Assign(serializedObject, "countdownText", refs.countdownText);
            SerializedReferenceUtility.Assign(serializedObject, "challengeHudPanel", refs.challengeHudPanel);
            SerializedReferenceUtility.Assign(serializedObject, "runTimerText", refs.runTimerText);
            SerializedReferenceUtility.Assign(serializedObject, "leaderboardPanel", refs.leaderboardPanel);
            SerializedReferenceUtility.Assign(serializedObject, "leaderboardTitleText", refs.leaderboardTitleText);
            SerializedReferenceUtility.Assign(serializedObject, "leaderboardPlayerBestText", refs.leaderboardPlayerBestText);
            SerializedReferenceUtility.Assign(serializedObject, "finalScoreText", refs.finalScoreText);
            SerializedReferenceUtility.Assign(serializedObject, "submitScoreButton", refs.submitScoreButton);
            SerializedReferenceUtility.Assign(serializedObject, "leaderboardMainMenuButton", refs.leaderboardMainMenuButton);
            SerializedReferenceUtility.AssignArray(serializedObject, "leaderboardEntryViews", refs.leaderboardEntryViews);
            SerializedReferenceUtility.Assign(serializedObject, "mintStatusPanel", refs.mintStatusPanel);
            SerializedReferenceUtility.Assign(serializedObject, "mintStatusText", refs.mintStatusText);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
#endif
        }

#if UNITY_EDITOR
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
            SerializedReferenceUtility.Assign(serializedObject, "contentRoot", contentRoot);
            SerializedReferenceUtility.Assign(serializedObject, "background", background);
            SerializedReferenceUtility.Assign(serializedObject, "rankText", rankText);
            SerializedReferenceUtility.Assign(serializedObject, "nameText", nameText);
            SerializedReferenceUtility.Assign(serializedObject, "timeText", timeText);
            SerializedReferenceUtility.Assign(serializedObject, "firstPlaceBadge", firstBadge);
            SerializedReferenceUtility.Assign(serializedObject, "secondPlaceBadge", secondBadge);
            SerializedReferenceUtility.Assign(serializedObject, "thirdPlaceBadge", thirdBadge);
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
            float alpha = image.color.a;
            image.sprite = GetRuntimeSprite();
            image.type = Image.Type.Simple;
            image.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, alpha);
            rect.gameObject.SetActive(active);
            return rect;
        }

        private RectTransform FindOrCreateCard(Transform parent, string name, Vector2 size, Color color)
        {
            RectTransform rect = FindChildRect(parent, name);
            bool created = false;
            if (rect == null)
            {
                rect = CreateRect(name, parent);
                created = true;
            }

            if (created)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                rect.anchoredPosition = Vector2.zero;
            }

            Image image = EnsureImage(rect.gameObject, color, Image.Type.Sliced);
            float alpha = image.color.a;
            image.sprite = GetRuntimeSprite();
            image.type = Image.Type.Sliced;
            image.color = new Color(color.r, color.g, color.b, alpha);
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

        private static Button FindButtonByLabel(Transform root, string labelText)
        {
            if (root == null || string.IsNullOrWhiteSpace(labelText))
                return null;

            string search = labelText.Trim().ToLowerInvariant();
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                TMP_Text label = buttons[i] != null ? buttons[i].GetComponentInChildren<TMP_Text>(true) : null;
                if (label != null && !string.IsNullOrWhiteSpace(label.text) && label.text.Trim().ToLowerInvariant() == search)
                    return buttons[i];
            }

            return null;
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
            bool createdRect = false;
            if (rect == null)
            {
                rect = CreateRect(name, parent);
                createdRect = true;
            }

            if (preferredSize.HasValue)
            {
                LayoutElement layout = rect.gameObject.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
                layout.preferredWidth = preferredSize.Value.x;
                layout.preferredHeight = preferredSize.Value.y;
            }

            StretchRect(rect);
            label = rect.GetComponent<TextMeshProUGUI>();
            bool createdLabel = false;
            if (label == null)
            {
                label = rect.gameObject.AddComponent<TextMeshProUGUI>();
                createdLabel = true;
            }

            if (createdLabel || string.IsNullOrEmpty(label.text))
                label.text = text;
            if (createdLabel || label.fontSize <= 0f)
                label.fontSize = fontSize;
            if (createdLabel)
            {
                label.color = color;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                label.textWrappingMode = TextWrappingModes.Normal;
            }

            if (createdLabel && TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;

            if (createdRect || createdLabel)
            {
                ContentSizeFitter fitter = rect.gameObject.GetComponent<ContentSizeFitter>() ?? rect.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        private static GameObject FindOrCreateMarker(Transform parent, string name, string text, float fontSize, Color color)
        {
            RectTransform rect = FindChildRect(parent, name);
            if (rect == null)
                rect = CreateRect(name, parent);

            StretchRect(rect);
            TextMeshProUGUI label = rect.GetComponent<TextMeshProUGUI>();
            bool createdLabel = false;
            if (label == null)
            {
                label = rect.gameObject.AddComponent<TextMeshProUGUI>();
                createdLabel = true;
            }

            if (createdLabel || string.IsNullOrEmpty(label.text))
                label.text = text;
            if (createdLabel || label.fontSize <= 0f)
                label.fontSize = fontSize;
            if (createdLabel)
            {
                label.color = color;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
            }

            if (createdLabel && TMP_Settings.defaultFontAsset != null)
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

        private static void ApplyLeaderboardEntryFont(TextMeshProUGUI label)
        {
            if (label == null)
                return;

            TMP_FontAsset fontAsset = GetLeaderboardEntryFontAsset();
            if (fontAsset == null)
                return;

            label.font = fontAsset;
            if (fontAsset.material != null)
                label.fontSharedMaterial = fontAsset.material;
        }

        private static TMP_FontAsset GetLeaderboardEntryFontAsset()
        {
            if (leaderboardEntryFontAsset != null)
                return leaderboardEntryFontAsset;

            leaderboardEntryFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/Octin College Rg SDF");
            return leaderboardEntryFontAsset;
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
            public GameObject mintStatusPanel;
            public TextMeshProUGUI mintStatusText;
            public Button submitScoreButton;
            public Button leaderboardMainMenuButton;
            public ChallengeLeaderboardEntryView[] leaderboardEntryViews;
        }
    }
}
