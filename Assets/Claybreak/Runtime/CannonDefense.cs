using JungleDegree.StudioKit.Characters;
using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The cannon defense: lobs a stone at the busiest lane; a shadow on the ground warns where it will
    /// land (GDD 4.3). Projectiles and shadows are preallocated by the scene builder (rule 2.2.1: no
    /// Instantiate/Destroy at run time); the level's maximumProjectiles caps how many fly at once.
    /// </summary>
    public sealed class CannonDefense : MonoBehaviour
    {
        [SerializeField] private Transform[] projectiles = new Transform[0];
        [SerializeField] private Transform[] shadows = new Transform[0];
        [SerializeField, Min(0f)] private float arcHeight = 7f;

        private Transform muzzle;
        private AssaultField field;
        private ClaybreakLevel level;
        private bool[] flying;
        private float[] progress;
        private Vector3[] starts;
        private Vector3[] targets;
        private float cooldown;
        private float intervalScale = 1f;
        private int limit;

        public bool Armed { get; set; }
        public int ActiveCount { get; private set; }
        public int PeakActive { get; private set; }
        public int PoolSize => projectiles == null ? 0 : projectiles.Length;

        /// <summary>Arms the cannon for a level, firing from <paramref name="towerMuzzle"/> (the shown tower's).</summary>
        public bool Configure(ClaybreakLevel levelData, AssaultField assaultField, Transform towerMuzzle)
        {
            if (towerMuzzle == null || projectiles == null || shadows == null || projectiles.Length == 0
                || projectiles.Length != shadows.Length || levelData == null || assaultField == null)
            {
                Debug.LogError("CannonDefense needs a muzzle and as many preallocated projectiles as shadows.", this);
                return false;
            }

            if (levelData.MaximumProjectiles > projectiles.Length)
            {
                Debug.LogError($"The level asks for {levelData.MaximumProjectiles} projectiles; the scene has {projectiles.Length}.", this);
                return false;
            }

            level = levelData;
            field = assaultField;
            muzzle = towerMuzzle;
            limit = level.CannonEnabled ? level.MaximumProjectiles : 0;
            if (flying == null)
            {
                flying = new bool[projectiles.Length];
                progress = new float[projectiles.Length];
                starts = new Vector3[projectiles.Length];
                targets = new Vector3[projectiles.Length];
            }

            ResetCannon();
            return true;
        }

        /// <summary>Boss rule "overheat": every later shot comes <paramref name="factor"/> times sooner (reset per level).</summary>
        public void Overheat(float factor)
        {
            intervalScale = Mathf.Max(0.2f, intervalScale * Mathf.Clamp01(factor));
            cooldown = Mathf.Min(cooldown, level == null ? 0f : level.CannonInterval * intervalScale);
        }

        public void ResetCannon()
        {
            Armed = false;
            ActiveCount = 0;
            PeakActive = 0;
            intervalScale = 1f;
            cooldown = level == null ? 0f : level.CannonInterval;
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (flying != null)
                {
                    flying[i] = false;
                }

                projectiles[i].gameObject.SetActive(false);
                shadows[i].gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (level == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            if (Armed)
            {
                cooldown -= deltaTime;
                if (cooldown <= 0f && ActiveCount < limit && field.TryPickTarget(level.ProjectileFlightTime, out Vector3 target))
                {
                    Fire(target);
                    ClaybreakSfx.Play(ClaybreakSfx.Cue.CannonFire);
                    cooldown = level.CannonInterval * intervalScale;
                }
            }

            for (int i = 0; i < projectiles.Length; i++)
            {
                if (!flying[i])
                {
                    continue;
                }

                progress[i] += deltaTime / level.ProjectileFlightTime;
                float t = Mathf.Min(progress[i], 1f);
                Vector3 position = Vector3.LerpUnclamped(starts[i], targets[i], t);
                position.y += arcHeight * 4f * t * (1f - t);
                projectiles[i].position = position;
                float warning = 0.6f + 0.6f * t;
                shadows[i].localScale = new Vector3(level.BlastRadius * 2f * warning, 0.02f, level.BlastRadius * 2f * warning);
                if (t >= 1f)
                {
                    Land(i);
                }
            }
        }

        private void Fire(Vector3 target)
        {
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (flying[i])
                {
                    continue;
                }

                flying[i] = true;
                progress[i] = 0f;
                starts[i] = muzzle.position;
                targets[i] = target;
                projectiles[i].position = starts[i];
                projectiles[i].gameObject.SetActive(true);
                shadows[i].position = new Vector3(target.x, target.y + 0.03f, target.z);
                shadows[i].gameObject.SetActive(true);
                ActiveCount++;
                if (ActiveCount > PeakActive)
                {
                    PeakActive = ActiveCount;
                }

                return;
            }
        }

        private void Land(int index)
        {
            flying[index] = false;
            ActiveCount--;
            projectiles[index].gameObject.SetActive(false);
            shadows[index].gameObject.SetActive(false);
            int killed = field.KillInRadius(targets[index], level.BlastRadius);
            if (killed > 0)
            {
                RunnerDeathEffects.Play(targets[index], Mathf.Min(killed, 6));
                GameAudio.Play(GameSound.UnitLoss);
            }
        }
    }
}
