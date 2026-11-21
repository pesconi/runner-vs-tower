# RT-6t — Direção visual "crowd toon": resultado das entregas

Plano e decisões: [`direcao-visual-crowd.md`](direcao-visual-crowd.md) (aprovado em 2026-11-21). Referência visual:
[`reference/crowd-proto.html`](reference/crowd-proto.html). Decisões técnicas: `decisoes.md` (T-RT6t-1..9 e
T-RT6t1-1..8).

## RT-6t.1 — Portão técnico (2026-11-21)

### O que foi feito

**Motor (com cara de kit, incubado em `Assets/StudioKitIncubator/`, sem citar o jogo):**

| Peça | Assembly | O que faz |
|---|---|---|
| `StudioKit/ToonLit` (`Toon/Shaders/ToonLit.shader`) | `JungleDegree.StudioKit.Toon` | Toon de faixas (rampa global, filtro point), ambiente de dois tons (chão → céu), neblina linear do URP, cor por vértice com máscara de tinta no alfa. Variante `STUDIOKIT_TOON_INSTANCES`: transformação e tinta lidas de `_ToonInstances[SV_InstanceID]`. Sem sombras, sem PBR. |
| `ToonInstance` + `ToonInstanceData.hlsl` | Toon | Formato da instância (64 bytes: 3×4 + tinta; `w` livre para o dono). |
| `ToonLook` (ScriptableObject) + `ToonLookApplier` | Toon | Níveis da rampa (43%/73%/100% do protótipo), cores do ambiente e intensidades como dado; a rampa é gerada na carga. |
| `LowPolyMeshBuilder` | Toon | Esfera/elipsoide, cone, cilindro, caixa, icosaedro, disco; posicionar, colorir, juntar numa malha de índices 16 bits; espelhamento mantém a orientação das faces. |
| `SwarmMotion`, `SwarmAnimateJob`, `SwarmSimulation` | `JungleDegree.StudioKit.Swarm` | Fórmulas do protótipo (quique `|sin(9,5t+φ)|`, salto 0,2, squash 0,8 + 0,32h com volume mantido, inclinação 0,12 rad à frente), formação girassol fixa; arrays nativos persistentes e job Burst paralelo (lotes de 64). |
| `SwarmRenderer` | Swarm | 1 `GraphicsBuffer` estruturado; **1 chamada** para os corpos + **1** para as sombras-bolha (`Graphics.RenderMeshPrimitives`), as duas lendo o mesmo buffer. Tudo alocado no `OnEnable`, liberado no `OnDisable`. Sem dispositivo gráfico (batchmode) simula sem desenhar. |
| `StudioKit/BlobShadowInstanced` | Swarm | Disco de 12 lados no chão, cor #2b2233 a 0,18, encolhe `1 − 0,35h` com o quique. |
| `DevFpsOverlay` | Swarm | FPS + número de bonecos na tela (só Editor, build de desenvolvimento e de medição) e a linha `[DevFps] window=N fps=F units=U` no log. |
| Testes EditMode | `JungleDegree.StudioKit.Incubator.Tests` | Contagem de triângulos de cada primitiva, orientação das faces (com rotação, escala e espelho), 16 bits e cores, rampa, fórmulas do quique/squash/girassol, simulação (posição, salto, squash por instância), limites. |

**Jogo:**
- Bolinha provisória (`UnitShapeRecipes.ProvisionalBolinha`): corpo 16×11, 2 olhos, caule e 2 folhas = **474
  triângulos** (teto de 500), uma malha, frente +Z, corpo com máscara de tinta. O validador do jogo confere
  200–500 triângulos, 16 bits e os olhos na frente.
- Cena `Assets/Claybreak/Lab/ToonLab.unity` (gerada por `ToonLabBuilder`, chamado pelo construtor de cena): paleta
  Pastel (chão #bfe6cf, pista #fff4ea/#ffe3d1, multidão #ff8fab, neblina #f3e3ea 22–75), luz direcional **sem
  sombra**, câmera fixa (fórmula do protótipo, campo de visão aberto até a formação caber no retrato), 1000
  unidades por padrão (capacidade 2048).
- Modo de medição: o APK de medição leva a `ToonLab`; `adb shell am start ... --ez toonlab true --ei units N
  [--ei msaa 1|4] [--ez hdr true|false] --ei fps 60` troca para a cena do laboratório antes do jogo começar e liga
  a sonda de tempo do kit (`[DevTiming]`). `Tools/rt2-gate.sh <rótulo> 60 20 <apk> toonlab:N:msaa:hdr` mede;
  `Tools/rt6t-toonlab-gate.sh [apk]` roda a matriz inteira (1000 × MSAA 1×/4× × HDR desligado/ligado, mais 2000
  para registro) e grava uma captura em `docs/art/rt6t/`.
- Regra 2.2.1 (nada de `Instantiate`/`Destroy` em jogo) agora também na incubadora e no laboratório; isolamento do
  kit também sobre `Assets/StudioKitIncubator/`.

### Validação padrão (`Tools/rt4-check.sh`, 2026-11-21)

`scene`, `validate`, `isolation`, `editmode` (150/150), `playmode` (14/14, inclui a `ToonLab` com 1000 unidades),
`economy`, `capture`, `apk-game`, `apk-measure`, `aab`: **todos PASS**. A `ToonLab` também roda no Editor com GPU
(teste PlayMode com Direct3D 12): 1000 unidades, 474 triângulos por unidade, chamadas 1 + 1, nenhum erro no log.

### Gate 3× no OPPO CPH2791 — **PASSOU** (2026-11-21)

Critério: CPU principal p95 ≤ 11 ms, GPU p95 ≤ 11 ms, quadro p95 ≤ 16,7 ms a 60 fps, < 5% dos quadros > 50 ms,
PSS ≤ 450 MB; chamadas da multidão 1 + 1.

Aparelho: **Vulkan 1.1, GPU Mali-G1-Ultra MC12**, CPU ARM64 ×8, tela 1080×2354, escala de render 0,8 (linha
`[ToonLab]` do log). Comando: `Tools/rt6t-toonlab-gate.sh /c/temp/rt6t1/Claybreak-measure.apk` (20 s de amostra
por rodada após 8 s de carga) e `Tools/rt2-gate.sh toon2000-m1-h1 60 20 <apk> toonlab:2000:1:1`. CPU e GPU = pior
p95 das janelas de 5 s estáveis (2–5; a janela 1 é a carga); quadro, > 50 ms e PSS = SurfaceFlinger e `meminfo`.

| Rodada | Unidades | MSAA | HDR | CPU p95 | GPU p95 | Quadro p95 | > 50 ms | PSS | Resultado |
|---|---|---|---|---|---|---|---|---|---|
| gate | 1000 | 1× | desligado | 1,4 ms | 4,5 ms | 16 ms (62,5 fps) | 0,0% | 201 MB | **passa** |
| gate | 1000 | 1× | ligado | 1,5 ms | 4,3 ms | 16 ms | 0,0% | 201 MB | **passa** |
| gate | 1000 | 4× | desligado | 1,9 ms | 5,4 ms | 16 ms | 0,0% | 204 MB | **passa** |
| gate | 1000 | 4× | ligado | 1,7 ms | 5,5 ms | 16 ms | 0,0% | 203 MB | **passa** |
| registro | 2000 | 1× | ligado | 1,5 ms | 5,9 ms | 16 ms | 0,0% | 201 MB | dentro do gate |
| registro | 2000 | 4× | ligado | 1,9 ms | 8,2 ms | 16 ms | 0,0% | 202 MB | dentro do gate |

Leitura: o caso mais pesado de 1000 (MSAA 4×, HDR ligado) usa metade do teto de GPU e um sexto do de CPU; nem o
plano B foi necessário. Mesmo 2000 unidades com MSAA 4× ficam dentro do gate (GPU 8,2 ms). HDR não muda o custo
neste aparelho; MSAA 4× custa ~1 ms de GPU com 1000. Chamadas da multidão: 1 (corpos) + 1 (sombras).

Captura no aparelho (1000 unidades, MSAA 4×, HDR ligado): [`art/rt6t/toonlab-1000.png`](art/rt6t/toonlab-1000.png).

**Primeira rodada descartada.** A primeira matriz (mesmo dia) mediu com o shader `StudioKit/ToonLit` sem compilar
(`SRGBToLinear` sem o include `Color.hlsl` do core): o Unity desenha o shader de erro (magenta) sem nenhum erro em
tempo de jogo, e o teste PlayMode com GPU também não acusa. A captura revelou. Corrigido o include, e o validador do
jogo agora falha se algum material da `ToonLab` usar shader com erro de compilação (`ShaderUtil.ShaderHasError`;
conferido: com o include removido, o validador acusa os dois materiais do ToonLit). Os números acima são da segunda
matriz, com o shader certo.
