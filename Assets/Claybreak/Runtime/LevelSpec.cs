using System;

namespace Claybreak
{
    /// <summary>
    /// Every number of one level as plain data (RT-6): what content writes into a <see cref="ClaybreakLevel"/>
    /// asset, whether a level is hand-made or produced by <see cref="LevelGenerator"/>. The defaults are the
    /// house defaults of a played level; a spec states only what differs.
    /// </summary>
    [Serializable]
    public sealed class LevelSpec
    {
        public const int DefaultUnitsInFlight = ArmyLimit.Units;

        public string title = "LEVEL";

        // Run
        public int startCount = 10;
        [UnityEngine.Tooltip("Run speed (m/s). World content states the course pace and RunPace scales it once when the levels are written.")]
        public float runSpeed = 5.5f;
        public RunGatePair[] runGates = new RunGatePair[0];
        public CratePlacement[] crates = new CratePlacement[0];
        public float firingLineZ = 80f;

        // Assault
        public float launchRate = 12f;
        public float unitSpeed = 9f;
        public float laneSpread = 0.7f;
        public int maximumUnitsInFlight = DefaultUnitsInFlight;
        public float assaultGateDistance = 9f;
        public GateSpec[] assaultGates =
        {
            new GateSpec(GateOperation.Multiply, 2),
            new GateSpec(GateOperation.Divide, 2),
            new GateSpec(GateOperation.Multiply, 3)
        };

        public bool autoLaunch;
        public bool recycleUnits;

        // Tower and boss
        public TowerKind tower = TowerKind.Keep;
        public float towerDistance = 32f;
        public int towerHealth = 250;
        public int floors = 1;
        public int damagePerUnit = 1;
        public BossRule bossRule = BossRule.None;

        // Defenses
        public bool wallEnabled;
        public int wallLane = 2;
        public float wallDistance = 20f;
        public int wallHealth = 60;

        public bool cannonEnabled = true;
        public float cannonInterval = 2f;
        public float projectileFlightTime = 0.9f;
        public float blastRadius = 1.1f;
        public int maximumProjectiles = 4;

        public bool sweeperEnabled;
        public float sweeperDistance = 15f;
        public float sweeperPeriod = 3.2f;

        public bool shieldEnabled;
        public int shieldLane;
        public float shieldDistance = 26f;
        public int shieldHealth = 60;
        public float shieldBlock = 0.5f;

        public int coinReward = 40;

        public float TowerZ => firingLineZ + towerDistance;

        /// <summary>A copy whose plain numbers can change without touching this spec (the arrays are shared, not copied).</summary>
        public LevelSpec ShallowCopy() => (LevelSpec)MemberwiseClone();
    }
}
