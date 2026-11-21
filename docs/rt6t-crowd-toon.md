# RT-6t — Direção visual "crowd toon": resultado das entregas

Plano e decisões: [`direcao-visual-crowd.md`](direcao-visual-crowd.md) (aprovado em 2026-11-21). Referência visual:
[`reference/crowd-proto.html`](reference/crowd-proto.html). Decisões técnicas: `decisoes.md` (T-RT6t-1..9,
T-RT6t1-1..9 e T-RT6t3-1..8).

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

## RT-6t.3 — Formação, arraste e câmera (2026-11-21)

### O que foi feito

**Motor (incubadora, `JungleDegree.StudioKit.Swarm`, sem citar o jogo):**

| Peça | O que faz |
|---|---|
| `SunflowerFormation` + `SwarmFormationSettings` | Fórmulas do protótipo: offset `√(i+0,5)·0,25·(cos(i·2,39996), sin(i·2,39996)·1,35)` (ângulo em precisão dupla, como o JavaScript), seguimento `1 − e^(−k·dt)` (centro k = 8, unidades k = 7), ritmo por unidade `0,8 + 0,4·((7i mod 10)/10)`, vaga presa na pista com 0,2 m de margem. Números como dado. |
| `SwarmFollowJob` | Job Burst paralelo: cada unidade segue sua vaga (X com o seu ritmo, Z relativo ao centro). Roda antes do `SwarmAnimateJob`, encadeado. |
| `SwarmSimulation` (refeita) | Tabela de offsets da capacidade inteira calculada na carga; posição = (x do mundo, z relativo ao centro); centro que segue o alvo; pista (`SetLane`); unidades novas nascem na vaga, as removidas são o anel de fora. |
| `SwarmRenderer` | Ganha `formation`, `laneHalfWidth` (pista centrada no próprio X), `SteerTo`, `PlaceCenter`, `CenterX`/`TargetX`, `UnitGroundPosition`; passo limitado a 0,05 s como o protótipo. |
| `SwarmSteering` + `SwarmDragSettings` | Arraste do protótipo: `alvo no toque + Δx/largura·16`, preso em `±(meia pista − 0,5)`; setas ±1,5. A leitura do dedo fica no jogo. |
| `SwarmCameraProfile` (ScriptableObject) + `SwarmCameraMath` + `SwarmCameraRig` | Câmera do protótipo espelhada para +Z: `d → 1 + √n/22` (suavizado `1 − e^(−2dt)`), posição `(cx·0,35, 3,2 + 4,2d, −(5,5 + 5,5d))`, olha `(cx·0,35, 0, +5)`, FOV 50, tremor 0,35 s com `±(resto·0,6)/2`; ajuste a retrato por dado (abaixo); `FramingScale` para a neblina. |

**Jogo (`ToonLab`):** arraste com o dedo (mouse no Editor; toque que começa no painel não arrasta) e setas; painel de
teste embaixo (**+100**, **1000!**, **Recomeçar** = 10, **Tremer**), escondido nas execuções de medição; câmera pelo rig
com o perfil `Assets/Claybreak/Lab/SwarmCameraLab.asset`; multidão presa na pista de 6 m de meia largura; neblina que
se afasta com o enquadramento; pista do laboratório começando 60 m atrás e corte da câmera a 250 m. O validador do jogo
confere o perfil de câmera.

### Enquadramento de retrato

O protótipo foi feito deitado: em retrato, com FOV 50, só se viam ~3,4 m de cada lado (os portões ficariam na borda).
Agora a pista inteira + 0,7 m por lado cabe na largura, em qualquer aspecto, com a multidão em qualquer ponto da pista
(decisões T-RT6t3-6 e 6b em `decisoes.md`). Valores medidos na captura (multidão parada, depois de 3,5 s):

| Tela | Unidades | FOV vertical | Câmera (x, y, z) | Distância ÷ protótipo | Neblina |
|---|---|---|---|---|---|
| 9:16 | 10 | 65° | (0; 12,9; −22,2) | 1,62 | 35,5–120,9 m |
| 9:16 | 100 | 65° | (0; 14,5; −23,7) | 1,55 | 34,3–116,8 m |
| 9:16 | 300 | 65° | (0; 16,1; −25,5) | 1,50 | 33,0–112,6 m |
| 9:16 | 1000 | 65° | (0; 19,1; −28,9) | 1,42 | 31,3–106,6 m |
| 9:16 | 1000, arrastada à direita | 65° | (1,9; 19,1; −28,9) | 1,42 | igual |
| OPPO 1080×2354 | 10 | 65° | (0; 15,3; −27,2) | 1,91 | 42,0–143,3 m |
| OPPO 1080×2354 | 1000 | 65° | (0; 21,8; −33,7) | 1,62 | 35,7–121,6 m |
| OPPO 1080×2354 | 1000, arrastada à direita | 65° | (1,9; 21,8; −33,7) | 1,62 | igual |
| 16:9 (tela larga) | qualquer | 50° | a do protótipo | 1,00 | 22–75 m |

A câmera se afasta de 10 → 1000 (z −22,2 → −28,9 em 9:16) e o arraste só a move de lado (sem zoom). Capturas (10, 100,
300, 1000 e 1000 na borda direita): [`art/rt6t/toonlab-9x16-10-1000.png`](art/rt6t/toonlab-9x16-10-1000.png) e
[`art/rt6t/toonlab-oppo-10-1000.png`](art/rt6t/toonlab-oppo-10-1000.png) (Editor com GPU, Direct3D 12; comando:
`-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.ToonLab -toonLabCaptureDir <pasta>`, sem `-nographics`).

### Testes

- EditMode (`SwarmFormationTests`, `SwarmCameraTests`): offsets das 2000 vagas iguais ao protótipo (2·10⁻⁵ m);
  ritmo, fator de seguimento e clamp; **simulação comparada quadro a quadro com uma cópia literal do laço `frame()` do
  protótipo** (240 quadros, dt variando de 1/120 a 0,05 s, arraste para as duas bordas, contagem 10 → 400 → 600);
  nenhuma unidade sai da pista; nascimento na vaga; arraste e setas iguais ao protótipo; `d` quadro a quadro, pose e
  tremor iguais ao protótipo; retrato (9:16, OPPO, 3:4) com as bordas da pista dentro da tela pela projeção do próprio
  Unity, ajuste justo, FOV aberto antes de recuar, afastamento crescente com a contagem, arraste sem zoom; padrões do
  perfil = protótipo.
- PlayMode (`ToonLabSceneTests`): botões do painel (+100, 1000!, Recomeçar), câmera assentando em `1 + √n/22` e
  recuando com 1000, arraste preso na pista com todas as unidades dentro, câmera seguindo de lado, tremor acabando em
  0,35 s.

### Validação

`scene`, `editmode` (164/164), PlayMode com GPU dos testes da `ToonLab` (3/3, incluindo a captura) e, completos com o
`rt4-check`: `validate`, `isolation`, `playmode` (15/15 com
`-nographics`; a captura é ignorada ali), `economy`, `capture`, `apk-game`, `apk-measure` (sem erro de shader no log),
`aab`: **todos PASS**.

Sem gate no OPPO nesta entrega (o aceite não pede; custo novo = um job Burst de ~1000 `float2`; decisão T-RT6t3-8).
Fora do escopo (6t.5): inclinação lateral pela velocidade, nascimento elástico e morte balística.
