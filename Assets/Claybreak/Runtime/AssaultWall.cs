using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>The wall defense: blocks one lane until the units that crash into it bring it down.</summary>
    public sealed class AssaultWall : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private TextMesh healthLabel;

        private int maximumHealth = 1;
        private int health;
        private Vector3 bodyScale;

        public bool IsStanding => health > 0;

        private void Awake()
        {
            if (body == null || healthLabel == null)
            {
                Debug.LogError("AssaultWall needs its body and health label.", this);
                enabled = false;
                return;
            }

            bodyScale = body.localScale;
        }

        public void ResetWall(int startHealth)
        {
            maximumHealth = Mathf.Max(1, startHealth);
            health = maximumHealth;
            gameObject.SetActive(true);
            Refresh();
        }

        public void Hit()
        {
            if (health <= 0)
            {
                return;
            }

            health--;
            GameAudio.Play(GameSound.UnitLoss);
            if (health == 0)
            {
                GameAudio.Play(GameSound.GateGood);
                gameObject.SetActive(false);
                return;
            }

            Refresh();
        }

        // The wall shrinks as it loses health, so the player reads progress without a number.
        private void Refresh()
        {
            float fraction = 0.35f + 0.65f * health / maximumHealth;
            body.localScale = new Vector3(bodyScale.x, bodyScale.y * fraction, bodyScale.z);
            body.localPosition = new Vector3(0f, bodyScale.y * fraction * 0.5f, 0f);
            healthLabel.text = NumberLabels.Plain(health);
        }
    }
}
