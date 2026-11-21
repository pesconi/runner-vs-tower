using System;
using JungleDegree.StudioKit.Characters;
using JungleDegree.StudioKit.Core;
using JungleDegree.StudioKit.Crowd;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// Units in flight during the assault, moved in one batch (reuso-tecnico 4.1, risk R-D2): one Update
    /// walks preallocated arrays; no per-unit Update, no physics, no allocation. A unit in flight never
    /// collides with another; it only meets the gate row, the sweeper, the wall, the shield generator and the
    /// tower, by distance along its lane (R-D4). While the shield stands the tower takes only part of each hit. Units come from the kit's RunnerPool and go back to it when they hit or die.
    /// A unit can be special (a type from the loadout, RT-5): its own size, color, damage, armor and speed.
    /// The damage upgrade multiplies every hit; fractions carry over, so +15% is exact over a stream.
    /// </summary>
    public sealed class AssaultField : MonoBehaviour
    {
        private const float SteerResponse = 6f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private RunnerPool runnerPool;
        [SerializeField] private AssaultTower tower;
        [SerializeField] private AssaultWall wall;
        [SerializeField] private SweeperDefense sweeper;
        [SerializeField] private ShieldDefense shield;
        [SerializeField] private Transform[] gateVisuals = new Transform[ClaybreakLevel.LaneCount];
        [SerializeField] private TextMesh[] gateLabels = new TextMesh[ClaybreakLevel.LaneCount];
        [SerializeField] private Renderer[] gatePanels = new Renderer[ClaybreakLevel.LaneCount];
        [SerializeField] private Material goodGateMaterial;
        [SerializeField] private Material badGateMaterial;

        private ClaybreakLevel level;
        private readonly float[] laneX = new float[ClaybreakLevel.LaneCount];
        private readonly int[] gatePassed = new int[ClaybreakLevel.LaneCount];
        private readonly bool[] gateTurned = new bool[ClaybreakLevel.LaneCount];
        private readonly int[] laneCounts = new int[ClaybreakLevel.LaneCount];
        private readonly float[] laneZSum = new float[ClaybreakLevel.LaneCount];
        private RunnerUnit[] units;
        private Transform[] transforms;
        private Renderer[] specialRenderers;
        private float[] xs;
        private float[] targetXs;
        private float[] zs;
        private int[] lanes;
        private int[] armor;
        private int[] kinds;
        private float[] speeds;
        private int count;
        private int capacity;
        private int gateShift;
        private uint spreadSeed = 1u;
        private Vector3 unitScale = Vector3.one * 0.5f;
        private Color crowdColor = Color.white;
        private readonly UnitType[] slotTypes = new UnitType[ClaybreakCatalog.MaximumLoadoutSlots];
        private float damageMultiplier = 1f;
        private float towerCarry;
        private float wallCarry;
        private float shieldCarry;
        private int towerDamage = 1;
        private MaterialPropertyBlock colorBlock;

        public int InFlight => count;
        public int PeakInFlight { get; private set; }
        public int TowerHits { get; private set; }
        public int Capacity => capacity;

        /// <summary>Units the runner pool holds (the most that can ever fly at once).</summary>
        public int PoolCapacity => runnerPool == null ? 0 : runnerPool.Capacity;
        public int GateShift => gateShift;
        public bool Running { get; set; }

        /// <summary>Special units in flight now (the device gate logs it).</summary>
        public int SpecialsInFlight
        {
            get
            {
                int specials = 0;
                for (int i = 0; i < count; i++)
                {
                    specials += kinds[i] >= 0 ? 1 : 0;
                }

                return specials;
            }
        }

        /// <summary>One-time setup: lane positions and preallocated arrays (the flow then dresses the pool in the skin).</summary>
        public bool Initialize(Renderer track, ClaybreakTheme theme, Vector3 followerScale)
        {
            if (runnerPool == null || tower == null || wall == null || sweeper == null || shield == null || gateVisuals == null
                || gateVisuals.Length != ClaybreakLevel.LaneCount || gateLabels.Length != ClaybreakLevel.LaneCount
                || gatePanels.Length != ClaybreakLevel.LaneCount || goodGateMaterial == null || badGateMaterial == null
                || track == null || theme == null)
            {
                Debug.LogError("AssaultField needs the pool, tower, wall, sweeper, shield, one gate per lane, the track and the theme.", this);
                return false;
            }

            unitScale = followerScale;
            crowdColor = theme.CrowdColor;
            colorBlock = new MaterialPropertyBlock();
            Bounds bounds = track.bounds;
            float laneWidth = bounds.size.x / ClaybreakLevel.LaneCount;
            for (int i = 0; i < ClaybreakLevel.LaneCount; i++)
            {
                laneX[i] = bounds.min.x + laneWidth * (i + 0.5f);
            }

            capacity = runnerPool.Capacity;
            units = new RunnerUnit[capacity];
            transforms = new Transform[capacity];
            specialRenderers = new Renderer[capacity];
            xs = new float[capacity];
            targetXs = new float[capacity];
            zs = new float[capacity];
            lanes = new int[capacity];
            armor = new int[capacity];
            kinds = new int[capacity];
            speeds = new float[capacity];
            return true;
        }

        /// <summary>Per-level setup: defenses placed and dressed for <paramref name="levelData"/>.</summary>
        public void Configure(ClaybreakLevel levelData, Renderer track, int wallHealth, int shieldHealth, int damagePerUnit)
        {
            level = levelData;
            towerDamage = Mathf.Max(1, damagePerUnit);
            capacity = Mathf.Min(level.MaximumUnitsInFlight, runnerPool.Capacity);
            Bounds bounds = track.bounds;
            sweeper.Configure(level.SweeperEnabled, level.SweeperZ, level.SweeperPeriod, bounds.min.x, bounds.max.x);
            Vector3 wallPosition = wall.transform.position;
            wall.transform.position = new Vector3(laneX[level.WallLane], wallPosition.y, level.WallZ);
            wall.ResetWall(level.WallEnabled, wallHealth);
            Vector3 towerPosition = tower.transform.position;
            tower.transform.position = new Vector3(towerPosition.x, towerPosition.y, level.TowerZ);
            shield.ResetShield(level.ShieldEnabled, shieldHealth, level.ShieldBlock, laneX[level.ShieldLane], level.ShieldZ, level.TowerZ);
            for (int i = 0; i < ClaybreakLevel.LaneCount; i++)
            {
                Vector3 position = gateVisuals[i].position;
                gateVisuals[i].position = new Vector3(laneX[i], position.y, level.AssaultGateZ);
            }

            gateShift = 0;
            Array.Clear(gateTurned, 0, gateTurned.Length);
            towerCarry = 0f;
            wallCarry = 0f;
            shieldCarry = 0f;
            DressGates();
        }

        /// <summary>
        /// Per-run loadout (menu time, field empty): the special unit types by slot, the crowd skin color a
        /// special goes back to, and the damage upgrade.
        /// </summary>
        public void ConfigureLoadout(UnitType[] slots, Color skinColor, float damage)
        {
            for (int i = 0; i < slotTypes.Length; i++)
            {
                slotTypes[i] = slots != null && i < slots.Length ? slots[i] : null;
            }

            crowdColor = skinColor;
            damageMultiplier = Mathf.Max(1f, damage);
        }

        /// <summary>
        /// Menu time only: every idle pool unit wears the crowd skin, so a unit cloned by a xN gate (never part
        /// of the crowd) looks like the others without dressing it mid-assault.
        /// </summary>
        public bool DressIdleUnits(ThemeCharacterRoster roster, Color color, CharacterController leader)
        {
            crowdColor = color;
            int available = runnerPool.AvailableCount;
            RunnerUnit[] taken = new RunnerUnit[available];
            bool dressed = true;
            for (int i = 0; i < available; i++)
            {
                runnerPool.TryGet(out taken[i]);
                taken[i].transform.localScale = unitScale;
                dressed &= taken[i].TryApplyColorLook(roster, ClaybreakTheme.CrowdColorId, color, leader);
            }

            for (int i = 0; i < available; i++)
            {
                runnerPool.Release(taken[i]);
            }

            return dressed;
        }

        public float GetLaneX(int lane)
        {
            return laneX[Mathf.Clamp(lane, 0, ClaybreakLevel.LaneCount - 1)];
        }

        public int GetNearestLane(float x)
        {
            int best = 0;
            for (int i = 1; i < ClaybreakLevel.LaneCount; i++)
            {
                if (Mathf.Abs(x - laneX[i]) < Mathf.Abs(x - laneX[best]))
                {
                    best = i;
                }
            }

            return best;
        }

        /// <summary>The gate that stands in <paramref name="lane"/> now (the boss rule shifts them).</summary>
        public GateSpec GateIn(int lane)
        {
            const int lanesCount = ClaybreakLevel.LaneCount;
            int index = ((lane - gateShift) % lanesCount + lanesCount) % lanesCount;
            return gateTurned[index] ? new GateSpec(GateOperation.Divide, 2) : level.AssaultGates[index];
        }

        /// <summary>
        /// Boss rule "turn gate": the strongest multiplier still standing in the row becomes ÷2. False when
        /// no multiplier is left to turn.
        /// </summary>
        public bool TurnBestGate()
        {
            int best = -1;
            for (int i = 0; i < ClaybreakLevel.LaneCount; i++)
            {
                GateSpec gate = level.AssaultGates[i];
                if (!gateTurned[i] && gate.operation == GateOperation.Multiply && gate.value > 1
                    && (best < 0 || gate.value > level.AssaultGates[best].value))
                {
                    best = i;
                }
            }

            if (best < 0)
            {
                return false;
            }

            gateTurned[best] = true;
            Array.Clear(gatePassed, 0, gatePassed.Length);
            DressGates();
            return true;
        }

        /// <summary>Boss rule "overheat": the sweeper swings faster.</summary>
        public void SpeedUpSweeper(float factor)
        {
            sweeper.SpeedUp(factor);
        }

        /// <summary>Boss rule "shield returns": the generator stands again at full health.</summary>
        public void RaiseShield()
        {
            shield.Raise();
        }

        public ShieldDefense Shield => shield;

        /// <summary>Boss rule (GDD 4.4): the gate row turns one lane to the right.</summary>
        public void ShiftGates()
        {
            gateShift = (gateShift + 1) % ClaybreakLevel.LaneCount;
            Array.Clear(gatePassed, 0, gatePassed.Length);
            DressGates();
        }

        /// <summary>Takes a free pool unit and sends it down <paramref name="lane"/>; <paramref name="slot"/> is the special type (-1 = plain).</summary>
        public bool TryLaunch(int lane, Vector3 from, int slot)
        {
            return TrySpawn(lane, from.x, from.z, slot >= 0 && slot < slotTypes.Length && slotTypes[slot] != null ? slot : -1);
        }

        /// <summary>Kills every unit within <paramref name="radius"/> of <paramref name="center"/> (cannon blast); armor absorbs one.</summary>
        public int KillInRadius(Vector3 center, float radius)
        {
            float radiusSquared = radius * radius;
            int killed = 0;
            for (int i = count - 1; i >= 0; i--)
            {
                float dx = xs[i] - center.x;
                float dz = zs[i] - center.z;
                if (dx * dx + dz * dz <= radiusSquared && !AbsorbHit(i))
                {
                    RemoveOrRecycle(i);
                    killed++;
                }
            }

            return killed;
        }

        /// <summary>
        /// Where a cannon should aim: the lane with the most units in flight, ahead of their mean position
        /// by <paramref name="leadSeconds"/>, never past the wall/tower line. False when nothing flies.
        /// </summary>
        public bool TryPickTarget(float leadSeconds, out Vector3 target)
        {
            target = default;
            if (count == 0)
            {
                return false;
            }

            Array.Clear(laneCounts, 0, laneCounts.Length);
            Array.Clear(laneZSum, 0, laneZSum.Length);
            for (int i = 0; i < count; i++)
            {
                laneCounts[lanes[i]]++;
                laneZSum[lanes[i]] += zs[i];
            }

            int lane = 0;
            for (int i = 1; i < ClaybreakLevel.LaneCount; i++)
            {
                if (laneCounts[i] > laneCounts[lane])
                {
                    lane = i;
                }
            }

            float z = laneZSum[lane] / laneCounts[lane] + level.UnitSpeed * leadSeconds;
            z = Mathf.Min(z, level.TowerZ - 2f);
            target = new Vector3(laneX[lane], transform.position.y, z);
            return true;
        }

        /// <summary>Sends every unit in flight back to the pool and clears the counters.</summary>
        public void ResetField()
        {
            for (int i = count - 1; i >= 0; i--)
            {
                Remove(i);
            }

            Array.Clear(gatePassed, 0, gatePassed.Length);
            PeakInFlight = 0;
            TowerHits = 0;
            Running = false;
            sweeper.Running = false;
        }

        private void Update()
        {
            if (!Running || count == 0 || level == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            float baseStep = level.UnitSpeed * deltaTime;
            float steer = 1f - Mathf.Exp(-SteerResponse * deltaTime);
            float gateZ = level.AssaultGateZ;
            float wallZ = level.WallZ;
            float sweeperZ = level.SweeperZ;
            float shieldZ = level.ShieldZ;
            float towerZ = level.TowerZ;
            int wallLane = level.WallLane;
            int shieldLane = level.ShieldLane;
            bool shielded = shield.IsStanding;
            bool sweeping = sweeper.IsPresent;
            int lostAtWall = 0;
            float y = transform.position.y;

            // Backwards, so a removal (swap with the last) never skips a unit; clones appended this
            // frame sit past the start index and move from the next frame.
            for (int i = count - 1; i >= 0; i--)
            {
                float previousZ = zs[i];
                float step = baseStep * speeds[i];
                float z = previousZ + step;
                int lane = lanes[i];

                if (previousZ < gateZ && z >= gateZ)
                {
                    int leaving = GateMath.UnitsOut(GateIn(lane), ++gatePassed[lane]);
                    if (leaving == 0)
                    {
                        Remove(i);
                        continue;
                    }

                    for (int clone = 1; clone < leaving; clone++)
                    {
                        // Clones are plain units that trail the original a little (a burst, not a stack)
                        // but never behind the gate, or they would cross it again.
                        if (!TrySpawn(lane, xs[i], Mathf.Max(gateZ, z - 0.35f * clone), -1))
                        {
                            break;
                        }
                    }
                }

                if (sweeping && previousZ < sweeperZ && z >= sweeperZ && sweeper.Covers(xs[i]) && !AbsorbHit(i))
                {
                    RunnerDeathEffects.Play(new Vector3(xs[i], y, sweeperZ), 2);
                    ClaybreakSfx.Play(ClaybreakSfx.Cue.Sweep);
                    RemoveOrRecycle(i);
                    continue;
                }

                if (lane == wallLane && wall.IsStanding && z >= wallZ)
                {
                    wall.Hit(Damage(i, 1, ref wallCarry));
                    Remove(i);
                    lostAtWall++;
                    continue;
                }

                if (shielded && lane == shieldLane && z >= shieldZ)
                {
                    shield.Hit(Damage(i, 1, ref shieldCarry));
                    shielded = shield.IsStanding;
                    RunnerDeathEffects.Play(new Vector3(xs[i], y, shieldZ - 0.5f), 1);
                    RemoveOrRecycle(i);
                    continue;
                }

                if (z >= towerZ)
                {
                    TowerHits++;
                    // The device gate keeps the slice's flat damage so its endless tower outlasts the measurement.
                    tower.Hit(level.RecycleUnits ? towerDamage : Damage(i, towerDamage, ref towerCarry, 1f - shield.Block));
                    RemoveOrRecycle(i);
                    continue;
                }

                float x = xs[i] + (targetXs[i] - xs[i]) * steer;
                xs[i] = x;
                zs[i] = z;
                transforms[i].position = new Vector3(x, y, z);
                units[i].AnimateRun(step);
            }

            if (lostAtWall > 0)
            {
                RunnerDeathEffects.Play(new Vector3(laneX[wallLane], y, wallZ - 0.5f), Mathf.Min(lostAtWall, 3));
            }
        }

        // Whole damage of one hit (base x special x upgrade x what the shield lets through); the fraction
        // carries to the next hit, so a half-blocked stream does exactly half the damage. A fully absorbed
        // hit deals 0 (the shield took it).
        private int Damage(int index, int baseDamage, ref float carry, float passes = 1f)
        {
            int special = kinds[index] >= 0 ? slotTypes[kinds[index]].damage : 1;
            carry += baseDamage * special * damageMultiplier * passes;
            int whole = (int)carry;
            carry -= whole;
            return passes >= 1f ? Mathf.Max(1, whole) : whole;
        }

        private bool AbsorbHit(int index)
        {
            if (armor[index] <= 0)
            {
                return false;
            }

            armor[index]--;
            return true;
        }

        private bool TrySpawn(int lane, float x, float z, int slot)
        {
            if (count >= capacity || !runnerPool.TryGet(out RunnerUnit unit))
            {
                return false;
            }

            Transform unitTransform = unit.transform;
            UnitType type = slot >= 0 ? slotTypes[slot] : null;
            unitTransform.localScale = type != null ? unitScale * type.scale : unitScale;
            unitTransform.rotation = Quaternion.identity;
            unitTransform.position = new Vector3(x, transform.position.y, z);
            units[count] = unit;
            transforms[count] = unitTransform;
            xs[count] = x;
            targetXs[count] = laneX[lane] + NextSpread() * level.LaneSpread;
            zs[count] = z;
            lanes[count] = lane;
            kinds[count] = type != null ? slot : -1;
            armor[count] = type != null ? type.armor : 0;
            speeds[count] = type != null ? type.speed : 1f;
            specialRenderers[count] = null;
            if (type != null)
            {
                // Only special units pay for a component lookup (a handful per level).
                Renderer renderer = unit.GetComponent<Renderer>();
                specialRenderers[count] = renderer;
                Tint(renderer, type.color);
            }

            count++;
            if (count > PeakInFlight)
            {
                PeakInFlight = count;
            }

            return true;
        }

        // Device-gate levels keep the unit in flight: back to the firing line, same lane.
        private void RemoveOrRecycle(int index)
        {
            if (level.RecycleUnits)
            {
                zs[index] = level.FiringLineZ;
                transforms[index].position = new Vector3(xs[index], transform.position.y, zs[index]);
                return;
            }

            Remove(index);
        }

        private void Remove(int index)
        {
            RunnerUnit unit = units[index];
            if (kinds[index] >= 0)
            {
                Tint(specialRenderers[index], crowdColor);
                unit.transform.localScale = unitScale;
            }

            int last = count - 1;
            units[index] = units[last];
            transforms[index] = transforms[last];
            specialRenderers[index] = specialRenderers[last];
            xs[index] = xs[last];
            targetXs[index] = targetXs[last];
            zs[index] = zs[last];
            lanes[index] = lanes[last];
            armor[index] = armor[last];
            kinds[index] = kinds[last];
            speeds[index] = speeds[last];
            units[last] = null;
            transforms[last] = null;
            specialRenderers[last] = null;
            count = last;
            runnerPool.Release(unit);
        }

        private void Tint(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.GetPropertyBlock(colorBlock);
            colorBlock.SetColor(BaseColorId, color);
            renderer.SetPropertyBlock(colorBlock);
        }

        // Deterministic spread in [-1, 1] (xorshift), so a run looks the same every time and costs nothing.
        private float NextSpread()
        {
            spreadSeed ^= spreadSeed << 13;
            spreadSeed ^= spreadSeed >> 17;
            spreadSeed ^= spreadSeed << 5;
            return (spreadSeed & 0xFFFF) / 32767.5f - 1f;
        }

        private void DressGates()
        {
            for (int i = 0; i < ClaybreakLevel.LaneCount; i++)
            {
                GateSpec gate = GateIn(i);
                gateLabels[i].text = gate.Label;
                gatePanels[i].sharedMaterial = gate.IsGood ? goodGateMaterial : badGateMaterial;
            }
        }
    }
}
