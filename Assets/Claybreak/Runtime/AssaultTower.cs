using System;
using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>The enemy tower: health, a health bar, and a sinking fall when it reaches zero.</summary>
    public sealed class AssaultTower : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Transform healthFill;
        [SerializeField] private TextMesh healthLabel;
        [SerializeField, Min(0.1f)] private float fallSeconds = 1.2f;
        [SerializeField, Min(0f)] private float fallDepth = 9f;

        private int maximumHealth = 1;
        private int health = 1;
        private int shownHealth = -1;
        private float fallTime = -1f;
        private Vector3 bodyStart;
        private Vector3 fillScale;

        public int Health => health;
        public int MaximumHealth => maximumHealth;
        public bool IsStanding => health > 0;

        public event Action Destroyed;
        public event Action<int> HealthChanged;

        private void Awake()
        {
            if (body == null || healthFill == null || healthLabel == null)
            {
                Debug.LogError("AssaultTower needs its body, health bar fill and label.", this);
                enabled = false;
                return;
            }

            bodyStart = body.localPosition;
            fillScale = healthFill.localScale;
        }

        public void ResetTower(int startHealth)
        {
            maximumHealth = Mathf.Max(1, startHealth);
            health = maximumHealth;
            fallTime = -1f;
            body.localPosition = bodyStart;
            body.gameObject.SetActive(true);
            healthFill.parent.gameObject.SetActive(true);
            Refresh();
        }

        public void Hit(int damage)
        {
            if (health <= 0)
            {
                return;
            }

            health = Mathf.Max(0, health - damage);
            GameAudio.Play(GameSound.Coin);
            Refresh();
            if (health == 0)
            {
                fallTime = 0f;
                healthFill.parent.gameObject.SetActive(false);
                Destroyed?.Invoke();
            }
        }

        private void Update()
        {
            if (fallTime < 0f || fallTime >= fallSeconds)
            {
                return;
            }

            fallTime = Mathf.Min(fallTime + Time.deltaTime, fallSeconds);
            float t = fallTime / fallSeconds;
            float wobble = Mathf.Sin(t * 40f) * 0.25f * (1f - t);
            body.localPosition = bodyStart + new Vector3(wobble, -fallDepth * t * t, 0f);
            if (fallTime >= fallSeconds)
            {
                body.gameObject.SetActive(false);
            }
        }

        private void Refresh()
        {
            float fraction = (float)health / maximumHealth;
            healthFill.localScale = new Vector3(fillScale.x * fraction, fillScale.y, fillScale.z);
            healthFill.localPosition = new Vector3(-0.5f * (1f - fraction), healthFill.localPosition.y,
                healthFill.localPosition.z);
            if (health != shownHealth)
            {
                shownHealth = health;
                healthLabel.text = NumberLabels.Plain(health);
                HealthChanged?.Invoke(health);
            }
        }
    }
}
