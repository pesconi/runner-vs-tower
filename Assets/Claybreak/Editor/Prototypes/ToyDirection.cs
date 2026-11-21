using UnityEngine;

namespace Claybreak.Editor.Prototypes
{
    /// <summary>
    /// Direction v2 prototype: the three candidate looks, as data on the same geometry (palette, light and the
    /// shader's gloss, clay and flat-band amounts). A = "Brinquedo Brilhante" (glossy toy, recommended),
    /// B = "Massinha Macia" (matte hand-pressed clay), C = "Pastel Chapado" (flat two-tone pastel).
    /// </summary>
    public sealed class ToyDirection
    {
        public string Name;
        public Color TrackTop, TrackEdge, Grass, Trunk, Rock, FlowerCenter;
        public Color[] Hills, Leaves, Flowers;
        public Color Soldier, SoldierHelmet, SoldierAccent, SoldierFeet;
        public Color TowerMain, TowerTrim, TowerRoof, Window, Door, Cannon;
        public Color GateFrame, GlassGood, GlassBad, LabelShadow;
        public Color Zenith, Horizon, AmbientSky, AmbientGround, Sun, Shadow;
        public float SunIntensity, Gloss, Clay, Bands, Rim;

        public static readonly ToyDirection GlossyToy = new ToyDirection
        {
            Name = "A Brinquedo Brilhante",
            TrackTop = Hex("FFD08A"), TrackEdge = Hex("F0784E"), Grass = Hex("79D896"), Trunk = Hex("C98A5E"), Rock = Hex("F3A57C"),
            FlowerCenter = Hex("FFB83D"),
            Hills = new[] { Hex("57CF9C"), Hex("97E187"), Hex("3DB7A4") },
            Leaves = new[] { Hex("4FC98A"), Hex("2FB5A8"), Hex("FFA43A") },
            Flowers = new[] { Hex("FFFFFF"), Hex("FFE066"), Hex("FFFFFF") },
            Soldier = Hex("2F6BFF"), SoldierAccent = Hex("FFC83A"), SoldierFeet = Hex("1C2C70"), SoldierHelmet = Hex("1E4FE0"),
            TowerMain = Hex("F2664E"), TowerTrim = Hex("FFEBD2"), TowerRoof = Hex("33366E"), Window = Hex("232048"), Door = Hex("6E3A2C"), Cannon = Hex("2A2C45"),
            GateFrame = Hex("FFFFFF"), GlassGood = new Color(0.1f, 0.65f, 1f, 0.66f), GlassBad = new Color(1f, 0.22f, 0.42f, 0.66f), LabelShadow = new Color(0.08f, 0.1f, 0.3f, 1f),
            Zenith = Hex("63BEFF"), Horizon = Hex("E6F6FF"), AmbientSky = Hex("A7C4EC"), AmbientGround = Hex("8E6E5E"),
            Sun = Hex("FFF2DE"), Shadow = new Color(0.25f, 0.12f, 0.2f, 0.38f),
            SunIntensity = 0.95f, Gloss = 1f, Clay = 0f, Bands = 0f, Rim = 0.6f
        };

        public static readonly ToyDirection SoftClay = new ToyDirection
        {
            Name = "B Massinha Macia",
            TrackTop = Hex("E8C79A"), TrackEdge = Hex("C2714F"), Grass = Hex("9CBA78"), Trunk = Hex("9A6B4C"), Rock = Hex("C98F72"),
            FlowerCenter = Hex("D9A04A"),
            Hills = new[] { Hex("8FB27A"), Hex("B3C88A"), Hex("7DA48B") },
            Leaves = new[] { Hex("E39A8A"), Hex("7FB8A6"), Hex("E8B96A") },
            Flowers = new[] { Hex("F4EBDD"), Hex("E8C46A"), Hex("E39A8A") },
            Soldier = Hex("4A74C9"), SoldierAccent = Hex("E8B96A"), SoldierFeet = Hex("3A3F66"), SoldierHelmet = Hex("3A5FAE"),
            TowerMain = Hex("D0674F"), TowerTrim = Hex("F1E2C8"), TowerRoof = Hex("4A4060"), Window = Hex("3E3040"), Door = Hex("6B3B2C"), Cannon = Hex("3A3440"),
            GateFrame = Hex("F4EBDD"), GlassGood = new Color(0.25f, 0.6f, 0.78f, 0.68f), GlassBad = new Color(0.82f, 0.3f, 0.3f, 0.68f), LabelShadow = new Color(0.2f, 0.12f, 0.1f, 1f),
            Zenith = Hex("A9C4DC"), Horizon = Hex("F6E6CF"), AmbientSky = Hex("B9B4C4"), AmbientGround = Hex("8C6E5A"),
            Sun = Hex("FFE9CC"), Shadow = new Color(0.3f, 0.15f, 0.12f, 0.38f),
            SunIntensity = 0.95f, Gloss = 0.12f, Clay = 1f, Bands = 0f, Rim = 0.2f
        };

        public static readonly ToyDirection FlatPastel = new ToyDirection
        {
            Name = "C Pastel Chapado",
            TrackTop = Hex("FBDCC0"), TrackEdge = Hex("FFB3A1"), Grass = Hex("BFEBCB"), Trunk = Hex("E0B49A"), Rock = Hex("FFC9B0"),
            FlowerCenter = Hex("FFE29A"),
            Hills = new[] { Hex("B5E6D2"), Hex("D6F0B8"), Hex("A9D8E8") },
            Leaves = new[] { Hex("FFC2D6"), Hex("A8E6E0"), Hex("FFE29A") },
            Flowers = new[] { Hex("FFFFFF"), Hex("FFE29A"), Hex("FFC2D6") },
            Soldier = Hex("7C9BFF"), SoldierAccent = Hex("FFD66B"), SoldierFeet = Hex("5B6BB3"), SoldierHelmet = Hex("6584F0"),
            TowerMain = Hex("FF9C9C"), TowerTrim = Hex("FFF6EA"), TowerRoof = Hex("8E8AC8"), Window = Hex("6A6496"), Door = Hex("C98A7A"), Cannon = Hex("6A6496"),
            GateFrame = Hex("FFFFFF"), GlassGood = new Color(0.4f, 0.78f, 1f, 0.68f), GlassBad = new Color(1f, 0.48f, 0.6f, 0.68f), LabelShadow = new Color(0.3f, 0.3f, 0.5f, 1f),
            Zenith = Hex("C9D6FF"), Horizon = Hex("FFF0EA"), AmbientSky = Hex("B9BCD8"), AmbientGround = Hex("A8968E"),
            Sun = Hex("FFFFFF"), Shadow = new Color(0.45f, 0.35f, 0.5f, 0.25f),
            SunIntensity = 0.5f, Gloss = 0f, Clay = 0f, Bands = 1f, Rim = 0f
        };

        public static ToyDirection[] All => new[] { GlossyToy, SoftClay, FlatPastel };

        private static Color Hex(string rgb)
        {
            ColorUtility.TryParseHtmlString("#" + rgb, out Color color);
            return color;
        }
    }
}
