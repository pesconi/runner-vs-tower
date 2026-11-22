using System.Collections.Generic;

namespace Claybreak
{
    /// <summary>
    /// The save validator (RT-5 acceptance): the rules every save must keep after it is read, migrated and
    /// normalized against the catalog. The game itself never needs it (normalization already repairs); the
    /// editor validator runs it over the fixture saves (Editor/SaveFixtures) and the tests over every
    /// migration and purchase path, so a format or catalog change that breaks a real player's save fails
    /// before it ships.
    /// </summary>
    public static class SaveValidator
    {
        /// <summary>Adds one line per broken rule to <paramref name="problems"/>; true when there is none.</summary>
        public static bool Validate(SaveData save, ClaybreakCatalog catalog, List<string> problems)
        {
            int before = problems.Count;
            if (save == null)
            {
                problems.Add("the save is null.");
                return false;
            }

            Check(save.version == SaveData.CurrentVersion, $"version {save.version} is not {SaveData.CurrentVersion}.", problems);
            Check(save.coins >= 0 && save.coins <= Progression.MaximumCoins, $"coins {save.coins} out of range.", problems);
            Check(save.levelsWon >= 0 && save.levelsLost >= 0 && save.levelsWon <= Progression.MaximumCount
                && save.levelsLost <= Progression.MaximumCount, "win or loss count out of range.", problems);
            Check(save.firstDay >= 0 && save.dailyLastDay >= 0 && save.dailyClaims >= 0 && save.dailyClaims <= Progression.MaximumCount,
                "first day or daily reward fields out of range.", problems);

            int worldCount = catalog.Worlds.Count;
            Check(save.worlds != null && save.worlds.Length == worldCount, "one world entry per catalog world is expected.", problems);
            for (int w = 0; save.worlds != null && w < save.worlds.Length && w < worldCount; w++)
            {
                WorldSave entry = save.worlds[w];
                ClaybreakWorld world = catalog.Worlds[w].world;
                if (entry == null)
                {
                    problems.Add($"worlds[{w}] is null.");
                    continue;
                }

                Check(entry.id == world.WorldId, $"worlds[{w}] is '{entry.id}', expected '{world.WorldId}'.", problems);
                Check(entry.nextLevel >= 0 && entry.nextLevel < world.LevelCount, $"{entry.id}: next level {entry.nextLevel} out of range.", problems);
                Check(entry.loop >= 0 && entry.loop <= Progression.MaximumLoop, $"{entry.id}: loop {entry.loop} out of range.", problems);
                Check(entry.stars != null && entry.stars.Length == world.LevelCount, $"{entry.id}: one star entry per level is expected.", problems);
                for (int i = 0; entry.stars != null && i < entry.stars.Length; i++)
                {
                    Check(entry.stars[i] >= 0 && entry.stars[i] <= 3, $"{entry.id}: stars[{i}] = {entry.stars[i]}.", problems);
                }
            }

            int selected = catalog.WorldIndexOf(save.world);
            Check(selected >= 0, $"selected world '{save.world}' is not in the catalog.", problems);

            Check(save.upgrades != null && save.upgrades.Length == catalog.Upgrades.Count, "one upgrade entry per track is expected.", problems);
            for (int i = 0; save.upgrades != null && i < save.upgrades.Length; i++)
            {
                int track = catalog.UpgradeIndexOf(save.upgrades[i].id);
                Check(track >= 0, $"upgrade '{save.upgrades[i].id}' is not in the catalog.", problems);
                if (track >= 0)
                {
                    int steps = catalog.Upgrades[track].steps.Length;
                    Check(save.upgrades[i].level >= 0 && save.upgrades[i].level <= steps,
                        $"upgrade '{save.upgrades[i].id}' at step {save.upgrades[i].level} of {steps}.", problems);
                }
            }

            CheckIds(save.units, id => catalog.UnitIndexOf(id) >= 0, "unit", problems);
            CheckIds(save.loadout, id => System.Array.IndexOf(save.units ?? new string[0], id) >= 0, "equipped unit (not owned)", problems);
            Check(save.loadout != null && save.loadout.Length <= catalog.LoadoutSlots, "more units equipped than loadout slots.", problems);
            CheckIds(save.skins, id => catalog.SkinIndexOf(id) >= 0, "skin", problems);
            Check(save.skins != null && System.Array.IndexOf(save.skins, catalog.Skins[0].id) >= 0, "the free skin is not owned.", problems);
            Check(save.skins != null && System.Array.IndexOf(save.skins, save.skin) >= 0, $"worn skin '{save.skin}' is not owned.", problems);
            Check(catalog.ShapeIndexOf(save.shape) >= 0, $"crowd shape '{save.shape}' is not in the catalog.", problems);
            return problems.Count == before;
        }

        private static void CheckIds(string[] ids, System.Func<string, bool> valid, string what, List<string> problems)
        {
            if (ids == null)
            {
                problems.Add($"the {what} list is null.");
                return;
            }

            var seen = new HashSet<string>();
            foreach (string id in ids)
            {
                Check(!string.IsNullOrEmpty(id) && valid(id), $"{what} '{id}' is not valid.", problems);
                Check(seen.Add(id ?? string.Empty), $"{what} '{id}' is listed twice.", problems);
            }
        }

        private static void Check(bool ok, string problem, List<string> problems)
        {
            if (!ok)
            {
                problems.Add(problem);
            }
        }
    }
}
