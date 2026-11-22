# RT-6t — Direção visual "crowd toon": resultado das entregas

Plano e decisões: [`direcao-visual-crowd.md`](direcao-visual-crowd.md) (aprovado em 2026-11-21). Referência visual:
[`reference/crowd-proto.html`](reference/crowd-proto.html). Decisões técnicas: `decisoes.md` (T-RT6t-1..9,
T-RT6t1-1..9, T-RT6t2-1..7, T-RT6t3-1..8, T-RT6t4-1..10, T-RT6t5-1..10, T-RT6t6a-1..10, T-RT6t8-1..11, T-RT6t7-1..10, T-RT6t9-1..11, T-RT6t10-1..8, T-RT6t11-1..7 e T-RT6t12-1..6).

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

## RT-6t.2 — As duas formas: Bolinha e Pintinho (2026-11-21)

### O que foi feito

**Jogo (Editor):** receitas finais em `UnitShapeRecipes` (o `unitGeo()` do protótipo espelhado para a frente +Z, com
menos segmentos), cada uma **com e sem olhos** (D25 sem resposta: as duas prontas), geradas por `UnitShapeAssets` em
`Assets/Claybreak/Content/Generated/Units/`:

| Forma | Peças (do protótipo) | Triângulos | Sem olhos |
|---|---|---|---|
| **Bolinha** | bola achatada 0,9 em Y (16×11), 2 olhos (6×4), haste (5 lados) e 2 folhas (6×4) | 484 | 412 |
| **Pintinho** | bola (16×11), 2 olhos, bico em cone laranja (8 lados) na frente, topete de 2 bolinhas laranja, rabinho em cone da cor do corpo (6 lados) para trás e para cima | 492 | 420 |

R = 0,22; malha única, índices 16 bits; máscara de tinta no alfa da cor (corpo e rabinho recebem a cor da paleta;
olhos #2b2233, folhas #5cc98a, bico e topete #ff9f43 ficam fixos). A Bolinha provisória da 6t.1 foi substituída pela
final (mesmo corpo; a haste ganhou as tampas do protótipo: 474 → 484).

**Validador** (`UnitShapeRules`, mesma regra nos testes), para cada uma das 4 malhas: 1 submalha de 200–500
triângulos, 16 bits, máscara só com alfa 255/0 e as duas classes presentes, apoiada no chão, olhos só nas variantes com
olhos e na frente, detalhe da frente (olhos, bico) apontando para +Z.

**`ToonLab`:** segunda linha no painel — **Bolinha, Pintinho, Com olhos, Sem olhos** — trocando a malha da multidão na
hora (`SwarmRenderer.SetUnitMesh`, sem realocar nada). Padrão provisório: **Bolinha com olhos**. No APK de medição:
`--ei shape 0|1 --ez eyes true|false`. O jogo ainda não usa as formas (é a 6t.4).

### Capturas lado a lado com o protótipo

Comando: `Tools/rt6t/shape-captures.sh` (Unity com GPU + Chrome sem tela renderizando `Tools/rt6t/proto-shapes.html`,
que é o código de forma, a rampa de 3 faixas e as luzes do protótipo, com um parâmetro para tirar os olhos; mesmas
poses de câmera, espelhadas).

- De perto, 4 formas × 3 vistas (frente 3/4, lado, costas 3/4), protótipo | Unity:
  [`art/rt6t/formas-closeup.png`](art/rt6t/formas-closeup.png)
- Multidão de 100 em 9:16 com a câmera do laboratório (protótipo parado na vaga, Unity pulando):
  [`art/rt6t/formas-multidao.png`](art/rt6t/formas-multidao.png)

**Revisor independente (subagente, modelo fable, só leitura; comparou as folhas e o `unitGeo()` linha a linha com a
receita): APROVADO COM OBSERVAÇÕES.** As quatro formas batem com o protótipo em proporção, posição das peças,
orientação (olhos e bico na frente, rabinho para trás e para cima, folhas e topete em cima) e cores; nada espelhado,
faltando ou sobrando. Diferenças vistas, nenhuma de forma: (1) o Unity está mais claro — a faixa iluminada é maior e a
faixa escura de baixo é mais fraca que o rosa profundo do protótipo, e o verde das folhas sai mais claro; é calibração
da rampa/luz do `ToonLit` (`ToonLook`), não da malha; (2) olhos um pouco mais facetados e silhueta um pouco mais
angulosa, efeito esperado dos segmentos reduzidos (orçamento). Na folha da multidão, pista, faixas, chão e neblina
diferem (cenário e paleta do laboratório, assunto da 6t.6), e a 100 unidades os olhos quase não aparecem em nenhum dos
dois lados.

**Para entregas seguintes (não trava a 6t.2):** calibrar a rampa e a luz do `ToonLit` para a faixa de sombra chegar
à profundidade do protótipo (a multidão perde volume sem isso) — fica para a 6t.6 (paletas/aplicador) ou antes, se a
6t.4 pedir; e, para decidir a D25, olhar as capturas de perto (`formas-closeup.png`), não a de 100 unidades.

### Testes e validação

- EditMode (`UnitShapeRecipeTests`): triângulos de cada variante (484/412/492/420); as quatro passam na regra do
  validador; a regra pega olhos faltando ou sobrando, forma virada para −Z e orçamento estourado; bico, topete,
  rabinho, olhos e folhas nas posições do protótipo (espelhadas); máscara de tinta.
- PlayMode (`ToonLabSceneTests`): a `ToonLab` abre com a Bolinha com olhos (484 tri) e o painel troca Pintinho → sem
  olhos → Bolinha → com olhos mantendo a multidão.
- `Tools/rt4-check.sh` completo (2026-11-21): `scene`, `validate`, `isolation`, `editmode` (171/171), `playmode`
  (16/16; as 2 capturas só rodam com GPU), `economy`, `capture`, `apk-game`, `apk-measure` (sem erro de shader no
  log), `aab`: **todos PASS**. Capturas: `Tools/rt6t/shape-captures.sh` (Unity com GPU, Direct3D 12).
- Sem gate no OPPO (o aceite diz "dentro do teto da 6t.1": 492 tri no máximo contra os 474 medidos, mesma chamada
  única; decisão T-RT6t2-7).

### D25 (olhos / público) — continua sem resposta

As duas variantes estão prontas e selecionáveis; a `ToonLab` abre com a versão com olhos (padrão provisório). **A escolha
final da D25 trava só a 6t.4 em diante** (qual malha a corrida usa e a ficha da loja na RT-9); trocar depois é mudar
uma referência de malha, sem código novo.

## RT-6t.4 — A multidão instanciada na corrida (2026-11-21)

### O que foi feito

A corrida do jogo deixou de usar o líder (`PlayerController` com `CharacterController`), os seguidores do kit
(`RunnerCrowd`) e a câmera que segue o líder (`CameraFollow`). Agora:

| Peça | O que faz |
|---|---|
| `RunCrowd` (jogo, `Runtime/RunCrowd.cs`) | Dirige o `SwarmRenderer` da cena: contagem = exército (sem líder), avanço pela velocidade da fase, arraste do protótipo (pista de 8 m: 10,7 m por largura de tela), centro da formação = posição que cruza portões e caixotes, tirar a unidade do anel de fora para o assalto, tinta da skin, forma do tema, tremor da câmera. Nada alocado em jogo. |
| `Swarm` na cena do jogo | 300 de capacidade (teto da D27 sem resposta), pista centrada de meia largura 4 m, materiais da multidão (agora em `Content/Generated/Units/`, compartilhados com a `ToonLab`), 1 chamada para os corpos + 1 para as sombras. |
| `SwarmCameraRig` na câmera principal | Câmera da corrida e do menu pelo perfil `Content/Data/SwarmCameraRun.asset` (valores do protótipo, ajuste de retrato da 6t.3); na linha de tiro passa para a `AssaultCamera` como antes. |
| `ToonLookApplier` no sol | Rampa e ambiente do `ToonLit` pelo `Content/Data/CrowdToonLook.asset`. Cenário, torres e portões continuam no visual atual (6t.6–6t.10). |
| `ClaybreakTheme.crowdShape` | A forma da multidão é dado: **Bolinha com olhos** (provisória, D25 aberta). Trocar para a sem olhos ou para o Pintinho é trocar essa referência. |
| `UnitGrounding` | `CharacterController` desligado só com o tamanho do antigo líder: o kit aterra as unidades do pool do assalto contra ele. Some com o pool na 6t.7. |
| Nível de gate da corrida | `--ez stress true --ez run true --ei world N`: o exército no teto correndo no mundo N, voltando ao começo 10 m antes da torre; `Tools/rt2-gate.sh <rótulo> 60 20 <apk> run:N`. |

Regras e save sem mudança: `GateMath` com o mesmo teto (300), caixotes, perda por exército 0, continuar, estrelas e
moedas iguais; a cor da skin vira a tinta e o save (v3) só guarda o id, como antes. O assalto não mudou: as unidades
em voo saem do pool do kit, lançadas da posição do anel de fora da multidão parada na linha de tiro. Portão ruim:
o anel de fora some na hora e a câmera treme (a morte balística é da 6t.5). Decisões T-RT6t4-1..10 em `decisoes.md`.

### Testes e validação

- EditMode 172/172 (novo: `StartingArmyMustFitTheCrowdCap`; testes de nível/catálogo com teto da multidão e capacidade
  de voo separados). PlayMode 17/17 com `-nographics` (3 capturas só com GPU): `RunKeepsTheCrowdOnTheTrackUntilTheFiringLine`
  adaptado (nenhum `CharacterController` ativo; toda unidade dentro da pista do começo à linha de tiro; o exército que chega
  é exatamente o que os portões da direita fazem com `GateMath`; multidão parada no assalto; a câmera do assalto volta ao
  seu FOV), novo `DeviceGateRunHoldsTheCrowdCapOnTheTrack` (300 = teto, volta ao começo, nunca assalta), menu e loja com a
  tinta da skin, continuar/sem obrigado/sem anúncio com `SetArmy`.
- `Tools/rt4-check.sh` completo (2026-11-21): `scene`, `validate`, `isolation`, `editmode`, `playmode`, `economy`,
  `capture`, `apk-game`, `apk-measure`, `aab`: **todos PASS** (depois da revisão, `validate editmode playmode apk` de novo:
  PASS).
- Captura com GPU da corrida no formato do OPPO (`RunCaptureTests`, `-runCaptureDir`): começo do nível 1 (10), multidão
  no teto (300), arrastada à borda direita, e o assalto —
  [`art/rt6t/corrida-oppo.png`](art/rt6t/corrida-oppo.png). Bolinha com olhos (484 tri), cor da skin, sem shader de erro.
  As capturas de `docs/art/telas/` (modo de edição) não mostram mais a multidão (o `Swarm` só desenha jogando); o
  `ArtDirectionCapture` é adaptado na 6t.12 (decidido na 6t.12: a prova da multidão fica nas capturas PlayMode, T-RT6t12-5).
- Revisor independente (subagente fable, só leitura): regras, contagens e save equivalentes; apontou o FOV do assalto
  herdado do rig e um quadro com instâncias velhas quando o fluxo muda a contagem depois do `Swarm` agendar — **corrigidos**
  (`AssaultCamera` com FOV próprio 60° em transição; `ClaybreakFlow` com ordem −40, entre o `RunCrowd` e o `Swarm`) — e
  asserts que não podiam falhar (removidos).
- Observação para a 6t.7: parada na linha de tiro, a multidão de 300 tem ~5,8 m de profundidade e metade dela fica à
  frente da linha (antes os seguidores ficavam atrás do líder); só visual, o lançamento já sai de `max(z, linha)`.

### Gate 3× na corrida (OPPO CPH2791) — **PASSOU** (2026-11-22)

Critério (T-RT6t4-8): CPU principal p95 ≤ 11 ms, GPU p95 ≤ 11 ms, quadro p95 ≤ 16,7 ms a 60 fps, < 5% dos quadros
> 50 ms, PSS ≤ 450 MB. Caso: corrida do nível de gate do **mundo 4** (o mais pesado), exército no teto (300, Bolinha com
olhos, skin "frost" da carga máxima), cenário, torre do chefe, escudo e defesas à frente, voltando ao começo 10 m antes
da torre. Comando: `Tools/rt2-gate.sh run-w4 60 20 /c/temp/rt6t4/Claybreak-measure.apk run:4`. CPU e GPU = pior p95 das
janelas estáveis (2–5); quadro, > 50 ms e PSS = SurfaceFlinger e `meminfo`.

| Rodada | CPU p95 | GPU p95 | Quadro p95 | > 50 ms | PSS | Alocação | Resultado |
|---|---|---|---|---|---|---|---|
| corrida, mundo 4, 300 | 2,7 ms | 3,0 ms | 16 ms (62,5 fps) | 0,0% | 298 MB | 0 B | **passa** |
| registro: assalto, mundo 4 (gate de antes) | 9,4 ms | 4,5 ms | 16 ms | 0,2% | 310 MB | 0 B | dentro do gate |

Captura no aparelho durante o gate: [`art/rt6t/corrida-oppo-gate.png`](art/rt6t/corrida-oppo-gate.png) (multidão
desenhada pelo `ToonLit`, sem shader de erro).

**Regressão achada e corrigida no gate.** A primeira rodada (mesmo APK da validação) deu quadro 16 ms e PSS 279 MB, mas
sem as linhas `[DevTiming]`: a sonda de CPU/GPU e o `--ei fps` eram ligados pelo `RunnerCrowd` do kit, que saiu da
cena. Agora o `ClaybreakFlow` faz isso (`ApplyMeasurementExtras`, só em builds de desenvolvimento/medição). Rodada
repetida com o APK novo (tabela acima); o assalto foi medido de novo só para confirmar que a medição dele voltou
(última janela com CPU 9,4 ms coincide com a coleta de memória do script, como já registrado na RT-5).

## RT-6t.5 — Animações procedurais (2026-11-22)

### O que foi feito

| Peça | Onde | O que faz |
|---|---|---|
| `SwarmAnimation` + `SwarmAnimationSettings` | incubadora, `Swarm` | Fórmulas do protótipo, puras e Burst: inclinação lateral `clamp(−vx·0,06, ±0,5)` (vx = velocidade lateral do quadro, escrita pelo `SwarmFollowJob`), nascimento elástico `1 − 2^(−10a)·cos(3,3πa)` em 0,45 s com atraso 0–0,25 s, sombra `(1 − 0,35h)·s`, composição `T·Rx(0,12)·Rz(inclinação)·S`. |
| `SwarmDying` + `SwarmDeathSettings` + `SwarmDyingJob` | incubadora, `Swarm` | Morte balística: empurrão para fora pela vaga (3 m/s de lado, 2 m/s na frente, ± sorteio), 4–8 m/s para cima, gravidade 22, giro `(r·t, 0,7·r·t)`, encolhe até sumir em 1,1 s. Anel de capacidade fixa (512 por padrão), o mais antigo sai quando enche; 1 `IJob` Burst depois da animação escreve os morrendo logo após os vivos. |
| `SwarmSimulation` / `SwarmRenderer` | incubadora | `SetCount(n, SwarmRemoval.Vanish|Die)`: remove sempre o anel de fora; `Die` lança no anel. `ResetCount` (recomeço: todos de pé na hora, sem pop nem morte). **1 chamada para vivos + morrendo**, sombras só para os vivos (como o protótipo); desenho com as contagens escritas pelo último quadro; limites crescem pelo alcance de quem morre. `DrawBodies` (ferramenta: só as sombras). |
| `StudioKit/BlobShadowInstanced` | incubadora | Lê a escala pronta da sombra em `tint.w` (encolhe com o pulo e nasce com o pop); `_LiftShrink` saiu (virou dado). |
| `ToonLab` | jogo, laboratório | Baixar a contagem mata o anel de fora (como o "Recomeçar" do protótipo); botão **÷2** (portão ruim: metade morre e a câmera treme); modo de rotação `--ei dying D` (mantém N vivos e D morrendo o tempo todo) para o gate. Log `[ToonLab] ... dying=D` e `[DevFps] ... dying=D`. |
| Corrida | `ClaybreakFlow.OnGateCrossed`, `RunCrowd` | Portão que diminui o exército = anel de fora em morte balística + tremor. Unidade tirada para o assalto continua sumindo (vai voar para a torre). Recomeço/reinício sem pop. |
| Gate | `Tools/rt2-gate.sh` | 5º campo do modo `toonlab`: `toonlab:1000:4:1:512` = 1000 vivos, MSAA 4×, HDR ligado, 512 morrendo. |

Decisões T-RT6t5-1..10 em `decisoes.md`.

### Testes e validação

- EditMode **184/184** (novos em `SwarmAnimationTests`: inclinação, nascimento, sombra, composição, lançamento, voo e
  encolhimento quadro a quadro, giro, ordem de remoção, anel cheio, morrendo desenhados depois dos vivos com a mesma
  matriz do protótipo, vivos com pop + inclinação + sombra iguais ao protótipo, limites com quem saiu do anel de fora,
  recomeço sem pop). O protótipo é reescrito ao pé da letra nos testes (expressões do `crowd-proto.html`) e a matriz é
  comparada com `Matrix4x4.TRS` do Euler do three espelhado.
- PlayMode **19/19** com `-nographics` (+4 capturas que só rodam com GPU): `ToonLab` (÷2 mata 500, todos somem depois de
  1,1 s, "Recomeçar" de 1000 para 10 enche o anel com os 512 mais novos; rotação mantém 1000 vivos e o anel ≥ 490), e
  `RunKeepsTheCrowdOnTheTrackUntilTheFiringLine` agora confere que **cada portão que diminui o exército lança a morte**.
- `Tools/rt4-check.sh`: `scene`, `validate`, `isolation`, `editmode`, `playmode`, `economy`, `capture`, `apk-game`,
  `apk-measure`, `aab`: **todos PASS** (o primeiro `apk-game` caiu num erro interno do Bee ao renomear cache; passou
  na repetição). Nenhum `Shader error` no log do APK de medição.
- Revisor independente (subagente fable, só leitura): nenhum bug sério; cinco pontos menores — quatro corrigidos
  (instâncias de um quadro antes, limites, recomeço com pop, teste do nascimento fraco), um aceito (a rotação do
  laboratório joga unidades que ainda não apareceram; a carga medida é a mesma).

### Capturas (Unity com GPU, formato do OPPO, metade do tamanho)

- [`art/rt6t/animacoes-oppo.png`](art/rt6t/animacoes-oppo.png): 1000 parados; ÷2 aos 0,12 s, 0,4 s e 0,8 s (os 500 do
  anel de fora voam para fora e para cima, giram e encolhem); 500 → 1000 aparecendo com o pop enquanto a multidão é
  puxada para a direita (inclinação lateral).
- [`art/rt6t/corrida-oppo.png`](art/rt6t/corrida-oppo.png): agora com 5 quadros; o 4º é o portão ruim na corrida (300 →
  150, 150 morrendo).
- **Chão sob a multidão (R-T7c)** — [`art/rt6t/chao-sob-multidao.png`](art/rt6t/chao-sob-multidao.png): 1000 parados,
  enquadramento do laboratório e de perto, com e sem os corpos. Luminância da imagem só com sombras: de perto, mínimo
  0,913 = p1 (uma sombra só; mediana 0,994) — **os discos quase não se sobrepõem no meio da multidão**, não há mancha.
  No enquadramento do laboratório o mínimo cai a 0,72 só **nas bordas da pista**, onde as vagas presas na largura da
  pista empilham unidades (linhas escuras visíveis só sem os corpos; com os corpos, cobertas). Fica como observação para
  a 6t.6 (opacidade por paleta) — sem stencil por enquanto.

### Gate 3× no OPPO (1000 vivos + 512 morrendo) — **PASSOU** (2026-11-22)

`Tools/rt2-gate.sh toon1000-d512 60 20 /c/temp/rt6t5/Claybreak-measure.apk toonlab:1000:4:1:512` (Bolinha, MSAA 4×,
HDR ligado, renderScale 0,8, Vulkan, Mali-G1-Ultra; `[ToonLab] units=1000 dying=512 draws=1+1`):

| Medida | Resultado | Limite |
|---|---|---|
| CPU principal p95 (janelas 2–5) | 1,8–1,9 ms | ≤ 11 ms |
| GPU p95 (janelas 2–5) | 6,2–6,4 ms | ≤ 11 ms |
| Quadro p95 (SurfaceFlinger, 1214 quadros) | 16 ms (média 62,5 fps; 60,5 fps no `[DevFps]`) | ≤ 16,7 ms |
| Quadros > 50 ms | 0,0% (0 perdidos, 0 travados) | < 5% |
| PSS | 177 MB | ≤ 450 MB |
| Alocação no quadro | 0 KB, GC +0 | — |

`dying=512` em todas as linhas `[DevFps]`. A janela 1 (33,4 fps) é a carga da cena, como nos gates anteriores. Captura do
aparelho durante o gate: [`art/rt6t/gate-d512-oppo.png`](art/rt6t/gate-d512-oppo.png) (o anel de fora voando e
encolhendo nas bordas da multidão).

## RT-6t.6a — Paletas, céu e neblina (2026-11-22)

A RT-6t.6 foi dividida (decisão do maestro em `decisoes.md`): esta é a parte sem a D26; formas e skins no menu e o
save v4 ficaram na RT-6t.6b, que espera a D26 (e a D25).

### O que foi feito

| Peça | Onde | O que faz |
|---|---|---|
| `ToonPalette` | incubadora, `Toon` | ScriptableObject: céu topo/base, neblina + início/fim (22–75), chão, pista, faixa, multidão, folhagem 2 tons, tronco, pedra, destaque e tinta de UI, estrutura clara/escura/detalhe e defesa (cores de torre da 6t.10). Todas opacas; `Validate` sem alocar. |
| `ToonPaletteApplier` | incubadora | Escreve as globais do céu (sRGB bruto), o skybox e a neblina linear do `RenderSettings`; `ApplyGlobally` estático para as capturas de modo de edição. |
| `StudioKit/SkyGradientScreen` | incubadora | Skybox: degradê vertical em espaço de tela, topo → base, mistura em sRGB como o canvas do protótipo, sem neblina, sem textura. |
| `ToonContrast` | incubadora | Razão WCAG e ΔE CIE76 (CIELAB, D65) para o validador. |
| `PaletteContent` | jogo, Editor | As 4 paletas como código, regravadas a cada construção da cena: Pastel, Pôr do sol e Doce com os valores exatos do usuário, Aurora provisória (seção 5.1). Cores de estrutura/defesa [P]. |
| `WorldLook.palette` | jogo | Substitui `WorldLook.sky` (o horizonte tem uma fonte só); sol e ambiente seguem para a arte antiga. `WorldScenery.Apply` aplica a paleta na troca de mundo/nível. A câmera limpa para o skybox. |
| Cor da multidão | `Progression.CrowdColorIn`, `ClaybreakFlow.DressArmy` | Skin padrão (BARRO) → cor da paleta do mundo; outras skins → a cor delas em todos os mundos. Corrida, bonecos do assalto e especiais na mesma cor; save e loja iguais. |
| Validador | `ClaybreakValidation` | Paleta de cada mundo = a do conteúdo, valor a valor; paletas não repetem entre mundos. **Avisos** (não reprovam) de contraste. |

Decisões T-RT6t6a-1..10 em `decisoes.md`.

### Avisos de contraste (valores exatos do usuário, mantidos)

- Mundo 1 (Pastel): multidão #FF8FAB × portão ruim #FF6B81 — ΔE 19,7, contraste 1,27:1. A 6t.9 (cor do portão
  semitransparente sobre a pista clara) confere na captura.
- Mundo 2 (Pôr do sol): multidão #FFF1D6 × pista #F6CAA6 — ΔE 17,7, contraste 1,35:1. A captura mostra a multidão
  creme legível (a pista ainda é a antiga, escura); a 6t.8 (pista listrada) confere.
- Tinta de UI × destaque: todas acima de 3:1 (Doce é a menor, 3,85:1).

### Testes e validação

- EditMode **193/193** (novos: `ToonPaletteTests` — padrão válido, ordem das cores, rejeição de transparente e neblina
  invertida, WCAG/CIELAB contra valores de referência, aplicador escreve céu e neblina; `PaletteContentTests` — os
  valores aprovados, uma paleta por mundo, multidões diferentes; `ProgressionTests` — BARRO usa a cor de cada mundo,
  skin comprada a própria cor, id desconhecido cai no padrão, save com o mesmo id).
- PlayMode **19/19** com `-nographics` (+5 capturas que só rodam com GPU): o menu com save novo veste a cor do mundo 1;
  o gate de cada mundo mostra a neblina e o céu da paleta e a skin mais cara com a cor dela.
- `Tools/rt4-check.sh`: `scene`, `validate`, `isolation`, `editmode`, `playmode`, `economy`, `apk-game`, `apk-measure`,
  `aab` PASS; `capture` falhou na primeira rodada (paleta descarregada ao abrir cena nova no modo de edição) e passou
  depois da correção, junto com um novo `validate`. Nenhum `Shader error` no log do APK de medição.
- Revisão independente (subagente fable, só leitura): o kit citava "tower" (isolamento) — corrigido; resto menor
  (T-RT6t6a-10).
- Sem gate no OPPO: o aceite não pede (1 desenho de céu no lugar da limpeza em cor sólida; o gate final é na 6t.12).

### Capturas

- [`art/rt6t/mundos-oppo.png`](art/rt6t/mundos-oppo.png) (Unity com GPU, formato do OPPO, metade do tamanho): os 4
  mundos na corrida do gate com 300, skin padrão. O topo da tela tem exatamente a cor de topo do céu de cada paleta
  (lido no pixel), a multidão sai rosa, creme, amarela e verde-limão, a neblina na cor do horizonte. Cenário e pista
  ainda são a arte da RT-6 (escura), até a 6t.8.
- `art/telas/` regravadas com as paletas (menu, loja, assalto e os 4 mundos). Observação para a 6t.10: com a neblina
  22–75 a torre fica lavada na cor da neblina (no Pastel quase some contra o céu).

## RT-6t.6b — Formas e skins no menu, save v4 (2026-11-22)

D25 = A (com olhos) e D26 = A (formas livres, BARRO = cor do mundo, 8 skins com cores novas, mesmos preços), respondidas
em 2026-11-22.

### O que foi feito

| Peça | Onde | O que faz |
|---|---|---|
| Formas no catálogo | `ClaybreakCatalog.Shapes` (`CrowdShapeOption`), `WorldContent.Shapes` | BALL (`ball`, Bolinha com olhos, padrão) e CHICK (`chick`, Pintinho com olhos); malhas geradas da 6t.2. Saiu o `ClaybreakTheme.crowdShape`. |
| Save v4 | `SaveData.shape`, `SaveMigration.AddShape` | v1/v2/v3 → v4 recebem `ball` (a forma que já mostravam); desconhecido/vazio/nulo cai no padrão; `SaveValidator` reprova forma fora do catálogo; backup do JSON original por versão (já existia). |
| Seletor | tela inicial, `ClaybreakScreens.ShowShape`, `ClaybreakFlow.OnShape` | Botão "SHAPE: BALL" sob a linha de elites; cada toque passa à próxima forma, grava o save e troca a malha da multidão atrás do menu (grátis; some com uma forma só). |
| Corrida | `ClaybreakFlow.Configure`/`OnShape` → `RunCrowd.UseShape` | Desenha com a forma do save; o gate veste a de mais triângulos (Pintinho, 492 contra 484). O assalto segue com os bonecos antigos até a 6t.7. |
| Skins | `WorldContent.Skins` | Mesmos ids, ordem e preços (total 19.380); cores e nomes novos: CORAL #ff8a5c, MANGO #ffc94a, SKY #6fa8ff, LILAC #b98cf0, INDIGO #3d3a7c, LAGOON #2fb3c4, BERRY #e0569b, BUBBLEGUM #ff9ed2. A grátis vira WORLD COLOR e mostra a cor do mundo atual na loja. |
| Contraste | `ClaybreakValidation.Warnings` | Cada skin paga contra pista, faixa e chão dos 4 mundos, os 2 portões e as outras skins (ΔE CIE76 < 20 avisa). Menor folga: INDIGO × chão do Pôr do sol 22,5; entre skins 26,0. **Nenhum aviso de skin**; seguem só os 2 avisos da paleta registrados na 6t.6a. |

Decisões T-RT6t6b-1..10 em `decisoes.md`.

### Testes e validação

- EditMode **214/214**: migração v3 → v4 (fixture `v3-four-worlds` intacta, sai com `ball` e o resto igual), v1 e v2 →
  `ball`, fixture nova `v4-chick-bubblegum` mantém `chick`, forma desconhecida/vazia/ausente/nula cai no padrão e passa no
  validador, troca de forma grátis e circular com ida e volta pelo JSON, validador reprova forma fora do catálogo, gate com
  a forma mais pesada (também com ela em primeiro), amostra da cor do mundo na loja com outra skin vestida.
- PlayMode **21/21** com `-nographics` (+8 capturas que só rodam com GPU): novo `ShapePickerDrawsTheRunWithThePickedShape`
  (o botão troca a malha atrás do menu, o rótulo, não cobra, e a corrida sai com a forma escolhida).
- `Tools/rt4-check.sh`: `scene`, `validate`, `isolation`, `editmode`, `playmode`, `economy`, `capture`, `apk-game`,
  `apk-measure`, `aab` PASS; sem `Shader error` no APK de medição. Depois dos ajustes da revisão e de uma correção vista
  na captura da loja (a amostra de WORLD COLOR mostrava a cor da skin vestida; agora `Progression.WorldCrowdColor`),
  `validate`, `editmode`, `playmode` e `capture` rodaram de novo: PASS.
- Revisão de segurança do save (subagente fable, só leitura; o `/security-review` não roda neste repositório): nada sério;
  ajustes aplicados em T-RT6t6b-10.
- Sem gate no OPPO: o aceite não pede (troca de malha e de cor; o Pintinho tem 8 triângulos a mais que a Bolinha medida
  na 6t.4/6t.5).

### Capturas (Unity com GPU, formato do OPPO)

- [`art/rt6t/menu-forma.png`](art/rt6t/menu-forma.png): a tela inicial com o seletor, antes e depois de um toque (BALL →
  CHICK; a multidão atrás do menu troca de forma).
- [`art/rt6t/formas-mundos.png`](art/rt6t/formas-mundos.png): Bolinha (em cima) e Pintinho (embaixo) nos 4 mundos, skin
  padrão (cor de cada mundo), corrida do gate com 300.
- [`art/rt6t/skins-mundos.png`](art/rt6t/skins-mundos.png): WORLD COLOR e as 8 skins (colunas) nos 4 mundos (linhas): todas
  legíveis sobre a pista; MANGO no mundo Doce fica perto da cor padrão do mundo (amarela), o que é esperado e não confunde
  com portão.
- `art/telas/` regravadas (menu com o botão SHAPE, loja de skins com os nomes novos); `art/rt6t/mundos-oppo.png` regravada
  com a Bolinha.

## RT-6t.8 — Cenário instanciado e pista listrada (2026-11-22)

### O que foi feito

| Peça | Onde | O que faz |
|---|---|---|
| `ToonLit` variante de slots | incubadora, `Toon` | Palavra-chave `STUDIOKIT_TOON_SLOTS`: o RGB da cor de vértice pesa três cores (`_ToonSlot0..2`); uma malha veste qualquer paleta. |
| `StudioKit/StripedTrack` | incubadora, `Toon` | Pista sem luz (como o protótipo), listras procedurais pelo Z do mundo (2 m + 2 m), borda filtrada por `fwidth`, neblina. Sem textura, sem offset por quadro. |
| `PropScatter` + `InstancedProps` | incubadora, `Swarm` | Janela de 90 m que segue a câmera (4 m atrás); posição de cada objeto é função da câmera (fase fixa + volta sorteada por hash): recicla para o fundo em outro lugar, inclusive ao voltar ao início. 1 `GraphicsBuffer` e **1 chamada por camada**; alocado na carga, recalculado só quando a câmera anda. |
| `AddIcosahedron(…, detail)` | incubadora, `LowPolyMeshBuilder` | Icosaedro subdividido (como o three.js) para a pedra. |
| `SceneryArt` | jogo, Editor | Receitas do protótipo: árvore (tronco + 2 cones, 50 triângulos), pedra (2 icosaedros detail 1, 160), chão 300 m + 2 trilhos numa malha só; materiais e montagem. |
| `WorldScenery` | jogo | Aplica a paleta (pista, faixa, chão, trilhos, árvores, pedras, céu, neblina), a densidade do mundo e a luz. Um conjunto só para os 4 mundos. |
| `WorldLook.trees/rocks` | jogo | Densidade por mundo [P]: Pastel 65/29 (a do protótipo), Pôr do sol 52/36, Doce 78/22, Aurora 44/44. |
| Validador | `ClaybreakValidation` | Densidade ≤ pré-alocado, **≤ 3 chamadas no cenário lateral**, objetos longe da pista e dos trilhos, nenhum outro `Renderer` no cenário. |

Retirados: `WorldSceneryArt` (cenário Simple Lit por mundo, silhuetas distantes), `SceneryStyle`, as cores antigas do
`WorldLook` e os materiais `Materials/world1..4`, `Ground` e `Field`. Decisões T-RT6t8-1..11 em `decisoes.md`.

### Chamadas de desenho

Cenário lateral: **3** (chão + trilhos, árvores, pedras) — antes eram dezenas de objetos por mundo com batching
estático. A pista é 1 chamada. Nenhum objeto por árvore/pedra; nada é criado nem destruído em jogo.

### Testes e validação

- EditMode **203/203** (novos: `PropScatterTests` — janela, faixa lateral, escala, giro só em Y com escala igual,
  reciclagem uma janela à frente em outro lugar, distribuição nos dois lados e ao longo da janela, camadas/sementes
  diferentes, configuração inválida; `InstancedPropsTests` — limite da capacidade, uma chamada por camada mostrada,
  posição igual à função pura, material sem as palavras-chave; icosaedro subdividido: contagem e esfera unitária).
- PlayMode: `EveryWorldDeviceGateRunsItsSceneryBossAndShield` confere densidade, cores e ≤ 3 desenhos em cada mundo;
  novo `SidePropsFollowTheCameraBesideTheTrack` (as árvores que a câmera passa voltam uma janela à frente; nenhum objeto
  sai da janela nem encosta na pista).
- Sem gate no OPPO: o aceite não pede (fica para a 6t.12).

### Capturas

- [`art/rt6t/cenario-oppo.png`](art/rt6t/cenario-oppo.png) (Unity com GPU, formato do OPPO, metade do tamanho): os 4
  mundos na corrida do gate com 300 — pista listrada, trilhos na cor da faixa, chão, árvores de 2 tons com tronco e
  pedras com "musgo", tudo nas cores de cada paleta e sumindo na neblina. O registro da 6t.6a
  ([`mundos-oppo.png`](art/rt6t/mundos-oppo.png)) fica com o cenário antigo.
- `art/telas/` regravadas com o cenário novo.

## RT-6t.7 — A multidão instanciada no assalto (2026-11-22)

### O que foi feito

| Peça | Onde | O que faz |
|---|---|---|
| `LooseSwarmRenderer` | incubadora, `Swarm` | Unidades soltas, cada uma posta pelo dono (as do assalto não têm formação): posição, velocidade lateral, **tamanho e tinta por instância**, pulo/agachamento/inclinação do `Swarm` num job Burst, mesma malha, `ToonLit` e sombra-bolha. **1 chamada para corpos (vivos + morrendo) + 1 para sombras.** Remoção = a última unidade ocupa o índice livre. Nada alocado em jogo. |
| `SwarmDyingRing` | incubadora, `Swarm` | O anel de morte balística saiu do `SwarmSimulation` (mesmo comportamento) e é usado pelos dois renderizadores; a morte guarda o tamanho (elites encolhem do próprio tamanho) e pode nascer acima do chão. |
| `SwarmCameraShot` + `SwarmCameraRig.HoldShot` | incubadora, `Swarm` | Plano fixo como dado: o rig sai da multidão e vai para o plano em 0,8 s, e volta com `ReleaseShot`. `Content/Data/SwarmCameraAssault.asset` e `SwarmCameraAssaultBoss.asset` = o enquadramento do antigo `AssaultCamera` (removido). |
| `AssaultField` | jogo | Os mesmos arrays e regras; o índice i do campo é a unidade i do renderizador. Portão ÷ (0 saem), varredor, muro, escudo e canhão (para longe do impacto) matam pelo anel; quem chega à torre some. O andar da torre que cai joga 6 bonecos por bloco, da altura do bloco. |
| Defesas e torre | jogo | Sem mudança de regra; o canhão e a torre deixaram o respingo do kit (`RunnerDeathEffects` saiu do jogo). |
| Forma e skin | `ClaybreakFlow` | A forma escolhida no menu vale na corrida e no assalto; os comuns vestem a cor da skin, os elites a cor e o tamanho do tipo. |
| Cena | `ClaybreakSceneBuilder` | Saíram o `RunnerPool` (300 objetos), o `UnitGrounding` e o `RunnerUnit.prefab`; entrou `AssaultField/UnitsInFlight` (300 + 512 morrendo; D27 sem resposta = 300). |

Regras, fases e economia sem mudança (`docs/rt6-economia.md` regenerada igual). Decisões T-RT6t7-1..10 em `decisoes.md`.

### Testes e validação

- `Tools/rt4-check.sh` completo (2026-11-22): `scene`, `validate`, `isolation`, `editmode` (**217/217**), `playmode`
  (**21/21**, 9 capturas só com GPU), `economy`, `capture`, `apk-game`, `aab`: PASS; `apk-measure` falhou na primeira
  vez (um `using` removido que só a medição compila) e passou depois da correção, junto com `apk-game` (sem erro de shader).
- Novos: EditMode `ShotPoseIsTheAnchorOffsetLookingPartOfTheWay`, `LooseLaunchThrowsAlongTheDirectionAtItsSize`,
  `DyingRingKeepsTheNewestAndShrinksFromTheSize`; PlayMode: no assalto do nível 1 cada unidade em voo é uma instância e
  fica na pista, o rig segura o plano do assalto com o FOV dele; a forma do menu chega às unidades em voo; no gate,
  300 instâncias, elites redimensionados e mortes pelo anel. Validador: nenhum collider, nada do pool do kit, capacidades,
  materiais e os dois planos.
- Revisor independente (subagente fable, só leitura): nada sério; os pontos menores foram aplicados (T-RT6t7-10).
- Capturas com GPU no formato do OPPO: [`art/rt6t/assalto-oppo.png`](art/rt6t/assalto-oppo.png) (gate do mundo 4:
  Pintinho 492 tri, skin BUBBLEGUM, 300 em voo com Brutos e Corredores, mortes) e o quadro "assault" de
  [`art/rt6t/corrida-oppo.png`](art/rt6t/corrida-oppo.png) (nível 1).

### Gate 3× no assalto (OPPO CPH2791) — **PASSOU** (2026-11-22)

Caso: nível de gate do **mundo 4** (torre do chefe, canhão com 12 pedras, varredor, muro, escudo), 300 em voo
reciclando com 100 elites, 40–65 morrendo o tempo todo, forma mais pesada e skin mais cara. Comando:
`Tools/rt2-gate.sh assault-w4 60 20 /c/temp/rt6t7/Claybreak-measure.apk 4`. CPU e GPU = pior p95 das janelas 1–5 (a 6ª
coincide com a coleta de memória do script).

| Rodada | CPU p95 | GPU p95 | Quadro p95 | > 50 ms | PSS | Alocação | Resultado |
|---|---|---|---|---|---|---|---|
| assalto, mundo 4, 300 em voo (6t.7) | 2,6 ms | 4,3 ms | 16 ms (62,5 fps) | 0,0% | 292 MB | 0 B | **passa** |
| antes: assalto com o pool do kit (6t.4) | 9,4 ms | 4,5 ms | 16 ms | 0,2% | 310 MB | 0 B | dentro do gate |

Captura do aparelho durante o gate: [`art/rt6t/assalto-oppo-gate.png`](art/rt6t/assalto-oppo-gate.png) (sem shader de
erro; elites no próprio tamanho e cor).

## RT-6t.9 — Portões, Baloo 2 e feedback (2026-11-22)

### O que foi feito

| Peça | Onde | O que faz |
|---|---|---|
| Baloo 2 ExtraBold | `Content/Fonts/` | Baixada só de `github.com/google/fonts` (`ofl/baloo2`, commit fixado), `OFL.txt` ao lado, hashes registrados em [`audio-licenses.md`](audio-licenses.md) seção 2. O repositório só tem a fonte variável: `Tools/fonts/baloo2-extrabold.py` confere os hashes e fixa o peso 800 (reprodutível). A Roboto saiu. |
| `GameFonts` | Editor | Atlas SDF **estático** do TextMeshPro (124 caracteres: ASCII, `− × ÷ · …`, acentos do português) e 4 presets de contorno no mesmo atlas: portão bom #1f7a55, portão ruim #a52a3a, número flutuante (branco), contador (#3b2f4a). TMP Essential Resources importados do próprio pacote `com.unity.ugui`. |
| Portões da corrida | `RunGateCourse`, `GateArt` | Como o protótipo: dois painéis de meia pista (#5ad1a0 / #ff6b81 a 0,45, transparentes sem escrita de profundidade), três postes brancos no `ToonLit`, rótulo TMP grande em Baloo 2 com o contorno do lado; caixotes com "+n" no mesmo estilo. Saíram a verga e os postes de pedra. |
| Fileira do assalto | `AssaultField` (só `DressGates`), cena | Mesmos painéis, rótulos TMP e postes brancos entre as faixas (saiu a barra de pedra); a regra de chefe que vira/gira portões troca o preset junto. |
| Sinais verdadeiros | `GateSpec` | `×2`, `−30`, `÷2`, `+10` (antes `x2`, `-30`); `WriteLabel(char[])` escreve sem alocar para `SetCharArray`. |
| Número flutuante | `FloatingNumbers` (HUD) | 4 rótulos TMP pré-alocados em rodízio; sobe 110 px do protótipo com o "bounce" 0,4 → 1,25 → 1 e some em 1 s; verde #2fbf86 / vermelho #ff4d6d com contorno branco. |
| Pop do contador | `ClaybreakHud` | Contador da multidão em TMP com contorno; escala 1,18 em 0,15 s e volta. |
| Shake no portão ruim | (já ligado na 6t.4/6t.5) | `OnGateCrossed` → `RunCrowd.Shake` → rig. |
| Telas e HUD | `ClaybreakSceneBuilder.FontPath` | Toda `UI.Text` e os rótulos de vida passam a usar a Baloo 2 (TTF dinâmica), sem migrar telas para TMP. Créditos: "FONT: BALOO 2 (SIL OFL 1.1)". |
| Capturas de modo de edição | `ArtDirectionCapture` | Passa a aplicar o `ToonLook` do jogo antes das fotos (o `OnEnable` do aplicador não roda em modo de edição; os postes brancos saíam pretos). |

Decisões T-RT6t9-1..11 em `decisoes.md` (a 11 é a revisão independente: nada sério, pontos menores aplicados). Nada foi tocado no cenário nem na pista.

### Testes e validação

- EditMode novos: rótulos com os sinais verdadeiros e escrita sem alocação; curva do número e do pop iguais ao protótipo;
  hashes da fonte e da licença, atlas estático com todos os glifos, presets com contorno no mesmo atlas.
- PlayMode novo `GateFeedbackTests`: rótulos da corrida e do assalto em Baloo 2 com o preset do lado; ao cruzar o primeiro
  portão, o número flutua (texto e cor do portão), o contador chega a 1,18 e volta, o número some em 1 s; nenhum objeto
  criado em jogo.
- `Tools/rt4-check.sh` (2026-11-22): `scene`, `validate`, `isolation`, `editmode` (**221/221**), `playmode` (**22/22**, 10 capturas só com
  GPU), `economy` (tabela igual), `capture`, `apk-game`, `apk-measure` (sem erro de shader), `aab`: PASS. Na primeira rodada um
  teste novo comparou cores sem tolerância (corrigido); depois do ajuste de tamanho dos rótulos, segunda rodada de `scene`,
  `validate`, `editmode`, `playmode`, `capture` e `apk`: PASS.

### Capturas (Unity com GPU, formato do OPPO)

- [`art/rt6t/portoes-oppo.png`](art/rt6t/portoes-oppo.png): (1) o primeiro par chegando (painéis, postes, rótulos com
  contorno); (2) logo depois de cruzar: "×2" flutuando e o contador no pop; (3) glifos — contador TMP com `+10 −30 ×2 ÷2`,
  número flutuante `÷2` e o banner `UI.Text` com `AÇÃO ÁÉÍÓÚ ÂÊÔ ÃÕ àçãéêõ` e os sinais; (4) a fileira do assalto.
  Comando: `-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.GateFeedbackTests -gateCaptureDir <pasta>` sem
  `-nographics`.
- `art/telas/` regeneradas (fonte Baloo 2 nas telas e nos portões).
- Gate de desempenho: não faz parte do aceite desta entrega (fica para a 6t.12). Tentativa extra no fim: o OPPO estava
  conectado mas com a tela de bloqueio (senha), então não rodou. A mudança em jogo é pequena: texto TMP com atlas estático
  (nada rasterizado em jogo), 4 rótulos de HUD e postes de 1 malha cada.

## RT-6t.10 — Arte do assalto (2026-11-22)

D28 sem resposta: vale a opção A da ficha (sombreado toon e cores de torre/defesa da paleta, sem remodelar, sem rostos).

### O que foi feito

| Peça | Onde | O que faz |
|---|---|---|
| Cores de estrutura no `ToonLit` | incubadora, `ToonLit.shader`, `ToonShaderIds`, `ToonPaletteApplier` | Por material: `_StructureWeights` (clara, escura, detalhe, defesa; o resto até 1 fica branco), `_ToonEmission` (0 iluminado, 1 sem luz) e `_ToonFogAmount` (fração da neblina). Globais `_StudioKitStructureLight/Dark/Detail` e `_StudioKitDefense` escritas pelo aplicador da paleta na troca de mundo. Com os valores padrão o shader é o mesmo de antes (multidão, cenário e postes iguais). |
| Materiais do assalto | `ArtPalette` | Os 28 papéis opacos (pedra, ferro, madeira, estandarte, tijolo, gelo, basalto, osso, muralha, caixote, chamas...) viram `ToonLit` com uma mistura das 4 cores da paleta; efeitos com emissão e a cor de detalhe clareada; estruturas com 35% da neblina. Cores fixas para sinais de jogo: vida, escudo, faixa de perigo, portões. Sombra do projétil #2b2233 a 0,4. |
| Primitivas | `ToonPrimitives`, `Prim` | Cubo, cilindro e esfera com cor de vértice branca (o shader lê a cor de vértice). Mesmas formas: nenhuma torre foi remodelada. |
| Paleta Pastel | `PaletteContent` | Torre clara/escura #f1d2e1 / #ab86b8 (antes #f6e9f2 / #c4a7c9, que sumiam no céu rosa). Demais paletas sem mudança. |
| Guardas | `ClaybreakValidation`, `AssaultArtTests` | Todo renderizador opaco do assalto é toon com cor de vértice; materiais com pesos válidos; nenhum Simple Lit sobrou. |

Decisões D28 (A por padrão) e T-RT6t10-1..8 em `decisoes.md`.

### Capturas (Unity com GPU, formato do OPPO, metade do tamanho)

- [`art/rt6t/chefes-oppo.png`](art/rt6t/chefes-oppo.png): os 4 chefes (cidadela, campanário afundado, martelo a vapor,
  coroa de gelo) no nível de gate de cada mundo, na câmera do assalto do jogo, com canhão, pedras em voo, muralha, escudo,
  varredor e a multidão atacando (o gate veste a skin mais cara).
- [`art/rt6t/chefes-perto.png`](art/rt6t/chefes-perto.png): os mesmos 4 de perto (câmera só da captura, acima da linha
  de tiro), para ver as 3 faixas do sombreado, as cores da paleta e o gerador do escudo.
- `art/telas/` regravadas (menu, loja, assalto e os 4 mundos com as torres novas).
- Comando: `-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.BossCaptureTests -bossCaptureDir <pasta>` sem
  `-nographics`.

## RT-6t.11 — Reserva para OpenGL ES (2026-11-22)

### O que foi feito

| Peça | Onde | O que faz |
|---|---|---|
| `ToonInstanceRouting` | incubadora, `Swarm` | Escolhe a rota uma vez por aparelho: **buffer** se `SystemInfo.supportsInstancing`, `maxComputeBufferInputsVertex > 0` e `graphicsShaderLevel >= 45`; senão **blocos**. Liga/desliga a palavra-chave global `STUDIOKIT_TOON_BUFFER` e escreve no log `[ToonRoute] route=… api=… gpu=… vertexBuffers=…`. Testes e o APK de medição podem forçar uma rota (`Forced`). |
| `ToonInstanceDraw` | incubadora, `Swarm` | Uma peça de desenho para as duas rotas. Buffer: o de antes (1 buffer estruturado, 1 chamada para corpos + 1 para sombras, `RenderMeshPrimitives`). Blocos: um job Burst escreve a matriz e a tinta de cada instância numa passada; cada bloco de **256** é uma chamada `RenderMeshInstanced` com as tintas no seu `MaterialPropertyBlock`; as sombras saem das mesmas matrizes e blocos. Tudo alocado na carga; nada por quadro. |
| `SwarmRenderer`, `LooseSwarmRenderer`, `InstancedProps` | incubadora | Os três desenham pela `ToonInstanceDraw` (corrida, unidades em voo do assalto e cenário). Expõem `Route` e `DrawCalls`. Se uma rota for forçada com o jogo rodando, trocam de rota no quadro seguinte. |
| `ToonLit`, `BlobShadowInstanced`, `ToonInstanceData.hlsl` | incubadora | Variante própria da rota de blocos: sem a palavra-chave global, a instância vem dos arrays de instancing do Unity (matriz + `_ToonInstanceTint`) em vez do buffer, com a **mesma conta** de posição, normal e tinta. `#pragma target 3.5` (OpenGL ES 3.0 basta) e `4.5` só com o buffer; `forcemaxcount:256` = 16 KB, o menor bloco uniforme do GLES 3. |
| Materiais | jogo | `CrowdToon`, `CrowdShadow` e `PropsToon` com GPU instancing ligado (a rota de blocos exige; o validador confere). |
| `DrawRouteExtra` | jogo | Só no APK de medição: `--ez chunks true|false` força a rota antes da primeira cena. Com `-e unity -force-gles` o aparelho roda em OpenGL ES. `Tools/rt2-gate.sh` aceita `RT2_EXTRA` para isso. |

Chamadas na rota de blocos: 300 na corrida = 2 (corpos) + 2 (sombras); 300 em voo + até 212 morrendo = 2 + 2;
1000 vivos + 512 morrendo = 6 + 4. Cenário: uma por camada (menos de 256 por camada).

### Testes e validação

- `Tools/rt4-check.sh` completo (2026-11-22): `scene`, `validate`, `isolation`, `editmode` (**229/229**), `playmode`
  (**22 passam**, 12 capturas só com GPU), `economy`, `capture`, `apk-game`, `apk-measure`, `aab`: **PASS**, nenhum erro de
  shader nos builds.
- Novos EditMode (`ToonInstanceDrawTests`): a matriz é a transformação da instância; o job escreve matriz e tinta de
  todas; uma chamada por bloco; os shaders usam blocos de 256 e compilam; rota forçada vence e liga a palavra-chave;
  material sem instancing é recusado. Validador: os três materiais instanciados com instancing ligado.
- Revisor independente (subagente fable, só leitura): nada bloqueante; aplicados o comentário da margem zero do bloco de
  16 KB e o job sem paralelismo para camadas pequenas.

### As duas rotas desenham igual (captura)

`-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.DrawRouteCaptureTests -routeCaptureDir <pasta>` sem
`-nographics`: corrida e assalto mais pesados do mundo 4 congelados (escala de tempo 0), desenhados por cada rota no
formato do OPPO e comparados pixel a pixel. Resultado no Editor (Direct3D 12): **0 pixels diferentes** nas duas cenas
(corrida: 300 + cenário, 4 chamadas na rota de blocos; assalto: 300 em voo + 12 morrendo, 4 chamadas).
[`art/rt6t/rotas-oppo.png`](art/rt6t/rotas-oppo.png): corrida buffer | corrida blocos | assalto buffer | assalto blocos.

### Gate 3× no OPPO CPH2791 em OpenGL ES — **PASSOU** (2026-11-22)

Comando: `RT2_EXTRA="--ez chunks true -e unity -force-gles" Tools/rt2-gate.sh assault-w4-gles-chunks 60 20
/c/temp/rt6t11/Claybreak-measure.apk 4` (assalto do mundo 4, o caso mais pesado: 300 em voo com 100 elites, 37–75
morrendo, canhão com 12 pedras, forma e skin mais pesadas, teto D27 = 300). Linha do log:
`[ToonRoute] route=Chunks forced=yes api=OpenGLES3 gpu="Mali-G1-Ultra MC12" shaderLevel=50 vertexBuffers=0`.

| Rodada | API | Rota | CPU p95 | GPU p95 | Quadro p95 | > 50 ms | PSS | Alocação | Resultado |
|---|---|---|---|---|---|---|---|---|---|
| assalto, mundo 4 (6t.11) | OpenGL ES 3 | blocos | 2,5 ms (3,5 na janela 6, coleta de memória) | 6,9 ms | 16 ms (62,5 fps) | 0,0% | 309 MB | 0 B | **passa** |
| antes: assalto, mundo 4 (6t.7) | Vulkan | buffer | 2,6 ms | 4,3 ms | 16 ms | 0,0% | 292 MB | 0 B | passa |

Leitura: **em OpenGL ES o Mali do OPPO não lê buffer estruturado no vertex shader** (`vertexBuffers=0`): sem a reserva, a
multidão não desenharia nesse modo; a escolha automática já cairia na rota de blocos. A reserva custa ~2,6 ms de GPU a
mais que o Vulkan com buffer e continua com mais de 3× de folga em CPU e GPU. Captura do aparelho durante o gate
(desenho certo, sem shader de erro): [`art/rt6t/assalto-oppo-gles.png`](art/rt6t/assalto-oppo-gles.png).

## RT-6t.12 — O motor no kit e o fechamento da série (2026-11-22)

### O que foi feito

- **Motor no `studiokit` 0.5.0 (K.11, tag `v0.5.0`).** `Assets/StudioKitIncubator/**` saiu deste repositório e foi para o
  pacote com os `.meta` (GUIDs iguais; cenas, materiais e assets do jogo apontam para os mesmos scripts e shaders sem
  mudança): `Runtime/Toon`, `Runtime/Swarm`, shaders em `Shared/Shaders`, testes no `JungleDegree.StudioKit.Tests` do kit.
  `package.json` com `com.unity.burst` 2.0.0 e `com.unity.collections` 6.6.0 (as versões que os dois jogos já
  resolviam). Decisões T-RT6t12-1..6 em `decisoes.md`; T23 no `decisoes.md` do studiokit.
- **Caminho antigo de massinha fora do jogo.** O tema perdeu `roster` (corpo `ClayRunnerFrames`) e `deathEffect`
  (respingo `RunnerSplat`), que só eram validados; os asmdefs do jogo não referenciam mais `StudioKit.Characters`.
  Prefab e pool de corredores já tinham saído na 6t.4/6t.7; a Roboto, na 6t.9. Varredura de órfãos por GUID
  (`Assets/Claybreak`, `Assets/Settings`, `Assets/Editor`): **nenhum asset sem referência**.
- **Validador e isolamento.** A regra 2.2.1 do validador do jogo continua sobre o código `Toon`/`Swarm`, agora no
  caminho do pacote; a lista de isolamento do jogo não cita mais a incubadora.
- **Capturas regravadas com o pacote** (PlayMode com GPU, 11/11): `art/rt6t/` — mundos, corrida, assalto, chefes, formas,
  skins, menu, portões, animações, chão sob a multidão e as duas rotas. Desenho igual ao da 6t.11, sem shader de erro.
  As telas de modo de edição (`art/telas/`) também foram regravadas; nelas a multidão não aparece (só existe jogando,
  T-RT6t12-5).

### Validação

- `Tools/rt4-check.sh` completo: `scene`, `validate`, `isolation` (PASS com `Toon` e `Swarm` dentro do pacote),
  `editmode` (**229/229**), `playmode`, `economy`, `capture`, `apk-game`, `apk-measure` (sem `Shader error`), `aab`:
  **todos PASS**.
- **Aceitação do studiokit no Wild Pack** (rodada sozinha, depois deste projeto): completude 90/0 com o mesmo aviso,
  assets 117/0, isolamento PASS, `AudioValidation` 16, save **87/87**, anúncios offline **25/25** (a linha de base
  passou de 24 para 25 na 8.6g do próprio Wild Pack, `50decda`; o kit não mexeu em anúncios), APK de teste do Forest
  gerado. Nada foi commitado no Wild Pack: ao abrir o projeto, o Unity acrescenta ao `packages-lock.json` as duas
  dependências novas na entrada do kit; a próxima sessão do Wild Pack commita essa mudança junto com o trabalho dela.
- `TestProject` do studiokit: EditMode **92/92**, isolamento com a lista ampla PASS (depois de tirar de um comentário
  do `MobileAdsGate` o nome de um jogo, que vinha de antes desta entrega).

### Gate 3× final no OPPO CPH2791 — **PASSOU** (2026-11-22)

APK de medição desta entrega (pacote 0.5.0), Vulkan, rota de buffer (`[ToonRoute] route=Buffer api=Vulkan gpu="Mali-G1-Ultra
MC12"`). CPU e GPU = pior p95 das janelas 1–5 (a 6ª coincide com a coleta de memória do script). O OPPO ficou bloqueado
durante a sessão e foi desbloqueado no fim; o gate rodou depois do commit da entrega.

| Caso | Comando | CPU p95 | GPU p95 | Quadro p95 | > 50 ms | PSS | Alocação | Resultado |
|---|---|---|---|---|---|---|---|---|
| corrida do mundo 4, 300 no teto + cenário | `Tools/rt2-gate.sh final-run-w4 60 20 /c/temp/rt612/Claybreak-measure.apk run:4` | 1,8 ms | 4,4 ms | 16 ms (62,5 fps) | 0,0% | 305 MB | 0 B | **passa** |
| assalto do mundo 4: chefe, canhão 12/12, varredor, muro, escudo, 300 em voo (100 elites, 14–59 morrendo) | `Tools/rt2-gate.sh final-assault-w4 60 20 /c/temp/rt612/Claybreak-measure.apk 4` | 2,3 ms | 4,4 ms | 16 ms (62,5 fps) | 0,0% | 313 MB | 0 B | **passa** |
| antes: assalto do mundo 4 (6t.7, kit incubado) | — | 2,6 ms | 4,3 ms | 16 ms | 0,0% | 292 MB | 0 B | passa |

Leitura: mudar o motor de lugar não mudou o custo (mesmos números da 6t.4/6t.7); folga de mais de 4× em CPU e 2,5× em GPU
sobre o limite de 11 ms, quadro sempre a 60 fps.

## RT-6t.13 — Ajustes pós-teste: neblina quase zero e líder da multidão (2026-11-22)

O usuário jogou no OPPO e pediu: neblina "próxima de zero" e um líder visível que decide o portão (desenho aprovado
"como proposto"). Decisões: usuário e T-RT6t13-1..8 em `decisoes.md`. Nada mudou no `studiokit` (resolvido no jogo).

### O que foi feito

- **Neblina 110–150 m nos 4 mundos** (`PaletteContent`, dado). Multidão, portões e a torre próxima ficam sem neblina; ela só
  fecha o chão longe e termina antes do plano de corte (170 m), então o horizonte não corta seco.
- **Nova proteção contra árvore "aparecendo do nada".** Janela de reciclagem do cenário de 90 → 180 m, com árvores e pedras
  de cada mundo dobradas (mesma densidade). O validador troca a regra antiga por `PropWrapGuard`: de todas as câmeras do jogo
  (corrida em vários tamanhos de multidão e posições, ajustada a 21:9/19,5:9/16:9, e os dois enquadramentos do assalto), o
  ponto mais perto onde uma árvore reaparece à vista fica a **155,8 m** — depois do fim da neblina (150 m). Com a janela
  antiga seria ~80 m (teste EditMode prova que falharia).
- **Líder da multidão** (`CrowdLeader`): mesma forma e cor da multidão, 1,6×, coroa de 5 pontas na cor de UI do mundo
  (joias brancas, emissão 0,5), pulando e inclinando como as unidades. Desenhado no pipeline instanciado
  (`ToonInstanceDraw`, 1 instância de corpo + sombra, 1 de coroa: +3 chamadas), sem renderer nem objeto criado em jogo.
- **A multidão segue atrás.** `RunCrowd.Position` é o líder (corre na velocidade da fase; o dedo o move de lado); o centro
  da formação girassol fica atrás dele pela profundidade do disco + 0,9 m, suavizado (recua 3× mais rápido que avança).
- **O líder decide:** portão e caixote pelo lado em que ele está; a fase entra no assalto quando ele chega à linha. A dupla
  cruzada fica de pé: o lado escolhido acende (painel quase opaco da cor) e o outro apaga (cinza, número a 30%).
- **Assalto:** o líder fica parado na linha de lançamento, andando de lado com o dedo para marcar a faixa; não é lançado nem
  conta no número; regras de dano iguais.

### Testes e validação

- `Tools/rt4-check.sh` completo: `scene`, `validate`, `isolation`, `editmode` (**305/305**), `playmode` (24 passam, 13 capturas
  ignoradas sem GPU), `economy`, `capture`, `apk-game`, `apk-measure`, `aab`: **todos PASS**; rodada de novo depois da
  revisão (`scene validate editmode playmode apk`): **todos PASS** (EditMode 305/305, PlayMode 24 + 13 ignoradas, sem `Shader error` no APK de medição); capturas PlayMode com GPU 5/5.
- Novos: EditMode `LeaderAndFogTests` (guarda com a janela antiga e a nova, coroa); PlayMode `CrowdLeaderTests` (líder fora do
  número, à frente de todas as unidades, portão da esquerda escolhido por ele, acender/apagar, nada criado em jogo; no assalto
  parado na linha).

### Capturas (Unity com GPU, formato do OPPO, metade do tamanho) — `art/rt6t/`

- `lider-oppo.png`: começo (líder com coroa à frente), líder à esquerda antes do 1º portão, logo depois (o "+10" aceso, o
  "×2" apagado), multidão no teto atrás do líder, assalto com o líder na linha.
- `mundos-oppo.png`: os 4 mundos com a neblina nova (regravada). `corrida-oppo.png` e `portoes-oppo.png` regravadas com o
  líder e o novo feedback.

### Gate 3× no OPPO CPH2791 — **PASSOU** (2026-11-22)

APK de medição desta entrega (`/c/temp/rt6t13/Claybreak-measure.apk`), Vulkan, rota de buffer (`[ToonRoute] route=Buffer
api=Vulkan gpu="Mali-G1-Ultra MC12"`). Critério de sempre (T-RT6t4-8): CPU principal p95 ≤ 11 ms, GPU p95 ≤ 11 ms, quadro
p95 ≤ 16,7 ms, < 5% dos quadros > 50 ms, PSS ≤ 450 MB. CPU e GPU = pior p95 das janelas 1–5 (a 6ª coincide com a coleta de
memória do script). O aparelho foi conectado e desbloqueado depois do commit da entrega; o gate rodou sobre o mesmo APK.

| Caso | Comando | CPU p95 | GPU p95 | Quadro p95 | > 50 ms | PSS | Alocação | Resultado |
|---|---|---|---|---|---|---|---|---|
| corrida do mundo 4, 300 no teto + líder + cenário com o dobro de árvores e pedras | `Tools/rt2-gate.sh rt6t13-run-w4 60 20 /c/temp/rt6t13/Claybreak-measure.apk run:4` | 2,4 ms | 4,9 ms | 16 ms (62,5 fps) | 0,0% | 295 MB | 0 B | **passa** |
| assalto do mundo 4: líder na linha, chefe, canhão 12/12, 299–300 em voo (100 elites, 16–54 morrendo) | `Tools/rt2-gate.sh rt6t13-assault-w4 60 20 /c/temp/rt6t13/Claybreak-measure.apk 4` | 2,5 ms | 4,4 ms | 16 ms (62,5 fps) | 0,0% | 296 MB | 0 B | **passa** |
| antes: 6t.12 (corrida / assalto) | — | 1,8 / 2,3 ms | 4,4 / 4,4 ms | 16 ms | 0,0% | 305 / 313 MB | 0 B | passa |

Leitura: o pior p95 de CPU e GPU cai na janela 1 (carga inicial); nas janelas estáveis (2–5) a CPU fica em 1,7–2,0 ms e a
GPU em 2,9–3,1 ms, iguais à 6t.12. O líder (+3 chamadas) e o cenário dobrado não mudaram o custo: folga de mais de 4× em CPU
e de 2,2× em GPU sobre o limite de 11 ms, quadro sempre a 60 fps, nenhuma alocação em jogo.

Depois do gate: `adb shell am force-stop com.jungledegree.runnervstower` e `adb install -r /c/temp/rt6t13/Claybreak.apk`
(save preservado; instalado em 2026-11-22 20:41). Falta só o usuário jogar e aprovar o líder e a neblina.
