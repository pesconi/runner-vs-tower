# Runner vs Tower — reuso técnico do motor do Wild Pack

Data: 2026-11-19. Fonte: `SISTEMA.md` (2.1–2.6, 3, 4, 5.4, 7.35) e listagem/
tamanho dos arquivos de `Assets/Scripts`, lidos em 2026-11-19. **[F]** = lido no
repositório; **[I]** = inferência/proposta nossa. Nada aqui foi executado: não
há extração, refatoração nem código novo.

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

## 5. Extração do "kit do estúdio" (proposta, **não executar**)

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

### 5.3 Fatiamento proposto de assemblies

Sugestão de limites, a validar quando abrir o código **[I]**:

| Assembly | Conteúdo |
|---|---|
| `JungleDegree.Studio.Core` | Pool genérico, save/preferências, `GameAudio`/`ThemeAudio`/`GameHaptics`, tema base (estruturas de dados comuns), utilidades de dispositivo (`FrameRateGovernor`, `DevFrameTimingProbe`) |
| `JungleDegree.Studio.Crowd` | `RunnerCrowd`, `RunnerUnit`, spatial hash, skins/visual/animação |
| `JungleDegree.Studio.Gates` | portões, percurso, obstáculos, moedas |
| `JungleDegree.Studio.UI` | `ClayUi` e componentes de UI |
| `JungleDegree.Studio.Ads` | `RewardedAdService`, UMP (+ intersticial depois) |
| `JungleDegree.Studio.Editor` | `ThemeKit`, validadores comuns, scripts de build/medição |
| `<Jogo>.Game` | `GameManager` do jogo e seu contrato de tema |

O ponto difícil é o **`GameManager`** (1.660 linhas, 1 classe): precisa ser
dividido em um núcleo de estados + extensões por jogo. **Estimativa de esforço
da extração: ver `plano-de-entregas.md` (RT-1)**; é inferência, sem prova.

### 5.4 O que NÃO mexer

Sem alterar o Wild Pack durante a publicação: a extração só começa depois que o
Wild Pack estiver estável na Play, ou em **cópia de trabalho** com o mesmo
histórico. Application ID, chaves de save e IDs do Wild Pack ficam intocados
[F: SISTEMA.md 2.1].

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
