using System;
using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The shield defense (GDD 4.3, fourth type): a generator on one lane powers a dome over the tower's
    /// front; while the generator stands, the tower ignores part of every hit (the level's block share). Units
    /// that reach the generator break against it, so the player decides how many to spend on it. Everything
    /// is preallocated; the generator and the dome are only shown or hidden.
    /// </summary>
    public sealed class ShieldDefense : MonoBehaviour
    {
        [SerializeField] private Transform generator;
        [SerializeField] private TextMesh healthLabel;
        [Tooltip("The dome in front of the tower, shown while the generator stands.")]
        [SerializeField] private Transform dome;

        private int maximumHealth = 1;
        private int health;
        private float block;
        private Vector3 generatorScale;
        private bool present;
        private bool ready;

        public bool IsStanding => health > 0;
        public bool IsPresent => present;
        public bool HasParts => generator != null && healthLabel != null && dome != null;

        /// <summary>Share of a tower hit absorbed right now (0 once the generator is down).</summary>
        public float Block => IsStanding ? block : 0f;

        /// <summary>Raised when the generator falls.</summary>
        public event Action Fallen;

        private void Awake()
        {
            if (!HasParts)
            {
                Debug.LogError("ShieldDefense needs its generator, health label and dome.", this);
                enabled = false;
                return;
            }

            generatorScale = generator.localScale;
            ready = true;
        }

        /// <summary>Places the generator at (<paramref name="x"/>, <paramref name="z"/>) and the dome at the tower, or hides both.</summary>
        public void ResetShield(bool enabled, int startHealth, float blockShare, float x, float z, float towerZ)
        {
            if (!ready)
            {
                return;
            }

            present = enabled;
            maximumHealth = Mathf.Max(1, startHealth);
            block = Mathf.Clamp(blockShare, 0f, 0.9f);
            transform.position = new Vector3(x, transform.position.y, z);
            dome.position = new Vector3(dome.position.x, dome.position.y, towerZ);
            health = 0;
            if (enabled)
            {
                Raise();
            }
            else
            {
                Show(false);
            }
        }

        /// <summary>The generator back at full health (also the world 2 boss rule).</summary>
        public void Raise()
        {
            if (!ready || !present)
            {
                return;
            }

            health = maximumHealth;
            Show(true);
            Refresh();
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
                Show(false);
                Fallen?.Invoke();
                return;
            }

            Refresh();
        }

        private void Show(bool shown)
        {
            generator.gameObject.SetActive(shown);
            healthLabel.gameObject.SetActive(shown);
            dome.gameObject.SetActive(shown);
        }

        // The generator sinks as it loses health (read without the number, like the wall).
        private void Refresh()
        {
            float fraction = 0.4f + 0.6f * health / maximumHealth;
            generator.localScale = new Vector3(generatorScale.x, generatorScale.y * fraction, generatorScale.z);
            healthLabel.text = NumberLabels.Plain(health);
        }
    }
}
