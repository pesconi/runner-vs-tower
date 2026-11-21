# Runner vs Tower — reuso técnico do motor do Wild Pack

Data: 2026-11-19. Fonte: `SISTEMA.md` (2.1–2.6, 3, 4, 5.4, 7.35) e listagem/
tamanho dos arquivos de `Assets/Scripts`, lidos em 2026-11-19. **[F]** = lido no
repositório; **[I]** = inferência/proposta nossa. Seções 1–5.2 são a análise de
2026-11-19, antes da extração (caminhos `Assets/Scripts/...` do Wild Pack daquela
data). **Atualização 2026-11-20 (K.6 do `studiokit`):** a extração foi feita pelo
plano K do [`studiokit`](https://github.com/pesconi/studiokit) (RT-1 em
[`plano-de-entregas.md`](plano-de-entregas.md)); o que de fato está no pacote e o
que ficou no Wild Pack está na **seção 5.3**, que substitui a proposta. Na tabela
da seção 2, portões, percurso, obstáculos, moedas, `GameTheme`, `MainMenuView`,
save e `ThemeKit` **não** estão no kit: reusá-los aqui exige primeiro levá-los ao
kit (seção 5.3), nunca copiá-los do `MultiplierRunner`.

## 1. Estado atual do código [F]

- `Assets/Scripts` tem **50 arquivos `.cs` em um único assembly** (não há
  nenhum `.asmdef` no projeto). Pastas: `Ads`, `Core`, `Crowd`, `Gates`,
  `Level`, `Player`, `Pooling`, `UI`.
- Maior arquivo: `Core/GameManager.cs` (1.660 linhas): estado, geração de
  percurso, requisito de vitória, save, anúncio e áudio passam por ele.
- Regra de isolamento: o motor não referencia jogos; `EngineIsolationValidation`
  falha se `Assets/Scripts` citar `forest|animal|carrot|bunny|species|zombie|
  graveyard|Assets/Games` [SISTEMA.md 2.2.8].
- Jogos = módulo de **dados** em `Assets/Games/<GameId>/` (tema + builder
  idempotente + cena), criado pelo `ThemeKit` [4.1–4.3]. O contrato é
  `GameTheme` (ScriptableObject).
- O gate de desempenho no OPPO (300 corredores): CPU principal p95 ≈ 5–10,5 ms
  contra o limite de 11 ms; GPU p95 ≤ 7,3 ms contra 11 ms; PSS 245–363 MB contra
  450 MB [5.4 e 7.35]. **A CPU principal já está perto do limite**.

## 2. O que se reaproveita como está

Tudo isto é independente do que a multidão faz depois da corrida [I]:

| Peça | Arquivos | Observação |
|---|---|---|
| Pool de 300 unidades | `Pooling/RunnerPool.cs`, `Crowd/RunnerUnit.cs` | Regra "sem Instantiate/Destroy em runtime". Capacidade 300 vira o teto do exército. |
| Multidão da corrida | `Crowd/RunnerCrowd.cs` (Seek + Separation, spatial hash) | Perfeito para o bloco de corrida. |
| Portões e percurso | `Gates/CrowdGate.cs`, `RandomGateCourse.cs`, `GateVisualView.cs` | Reuso da geração por alvo; parâmetros de menos pares. |
| Obstáculos/armadilhas/moedas | `Level/*` | Corrida igual à do Wild Pack. |
| Jogador e câmera | `Player/PlayerController.cs`, `CameraFollow.cs` | Câmera já enquadra bounds da multidão. |
| Visual de massinha e skins | `RunnerSkin`, `RunnerBodyFrames`, `RunnerRunMeshFactory`, `RunnerRunVisual`, pipeline Blender | Mesmo corpo, novas skins/unidades. |
| Efeitos de morte | `RunnerDeathEffects` | Respingo curto de massinha já pronto. |
| Tema por dados | `GameTheme`, `ThemeCharacterRoster`, `ThemeWorldProfile`, `ThemeInterfaceStyle` | Contrato estável; mundos novos = novos temas. |
| UI clay | `UI/ClayUi.cs`, `ClayPress`, `MobileHudLayout`, `MainMenuView` | Menus, HUD e loja. |
| Áudio e vibração | `Core/GameAudio.cs`, `ThemeAudio.cs`, `GameHaptics.cs` | Som como dado, 6 vozes de efeito e limite de intervalo por som. |
| Anúncio recompensado + UMP | `Ads/RewardedAdService.cs` | Reuso; consentimento já fluindo. |
| Save e preferências | `GameManager` (progresso), `PlayerPrefs` para som/vibração | Mantém o princípio "preferência fora do registro de progresso". |
| Ferramentas | `Assets/Editor/ThemeKit`, validadores, builds de release/medição, capturas, rascunhos de política | Principal ganho de tempo [I]. |
| Gate 3× e medição | scripts `Tools/v4-measure-run.sh` etc. | Mesmo método. |

## 3. O que se adapta

| Peça | Adaptação [I] |
|---|---|
| `GameManager` | Hoje assume "corrida termina na chegada com requisito de contagem". Precisa de um **novo estado** `Assault` entre corrida e resultado, e do resultado "vence pela torre". O arquivo é grande e central: **maior ponto de atrito**. Mexer sem extrair é arriscado. |
| `RandomGateCourse` | Percurso mais curto, sem exigir "70% do máximo" como requisito de chegada; reaproveitar o alvo de contagem como dificuldade do assalto. Reaproveitar também para as fileiras de portões do assalto. |
| `FinishLine` | Passa a ser a **linha de tiro** (gatilho que abre o assalto), não o fim. |
| `RunnerCrowd` | A multidão da corrida segue; no assalto cada unidade muda para modo "em voo" (ver 4). |
| `GameTheme` | Campos novos para torre, defesas, mundo do assalto; o contrato precisa de uma **extensão** (blocos opcionais, como `world`/`gates`/`deathEffect` hoje [3.2]). |
| UI | Barra de vida da torre, seletor de faixa, botão de lançar, tela de resultado com estrelas. |
| Save | Upgrades, coleção de unidades, mundo atual; seguir o formato versionado atual sem quebrar. |
| Validadores | Torre válida, defesas válidas, nenhum limite de pool estourado. |

## 4. O que é novo

1. **Modo "em voo" da unidade** (o mais importante) [I]: no assalto as unidades
   deixam de fazer Seek/Separation e passam a **mover-se em linha pela faixa**
   (sem colisão unidade-unidade). Objetivo: custo por unidade bem **menor** que
   o `RunnerCrowd`, porque a CPU principal é o gargalo medido [5.4]. Proposta:
   componente separado de movimento em lote (um `Update` único percorrendo
   arrays pré-alocados, zero GC), nunca 300 `Update` individuais.
2. **Torre**: vida, andares, defesas, telegrafia de ataque, queda. Um
   `ScriptableObject` por torre (dados) + um comportamento único.
3. **Projéteis**: pool pré-alocado (regra 2.2.1), dano em área por consulta
   simples (sem física global), limite de projéteis simultâneos por dados.
4. **Lançamento e mira**: entrada de faixa e "segurar para lançar" (L1) ou
   toque único (L2) (ver GDD 4.2).
5. **Fases e catálogo**: gerador/estrutura de fases de assalto (torre +
   defesas + fileiras de portões), dificuldade por perfil como hoje.
6. **Upgrades e unidades**: dados + UI + save.
7. **Anúncio intersticial** (se aprovado): hoje só existe recompensado [F]. SDK
   AdMob já presente; é uma nova integração, declaração e teste.
8. **IAP "remover anúncios"** (se aprovado): Play Billing não existe hoje [F:
   `docs/store/declaracoes.md`, sem compras no app].
9. **Analytics** (se aprovado): nada existe hoje [F]. Ver `riscos-e-decisoes.md`.
10. **Conteúdo**: arte de torres, defesas, unidades especiais e chefes; áudio
    de impacto/queda.

## 5. Extração do "kit do estúdio" (feita pelo plano K do `studiokit`; 5.1–5.2 são a proposta original)

### 5.1 Problema

Hoje o motor e o jogo Wild Pack vivem no mesmo projeto Unity e no mesmo
assembly. O segundo jogo (Runner vs Tower) é um **app diferente** com outro
`application ID`, e o `SISTEMA.md` proíbe "publicar vários apps quase
idênticos" até decisão contrária [F]. Reaproveitar o motor para um jogo
diferente exige que o motor seja uma **dependência**, não uma cópia.

### 5.2 Alternativas

| Alternativa | Como | Prós | Contras |
|---|---|---|---|
| **A. Pacote local compartilhado (recomendada [I])** | Pasta `Packages/com.jungledegree.studiokit/` com `.asmdef` por área; cada jogo é um projeto Unity que referencia o pacote (`file:` no manifest ou submódulo git) | Uma fonte da verdade; correções valem para os dois; imposição de dependência pelos asmdefs | Trabalho de extração (desemaranhar `GameManager`); risco de regressão no Wild Pack |
| **B. Fork do projeto** | Copiar o projeto atual para um novo repositório e evoluir à parte | Mais rápido no começo; zero risco ao Wild Pack | Dois motores divergentes; correções e ferramentas duplicadas; piora com o tempo |
| **C. Segundo jogo dentro do mesmo projeto** | Novo módulo em `Assets/Games/RunnerVsTower` com cena própria | Reuso máximo; sem mexer na estrutura | Mesmo repositório e mesmo assembly; builds com dois application IDs e dois conjuntos de cenas; o motor teria de saber de mecânica de torre (viola a isolamento 2.2.8 ou cria "motor gordo") |

**Recomendação [I]: A, executada em duas etapas** (RT-1 no plano de entregas):
(i) criar asmdefs **dentro do projeto atual** sem mover nada (risco baixo,
valida dependências); (ii) só então mover para o pacote e abrir o projeto novo.
B só se o usuário preferir velocidade ao custo de manutenção; C não, por quebrar
a regra do motor.

### 5.3 Fatiamento real: kit v0 (`studiokit` 0.1.0)

Fonte [F]: `studiokit` na tag `v0.1.0` (`com.jungledegree.studiokit/README.md`,
`docs/arquitetura.md` seção 2, `docs/decisoes.md` T10–T18) e registro 7.41 do
`SISTEMA.md` do Wild Pack (commit `87834b5`, K.5). A regra da decisão K1 foi:
**o kit v0 contém só o que já era comum**; o `GameManager` não foi dividido.

**No pacote** (`com.jungledegree.studiokit`, um assembly por pasta, namespaces
`JungleDegree.StudioKit.<Área>`):

| Assembly / pasta | Conteúdo |
|---|---|
| `JungleDegree.StudioKit.Core` (`Runtime/Core`) | fase da partida `RunPhase` + classe base `RunPhaseSource` (o jogo herda e traduz o próprio estado; ordem `Menu, Playing, Lost, Won` é contrato com teste); `GameAudio`/`ThemeAudio` (motor de som, sem os clipes; desde a K.8/RT-8 com a tabela de sons extras nomeados pelo jogo); `GameHaptics` + `Vibration.androidlib` (permissão `VIBRATE` vem do pacote; vibração sem alocação desde a K.8); `FrameRateGovernor`; blocos de tema `ThemeWorldProfile`, `ThemeInterfaceStyle`, `ThemeColorRules`; `GeneratedEnvironmentAssets`; `RunnerTrackContactExtension` |
| `JungleDegree.StudioKit.Characters` | visual de massinha (`RunnerSkin`, `RunnerBodyFrames`, malha/animação/pose/inclinação/sombra), `RunnerReactions`, `RunnerDeathEffects`, `ThemeCharacterRoster`, `IRunnerLook` |
| `JungleDegree.StudioKit.Crowd` | `RunnerPool`, `RunnerUnit`, `RunnerCrowd`, `PlayerController`, `CameraFollow`, `DevFrameTimingProbe` |
| `JungleDegree.StudioKit.World` | `RunnerEnvironmentView` (cenário, céu, neblina e pista por dados) |
| `JungleDegree.StudioKit.UI` | `ClayUi`, `ClayPress`, `HudPulse`, `MobileHudLayout`, `SafeAreaFitter` (K.8) |
| `JungleDegree.StudioKit.Ads` | `RewardedAdService` (SDK `com.google.ads.mobile` 11.6.0, do OpenUPM) |
| `JungleDegree.StudioKit.Editor` | `EngineIsolationValidation` (lê a lista de termos do jogo), `StageOProfileReport` |
| `Shared/` | shaders `StudioKit/RunnerClay` e `StudioKit/RunnerGroundShadow`, materiais `RunnerClay`, `RunnerGroundShadow`, `RunnerSplat`, corpo `ClayRunner.fbx` + `ClayRunnerFrames.asset`, `ClayAccessories.fbx` |
| `Tests/Editor` | testes EditMode do kit (pool sem alocação, ordem das fases, `RunPhaseSource`) |
| `Tools~` | medição no aparelho (`stage7-measure.sh`, `v4-measure-run.sh`, exigem o application ID do jogo) e `compare-guids.py` |

**Ficou no Wild Pack** (`MultiplierRunner`, assemblies `JungleDegree.WildPack` e
`.Editor`):

| O quê | Por quê |
|---|---|
| `GameManager` (fluxo, `GameState`, save v5 e migrações, loja, `PlayerColorCatalog`) | K1: não dividir agora; regra de vitória e save são do Wild Pack |
| `GameTheme` (contrato agregado de tema) | junta blocos do kit com textos e regras do jogo; o kit recebe só os blocos (T7) |
| portões e percurso (`CrowdGate`, `RandomGateCourse`, `GateVisualView`) | K1: só viram kit depois do segundo uso (este jogo muda a regra) |
| nível (`CoinCollectible`, `FinishLine`, `FixedTrapCourse`, `RandomCoinCourse`, `RandomObstacleCourse`, `RunnerHazard`, `RunnerObstacle`) | dependem do `GameManager` e da geração de percurso |
| telas (`GameUIController`, `MainMenuView`, `GameResultView`, HUD de moedas/contagem, link de privacidade) | falam de moedas, nível e URL do Wild Pack |
| **áudio** (clipes em `Assets/Themes/Shared`) | conteúdo; o kit tem só o motor de som |
| **`ClayZombie.fbx` e `ClayZombieFrames.asset`** | corpo de um tema do Wild Pack; levá-los poria conteúdo de jogo no kit (T14) |
| **lista de isolamento** (`StudioKitIsolationTerms` do jogo) | os termos proibidos são de cada jogo; o kit só lê o arquivo (T12) |
| **projeto Android** (`Assets/GoogleMobileAds`, templates do Gradle em `Assets/Plugins/Android`, application ID, assinatura, Player Settings) | identidade e build são de cada app |
| **ferramentas de autoria** (`ClayRunnerAuthoring`, `ClaySkinAuthoring`, `LightweightVisualAuthoring`, `WorldVisualThemeAuthoring`, `Tools/blender/clay_runner.py` e `clay_accessories.py`) | continuam no Wild Pack, mas **gravam no pacote** (`StudioKitSharedPaths`): regenerar corpo, acessórios ou `RunnerSplat` muda o `studiokit`, e a mudança é commitada lá (T18) |
| `ThemeKit`, demais validadores e editores, builds de release/medição, cenas, temas, módulos `Assets/Games/*` | citam o jogo ou abrem cenas dele; candidatos ao kit só quando este jogo precisar do mesmo |

**Consequências para este jogo [I]:**
- Reuso **só pelo pacote** (`file:../../studiokit/com.jungledegree.studiokit`,
  ver [`studiokit/docs/consumo-runner-vs-tower.md`](../../studiokit/docs/consumo-runner-vs-tower.md)).
  Nunca copiar C# ou assets do `MultiplierRunner`: o que este jogo precisar de lá
  (portões, percurso, telas, ferramentas) vai primeiro ao kit, por entrega no
  `studiokit` com os `.meta` preservados, para servir aos dois jogos.
- Este jogo traz o próprio `GameManager`/fluxo (herdando `RunPhaseSource`), o
  próprio contrato de tema, áudio, lista de isolamento e projeto Android.
- As "regras da partida" (inimigo persistente, portão que dispara ação, condição
  de vitória, HUD/save, geração de percurso) **não** têm API no kit: cada uma é
  nomeada na primeira entrega deste jogo que tocar nela, comparando com o Wild
  Pack (pauta em
  [`studiokit/docs/arquitetura.md` seção 5.1](../../studiokit/docs/arquitetura.md#51-pauta-de-observação-por-entrega-do-runner-vs-tower)).

### 5.4 O que NÃO mexer

Application ID, chaves de save e IDs do Wild Pack ficam intocados [F:
SISTEMA.md 2.1]. Mudança no kit que afete o Wild Pack passa pelo aceite do
`studiokit` (validadores e save do Wild Pack; gate 3× quando tocar em desempenho).

## 6. Riscos de desempenho e o gate 3×

### 6.1 Fatos

- Gate: CPU principal p95 ≤ 11 ms, GPU p95 ≤ 11 ms, p95 ≤ 33,3 ms com 30 fps,
  < 5% de quadros > 50 ms, PSS ≤ 450 MB, 300 corredores; 400 só registro [F].
- A CPU principal é o gargalo (folga ~3,3×); a GPU sobra [F].
- Caveat registrado em 7.35: na corrida do Wild Pack **a multidão encolhe**
  (88–155 corredores na janela 3, 1–61 na janela 4), então o pior caso de 300
  só ocorre no início [F]. **Neste jogo o assalto é o momento de pico**, porque
  o jogador queria chegar com muita gente [I].
- O jogo não define `Application.targetFrameRate`: padrão do Android 30 fps; a
  medição em 7.35 usou 60 fps [F].

### 6.2 Riscos específicos [I]

| Risco | Por quê | Mitigação |
|---|---|---|
| **R-D1 Pico sustentado de 300 unidades no assalto** | A janela do Wild Pack nunca mediu 300 sustentado | Medir no protótipo (1ª semana) o assalto com 300 em voo; critério de aceite é o gate |
| **R-D2 Custo por unidade em voo** | 300 `Update` por quadro, ou física por unidade, estoura CPU | Movimento em lote, arrays pré-alocados, zero GC (já é padrão do motor), sem física por unidade |
| **R-D3 Projéteis + respingos + áudio** | Muitos eventos simultâneos | Limite de projéteis por dados; respingos em pool; limite de vozes (6) e intervalo mínimo por som já existentes [7.35]; vibração com intervalo mínimo |
| **R-D4 Interação unidade×torre/defesa** | Detecção de acerto por quadro | Acerto por distância no eixo da faixa, não por collider; sem `Physics` por quadro (regra 2.2.7) |
| **R-D5 Câmera** | `CameraFollow` enquadra bounds da multidão; no assalto o enquadramento muda | Perfil próprio de câmera do assalto, definido por dados |
| **R-D6 Margem da CPU** | Medido 7–10,5 ms contra 11 ms | Qualquer sistema novo entra com orçamento em ms **antes** de escrever; sem folga, cortar unidades máximas do assalto por dados |
| **R-D7 Memória** | Arte de torres/mundos por cena | Carregar um mundo por vez; PSS no gate |

### 6.3 Como o gate 3× se aplica

O gate vale como definido (seção 5.4), mais três regras **propostas**:
1. **Gate de protótipo:** medir no OPPO na semana 1, **antes** de investir em
   arte: 300 unidades em voo + 1 torre + projéteis no limite do dado.
2. **Gate de fatia vertical e de cada mundo novo:** com a torre mais pesada e a
   multidão no máximo.
3. **Gate de release:** igual ao 8.1 do Wild Pack, no AAB/APK de release com som
   e vibração ligados.
Se algum gate falhar, a resposta é **reduzir limites por dados** (unidades em
voo, projéteis, partículas), não relaxar o gate. Não há aparelho fraco de
referência; a folga de 3× é a proxy aprovada pelo usuário [F: decisão 2026-11-16].
