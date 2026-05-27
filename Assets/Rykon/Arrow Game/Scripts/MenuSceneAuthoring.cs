using System.Collections.Generic;
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
            public Button challengePlayButton;
        }

        private sealed class SettingsMenuReferences
        {
            public TMP_InputField userNameInputField;
            public Button userNameEditButton;
            public GameObject userNameEditPanel;
            public TMP_InputField userNameEditInputField;
            public Button userNameEditSaveButton;
            public Button userNameEditCancelButton;
            public TextMeshProUGUI userNameEditTitleText;
            public TextMeshProUGUI userNameEditStatusText;
            public Button vibrationToggleButton;
            public Image vibrationToggleBackground;
            public RectTransform vibrationToggleKnob;
            public Button soundToggleButton;
            public Image soundToggleBackground;
            public RectTransform soundToggleKnob;
            public Button darkModeToggleButton;
            public Image darkModeToggleBackground;
            public RectTransform darkModeToggleKnob;
            public Button privacyButton;
            public Button termsButton;
            public Button faqButton;
            public Button telegramButton;
            public Button twitterButton;
            public Image[] themeSurfaceImages;
            public Image[] themeAccentImages;
            public TextMeshProUGUI[] themePrimaryTexts;
            public TextMeshProUGUI[] themeSecondaryTexts;
        }

        private sealed class PurchaseGateReferences
        {
            public GameObject panel;
            public Button payButton;
            public TextMeshProUGUI titleText;
            public TextMeshProUGUI bodyText;
            public TextMeshProUGUI priceText;
            public TextMeshProUGUI statusText;
        }

        private static Sprite runtimeSprite;
        private static TMP_FontAsset primaryUiFontAsset;
        private static TMP_FontAsset displayFontAsset;

        private readonly Color cardColor = new(0.18f, 0.2f, 0.31f, 1f);
        private readonly Color accentColor = new(0.35f, 0.43f, 0.98f, 1f);
        private readonly Color textPrimaryColor = new(0.95f, 0.96f, 1f, 1f);
        private readonly Color textSecondaryColor = new(0.67f, 0.7f, 0.84f, 1f);
        private readonly Color settingsPanelColor = new(0.16f, 0.16f, 0.18f, 1f);
        private readonly Color settingsCardColor = new(0.25f, 0.41f, 0.59f, 1f);
        private readonly Color settingsAccentColor = new(1f, 0.82f, 0.29f, 1f);
        private readonly Color toggleKnobColor = Color.white;

        private void OnEnable()
        {
            if (Application.isPlaying)
                return;

            EnsureHomeShopUi();
            EnsureChallengeUi();
            EnsureSettingsUi();
            EnsurePurchaseGateUi();
            EnsurePreferredUiFonts();
        }

        [ContextMenu("Rebuild Menu Hierarchy")]
        public void RebuildMenuHierarchy()
        {
            EnsureHomeShopUi(forceRebuild: true);
            EnsureChallengeUi(forceRebuild: true);
            EnsureSettingsUi(forceRebuild: true);
            EnsurePurchaseGateUi(forceRebuild: true);
            EnsurePreferredUiFonts();
        }

        private void EnsurePreferredUiFonts()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            bool canvasChanged = false;
            if (!canvas.pixelPerfect)
            {
                canvas.pixelPerfect = true;
                canvasChanged = true;
            }

            TMP_FontAsset primaryFont = GetPrimaryUiFontAsset();
            TMP_FontAsset displayFont = GetDisplayFontAsset();
            if (primaryFont == null)
                return;

            TextMeshProUGUI[] labels = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI label in labels)
            {
                TMP_FontAsset desiredFont = ShouldUseDisplayFont(label) && displayFont != null
                    ? displayFont
                    : primaryFont;

                bool changed = false;
                if (label.font != desiredFont)
                {
                    label.font = desiredFont;
                    changed = true;
                }

                Material sharedMaterial = desiredFont.material;
                if (sharedMaterial != null && label.fontSharedMaterial != sharedMaterial)
                {
                    label.fontSharedMaterial = sharedMaterial;
                    changed = true;
                }

                if (label.extraPadding)
                {
                    label.extraPadding = false;
                    changed = true;
                }

                if (!label.isTextObjectScaleStatic)
                {
                    label.isTextObjectScaleStatic = true;
                    changed = true;
                }

                if (!label.enableAutoSizing)
                {
                    float roundedFontSize = Mathf.Round(label.fontSize);
                    if (!Mathf.Approximately(label.fontSize, roundedFontSize))
                    {
                        label.fontSize = roundedFontSize;
                        changed = true;
                    }
                }

                RectTransform rectTransform = label.rectTransform;
                if (rectTransform != null)
                {
                    Vector2 anchoredPosition = rectTransform.anchoredPosition;
                    Vector2 roundedAnchoredPosition = new(
                        Mathf.Round(anchoredPosition.x),
                        Mathf.Round(anchoredPosition.y));

                    if ((roundedAnchoredPosition - anchoredPosition).sqrMagnitude > 0.0001f)
                    {
                        rectTransform.anchoredPosition = roundedAnchoredPosition;
                        changed = true;
                    }

                    Vector2 sizeDelta = rectTransform.sizeDelta;
                    Vector2 roundedSizeDelta = new(
                        Mathf.Round(sizeDelta.x),
                        Mathf.Round(sizeDelta.y));

                    if ((roundedSizeDelta - sizeDelta).sqrMagnitude > 0.0001f)
                    {
                        rectTransform.sizeDelta = roundedSizeDelta;
                        changed = true;
                    }
                }

                if (changed)
                {
                    label.UpdateMeshPadding();
                    label.ForceMeshUpdate();
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(label);
                    if (label.gameObject.scene.IsValid())
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(label.gameObject.scene);
#endif
                }
            }

            if (canvasChanged)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(canvas);
                if (canvas.gameObject.scene.IsValid())
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
#endif
            }
        }

        private static TMP_FontAsset GetPrimaryUiFontAsset()
        {
            if (primaryUiFontAsset == null)
                primaryUiFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/Octin College Rg SDF");

            return primaryUiFontAsset != null ? primaryUiFontAsset : TMP_Settings.defaultFontAsset;
        }

        private static TMP_FontAsset GetDisplayFontAsset()
        {
            if (displayFontAsset == null)
                displayFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/Sports World-Regular SDF");

            return displayFontAsset;
        }

        private static bool ShouldUseDisplayFont(TextMeshProUGUI label)
        {
            if (label == null)
                return false;

            string objectName = label.gameObject.name;
            string text = label.text ?? string.Empty;
            return objectName.Contains("Game Title") ||
                   text == "Arrows Game" ||
                   text == "ARROWS Game";
        }

        private static void ApplyPreferredFont(TextMeshProUGUI label, bool useDisplayFont = false)
        {
            if (label == null)
                return;

            TMP_FontAsset fontAsset = useDisplayFont ? GetDisplayFontAsset() : GetPrimaryUiFontAsset();
            if (fontAsset == null)
                fontAsset = TMP_Settings.defaultFontAsset;

            if (fontAsset == null)
                return;

            label.font = fontAsset;
            if (fontAsset.material != null)
                label.fontSharedMaterial = fontAsset.material;
            label.extraPadding = false;
            label.isTextObjectScaleStatic = true;
            label.UpdateMeshPadding();
            label.ForceMeshUpdate();
        }

        private void EnsureHomeShopUi(bool forceRebuild = false)
        {
            MenuSceneController controller = GetComponent<MenuSceneController>();
            if (controller == null)
                return;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            EnsureEventSystem();

            Transform homePanel = FindDeepChild(canvas.transform, "Home Panel");
            if (homePanel == null)
                return;

            Transform shopButton = FindDeepChild(homePanel, "Shop Button");
            if (forceRebuild && shopButton != null)
                DestroyEditorSafe(shopButton.gameObject);

            if (shopButton == null)
                BuildHomeShopButton(homePanel);

            Transform leaderboardButton = FindDeepChild(homePanel, "Leaderboard Button");
            if (forceRebuild && leaderboardButton != null)
                DestroyEditorSafe(leaderboardButton.gameObject);

            if (leaderboardButton == null)
                BuildHomeLeaderboardButton(homePanel);

            EnsureHomeActionButtonsLayout(homePanel);

            Transform shopPanel = FindDeepChild(canvas.transform, "Shop Panel");
            if (forceRebuild || NeedsShopUiRebuild(shopPanel))
            {
                if (shopPanel != null)
                    DestroyEditorSafe(shopPanel.gameObject);

                BuildShopPanel(canvas.transform);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(controller);
            if (controller.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
#endif
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
            if (streakPanel != null)
                DestroyEditorSafe(streakPanel.gameObject);

            Transform streakButton = challengeMenuPanel != null ? FindDeepChild(challengeMenuPanel, "Streak Button") : null;
            if (streakButton != null)
                DestroyEditorSafe(streakButton.gameObject);

            ChallengeMenuReferences refs = CollectChallengeReferences(challengePanel, canvas.transform);
            if (!HasRequiredChallengeReferences(refs))
                return;

#if UNITY_EDITOR
            AssignChallengeReferences(controller, refs);
#endif

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(controller);
            if (controller.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
#endif
        }

        private void EnsureSettingsUi(bool forceRebuild = false)
        {
            MenuSceneController controller = GetComponent<MenuSceneController>();
            if (controller == null)
                return;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            EnsureEventSystem();

            Transform settingsPanel = FindDeepChild(canvas.transform, "Settings Panel");
            if (settingsPanel == null)
                return;

            EnsureSettingsNameEditUi(settingsPanel, canvas.transform);

            SettingsMenuReferences refs = CollectSettingsReferences(settingsPanel);
            if (!HasRequiredSettingsReferences(refs))
                return;

#if UNITY_EDITOR
            AssignSettingsReferences(controller, refs);
#endif

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(controller);
            if (controller.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
#endif
        }

        private void EnsurePurchaseGateUi(bool forceRebuild = false)
        {
            MenuSceneController controller = GetComponent<MenuSceneController>();
            if (controller == null)
                return;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            EnsureEventSystem();

            Transform purchaseGatePanel = FindDeepChild(canvas.transform, "Purchase Gate Panel");
            if (forceRebuild || NeedsPurchaseGateUiRebuild(purchaseGatePanel))
            {
                if (purchaseGatePanel != null)
                    DestroyEditorSafe(purchaseGatePanel.gameObject);

                BuildPurchaseGatePanel(canvas.transform);
            }

            Transform purchaseSuccessPanel = FindDeepChild(canvas.transform, "Purchase Success Panel");
            if (forceRebuild || NeedsPurchaseSuccessUiRebuild(purchaseSuccessPanel))
            {
                if (purchaseSuccessPanel != null)
                    DestroyEditorSafe(purchaseSuccessPanel.gameObject);

                BuildPurchaseSuccessPanel(canvas.transform);
            }

            Transform hintRewardPanel = FindDeepChild(canvas.transform, "Hint Reward Panel");
            if (forceRebuild || NeedsHintRewardUiRebuild(hintRewardPanel))
            {
                if (hintRewardPanel != null)
                    DestroyEditorSafe(hintRewardPanel.gameObject);

                BuildHintRewardPanel(canvas.transform);
            }

            Transform paymentFailedPanel = FindDeepChild(canvas.transform, "Payment Failed Panel");
            if (forceRebuild || NeedsPaymentFailedUiRebuild(paymentFailedPanel))
            {
                if (paymentFailedPanel != null)
                    DestroyEditorSafe(paymentFailedPanel.gameObject);

                BuildPaymentFailedPanel(canvas.transform);
            }

            Transform menuLeaderboardPanel = FindDeepChild(canvas.transform, "Menu Leaderboard Panel");
            if (forceRebuild || NeedsMenuLeaderboardUiRebuild(menuLeaderboardPanel))
            {
                if (menuLeaderboardPanel != null)
                    DestroyEditorSafe(menuLeaderboardPanel.gameObject);

                BuildMenuLeaderboardPanel(canvas.transform);
            }

            PurchaseGateReferences refs = CollectPurchaseGateReferences(canvas.transform);
            if (!HasRequiredPurchaseGateReferences(refs))
                return;

#if UNITY_EDITOR
            AssignPurchaseGateReferences(controller, refs);
#endif

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
                   FindDeepChild(challengeMenuPanel, "Play Challenge Button") == null;
        }

        private static bool NeedsShopUiRebuild(Transform shopPanel)
        {
            return shopPanel == null ||
                   FindDeepChild(shopPanel, "Shop Card") == null ||
                   FindDeepChild(shopPanel, "Shop Header") == null ||
                   FindDeepChild(shopPanel, "Shop Close Button") == null ||
                   FindDeepChild(shopPanel, "Hint Offer Card") == null ||
                   FindDeepChild(shopPanel, "Hint Buy Button") == null;
        }

        private static bool NeedsPurchaseGateUiRebuild(Transform purchaseGatePanel)
        {
            return purchaseGatePanel == null ||
                   FindDeepChild(purchaseGatePanel, "Purchase Gate Card") == null ||
                   FindDeepChild(purchaseGatePanel, "Purchase Gate Title") == null ||
                   FindDeepChild(purchaseGatePanel, "Purchase Gate Body") == null ||
                   FindDeepChild(purchaseGatePanel, "Purchase Gate Price Text") == null ||
                   FindDeepChild(purchaseGatePanel, "Purchase Gate Status Text") == null ||
                   FindDeepChild(purchaseGatePanel, "Purchase Gate Pay Button") == null;
        }

        private static bool NeedsPurchaseSuccessUiRebuild(Transform purchaseSuccessPanel)
        {
            return purchaseSuccessPanel == null ||
                   FindDeepChild(purchaseSuccessPanel, "Purchase Success Card") == null ||
                   FindDeepChild(purchaseSuccessPanel, "Purchase Success Title") == null ||
                   FindDeepChild(purchaseSuccessPanel, "Purchase Success Body") == null ||
                   FindDeepChild(purchaseSuccessPanel, "Purchase Success Name Input") == null ||
                   FindDeepChild(purchaseSuccessPanel, "Purchase Success OK Button") == null;
        }

        private static bool NeedsPaymentFailedUiRebuild(Transform paymentFailedPanel)
        {
            return paymentFailedPanel == null ||
                   FindDeepChild(paymentFailedPanel, "Payment Failed Card") == null ||
                   FindDeepChild(paymentFailedPanel, "Payment Failed Title") == null ||
                   FindDeepChild(paymentFailedPanel, "Payment Failed Body") == null ||
                   FindDeepChild(paymentFailedPanel, "Payment Failed Retry Button") == null;
        }

        private static bool NeedsHintRewardUiRebuild(Transform hintRewardPanel)
        {
            return hintRewardPanel == null ||
                   FindDeepChild(hintRewardPanel, "Hint Reward Card") == null ||
                   FindDeepChild(hintRewardPanel, "Hint Reward Title") == null ||
                   FindDeepChild(hintRewardPanel, "Hint Reward Body") == null ||
                   FindDeepChild(hintRewardPanel, "Hint Reward Status Text") == null ||
                   FindDeepChild(hintRewardPanel, "Hint Reward OK Button") == null;
        }

        private static bool NeedsMenuLeaderboardUiRebuild(Transform leaderboardPanel)
        {
            return leaderboardPanel == null ||
                   FindDeepChild(leaderboardPanel, "Menu Leaderboard Card") == null ||
                   FindDeepChild(leaderboardPanel, "Menu Leaderboard Title") == null ||
                   FindDeepChild(leaderboardPanel, "Menu Leaderboard List") == null ||
                   FindDeepChild(leaderboardPanel, "Menu Leaderboard Scroll View") == null ||
                   FindDeepChild(leaderboardPanel, "Menu Leaderboard Close Button") == null ||
                   leaderboardPanel.GetComponentsInChildren<ChallengeLeaderboardEntryView>(true).Length < 25;
        }

        private void BuildHomeShopButton(Transform homePanel)
        {
            BuildHomeActionButton(homePanel, "Shop Button", "Shop", new Color(0.95f, 0.68f, 0.15f, 1f));
        }

        private void BuildHomeLeaderboardButton(Transform homePanel)
        {
            BuildHomeActionButton(homePanel, "Leaderboard Button", "Leaderboard", new Color(0.35f, 0.43f, 0.98f, 1f));
        }

        private void BuildHomeActionButton(Transform homePanel, string buttonName, string label, Color backgroundColor)
        {
            Transform playButton = FindDeepChild(homePanel, "Play Button");
            RectTransform playRect = playButton as RectTransform;
            RectTransform buttonRect = CreateRect(buttonName, homePanel);
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = playRect != null && playRect.rect.size.sqrMagnitude > 0.01f
                ? playRect.rect.size
                : new Vector2(925f, 120f);
            buttonRect.anchoredPosition = playRect != null
                ? playRect.anchoredPosition + new Vector2(0f, -(buttonRect.sizeDelta.y + 44f))
                : new Vector2(0f, -220f);

            LayoutElement layout = buttonRect.gameObject.GetComponent<LayoutElement>() ?? buttonRect.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = false;
            layout.flexibleWidth = 1f;
            layout.preferredHeight = buttonRect.sizeDelta.y;
            layout.preferredWidth = buttonRect.sizeDelta.x;

            Image image = EnsureImage(buttonRect.gameObject, backgroundColor, Image.Type.Sliced);
            Button button = buttonRect.gameObject.GetComponent<Button>() ?? buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            RectTransform shineRect = FindDeepChild(buttonRect, "Top Shine") as RectTransform ?? CreateRect("Top Shine", buttonRect);
            shineRect.anchorMin = new Vector2(0.08f, 0.56f);
            shineRect.anchorMax = new Vector2(0.92f, 0.88f);
            shineRect.offsetMin = Vector2.zero;
            shineRect.offsetMax = Vector2.zero;
            EnsureImage(shineRect.gameObject, new Color(1f, 0.9f, 0.55f, 0.45f), Image.Type.Sliced);

            TextMeshProUGUI buttonLabel = GetText(buttonRect, "Label");
            if (buttonLabel == null)
                CreateNamedLabel(buttonRect, "Label", label, 34f, textPrimaryColor);
            else
                buttonLabel.text = label;
        }

        private void EnsureHomeActionButtonsLayout(Transform homePanel)
        {
            RectTransform playRect = FindDeepChild(homePanel, "Play Button") as RectTransform;
            if (playRect == null)
                return;

            RectTransform stack = FindDeepChild(homePanel, "Home Action Buttons") as RectTransform;
            if (stack == null)
            {
                stack = CreateRect("Home Action Buttons", homePanel);
                stack.anchorMin = new Vector2(0.5f, 0.5f);
                stack.anchorMax = new Vector2(0.5f, 0.5f);
                stack.pivot = new Vector2(0.5f, 0.5f);
                stack.sizeDelta = new Vector2(playRect.rect.width > 0f ? playRect.rect.width : 925f, 0f);
                stack.anchoredPosition = playRect.anchoredPosition;
            }

            VerticalLayoutGroup layout = stack.gameObject.GetComponent<VerticalLayoutGroup>() ?? stack.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = stack.gameObject.GetComponent<ContentSizeFitter>() ?? stack.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            PrepareHomeActionButtonForLayout(playRect, stack);
            PrepareHomeActionButtonForLayout(FindDeepChild(homePanel, "Shop Button") as RectTransform, stack);
            PrepareHomeActionButtonForLayout(FindDeepChild(homePanel, "Leaderboard Button") as RectTransform, stack);
        }

        private static void PrepareHomeActionButtonForLayout(RectTransform buttonRect, RectTransform stack)
        {
            if (buttonRect == null || stack == null)
                return;

            buttonRect.SetParent(stack, false);
            buttonRect.anchorMin = new Vector2(0.5f, 1f);
            buttonRect.anchorMax = new Vector2(0.5f, 1f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.localScale = Vector3.one;

            LayoutElement layout = buttonRect.gameObject.GetComponent<LayoutElement>() ?? buttonRect.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = false;
            layout.flexibleWidth = 1f;
            layout.preferredWidth = buttonRect.sizeDelta.x > 1f ? buttonRect.sizeDelta.x : 925f;
            layout.preferredHeight = buttonRect.sizeDelta.y > 1f ? buttonRect.sizeDelta.y : 120f;
        }

        private void BuildShopPanel(Transform canvasRoot)
        {
            RectTransform overlay = CreateRect("Shop Panel", canvasRoot);
            StretchRect(overlay);
            EnsureImage(overlay.gameObject, new Color(0.04f, 0.05f, 0.08f, 0.76f));
            overlay.gameObject.SetActive(false);

            RectTransform card = CreateRect("Shop Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(760f, 980f);
            card.anchoredPosition = Vector2.zero;
            EnsureImage(card.gameObject, new Color(0.98f, 0.91f, 0.69f, 1f), Image.Type.Sliced);

            RectTransform header = CreateRect("Shop Header", card);
            header.anchorMin = new Vector2(0.5f, 1f);
            header.anchorMax = new Vector2(0.5f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(430f, 120f);
            header.anchoredPosition = new Vector2(0f, 24f);
            EnsureImage(header.gameObject, new Color(0.95f, 0.68f, 0.15f, 1f), Image.Type.Sliced);
            CreateNamedLabel(header, "Shop Title", "SHOP", 54f, Color.white);

            RectTransform closeRect = CreateRect("Shop Close Button", card);
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.sizeDelta = new Vector2(92f, 92f);
            closeRect.anchoredPosition = new Vector2(-58f, -52f);
            Image closeImage = EnsureImage(closeRect.gameObject, new Color(0.28f, 0.76f, 0.95f, 1f), Image.Type.Sliced);
            Button closeButton = closeRect.gameObject.AddComponent<Button>();
            closeButton.targetGraphic = closeImage;
            CreateNamedLabel(closeRect, "Label", "X", 38f, Color.white);

            RectTransform offersRoot = CreateRect("Shop Offers", card);
            offersRoot.anchorMin = new Vector2(0.5f, 0.5f);
            offersRoot.anchorMax = new Vector2(0.5f, 0.5f);
            offersRoot.pivot = new Vector2(0.5f, 0.5f);
            offersRoot.sizeDelta = new Vector2(620f, 520f);
            offersRoot.anchoredPosition = new Vector2(0f, -30f);

            VerticalLayoutGroup offersLayout = offersRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            offersLayout.padding = new RectOffset(0, 0, 0, 0);
            offersLayout.spacing = 28f;
            offersLayout.childAlignment = TextAnchor.UpperCenter;
            offersLayout.childControlWidth = true;
            offersLayout.childControlHeight = false;
            offersLayout.childForceExpandWidth = true;
            offersLayout.childForceExpandHeight = false;

            CreateShopOfferCard(offersRoot, "Hint", "H", "5 Hints", "$0.10");
        }

        private void BuildPurchaseGatePanel(Transform canvasRoot)
        {
            RectTransform overlay = CreateRect("Purchase Gate Panel", canvasRoot);
            StretchRect(overlay);
            EnsureImage(overlay.gameObject, new Color(0.04f, 0.05f, 0.08f, 0.8f));
            overlay.gameObject.SetActive(false);

            RectTransform card = CreateRect("Purchase Gate Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(720f, 620f);
            card.anchoredPosition = Vector2.zero;
            EnsureImage(card.gameObject, Color.white, Image.Type.Sliced);

            RectTransform titleRect = CreateRect("Purchase Gate Title", card);
            titleRect.anchorMin = new Vector2(0.1f, 1f);
            titleRect.anchorMax = new Vector2(0.9f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 96f);
            titleRect.anchoredPosition = new Vector2(0f, -42f);
            CreateRectLabel(titleRect, "Unlock Arrow Out", 46f, new Color(0.16f, 0.18f, 0.3f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;

            RectTransform bodyRect = CreateRect("Purchase Gate Body", card);
            bodyRect.anchorMin = new Vector2(0.12f, 0.46f);
            bodyRect.anchorMax = new Vector2(0.88f, 0.74f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;
            CreateRectLabel(bodyRect, "Finish the tutorial, then pay once in MiniPay to unlock the full game.", 29f, new Color(0.28f, 0.31f, 0.48f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;

            RectTransform priceBadge = CreateRect("Purchase Gate Price Badge", card);
            priceBadge.anchorMin = new Vector2(0.5f, 0.5f);
            priceBadge.anchorMax = new Vector2(0.5f, 0.5f);
            priceBadge.pivot = new Vector2(0.5f, 0.5f);
            priceBadge.sizeDelta = new Vector2(260f, 100f);
            priceBadge.anchoredPosition = new Vector2(0f, 16f);
            EnsureImage(priceBadge.gameObject, new Color(0.95f, 0.68f, 0.15f, 1f), Image.Type.Sliced);

            RectTransform priceTextRect = CreateRect("Purchase Gate Price Text", priceBadge);
            StretchRect(priceTextRect);
            CreateRectLabel(priceTextRect, "$0.50", 42f, Color.white, TextAlignmentOptions.Center);

            RectTransform statusRect = CreateRect("Purchase Gate Status Text", card);
            statusRect.anchorMin = new Vector2(0.12f, 0.22f);
            statusRect.anchorMax = new Vector2(0.88f, 0.34f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            CreateRectLabel(statusRect, "Complete the payment to continue into the main game.", 24f, new Color(0.35f, 0.38f, 0.53f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;

            RectTransform buttonRect = CreateRect("Purchase Gate Pay Button", card);
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.sizeDelta = new Vector2(340f, 96f);
            buttonRect.anchoredPosition = new Vector2(0f, 42f);
            Image buttonImage = EnsureImage(buttonRect.gameObject, accentColor, Image.Type.Sliced);
            Button payButton = buttonRect.gameObject.AddComponent<Button>();
            payButton.targetGraphic = buttonImage;

            RectTransform buttonLabelRect = CreateRect("Label", buttonRect);
            StretchRect(buttonLabelRect);
            CreateRectLabel(buttonLabelRect, "Pay", 34f, Color.white, TextAlignmentOptions.Center);
        }

        private void BuildPurchaseSuccessPanel(Transform canvasRoot)
        {
            RectTransform overlay = CreateRect("Purchase Success Panel", canvasRoot);
            StretchRect(overlay);
            EnsureImage(overlay.gameObject, new Color(0.04f, 0.05f, 0.08f, 0.8f));
            overlay.gameObject.SetActive(false);

            RectTransform card = CreateRect("Purchase Success Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(760f, 660f);
            card.anchoredPosition = Vector2.zero;
            EnsureImage(card.gameObject, Color.white, Image.Type.Sliced);

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(52, 52, 52, 52);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateStandaloneLabel(card, "Purchase Success Title", "Payment Successful", 44f, new Color(0.16f, 0.18f, 0.3f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;
            CreateStandaloneLabel(card, "Purchase Success Body", "Welcome to Arrow Out. Enter your player name to continue.", 28f, new Color(0.28f, 0.31f, 0.48f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;
            CreateInputField(card, "Purchase Success Name Input", "Enter your name");
            CreateStandaloneLabel(card, "Purchase Success Status Text", "Your name will be saved to MiniPay.", 22f, new Color(0.35f, 0.38f, 0.53f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;

            Button okButton = CreateButton(card, "Purchase Success OK", new Vector2(0f, 92f));
            okButton.gameObject.name = "Purchase Success OK Button";
            TextMeshProUGUI okLabel = GetText(okButton.transform, "Label");
            if (okLabel != null)
                okLabel.text = "OK";
        }

        private void BuildHintRewardPanel(Transform canvasRoot)
        {
            RectTransform overlay = CreateRect("Hint Reward Panel", canvasRoot);
            StretchRect(overlay);
            EnsureImage(overlay.gameObject, new Color(0.04f, 0.05f, 0.08f, 0.8f));
            overlay.gameObject.SetActive(false);

            RectTransform card = CreateRect("Hint Reward Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(760f, 520f);
            card.anchoredPosition = Vector2.zero;
            EnsureImage(card.gameObject, Color.white, Image.Type.Sliced);

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(52, 52, 52, 52);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateStandaloneLabel(card, "Hint Reward Title", "Free Hints Unlocked", 44f, new Color(0.16f, 0.18f, 0.3f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;
            CreateStandaloneLabel(card, "Hint Reward Body", "Congrats! You received 5 free hints with your unlock purchase.", 28f, new Color(0.28f, 0.31f, 0.48f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;
            CreateStandaloneLabel(card, "Hint Reward Status Text", "These hints are saved to your MiniPay account.", 22f, new Color(0.35f, 0.38f, 0.53f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;

            Button okButton = CreateButton(card, "Hint Reward OK", new Vector2(0f, 92f));
            okButton.gameObject.name = "Hint Reward OK Button";
            TextMeshProUGUI okLabel = GetText(okButton.transform, "Label");
            if (okLabel != null)
                okLabel.text = "OK";
        }

        private void BuildPaymentFailedPanel(Transform canvasRoot)
        {
            RectTransform overlay = CreateRect("Payment Failed Panel", canvasRoot);
            StretchRect(overlay);
            EnsureImage(overlay.gameObject, new Color(0.04f, 0.05f, 0.08f, 0.8f));
            overlay.gameObject.SetActive(false);

            RectTransform card = CreateRect("Payment Failed Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(720f, 520f);
            card.anchoredPosition = Vector2.zero;
            EnsureImage(card.gameObject, Color.white, Image.Type.Sliced);

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(52, 52, 52, 52);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateStandaloneLabel(card, "Payment Failed Title", "Payment Failed", 44f, new Color(0.16f, 0.18f, 0.3f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;
            CreateStandaloneLabel(card, "Payment Failed Body", "Payment failed. Please try again.", 28f, new Color(0.28f, 0.31f, 0.48f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;

            Button retryButton = CreateButton(card, "Payment Failed Retry", new Vector2(0f, 92f));
            retryButton.gameObject.name = "Payment Failed Retry Button";
            TextMeshProUGUI retryLabel = GetText(retryButton.transform, "Label");
            if (retryLabel != null)
                retryLabel.text = "Retry";
        }

        private void BuildMenuLeaderboardPanel(Transform canvasRoot)
        {
            RectTransform overlay = CreateRect("Menu Leaderboard Panel", canvasRoot);
            StretchRect(overlay);
            EnsureImage(overlay.gameObject, new Color(0.04f, 0.05f, 0.08f, 0.8f));
            overlay.gameObject.SetActive(false);

            RectTransform card = CreateRect("Menu Leaderboard Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(860f, 1120f);
            card.anchoredPosition = Vector2.zero;
            EnsureImage(card.gameObject, cardColor, Image.Type.Sliced);

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(44, 44, 44, 44);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateStandaloneLabel(card, "Menu Leaderboard Title", "Weekly Challenge Leaderboard", 42f, textPrimaryColor, TextAlignmentOptions.Center);
            CreateStandaloneLabel(card, "Menu Leaderboard Best Text", "Your Best: Not set yet", 26f, textSecondaryColor, TextAlignmentOptions.Center);

            RectTransform scrollRoot = CreateRect("Menu Leaderboard Scroll View", card);
            LayoutElement scrollLayout = scrollRoot.gameObject.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1f;
            scrollLayout.preferredHeight = 720f;
            Image scrollBackground = EnsureImage(scrollRoot.gameObject, new Color(1f, 1f, 1f, 0.02f));

            ScrollRect scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 24f;

            RectTransform viewport = CreateRect("Menu Leaderboard Viewport", scrollRoot);
            StretchRect(viewport);
            EnsureImage(viewport.gameObject, new Color(1f, 1f, 1f, 0.01f));
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform listRoot = CreateRect("Menu Leaderboard List", viewport);
            listRoot.anchorMin = new Vector2(0f, 1f);
            listRoot.anchorMax = new Vector2(1f, 1f);
            listRoot.pivot = new Vector2(0.5f, 1f);
            listRoot.anchoredPosition = Vector2.zero;
            listRoot.sizeDelta = Vector2.zero;

            VerticalLayoutGroup listLayout = listRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            listLayout.padding = new RectOffset(0, 0, 0, 0);
            listLayout.spacing = 12f;
            listLayout.childAlignment = TextAnchor.UpperCenter;
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = false;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;
            ContentSizeFitter listFitter = listRoot.gameObject.AddComponent<ContentSizeFitter>();
            listFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport;
            scrollRect.content = listRoot;

            for (int i = 0; i < 25; i++)
                CreateMenuLeaderboardEntry(listRoot, i);

            RectTransform bottomSpacer = CreateRect("Menu Leaderboard Bottom Spacer", card);
            LayoutElement spacerLayout = bottomSpacer.gameObject.GetComponent<LayoutElement>() ?? bottomSpacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleHeight = 1f;
            spacerLayout.minHeight = 24f;
            spacerLayout.preferredHeight = 24f;

            Button closeButton = CreateButton(card, "Menu Leaderboard Close", new Vector2(0f, 80f));
            closeButton.gameObject.name = "Menu Leaderboard Close Button";
            TextMeshProUGUI closeLabel = GetText(closeButton.transform, "Label");
            if (closeLabel != null)
                closeLabel.text = "Close";
        }

        private ChallengeLeaderboardEntryView CreateMenuLeaderboardEntry(Transform parent, int index)
        {
            RectTransform root = CreateRect($"Menu Leaderboard Entry {index + 1}", parent);
            LayoutElement rootLayout = root.gameObject.AddComponent<LayoutElement>();
            rootLayout.preferredHeight = 120f;
            rootLayout.flexibleWidth = 1f;

            Image rootImage = EnsureImage(root.gameObject, new Color(0.17f, 0.18f, 0.29f, 0.94f), Image.Type.Sliced);

            HorizontalLayoutGroup layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 18, 18);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI rankText = CreateStandaloneLabel(root, "Rank Text", $"{index + 1}", 34f, textPrimaryColor, TextAlignmentOptions.Center);
            rankText.rectTransform.sizeDelta = new Vector2(90f, 84f);
            TextMeshProUGUI nameText = CreateStandaloneLabel(root, "Name Text", $"Player {index + 1}", 30f, textPrimaryColor, TextAlignmentOptions.Left);
            nameText.rectTransform.sizeDelta = new Vector2(320f, 84f);
            TextMeshProUGUI timeText = CreateStandaloneLabel(root, "Time Text", "00:00.000", 30f, textPrimaryColor, TextAlignmentOptions.Right);
            timeText.rectTransform.sizeDelta = new Vector2(220f, 84f);

            GameObject firstBadge = CreateMarker(root, "First Place Badge", "1", 28f, Color.white);
            GameObject secondBadge = CreateMarker(root, "Second Place Badge", "2", 28f, Color.white);
            GameObject thirdBadge = CreateMarker(root, "Third Place Badge", "3", 28f, Color.white);
            firstBadge.SetActive(false);
            secondBadge.SetActive(false);
            thirdBadge.SetActive(false);

            ChallengeLeaderboardEntryView view = root.gameObject.AddComponent<ChallengeLeaderboardEntryView>();

#if UNITY_EDITOR
            AssignLeaderboardEntryView(view, root.gameObject, rootImage, rankText, nameText, timeText, firstBadge, secondBadge, thirdBadge);
#endif
            return view;
        }

        private void CreateShopOfferCard(Transform parent, string prefix, string iconLabel, string amountText, string priceText)
        {
            RectTransform card = CreateRect($"{prefix} Offer Card", parent);
            card.sizeDelta = new Vector2(0f, 150f);

            LayoutElement layout = card.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 150f;
            layout.flexibleWidth = 1f;

            EnsureImage(card.gameObject, new Color(1f, 0.96f, 0.8f, 1f), Image.Type.Sliced);

            HorizontalLayoutGroup rowLayout = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(28, 28, 20, 20);
            rowLayout.spacing = 20f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            RectTransform iconRect = CreateRect($"{prefix} Icon", card);
            iconRect.sizeDelta = new Vector2(94f, 94f);
            EnsureImage(iconRect.gameObject, new Color(0.36f, 0.86f, 0.92f, 1f), Image.Type.Sliced);
            CreateNamedLabel(iconRect, "Icon Label", iconLabel, 28f, Color.white);

            RectTransform amountRect = CreateRect($"{prefix} Amount Text", card);
            amountRect.sizeDelta = new Vector2(220f, 70f);
            CreateRectLabel(amountRect, amountText, 34f, new Color(0.35f, 0.28f, 0.16f, 1f), TextAlignmentOptions.Center);

            RectTransform spacer = CreateRect($"{prefix} Spacer", card);
            LayoutElement spacerLayout = spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1f;

            RectTransform buyRect = CreateRect($"{prefix} Buy Button", card);
            buyRect.sizeDelta = new Vector2(210f, 74f);
            Image buyImage = EnsureImage(buyRect.gameObject, new Color(0.95f, 0.23f, 0.61f, 1f), Image.Type.Sliced);
            Button buyButton = buyRect.gameObject.AddComponent<Button>();
            buyButton.targetGraphic = buyImage;
            CreateNamedLabel(buyRect, $"{prefix} Price Text", priceText, 28f, Color.white);
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

            Image cardImage = EnsureImage(card.gameObject, cardColor, Image.Type.Sliced);

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
            CreateButton(card, "Play Challenge", new Vector2(0f, 88f));
        }

        private void BuildSettingsPanel(Transform settingsPanel)
        {
            RectTransform content = CreateRect("Settings Content", settingsPanel);
            StretchRect(content);

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(42, 42, 42, 42);
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateStandaloneLabel(content, "Settings Title", "Settings", 60f, accentColor, TextAlignmentOptions.Center);
            CreateStandaloneLabel(content, "Username Label", "Username", 24f, textSecondaryColor, TextAlignmentOptions.Center);
            CreateInputField(content, "Username Input Field", "Enter your name");
            CreateStandaloneLabel(content, "Preferences Heading", "Preferences", 22f, textSecondaryColor, TextAlignmentOptions.Center);

            RectTransform preferencesCard = CreateCard(content, "Preferences Card", 420f);
            CreateToggleRow(preferencesCard, "Vibrations");
            CreateToggleRow(preferencesCard, "Sounds");
            CreateToggleRow(preferencesCard, "Dark Mode");

            CreateStandaloneLabel(content, "More Heading", "More", 22f, textSecondaryColor, TextAlignmentOptions.Center);
            RectTransform linksCard = CreateCard(content, "Links Card", 560f);
            CreateLinkRow(linksCard, "Privacy");
            CreateLinkRow(linksCard, "Terms & Conditions");
            CreateLinkRow(linksCard, "FAQ");
            CreateLinkRow(linksCard, "Join Telegram");
            CreateLinkRow(linksCard, "Twitter");
        }

        private void EnsureSettingsNameEditUi(Transform settingsPanel, Transform canvasRoot)
        {
            TMP_InputField userNameInputField = GetInputField(settingsPanel, "Username Input Field");
            if (userNameInputField != null)
            {
                userNameInputField.readOnly = true;
                EnsureUsernameEditButton(userNameInputField.transform as RectTransform);
            }

            Transform existingPanel = FindDeepChild(canvasRoot, "Username Edit Panel");
            if (existingPanel == null)
                BuildUsernameEditPanel(canvasRoot);
        }

        private void EnsureUsernameEditButton(RectTransform inputRoot)
        {
            if (inputRoot == null)
                return;

            RectTransform buttonRect = GetRect(inputRoot, "Username Edit Button");
            if (buttonRect == null)
            {
                buttonRect = CreateRect("Username Edit Button", inputRoot);
                StretchRect(buttonRect);
                Image buttonImage = EnsureImage(buttonRect.gameObject, new Color(1f, 1f, 1f, 0.01f));
                Button button = buttonRect.gameObject.AddComponent<Button>();
                button.targetGraphic = buttonImage;
            }

            RectTransform iconRect = GetRect(inputRoot, "Username Edit Pencil");
            if (iconRect == null)
            {
                iconRect = CreateRect("Username Edit Pencil", inputRoot);
                iconRect.anchorMin = new Vector2(1f, 0.5f);
                iconRect.anchorMax = new Vector2(1f, 0.5f);
                iconRect.pivot = new Vector2(1f, 0.5f);
                iconRect.sizeDelta = new Vector2(40f, 40f);
                iconRect.anchoredPosition = new Vector2(-16f, 0f);
                CreateRectLabel(iconRect, "✏", 28f, textSecondaryColor, TextAlignmentOptions.Center);
            }
        }

        private void BuildUsernameEditPanel(Transform canvasRoot)
        {
            RectTransform overlay = CreateRect("Username Edit Panel", canvasRoot);
            StretchRect(overlay);
            EnsureImage(overlay.gameObject, new Color(0.04f, 0.05f, 0.08f, 0.8f));
            overlay.gameObject.SetActive(false);

            RectTransform card = CreateRect("Username Edit Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(760f, 520f);
            card.anchoredPosition = Vector2.zero;
            EnsureImage(card.gameObject, Color.white, Image.Type.Sliced);

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(52, 52, 52, 52);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateStandaloneLabel(card, "Username Edit Title", "Change Your Name", 42f, new Color(0.16f, 0.18f, 0.3f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;
            CreateStandaloneLabel(card, "Username Edit Status", "Enter a new name and save it to your MiniPay profile.", 24f, new Color(0.35f, 0.38f, 0.53f, 1f), TextAlignmentOptions.Center).textWrappingMode = TextWrappingModes.Normal;
            CreateInputField(card, "Username Edit Input Field", "Enter your name");

            RectTransform buttonRow = CreateRect("Username Edit Button Row", card);
            LayoutElement rowLayout = buttonRow.gameObject.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 92f;
            HorizontalLayoutGroup row = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 24f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = false;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            Button cancelButton = CreateButton(buttonRow, "Cancel", new Vector2(220f, 84f));
            cancelButton.gameObject.name = "Username Edit Cancel Button";
            TextMeshProUGUI cancelLabel = GetText(cancelButton.transform, "Label");
            if (cancelLabel != null)
                cancelLabel.text = "Cancel";

            Button saveButton = CreateButton(buttonRow, "Save", new Vector2(220f, 84f));
            saveButton.gameObject.name = "Username Edit Save Button";
            TextMeshProUGUI saveLabel = GetText(saveButton.transform, "Label");
            if (saveLabel != null)
                saveLabel.text = "Save";
        }

        private ChallengeMenuReferences CollectChallengeReferences(Transform challengePanel, Transform canvasRoot)
        {
            ChallengeMenuReferences refs = new();
            Transform menuPanel = challengePanel.Find("Challenge Menu Panel") ?? FindDeepChild(challengePanel, "Challenge Menu Panel");
            Transform card = menuPanel != null ? FindDeepChild(menuPanel, "Challenge Card") : null;

            refs.titleText = GetText(card, "Challenge Title");
            refs.patternNameText = GetText(card, "Pattern Name");
            refs.cycleTimerText = GetText(card, "Cycle Timer");
            refs.chanceText = GetText(card, "Challenge Text");
            refs.nextChanceTimerText = GetText(card, "Next Chance Timer");
            refs.statusText = GetText(card, "Status Text");
            refs.challengePlayButton = GetButton(card, "Play Challenge Button");
            return refs;
        }

        private SettingsMenuReferences CollectSettingsReferences(Transform settingsPanel)
        {
            SettingsMenuReferences refs = new();
            if (settingsPanel == null)
                return refs;

            refs.userNameInputField = settingsPanel.GetComponentInChildren<TMP_InputField>(true);
            refs.userNameEditButton = GetButton(settingsPanel, "Username Edit Button");
            refs.vibrationToggleButton = FindButtonByKeywords(settingsPanel, "vibration", "vibrations");
            refs.vibrationToggleBackground = FindButtonBackground(refs.vibrationToggleButton);
            refs.vibrationToggleKnob = FindToggleKnob(refs.vibrationToggleButton);
            refs.soundToggleButton = FindButtonByKeywords(settingsPanel, "sound", "sounds");
            refs.soundToggleBackground = FindButtonBackground(refs.soundToggleButton);
            refs.soundToggleKnob = FindToggleKnob(refs.soundToggleButton);
            refs.darkModeToggleButton = FindButtonByKeywords(settingsPanel, "dark");
            refs.darkModeToggleBackground = FindButtonBackground(refs.darkModeToggleButton);
            refs.darkModeToggleKnob = FindToggleKnob(refs.darkModeToggleButton);
            refs.privacyButton = FindButtonByKeywords(settingsPanel, "privacy");
            refs.termsButton = FindButtonByKeywords(settingsPanel, "terms");
            refs.faqButton = FindButtonByKeywords(settingsPanel, "faq");
            refs.telegramButton = FindButtonByKeywords(settingsPanel, "telegram");
            refs.twitterButton = FindButtonByKeywords(settingsPanel, "twitter");
            Transform canvasRoot = settingsPanel.root;
            refs.userNameEditPanel = FindDeepChild(canvasRoot, "Username Edit Panel")?.gameObject;
            refs.userNameEditInputField = GetInputField(canvasRoot, "Username Edit Input Field");
            refs.userNameEditSaveButton = GetButton(canvasRoot, "Username Edit Save Button");
            refs.userNameEditCancelButton = GetButton(canvasRoot, "Username Edit Cancel Button");
            refs.userNameEditTitleText = GetText(canvasRoot, "Username Edit Title");
            refs.userNameEditStatusText = GetText(canvasRoot, "Username Edit Status");

            List<Image> surfaceImages = new();
            AddIfNotNull(surfaceImages, settingsPanel.GetComponent<Image>());
            AddIfNotNull(surfaceImages, refs.userNameInputField != null ? refs.userNameInputField.GetComponent<Image>() : null);
            refs.themeSurfaceImages = surfaceImages.ToArray();

            List<Image> accentImages = new();
            AddIfNotNull(accentImages, refs.privacyButton != null ? FindButtonBackground(refs.privacyButton) : null);
            AddIfNotNull(accentImages, refs.termsButton != null ? FindButtonBackground(refs.termsButton) : null);
            AddIfNotNull(accentImages, refs.faqButton != null ? FindButtonBackground(refs.faqButton) : null);
            AddIfNotNull(accentImages, refs.telegramButton != null ? FindButtonBackground(refs.telegramButton) : null);
            AddIfNotNull(accentImages, refs.twitterButton != null ? FindButtonBackground(refs.twitterButton) : null);
            refs.themeAccentImages = accentImages.ToArray();

            refs.themePrimaryTexts = settingsPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            refs.themeSecondaryTexts = new TextMeshProUGUI[0];

            return refs;
        }

        private PurchaseGateReferences CollectPurchaseGateReferences(Transform canvasRoot)
        {
            PurchaseGateReferences refs = new();
            Transform panel = FindDeepChild(canvasRoot, "Purchase Gate Panel");
            Transform card = panel != null ? FindDeepChild(panel, "Purchase Gate Card") : null;
            refs.panel = panel != null ? panel.gameObject : null;
            refs.payButton = GetButton(card, "Purchase Gate Pay Button");
            refs.titleText = GetText(card, "Purchase Gate Title");
            refs.bodyText = GetText(card, "Purchase Gate Body");
            refs.priceText = GetText(card, "Purchase Gate Price Text");
            refs.statusText = GetText(card, "Purchase Gate Status Text");
            return refs;
        }

        private static bool HasRequiredChallengeReferences(ChallengeMenuReferences refs)
        {
            return refs != null &&
                   refs.titleText != null &&
                   refs.patternNameText != null &&
                   refs.cycleTimerText != null &&
                   refs.chanceText != null &&
                   refs.nextChanceTimerText != null &&
                   refs.statusText != null &&
                   refs.challengePlayButton != null;
        }

        private static bool HasRequiredSettingsReferences(SettingsMenuReferences refs)
        {
            return refs != null &&
                   refs.userNameInputField != null &&
                   refs.userNameEditButton != null &&
                   refs.userNameEditPanel != null &&
                   refs.userNameEditInputField != null &&
                   refs.userNameEditSaveButton != null &&
                   refs.userNameEditCancelButton != null &&
                   refs.userNameEditTitleText != null &&
                   refs.userNameEditStatusText != null &&
                   refs.vibrationToggleButton != null &&
                   refs.vibrationToggleBackground != null &&
                   refs.vibrationToggleKnob != null &&
                   refs.soundToggleButton != null &&
                   refs.soundToggleBackground != null &&
                   refs.soundToggleKnob != null &&
                   refs.darkModeToggleButton != null &&
                   refs.darkModeToggleBackground != null &&
                   refs.darkModeToggleKnob != null &&
                   refs.privacyButton != null &&
                   refs.termsButton != null &&
                   refs.faqButton != null &&
                   refs.telegramButton != null &&
                   refs.twitterButton != null;
        }

        private static bool HasRequiredPurchaseGateReferences(PurchaseGateReferences refs)
        {
            return refs != null &&
                   refs.panel != null &&
                   refs.payButton != null &&
                   refs.titleText != null &&
                   refs.bodyText != null &&
                   refs.priceText != null &&
                   refs.statusText != null;
        }

#if UNITY_EDITOR
        private static void AssignChallengeReferences(MenuSceneController controller, ChallengeMenuReferences refs)
        {
            UnityEditor.SerializedObject so = new(controller);
            SerializedReferenceUtility.Assign(so, "challengeTitleText", refs.titleText);
            SerializedReferenceUtility.Assign(so, "challengePatternText", refs.patternNameText);
            SerializedReferenceUtility.Assign(so, "challengeCycleTimerText", refs.cycleTimerText);
            SerializedReferenceUtility.Assign(so, "challengeChanceText", refs.chanceText);
            SerializedReferenceUtility.Assign(so, "challengeNextChanceTimerText", refs.nextChanceTimerText);
            SerializedReferenceUtility.Assign(so, "challengeStatusText", refs.statusText);
            SerializedReferenceUtility.Assign(so, "challengePlayButton", refs.challengePlayButton);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignSettingsReferences(MenuSceneController controller, SettingsMenuReferences refs)
        {
            UnityEditor.SerializedObject so = new(controller);
            SerializedReferenceUtility.Assign(so, "userNameInputField", refs.userNameInputField);
            SerializedReferenceUtility.Assign(so, "userNameEditButton", refs.userNameEditButton);
            SerializedReferenceUtility.Assign(so, "userNameEditPanel", refs.userNameEditPanel);
            SerializedReferenceUtility.Assign(so, "userNameEditInputField", refs.userNameEditInputField);
            SerializedReferenceUtility.Assign(so, "userNameEditSaveButton", refs.userNameEditSaveButton);
            SerializedReferenceUtility.Assign(so, "userNameEditCancelButton", refs.userNameEditCancelButton);
            SerializedReferenceUtility.Assign(so, "userNameEditTitleText", refs.userNameEditTitleText);
            SerializedReferenceUtility.Assign(so, "userNameEditStatusText", refs.userNameEditStatusText);
            SerializedReferenceUtility.Assign(so, "vibrationToggleButton", refs.vibrationToggleButton);
            SerializedReferenceUtility.Assign(so, "vibrationToggleBackground", refs.vibrationToggleBackground);
            SerializedReferenceUtility.Assign(so, "vibrationToggleKnob", refs.vibrationToggleKnob);
            SerializedReferenceUtility.Assign(so, "soundToggleButton", refs.soundToggleButton);
            SerializedReferenceUtility.Assign(so, "soundToggleBackground", refs.soundToggleBackground);
            SerializedReferenceUtility.Assign(so, "soundToggleKnob", refs.soundToggleKnob);
            SerializedReferenceUtility.Assign(so, "darkModeToggleButton", refs.darkModeToggleButton);
            SerializedReferenceUtility.Assign(so, "darkModeToggleBackground", refs.darkModeToggleBackground);
            SerializedReferenceUtility.Assign(so, "darkModeToggleKnob", refs.darkModeToggleKnob);
            SerializedReferenceUtility.Assign(so, "privacyButton", refs.privacyButton);
            SerializedReferenceUtility.Assign(so, "termsButton", refs.termsButton);
            SerializedReferenceUtility.Assign(so, "faqButton", refs.faqButton);
            SerializedReferenceUtility.Assign(so, "telegramButton", refs.telegramButton);
            SerializedReferenceUtility.Assign(so, "twitterButton", refs.twitterButton);
            SerializedReferenceUtility.AssignArray(so, "themeSurfaceImages", refs.themeSurfaceImages);
            SerializedReferenceUtility.AssignArray(so, "themeAccentImages", refs.themeAccentImages);
            SerializedReferenceUtility.AssignArray(so, "themePrimaryTexts", refs.themePrimaryTexts);
            SerializedReferenceUtility.AssignArray(so, "themeSecondaryTexts", refs.themeSecondaryTexts);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignPurchaseGateReferences(MenuSceneController controller, PurchaseGateReferences refs)
        {
            UnityEditor.SerializedObject so = new(controller);
            SerializedReferenceUtility.Assign(so, "purchaseGatePanel", refs.panel);
            SerializedReferenceUtility.Assign(so, "purchaseGatePayButton", refs.payButton);
            SerializedReferenceUtility.Assign(so, "purchaseGateTitleText", refs.titleText);
            SerializedReferenceUtility.Assign(so, "purchaseGateBodyText", refs.bodyText);
            SerializedReferenceUtility.Assign(so, "purchaseGatePriceText", refs.priceText);
            SerializedReferenceUtility.Assign(so, "purchaseGateStatusText", refs.statusText);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
#endif

        private RectTransform CreateCard(Transform parent, string name, float preferredHeight)
        {
            RectTransform card = CreateRect(name, parent);
            LayoutElement layout = card.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = preferredHeight;
            layout.flexibleWidth = 1f;

            Image image = EnsureImage(card.gameObject, settingsCardColor, Image.Type.Sliced);

            VerticalLayoutGroup verticalLayout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.padding = new RectOffset(24, 24, 24, 24);
            verticalLayout.spacing = 18f;
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            return card;
        }

        private void CreateToggleRow(Transform parent, string label)
        {
            RectTransform row = CreateRect($"{label} Row", parent);
            LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 92f;

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 0, 0);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            RectTransform iconRect = CreateRect($"{label} Icon", row);
            LayoutElement iconLayout = iconRect.gameObject.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 48f;
            iconLayout.preferredHeight = 48f;
            Image iconImage = EnsureImage(iconRect.gameObject, Color.white, Image.Type.Sliced);

            RectTransform labelRect = CreateRect($"{label} Label", row);
            LayoutElement labelLayout = labelRect.gameObject.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1f;
            labelLayout.preferredHeight = 48f;
            CreateRectLabel(labelRect, label, 34f, textPrimaryColor, TextAlignmentOptions.Left);

            RectTransform toggleRect = CreateRect($"{label} Toggle Button", row);
            LayoutElement toggleLayout = toggleRect.gameObject.AddComponent<LayoutElement>();
            toggleLayout.preferredWidth = 126f;
            toggleLayout.preferredHeight = 62f;
            Image toggleImage = EnsureImage(toggleRect.gameObject, settingsAccentColor, Image.Type.Sliced);
            Button toggleButton = toggleRect.gameObject.AddComponent<Button>();
            toggleButton.targetGraphic = toggleImage;

            RectTransform knob = CreateRect($"{label} Toggle Knob", toggleRect);
            knob.anchorMin = new Vector2(0.5f, 0.5f);
            knob.anchorMax = new Vector2(0.5f, 0.5f);
            knob.pivot = new Vector2(0.5f, 0.5f);
            knob.sizeDelta = new Vector2(52f, 52f);
            knob.anchoredPosition = new Vector2(26f, 0f);
            Image knobImage = EnsureImage(knob.gameObject, toggleKnobColor, Image.Type.Sliced);
        }

        private void CreateLinkRow(Transform parent, string label)
        {
            RectTransform buttonRect = CreateRect($"{label} Button", parent);
            LayoutElement layout = buttonRect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 76f;
            layout.flexibleWidth = 1f;

            Image image = EnsureImage(buttonRect.gameObject, settingsAccentColor, Image.Type.Sliced);

            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            HorizontalLayoutGroup rowLayout = buttonRect.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(20, 20, 0, 0);
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            CreateText(buttonRect, $"{label} Label", label, 28f, textPrimaryColor, TextAlignmentOptions.Center);
        }

        private TMP_InputField CreateInputField(Transform parent, string name, string placeholderText)
        {
            RectTransform root = CreateRect(name, parent);
            LayoutElement layout = root.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 78f;
            layout.preferredWidth = 0f;
            layout.flexibleWidth = 1f;

            Image background = EnsureImage(root.gameObject, settingsCardColor, Image.Type.Sliced);

            TMP_InputField inputField = root.gameObject.AddComponent<TMP_InputField>();
            inputField.targetGraphic = background;
            inputField.lineType = TMP_InputField.LineType.SingleLine;

            RectTransform textViewport = CreateRect("Text Viewport", root);
            textViewport.anchorMin = Vector2.zero;
            textViewport.anchorMax = Vector2.one;
            textViewport.offsetMin = new Vector2(24f, 14f);
            textViewport.offsetMax = new Vector2(-24f, -14f);
            textViewport.gameObject.AddComponent<RectMask2D>();

            TextMeshProUGUI textComponent = CreateText(textViewport, "Username Text", string.Empty, 28f, textPrimaryColor, TextAlignmentOptions.Left);
            TextMeshProUGUI placeholder = CreateText(textViewport, "Username Placeholder", placeholderText, 28f, textSecondaryColor, TextAlignmentOptions.Left);
            placeholder.fontStyle = FontStyles.Italic;

            inputField.textViewport = textViewport;
            inputField.textComponent = textComponent;
            inputField.placeholder = placeholder;
            return inputField;
        }

        private Button CreateButton(Transform parent, string label, Vector2 size)
        {
            RectTransform rect = CreateRect($"{label} Button", parent);
            rect.sizeDelta = size;
            Image image = EnsureImage(rect.gameObject, accentColor, Image.Type.Sliced);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CreateNamedLabel(rect, "Label", label, 34f, textPrimaryColor);
            return button;
        }

        private TextMeshProUGUI CreateStandaloneLabel(Transform parent, string objectName, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            RectTransform rect = CreateRect(objectName, parent);
            LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = fontSize + 18f;
            layout.flexibleWidth = 1f;
            return CreateRectLabel(rect, text, fontSize, color, alignment);
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
            ApplyPreferredFont(label);
            ContentSizeFitter fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return label;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string objectName, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            RectTransform rect = CreateRect(objectName, parent);
            StretchRect(rect);
            return CreateRectLabel(rect, text, fontSize, color, alignment);
        }

        private static TextMeshProUGUI CreateRectLabel(RectTransform rect, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
            ApplyPreferredFont(label);
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
            ApplyPreferredFont(label);
            return rect.gameObject;
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
#if UNITY_EDITOR
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

        private static Image GetImage(Transform parent, string name)
        {
            Transform child = FindDeepChild(parent, name);
            return child != null ? child.GetComponent<Image>() : null;
        }

        private static RectTransform GetRect(Transform parent, string name)
        {
            Transform child = FindDeepChild(parent, name);
            return child as RectTransform;
        }

        private static TMP_InputField GetInputField(Transform parent, string name)
        {
            Transform child = FindDeepChild(parent, name);
            return child != null ? child.GetComponent<TMP_InputField>() : null;
        }

        private static Button FindButtonByKeywords(Transform root, params string[] keywords)
        {
            if (root == null || keywords == null || keywords.Length == 0)
                return null;

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (ButtonMatches(buttons[i], keywords))
                    return buttons[i];
            }

            return null;
        }

        private static bool ButtonMatches(Button button, params string[] keywords)
        {
            if (button == null)
                return false;

            string searchText = BuildSearchText(button);
            for (int i = 0; i < keywords.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(keywords[i]) && searchText.Contains(keywords[i].ToLowerInvariant()))
                    return true;
            }

            return false;
        }

        private static string BuildSearchText(Component component)
        {
            if (component == null)
                return string.Empty;

            string searchText = component.name.ToLowerInvariant();
            TextMeshProUGUI[] labels = component.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] != null && !string.IsNullOrWhiteSpace(labels[i].text))
                    searchText += " " + labels[i].text.ToLowerInvariant();
            }

            return searchText;
        }

        private static Image FindButtonBackground(Button button)
        {
            if (button == null)
                return null;

            if (button.targetGraphic is Image targetImage)
                return targetImage;

            return button.GetComponent<Image>();
        }

        private static RectTransform FindToggleKnob(Button button)
        {
            if (button == null)
                return null;

            Image[] images = button.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].transform != button.transform)
                    return images[i].rectTransform;
            }

            return null;
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
            runtimeSprite.name = "MenuRuntimeSprite";
            return runtimeSprite;
        }

        private static void AddIfNotNull<T>(List<T> list, T value) where T : Object
        {
            if (value != null)
                list.Add(value);
        }
    }

}
