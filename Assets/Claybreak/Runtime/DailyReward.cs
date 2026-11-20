using System;

namespace Claybreak
{
    /// <summary>
    /// The daily reward (D5): one claim per local calendar day, coins from a fixed cycle that only moves forward
    /// on a claim, so missing days never resets anything. Local only (no server): a claim is due on any calendar
    /// day other than the last claim's, so a phone clock moved forward and back can give an extra claim (accepted:
    /// the game has no ranking) but can never lock the gift until a future date. Days are counted from 1970-01-01
    /// in local time; 0 in the save means "never".
    /// </summary>
    public static class DailyReward
    {
        /// <summary>Ceiling of one claim (a hand-edited asset can never pay more).</summary>
        public const int MaximumClaim = 10_000;

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1);

        /// <summary>Today's day number in local time.</summary>
        public static int Today()
        {
            return DayOf(DateTime.Now);
        }

        public static int DayOf(DateTime localTime)
        {
            return (int)(localTime.Date - Epoch).TotalDays;
        }

        /// <summary>True when a claim is due today.</summary>
        public static bool CanClaim(SaveData save, int today)
        {
            return save != null && today > 0 && today != save.dailyLastDay;
        }

        /// <summary>The coins the next claim pays.</summary>
        public static int NextAmount(SaveData save, int[] cycle)
        {
            if (save == null || cycle == null || cycle.Length == 0)
            {
                return 0;
            }

            int index = (int)((uint)save.dailyClaims % (uint)cycle.Length);
            return Math.Max(0, Math.Min(cycle[index], MaximumClaim));
        }

        /// <summary>Whole days since the first launch (0 on the first day), for D1/D7.</summary>
        public static int DaysSinceInstall(SaveData save, int today)
        {
            return save == null || save.firstDay <= 0 ? 0 : Math.Max(0, today - save.firstDay);
        }
    }
}
