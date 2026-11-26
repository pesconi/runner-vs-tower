using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The art-review checklist, generated from the <see cref="StyleGuide"/> (RT-6v3.1, docs 6): one line per rule, so the agent that
    /// critiques the captures judges by the guide and invents no criterion. <c>docs/art/v3/checklist.md</c> is rewritten by
    /// <c>-executeMethod Claybreak.Editor.StyleChecklist.WriteAndQuit</c> (and by the scene build); a test fails when the file is out
    /// of date with the guide.
    /// </summary>
    public static class StyleChecklist
    {
        public const string Path = "docs/art/v3/checklist.md";

        public static string Generate(StyleGuide guide)
        {
            var text = new StringBuilder();
            text.Append("# Checklist de estilo (gerado)\n\n");
            text.Append("Gerado de `StyleGuide` por `Claybreak.Editor.StyleChecklist`. Não editar à mão: mude o guia em ");
            text.Append("`StyleGuideContent` e rode `Tools/rt4-check.sh` (passo `scene`) ou `-executeMethod Claybreak.Editor.StyleChecklist.WriteAndQuit`.\n");
            text.Append("Cada linha é uma regra que a captura deve cumprir; classe `legado` = ainda no visual anterior (o guia descreve o que o jogo tem hoje).\n\n");

            text.Append("## Classes\n\n");
            foreach (ClassStyle entry in guide.Classes)
            {
                text.Append($"- [ ] **{entry.styleClass}** (forma {Status(entry.status)}, superfície {Status(entry.surface)}): ");
                if (entry.maxTriangles == 0)
                {
                    text.Append("sem malha própria.\n");
                    continue;
                }

                text.Append($"teto atual {entry.maxTriangles} triângulos e {entry.maxDrawCalls} chamadas por objeto");
                text.Append($"; meta v3 {entry.v3MinTriangles}–{entry.v3MaxTriangles} triângulos, até {entry.v3MaxDrawCalls} chamadas, ");
                if (entry.reserveMaxTriangles > 0)
                {
                    text.Append($"reserva {entry.reserveMinTriangles}–{entry.reserveMaxTriangles} triângulos, ");
                }

                text.Append($"nenhuma aresta viva acima de {entry.maxEdgeAngle:0}° (chanfro).\n");
            }

            text.Append("\n## Paletas (antes do pós; tolerância ΔE CIE76 ≤ ");
            text.Append(guide.ColorTolerance.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture));
            text.Append(" sobre os três níveis: base, sombra, destaque)\n\n");
            foreach (WorldStyle world in guide.Worlds)
            {
                text.Append($"### {world.name}\n\n");
                text.Append("| Papel | Classe | Base | Sombra | Destaque |\n|---|---|---|---|---|\n");
                foreach (PaletteRole role in world.roles)
                {
                    text.Append(RoleRow(role));
                }

                text.Append('\n');
                WorldLight light = world.light;
                WorldPost post = world.post;
                text.Append($"- [ ] Clima: {world.mood}\n");
                text.Append($"- [ ] Luz (vinil): sol {Vector(world.sunEuler)}, principal {Hex(light.keyColor)} × {Number(light.keyIntensity)} (direta {Number(light.directIntensity)}); ");
                text.Append($"ambiente céu {Hex(light.skyAmbient)} chão {Hex(light.groundAmbient)} × {Number(light.ambientIntensity)}; sombra {Hex(light.shadowTint)}; ");
                text.Append($"contorno {Hex(light.rimColor)} × {Number(light.rimStrength)}; rampa {Number(light.softness)}; ombro {Number(light.shoulderKnee)}; pico {Number(light.highlightPeak)}.\n");
                text.Append($"- [ ] Névoa na cor `fog` da paleta, {Number(world.fogStart)}–{Number(world.fogEnd)} m.\n");
                text.Append($"- [ ] Pós: exposição {Number(post.exposure)}, contraste {Number(post.contrast)}, saturação {Number(post.saturation)}, ");
                text.Append($"vinheta {Number(post.vignette)} ({Hex(post.vignetteColor)}), bloom {Number(post.bloom)} acima de {Number(post.bloomThreshold)}; sem tonemapping; UI fora do pós.\n\n");
            }

            ContrastRule contrast = guide.Contrast;
            text.Append("## Contraste (D36, CIELAB; paleta antes do pós e captura dos 4 mundos depois dele)\n\n");
            text.Append($"- [ ] A multidão tem o maior croma da tela: ≥ {Number(contrast.crowdChromaMargin)} acima de qualquer outra cor do mundo na paleta; ");
            text.Append($"≥ {Number(contrast.capturedCrowdChromaMargin)} acima de pista, chão, cenário, inimigos, torre e céu na captura.\n");
            text.Append($"- [ ] Pista neutra (croma ≤ {Number(contrast.trackMaxChroma)}) e longe da multidão em valor: |ΔL*| ≥ {Number(contrast.crowdTrackMinLightness)} na paleta, ");
            text.Append($"≥ {Number(contrast.capturedCrowdTrackMinLightness)} na captura.\n");
            text.Append($"- [ ] Inimigos na cor complementar: matiz a ≥ {Number(contrast.enemyMinHue)}° da multidão na paleta, ≥ {Number(contrast.capturedEnemyMinHue)}° na captura.\n");
            text.Append($"- [ ] Números dos portões legíveis: preenchimento × contorno ≥ {Number(contrast.gateLabelMinRatio)}:1 na captura.\n");
            text.Append($"- [ ] Toda skin a ΔE ≥ {Number(contrast.skinMinDeltaE)} da pista na captura (depois do pós).\n\n");

            text.Append("### Cores iguais em todos os mundos\n\n| Papel | Classe | Base | Sombra | Destaque |\n|---|---|---|---|---|\n");
            foreach (PaletteRole role in guide.SharedRoles)
            {
                text.Append(RoleRow(role));
            }

            text.Append("\n## Materiais\n\n");
            foreach (MaterialProfile profile in guide.Profiles)
            {
                text.Append($"- [ ] Perfil `{profile.id}` ({(profile.legacy ? "legado" : "v3")}): {profile.note}");
                text.Append(profile.shaders != null && profile.shaders.Length > 0 ? $" Shaders: {string.Join(", ", profile.shaders)}." : " Sem shader ainda.");
                if (profile.vinyl)
                {
                    ToonVinylSurface surface = profile.surface;
                    text.Append($" Superfície: brilho {Number(surface.Gloss)} (tamanho {Number(surface.SpecSize)}, no topo {Number(surface.TopGloss)}), ");
                    text.Append($"contorno {Number(surface.Rim)} {Hex(surface.RimTint)}, escurecer {Number(surface.Darken)}{(surface.Translucent ? $", translúcido {Number(surface.Alpha)}" : "")}.");
                }

                text.Append('\n');
            }

            text.Append('\n');
            foreach (StyleClass styleClass in Enum.GetValues(typeof(StyleClass)))
            {
                var names = new List<string>();
                foreach (MaterialStyle entry in guide.Materials)
                {
                    if (entry.styleClass == styleClass)
                    {
                        names.Add($"{entry.material} ({entry.profile})");
                    }
                }

                if (names.Count > 0)
                {
                    text.Append($"- [ ] **{styleClass}** usa só: {string.Join(", ", names)}.\n");
                }
            }

            text.Append("\n## Efeitos (limite de ativos ao mesmo tempo)\n\n");
            foreach (EffectStyle effect in guide.Effects)
            {
                text.Append($"- [ ] **{effect.effect}**: até {effect.maxActive} ({effect.note}).\n");
            }

            text.Append("\n## Sons (pico alvo ± tolerância, antes do mixer)\n\n");
            foreach (SoundStyle sound in guide.Sounds)
            {
                text.Append($"- [ ] `{sound.clip}`: {sound.playedBy}; pico {sound.peakDb:0.#} dBFS ± {sound.peakToleranceDb:0.#} dB; volume {sound.loudnessDb:0.#} ± {sound.loudnessToleranceDb:0.#} dBFS; {sound.minSeconds:0.##}–{sound.maxSeconds:0.##} s.\n");
            }

            return text.ToString();
        }

        private static string Status(StyleStatus status) => status == StyleStatus.V3 ? "v3" : "legado";

        private static string RoleRow(PaletteRole role)
        {
            return $"| {role.role} | {role.styleClass} | {Hex(role.levels.baseColor)} | {Hex(role.levels.shadow)} | {Hex(role.levels.highlight)} |\n";
        }

        private static string Hex(Color color) => "#" + ColorUtility.ToHtmlStringRGB(color).ToLowerInvariant();

        private static string Number(float value) => value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

        private static string Vector(Vector3 value) => $"({Number(value.x)}, {Number(value.y)}, {Number(value.z)})";

        [MenuItem("Claybreak/Write Style Checklist")]
        public static void Write()
        {
            var guide = AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);
            if (guide == null)
            {
                throw new InvalidOperationException(StyleGuide.AssetPath + " is missing (run Claybreak > Write Style Guide)");
            }

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            File.WriteAllText(Path, Generate(guide), new UTF8Encoding(false));
            Debug.Log("[StyleChecklist] wrote " + Path);
        }

        public static void WriteAndQuit()
        {
            BatchRunner.Run("StyleChecklist", Write);
        }
    }
}
