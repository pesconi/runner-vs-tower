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
    /// collides with another; it only meets the gate row, the wall and the tower, by distance along its
    /// lane (R-D4). Units come from the kit's RunnerPool and go back to it when they hit or die.
    /// </summary>
    public sealed class AssaultField : MonoBehaviour
    {
        private const float SteerResponse = 6f;

        [SerializeField] private RunnerPool runnerPool;
        [SerializeField] private AssaultTower tower;
        [SerializeField] private AssaultWall wall;
        [SerializeField] private Transform[] gateVisuals = new Transform[PrototypeLevel.LaneCount];
        [SerializeField] private TextMesh[] gateLabels = new TextMesh[PrototypeLevel.LaneCount];
        [SerializeField] private Renderer[] gatePanels = new Renderer[PrototypeLevel.LaneCount];
        [SerializeField] private Material goodGateMaterial;
        [SerializeField] private Material badGateMaterial;

        private PrototypeLevel level;
        private readonly float[] laneX = new float[PrototypeLevel.LaneCount];
        private readonly int[] gatePassed = new int[PrototypeLevel.LaneCount];
        private readonly int[] laneCounts = new int[PrototypeLevel.LaneCount];
        private readonly float[] laneZSum = new float[PrototypeLevel.LaneCount];
        private RunnerUnit[] units;
        private Transform[] transforms;
        private float[] xs;
        private float[] targetXs;
        private float[] zs;
        private int[] lanes;
        private int count;
        private uint spreadSeed = 1u;
        private Vector3 unitScale = Vector3.one * 0.5f;

        public int InFlight => count;
        public int PeakInFlight { get; private set; }
        public int Capacity => units == null ? 0 : units.Length;
        public bool Running { get; set; }

        public bool Configure(PrototypeLevel levelData, Renderer track, ClaybreakTheme theme, CharacterController leader,
            Vector3 followerScale)
        {
            if (runnerPool == null || tower == null || wall == null || gateVisuals == null
                || gateVisuals.Length != PrototypeLevel.LaneCount || gateLabels.Length != PrototypeLevel.LaneCount
                || gatePanels.Length != PrototypeLevel.LaneCount || goodGateMaterial == null || badGateMaterial == null
                || levelData == null || track == null)
            {
                Debug.LogError("AssaultField needs the pool, tower, wall, one gate per lane, a level and the track.", this);
                return false;
            }

            level = levelData;
            unitScale = followerScale;
            Bounds bounds = track.bounds;
            float laneWidth = bounds.size.x / PrototypeLevel.LaneCount;
            for (int i = 0; i < PrototypeLevel.LaneCount; i++)
            {
                laneX[i] = bounds.min.x + laneWidth * (i + 0.5f);
            }

            int capacity = Mathf.Min(level.MaximumUnitsInFlight, runnerPool.Capacity);
            units = new RunnerUnit[capacity];
            transforms = new Transform[capacity];
            xs = new float[capacity];
            targetXs = new float[capacity];
            zs = new float[capacity];
            lanes = new int[capacity];
            PlaceDefenses();
            return DressIdleUnits(theme, leader);
        }

        public float GetLaneX(int lane)
        {
            return laneX[Mathf.Clamp(lane, 0, PrototypeLevel.LaneCount - 1)];
        }

        public int GetNearestLane(float x)
        {
            int best = 0;
            for (int i = 1; i < PrototypeLevel.LaneCount; i++)
            {
                if (Mathf.Abs(x - laneX[i]) < Mathf.Abs(x - laneX[best]))
                {
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Takes a free pool unit and sends it down <paramref name="lane"/> from <paramref name="from"/>.</summary>
        public bool TryLaunch(int lane, Vector3 from)
        {
            return TrySpawn(lane, from.x, from.z);
        }

        /// <summary>Kills every unit within <paramref name="radius"/> of <paramref name="center"/> (cannon blast).</summary>
        public int KillInRadius(Vector3 center, float radius)
        {
            float radiusSquared = radius * radius;
            int killed = 0;
            for (int i = count - 1; i >= 0; i--)
            {
                float dx = xs[i] - center.x;
                float dz = zs[i] - center.z;
                if (dx * dx + dz * dz <= radiusSquared)
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
            for (int i = 1; i < PrototypeLevel.LaneCount; i++)
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

        /// <summary>Sends every unit in flight back to the pool and rearms the gate row and defenses.</summary>
        public void ResetField()
        {
            for (int i = count - 1; i >= 0; i--)
            {
                Remove(i);
            }

            Array.Clear(gatePassed, 0, gatePassed.Length);
            PeakInFlight = 0;
            Running = false;
            tower.ResetTower(level.TowerHealth);
            wall.ResetWall(level.WallHealth);
        }

        private void Update()
        {
            if (!Running || count == 0)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            float step = level.UnitSpeed * deltaTime;
            float steer = 1f - Mathf.Exp(-SteerResponse * deltaTime);
            float gateZ = level.AssaultGateZ;
            float wallZ = level.WallZ;
            float towerZ = level.TowerZ;
            int wallLane = level.WallLane;
            int lostAtWall = 0;

            // Backwards, so a removal (swap with the last) never skips a unit; clones appended this
            // frame sit past the start index and move from the next frame.
            for (int i = count - 1; i >= 0; i--)
            {
                float previousZ = zs[i];
                float z = previousZ + step;
                int lane = lanes[i];

                if (previousZ < gateZ && z >= gateZ)
                {
                    int leaving = GateMath.UnitsOut(level.AssaultGates[lane], ++gatePassed[lane]);
                    if (leaving == 0)
                    {
                        Remove(i);
                        continue;
                    }

                    for (int clone = 1; clone < leaving; clone++)
                    {
                        // Clones trail the original a little (a burst, not a stack) but never behind the
                        // gate, or they would cross it again.
                        if (!TrySpawn(lane, xs[i], Mathf.Max(gateZ, z - 0.35f * clone)))
                        {
                            break;
                        }
                    }
                }

                if (lane == wallLane && wall.IsStanding && z >= wallZ)
                {
                    wall.Hit();
                    Remove(i);
                    lostAtWall++;
                    continue;
                }

                if (z >= towerZ)
                {
                    tower.Hit(level.DamagePerUnit);
                    RemoveOrRecycle(i);
                    continue;
                }

                float x = xs[i] + (targetXs[i] - xs[i]) * steer;
                xs[i] = x;
                zs[i] = z;
                transforms[i].position = new Vector3(x, transform.position.y, z);
                units[i].AnimateRun(step);
            }

            if (lostAtWall > 0)
            {
                RunnerDeathEffects.Play(new Vector3(laneX[wallLane], transform.position.y, wallZ - 0.5f),
                    Mathf.Min(lostAtWall, 3));
            }
        }

        private bool TrySpawn(int lane, float x, float z)
        {
            if (count >= units.Length || !runnerPool.TryGet(out RunnerUnit unit))
            {
                return false;
            }

            Transform unitTransform = unit.transform;
            unitTransform.localScale = unitScale;
            unitTransform.rotation = Quaternion.identity;
            unitTransform.position = new Vector3(x, transform.position.y, z);
            units[count] = unit;
            transforms[count] = unitTransform;
            xs[count] = x;
            targetXs[count] = laneX[lane] + NextSpread() * level.LaneSpread;
            zs[count] = z;
            lanes[count] = lane;
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
            int last = count - 1;
            units[index] = units[last];
            transforms[index] = transforms[last];
            xs[index] = xs[last];
            targetXs[index] = targetXs[last];
            zs[index] = zs[last];
            lanes[index] = lanes[last];
            units[last] = null;
            transforms[last] = null;
            count = last;
            runnerPool.Release(unit);
        }

        // Deterministic spread in [-1, 1] (xorshift), so a run looks the same every time and costs nothing.
        private float NextSpread()
        {
            spreadSeed ^= spreadSeed << 13;
            spreadSeed ^= spreadSeed >> 17;
            spreadSeed ^= spreadSeed << 5;
            return (spreadSeed & 0xFFFF) / 32767.5f - 1f;
        }

        private void PlaceDefenses()
        {
            for (int i = 0; i < PrototypeLevel.LaneCount; i++)
            {
                Vector3 position = gateVisuals[i].position;
                gateVisuals[i].position = new Vector3(laneX[i], position.y, level.AssaultGateZ);
                GateSpec gate = level.AssaultGates[i];
                gateLabels[i].text = gate.Label;
                gatePanels[i].sharedMaterial = gate.IsGood ? goodGateMaterial : badGateMaterial;
            }

            Vector3 wallPosition = wall.transform.position;
            wall.transform.position = new Vector3(laneX[level.WallLane], wallPosition.y, level.WallZ);
            Vector3 towerPosition = tower.transform.position;
            tower.transform.position = new Vector3(towerPosition.x, towerPosition.y, level.TowerZ);
        }

        // Every pool unit wears the crowd's clay look before the run starts, so a unit cloned by a xN gate
        // (never part of the crowd) looks like the others without dressing it mid-assault.
        private bool DressIdleUnits(ClaybreakTheme theme, CharacterController leader)
        {
            int available = runnerPool.AvailableCount;
            RunnerUnit[] taken = new RunnerUnit[available];
            bool dressed = true;
            for (int i = 0; i < available; i++)
            {
                runnerPool.TryGet(out taken[i]);
                taken[i].transform.localScale = unitScale;
                dressed &= taken[i].TryApplyColorLook(theme.Roster, ClaybreakTheme.CrowdColorId, theme.CrowdColor, leader);
            }

            for (int i = 0; i < available; i++)
            {
                runnerPool.Release(taken[i]);
            }

            return dressed;
        }
    }
}
