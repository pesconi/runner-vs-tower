# Tools/blender — modelos por Blender em dados simples (RT-6v3.4a)

Blender **5.3 fixo** (`C:\Program Files\Blender Foundation\Blender 5.3`), sem interface (`blender -b`). Os scripts gravam
**dados simples** versionados em `out/` (JSON: posições, normais, UV, UV2, índice de paleta por vértice com a cor em espaço
linear, índices), já no referencial do Unity. Nada de FBX. O jogo **não precisa do Blender**: o gerador de Editor
(`Claybreak.Editor.SimpleMeshData` + `UnitShapeAssets`, rodado pelo passo `scene`) grava os `.asset` a partir do JSON, e o
validador confere que o JSON é a saída dos scripts atuais (hash SHA-256 dos scripts, fim de linha normalizado).

| Arquivo | O que é |
|---|---|
| `bibopzi.py` | O Bibopzi (D3 "Feijão" + D36) em três níveis: `detail` (3–5 mil tri, líder), `crowd` (800–1.200, as 300 unidades), `reserve` (450–550, reserva do gate) |
| `simple_mesh.py` | Exportador compartilhado (formato `jungledegree.simple-mesh/1`), reflexão Blender → Unity e enrolamento invertido |
| `preview.py` | Prévia rápida (Workbench) de frente, 3/4, costas e câmera da corrida |
| `run.sh` | Roda o Blender, confere que duas rodadas dão os mesmos bytes; `--preview <png>` faz a prévia |

Fluxo ao mudar um modelo: editar o script → `Tools/blender/run.sh --preview /c/temp/prev.png` → olhar a prévia →
`Tools/rt4-check.sh <out> "scene validate editmode"` → commitar scripts **e** `out/*.json` juntos (o validador recusa um sem o
outro).

## Convenções

- Referencial: metros, +Y para cima, +Z frente, +X a mão direita **dele**; o Blender é destro com o boneco olhando −Y:
  `unity = (−bx, bz, −by)` (reflexão: o enrolamento dos triângulos é invertido na saída).
- Cor: paleta por índice; `tint: true` = toma a skin (alfa 255 no jogo), `false` = cor fixa (alfa 0); o `.asset` guarda o
  sRGB de 8 bits da folha do personagem (o teste confere a volta linear → sRGB).
- UV2 (layout animado do kit, K.v3b): x peso da mola (0 na cabeça → 1 na ponta), y lado da mão (+1 direita, −1 esquerda),
  z peça que gira (1 = chave de corda; eixo e pivô em `meta.keyAxis`/`meta.keyPivot`), w máscara do rosto (0 até a 6v3.4b).
  UV0: a frente da cabeça projetada na caixa `meta.faceBox` (para o atlas da 6v3.4b), 0 no resto.
- Determinismo: o bmesh não mantém a ordem das faces nem orienta as normais igual de uma rodada para outra; o script canoniza
  a malha (faces por peça e posição, cada uma começando no menor canto) e orienta as faces ele mesmo.
