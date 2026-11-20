namespace Claybreak
{
    /// <summary>
    /// Cached decimal strings for the counters the HUD and world labels show many times per second
    /// (army, tower and wall health), so a counter change never allocates. Filled once, on first use.
    /// </summary>
    public static class NumberLabels
    {
        public const int CachedMaximum = 4096;

        private static string[] plain;
        private static string[] army;
        private static string[] tower;
        private static string[] brutes;

        public static string Plain(int value)
        {
            return Get(ref plain, string.Empty, value);
        }

        public static string Army(int value)
        {
            return Get(ref army, "ARMY ", value);
        }

        public static string Tower(int value)
        {
            return Get(ref tower, "TOWER ", value);
        }

        public static string Brutes(int value)
        {
            return Get(ref brutes, "BRUTES ", value);
        }

        private static string Get(ref string[] cache, string prefix, int value)
        {
            if (value < 0 || value > CachedMaximum)
            {
                return prefix + value;
            }

            if (cache == null)
            {
                cache = new string[CachedMaximum + 1];
                for (int i = 0; i <= CachedMaximum; i++)
                {
                    cache[i] = prefix + i;
                }
            }

            return cache[value];
        }
    }
}
