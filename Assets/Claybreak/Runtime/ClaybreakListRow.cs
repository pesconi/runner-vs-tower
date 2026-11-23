using JungleDegree.StudioKit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Claybreak
{
    /// <summary>
    /// One row of a menu list (shop item, world): a light card with a title, a detail, an optional color swatch and one
    /// button (price, action or a seal when owned). Built by the scene builder, one per catalog entry, so the lists grow
    /// with the data. RT-6t.17: TextMeshPro labels; the button's slab follows its color.
    /// </summary>
    public sealed class ClaybreakListRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text detail;
        [SerializeField] private Image swatch;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text buttonLabel;

        public bool HasParts => title != null && detail != null && swatch != null && button != null && buttonLabel != null;
        public Button Button => button;
        public TMP_Text ButtonLabel => buttonLabel;

        public void Show(string heading, string line, string action, bool enabled, Color buttonColor)
        {
            gameObject.SetActive(true);
            title.text = heading;
            detail.text = line;
            buttonLabel.text = action;
            button.interactable = enabled;
            button.image.color = buttonColor;
            Shadow slab = button.image.GetComponent<Shadow>();
            if (slab != null)
            {
                slab.effectColor = ClayUi.SlabColor(buttonColor);
            }

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
