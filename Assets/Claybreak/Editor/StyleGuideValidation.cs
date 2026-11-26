using System;
using System.Collections.Generic;
using System.IO;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The validator of the <see cref="StyleGuide"/> (RT-6v3.1, <c>docs/direcao-visual-v3.md</c> 2.2), called by
    /// <see cref="ClaybreakValidation"/>. Rules that are ALWAYS on: the guide covers every class; every material asset, effect and
    /// clip of the game has an entry (and no entry points at nothing); every clip is listed in docs/audio-licenses.md; a legacy
    /// profile's shaders match the material's; every mesh and scene-built object fits the class budget; the palettes of the guide
    /// are the game's. Rules that are ON ONLY for a class marked <see cref="StyleStatus.V3"/> (each art delivery turns on its own):
    /// the material's profile is a v3 one and accepts its shader; the mesh is inside the v3 triangle range and draw-call budget and
    /// has no edge sharper than the class's limit; a palette color is within ΔE (CIE76) of one of its three levels, measured before
    /// the post effects; an effect's pool is within its limit; a clip's peak is at the guide's target.
    /// RT-6v3.3: a class can be v3 on its SURFACE before its meshes (<see cref="ClassStyle.surface"/>): its materials follow a v3
    /// profile and hold the profile's vinyl surface; nothing of the floor or the scenery shines on top; each world shows the
    /// guide's light, post and colored fog; the UI is outside the post; and the D36 contrast logic is a rule, on the palette
    /// (before the post) and on the captures of the four worlds (after it, <see cref="StyleContrast.MeasuresPath"/>).
    /// </summary>
    public static class StyleGuideValidation
    {
        /// <summary>The failures of the guide asset against the game as built now (an empty list passes).</summary>
        public static List<string> Run()
        {
            var guide = AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);
            if (guide == null)
            {
                return new List<string> { $"style: {StyleGuide.AssetPath} is missing (run Claybreak > Build Scene or Claybreak > Write Style Guide)" };
            }

            return Run(guide, StyleFacts.Gather());
        }

        /// <summary>The failures of <paramref name="guide"/> against <paramref name="facts"/>.</summary>
        public static List<string> Run(StyleGuide guide, StyleFacts facts)
        {
            var failures = new List<string>();
            CheckClasses(guide, failures);
            CheckProfiles(guide, failures);
            CheckMaterials(guide, facts, failures);
            CheckMeshes(guide, facts, failures);
            CheckPalettes(guide, facts, failures);
            CheckEffects(guide, facts, failures);
            CheckSounds(guide, facts, failures);
            string faces = guide.Faces == null ? "no face table" : guide.Faces.Validate();
            if (faces != null)
            {
                failures.Add("style faces (RT-6v3.4b): " + faces);
            }

            CheckLooks(guide, facts, failures);
            CheckPaletteContrast(guide, failures);
            CheckCapturedContrast(guide, facts.Measures, failures);
            return failures;
        }

        /// <summary>One line per class with the largest mesh and draw-call count measured (the log the legacy ceilings are set from).</summary>
        public static string Measures(StyleFacts facts)
        {
            var parts = new List<string>();
            foreach (StyleClass styleClass in Enum.GetValues(typeof(StyleClass)))
            {
                int triangles = 0;
                int calls = 0;
                float edge = 0f;
                int count = 0;
                foreach (MeshFact mesh in facts.Meshes)
                {
                    if (mesh.styleClass == styleClass)
                    {
                        count++;
                        triangles = Math.Max(triangles, mesh.triangles);
                        calls = Math.Max(calls, mesh.drawCalls);
                        edge = Mathf.Max(edge, mesh.maxEdgeAngle);
                    }
                }

                if (count > 0)
                {
                    parts.Add($"{styleClass}: {count} meshes, most {triangles} triangles / {calls} draws, sharpest edge {edge:0}°");
                }
            }

            return string.Join("; ", parts);
        }

        // ---------- always on ----------

        private static void CheckClasses(StyleGuide guide, List<string> failures)
        {
            foreach (StyleClass styleClass in Enum.GetValues(typeof(StyleClass)))
            {
                ClassStyle entry = guide.ClassOf(styleClass);
                if (entry == null)
                {
                    failures.Add($"style: the guide has no entry for the class {styleClass}");
                    continue;
                }

                if (entry.maxTriangles < 0 || entry.maxDrawCalls < 0 || entry.v3MinTriangles > entry.v3MaxTriangles || entry.maxEdgeAngle <= 0f
                    || entry.reserveMinTriangles > entry.reserveMaxTriangles)
                {
                    failures.Add($"style: the class {styleClass} has impossible limits");
                }
            }
        }

        private static void CheckProfiles(StyleGuide guide, List<string> failures)
        {
            var seen = new HashSet<string>();
            foreach (MaterialProfile profile in guide.Profiles)
            {
                if (string.IsNullOrEmpty(profile.id) || !seen.Add(profile.id))
                {
                    failures.Add($"style: material profile '{profile.id}' is empty or repeated");
                }
            }
        }

        private static void CheckMaterials(StyleGuide guide, StyleFacts facts, List<string> failures)
        {
            var present = new HashSet<string>();
            foreach (MaterialFact material in facts.Materials)
            {
                present.Add(material.name);
                MaterialStyle entry = guide.MaterialNamed(material.name);
                if (entry == null)
                {
                    failures.Add($"style: material {material.name} has no entry in the guide");
                    continue;
                }

                MaterialProfile profile = guide.ProfileOf(entry.profile);
                if (profile == null)
                {
                    failures.Add($"style: material {material.name} names the profile '{entry.profile}', which the guide does not have");
                    continue;
                }

                bool v3 = guide.SurfaceIsV3(entry.styleClass);
                if (v3 && profile.legacy)
                {
                    failures.Add($"style: material {material.name} is in the v3 class {entry.styleClass} but follows the legacy profile '{profile.id}'");
                }
                else if ((v3 || profile.legacy || profile.vinyl) && !profile.Accepts(material.shader))
                {
                    failures.Add($"style: material {material.name} uses shader '{material.shader}', outside the profile '{profile.id}'");
                }
                else if (profile.vinyl)
                {
                    CheckSurface(entry, profile, material, failures);
                }
            }

            foreach (MaterialStyle entry in guide.Materials)
            {
                if (!present.Contains(entry.material))
                {
                    failures.Add($"style: the guide lists material {entry.material}, which the game does not have");
                }
            }
        }

        // RT-6v3.3: a vinyl material holds its profile's surface (rebuild the scene after a change), and nothing of the floor or
        // the scenery is glossy on faces looking up (RT-6v3.2 review: the track and the ground shone at a grazing angle).
        private static void CheckSurface(MaterialStyle entry, MaterialProfile profile, MaterialFact material, List<string> failures)
        {
            ToonVinylSurface surface = profile.surface;
            string error = surface.Validate();
            if (error != null)
            {
                failures.Add($"style: profile '{profile.id}' has an unusable surface: {error}");
                return;
            }

            var expected = new Vector4(surface.Gloss, surface.SpecSize, surface.Rim, surface.TopGloss);
            if (!material.vinyl || (material.surface - expected).sqrMagnitude > 1e-6f || Mathf.Abs(material.darken - surface.Darken) > 1e-3f
                || material.translucent != surface.Translucent)
            {
                failures.Add($"style: material {material.name} does not hold the surface of the profile '{profile.id}' (rebuild the scene)");
            }

            if ((entry.styleClass == StyleClass.Floor || entry.styleClass == StyleClass.Scenery) && surface.TopGloss > 0f)
            {
                failures.Add($"style: {entry.styleClass} material {material.name} shines on faces looking up (profile '{profile.id}' top gloss {surface.TopGloss:0.##}; the floor is matte)");
            }
        }

        private static void CheckMeshes(StyleGuide guide, StyleFacts facts, List<string> failures)
        {
            foreach (string path in facts.UnclassifiedMeshes)
            {
                failures.Add($"style: mesh asset {path} belongs to no style class (add it to StyleFacts.ClassOfMeshAsset)");
            }

            foreach (MeshFact mesh in facts.Meshes)
            {
                ClassStyle limits = guide.ClassOf(mesh.styleClass);
                if (limits == null)
                {
                    continue;
                }

                // The ceiling of what the game draws today holds until the class migrates; from then on the v3 budget replaces it.
                // RT-6v3.4a: a mesh already remade in v3 (or the class's reserve) answers to its own budget before the class does.
                if (mesh.tier == MeshTier.Reserve)
                {
                    if (limits.reserveMaxTriangles == 0 || mesh.triangles < limits.reserveMinTriangles || mesh.triangles > limits.reserveMaxTriangles)
                    {
                        failures.Add($"style: reserve {mesh.styleClass} {mesh.name} has {mesh.triangles} triangles, outside the reserve "
                            + $"{limits.reserveMinTriangles}-{limits.reserveMaxTriangles}");
                    }
                }
                else if (limits.status != StyleStatus.V3 && mesh.tier != MeshTier.V3)
                {
                    if (mesh.triangles > limits.maxTriangles)
                    {
                        failures.Add($"style: {mesh.styleClass} {mesh.name} has {mesh.triangles} triangles, over the {limits.maxTriangles} budget");
                    }

                    if (mesh.drawCalls > limits.maxDrawCalls)
                    {
                        failures.Add($"style: {mesh.styleClass} {mesh.name} takes {mesh.drawCalls} draws, over the {limits.maxDrawCalls} budget");
                    }

                    continue;
                }
                else if (mesh.triangles < limits.v3MinTriangles || mesh.triangles > limits.v3MaxTriangles)
                {
                    failures.Add($"style: v3 {mesh.styleClass} {mesh.name} has {mesh.triangles} triangles, outside {limits.v3MinTriangles}-{limits.v3MaxTriangles}");
                }

                if (mesh.drawCalls > limits.v3MaxDrawCalls)
                {
                    failures.Add($"style: v3 {mesh.styleClass} {mesh.name} takes {mesh.drawCalls} draws, over the {limits.v3MaxDrawCalls} of the v3 budget");
                }

                if (mesh.maxEdgeAngle > limits.maxEdgeAngle)
                {
                    failures.Add($"style: v3 {mesh.styleClass} {mesh.name} has an unbevelled edge ({mesh.maxEdgeAngle:0}° between faces, over {limits.maxEdgeAngle:0}°)");
                }
            }
        }

        private static void CheckPalettes(StyleGuide guide, StyleFacts facts, List<string> failures)
        {
            string[] names = ToonPalette.ColorNames;
            for (int w = 0; w < guide.Worlds.Length; w++)
            {
                WorldStyle world = guide.Worlds[w];
                foreach (string name in names)
                {
                    if (RoleOf(world.roles, name) == null)
                    {
                        failures.Add($"style: world {w + 1} ({world.name}) has no role '{name}'");
                    }
                }
            }

            foreach (ColorFact fact in facts.Colors)
            {
                PaletteRole role = fact.world < 0
                    ? RoleOf(guide.SharedRoles, fact.role)
                    : fact.world < guide.Worlds.Length ? RoleOf(guide.Worlds[fact.world].roles, fact.role) : null;
                string where = fact.world < 0 ? fact.role : $"world {fact.world + 1} {fact.role}";
                if (role == null)
                {
                    failures.Add($"style: the guide has no palette role for {where}");
                    continue;
                }

                if (!guide.SurfaceIsV3(role.styleClass))
                {
                    continue;
                }

                float nearest = float.MaxValue;
                for (int level = 0; level < ColorLevels.Count; level++)
                {
                    nearest = Mathf.Min(nearest, ToonContrast.DeltaE(fact.color, role.levels[level]));
                }

                if (nearest > guide.ColorTolerance)
                {
                    failures.Add($"style: v3 {role.styleClass} color {where} is ΔE {nearest:0.0} from the nearest of its three levels (over {guide.ColorTolerance:0.#})");
                }
            }
        }

        // ---------- RT-6v3.3: light, post and fog per world; the UI outside the post ----------

        /// <summary>Least CIELAB chroma of a fog that is not "white" (D36: colored fog), unless it is dark (L* under 70).</summary>
        public const float FogMinChroma = 10f;

        private static void CheckLooks(StyleGuide guide, StyleFacts facts, List<string> failures)
        {
            if (!facts.UiOutsidePost)
            {
                failures.Add("style: the game canvas must be a screen-space overlay: the UI stays outside the post (no color filter on the buttons)");
            }

            if (!facts.CameraPost)
            {
                failures.Add("style: the game camera does not render the post stack (renderPostProcessing)");
            }

            for (int w = 0; w < guide.Worlds.Length; w++)
            {
                WorldStyle world = guide.Worlds[w];
                string where = $"world {w + 1} ({world.name})";
                WorldLookFact look = default;
                bool found = false;
                foreach (WorldLookFact each in facts.Looks)
                {
                    if (each.world == w)
                    {
                        look = each;
                        found = true;
                    }
                }

                if (!found)
                {
                    failures.Add($"style: {where} shows no light, post or fog (rebuild the scene)");
                    continue;
                }

                if (!StyleGuideContent.LookMatches(look.light, world.light))
                {
                    failures.Add($"style: {where} shows a vinyl light that is not the guide's (rebuild the scene)");
                }

                if (!StyleGuideContent.PostMatches(look.post, world.post))
                {
                    failures.Add($"style: {where} shows a post stack that is not the guide's (rebuild the scene)");
                }

                if (!Mathf.Approximately(look.fogStart, world.fogStart) || !Mathf.Approximately(look.fogEnd, world.fogEnd) || look.fogEnd <= look.fogStart)
                {
                    failures.Add($"style: {where} fog {look.fogStart:0}-{look.fogEnd:0} m, the guide says {world.fogStart:0}-{world.fogEnd:0} m");
                }

                if (StyleContrast.Chroma(look.fog) < FogMinChroma && StyleContrast.Lightness(look.fog) >= 70f)
                {
                    failures.Add($"style: {where} fog #{ColorUtility.ToHtmlStringRGB(look.fog)} is a white veil (D36: colored fog)");
                }
            }
        }

        // ---------- RT-6v3.3: the D36 contrast logic ----------

        // The colors of a world drawn in play (not the UI, not the fog), against which the crowd must be the most saturated.
        private static readonly string[] DrawnRoles =
        {
            "skyTop", "skyBottom", "ground", "track", "trackStripe", "foliageDark", "foliageLight", "trunk", "rock",
            "structureLight", "structureDark", "structureDetail", "defense",
        };

        /// <summary>The palette, before the post: the crowd's chroma, the neutral track far in value, the defenses across the hue wheel.</summary>
        private static void CheckPaletteContrast(StyleGuide guide, List<string> failures)
        {
            ContrastRule rule = guide.Contrast;
            if (rule == null)
            {
                failures.Add("style: the guide has no contrast rule");
                return;
            }

            for (int w = 0; w < guide.Worlds.Length; w++)
            {
                WorldStyle world = guide.Worlds[w];
                string where = $"world {w + 1} ({world.name})";
                PaletteRole crowd = RoleOf(world.roles, "crowd");
                PaletteRole track = RoleOf(world.roles, "track");
                PaletteRole defense = RoleOf(world.roles, "defense");
                if (crowd == null || track == null || defense == null)
                {
                    continue; // CheckPalettes reports the missing role
                }

                Color crowdColor = crowd.levels.baseColor;
                float crowdChroma = StyleContrast.Chroma(crowdColor);
                var rivals = new List<PaletteRole>();
                foreach (string name in DrawnRoles)
                {
                    PaletteRole role = RoleOf(world.roles, name);
                    if (role != null)
                    {
                        rivals.Add(role);
                    }
                }

                rivals.AddRange(guide.SharedRoles);
                foreach (PaletteRole rival in rivals)
                {
                    float chroma = StyleContrast.Chroma(rival.levels.baseColor);
                    if (crowdChroma < chroma + rule.crowdChromaMargin)
                    {
                        failures.Add($"style: {where} crowd chroma {crowdChroma:0} is not {rule.crowdChromaMargin:0} above {rival.role} ({chroma:0}): the crowd must be the most saturated color (D36)");
                    }
                }

                foreach (string name in new[] { "track", "trackStripe" })
                {
                    PaletteRole role = RoleOf(world.roles, name);
                    float chroma = role == null ? 0f : StyleContrast.Chroma(role.levels.baseColor);
                    if (chroma > rule.trackMaxChroma)
                    {
                        failures.Add($"style: {where} {name} chroma {chroma:0} over {rule.trackMaxChroma:0}: the track is neutral (D36)");
                    }
                }

                float gap = Mathf.Abs(StyleContrast.Lightness(crowdColor) - StyleContrast.Lightness(track.levels.baseColor));
                if (gap < rule.crowdTrackMinLightness)
                {
                    failures.Add($"style: {where} crowd and track are {gap:0} apart in L*, under {rule.crowdTrackMinLightness:0} (D36: far in value)");
                }

                float hue = StyleContrast.HueGap(StyleContrast.Hue(crowdColor), StyleContrast.Hue(defense.levels.baseColor));
                if (hue < rule.enemyMinHue)
                {
                    failures.Add($"style: {where} defenses are {hue:0}° of hue from the crowd, under {rule.enemyMinHue:0}° (D36: complementary)");
                }
            }
        }

        /// <summary>
        /// The captures of the four worlds, after the light and the post (<c>StyleCaptureTests</c> writes them): taken with this
        /// look, every world measured, the crowd the most saturated role on screen, far from the track in value, the enemies
        /// across the hue wheel, the gate numbers legible and every skin apart from the track.
        /// </summary>
        public static void CheckCapturedContrast(StyleGuide guide, ContrastMeasures measures, List<string> failures)
        {
            ContrastRule rule = guide.Contrast;
            if (rule == null)
            {
                return;
            }

            const string again = "run the style capture: -testFilter Claybreak.Tests.StyleCaptureTests -styleCaptureDir <folder>";
            if (measures == null)
            {
                failures.Add($"style: {StyleContrast.MeasuresPath} is missing ({again})");
                return;
            }

            if (measures.fingerprint != guide.LookFingerprint())
            {
                failures.Add($"style: {StyleContrast.MeasuresPath} was measured with another look (palette, light, post or surfaces changed; {again})");
                return;
            }

            for (int w = 0; w < guide.Worlds.Length; w++)
            {
                bool run = false;
                foreach (WorldMeasure shot in measures.shots)
                {
                    if (shot.world != w)
                    {
                        continue;
                    }

                    run |= shot.shot == "corrida";
                    CheckShot(rule, shot, $"world {w + 1} {shot.shot}", failures);
                }

                if (!run)
                {
                    failures.Add($"style: world {w + 1} has no measured run in {StyleContrast.MeasuresPath} ({again})");
                }
            }
        }

        private static void CheckShot(ContrastRule rule, WorldMeasure shot, string where, List<string> failures)
        {
            RoleMeasure crowd = shot.Role(StyleContrast.Crowd);
            if (crowd == null || crowd.pixels == 0)
            {
                failures.Add($"style: {where}: no crowd on screen in the capture");
                return;
            }

            foreach (RoleMeasure role in shot.roles)
            {
                if (role.role != StyleContrast.Crowd && role.pixels > 0 && crowd.chroma < role.chroma + rule.capturedCrowdChromaMargin)
                {
                    failures.Add($"style: {where}: on screen the crowd's chroma {crowd.chroma:0} is not above the {role.role}'s ({role.chroma:0}) (D36)");
                }
            }

            RoleMeasure track = shot.Role(StyleContrast.Track);
            if (track != null && track.pixels > 0 && Mathf.Abs(crowd.lightness - track.lightness) < rule.capturedCrowdTrackMinLightness)
            {
                failures.Add($"style: {where}: on screen crowd and track are {Mathf.Abs(crowd.lightness - track.lightness):0} apart in L*, under {rule.capturedCrowdTrackMinLightness:0}");
            }

            RoleMeasure enemy = shot.Role(StyleContrast.Enemy);
            if (enemy != null && enemy.pixels >= StyleContrast.MinimumHuePixels && StyleContrast.HueGap(crowd.hue, enemy.hue) < rule.capturedEnemyMinHue)
            {
                failures.Add($"style: {where}: on screen the enemies are {StyleContrast.HueGap(crowd.hue, enemy.hue):0}° of hue from the crowd, under {rule.capturedEnemyMinHue:0}°");
            }

            if (shot.shot == "corrida" && shot.gateLabelRatio < rule.gateLabelMinRatio)
            {
                failures.Add($"style: {where}: the gate numbers measure {shot.gateLabelRatio:0.00}:1 between fill and outline, under {rule.gateLabelMinRatio:0.#}:1");
            }

            foreach (SkinMeasure skin in shot.skins)
            {
                if (skin.deltaE < rule.skinMinDeltaE)
                {
                    failures.Add($"style: {where}: skin {skin.skin} is ΔE {skin.deltaE:0} from the track on screen, under {rule.skinMinDeltaE:0}");
                }
            }
        }

        private static PaletteRole RoleOf(PaletteRole[] roles, string name)
        {
            if (roles == null)
            {
                return null;
            }

            foreach (PaletteRole role in roles)
            {
                if (role.role == name)
                {
                    return role;
                }
            }

            return null;
        }

        private static void CheckEffects(StyleGuide guide, StyleFacts facts, List<string> failures)
        {
            bool v3 = guide.IsV3(StyleClass.Effect);
            foreach (StyleEffect effect in Enum.GetValues(typeof(StyleEffect)))
            {
                EffectStyle entry = guide.EffectOf(effect);
                if (entry == null)
                {
                    failures.Add($"style: effect {effect} has no entry in the guide");
                    continue;
                }

                if (v3 && facts.EffectCapacity.TryGetValue(effect, out int capacity) && capacity > entry.maxActive)
                {
                    failures.Add($"style: v3 effect {effect} preallocates {capacity}, over its limit of {entry.maxActive}");
                }
            }
        }

        private static void CheckSounds(StyleGuide guide, StyleFacts facts, List<string> failures)
        {
            bool v3 = guide.IsV3(StyleClass.Sound);
            var present = new HashSet<string>();
            foreach (ClipFact clip in facts.Clips)
            {
                present.Add(clip.name);
                SoundStyle entry = guide.SoundOf(clip.name);
                if (entry == null)
                {
                    failures.Add($"style: clip {clip.name} has no entry in the guide");
                    continue;
                }

                if (!clip.licensed)
                {
                    failures.Add($"style: clip {clip.name} is not listed in {ClaybreakAudio.LicensesPath}");
                }

                if (v3 && Mathf.Abs(clip.peakDb - entry.peakDb) > entry.peakToleranceDb)
                {
                    failures.Add($"style: v3 clip {clip.name} peaks at {clip.peakDb:0.0} dBFS, not within {entry.peakToleranceDb:0.#} dB of {entry.peakDb:0.#}");
                }

                if (v3 && entry.loudnessToleranceDb > 0f && Mathf.Abs(clip.loudnessDb - entry.loudnessDb) > entry.loudnessToleranceDb)
                {
                    failures.Add($"style: v3 clip {clip.name} has volume {clip.loudnessDb:0.0} dBFS, not within {entry.loudnessToleranceDb:0.#} dB of {entry.loudnessDb:0.#}");
                }

                if (v3 && entry.maxSeconds > 0f && (clip.seconds < entry.minSeconds || clip.seconds > entry.maxSeconds))
                {
                    failures.Add($"style: v3 clip {clip.name} lasts {clip.seconds:0.00} s, outside {entry.minSeconds:0.##}-{entry.maxSeconds:0.##} s");
                }
            }

            foreach (SoundStyle entry in guide.Sounds)
            {
                if (!present.Contains(entry.clip))
                {
                    failures.Add($"style: the guide lists clip {entry.clip}, which the game does not have");
                }
            }
        }
    }
}
