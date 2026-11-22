namespace Claybreak
{
    /// <summary>
    /// This game's crowd limit, its one source of truth (D27 revised 2026-11-22, RT-6t.14): at most this many units
    /// alive in the run and in flight in the assault. The engine takes the limit as data, never as a constant of its
    /// own: the scene builder writes it into the run swarm's and the units-in-flight renderer's capacity (the kit's
    /// <c>SwarmRenderer</c>/<c>LooseSwarmRenderer</c>, built for 2000+), the level generator takes it as a parameter
    /// and makes run gates that never promise past it, and the validation checks every level against it. Another
    /// game on the same engine sets its own number; raising this one means reviewing the camera and layout first
    /// (the user's words: 300 already needs a wide zoom-out).
    /// </summary>
    public static class ArmyLimit
    {
        public const int Units = 300;
    }
}
