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

                if (entry.maxTriangles < 0 || entry.maxDrawCalls < 0 || entry.v3MinTriangles > entry.v3MaxTriangles || entry.maxEdgeAngle <= 0f)
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

                bool v3 = guide.IsV3(entry.styleClass);
                if (v3 && profile.legacy)
                {
                    failures.Add($"style: material {material.name} is in the v3 class {entry.styleClass} but follows the legacy profile '{profile.id}'");
                }
                else if ((v3 || profile.legacy) && !profile.Accepts(material.shader))
                {
                    failures.Add($"style: material {material.name} uses shader '{material.shader}', outside the profile '{profile.id}'");
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
                if (limits.status != StyleStatus.V3)
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

                if (mesh.triangles < limits.v3MinTriangles || mesh.triangles > limits.v3MaxTriangles)
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

                if (!guide.IsV3(role.styleClass))
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
