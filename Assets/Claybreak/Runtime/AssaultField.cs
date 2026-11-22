using System;
using JungleDegree.StudioKit.Swarm;
using TMPro;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// Units in flight during the assault, moved in one batch (reuso-tecnico 4.1, risk R-D2): one Update
    /// walks preallocated arrays; no per-unit Update, no physics, no allocation. A unit in flight never
    /// collides with another; it only meets the gate row, the sweeper, the wall, the shield generator and the
    /// tower, by distance along its lane (R-D4). While the shield stands the tower takes only part of each hit.
    /// RT-6t.7: the units are drawn by an instanced <see cref="LooseSwarmRenderer"/> (no object per unit, the crowd's
    /// shape and look); index i of these arrays is unit i there (both remove by moving the last unit in). A unit lost to
    /// the gate row or a defense dies the crowd's ballistic death; a unit that reaches the tower is gone.
    /// A unit can be special (a type from the loadout, RT-5): its own size, color, damage, armor and speed.
    /// The damage upgrade multiplies every hit; fractions carry over, so +15% is exact over a stream.
    /// </summary>
    public sealed class AssaultField : MonoBehaviour
    {
        private const float SteerResponse = 6f;
        // Draw box of the units in flight: from this far behind the firing line to this far past the tower, this high.
        private const float AreaBehind = 8f;
        private const float AreaPast = 3f;
        private const float AreaHeight = 3f;
        private static readonly Vector2 Backward = new Vector2(0f, -1f);

        [SerializeField] private LooseSwarmRenderer flyers;
        [SerializeField] private AssaultTower tower;
        [SerializeField] private AssaultWall wall;
        [SerializeField] private SweeperDefense sweeper;
        [SerializeField] private ShieldDefense shield;
        [SerializeField] private Transform[] gateVisuals = new Transform[ClaybreakLevel.LaneCount];
        [SerializeField] private TMP_Text[] gateLabels = new TMP_Text[ClaybreakLevel.LaneCount];
        [SerializeField] private Renderer[] gatePanels = new Renderer[ClaybreakLevel.LaneCount];
        [SerializeField] private Material goodGateMaterial;
        [SerializeField] private Material badGateMaterial;
        [SerializeField] private Material goodLabelMaterial;
        [SerializeField] private Material badLabelMaterial;
        private readonly char[] labelBuffer = new char[GateSpec.MaximumLabelLength];

        private ClaybreakLevel level;
        private readonly float[] laneX = new float[ClaybreakLevel.LaneCount];
        private readonly int[] gatePassed = new int[ClaybreakLevel.LaneCount];
        private readonly bool[] gateTurned = new bool[ClaybreakLevel.LaneCount];
        private readonly int[] laneCounts = new int[ClaybreakLevel.LaneCount];
        private readonly float[] laneZSum = new float[ClaybreakLevel.LaneCount];
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
        private Color crowdColor = Color.white;
        private readonly UnitType[] slotTypes = new UnitType[ClaybreakCatalog.MaximumLoadoutSlots];
        private float damageMultiplier = 1f;
        private float towerCarry;
        private float wallCarry;
        private float shieldCarry;
        private int towerDamage = 1;

        public int InFlight => count;
        public int PeakInFlight { get; private set; }
        public int TowerHits { get; private set; }
        public int Capacity => capacity;

        /// <summary>The most units that can ever fly at once (the instanced renderer's capacity).</summary>
        public int FlightCapacity => flyers == null ? 0 : flyers.Capacity;

        /// <summary>The instanced units in flight and their deaths.</summary>
        public LooseSwarmRenderer Flyers => flyers;
        public int GateShift => gateShift;
        public bool Running { get; set; }

        /// <summary>The crowd skin color the plain units wear now.</summary>
        public Color CrowdColor => crowdColor;

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

        /// <summary>One-time setup: lane positions and preallocated arrays (the flow then sets the skin color and the shape).</summary>
        public bool Initialize(Renderer track, ClaybreakTheme theme)
        {
            if (flyers == null || !flyers.isActiveAndEnabled || tower == null || wall == null || sweeper == null || shield == null
                || gateVisuals == null || gateVisuals.Length != ClaybreakLevel.LaneCount || gateLabels.Length != ClaybreakLevel.LaneCount
                || gatePanels.Length != ClaybreakLevel.LaneCount || goodGateMaterial == null || badGateMaterial == null
                || goodLabelMaterial == null || badLabelMaterial == null
                || track == null || theme == null)
            {
                Debug.LogError("AssaultField needs its running unit renderer, tower, wall, sweeper, shield, one gate per lane, the track and the theme.", this);
                return false;
            }

            crowdColor = theme.CrowdColor;
            Bounds bounds = track.bounds;
            float laneWidth = bounds.size.x / ClaybreakLevel.LaneCount;
            for (int i = 0; i < ClaybreakLevel.LaneCount; i++)
            {
                laneX[i] = bounds.min.x + laneWidth * (i + 0.5f);
            }

            capacity = flyers.Capacity;
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
            capacity = Mathf.Min(level.MaximumUnitsInFlight, flyers.Capacity);
            Bounds bounds = track.bounds;
            float areaStart = level.FiringLineZ - AreaBehind;
            float areaEnd = level.TowerZ + AreaPast;
            flyers.Area = new Bounds(new Vector3(bounds.center.x, AreaHeight * 0.5f, (areaStart + areaEnd) * 0.5f),
                new Vector3(bounds.size.x + 2f, AreaHeight, areaEnd - areaStart));
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
        /// Per-run loadout (menu time, field empty): the special unit types by slot, the crowd skin color the
        /// plain units wear, and the damage upgrade.
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

        /// <summary>The crowd skin color every plain unit sent from now on wears (a special wears its type's color).</summary>
        public void Wear(Color color)
        {
            crowdColor = color;
        }

        /// <summary>The unit shape the units in flight are drawn with (the crowd's, picked in the menu).</summary>
        public void UseShape(Mesh shape)
        {
            flyers.SetUnitMesh(shape);
        }

        /// <summary>
        /// <paramref name="units"/> plain units thrown off at <paramref name="position"/> in the ballistic death (a
        /// tower floor breaking up); nothing in flight changes.
        /// </summary>
        public void ThrowOff(Vector3 position, int units)
        {
            for (int i = 0; i < units; i++)
            {
                float angle = (i + 0.5f) * (2f * Mathf.PI / units);
                flyers.Throw(position.x, position.y, position.z, 1f, crowdColor, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
            }
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

        /// <summary>Sends a unit down <paramref name="lane"/> from <paramref name="from"/>; <paramref name="slot"/> is the special type (-1 = plain).</summary>
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
                    // Thrown away from the blast.
                    RemoveOrRecycle(i, true, new Vector2(dx, dz));
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

        /// <summary>Clears every unit in flight (and the ones dying) and the counters.</summary>
        public void ResetField()
        {
            count = 0;
            flyers.Clear();
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

            if (flyers.Count != count)
            {
                // The renderer was disabled and lost its units (never in play): start over rather than desync.
                Debug.LogError("[AssaultField] the units in flight lost their renderer; clearing the field", this);
                ResetField();
                return;
            }

            float deltaTime = Time.deltaTime;
            float inverseDeltaTime = deltaTime > 0f ? 1f / deltaTime : 0f;
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
                        Remove(i, true, Backward);
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
                    ClaybreakSfx.Play(ClaybreakSfx.Cue.Sweep);
                    RemoveOrRecycle(i, true, Backward);
                    continue;
                }

                if (lane == wallLane && wall.IsStanding && z >= wallZ)
                {
                    wall.Hit(Damage(i, 1, ref wallCarry));
                    Remove(i, true, Backward);
                    continue;
                }

                if (shielded && lane == shieldLane && z >= shieldZ)
                {
                    shield.Hit(Damage(i, 1, ref shieldCarry));
                    shielded = shield.IsStanding;
                    RemoveOrRecycle(i, true, Backward);
                    continue;
                }

                if (z >= towerZ)
                {
                    TowerHits++;
                    // The device gate keeps the slice's flat damage so its endless tower outlasts the measurement.
                    tower.Hit(level.RecycleUnits ? towerDamage : Damage(i, towerDamage, ref towerCarry, 1f - shield.Block));
                    RemoveOrRecycle(i, false, Backward);
                    continue;
                }

                float x = xs[i] + (targetXs[i] - xs[i]) * steer;
                flyers.Place(i, x, z, (x - xs[i]) * inverseDeltaTime);
                xs[i] = x;
                zs[i] = z;
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
            UnitType type = slot >= 0 ? slotTypes[slot] : null;
            // Elites (the RT-5 types) are drawn at their own size and in their own color, per instance.
            if (count >= capacity || !flyers.TryAdd(x, z, type != null ? type.scale : 1f, type != null ? type.color : crowdColor))
            {
                return false;
            }

            xs[count] = x;
            targetXs[count] = laneX[lane] + NextSpread() * level.LaneSpread;
            zs[count] = z;
            lanes[count] = lane;
            kinds[count] = type != null ? slot : -1;
            armor[count] = type != null ? type.armor : 0;
            speeds[count] = type != null ? type.speed : 1f;
            count++;
            if (count > PeakInFlight)
            {
                PeakInFlight = count;
            }

            return true;
        }

        // Device-gate levels keep the unit in flight: back to the firing line, same lane. A unit that would have died
        // leaves a dying copy behind, so the gate draws the deaths too.
        private void RemoveOrRecycle(int index, bool dies, Vector2 outward)
        {
            if (level.RecycleUnits)
            {
                if (dies)
                {
                    UnitType type = kinds[index] >= 0 ? slotTypes[kinds[index]] : null;
                    flyers.Throw(xs[index], zs[index], type != null ? type.scale : 1f, type != null ? type.color : crowdColor, outward);
                }

                zs[index] = level.FiringLineZ;
                flyers.Place(index, xs[index], zs[index], 0f);
                return;
            }

            Remove(index, dies, outward);
        }

        // The last unit takes the freed index, here and in the renderer alike.
        private void Remove(int index, bool dies, Vector2 outward)
        {
            if (dies)
            {
                flyers.KillAt(index, outward);
            }
            else
            {
                flyers.RemoveAt(index);
            }

            int last = count - 1;
            xs[index] = xs[last];
            targetXs[index] = targetXs[last];
            zs[index] = zs[last];
            lanes[index] = lanes[last];
            armor[index] = armor[last];
            kinds[index] = kinds[last];
            speeds[index] = speeds[last];
            count = last;
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
                gateLabels[i].SetCharArray(labelBuffer, 0, gate.WriteLabel(labelBuffer));
                gateLabels[i].fontSharedMaterial = gate.IsGood ? goodLabelMaterial : badLabelMaterial;
                gatePanels[i].sharedMaterial = gate.IsGood ? goodGateMaterial : badGateMaterial;
            }
        }
    }
}
