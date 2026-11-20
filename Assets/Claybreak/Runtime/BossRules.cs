namespace Claybreak
{
    /// <summary>What the player is told about each boss rule (GDD 4.4): when the boss appears and when a floor falls.</summary>
    public static class BossRules
    {
        public static string Intro(BossRule rule)
        {
            switch (rule)
            {
                case BossRule.ShiftGates: return "BOSS! EACH FLOOR SHIFTS THE GATES";
                case BossRule.ShieldReturns: return "BOSS! EACH FLOOR RAISES THE SHIELD";
                case BossRule.Overheat: return "BOSS! EACH FLOOR HEATS THE GUNS";
                case BossRule.TurnGate: return "BOSS! EACH FLOOR TURNS A GATE";
                default: return "BOSS!";
            }
        }

        public static string FloorBanner(BossRule rule)
        {
            switch (rule)
            {
                case BossRule.ShiftGates: return "FLOOR DOWN! GATES SHIFTED";
                case BossRule.ShieldReturns: return "FLOOR DOWN! THE SHIELD IS BACK";
                case BossRule.Overheat: return "FLOOR DOWN! THE GUNS RUN HOT";
                case BossRule.TurnGate: return "FLOOR DOWN! A GATE TURNED";
                default: return "FLOOR DOWN!";
            }
        }
    }
}
