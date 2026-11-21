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

### Gate 3× no OPPO CPH2791

Critério: CPU principal p95 ≤ 11 ms, GPU p95 ≤ 11 ms, quadro p95 ≤ 16,7 ms a 60 fps, < 5% dos quadros > 50 ms,
PSS ≤ 450 MB; chamadas da multidão 1 + 1.

**Pendente:** o OPPO estava conectado mas **bloqueado** (`isKeyguardShowing=true`) durante toda a sessão; o gate
não foi medido. Passo exato para fechar: desbloquear o OPPO (cabo USB ligado, depuração USB ativa) e rodar, na raiz
do repositório, no Git Bash:

```
Tools/rt6t-toonlab-gate.sh /c/temp/rt6t1/Claybreak-measure.apk
```

(o APK de medição é refeito por `Tools/rt4-check.sh /c/temp/rt6t1 "apk"` se a pasta tiver sido limpa). O resultado
de cada rodada fica em `/c/temp/rt2/gate-toon*.txt` e entra na tabela abaixo; se 1000 não passar, aplica-se o
plano B da seção 1.2 de `direcao-visual-crowd.md` na ordem.

| Rodada | Unidades | MSAA | HDR | CPU p95 | GPU p95 | Quadro p95 | > 50 ms | PSS | Resultado |
|---|---|---|---|---|---|---|---|---|---|
| — | 1000 | 1× | desligado | | | | | | pendente |
| — | 1000 | 1× | ligado | | | | | | pendente |
| — | 1000 | 4× | desligado | | | | | | pendente |
| — | 1000 | 4× | ligado | | | | | | pendente |
| registro | 2000 | 4× | ligado | | | | | | pendente |

API gráfica e GPU do OPPO: saem na linha `[ToonLab]` do log de cada rodada (pendente).
