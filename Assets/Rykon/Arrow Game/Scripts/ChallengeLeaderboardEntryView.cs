using ArrowGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowGame
{
    public class ChallengeLeaderboardEntryView : MonoBehaviour
    {
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private GameObject firstPlaceBadge;
        [SerializeField] private GameObject secondPlaceBadge;
        [SerializeField] private GameObject thirdPlaceBadge;
        [SerializeField] private Color defaultBackgroundColor = new(0.14f, 0.16f, 0.25f, 0.92f);
        [SerializeField] private Color firstPlaceBackgroundColor = new(0.95f, 0.76f, 0.23f, 0.95f);
        [SerializeField] private Color secondPlaceBackgroundColor = new(0.73f, 0.79f, 0.88f, 0.92f);
        [SerializeField] private Color thirdPlaceBackgroundColor = new(0.88f, 0.58f, 0.35f, 0.92f);
        [SerializeField] private Color playerBackgroundColor = new(0.35f, 0.43f, 0.98f, 0.9f);
        [SerializeField] private Color highlightedTextColor = Color.white;
        [SerializeField] private Color defaultTextColor = new(0.92f, 0.94f, 1f, 1f);

        public void Bind(ChallengeLeaderboardEntryData entryData)
        {
            if (entryData == null)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            if (rankText != null)
                rankText.text = entryData.rank.ToString();
            if (nameText != null)
                nameText.text = entryData.playerName;
            if (timeText != null)
                timeText.text = FormatTime(entryData.completionSeconds);

            if (background != null)
                background.color = GetBackgroundColor(entryData);

            ThemeManager.ThemePalette palette = ThemeManager.CurrentPalette;
            Color textColor = entryData.rank <= 3 || entryData.isPlayer
                ? palette.LeaderboardHighlightedTextColor
                : palette.LeaderboardDefaultTextColor;
            SetTextColor(rankText, textColor);
            SetTextColor(nameText, textColor);
            SetTextColor(timeText, textColor);

            if (firstPlaceBadge != null)
                firstPlaceBadge.SetActive(entryData.rank == 1);
            if (secondPlaceBadge != null)
                secondPlaceBadge.SetActive(entryData.rank == 2);
            if (thirdPlaceBadge != null)
                thirdPlaceBadge.SetActive(entryData.rank == 3);
        }

        private void SetVisible(bool isVisible)
        {
            if (contentRoot != null)
                contentRoot.SetActive(isVisible);
            else
                gameObject.SetActive(isVisible);
        }

        private Color GetBackgroundColor(ChallengeLeaderboardEntryData entryData)
        {
            ThemeManager.ThemePalette palette = ThemeManager.CurrentPalette;

            if (entryData.isPlayer)
                return palette.LeaderboardPlayerBackgroundColor;
            if (entryData.rank == 1)
                return palette.LeaderboardFirstPlaceBackgroundColor;
            if (entryData.rank == 2)
                return palette.LeaderboardSecondPlaceBackgroundColor;
            if (entryData.rank == 3)
                return palette.LeaderboardThirdPlaceBackgroundColor;

            return palette.LeaderboardDefaultBackgroundColor;
        }

        private static void SetTextColor(TextMeshProUGUI textComponent, Color color)
        {
            if (textComponent != null)
                textComponent.color = color;
        }

        private static string FormatTime(float seconds)
        {
            int totalMilliseconds = Mathf.RoundToInt(seconds * 1000f);
            int minutes = totalMilliseconds / 60000;
            int remainingMilliseconds = totalMilliseconds % 60000;
            int wholeSeconds = remainingMilliseconds / 1000;
            int milliseconds = remainingMilliseconds % 1000;
            return $"{minutes:00}:{wholeSeconds:00}.{milliseconds:000}";
        }
    }
}
