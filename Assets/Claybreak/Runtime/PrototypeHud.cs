using System;
using UnityEngine;
using UnityEngine.UI;

namespace Claybreak
{
    /// <summary>
    /// Prototype HUD (no new menu, RT-2): army count, tower health, one hint line, and the result card
    /// with a retry button. Texts change only when their value changes (no per-frame strings).
    /// </summary>
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private Text armyText;
        [SerializeField] private Text towerText;
        [SerializeField] private Text hintText;
        [SerializeField] private GameObject resultCard;
        [SerializeField] private Text resultTitle;
        [SerializeField] private Text resultDetail;
        [SerializeField] private Button retryButton;

        private int shownArmy = -1;
        private int shownTower = -1;

        public event Action RetryPressed;

        private void Awake()
        {
            if (armyText == null || towerText == null || hintText == null || resultCard == null
                || resultTitle == null || resultDetail == null || retryButton == null)
            {
                Debug.LogError("PrototypeHud is missing a text or the retry button (rebuild the scene).", this);
                enabled = false;
                return;
            }

            retryButton.onClick.AddListener(() => RetryPressed?.Invoke());
        }

        public void SetArmy(int army)
        {
            if (army != shownArmy)
            {
                shownArmy = army;
                armyText.text = NumberLabels.Army(army);
            }
        }

        public void SetTower(int health, bool visible)
        {
            towerText.gameObject.SetActive(visible);
            if (visible && health != shownTower)
            {
                shownTower = health;
                towerText.text = NumberLabels.Tower(health);
            }
        }

        public void SetHint(string hint)
        {
            hintText.gameObject.SetActive(!string.IsNullOrEmpty(hint));
            hintText.text = hint;
        }

        public void ShowResult(bool won, string detail)
        {
            resultTitle.text = won ? "TOWER DOWN!" : "ARMY LOST";
            resultDetail.text = detail;
            resultCard.SetActive(true);
        }

        public void HideResult()
        {
            resultCard.SetActive(false);
        }
    }
}
