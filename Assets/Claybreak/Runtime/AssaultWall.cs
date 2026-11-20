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
        private bool ready;

        public bool IsStanding => health > 0;
        public bool HasParts => body != null && healthLabel != null;

        private void Awake()
        {
            if (!HasParts)
            {
                Debug.LogError("AssaultWall needs its body and health label.", this);
                enabled = false;
                return;
            }

            bodyScale = body.localScale;
            ready = true;
        }

        /// <summary>Raises the wall with <paramref name="startHealth"/>, or keeps it down when the level has none.</summary>
        public void ResetWall(bool present, int startHealth)
        {
            if (!ready)
            {
                return;
            }

            maximumHealth = Mathf.Max(1, startHealth);
            health = present ? maximumHealth : 0;
            gameObject.SetActive(present);
            if (present)
            {
                Refresh();
            }
        }

        public void Hit(int damage)
        {
            if (health <= 0)
            {
                return;
            }

            health = Mathf.Max(0, health - damage);
            GameAudio.Play(GameSound.UnitLoss);
            if (health == 0)
            {
                ClaybreakSfx.Play(ClaybreakSfx.Cue.WallBreak);
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
