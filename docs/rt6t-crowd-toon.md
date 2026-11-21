# RT-6t — Direção visual "crowd toon": resultado das entregas

Plano e decisões: [`direcao-visual-crowd.md`](direcao-visual-crowd.md) (aprovado em 2026-11-21). Referência visual:
[`reference/crowd-proto.html`](reference/crowd-proto.html). Decisões técnicas: `decisoes.md` (T-RT6t-1..9,
T-RT6t1-1..9, T-RT6t2-1..7, T-RT6t3-1..8 e T-RT6t4-1..10).

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
  `ArtDirectionCapture` é adaptado na 6t.12.
- Revisor independente (subagente fable, só leitura): regras, contagens e save equivalentes; apontou o FOV do assalto
  herdado do rig e um quadro com instâncias velhas quando o fluxo muda a contagem depois do `Swarm` agendar — **corrigidos**
  (`AssaultCamera` com FOV próprio 60° em transição; `ClaybreakFlow` com ordem −40, entre o `RunCrowd` e o `Swarm`) — e
  asserts que não podiam falhar (removidos).
- Observação para a 6t.7: parada na linha de tiro, a multidão de 300 tem ~5,8 m de profundidade e metade dela fica à
  frente da linha (antes os seguidores ficavam atrás do líder); só visual, o lançamento já sai de `max(z, linha)`.

### Gate 3× na corrida (OPPO CPH2791)

**PENDENTE** — o OPPO estava conectado mas bloqueado (`isKeyguardShowing=true`) em todas as conferências desta sessão.
Comando, com o aparelho desbloqueado, no APK de medição desta entrega:
`Tools/rt2-gate.sh run-w4 60 20 /c/temp/rt6t4/Claybreak-measure.apk run:4` (mundo 4, o mais pesado; critério da
T-RT6t4-8). Depois, `adb shell am force-stop com.jungledegree.runnervstower`.
