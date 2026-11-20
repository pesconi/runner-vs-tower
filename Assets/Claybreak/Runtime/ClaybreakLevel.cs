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

    /// <summary>A special-unit crate on the run: the leader picks it up by passing on its side.</summary>
    [Serializable]
    public struct CratePlacement
    {
        [Min(1f)] public float z;
        [Tooltip("Track side: false = left half, true = right half.")]
        public bool right;
        [Min(1)] public int units;
    }

    /// <summary>
    /// Which tower body stands at the end of the assault field (each has its own silhouette, built by the
    /// scene builder from the art style chosen for it). Values are stable: new kinds go at the end.
    /// </summary>
    public enum TowerKind
    {
        Keep,
        Forge,
        Boss,
        World2Tower,
        World2Boss,
        World3Tower,
        World3Boss,
        World4Tower,
        World4Boss
    }

    /// <summary>
    /// What a boss does when one of its floors falls (GDD 4.4: one mechanic of its own per boss). A level
    /// with a rule is the world's boss; a plain level has none.
    /// </summary>
    public enum BossRule
    {
        None,
        /// <summary>The gate row turns one lane to the right: the good gate moves away (world 1).</summary>
        ShiftGates,
        /// <summary>The shield generator rises again at full health (needs the shield).</summary>
        ShieldReturns,
        /// <summary>The cannon fires faster and the sweeper swings faster.</summary>
        Overheat,
        /// <summary>The best multiplier of the gate row turns into a divider.</summary>
        TurnGate
    }

    /// <summary>
    /// One level as data (RT-4, RT-6): the short run with its math gates and special-unit crates, the assault
    /// field (gate row), the tower (body, health, floors) and its defenses (wall, cannon, sweeper, shield),
    /// plus the boss rule. Content writes it from a <see cref="LevelSpec"/> (hand-made or generated). Every limit the device gate depends on (units, projectiles, launch rate) lives here,
    /// so a failed gate is answered by changing data, not code.
    /// </summary>
    [CreateAssetMenu(menuName = "Claybreak/Level")]
    public sealed class ClaybreakLevel : ScriptableObject
    {
        public const int LaneCount = 3;
        public const int MaximumFloors = 4;
        public const int MaximumCrates = 4;

        [Header("Identity")]
        [SerializeField] private string title = "LEVEL";

        [Header("Run")]
        [SerializeField, Min(1)] private int startCount = 10;
        [SerializeField, Min(0.5f)] private float runSpeed = 7f;
        [SerializeField] private RunGatePair[] runGates = new RunGatePair[0];
        [Tooltip("Special-unit crates (only shown once the special unit is unlocked).")]
        [SerializeField] private CratePlacement[] crates = new CratePlacement[0];
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
        [SerializeField] private TowerKind tower = TowerKind.Keep;
        [Tooltip("Distance from the firing line to the tower's front face.")]
        [SerializeField, Min(5f)] private float towerDistance = 32f;
        [SerializeField, Min(1)] private int towerHealth = 250;
        [Tooltip("Health is split in this many floors; each floor that falls drops a block of the tower.")]
        [SerializeField, Range(1, MaximumFloors)] private int floors = 1;
        [SerializeField, Min(1)] private int damagePerUnit = 1;

        [Header("Boss rule (GDD 4.4)")]
        [Tooltip("What happens when a floor falls; a level with a rule is the world's boss.")]
        [SerializeField] private BossRule bossRule;

        [Header("Defense: wall")]
        [SerializeField] private bool wallEnabled = true;
        [SerializeField, Range(0, LaneCount - 1)] private int wallLane = 2;
        [Tooltip("Distance from the firing line to the wall.")]
        [SerializeField, Min(2f)] private float wallDistance = 20f;
        [SerializeField, Min(1)] private int wallHealth = 60;

        [Header("Defense: cannon")]
        [SerializeField] private bool cannonEnabled = true;
        [SerializeField, Min(0.05f)] private float cannonInterval = 1.6f;
        [Tooltip("Seconds the shadow warns before impact (GDD 4.3: 0.5-1 s).")]
        [SerializeField, Range(0.3f, 2f)] private float projectileFlightTime = 0.9f;
        [SerializeField, Min(0.2f)] private float blastRadius = 1.4f;
        [Tooltip("Projectiles in the air at once: the pool size, preallocated by the scene builder.")]
        [SerializeField, Range(1, 32)] private int maximumProjectiles = 4;

        [Header("Defense: sweeper")]
        [SerializeField] private bool sweeperEnabled;
        [Tooltip("Distance from the firing line to the sweeper's arm.")]
        [SerializeField, Min(2f)] private float sweeperDistance = 15f;
        [Tooltip("Seconds for the arm to cross the track and come back.")]
        [SerializeField, Min(1f)] private float sweeperPeriod = 3.2f;

        [Header("Defense: shield (GDD 4.3, fourth type)")]
        [Tooltip("A generator on one lane; while it stands the tower ignores part of every hit.")]
        [SerializeField] private bool shieldEnabled;
        [SerializeField, Range(0, LaneCount - 1)] private int shieldLane;
        [Tooltip("Distance from the firing line to the generator.")]
        [SerializeField, Min(2f)] private float shieldDistance = 26f;
        [SerializeField, Min(1)] private int shieldHealth = 60;
        [Tooltip("Share of each tower hit the shield absorbs while the generator stands.")]
        [SerializeField, Range(0.1f, 0.9f)] private float shieldBlock = 0.5f;

        [Header("Reward")]
        [SerializeField, Min(0)] private int coinReward = 40;

        public string Title => title;
        public int StartCount => startCount;
        public float RunSpeed => runSpeed;
        public IReadOnlyList<RunGatePair> RunGates => runGates;
        public IReadOnlyList<CratePlacement> Crates => crates;
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
        public TowerKind Tower => tower;
        public bool IsBoss => bossRule != BossRule.None;
        public BossRule BossRule => bossRule;
        public int TowerHealth => towerHealth;
        public int Floors => floors;
        public int DamagePerUnit => damagePerUnit;
        public bool WallEnabled => wallEnabled;
        public int WallLane => wallLane;
        public float WallZ => firingLineZ + wallDistance;
        public int WallHealth => wallHealth;
        public bool CannonEnabled => cannonEnabled;
        public float CannonInterval => cannonInterval;
        public float ProjectileFlightTime => projectileFlightTime;
        public float BlastRadius => blastRadius;
        public int MaximumProjectiles => maximumProjectiles;
        public bool SweeperEnabled => sweeperEnabled;
        public float SweeperZ => firingLineZ + sweeperDistance;
        public float SweeperPeriod => sweeperPeriod;
        public bool ShieldEnabled => shieldEnabled;
        public int ShieldLane => shieldLane;
        public float ShieldZ => firingLineZ + shieldDistance;
        public int ShieldHealth => shieldHealth;
        public float ShieldBlock => shieldBlock;
        public int CoinReward => coinReward;

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

            if (crates.Length > MaximumCrates)
            {
                error = $"at most {MaximumCrates} crates per level.";
                return false;
            }

            for (int i = 0; i < crates.Length; i++)
            {
                if (crates[i].z >= firingLineZ - 2f || crates[i].units < 1 || NearGate(crates[i].z))
                {
                    error = $"crates[{i}] must sit before the firing line, 3 m away from any gate pair, with at least 1 unit.";
                    return false;
                }
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

            if (wallEnabled && (assaultGateDistance >= wallDistance || wallDistance >= towerDistance - 2f))
            {
                error = "Order along the lane must be: firing line, gate row, wall, tower (2 m before the tower).";
                return false;
            }

            if (sweeperEnabled && (sweeperDistance <= assaultGateDistance + 1f || sweeperDistance >= towerDistance - 3f))
            {
                error = "The sweeper must sit between the gate row (1 m after it) and the tower (3 m before it).";
                return false;
            }

            if (sweeperEnabled && wallEnabled && Mathf.Abs(sweeperDistance - wallDistance) < 2f)
            {
                error = "The sweeper and the wall need at least 2 m between them.";
                return false;
            }

            if (wallLane < 0 || wallLane >= LaneCount)
            {
                error = "wallLane is out of range.";
                return false;
            }

            if (floors < 1 || floors > MaximumFloors || towerHealth < floors)
            {
                error = $"floors must be 1-{MaximumFloors} and the tower needs at least 1 health per floor.";
                return false;
            }

            if (bossRule != BossRule.None && floors < 2)
            {
                error = "a boss rule acts when a floor falls: it needs at least 2 floors.";
                return false;
            }

            if (bossRule == BossRule.ShieldReturns && !shieldEnabled)
            {
                error = "the boss rule ShieldReturns needs the shield.";
                return false;
            }

            if (shieldEnabled && (shieldLane < 0 || shieldLane >= LaneCount
                || shieldDistance <= assaultGateDistance + 1f || shieldDistance >= towerDistance - 1.5f))
            {
                error = "The shield generator must sit on a lane between the gate row (1 m after it) and the tower (1.5 m before it).";
                return false;
            }

            if (shieldEnabled && wallEnabled && shieldLane == wallLane && Mathf.Abs(shieldDistance - wallDistance) < 2f)
            {
                error = "The shield generator and the wall on the same lane need at least 2 m between them.";
                return false;
            }

            if (shieldEnabled && sweeperEnabled && Mathf.Abs(shieldDistance - sweeperDistance) < 1.5f)
            {
                error = "The shield generator and the sweeper need at least 1.5 m between them.";
                return false;
            }

            error = null;
            return true;
        }

#if UNITY_EDITOR
        /// <summary>Editor only (content and tests): writes every field from <paramref name="spec"/>.</summary>
        public void EditorAssign(LevelSpec spec)
        {
            title = spec.title;
            startCount = spec.startCount;
            runSpeed = spec.runSpeed;
            runGates = spec.runGates == null ? new RunGatePair[0] : (RunGatePair[])spec.runGates.Clone();
            crates = spec.crates == null ? new CratePlacement[0] : (CratePlacement[])spec.crates.Clone();
            firingLineZ = spec.firingLineZ;
            launchRate = spec.launchRate;
            unitSpeed = spec.unitSpeed;
            laneSpread = spec.laneSpread;
            maximumUnitsInFlight = spec.maximumUnitsInFlight;
            assaultGateDistance = spec.assaultGateDistance;
            assaultGates = spec.assaultGates == null ? new GateSpec[0] : (GateSpec[])spec.assaultGates.Clone();
            autoLaunch = spec.autoLaunch;
            recycleUnits = spec.recycleUnits;
            tower = spec.tower;
            towerDistance = spec.towerDistance;
            towerHealth = spec.towerHealth;
            floors = spec.floors;
            damagePerUnit = spec.damagePerUnit;
            bossRule = spec.bossRule;
            wallEnabled = spec.wallEnabled;
            wallLane = spec.wallLane;
            wallDistance = spec.wallDistance;
            wallHealth = spec.wallHealth;
            cannonEnabled = spec.cannonEnabled;
            cannonInterval = spec.cannonInterval;
            projectileFlightTime = spec.projectileFlightTime;
            blastRadius = spec.blastRadius;
            maximumProjectiles = spec.maximumProjectiles;
            sweeperEnabled = spec.sweeperEnabled;
            sweeperDistance = spec.sweeperDistance;
            sweeperPeriod = spec.sweeperPeriod;
            shieldEnabled = spec.shieldEnabled;
            shieldLane = spec.shieldLane;
            shieldDistance = spec.shieldDistance;
            shieldHealth = spec.shieldHealth;
            shieldBlock = spec.shieldBlock;
            coinReward = spec.coinReward;
        }
#endif

        private bool NearGate(float z)
        {
            for (int i = 0; i < runGates.Length; i++)
            {
                if (Mathf.Abs(runGates[i].z - z) < 3f)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
