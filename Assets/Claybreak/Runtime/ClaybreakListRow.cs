using UnityEngine;
using UnityEngine.UI;

namespace Claybreak
{
    /// <summary>
    /// One row of a menu list (shop item, world): title, detail, an optional color swatch and one button.
    /// Built by the scene builder, one per catalog entry, so the lists grow with the data.
    /// </summary>
    public sealed class ClaybreakListRow : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text detail;
        [SerializeField] private Image swatch;
        [SerializeField] private Button button;
        [SerializeField] private Text buttonLabel;

        public bool HasParts => title != null && detail != null && swatch != null && button != null && buttonLabel != null;
        public Button Button => button;

        public void Show(string heading, string line, string action, bool enabled, Color buttonColor)
        {
            gameObject.SetActive(true);
            title.text = heading;
            detail.text = line;
            buttonLabel.text = action;
            button.interactable = enabled;
            button.image.color = buttonColor;
            swatch.gameObject.SetActive(false);
        }

        public void ShowSwatch(Color color)
        {
            swatch.color = color;
            swatch.gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
