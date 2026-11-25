# Imagem-alvo v3: telas-conceito medidas (RT-6v3.2, 2026-11-25)

Três direções de "brinquedo de vinil premium" e a mistura **M** recomendada pelo revisor, para a decisão **D36** (ficha em `docs/riscos-e-decisoes.md`), feitas
**no Unity**, no próprio jogo, com o shader de laboratório `Claybreak/VinylLab` (local ao jogo; o `studiokit` não mudou) e
os mesmos enquadramentos das capturas "antes" (`../antes/`, `StyleCaptureRig`). Nenhuma imagem foi gerada por IA (D37 = C).

| Direção | Ideia | Números principais |
|---|---|---|
| **A** vinil brilhante e saturado | brilho e borda em tudo, saturação +22, bloom leve nos brilhos | rampa 0,12; brilho 0,6; borda 0,45; cenário 50% do brilho |
| **B** pastel fosco, brilho só no personagem | cenário fosco (4% do brilho), sombras claras, saturação −10, sem bloom | rampa 0,30; brilho 0,5; borda 0,4 |
| **C** contraste alto, fim de tarde | sol baixo e quente, sombras roxas, céu roxo→laranja, contraste +26, vinheta 0,28 | rampa 0,07; borda laranja 0,65 |
| **M** mistura (revisor fable) | vinil da A no personagem, cenário fosco da B (6% do brilho), contraste +14, saturação +10, bloom só acima de 1,3 | rampa 0,15; brilho 0,45; borda 0,45 |

O que mudou nas telas: o shader (luz suave, sombra colorida, brilho, borda), a luz, o céu e o pós; o Bibopzi suavizado
(490 → 1.960 triângulos, normais suaves + subdivisão PN). **Não** mudaram: modelos do cenário, inimigos, portões,
obstáculos, pista e UI (são as próximas entregas, RT-6v3.4–3.10). A torre "tema do mundo" é o modelo de hoje com normais
suavizadas; a "fábrica de brinquedo" (13.920 triângulos) é um esboço procedural de engrenagem, molas, parafusos, cúpula e
chave de corda, nas cores do mundo. Nas telas da home o pós também pinta a UI (só na captura; no jogo a UI fica fora do pós).

## Arquivos

- `comparar-corrida.png`, `comparar-assalto.png`, `comparar-home.png`: hoje | A | B | C | M, mesmo enquadramento.
- `comparar-lider.png`: o Bibopzi de perto, três ângulos, hoje, nas três direções e na mistura.
- `torres.png`: por direção, torre do mundo 4 de perto (tema do mundo × fábrica) e o assalto com a fábrica.
- `icones.png`: ícone de hoje e três esboços por direção — 1 rosto em destaque, 2 multidão de costas diante da torre,
  3 um Bibopzi de costas diante da torre (a D25 pede ícone sem rosto em destaque).
- `A/`, `B/`, `C/`, `M/`: cada captura em tamanho real (corrida, assalto e torre nas duas versões, telas nas 4 proporções,
  líder, ícones 512).
- `aparelho-0..3.png`: captura de tela do OPPO no laboratório de medição (0 = visual de hoje com o Bibopzi suavizado,
  1–3 = A–C, pós ligado, metade do tamanho; a M ainda não).

## Ensaio no OPPO CPH2791 (Mali-G1-Ultra MC12), 2026-11-25

300 Bibopzis suavizados de **1.960 triângulos** (acima da meta v3 de 1.200: medida conservadora), 60 fps pedidos, MSAA e
HDR do projeto (1×, HDR ligado, escala 0,8), laboratório `ToonLab`. GPU p95 por janela de 5 s (janelas 2–5; a 1ª é a
carga). Teto do gate: **11 ms**. CPU principal p95 ≤ 2,2 ms em todas; quadro p95 16 ms; ≥ 62 fps médios; > 50 ms ≤ 0,1%;
PSS 199–213 MB.

| Visual | Pós | Vulkan (buffer) | GLES (blocos de 256) |
|---|---|---|---|
| Hoje (ToonLit) + Bibopzi suavizado | desligado | 4,0–4,1 | 7,1–7,6 |
| A | desligado | 4,5–4,7 | 7,5–7,6 (uma janela 18,4) |
| A | ligado | 5,4–5,6 (repetição 5,1–5,3) | 6,1–7,1 |
| B | desligado | 4,5–4,6 | 7,4–7,9 |
| B | ligado | 7,4–7,6 (repetição 7,3–7,5) | 8,3–8,5 |
| C | desligado | 4,4–4,5 | 19,2–20,4 na 1ª rodada; **7,3–8,3 na repetição** |
| C | ligado | 5,2–5,4 | 6,2–7,5 |
| M | desligado / ligado | pendente: o OPPO ficou bloqueado depois das outras rodadas | `TOONLAB_VINYL="4" Tools/rt6t-toonlab-gate.sh /c/temp/rt6v32/Claybreak-measure.apk 300 0 v3` |

Leitura: o shader de vinil custa ~0,5 ms de GPU a mais que o de hoje (Vulkan) e quase nada em GLES; o pós custa de 0,7 a
3 ms. Tudo cabe no teto com folga (pior caso estável: 8,5 ms, B com pós em GLES). A 1ª rodada de C sem pós em GLES e uma
janela de A saíram fora (picos isolados com o quadro a 60 fps estável); repetidas uma vez dentro, como manda a regra
do gate. Anomalia registrada: B com pós (sem bloom) mede ~2 ms a mais que A com pós e bloom, reproduzido; investigar na
K.v3a/RT-6v3.3. Arquivos brutos: `C:\temp\rt2\gate-v3300-*.txt` (fora do repositório).

## Referência do gênero (só por link; nenhuma arte copiada)

- Mob Control (Voodoo): https://play.google.com/store/apps/details?id=com.vincentb.MobControl — multidão contra a base
  inimiga; cores chapadas e saturadas, sombreado simples, muito contraste azul × vermelho.
- Count Masters (Freeplay): https://play.google.com/store/apps/details?id=freeplay.crowdrun.com — corrida com portões e
  castelo no fim; bonecos lisos, pista clara, cenário mínimo.
- Join Clash 3D (Freeplay): https://play.google.com/store/apps/details?id=com.freeplay.runandfight — corrida e briga no
  castelo; personagens lisos com brilho leve, céu em degradê.

Os três líderes do gênero usam formas lisas, poucas cores fortes e pista clara; nenhum usa "brinquedo de vinil" com
brilho e borda no personagem — é o espaço que a v3 ocupa.

## Como refazer

```
Tools/rt4-check.sh <out> "scene"     # regera o laboratório (Bibopzi suavizado, material, perfis de pós)
"<Unity.exe>" -batchmode -buildTarget Android -projectPath <projeto> -runTests -testPlatform PlayMode \
  -testFilter Claybreak.Tests.VinylConceptTests -vinylConceptDir <pasta> -logFile <log>     # sem -nographics
python -X utf8 Tools/art/vinyl_sheets.py <pasta>
git checkout -- Assets/Claybreak/Content   # o editor grava _VinylHero nos .mat ao sair
TOONLAB_VINYL="0 1 2 3 4" Tools/rt6t-toonlab-gate.sh <apk de medição> 300 0 v3   # 4 = M
```
