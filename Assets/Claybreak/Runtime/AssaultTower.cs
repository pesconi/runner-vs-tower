using System;
using JungleDegree.StudioKit.Characters;
using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The enemy tower: one preallocated body per <see cref="TowerKind"/> (only the level's is shown),
    /// health split in floors (each fallen floor drops blocks off the top), a health bar, a shake on
    /// hits and a sinking fall at zero. No objects are created or destroyed while playing.
    /// </summary>
    public sealed class AssaultTower : MonoBehaviour
    {
        public const int BlocksPerBody = ClaybreakLevel.MaximumFloors;
        private static readonly int KindCount = Enum.GetValues(typeof(TowerKind)).Length;

        [Tooltip("One body per TowerKind, in enum order.")]
        [SerializeField] private Transform[] bodies = new Transform[0];
        [Tooltip("Blocks that drop as floors fall: BlocksPerBody per body, top block first.")]
        [SerializeField] private Transform[] floorBlocks = new Transform[0];
        [Tooltip("Where the cannon fires from, one per body.")]
        [SerializeField] private Transform[] muzzles = new Transform[0];
        [Tooltip("Height of the health bar above the ground, one per body.")]
        [SerializeField] private float[] barHeights = new float[0];
        [SerializeField] private Transform healthBar;
        [SerializeField] private Transform healthFill;
        [SerializeField] private TextMesh healthLabel;
        [SerializeField, Min(0.1f)] private float fallSeconds = 1.4f;
        [SerializeField, Min(0f)] private float fallDepth = 12f;
        [SerializeField, Min(0f)] private float hitShake = 0.12f;

        private Transform body;
        private int kind;
        private int maximumHealth = 1;
        private int health = 1;
        private int floors = 1;
        private int floorsStanding = 1;
        private int blocksStanding = BlocksPerBody;
        private int shownHealth = -1;
        private float fallTime = -1f;
        private float shake;
        private Vector3[] bodyStarts;
        private Vector3 fillScale;
        private bool ready;

        public int Health => health;
        public int MaximumHealth => maximumHealth;
        public int FloorsStanding => floorsStanding;
        public bool IsStanding => health > 0;
        public Transform Muzzle => muzzles[kind];

        public event Action Destroyed;
        public event Action<int> HealthChanged;

        /// <summary>Raised when a floor falls, with the floors still standing (never for the last one).</summary>
        public event Action<int> FloorFallen;

        public bool TryValidate(out string error)
        {
            if (bodies == null || bodies.Length != KindCount || floorBlocks == null
                || floorBlocks.Length != KindCount * BlocksPerBody || muzzles == null || muzzles.Length != KindCount
                || barHeights == null || barHeights.Length != KindCount || healthBar == null || healthFill == null
                || healthLabel == null)
            {
                error = $"AssaultTower needs {KindCount} bodies, {KindCount * BlocksPerBody} floor blocks, a muzzle and bar height per body, and its health bar.";
                return false;
            }

            for (int i = 0; i < floorBlocks.Length; i++)
            {
                if (floorBlocks[i] == null || bodies[i / BlocksPerBody] == null || muzzles[i / BlocksPerBody] == null)
                {
                    error = "AssaultTower has an empty body, floor block or muzzle slot.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private void Awake()
        {
            if (!TryValidate(out string error))
            {
                Debug.LogError(error, this);
                enabled = false;
                return;
            }

            bodyStarts = new Vector3[bodies.Length];
            for (int i = 0; i < bodies.Length; i++)
            {
                bodyStarts[i] = bodies[i].localPosition;
            }

            fillScale = healthFill.localScale;
            ready = true;
        }

        public void ResetTower(TowerKind towerKind, int startHealth, int floorCount)
        {
            if (!ready)
            {
                return;
            }

            kind = (int)towerKind;
            maximumHealth = Mathf.Max(1, startHealth);
            health = maximumHealth;
            floors = Mathf.Clamp(floorCount, 1, ClaybreakLevel.MaximumFloors);
            floorsStanding = floors;
            blocksStanding = BlocksPerBody;
            fallTime = -1f;
            shake = 0f;
            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].localPosition = bodyStarts[i];
                bodies[i].gameObject.SetActive(i == kind);
            }

            for (int i = 0; i < floorBlocks.Length; i++)
            {
                floorBlocks[i].gameObject.SetActive(true);
            }

            body = bodies[kind];
            healthBar.localPosition = new Vector3(healthBar.localPosition.x, barHeights[kind], healthBar.localPosition.z);
            healthLabel.transform.localPosition = new Vector3(healthLabel.transform.localPosition.x, barHeights[kind] + 1.2f,
                healthLabel.transform.localPosition.z);
            healthBar.gameObject.SetActive(true);
            healthLabel.gameObject.SetActive(true);
            shownHealth = -1;
            Refresh();
        }

        public void Hit(int damage)
        {
            if (health <= 0)
            {
                return;
            }

            health = Mathf.Max(0, health - damage);
            shake = 1f;
            ClaybreakSfx.Play(ClaybreakSfx.Cue.TowerHit);
            Refresh();
            if (health == 0)
            {
                fallTime = 0f;
                healthBar.gameObject.SetActive(false);
                healthLabel.gameObject.SetActive(false);
                ClaybreakSfx.Play(ClaybreakSfx.Cue.TowerFall);
                Destroyed?.Invoke();
                return;
            }

            int perFloor = Mathf.CeilToInt((float)maximumHealth / floors);
            int standing = Mathf.CeilToInt((float)health / perFloor);
            if (standing < floorsStanding)
            {
                floorsStanding = standing;
                DropBlocks();
                FloorFallen?.Invoke(floorsStanding);
            }
        }

        private void DropBlocks()
        {
            int keep = Mathf.RoundToInt((float)BlocksPerBody * floorsStanding / floors);
            int first = kind * BlocksPerBody;
            while (blocksStanding > keep)
            {
                Transform block = floorBlocks[first + BlocksPerBody - blocksStanding];
                block.gameObject.SetActive(false);
                RunnerDeathEffects.Play(block.position, RunnerDeathEffects.MaximumJumpersPerHit);
                blocksStanding--;
            }

            ClaybreakSfx.Play(ClaybreakSfx.Cue.FloorBreak);
        }

        private void Update()
        {
            if (body == null)
            {
                return;
            }

            Vector3 start = bodyStarts[kind];
            if (fallTime >= 0f && fallTime < fallSeconds)
            {
                fallTime = Mathf.Min(fallTime + Time.deltaTime, fallSeconds);
                float t = fallTime / fallSeconds;
                float wobble = Mathf.Sin(t * 40f) * 0.3f * (1f - t);
                body.localPosition = start + new Vector3(wobble, -fallDepth * t * t, 0f);
                if (fallTime >= fallSeconds)
                {
                    body.gameObject.SetActive(false);
                }

                return;
            }

            if (shake > 0f)
            {
                shake = Mathf.Max(0f, shake - Time.deltaTime * 8f);
                body.localPosition = start + new Vector3(Mathf.Sin(Time.time * 70f) * hitShake * shake, 0f, 0f);
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
