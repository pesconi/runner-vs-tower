using System;
using System.Collections.Generic;
using UnityEngine;

namespace Claybreak
{
    /// <summary>A pair of run gates at one distance: the leader passes through the side it is on.</summary>
    [Serializable]
    public struct RunGatePair
    {
        [Min(1f)] public float z;
        public GateSpec left;
        public GateSpec right;
    }

    /// <summary>
    /// One prototype level as data (RT-2): the short run, the assault field, the tower and its two
    /// defenses. Every limit the device gate depends on (units, projectiles, launch rate) lives here,
    /// so a failed gate is answered by changing data, not code.
    /// </summary>
    [CreateAssetMenu(menuName = "Claybreak/Prototype Level")]
    public sealed class PrototypeLevel : ScriptableObject
    {
        public const int LaneCount = 3;

        [Header("Run")]
        [SerializeField, Min(1)] private int startCount = 10;
        [SerializeField, Min(0.5f)] private float runSpeed = 7f;
        [SerializeField] private RunGatePair[] runGates = new RunGatePair[0];
        [Tooltip("Where the run ends and the assault starts (the firing line).")]
        [SerializeField, Min(5f)] private float firingLineZ = 80f;

        [Header("Assault: launch (L1, hold to stream)")]
        [SerializeField, Min(1f)] private float launchRate = 18f;
        [SerializeField, Min(0.5f)] private float unitSpeed = 9f;
        [Tooltip("Sideways spread of a unit inside its lane, in metres (purely visual).")]
        [SerializeField, Range(0f, 1.2f)] private float laneSpread = 0.7f;
        [Tooltip("Hard cap on units in flight; must fit the runner pool.")]
        [SerializeField, Min(1)] private int maximumUnitsInFlight = 300;

        [Header("Assault: gate row (one gate per lane, x or ÷ only)")]
        [SerializeField, Min(1f)] private float assaultGateDistance = 9f;
        [SerializeField] private GateSpec[] assaultGates =
        {
            new GateSpec(GateOperation.Multiply, 2),
            new GateSpec(GateOperation.Divide, 2),
            new GateSpec(GateOperation.Multiply, 3)
        };

        [Header("Device gate only (never in a played level)")]
        [Tooltip("Starts by itself and launches without input, round robin over the lanes.")]
        [SerializeField] private bool autoLaunch;
        [Tooltip("A unit that hits the tower or dies is sent back to the firing line instead of the pool, so the units in flight stay at the measured peak.")]
        [SerializeField] private bool recycleUnits;

        [Header("Tower")]
        [Tooltip("Distance from the firing line to the tower's front face.")]
        [SerializeField, Min(5f)] private float towerDistance = 32f;
        [SerializeField, Min(1)] private int towerHealth = 250;
        [SerializeField, Min(1)] private int damagePerUnit = 1;

        [Header("Defense: wall")]
        [SerializeField, Range(0, LaneCount - 1)] private int wallLane = 2;
        [Tooltip("Distance from the firing line to the wall.")]
        [SerializeField, Min(2f)] private float wallDistance = 20f;
        [SerializeField, Min(1)] private int wallHealth = 60;

        [Header("Defense: cannon")]
        [SerializeField, Min(0.05f)] private float cannonInterval = 1.6f;
        [Tooltip("Seconds the shadow warns before impact (GDD 4.3: 0.5-1 s).")]
        [SerializeField, Range(0.3f, 2f)] private float projectileFlightTime = 0.9f;
        [SerializeField, Min(0.2f)] private float blastRadius = 1.4f;
        [Tooltip("Projectiles in the air at once: the pool size, preallocated by the scene builder.")]
        [SerializeField, Range(1, 32)] private int maximumProjectiles = 4;

        public int StartCount => startCount;
        public float RunSpeed => runSpeed;
        public IReadOnlyList<RunGatePair> RunGates => runGates;
        public float FiringLineZ => firingLineZ;
        public float LaunchRate => launchRate;
        public float UnitSpeed => unitSpeed;
        public float LaneSpread => laneSpread;
        public int MaximumUnitsInFlight => maximumUnitsInFlight;
        public float AssaultGateZ => firingLineZ + assaultGateDistance;
        public IReadOnlyList<GateSpec> AssaultGates => assaultGates;
        public float TowerZ => firingLineZ + towerDistance;
        public bool AutoLaunch => autoLaunch;
        public bool RecycleUnits => recycleUnits;
        public bool IsDeviceGateLevel => autoLaunch || recycleUnits;
        public int TowerHealth => towerHealth;
        public int DamagePerUnit => damagePerUnit;
        public int WallLane => wallLane;
        public float WallZ => firingLineZ + wallDistance;
        public int WallHealth => wallHealth;
        public float CannonInterval => cannonInterval;
        public float ProjectileFlightTime => projectileFlightTime;
        public float BlastRadius => blastRadius;
        public int MaximumProjectiles => maximumProjectiles;

        /// <summary>Checks the level against the pool it will run on. Used by the validator and at start.</summary>
        public bool TryValidate(int poolCapacity, out string error)
        {
            int crowdMaximum = poolCapacity + 1;
            if (startCount > crowdMaximum)
            {
                error = $"startCount {startCount} exceeds the crowd maximum {crowdMaximum}.";
                return false;
            }

            if (maximumUnitsInFlight > poolCapacity)
            {
                error = $"maximumUnitsInFlight {maximumUnitsInFlight} exceeds the pool capacity {poolCapacity}.";
                return false;
            }

            float previous = 0f;
            for (int i = 0; i < runGates.Length; i++)
            {
                if (runGates[i].z <= previous + 4f || runGates[i].z >= firingLineZ - 4f)
                {
                    error = $"runGates[{i}].z must be increasing, at least 4 m apart and before the firing line.";
                    return false;
                }

                if (runGates[i].left.value < 1 || runGates[i].right.value < 1)
                {
                    error = $"runGates[{i}] operands must be at least 1.";
                    return false;
                }

                previous = runGates[i].z;
            }

            if (assaultGates == null || assaultGates.Length != LaneCount)
            {
                error = $"assaultGates needs exactly {LaneCount} gates, one per lane.";
                return false;
            }

            for (int i = 0; i < assaultGates.Length; i++)
            {
                if (!GateMath.IsStreamOperation(assaultGates[i].operation) || assaultGates[i].value < 1)
                {
                    error = $"assaultGates[{i}] must multiply or divide by at least 1.";
                    return false;
                }
            }

            if (assaultGateDistance >= wallDistance || wallDistance >= towerDistance - 2f)
            {
                error = "Order along the lane must be: firing line, gate row, wall, tower (2 m before the tower).";
                return false;
            }

            if (wallLane < 0 || wallLane >= LaneCount)
            {
                error = "wallLane is out of range.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
