using JungleDegree.StudioKit.Editor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// RT-6v (direction v2, D24 = A): world 1's forts in the glossy toy look, built with the kit's
    /// <see cref="SoftMeshBuilder"/>: a round cream plaza, a coral base drum with an arched door, windows, a dark
    /// cannon and two turrets with navy pointed roofs (permanent), and four stacked drums (the floors that drop in
    /// the assault, top first) with round merlons, the top one under a navy roof and a flag. The second tower is
    /// the same fort in cream with coral trim and rounded square floors; the boss is bigger, with gold trim, a
    /// crown of round spikes and a gold cannon ball. Same contract as <see cref="TowerArt"/>: front face at z = 0,
    /// body towards +z, one material (vertex colors), one mesh per part.
    /// </summary>
    public static class ToyTowerArt
    {
        private const float CenterZ = 4f;

        private struct Look
        {
            public string Name;
            public float Scale;
            public Color Main;
            public Color Trim;
            public Color Roof;
            public bool SquareFloors;
            public bool Boss;
        }

        public static void Build(TowerStyle style, Transform body, ref TowerParts parts)
        {
            Look look;
            switch (style)
            {
                case TowerStyle.ToyFort:
                    look = new Look { Name = "ToyFort", Scale = 1f, Main = ToyPalette.FortMain, Trim = ToyPalette.FortTrim, Roof = ToyPalette.FortRoof };
                    break;
                case TowerStyle.ToyFortTwin:
                    look = new Look
                    {
                        Name = "ToyFortTwin", Scale = 1f, Main = ToyPalette.FortTrim, Trim = ToyPalette.FortMain, Roof = ToyPalette.FortRoof,
                        SquareFloors = true
                    };
                    break;
                case TowerStyle.ToyCitadel:
                    look = new Look
                    {
                        Name = "ToyCitadel", Scale = 1.25f, Main = ToyPalette.FortMain, Trim = ToyPalette.FortTrim, Roof = ToyPalette.FortRoof,
                        Boss = true
                    };
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(style), style, "not a toy tower style");
            }

            Material material = ToyArt.Painted("ToyForts");
            float s = look.Scale;
            var b = new SoftMeshBuilder();

            // Permanent base: plaza, base drum, door, windows, turrets.
            // The plaza reaches down to the meadow's grass (ToyScenery.GrassTop) around the track.
            b.Drum(new Vector3(0f, ToyScenery.GrassTop - 0.1f, CenterZ), 6.6f * s, 0.16f - ToyScenery.GrassTop, 0.12f,
                ToyArt.Paint(ToyPalette.TrackTop, 0.3f), 40);
            b.Ring(new Vector3(0f, 0.04f, CenterZ), 6.6f * s - 0.05f, 0.1f, ToyArt.Paint(ToyPalette.Curb, 0.8f), 40, 6);
            float baseTop = 0.25f + 3f;
            b.Drum(new Vector3(0f, 0f, CenterZ), 3.4f * s, baseTop, 0.35f, ToyArt.Paint(look.Main), 32);
            b.Ring(new Vector3(0f, baseTop - 0.05f, CenterZ), 3.4f * s, 0.16f, ToyArt.Paint(look.Trim), 32, 6);
            float front = CenterZ - 3.4f * s;
            b.RoundedBox(new Vector3(0f, 1.35f, front + 0.05f), new Vector3(1.9f, 2.2f, 0.5f), 0.25f, ToyArt.Paint(ToyPalette.FortDoor, 0.6f), 3);
            b.Ellipsoid(new Vector3(0f, 2.45f, front + 0.05f), new Vector3(0.95f, 0.5f, 0.25f), ToyArt.Paint(ToyPalette.FortDoor, 0.6f), 12, 5);
            for (int side = -1; side <= 1; side += 2)
            {
                Window(b, new Vector3(side * 1.9f * s, 2f, front + 0.75f * s));
                Vector3 turret = new Vector3(side * 3.7f * s, 0f, CenterZ - 0.6f * s);
                b.Drum(turret, 1f * s, 5.45f * s, 0.25f, ToyArt.Paint(look.Trim), 20);
                b.Ring(turret + Vector3.up * 5.45f * s, 1f * s, 0.12f, ToyArt.Paint(look.Main), 20, 5);
                b.Dome(turret + Vector3.up * 5.45f * s, 1.25f * s, 1.9f * s, ToyArt.Paint(look.Roof), 1.5f, 20, 7);
                b.Ellipsoid(turret + Vector3.up * (7.4f * s), Vector3.one * 0.22f * s, ToyArt.Paint(ToyPalette.Gold), 8, 5);
                Window(b, turret + new Vector3(0f, 3.4f * s, -1f * s + 0.05f));
                if (look.Boss)
                {
                    Vector3 back = new Vector3(side * 2.6f * s, 0f, CenterZ + 2.6f * s);
                    b.Drum(back, 0.9f * s, 6.4f * s, 0.2f, ToyArt.Paint(look.Trim), 18);
                    b.Dome(back + Vector3.up * 6.4f * s, 1.1f * s, 1.8f * s, ToyArt.Paint(look.Roof), 1.5f, 18, 6);
                }
            }

            ToyArt.Place(body, "Base", ToyArt.SaveMesh(b.Build(look.Name + "_Base"), look.Name + "_Base"), material);

            // The cannon on the base, over the door (the cannon defense fires from its muzzle).
            Cannon(body, material, new Vector3(0f, baseTop + 0.45f, front + 0.7f * s), look.Boss, ref parts);

            // Four floors, bottom to top; the top one carries the roof (or the boss crown).
            float y = baseTop;
            float[] radii = { 3f, 2.6f, 2.2f, 1.8f };
            for (int i = 0; i < 4; i++)
            {
                b.Clear();
                float r = radii[i] * s;
                const float height = 1.8f;
                // One wall color with a thin trim rim: alternating colored layers read as a cake (second opinion of D24).
                Color wall = ToyArt.Paint(look.Main);
                Color band = ToyArt.Paint(look.Trim);
                Vector3 at = new Vector3(0f, y, CenterZ);
                if (look.SquareFloors)
                {
                    b.RoundedBox(at + Vector3.up * height * 0.5f, new Vector3(r * 1.8f, height, r * 1.8f), 0.4f, wall, 4);
                }
                else
                {
                    b.Drum(at, r, height, 0.3f, wall, 28);
                }

                b.Ring(at + Vector3.up * (height - 0.08f), r * (look.SquareFloors ? 0.98f : 1f), 0.14f, band, 28, 5);
                int merlons = 8 + i;
                for (int m = 0; m < merlons; m++)
                {
                    float angle = (m + 0.5f) * Mathf.PI * 2f / merlons;
                    var place = new Vector3(Mathf.Sin(angle) * r * 0.86f, y + height + 0.18f, CenterZ - Mathf.Cos(angle) * r * 0.86f);
                    b.Ellipsoid(place, new Vector3(0.3f, 0.24f, 0.3f) * s, wall, 8, 5);
                }

                Window(b, new Vector3(0f, y + height * 0.5f, CenterZ - r * (look.SquareFloors ? 0.9f : 1f) + 0.05f));
                if (i == 3)
                {
                    Vector3 top = new Vector3(0f, y + height, CenterZ);
                    if (look.Boss)
                    {
                        b.Dome(top, r * 0.9f, 1f * s, ToyArt.Paint(ToyPalette.BossGold), 0f, 24, 6);
                        for (int k = 0; k < 7; k++)
                        {
                            float angle = k * Mathf.PI * 2f / 7f;
                            var spike = top + new Vector3(Mathf.Sin(angle) * r * 0.7f, 0.9f * s, -Mathf.Cos(angle) * r * 0.7f);
                            b.Ellipsoid(spike, new Vector3(0.3f, 0.55f, 0.3f) * s, ToyArt.Paint(ToyPalette.BossGold), 8, 6);
                        }

                        b.Ellipsoid(top + Vector3.up * 1.6f * s, Vector3.one * 0.7f * s, ToyArt.Paint(ToyPalette.Cannon), 14, 9);
                        b.Ellipsoid(top + new Vector3(0.25f, 1.9f, -0.35f) * s, Vector3.one * 0.18f * s, ToyArt.Paint(ToyPalette.Gold), 8, 5);
                    }
                    else
                    {
                        b.Dome(top, r * 1.18f, 2.4f * s, ToyArt.Paint(look.Roof), 1.5f, 28, 8);
                        b.Drum(top + Vector3.up * 2.2f * s, 0.06f, 1.4f * s, 0.03f, ToyArt.Paint(ToyPalette.FortWindow, 0.4f), 6);
                        b.RoundedBox(top + new Vector3(0.5f, 3.25f, 0f) * s, new Vector3(0.95f, 0.6f, 0.08f) * s, 0.04f,
                            ToyArt.Paint(ToyPalette.Gold), 2);
                    }
                }

                Transform tier = Tier(body, i, ref parts);
                string name = $"{look.Name}_Floor{i + 1}";
                ToyArt.Place(tier, "Floor", ToyArt.SaveMesh(b.Build(name), name), material);
                y += height + 0.05f;
            }

            parts.BarHeight = y + (look.Boss ? 3.6f : 3.4f) * s;
        }

        // An arched window: a dark rounded slot with a round top.
        private static void Window(SoftMeshBuilder b, Vector3 at)
        {
            Color glass = ToyArt.Paint(ToyPalette.FortWindow, 0.9f);
            b.RoundedBox(at, new Vector3(0.42f, 0.6f, 0.2f), 0.1f, glass, 2);
            b.Ellipsoid(at + Vector3.up * 0.3f, new Vector3(0.21f, 0.2f, 0.1f), glass, 8, 4);
        }

        private static Transform Tier(Transform body, int indexFromBottom, ref TowerParts parts)
        {
            var tier = new GameObject("Tier" + (indexFromBottom + 1)).transform;
            tier.SetParent(body, false);
            parts.Tiers[AssaultTower.BlocksPerBody - 1 - indexFromBottom] = tier;
            return tier;
        }

        private static void Cannon(Transform body, Material material, Vector3 at, bool boss, ref TowerParts parts)
        {
            var cannon = new GameObject("Cannon").transform;
            cannon.SetParent(body, false);
            cannon.localPosition = at;
            string name = boss ? "ToyCannonBoss" : "ToyCannon";
            var b = new SoftMeshBuilder();
            Color dark = ToyArt.Paint(ToyPalette.Cannon, 1f);
            Color trim = ToyArt.Paint(boss ? ToyPalette.BossGold : ToyPalette.Gold);
            b.RoundedBox(new Vector3(0f, -0.3f, 0.3f), new Vector3(1.4f, 0.5f, 1.3f), 0.2f, ToyArt.Paint(ToyPalette.FortTrim), 3);
            b.Ellipsoid(new Vector3(0f, 0.15f, 0.1f), new Vector3(0.55f, 0.5f, 0.6f), dark, 12, 7);
            // The barrel points along -Z (towards the army), raised a little.
            b.Frame = Matrix4x4.TRS(new Vector3(0f, 0.2f, -0.1f), Quaternion.Euler(-80f, 0f, 0f), Vector3.one);
            b.Drum(Vector3.zero, 0.36f, 1.5f, 0.12f, dark, 14, 0.42f, 3);
            b.Ring(new Vector3(0f, 1.45f, 0f), 0.42f, 0.09f, trim, 14, 5);
            Mesh barrel = b.Build(name);
            ToyArt.Place(cannon, "Cannon", ToyArt.SaveMesh(barrel, name), material);
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(cannon, false);
            muzzle.localPosition = new Vector3(0f, 0.45f, -1.6f);
            parts.Muzzle = muzzle;
        }
    }
}
