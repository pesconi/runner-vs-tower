using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Editor
{
    /// <summary>The visual styles a tower body can be built in (RT-4 art direction options).</summary>
    public enum TowerStyle
    {
        /// <summary>Tower option A: stone bastion, square tiers with merlons, banners and torches.</summary>
        StoneBastion,
        /// <summary>Tower option B: iron forge, round tiers split by glowing seams, smoking chimney.</summary>
        IronForge,
        /// <summary>Tower option C: clay ruin, leaning terracotta drums, cracks and moss.</summary>
        ClayRuin,
        /// <summary>Boss option A: twin-towered citadel with a spiked crown.</summary>
        Citadel,
        /// <summary>Boss option B: forge colossus, a furnace drum with a glowing core and two stacks.</summary>
        ForgeColossus,
        /// <summary>Boss option C: clay totem, stacked rings with horns, no face.</summary>
        ClayTotem,
        /// <summary>World 2 tower option: a tarred wooden fort on stilts, palisade and lanterns.</summary>
        StiltFort,
        /// <summary>World 2 boss option: a half-sunken stone bell tower with lit arches and a bell cage.</summary>
        SunkenBell,
        /// <summary>World 3 tower option: a brick mill with saw-tooth roof, two smokestacks and lit floors.</summary>
        SmokestackMill,
        /// <summary>World 3 tower option: a gasholder, steel rings inside a lattice frame under a dome.</summary>
        Gasholder,
        /// <summary>World 3 boss option: a steam press, two steel columns and a crossbeam holding the hammer.</summary>
        PistonHammer
    }

    /// <summary>What the game needs from a built tower body.</summary>
    public struct TowerParts
    {
        public Transform Body;
        /// <summary>The 4 tiers that drop as floors fall, top first.</summary>
        public Transform[] Tiers;
        public Transform Muzzle;
        public float BarHeight;
    }

    /// <summary>
    /// Tower and boss art, built from stylized primitives in code (free, own means: no paid tool, asset or
    /// image service). Each body is a permanent base plus <see cref="AssaultTower.BlocksPerBody"/> tiers
    /// that drop as floors fall, a cannon on the front and the height for the health bar. The local front
    /// face is at z = 0; the body extends to +z. Track width is 8 m.
    /// </summary>
    public static class TowerArt
    {
        /// <summary>
        /// Styles used in the game until the art direction is approved (decision D17, ficha in
        /// docs/decisoes.md). Changing these two lines and rebuilding the scene swaps the art.
        /// </summary>
        public const TowerStyle ChosenTower = TowerStyle.StoneBastion;
        public const TowerStyle ChosenBoss = TowerStyle.Citadel;
        /// <summary>The second tower of the world (not under art review: forge levels).</summary>
        public const TowerStyle SecondTower = TowerStyle.IronForge;

        /// <summary>
        /// The style each tower kind is built in (RT-6: one line per kind; a world's options are compared in
        /// docs/art/rt6 and the chosen one is set here, provisional until the user approves the art).
        /// </summary>
        public static TowerStyle StyleFor(TowerKind kind)
        {
            switch (kind)
            {
                case TowerKind.Keep: return ChosenTower;
                case TowerKind.Forge: return SecondTower;
                case TowerKind.Boss: return ChosenBoss;
                case TowerKind.World2Tower: return WorldContent.ChosenTowerStyle(1);
                case TowerKind.World2Boss: return WorldContent.ChosenBossStyle(1);
                case TowerKind.World3Tower: return WorldContent.ChosenTowerStyle(2);
                case TowerKind.World3Boss: return WorldContent.ChosenBossStyle(2);
                default: throw new System.ArgumentOutOfRangeException(nameof(kind), kind, "no art style for this tower kind");
            }
        }

        public static TowerParts Build(TowerStyle style, Transform parent, ArtPalette p)
        {
            var body = new GameObject(style.ToString()).transform;
            body.SetParent(parent, false);
            var parts = new TowerParts { Body = body, Tiers = new Transform[AssaultTower.BlocksPerBody] };
            switch (style)
            {
                case TowerStyle.StoneBastion: StoneBastion(body, p, ref parts); break;
                case TowerStyle.IronForge: IronForge(body, p, ref parts, 1f); break;
                case TowerStyle.ClayRuin: ClayRuin(body, p, ref parts, 1f); break;
                case TowerStyle.Citadel: Citadel(body, p, ref parts); break;
                case TowerStyle.ForgeColossus: IronForge(body, p, ref parts, 1.3f); ForgeColossusExtras(body, p); break;
                case TowerStyle.ClayTotem: ClayRuin(body, p, ref parts, 1.3f); ClayTotemExtras(body, p, parts); break;
                case TowerStyle.StiltFort: StiltFort(body, p, ref parts); break;
                case TowerStyle.SunkenBell: SunkenBell(body, p, ref parts); break;
                case TowerStyle.SmokestackMill: SmokestackMill(body, p, ref parts); break;
                case TowerStyle.Gasholder: Gasholder(body, p, ref parts); break;
                case TowerStyle.PistonHammer: PistonHammer(body, p, ref parts); break;
                default: throw new System.ArgumentOutOfRangeException(nameof(style), style, "no builder for this style");
            }

            return parts;
        }

        // ---------- tower A: stone bastion ----------

        private static void StoneBastion(Transform body, ArtPalette p, ref TowerParts parts)
        {
            Prim.Box(body, "Plinth", new Vector3(0f, 0.5f, 3f), new Vector3(10f, 1f, 7f), p.StoneDark);
            Prim.Box(body, "FrontWall", new Vector3(0f, 2.6f, 0.9f), new Vector3(9f, 3.4f, 1.6f), p.StoneLight);
            Prim.Box(body, "Keep", new Vector3(0f, 2.6f, 4f), new Vector3(8f, 3.4f, 5f), p.Stone);
            Prim.Box(body, "Gate", new Vector3(0f, 2.1f, 0.05f), new Vector3(2.4f, 2.6f, 0.2f), p.Wood);
            Prim.Box(body, "GateArch", new Vector3(0f, 3.5f, 0.06f), new Vector3(3f, 0.4f, 0.25f), p.StoneDark);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim.Box(body, "Banner", new Vector3(side * 2.6f, 2.8f, 0f), new Vector3(1f, 2.4f, 0.08f), p.Banner);
                Prim.Box(body, "BannerRod", new Vector3(side * 2.6f, 4.05f, 0f), new Vector3(1.3f, 0.12f, 0.12f), p.Wood);
                Prim.Cyl(body, "Torch", new Vector3(side * 4.1f, 3.6f, -0.1f), new Vector3(0.2f, 0.4f, 0.2f), p.Wood);
                Prim.Ball(body, "Flame", new Vector3(side * 4.1f, 4.15f, -0.1f), new Vector3(0.38f, 0.5f, 0.38f), p.Glow);
                Prim.Box(body, "Buttress", new Vector3(side * 4.7f, 1.8f, 1.2f), new Vector3(0.9f, 2.6f, 2.2f), p.StoneDark);
            }

            Cannon(body, p, new Vector3(0f, 4.75f, 0.5f), ref parts);
            float[] widths = { 7.4f, 6.2f, 5f, 3.6f };
            float y = 4.3f;
            for (int i = 0; i < 4; i++)
            {
                Transform tier = Tier(body, i, ref parts);
                float w = widths[i];
                float h = i == 3 ? 1.6f : 1.9f;
                Prim.Box(tier, "Block", new Vector3(0f, y + h * 0.5f, 3.6f), new Vector3(w, h, w * 0.65f), i % 2 == 0 ? p.StoneLight : p.Stone);
                Prim.Box(tier, "Band", new Vector3(0f, y + h - 0.1f, 3.6f), new Vector3(w + 0.3f, 0.25f, w * 0.65f + 0.3f), p.StoneDark);
                for (int c = 0; c < 4; c++)
                {
                    float sx = (c % 2 == 0 ? -1f : 1f) * (w * 0.5f - 0.3f);
                    float sz = 3.6f + (c < 2 ? -1f : 1f) * (w * 0.325f - 0.3f);
                    Prim.Box(tier, "Merlon", new Vector3(sx, y + h + 0.3f, sz), new Vector3(0.6f, 0.6f, 0.6f), p.StoneLight);
                }

                Prim.Box(tier, "Slit", new Vector3(0f, y + h * 0.5f, 3.6f - w * 0.325f - 0.02f), new Vector3(0.3f, 0.8f, 0.05f), p.Glow);
                if (i == 3)
                {
                    // A brazier, not a flag: a war post, not a toy castle (GDD 9; review of D17).
                    Prim.Cyl(tier, "Brazier", new Vector3(0f, y + h + 0.5f, 3.6f), new Vector3(1f, 0.35f, 1f), p.Iron);
                    Prim.Ball(tier, "Fire", new Vector3(0f, y + h + 0.95f, 3.6f), new Vector3(0.9f, 0.6f, 0.9f), p.Glow);
                }

                y += h + 0.15f;
            }

            parts.BarHeight = y + 3.2f;
        }

        // ---------- tower B / boss B: iron forge ----------

        private static void IronForge(Transform body, ArtPalette p, ref TowerParts parts, float scale)
        {
            Prim.Box(body, "Plinth", new Vector3(0f, 0.5f, 3f), new Vector3(10f, 1f, 7f), p.IronWarm);
            Prim.Cyl(body, "Base", new Vector3(0f, 2.4f, 3.6f), new Vector3(7.6f * scale, 1.5f, 7.6f * scale), p.Iron);
            Prim.Box(body, "Door", new Vector3(0f, 2f, 3.6f - 3.8f * scale + 0.1f), new Vector3(2.2f, 2.4f, 0.3f), p.Glow);
            Prim.Box(body, "DoorFrame", new Vector3(0f, 3.4f, 3.6f - 3.8f * scale), new Vector3(2.8f, 0.4f, 0.4f), p.IronWarm);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim.Box(body, "Rivets", new Vector3(side * 2.4f * scale, 2.4f, 3.6f - 3.4f * scale), new Vector3(0.3f, 2.6f, 0.3f), p.IronWarm);
                Prim.Cyl(body, "Pipe", new Vector3(side * 4.4f, 3f, 2.5f), new Vector3(0.6f, 2.8f, 0.6f), p.IronWarm);
                Prim.Ball(body, "PipeGlow", new Vector3(side * 4.4f, 5.9f, 2.5f), new Vector3(0.7f, 0.35f, 0.7f), p.Glow);
            }

            Cannon(body, p, new Vector3(0f, 4.4f, 3.6f - 3.6f * scale), ref parts);
            float[] diameters = { 6.4f, 5.4f, 4.4f, 3.2f };
            float y = 3.9f;
            for (int i = 0; i < 4; i++)
            {
                Transform tier = Tier(body, i, ref parts);
                float d = diameters[i] * scale;
                float h = 1.8f * scale;
                Prim.Cyl(tier, "Seam", new Vector3(0f, y + 0.08f, 3.6f), new Vector3(d + 0.25f, 0.08f, d + 0.25f), p.Glow);
                Prim.Cyl(tier, "Drum", new Vector3(0f, y + h * 0.5f + 0.15f, 3.6f), new Vector3(d, h * 0.5f, d), i % 2 == 0 ? p.Iron : p.IronWarm);
                Prim.Box(tier, "Vent", new Vector3(0f, y + h * 0.55f, 3.6f - d * 0.5f), new Vector3(d * 0.35f, 0.3f, 0.12f), p.GlowHot);
                if (i == 3)
                {
                    Prim.Cyl(tier, "Chimney", new Vector3(0.6f * scale, y + h + 1.4f, 3.9f), new Vector3(0.9f * scale, 1.2f, 0.9f * scale), p.IronWarm);
                    Prim.Cyl(tier, "Ember", new Vector3(0.6f * scale, y + h + 2.65f, 3.9f), new Vector3(0.75f * scale, 0.06f, 0.75f * scale), p.GlowHot);
                }

                y += h + 0.15f;
            }

            parts.BarHeight = y + 3.4f;
        }

        private static void ForgeColossusExtras(Transform body, ArtPalette p)
        {
            // A furnace grate: vertical bars only, so the glow never reads as a face (GDD 2).
            Prim.Box(body, "Core", new Vector3(0f, 6f, -0.3f), new Vector3(2.6f, 1.6f, 0.2f), p.GlowHot);
            for (int bar = -2; bar <= 2; bar++)
            {
                Prim.Box(body, "Grate", new Vector3(bar * 0.55f, 6f, -0.4f), new Vector3(0.2f, 2f, 0.25f), p.Iron);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Prim.Cyl(body, "Stack", new Vector3(side * 4.6f, 6.5f, 5.5f), new Vector3(1.1f, 6f, 1.1f), p.Iron);
                Prim.Cyl(body, "StackGlow", new Vector3(side * 4.6f, 12.55f, 5.5f), new Vector3(0.9f, 0.06f, 0.9f), p.GlowHot);
                Prim.Box(body, "Chain", new Vector3(side * 2.6f, 7.5f, 0.2f), new Vector3(0.12f, 5f, 0.12f), p.Iron, new Vector3(0f, 0f, side * 30f));
            }
        }

        // ---------- tower C / boss C: clay ruin ----------

        private static void ClayRuin(Transform body, ArtPalette p, ref TowerParts parts, float scale)
        {
            Prim.Box(body, "Rubble", new Vector3(0f, 0.4f, 3f), new Vector3(10f, 0.8f, 7f), p.ClayDark);
            Prim.Cyl(body, "Base", new Vector3(0f, 2.3f, 3.6f), new Vector3(8f * scale, 1.5f, 7f * scale), p.Terracotta);
            Prim.Box(body, "Hole", new Vector3(0f, 1.9f, 3.6f - 3.5f * scale), new Vector3(2.2f, 2.2f, 0.3f), p.Iron);
            for (int k = 0; k < 5; k++)
            {
                float x = -3.6f + k * 1.8f;
                Prim.Ball(body, "Moss", new Vector3(x, 0.95f, 0.2f + (k % 2) * 0.4f), new Vector3(1.2f, 0.4f, 0.9f), p.Moss);
                Prim.Box(body, "Crack", new Vector3(x * 0.8f, 2.6f + (k % 3) * 0.4f, 3.6f - 3.5f * scale - 0.05f),
                    new Vector3(0.1f, 1.1f, 0.06f), p.ClayDark, new Vector3(0f, 0f, 25f - k * 14f));
            }

            Cannon(body, p, new Vector3(0f, 4.3f, 3.6f - 3.3f * scale), ref parts);
            float[] diameters = { 6.8f, 5.6f, 4.6f, 3.4f };
            float y = 3.8f;
            for (int i = 0; i < 4; i++)
            {
                Transform tier = Tier(body, i, ref parts);
                float d = diameters[i] * scale;
                float h = 1.8f * scale;
                float lean = (i % 2 == 0 ? 1f : -1f) * 0.25f * (i + 1);
                Prim.Cyl(tier, "Drum", new Vector3(lean, y + h * 0.5f, 3.6f), new Vector3(d, h * 0.5f, d * 0.9f),
                    i % 2 == 0 ? p.Terracotta : p.Wall, new Vector3(0f, 0f, lean * 2.5f));
                Prim.Ball(tier, "Lip", new Vector3(lean, y + h, 3.6f), new Vector3(d * 1.02f, 0.45f, d * 0.92f), p.ClayDark);
                Prim.Box(tier, "Crack", new Vector3(lean + d * 0.2f, y + h * 0.5f, 3.6f - d * 0.45f), new Vector3(0.1f, h * 0.7f, 0.06f), p.ClayDark,
                    new Vector3(0f, 0f, 20f));
                Prim.Ball(tier, "Moss", new Vector3(lean - d * 0.3f, y + h * 0.95f, 3.6f - d * 0.35f), new Vector3(0.9f, 0.3f, 0.7f), p.Moss);
                y += h + 0.1f;
            }

            parts.BarHeight = y + 3.2f;
        }

        private static void ClayTotemExtras(Transform body, ArtPalette p, TowerParts parts)
        {
            // Horns and bone plates on the tiers (they fall with them); no eyes, no face (GDD 2).
            for (int i = 0; i < parts.Tiers.Length; i++)
            {
                Transform tier = parts.Tiers[i];
                float y = 5f + (3 - i) * 2.45f;
                float reach = 3.4f - i * 0.4f;
                for (int side = -1; side <= 1; side += 2)
                {
                    Prim.Box(tier, "Horn", new Vector3(side * (reach + 0.9f), y + 0.5f, 3.6f), new Vector3(2.2f, 0.45f, 0.45f), p.Bone,
                        new Vector3(0f, 0f, side * 35f));
                }

                Prim.Box(tier, "Plate", new Vector3(0f, y, 3.6f - reach * 0.95f), new Vector3(1.6f, 0.9f, 0.15f), p.Bone);
            }
        }

        // ---------- boss A: citadel ----------

        private static void Citadel(Transform body, ArtPalette p, ref TowerParts parts)
        {
            Prim.Box(body, "Plinth", new Vector3(0f, 0.6f, 3.5f), new Vector3(11f, 1.2f, 8f), p.StoneDark);
            Prim.Box(body, "Curtain", new Vector3(0f, 3f, 1.2f), new Vector3(7f, 3.8f, 1.8f), p.Stone);
            Prim.Box(body, "Hall", new Vector3(0f, 3f, 4.5f), new Vector3(8.4f, 3.8f, 5.5f), p.StoneWarm);
            Prim.Box(body, "Gate", new Vector3(0f, 2.4f, 0.25f), new Vector3(3f, 3.2f, 0.2f), p.Iron);
            Prim.Box(body, "Portcullis", new Vector3(0f, 2.4f, 0.15f), new Vector3(2.6f, 0.15f, 0.1f), p.Glow);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim.Cyl(body, "Turret", new Vector3(side * 4.6f, 4.8f, 1.6f), new Vector3(2.4f, 4.2f, 2.4f), p.Stone);
                Prim.Cyl(body, "TurretRim", new Vector3(side * 4.6f, 9.1f, 1.6f), new Vector3(2.8f, 0.2f, 2.8f), p.StoneDark);
                for (int s = 0; s < 4; s++)
                {
                    float a = s * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                    Prim.Box(body, "TurretSpike", new Vector3(side * 4.6f + Mathf.Cos(a) * 1.1f, 9.7f, 1.6f + Mathf.Sin(a) * 1.1f),
                        new Vector3(0.35f, 0.35f, 0.35f), p.Iron, new Vector3(45f, 45f, 0f));
                }

                Prim.Box(body, "Banner", new Vector3(side * 2.4f, 3.3f, 0.25f), new Vector3(1.1f, 3f, 0.08f), p.Banner);
                Prim.Ball(body, "Brazier", new Vector3(side * 4.6f, 9.4f, 1.6f), new Vector3(1f, 0.5f, 1f), p.Glow);
            }

            Cannon(body, p, new Vector3(0f, 5.4f, 1.6f), ref parts);
            float[] widths = { 7.6f, 6.4f, 5.2f, 3.8f };
            float y = 4.9f;
            for (int i = 0; i < 4; i++)
            {
                Transform tier = Tier(body, i, ref parts);
                float w = widths[i];
                float h = i == 3 ? 2f : 2.2f;
                Prim.Box(tier, "Block", new Vector3(0f, y + h * 0.5f, 4.5f), new Vector3(w, h, w * 0.7f), i % 2 == 0 ? p.StoneWarm : p.Stone);
                Prim.Box(tier, "Band", new Vector3(0f, y + h - 0.12f, 4.5f), new Vector3(w + 0.35f, 0.3f, w * 0.7f + 0.35f), p.StoneDark);
                Prim.Box(tier, "Window", new Vector3(0f, y + h * 0.5f, 4.5f - w * 0.35f - 0.02f), new Vector3(w * 0.3f, 0.5f, 0.05f), p.Glow);
                int spikes = 3 + (3 - i);
                for (int s = 0; s < spikes; s++)
                {
                    float x = Mathf.Lerp(-w * 0.42f, w * 0.42f, spikes == 1 ? 0.5f : (float)s / (spikes - 1));
                    Prim.Box(tier, "Spike", new Vector3(x, y + h + 0.3f, 4.5f - w * 0.3f), new Vector3(0.42f, 0.42f, 0.42f), p.Iron,
                        new Vector3(45f, 0f, 45f));
                }

                if (i == 3)
                {
                    for (int s = 0; s < 5; s++)
                    {
                        float a = s * Mathf.PI * 2f / 5f;
                        Prim.Box(tier, "Crown", new Vector3(Mathf.Cos(a) * 1.3f, y + h + 0.8f, 4.5f + Mathf.Sin(a) * 1.1f),
                            new Vector3(0.35f, 1.4f, 0.35f), p.Iron);
                    }

                    Prim.Ball(tier, "CrownGlow", new Vector3(0f, y + h + 0.6f, 4.5f), new Vector3(1.4f, 0.8f, 1.4f), p.GlowHot);
                }

                y += h + 0.15f;
            }

            parts.BarHeight = y + 3.4f;
        }

        // ---------- world 2 tower: stilt fort ----------

        private static void StiltFort(Transform body, ArtPalette p, ref TowerParts parts)
        {
            Prim.Ball(body, "Mound", new Vector3(0f, 0.1f, 3.4f), new Vector3(11f, 1.2f, 8f), p.ClayDark);
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sz = 0; sz < 2; sz++)
                {
                    Prim.Cyl(body, "Stilt", new Vector3(sx * 3.4f, 1.6f, 1.2f + sz * 4.6f), new Vector3(0.7f, 1.7f, 0.7f), p.Wood);
                }

                Prim.Box(body, "Brace", new Vector3(sx * 3.4f, 1.6f, 3.5f), new Vector3(0.25f, 0.25f, 4.8f), p.WoodDark,
                    new Vector3(25f, 0f, 0f));
                Prim.Cyl(body, "LanternPole", new Vector3(sx * 4.7f, 2.6f, 0.2f), new Vector3(0.18f, 2.6f, 0.18f), p.Wood);
                Prim.Ball(body, "Lantern", new Vector3(sx * 4.7f, 5.35f, 0.2f), new Vector3(0.6f, 0.75f, 0.6f), p.Lantern);
            }

            Prim.Box(body, "Deck", new Vector3(0f, 3.4f, 3.4f), new Vector3(8.8f, 0.45f, 6.6f), p.WoodDark);
            for (int i = 0; i < 11; i++)
            {
                float x = -4f + i * 0.8f;
                float h = 2.2f + (i % 3) * 0.35f;
                Prim.Cyl(body, "Stake", new Vector3(x, 3.5f + h * 0.5f, 0.35f), new Vector3(0.55f, h * 0.5f, 0.55f), i % 2 == 0 ? p.Wood : p.WoodDark);
            }

            Prim.Box(body, "Gate", new Vector3(0f, 1.6f, 0.9f), new Vector3(2.2f, 2.8f, 0.2f), p.Iron);
            Prim.Box(body, "Ramp", new Vector3(0f, 0.9f, -0.6f), new Vector3(2.4f, 0.15f, 3.2f), p.Wood, new Vector3(-28f, 0f, 0f));
            Cannon(body, p, new Vector3(0f, 6.4f, 0.6f), ref parts);
            float[] widths = { 7f, 5.8f, 4.6f, 3.4f };
            float y = 3.65f;
            for (int i = 0; i < 4; i++)
            {
                Transform tier = Tier(body, i, ref parts);
                float w = widths[i];
                float h = 1.9f;
                Prim.Box(tier, "Hut", new Vector3(0f, y + h * 0.5f, 3.8f), new Vector3(w, h, w * 0.7f), i % 2 == 0 ? p.WoodDark : p.Wood);
                Prim.Box(tier, "Eave", new Vector3(0f, y + h + 0.05f, 3.8f), new Vector3(w + 0.6f, 0.22f, w * 0.7f + 0.6f), p.Thatch);
                for (int plank = -1; plank <= 1; plank++)
                {
                    Prim.Box(tier, "Plank", new Vector3(plank * w * 0.3f, y + h * 0.5f, 3.8f - w * 0.35f - 0.03f),
                        new Vector3(0.12f, h * 0.9f, 0.06f), p.Iron);
                }

                Prim.Box(tier, "Window", new Vector3(w * 0.15f, y + h * 0.55f, 3.8f - w * 0.35f - 0.04f), new Vector3(0.5f, 0.45f, 0.05f), p.Lantern);
                if (i == 3)
                {
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Prim.Box(tier, "Roof", new Vector3(side * 0.95f, y + h + 0.95f, 3.8f), new Vector3(2.4f, 0.3f, w * 0.85f), p.Thatch,
                            new Vector3(0f, 0f, -side * 38f));
                    }

                    Prim.Ball(tier, "TopLantern", new Vector3(0f, y + h + 2.05f, 3.8f), new Vector3(0.7f, 0.85f, 0.7f), p.Lantern);
                }

                y += h + 0.25f;
            }

            parts.BarHeight = y + 3.6f;
        }

        // ---------- world 2 boss: sunken bell tower ----------

        private static void SunkenBell(Transform body, ArtPalette p, ref TowerParts parts)
        {
            Prim.Cyl(body, "Pool", new Vector3(0f, 0.05f, 4.2f), new Vector3(12f, 0.05f, 9f), p.StoneDark);
            Prim.Cyl(body, "Base", new Vector3(0f, 2.2f, 4.4f), new Vector3(8.6f, 2.2f, 8.6f), p.Stone, new Vector3(0f, 0f, -4f));
            for (int k = 0; k < 6; k++)
            {
                float a = (k - 2.5f) * 0.42f;
                Prim.Ball(body, "Moss", new Vector3(Mathf.Sin(a) * 4.2f, 0.6f + (k % 2) * 0.5f, 4.4f - Mathf.Cos(a) * 4.2f),
                    new Vector3(1.6f, 0.6f, 1.1f), p.Moss);
            }

            Prim.Box(body, "Arch", new Vector3(0f, 2.1f, 0.15f), new Vector3(2.6f, 3f, 0.3f), p.Iron);
            Prim.Box(body, "ArchLight", new Vector3(0f, 2.1f, 0.05f), new Vector3(1.8f, 2.2f, 0.1f), p.Lantern);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim.Box(body, "Chain", new Vector3(side * 3.2f, 5.5f, 0.6f), new Vector3(0.15f, 6f, 0.15f), p.Iron, new Vector3(0f, 0f, side * 20f));
                Prim.Ball(body, "Weight", new Vector3(side * 4.2f, 2.6f, 0.6f), new Vector3(0.9f, 0.9f, 0.9f), p.Iron);
            }

            Cannon(body, p, new Vector3(0f, 5f, 0.4f), ref parts);
            float[] diameters = { 7.4f, 6.2f, 5f, 3.8f };
            float y = 4.4f;
            for (int i = 0; i < 4; i++)
            {
                Transform tier = Tier(body, i, ref parts);
                float d = diameters[i];
                float h = 1.8f;
                Prim.Cyl(tier, "Drum", new Vector3(0f, y + h * 0.5f, 4.4f), new Vector3(d, h * 0.5f, d), i % 2 == 0 ? p.StoneWarm : p.Stone);
                Prim.Cyl(tier, "Lip", new Vector3(0f, y + h, 4.4f), new Vector3(d + 0.4f, 0.12f, d + 0.4f), p.StoneDark);
                for (int w = -1; w <= 1; w++)
                {
                    Prim.Box(tier, "Arch", new Vector3(w * d * 0.28f, y + h * 0.5f, 4.4f - d * 0.47f), new Vector3(0.55f, 1.1f, 0.12f), p.Lantern);
                }

                Prim.Ball(tier, "Moss", new Vector3(-d * 0.3f, y + h - 0.1f, 4.4f - d * 0.38f), new Vector3(1.1f, 0.4f, 0.8f), p.Moss);
                if (i == 3)
                {
                    for (int post = 0; post < 4; post++)
                    {
                        float a = post * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                        Prim.Box(tier, "CagePost", new Vector3(Mathf.Cos(a) * 1.3f, y + h + 1.2f, 4.4f + Mathf.Sin(a) * 1.3f),
                            new Vector3(0.25f, 2.4f, 0.25f), p.Iron);
                    }

                    Prim.Box(tier, "CageRoof", new Vector3(0f, y + h + 2.5f, 4.4f), new Vector3(3.2f, 0.3f, 3.2f), p.Iron, new Vector3(0f, 45f, 0f));
                    Prim.Ball(tier, "Bell", new Vector3(0f, y + h + 1.3f, 4.4f), new Vector3(1.6f, 1.9f, 1.6f), p.Bone);
                    Prim.Ball(tier, "Clapper", new Vector3(0f, y + h + 0.45f, 4.4f), new Vector3(0.5f, 0.5f, 0.5f), p.Lantern);
                }

                y += h + 0.15f;
            }

            parts.BarHeight = y + 3.9f;
        }

        // ---------- world 3 tower: smokestack mill ----------

        private static void SmokestackMill(Transform body, ArtPalette p, ref TowerParts parts)
        {
            Prim.Box(body, "Plinth", new Vector3(0f, 0.5f, 3.4f), new Vector3(10.5f, 1f, 7.5f), p.Soot);
            Prim.Box(body, "Hall", new Vector3(0f, 2.8f, 2.4f), new Vector3(9f, 3.6f, 4.4f), p.Brick);
            Prim.Box(body, "Shutter", new Vector3(0f, 2.3f, 0.15f), new Vector3(2.8f, 2.6f, 0.15f), p.Steel);
            for (int slat = 0; slat < 4; slat++)
            {
                Prim.Box(body, "Slat", new Vector3(0f, 1.3f + slat * 0.6f, 0.05f), new Vector3(2.9f, 0.08f, 0.08f), p.Soot);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                Prim.Box(body, "Furnace", new Vector3(side * 3f, 2f, 0.12f), new Vector3(1.4f, 1f, 0.1f), p.Glow);
                Prim.Cyl(body, "Pipe", new Vector3(side * 2.2f, 4.85f, 0.4f), new Vector3(0.45f, 2.3f, 0.45f), p.Steel, new Vector3(0f, 0f, 90f));
                Prim.Cyl(body, "Stack", new Vector3(side * 3.8f, 7f, 5.8f), new Vector3(1.3f, 7f, 1.3f), p.Brick);
                Prim.Cyl(body, "StackBand", new Vector3(side * 3.8f, 9.5f, 5.8f), new Vector3(1.45f, 0.15f, 1.45f), p.Soot);
                Prim.Cyl(body, "StackRim", new Vector3(side * 3.8f, 14.05f, 5.8f), new Vector3(1.1f, 0.06f, 1.1f), p.Glow);
            }

            Cannon(body, p, new Vector3(0f, 5.2f, 0.6f), ref parts);
            float[] widths = { 7.2f, 6f, 4.8f, 3.6f };
            float y = 4.6f;
            for (int i = 0; i < 4; i++)
            {
                Transform tier = Tier(body, i, ref parts);
                float w = widths[i];
                float h = 1.8f;
                Prim.Box(tier, "Floor", new Vector3(0f, y + h * 0.5f, 3.6f), new Vector3(w, h, w * 0.7f), i % 2 == 0 ? p.Brick : p.Soot);
                Prim.Box(tier, "Windows", new Vector3(0f, y + h * 0.55f, 3.6f - w * 0.35f - 0.03f), new Vector3(w * 0.7f, 0.35f, 0.05f), p.Glow);
                Prim.Box(tier, "Girder", new Vector3(0f, y + h + 0.05f, 3.6f), new Vector3(w + 0.4f, 0.2f, w * 0.7f + 0.4f), p.Steel);
                for (int tooth = 0; tooth < 3 && i < 3; tooth++)
                {
                    float x = (tooth - 1) * w * 0.3f;
                    Prim.Box(tier, "Roof", new Vector3(x, y + h + 0.45f, 3.6f), new Vector3(w * 0.28f, 0.7f, w * 0.6f), p.Steel,
                        new Vector3(0f, 0f, 30f));
                }

                if (i == 3)
                {
                    for (int leg = 0; leg < 4; leg++)
                    {
                        Prim.Box(tier, "TankLeg", new Vector3((leg % 2 - 0.5f) * 1.6f, y + h + 0.7f, 3.6f + (leg / 2 - 0.5f) * 1.6f),
                            new Vector3(0.15f, 1.4f, 0.15f), p.Steel);
                    }

                    Prim.Cyl(tier, "Tank", new Vector3(0f, y + h + 2f, 3.6f), new Vector3(2.4f, 0.75f, 2.4f), p.Brick);
                    Prim.Cyl(tier, "TankRim", new Vector3(0f, y + h + 2.8f, 3.6f), new Vector3(2.5f, 0.06f, 2.5f), p.Glow);
                }

                y += h + 0.2f;
            }

            parts.BarHeight = y + 4f;
        }

        // ---------- world 3 tower option B: gasholder ----------

        private static void Gasholder(Transform body, ArtPalette p, ref TowerParts parts)
        {
            Prim.Box(body, "Plinth", new Vector3(0f, 0.5f, 3.6f), new Vector3(10.5f, 1f, 7.5f), p.Soot);
            Prim.Cyl(body, "Base", new Vector3(0f, 2.4f, 4f), new Vector3(8f, 1.5f, 8f), p.Steel);
            Prim.Box(body, "Hatch", new Vector3(0f, 2.2f, 0.05f), new Vector3(2.2f, 2.2f, 0.2f), p.Glow);
            Cannon(body, p, new Vector3(0f, 4.6f, 0.4f), ref parts);
            for (int post = 0; post < 8; post++)
            {
                float a = post * Mathf.PI * 0.25f;
                Prim.Box(body, "Lattice", new Vector3(Mathf.Cos(a) * 4.6f, 7f, 4f + Mathf.Sin(a) * 4.6f), new Vector3(0.3f, 12f, 0.3f), p.Soot);
            }

            Prim.Cyl(body, "Ring", new Vector3(0f, 12.8f, 4f), new Vector3(9.6f, 0.15f, 9.6f), p.Soot);
            float[] diameters = { 7.2f, 6.8f, 6.4f, 6f };
            float y = 3.9f;
            for (int i = 0; i < 4; i++)
            {
                Transform tier = Tier(body, i, ref parts);
                float d = diameters[i];
                float h = 2f;
                Prim.Cyl(tier, "Lift", new Vector3(0f, y + h * 0.5f, 4f), new Vector3(d, h * 0.5f, d), i % 2 == 0 ? p.Brick : p.Steel);
                Prim.Cyl(tier, "Seam", new Vector3(0f, y + h, 4f), new Vector3(d + 0.2f, 0.08f, d + 0.2f), p.Glow);
                if (i == 3)
                {
                    Prim.Ball(tier, "Dome", new Vector3(0f, y + h + 0.3f, 4f), new Vector3(d, 1.8f, d), p.Steel);
                }

                y += h + 0.1f;
            }

            parts.BarHeight = y + 4.4f;
        }

        // ---------- world 3 boss: piston hammer ----------

        private static void PistonHammer(Transform body, ArtPalette p, ref TowerParts parts)
        {
            Prim.Box(body, "Anvil", new Vector3(0f, 0.8f, 4f), new Vector3(11.5f, 1.6f, 8.5f), p.Soot);
            Prim.Box(body, "Bed", new Vector3(0f, 2.4f, 2.8f), new Vector3(7.4f, 1.6f, 5f), p.Iron);
            Prim.Box(body, "Mouth", new Vector3(0f, 2.4f, 0.25f), new Vector3(4.4f, 1f, 0.1f), p.GlowHot);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim.Box(body, "Column", new Vector3(side * 4.6f, 6.8f, 4f), new Vector3(1.6f, 11.6f, 2.2f), p.Steel);
                Prim.Box(body, "ColumnBand", new Vector3(side * 4.6f, 4f, 2.85f), new Vector3(1.7f, 0.3f, 0.1f), p.Glow);
                Prim.Box(body, "ColumnBand", new Vector3(side * 4.6f, 9f, 2.85f), new Vector3(1.7f, 0.3f, 0.1f), p.Glow);
                Prim.Cyl(body, "Gauge", new Vector3(side * 4.6f, 11f, 2.8f), new Vector3(1f, 0.08f, 1f), p.Bone, new Vector3(90f, 0f, 0f));
            }

            Prim.Box(body, "Crossbeam", new Vector3(0f, 13.1f, 4f), new Vector3(11f, 1.2f, 2.6f), p.Iron);
            Prim.Cyl(body, "Ram", new Vector3(0f, 12.1f, 4f), new Vector3(1.2f, 0.6f, 1.2f), p.Steel);
            Cannon(body, p, new Vector3(0f, 4.3f, 0.5f), ref parts);
            float[] widths = { 6.6f, 5.8f, 5f, 4.2f };
            float y = 3.2f;
            for (int i = 0; i < 4; i++)
            {
                Transform tier = Tier(body, i, ref parts);
                float w = widths[i];
                float h = 1.9f;
                Prim.Box(tier, "Plate", new Vector3(0f, y + h * 0.5f, 4f), new Vector3(w, h, w * 0.75f), i % 2 == 0 ? p.Brick : p.IronWarm);
                Prim.Box(tier, "Seam", new Vector3(0f, y + 0.08f, 4f - w * 0.375f - 0.03f), new Vector3(w * 0.9f, 0.16f, 0.05f), p.GlowHot);
                for (int rivet = -1; rivet <= 1; rivet += 2)
                {
                    Prim.Box(tier, "Strap", new Vector3(rivet * w * 0.35f, y + h * 0.5f, 4f - w * 0.375f - 0.04f), new Vector3(0.3f, h, 0.06f), p.Steel);
                }

                y += h + 0.12f;
            }

            parts.BarHeight = 15.6f;
        }

        // ---------- shared ----------

        private static Transform Tier(Transform body, int indexFromBottom, ref TowerParts parts)
        {
            var tier = new GameObject("Tier" + (indexFromBottom + 1)).transform;
            tier.SetParent(body, false);
            parts.Tiers[AssaultTower.BlocksPerBody - 1 - indexFromBottom] = tier;
            return tier;
        }

        private static void Cannon(Transform body, ArtPalette p, Vector3 at, ref TowerParts parts)
        {
            var cannon = new GameObject("Cannon").transform;
            cannon.SetParent(body, false);
            cannon.localPosition = at;
            Prim.Box(cannon, "Carriage", new Vector3(0f, -0.35f, 0.3f), new Vector3(1.4f, 0.5f, 1.4f), p.Wood);
            Prim.Cyl(cannon, "Barrel", new Vector3(0f, 0.15f, -0.4f), new Vector3(0.8f, 1.1f, 0.8f), p.Iron, new Vector3(-80f, 0f, 0f));
            Prim.Cyl(cannon, "Mouth", new Vector3(0f, 0.34f, -1.45f), new Vector3(0.95f, 0.12f, 0.95f), p.IronWarm, new Vector3(-80f, 0f, 0f));
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(cannon, false);
            muzzle.localPosition = new Vector3(0f, 0.4f, -1.6f);
            parts.Muzzle = muzzle;
        }
    }

    /// <summary>Primitive props without colliders or shadows (the game never queries physics; rule 2.2.7).</summary>
    public static class Prim
    {
        public static MeshRenderer Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material,
            Vector3 euler = default)
        {
            return Make(PrimitiveType.Cube, parent, name, position, scale, material, euler);
        }

        public static MeshRenderer Cyl(Transform parent, string name, Vector3 position, Vector3 scale, Material material,
            Vector3 euler = default)
        {
            return Make(PrimitiveType.Cylinder, parent, name, position, scale, material, euler);
        }

        public static MeshRenderer Ball(Transform parent, string name, Vector3 position, Vector3 scale, Material material,
            Vector3 euler = default)
        {
            return Make(PrimitiveType.Sphere, parent, name, position, scale, material, euler);
        }

        public static MeshRenderer Make(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale,
            Material material, Vector3 euler = default)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }
    }
}
