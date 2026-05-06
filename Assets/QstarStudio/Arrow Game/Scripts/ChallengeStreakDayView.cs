using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowGame
{
    public class ChallengeStreakDayView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI dayLabel;
        [SerializeField] private TextMeshProUGUI stateLabel;
        [SerializeField] private GameObject playedMarker;
        [SerializeField] private GameObject currentMarker;
        [SerializeField] private Color playedColor = new(0.34f, 0.7f, 0.43f, 0.95f);
        [SerializeField] private Color currentColor = new(0.35f, 0.43f, 0.98f, 0.95f);
        [SerializeField] private Color missedColor = new(0.43f, 0.2f, 0.23f, 0.92f);
        [SerializeField] private Color pendingColor = new(0.18f, 0.2f, 0.31f, 0.95f);

        public void Bind(int dayNumber, bool isPlayed, bool isCurrentDay, bool isMissed)
        {
            if (dayLabel != null)
                dayLabel.text = $"Day {dayNumber}";

            if (stateLabel != null)
            {
                if (isPlayed)
                    stateLabel.text = "Played";
                else if (isCurrentDay)
                    stateLabel.text = "Today";
                else if (isMissed)
                    stateLabel.text = "Missed";
                else
                    stateLabel.text = "Upcoming";
            }

            if (playedMarker != null)
                playedMarker.SetActive(isPlayed);
            if (currentMarker != null)
                currentMarker.SetActive(isCurrentDay);
            if (background != null)
                background.color = ResolveBackgroundColor(isPlayed, isCurrentDay, isMissed);
        }

        private Color ResolveBackgroundColor(bool isPlayed, bool isCurrentDay, bool isMissed)
        {
            if (isPlayed)
                return playedColor;
            if (isCurrentDay)
                return currentColor;
            if (isMissed)
                return missedColor;

            return pendingColor;
        }
    }
}
