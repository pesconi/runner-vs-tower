using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak.Editor.Prototypes
{
    /// <summary>
    /// Direction v2 prototype (visual review before RT-9). Builds, in throwaway scenes, one stretch of track with a
    /// crowd, a gate pair and the enemy tower twice: as the game draws it today and in the proposed "soft toy" look,
    /// and renders both from the same cameras (side-by-side sheets in docs/art/direcao-v2). It also renders the three candidate
    /// directions (A glossy toy, B soft clay, C flat pastel) on the same geometry. It never touches the game scene,
    /// the game materials or the kit. Needs a graphics device: batchmode WITHOUT -nographics.
    /// -executeMethod Claybreak.Editor.Prototypes.DirectionV2Prototype.CaptureAndQuit [-claybreakCaptureDir C:\temp\v2]
    /// </summary>
    public static class DirectionV2Prototype
    {
        public const string OutputFolder = "docs/art/direcao-v2";
        private const string ToyShader = "Claybreak/Prototype/SoftToy";
        private const string GlassShader = "Claybreak/Prototype/SoftToyGlass";
        private const string SkyShader = "Claybreak/Prototype/SoftSky";
        private const float GateZ = 8f;
        private const float TowerZ = 42f;
        private const int CrowdSize = 44;

        private struct Framing
        {
            public string Name;
            public Vector3 Position;
            public Vector3 Target;
            public float FieldOfView;
        }

        // The run camera of the game (CameraFollow: offset (0, 9, -8), looking 6 m ahead), a close look at the crowd
        // and the gate, and a close look at the tower.
        private static readonly Framing[] Framings =
        {
            new Framing { Name = "corrida", Position = new Vector3(0f, 9f, -8f), Target = new Vector3(0f, 0f, 6f), FieldOfView = 60f },
            new Framing { Name = "perto", Position = new Vector3(3.8f, 2.4f, -4.2f), Target = new Vector3(0.3f, 1.1f, 6f), FieldOfView = 50f },
            new Framing { Name = "torre", Position = new Vector3(-9f, 6f, 24f), Target = new Vector3(0f, 4.5f, TowerZ), FieldOfView = 50f }
        };

        public static void CaptureAndQuit()
        {
            BatchRunner.Run("DirectionV2Prototype", () =>
            {
                Capture(BatchRunner.GetArgument("-claybreakCaptureDir") ?? @"C:\temp\v2");
            });
        }

        [MenuItem("Claybreak/Prototype/Capture Direction v2")]
        public static void CaptureFromMenu()
        {
            Capture(@"C:\temp\v2");
        }

        public static void Capture(string folder)
        {
            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(OutputFolder);
            var before = new Color[Framings.Length][];
            var after = new Color[Framings.Length][];

            BuildCurrentLook();
            for (int f = 0; f < Framings.Length; f++)
            {
                before[f] = Shoot(Framings[f], 1080, 1920, folder, "antes-" + Framings[f].Name);
            }

            ToyDirection[] directions = ToyDirection.All;
            var options = new Color[directions.Length][];
            for (int d = 0; d < directions.Length; d++)
            {
                BuildToyLook(directions[d]);
                options[d] = Shoot(Framings[0], 720, 1280, folder, null);
                if (d == 0)
                {
                    for (int f = 0; f < Framings.Length; f++)
                    {
                        after[f] = Shoot(Framings[f], 1080, 1920, folder, "depois-" + Framings[f].Name);
                    }
                }
            }

            for (int f = 0; f < Framings.Length; f++)
            {
                WriteSheet(folder, "antes-depois-" + Framings[f].Name, new[] { before[f], after[f] }, 1080, 1920, false);
            }

            WriteSheet(folder, "direcoes-abc", options, 720, 1280, true);
        }

        // ---------- today's look: the game's own builders ----------

        private static void BuildCurrentLook()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var palette = new ArtPalette();
            const float trackEnd = 90f;
            Renderer track = ClaybreakSceneBuilder.CreateEnvironment(palette, trackEnd, out Light sun);
            WorldDefinition world = WorldContent.Worlds[0];
            var towerRoot = new GameObject("Tower").transform;
            towerRoot.position = new Vector3(0f, 0f, TowerZ);
            TowerArt.Build(WorldContent.ChosenTowerStyle(0), towerRoot, palette);
            Camera camera = NewCamera();
            ClaybreakSceneBuilder.DressWorld(world.Id, world.Look, palette, trackEnd, track, sun, camera);

            // The run gate pair exactly as ClaybreakSceneBuilder.CreateRunGates draws it.
            float width = ClaybreakSceneBuilder.TrackWidth;
            float half = width * 0.25f;
            var pair = new GameObject("GatePair").transform;
            pair.position = new Vector3(0f, 0f, GateZ);
            Prim.Box(pair, "Left", new Vector3(-half, 1.1f, 0f), new Vector3(width * 0.48f, 2.2f, 0.12f), palette.GateGood);
            Prim.Box(pair, "Right", new Vector3(half, 1.1f, 0f), new Vector3(width * 0.48f, 2.2f, 0.12f), palette.GateBad);
            ClaybreakSceneBuilder.CreateLabel("LeftLabel", pair, new Vector3(-half, 1.2f, -0.1f), 0.32f).text = "+20";
            ClaybreakSceneBuilder.CreateLabel("RightLabel", pair, new Vector3(half, 1.2f, -0.1f), 0.32f).text = "-10";
            Prim.Box(pair, "Post", new Vector3(0f, 1.2f, 0f), new Vector3(0.18f, 2.4f, 0.18f), palette.Line);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim.Box(pair, "SidePost", new Vector3(side * (width * 0.5f + 0.1f), 1.25f, 0f), new Vector3(0.3f, 2.5f, 0.3f),
                    palette.StoneDark);
            }

            Prim.Box(pair, "Lintel", new Vector3(0f, 2.45f, 0f), new Vector3(width + 0.5f, 0.2f, 0.25f), palette.StoneDark);

            // The crowd: the kit's clay runner with the kit's crowd material, drawn as authored (no pose texture).
            Mesh body = ClaybreakSceneBuilder.LoadFirstMesh(ClaybreakSceneBuilder.BodyPath);
            var clay = AssetDatabase.LoadAssetAtPath<Material>(ClaybreakSceneBuilder.ClayMaterialPath);
            foreach (Pose pose in CrowdPoses())
            {
                Place("Runner", body, clay, pose.position, pose.rotation);
            }

            Debug.Log($"[DirectionV2] today's runner: {body.vertexCount} vertices, {body.triangles.Length / 3} triangles");
        }

        // ---------- the proposed look ----------

        private static void BuildToyLook(ToyDirection look)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var toy = new Material(Shader.Find(ToyShader) ?? throw new InvalidOperationException(ToyShader));
            var good = new Material(Shader.Find(GlassShader) ?? throw new InvalidOperationException(GlassShader));
            var bad = new Material(good);
            good.SetColor("_BaseColor", look.GlassGood);
            bad.SetColor("_BaseColor", look.GlassBad);
            var sky = new Material(Shader.Find(SkyShader) ?? throw new InvalidOperationException(SkyShader));
            sky.SetColor("_Horizon", look.Horizon);
            sky.SetColor("_Zenith", look.Zenith);
            var blob = new Material(sky);
            blob.SetFloat("_Blob", 1f);
            blob.SetColor("_BlobColor", look.Shadow);

            Shader.SetGlobalColor("_ToySkyColor", look.AmbientSky);
            Shader.SetGlobalColor("_ToyGroundColor", look.AmbientGround);
            Shader.SetGlobalFloat("_ToyGloss", look.Gloss);
            Shader.SetGlobalFloat("_ToyClay", look.Clay);
            Shader.SetGlobalFloat("_ToyBands", look.Bands);
            Shader.SetGlobalFloat("_ToyRim", look.Rim);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = look.AmbientSky;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 150f;
            RenderSettings.fogColor = look.Horizon;

            var sunObject = new GameObject("Sun", typeof(Light));
            Light sun = sunObject.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.None;
            sun.color = look.Sun;
            sun.intensity = look.SunIntensity;
            sunObject.transform.rotation = Quaternion.Euler(48f, 28f, 0f);
            RenderSettings.sun = sun;

            Place("Sky", new SoftMeshBuilder().Ellipsoid(Vector3.zero, Vector3.one * 450f, Color.white, 24, 16).Build("Sky"), sky,
                Vector3.zero, Quaternion.identity);
            Mesh blobMesh = BlobQuad();
            Place("Scenery", BuildScenery(look), toy, Vector3.zero, Quaternion.identity);
            Place("Tower", BuildTower(look), toy, new Vector3(0f, 0f, TowerZ), Quaternion.identity);
            PlaceBlob(blobMesh, blob, new Vector3(0f, 0.012f, TowerZ), 13f);
            BuildGate(look, toy, good, bad, blobMesh, blob);

            Mesh soldier = BuildSoldier(look).Build("Soldier");
            foreach (Pose pose in CrowdPoses())
            {
                Place("Soldier", soldier, toy, pose.position, pose.rotation);
                PlaceBlob(blobMesh, blob, pose.position + new Vector3(0f, 0.012f, 0f), 0.85f);
            }

            NewCamera().backgroundColor = look.Horizon;
            Debug.Log($"[DirectionV2] {look.Name}: soldier {soldier.vertexCount} vertices, {soldier.triangles.Length / 3} triangles");
        }

        private static Mesh BuildScenery(ToyDirection look)
        {
            var mesh = new SoftMeshBuilder();
            const float start = -16f;
            float length = TowerZ - start;
            // The track: a raised cream plateau with rounded curbs, ending in a round plaza under the tower.
            mesh.RoundedBox(new Vector3(0f, -0.6f, start + length * 0.5f), new Vector3(8f, 1.2f, length), 0.4f, Gloss(look.TrackTop, 0.25f), 8);
            mesh.Drum(new Vector3(0f, -1.2f, TowerZ), 7f, 1.2f, 0.4f, Gloss(look.TrackTop, 0.25f), 40);
            for (int side = -1; side <= 1; side += 2)
            {
                mesh.RoundedBox(new Vector3(side * 4.05f, 0.08f, start + (length - 6f) * 0.5f), new Vector3(0.42f, 0.42f, length - 6f), 0.2f,
                    Gloss(look.TrackEdge, 0.6f), 4);
                // Low grass on both sides, and the soft hills behind it.
                mesh.RoundedBox(new Vector3(side * 20f, -1.1f, 60f), new Vector3(31f, 1.6f, 170f), 0.6f, Gloss(look.Grass, 0.15f), 8);
            }

            var random = new System.Random(7);
            for (float z = -14f; z < 160f; z += 7.5f)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side * Range(random, 10f, 17f);
                    var radii = new Vector3(Range(random, 4f, 7f), Range(random, 2.4f, 5f), Range(random, 4f, 7f));
                    mesh.Ellipsoid(new Vector3(x, -0.6f, z + Range(random, -2f, 2f)), radii, Gloss(Pick(random, look.Hills), 0.2f), 18, 10);
                }
            }

            for (float z = -10f; z < TowerZ + 20f; z += 5.5f)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    if (random.NextDouble() < 0.25)
                    {
                        continue;
                    }

                    var basePoint = new Vector3(side * Range(random, 5.4f, 8.2f), -0.3f, z + Range(random, -1.5f, 1.5f));
                    float size = Range(random, 0.8f, 1.25f);
                    mesh.Drum(basePoint, 0.13f * size, 1.5f * size, 0.06f, Gloss(look.Trunk, 0.3f), 10);
                    mesh.Ellipsoid(basePoint + new Vector3(0f, 2.05f * size, 0f), new Vector3(0.95f, 1.05f, 0.95f) * size,
                        Gloss(Pick(random, look.Leaves), 0.55f), 16, 10);
                }
            }

            for (int i = 0; i < 140; i++)
            {
                int side = random.NextDouble() < 0.5 ? -1 : 1;
                var center = new Vector3(side * Range(random, 4.7f, 10f), -0.27f, Range(random, -12f, TowerZ + 12f));
                Flower(mesh, center, Range(random, 0.14f, 0.22f), Pick(random, look.Flowers), look.FlowerCenter);
            }

            for (int i = 0; i < 18; i++)
            {
                int side = random.NextDouble() < 0.5 ? -1 : 1;
                var center = new Vector3(side * Range(random, 5f, 9f), -0.3f, Range(random, -8f, TowerZ + 10f));
                float size = Range(random, 0.25f, 0.6f);
                mesh.Ellipsoid(center, new Vector3(size * 1.3f, size * 0.8f, size), Gloss(look.Rock, 0.4f), 12, 8);
            }

            // Clouds low over the horizon.
            for (int i = 0; i < 7; i++)
            {
                var center = new Vector3(Range(random, -45f, 45f), Range(random, 14f, 26f), Range(random, 110f, 170f));
                for (int puff = 0; puff < 4; puff++)
                {
                    var offset = new Vector3(Range(random, -4f, 4f), Range(random, -1f, 1.5f), Range(random, -1.5f, 1.5f));
                    mesh.Ellipsoid(center + offset, Vector3.one * Range(random, 2.5f, 4.5f), Gloss(Color.white, 0.1f), 14, 8);
                }
            }

            return mesh.Build("Scenery");
        }

        private static void Flower(SoftMeshBuilder mesh, Vector3 center, float size, Color petal, Color heart)
        {
            for (int p = 0; p < 5; p++)
            {
                float angle = p * Mathf.PI * 2f / 5f;
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * size;
                mesh.Ellipsoid(center + offset, new Vector3(size * 0.75f, size * 0.3f, size * 0.75f), Gloss(petal, 0.4f), 8, 5);
            }

            mesh.Ellipsoid(center + Vector3.up * size * 0.15f, Vector3.one * size * 0.45f, Gloss(heart, 0.6f), 8, 5);
        }

        // Four drums (the four floors of the game, which fall one by one), pointed dark roofs, round merlons and a
        // cannon on the first ledge.
        private static Mesh BuildTower(ToyDirection look)
        {
            var mesh = new SoftMeshBuilder();
            Color main = Gloss(look.TowerMain, 0.75f);
            Color trim = Gloss(look.TowerTrim, 0.6f);
            Color roof = Gloss(look.TowerRoof, 0.9f);
            Color window = Gloss(look.Window, 1f);
            Color metal = Gloss(look.Cannon, 1f);
            mesh.Drum(Vector3.zero, 5.2f, 0.7f, 0.3f, trim, 40);
            float[] radii = { 3.6f, 3.1f, 2.6f, 2.1f };
            float[] heights = { 2.6f, 2.3f, 2.1f, 1.9f };
            float y = 0.7f;
            for (int floor = 0; floor < radii.Length; floor++)
            {
                Color body = floor % 2 == 0 ? main : trim;
                Color band = floor % 2 == 0 ? trim : main;
                mesh.Drum(new Vector3(0f, y, 0f), radii[floor], heights[floor], 0.38f, body, 36);
                y += heights[floor];
                mesh.Ring(new Vector3(0f, y, 0f), radii[floor] - 0.05f, 0.2f, band, 36);
                int merlons = 10 + (3 - floor) * 2;
                for (int i = 0; i < merlons; i++)
                {
                    float angle = (i + 0.5f) * Mathf.PI * 2f / merlons;
                    float r = radii[floor] - 0.12f;
                    mesh.Ellipsoid(new Vector3(Mathf.Cos(angle) * r, y + 0.24f, Mathf.Sin(angle) * r), new Vector3(0.26f, 0.3f, 0.26f),
                        band, 10, 6);
                }

                if (floor > 0)
                {
                    Window(mesh, Vector3.zero, 0f, y - heights[floor] * 0.5f, radii[floor] - 0.03f, 0.3f, window);
                }
            }

            mesh.Dome(new Vector3(0f, y, 0f), 2.2f, 2.6f, roof, 1.2f, 28);
            mesh.Ellipsoid(new Vector3(0f, y + 2.6f, 0f), Vector3.one * 0.3f, trim, 12, 8);
            mesh.Drum(new Vector3(0f, y + 2.75f, 0f), 0.06f, 1.5f, 0.03f, trim, 8);
            mesh.RoundedBox(new Vector3(0.5f, y + 3.9f, 0f), new Vector3(0.9f, 0.55f, 0.08f), 0.04f, main, 3);

            for (int side = -1; side <= 1; side += 2)
            {
                var turret = new Vector3(side * 3.9f, 0.7f, -1.1f);
                mesh.Drum(turret, 1.0f, 5.6f, 0.3f, trim, 22);
                mesh.Ring(turret + new Vector3(0f, 5.6f, 0f), 1.0f, 0.16f, main, 22);
                mesh.Dome(turret + new Vector3(0f, 5.6f, 0f), 1.3f, 2.2f, roof, 1.2f, 22);
                Window(mesh, turret, 0f, 3.8f, 0.97f, 0.24f, window);
            }

            // The door on the ground floor, the cannon on the ledge above it, aimed down the track.
            Color door = Gloss(look.Door, 0.7f);
            mesh.RoundedBox(new Vector3(0f, 1.25f, -3.5f), new Vector3(1.4f, 1.2f, 0.5f), 0.2f, door, 4);
            mesh.Ellipsoid(new Vector3(0f, 1.85f, -3.5f), new Vector3(0.7f, 0.7f, 0.25f), door, 14, 8);
            var cannon = new Vector3(0f, 3.75f, -3.15f);
            mesh.RoundedBox(cannon + new Vector3(0f, -0.35f, 0.1f), new Vector3(1.3f, 0.5f, 1.0f), 0.2f, door, 4);
            mesh.Lathe(new[]
                {
                    new Vector2(0f, 0f), new Vector2(0.42f, 0.05f), new Vector2(0.45f, 0.4f), new Vector2(0.36f, 1.3f),
                    new Vector2(0.46f, 1.45f), new Vector2(0.44f, 1.6f), new Vector2(0.25f, 1.6f), new Vector2(0f, 1.5f)
                }, 16, Matrix4x4.TRS(cannon, Quaternion.Euler(-100f, 0f, 0f), Vector3.one), metal);
            mesh.Ellipsoid(cannon + new Vector3(0f, 0f, 0.1f), Vector3.one * 0.5f, metal, 14, 9);
            return mesh.Build("Tower");
        }

        // A dark glossy window on the front of a drum (axis at x = 0 of <paramref name="axis"/>), x across, y up.
        private static void Window(SoftMeshBuilder mesh, Vector3 axis, float x, float y, float radius, float size, Color color)
        {
            float z = -Mathf.Sqrt(Mathf.Max(0f, radius * radius - x * x));
            var outward = new Vector3(x, 0f, z).normalized;
            Quaternion facing = Quaternion.LookRotation(outward);
            mesh.RoundedBox(axis + new Vector3(x, y - size * 0.35f, z), new Vector3(size * 1.6f, size * 1.6f, 0.24f), size * 0.25f, color, 3,
                facing);
            mesh.Ellipsoid(axis + new Vector3(x, y + size * 0.45f, z), new Vector3(size * 0.8f, size * 0.8f, 0.12f), color, 12, 8, facing);
        }

        private static void BuildGate(ToyDirection look, Material toy, Material good, Material bad, Mesh blobMesh, Material blob)
        {
            var frame = new SoftMeshBuilder();
            Color white = Gloss(look.GateFrame, 0.9f);
            Color gold = Gloss(look.SoldierAccent, 1f);
            for (int post = -1; post <= 1; post++)
            {
                float x = post * 4.25f;
                frame.Drum(new Vector3(x, 0f, 0f), 0.24f, 2.62f, 0.1f, white, 16);
                frame.Ellipsoid(new Vector3(x, 2.92f, 0f), Vector3.one * 0.3f, gold, 14, 9);
                PlaceBlob(blobMesh, blob, new Vector3(x, 0.012f, GateZ), 1.1f);
            }

            // The game font is dynamic: fill its atlas first, or a rebuild during the render leaves labels with stale glyphs.
            AssetDatabase.LoadAssetAtPath<Font>(ClaybreakSceneBuilder.FontPath).RequestCharactersInTexture("+-0123456789", 64);
            frame.RoundedBox(new Vector3(0f, 2.58f, 0f), new Vector3(8.9f, 0.38f, 0.38f), 0.18f, white, 6);
            Place("GateFrame", frame.Build("GateFrame"), toy, new Vector3(0f, 0f, GateZ), Quaternion.identity);
            for (int side = -1; side <= 1; side += 2)
            {
                Mesh panel = new SoftMeshBuilder().RoundedBox(Vector3.zero, new Vector3(3.95f, 2.25f, 0.16f), 0.08f, Color.white, 4)
                    .Build("GatePanel");
                Place(side < 0 ? "GoodPanel" : "BadPanel", panel, side < 0 ? good : bad, new Vector3(side * 2.13f, 1.27f, GateZ),
                    Quaternion.identity);
                string text = side < 0 ? "+20" : "-10";
                TextMesh shadow = ClaybreakSceneBuilder.CreateLabel("LabelShadow", null,
                    new Vector3(side * 2.13f + 0.06f, 1.26f, GateZ - 0.11f), 0.75f);
                shadow.text = text;
                shadow.color = look.LabelShadow;
                TextMesh label = ClaybreakSceneBuilder.CreateLabel("Label", null, new Vector3(side * 2.13f, 1.32f, GateZ - 0.14f), 0.75f);
                label.text = text;
            }
        }

        // The soldier: a round glossy bean with a helmet, plume, feet, a fist and a shield; no face (rule of D17). One mesh, smooth normals.
        private static SoftMeshBuilder BuildSoldier(ToyDirection look)
        {
            var mesh = new SoftMeshBuilder();
            Color body = Gloss(look.Soldier, 0.9f);
            Color accent = Gloss(look.SoldierAccent, 1f);
            Color dark = Gloss(look.SoldierFeet, 0.7f);
            for (int side = -1; side <= 1; side += 2)
            {
                mesh.Ellipsoid(new Vector3(side * 0.13f, 0.07f, 0.03f), new Vector3(0.11f, 0.075f, 0.15f), dark, 10, 6);
            }

            mesh.Ellipsoid(new Vector3(0f, 0.42f, 0f), new Vector3(0.3f, 0.34f, 0.28f), body, 14, 10);
            mesh.Dome(new Vector3(0f, 0.55f, 0f), 0.31f, 0.27f, Gloss(look.SoldierHelmet, 1f), 0f, 14);
            mesh.Ring(new Vector3(0f, 0.56f, 0f), 0.3f, 0.045f, accent, 14);
            mesh.Ellipsoid(new Vector3(0f, 0.84f, -0.01f), new Vector3(0.055f, 0.08f, 0.13f), accent, 8, 6);
            mesh.Ellipsoid(new Vector3(0.3f, 0.34f, 0.07f), Vector3.one * 0.075f, body, 8, 6);
            mesh.Ellipsoid(new Vector3(-0.31f, 0.4f, 0.04f), new Vector3(0.04f, 0.15f, 0.15f), accent, 12, 8);
            return mesh;
        }

        // ---------- shared helpers ----------

        // A compact sunflower cluster around the leader, as the crowd gathers on the track.
        private static IEnumerable<Pose> CrowdPoses()
        {
            var random = new System.Random(3);
            for (int i = 0; i < CrowdSize; i++)
            {
                float radius = 0.43f * Mathf.Sqrt(i + 0.5f);
                float angle = i * 137.508f * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, 0.6f + Mathf.Sin(angle) * radius * 0.85f);
                yield return new Pose(position, Quaternion.Euler(0f, Range(random, -18f, 18f), 0f));
            }
        }

        private static Camera NewCamera()
        {
            var cameraObject = new GameObject("Camera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.farClipPlane = 1000f;
            return camera;
        }

        private static Color[] Shoot(Framing framing, int width, int height, string folder, string name)
        {
            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            camera.fieldOfView = framing.FieldOfView;
            camera.transform.position = framing.Position;
            camera.transform.LookAt(framing.Target);
            // A scene made in this frame has not registered its sun yet: the first render warms it up.
            UnityEngine.Object.DestroyImmediate(ArtDirectionCapture.Render(camera, 64, 64));
            Texture2D shot = ArtDirectionCapture.Render(camera, width, height);
            if (name != null)
            {
                byte[] png = shot.EncodeToPNG();
                // Single shots stay in the capture folder; the repository keeps only the side-by-side sheets.
                File.WriteAllBytes(Path.Combine(folder, name + ".png"), png);
                Debug.Log("[DirectionV2] " + name);
            }

            Color[] pixels = shot.GetPixels();
            UnityEngine.Object.DestroyImmediate(shot);
            return pixels;
        }

        private static void WriteSheet(string folder, string name, Color[][] panels, int width, int height, bool letters)
        {
            const int gap = 12;
            int sheetWidth = width * panels.Length + gap * (panels.Length - 1);
            var sheet = new Texture2D(sheetWidth, height, TextureFormat.RGB24, false);
            var fill = new Color[gap * height];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = Color.white;
            }

            for (int i = 0; i < panels.Length; i++)
            {
                sheet.SetPixels(i * (width + gap), 0, width, height, panels[i]);
                if (i > 0)
                {
                    sheet.SetPixels(i * (width + gap) - gap, 0, gap, height, fill);
                }

                if (letters)
                {
                    ArtDirectionCapture.DrawLetter(sheet, i * (width + gap) + 24, height - 110, i);
                }
            }

            sheet.Apply();
            byte[] png = sheet.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(folder, name + ".png"), png);
            File.WriteAllBytes(Path.Combine(OutputFolder, name + ".png"), png);
            UnityEngine.Object.DestroyImmediate(sheet);
            Debug.Log("[DirectionV2] " + name);
        }

        private static GameObject Place(string name, Mesh mesh, Material material, Vector3 position, Quaternion rotation)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetPositionAndRotation(position, rotation);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }

        private static void PlaceBlob(Mesh quad, Material blob, Vector3 position, float size)
        {
            Place("Blob", quad, blob, position, Quaternion.identity).transform.localScale = Vector3.one * size;
        }

        private static Mesh BlobQuad()
        {
            var mesh = new Mesh { name = "Blob", hideFlags = HideFlags.DontSave };
            mesh.SetVertices(new[] { new Vector3(-0.5f, 0f, -0.5f), new Vector3(-0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, -0.5f) });
            mesh.SetUVs(0, new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) });
            mesh.SetNormals(new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up });
            mesh.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        // Vertex colors are not converted from sRGB like material colors: convert them here in a linear project.
        private static Color Gloss(Color color, float gloss)
        {
            Color c = QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
            return new Color(c.r, c.g, c.b, gloss);
        }

        private static float Range(System.Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);

        private static Color Pick(System.Random random, Color[] colors) => colors[random.Next(colors.Length)];
    }
}
