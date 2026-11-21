# Runner vs Tower — plano de entregas

Data: 2026-11-19. Formato igual ao da Fase 8 do Wild Pack (`SISTEMA.md` 7.27):
cada entrega tem **Escopo, Depende de, Aceite, Exige o usuário**; uma entrega
por sessão; commit `Stage RT.X: <resumo>` e registro no `SISTEMA.md` do jogo.
Regras herdadas: nada de segredos no repositório; não inventar declarações da
Play; nada é enviado ao Play Console/AdMob pela sessão; desempenho só aprovado
pelo **gate de folga 3×** (seção 5.4 do SISTEMA.md).

**Estimativas são [I]**: 1 desenvolvedor conduzindo Claude Code, semanas de
trabalho corrido, **com ~20% de margem somada**. Não há dado de velocidade
real do estúdio para este tipo de entrega; a única base é a estimativa M
(4–6 semanas) da pesquisa e o histórico de entregas de poucas horas a poucos
dias do Wild Pack. Revisar após o protótipo.

Roadmap pedido: protótipo fev/2027, produção mar–abr, soft launch mai,
lançamento jun/2027.

## Visão geral

| Entrega | Semanas base | Com ~20% | Janela [I] |
|---|---|---|---|
| ~~RT-0 Decisões e varredura de loja~~ — concluída em 2026-11-20 (D1–D6 em `decisoes.md`) | 1 | 1,2 | ~~dez/2026~~ |
| ~~RT-1 Extração do kit do estúdio~~ — feita pelo plano K do `studiokit` (nov/2026) | 3 | 3,6 | ~~jan/2027~~ |
| RT-2 Protótipo (1 semana) — concluída em 2026-11-20 (gate 3× aprovado no OPPO, usuário jogou e aprovou; `rt2-prototipo.md`) | 1 | 1,2 | início de fev |
| ~~RT-3 Teste do protótipo e portão de decisão~~ — dispensada em 2026-11-20 pelo usuário (`decisoes.md`, D13) | 0,5 | 0,6 | ~~meados de fev~~ |
| ~~RT-4 Fatia vertical~~ — concluída em 2026-11-20 (`rt4-fatia.md`) | 2 | 2,4 | ~~fev–início de mar~~ |
| ~~RT-5 Produção — núcleo de meta~~ — concluída em 2026-11-20 (gate 3× aprovado; fichas D12/D19 pendentes; `rt5-meta.md`) | 2 | 2,4 | ~~mar~~ |
| ~~RT-6 Produção — conteúdo (mundos, torres, chefes)~~ — concluída em 2026-11-20 (`rt6-conteudo.md`) | 2,5 | 3 | ~~mar–abr~~ |
| ~~RT-7 Produção — monetização, retenção e analytics~~ — concluída em 2026-11-20 (D21 = A, Firebase; `rt7-monetizacao.md`) | 1,5 | 1,8 | ~~abr~~ |
| ~~RT-8 Produção — polimento, áudio e gate de release~~ — concluída em 2026-11-21 (gate 3× aprovado no release; som aprovado, D22 = A; `rt8-producao.md`) | 1 | 1,2 | ~~fim de abr~~ |
| ~~RT-8b Firebase Analytics mínimo (D21 = A)~~ — feita em 2026-11-21 (`rt8b-firebase.md`; ver status) | — | — | — |
| RT-6v Refazer o visual (direção v2; **D24 = A, em etapas**) | 2,5 | 3 | antes da RT-9 |
| RT-9 Preparação do soft launch | 1 | 1,2 | início de mai |
| RT-10 Soft launch e ajuste | 4 | 4,8 | meados de mai–meados de jun |
| RT-11 Lançamento | 2 | 2,4 | meados–fim de jun |
| RT-12 Operação | contínuo | — | a partir de jul |

Soma até o lançamento (RT-2…RT-11): 17,5 semanas base, ~21 com a margem.
Calendário: o encaixe em jun/2027 é **justo**; ver "Folga e riscos de prazo".

---

## RT-0 — Decisões e varredura de loja

- **Escopo:** fechar as decisões de `riscos-e-decisoes.md` que bloqueiam o
  trabalho seguinte (nome, gancho, escopo "apps quase idênticos", analytics,
  contas). Varredura de 1–2 horas nas lojas (Google Play, App Store só para
  referência) com capturas: quem já faz "correr e depois empurrar", o que
  mostram nos primeiros 30 s, nomes e ícones parecidos. Saída:
  `docs/varredura.md` (fatos com data e URL).
- **Depende de:** nada técnico; ideal depois que o Wild Pack estiver em
  produção ou estável em teste.
- **Aceite:** decisões D1–D6 respondidas (ou decisão explícita de adiar); gancho
  confirmado ou trocado; lista de "jogos que o testador vai citar como
  parecidos".
- **Exige o usuário:** responder as decisões; aprovar nome de trabalho.
- **Status: concluída em 2026-11-20** (`Stage RT.0`). Aceite conferido:
  - D1–D6 respondidas em 2026-11-20 (registro em `docs/decisoes.md`; tabela da
    seção 2 de `docs/riscos-e-decisoes.md`): D1 nome de trabalho "Claybreak"
    (checagem de marca fica para antes de RT-4/RT-9), D2 gancho **A** (com traço
    de B só em chefes, conforme GDD 5), D3 A, D4 A, D5 B, D6 B. Também decididas
    D14 (só Android), D15 (B) e D16 (só o portão RT-3).
  - Gancho confirmado: **A**, reapresentado e decidido após a varredura.
  - Varredura em `docs/varredura.md` (2026-11-20, `Stage RT.0a`); lista de
    "jogos que o testador vai citar como parecidos" na seção 4.
  - Limite aceito: a varredura foi só por pesquisa web gratuita, **sem capturas,
    vídeos nem ícones**; nomes e ícones ficam para conferir na Play antes da arte
    (RT-4). O jogo ainda não tem `SISTEMA.md`; o registro desta entrega fica aqui
    e em `decisoes.md`.

## RT-1 — Extração do kit do estúdio

> **Feita pelo plano K do `studiokit` (2026-11-19 a 2026-11-20).** Por decisão K1
> ([`decisoes.md`](decisoes.md)) a extração foi executada no repositório
> [`studiokit`](https://github.com/pesconi/studiokit) pelas entregas K.0–K.6
> ([plano de entregas do kit](../../studiokit/docs/plano-de-entregas.md)), não aqui. Resultado:
> pacote `com.jungledegree.studiokit` `0.1.0`, tag `v0.1.0` no `studiokit`; o Wild Pack consome o
> pacote por `file:` desde o commit `87834b5` do `MultiplierRunner` (registro 7.41 do
> `SISTEMA.md` de lá: validadores iguais, save 87/87, AAB +0,01% contra o da tag
> `wildpack-internal-0.1.0-2`, gate 3× aprovado no OPPO). Diferenças em relação ao escopo
> original abaixo: o `GameManager` **não** foi dividido (K1: fica como módulo do Wild Pack; as
> "regras da partida" só serão nomeadas comparando os dois jogos — pauta em
> [`studiokit/docs/arquitetura.md` seção 5.1](../../studiokit/docs/arquitetura.md#51-pauta-de-observação-por-entrega-do-runner-vs-tower));
> portões, percurso e fluxo ficaram no Wild Pack. O que está no pacote e o que ficou no jogo:
> [`reuso-tecnico.md` seção 5.3](reuso-tecnico.md#53-fatiamento-real-kit-v0-studiokit-010).
> Como este projeto referencia o kit:
> [`studiokit/docs/consumo-runner-vs-tower.md`](../../studiokit/docs/consumo-runner-vs-tower.md)
> (feito na primeira entrega de código, RT-2). O texto original fica abaixo como histórico.

- **Escopo (original):** executar a proposta de `reuso-tecnico.md` seção 5, em duas etapas.
  (i) criar `.asmdef` dentro do projeto atual sem mover arquivos; compilar;
  validadores 1–4 iguais. (ii) mover para o pacote local, abrir o projeto
  novo e conferir que **o Wild Pack builda idêntico** (mesmos validadores, AAB
  com mesmo tamanho dentro de tolerância documentada, gate 3× no OPPO).
  Dividir `GameManager` em núcleo + extensões por jogo.
- **Depende de:** RT-0 (opção A ou B decidida) e **Wild Pack estável na Play**
  (não mexer no que está em publicação).
- **Aceite:** Wild Pack gera o mesmo APK/AAB funcional com o kit como
  dependência; `EngineIsolationValidation` e demais validadores sem falha nova;
  gate 3× do Wild Pack ainda aprovado; projeto novo vazio abre e compila.
- **Exige o usuário:** aprovar a estrutura (pacote local vs fork); OPPO
  conectado para o gate.
- **Risco:** é a entrega mais incerta (`GameManager` de 1.660 linhas). Se passar
  de 4,3 semanas (3,6 + 20%), usar a **alternativa B** (fork) e adiar a
  extração para depois do lançamento. [I]

## RT-2 — Protótipo (1 semana)

- **Escopo:** fase única jogável no aparelho. Corrida curta (3–5 pares) +
  assalto (L1 ou L2, ver GDD 4.2) contra 1 torre com vida, 2 tipos de defesa
  (canhão e muralha), uma fileira de portões no assalto, projéteis em pool.
  Placeholders de som já existentes do kit. Sem menu novo, sem upgrades, sem
  anúncios. Cena e tema em módulo de jogo novo.
- **Depende de:** RT-1 (ou cópia de trabalho), RT-0.
- **Aceite:** build no OPPO; fase jogável de 30–90 s; **medição do gate 3× com
  300 unidades em voo + projéteis no limite do dado** (se falhar, reduzir
  limites por dados e registrar o limite que passou); registro do que foi
  cortado.
- **Exige o usuário:** OPPO conectado; jogar e dar a primeira impressão.
- **Status: concluída em 2026-11-20** (`Stage RT.2` + correção `bb8f1ff`); gate 3× aprovado no OPPO a 60 fps (120 fps real a conferir na RT-4); usuário jogou, venceu na 2ª tentativa e aprovou seguir.
  Fase jogável em `Assets/Claybreak/` (L1, 4 pares de portões, torre, canhão, muralha, fileira
  de portões no assalto, 300 unidades e 32 projéteis pré-alocados), consumindo o kit 0.1.0.
  Validadores do jogo e do kit PASS, testes EditMode 24/24 e PlayMode 2/2 (o nível do gate
  segura 300 em voo e 12 pedras no ar no Editor), APKs gerados. Cortes, decisões e o passo a
  passo do gate 3× no OPPO: `docs/rt2-prototipo.md`.
  Correção pós-aparelho (`Stage RT.2 fix`): pista sem colisor derrubava a multidão; corrigida,
  com teste de regressão e conferida no OPPO (seção 7.1 do `rt2-prototipo.md`).

## RT-3 — Teste do protótipo e portão de decisão

- **Status: dispensada em 2026-11-20** pelo usuário (`decisoes.md`, D13): ele jogou o protótipo,
  aprovou e segue para a RT-4 sem teste externo. O texto abaixo fica como histórico.

- **Escopo:** teste com 5 testadores (sem analytics: cronômetro e contagem
  manual, como na pesquisa). Medir: (i) tentativas até vencer a 1ª fase; (ii)
  tempo da fase; (iii) **pedem "de novo" sem incentivo**; (iv) aceitam o anúncio
  recompensado simulado; (v) fps no gate; (vi) "com que jogo isso se parece?".
- **Critério do portão [pedido do usuário]:** **3 de 5 testadores pedem "de
  novo"**. Critérios de apoio: fps no gate aprovado; menos de 3 de 5 dizem
  "igual ao Wild Pack" ou "igual ao Mob Control" (descarte de gancho, ver GDD
  5).
- **Resultados possíveis:** (a) passa → RT-4; (b) o jogo agrada mas o gancho
  não → testar a Opção B no mesmo protótipo (+1 semana, só uma vez); (c) falha
  → **parar** e voltar à pesquisa (candidatos 2 e 3 da pesquisa); não insistir.
- **Depende de:** RT-2.
- **Aceite:** documento `teste-prototipo.md` com os 5 testadores (anonimizados),
  respostas, decisão registrada.
- **Exige o usuário:** recrutar 5 testadores; decisão final de seguir ou parar.

## RT-4 — Fatia vertical

- **Escopo:** um mundo completo e polido: 5–8 fases **[P]**, 2–3 torres, 3
  defesas, 1 chefe, menu, HUD, resultado, som e vibração, arte final de pelo
  menos 1 torre e 1 chefe, 1 upgrade e 1 unidade especial ponta-a-ponta, save.
  Objetivo: provar que o jogo se sustenta por uma sessão de 5 minutos.
- **Depende de:** RT-3 (passou) — RT-3 dispensada em 2026-11-20; depende só da RT-2 concluída.
- **Aceite:** fatia no OPPO; gate 3× aprovado com a torre mais pesada; o
  usuário e a Flávia jogam a fatia no OPPO e respondem "voltaria amanhã?" (sem
  critério numérico); lista do que falta para a produção. (Antes: "os mesmos 5
  + 2 novos" — trocado em 2026-11-20 porque a RT-3 foi dispensada, D13; registro
  em `decisoes.md`.)
- **Exige o usuário:** aprovar direção de arte da torre e do chefe; testar.
- **Status: concluída em 2026-11-20** — gate 3× aprovado no OPPO com a torre do chefe (`f04fd32`);
  D17 torre A + chefe A, D18 nome de trabalho "Clay Siege" (`Stage RT.4c`); usuário jogou e voltaria
  amanhã. Pendente para antes da RT-9: checar "Clay Siege" à mão no INPI, TMview e USPTO.
- Histórico: entregue em 2026-11-20 (`Stage RT.4a`/`RT.4b`; detalhes em
  `rt4-fatia.md`): 7 fases, 3 torres, 3 defesas, chefe, menu, HUD, resultado, som e vibração, upgrade,
  Bruto e save no jogo; validações em batchmode passaram. Falta: gate 3× no OPPO com a torre do chefe
  (aparelho desconectado na sessão) e o usuário e a Flávia jogarem ("voltaria amanhã?"). D17 (torre A,
  chefe A, arte final aplicada) e D18 (nome de trabalho "Clay Siege", nome exibido trocado) respondidas
  em 2026-11-20. Pendência "120 fps" da RT-2 analisada e fechada (`rt4-fatia.md` seção 3).

## RT-5 — Produção: núcleo de meta

- **Escopo:** upgrades, coleção de unidades (tipos e skins), loja, moeda,
  estrelas por fase, tela de mundos, save versionado; sem quebrar saves da
  fatia.
- **Depende de:** RT-4.
- **Aceite:** progressão local completa do mundo 1; validador de save; gate 3×
  com loadout máximo.
- **Exige o usuário:** aprovar preços e curvas dos upgrades (sem dado de
  mercado; ficam [P], calibrados no soft launch).
- **Status: concluída em 2026-11-20** (`Stage RT.5a`, `RT.5b`; `rt5-meta.md`): upgrades, elites com
  loadout, skins, loja, estrelas, tela de mundos, save v2 com migração do v1 e validador de save; gate 3×
  aprovado no OPPO com o loadout máximo. Pendentes do usuário: jogar a meta e responder as fichas D19
  (preços) e D12 (conteúdo, define a RT-6) em `decisoes.md`.

## RT-6 — Produção: conteúdo

- **Escopo:** mundos, torres, defesas e chefes restantes; geração de fases por
  dados; ajuste de dificuldade por perfil. Quantidade final: **decisão**
  (pesquisa cita ~30–50 fases por dados como ordem de grandeza [I] dela; a
  meta de D30 ≥ 5% exigiria ~500 níveis e eventos [F: Azur, 2026-04-28],
  fora de alcance no lançamento).
- **Depende de:** RT-5.
- **Aceite:** todos os mundos jogáveis do lançamento; `GameThemeCompleteness`-
  like sem falha; gate 3× uma vez, no mundo mais pesado (decisão do usuário, 2026-11-20; nesta entrega rodou nos 4); sem repetição visual óbvia.
- **Exige o usuário:** aprovar arte dos mundos; aprovar nº de fases.
- **Status: concluída em 2026-11-20** (`Stage RT.6a`–`RT.6d`; `rt6-conteudo.md`): 4 mundos × 10 fases
  jogáveis (D12 = A), geração de fases por perfil de dificuldade, escudo, um chefe com regra própria por
  mundo, visual próprio por mundo, voltas ajustadas ao poder do jogador (correção da D19), save v3 com
  migração, economia acertada (D19 A: +3 skins, loja de 19.380); validador sem falha e gate 3× aprovado no
  OPPO em cada mundo. D20 respondida em 2026-11-20: A (arte recomendada
  como provisória até o soft launch). Pendente do usuário: jogar os 4 mundos no OPPO.

## RT-7 — Produção: monetização, retenção e analytics

- **Escopo:** anúncio recompensado nos usos de GDD 7.2 (dobrar moeda, continuar
  uma vez, etc.); intersticial entre fases com limites (se aprovado); IAP
  "remover anúncios" via Play Billing (se aprovado); recompensa diária e missões
  leves (se o escopo for reaberto); **analytics mínimo, só o necessário para
  D1/D7/aceite de anúncio, se aprovado**. Atualizar política de privacidade,
  UMP e rascunhos de Data safety **só** do que realmente mudar, com evidência no
  código (como na 8.5).
- **Depende de:** RT-5, decisões de analytics/IAP/escopo, conta AdMob e Play.
- **Aceite:** fluxo de consentimento correto; build com ids de teste mostra
  recompensado/intersticial/compra de teste; `RewardedAdsValidation` e novo
  validador de monetização sem falha; política em `docs/` atualizada e
  coerente com o código.
- **Exige o usuário:** IDs do AdMob, produtos no Play Console, aprovar política
  e declarações, testadores de licença para a compra de teste.
- **Status: concluída em 2026-11-20** (`Stage RT.7`; `rt7-monetizacao.md`). D6 = B, D5 = B, D4 = A:
  premiado (dobrar moedas, continuar uma vez por fase), intersticial pronto e **desligado**, presente diário
  local, medição mínima atrás de uma porta sem SDK (ferramenta: **D21 = A, Firebase**, a instalar antes da RT-9), UMP antes de
  qualquer anúncio (portão comum no kit, K.7), só unidades de teste do Google. **A compra de teste não se
  aplica** (D6: sem IAP na v1). Aceite: validador do jogo e **validador de monetização** sem falha; EditMode 119/119; PlayMode 12/12 (6 novos com
  anúncio simulado: continuar, NO THANKS, sem anúncio sem oferta, dobrar moedas, intersticial, presente diário);
  isolamento do kit; teste offline de anúncios do Wild Pack 24/24 (o antigo `RewardedAdsValidation`), validadores
  1–3 do Wild Pack e save 87/87; revisão de segurança (3 achados corrigidos: toque duplo no continuar, intersticial
  sem reconferir consentimento, relógio adiantado travando o presente); APKs com anúncios de teste em
  `C:\temp\rt7` (36,1 MB, +0,03 MB sobre a RT-6). **Gate 3× (2026-11-20, mundo 4, chefe, 299 em voo, 12/12
  pedras):** quadro p95 16 ms, 0,0% > 50 ms, main p95 ≤ 5,4 ms, gpu p95 ≤ 4,7 ms, PSS 243 MB — **passou**
  (`C:\temp\rt2\gate-rt7-w4.txt`); APK normal instalado no OPPO. **Pendente do usuário:** ver os anúncios de teste.

## RT-8 — Produção: polimento, áudio e gate de release

- **Escopo:** áudio completo (impacto, queda da torre, aviso de projétil,
  chefe) com licenças registradas em `docs/audio-licenses.md`; vibração;
  correção de bugs; build de release (AAB/APK) e **gate 3× no build de
  release**, som e vibração ligados.
- **Depende de:** RT-6, RT-7.
- **Aceite:** gate aprovado nos mundos; zero coletas na corrida; PSS ≤ 450 MB;
  registro de tamanho do AAB.
- **Exige o usuário:** ouvir e aprovar o som; OPPO conectado.
- **Status: concluída em 2026-11-21** (`Stage RT.8`; `rt8-producao.md`). Áudio definitivo **próprio** (20 efeitos + 2
  músicas por síntese, sem terceiros; `audio-licenses.md`), com aviso de projétil, impacto da pedra, golpe e queda da
  torre e chefe; sons do jogo na tabela de sons extras do kit (K.8, pacote 0.3.0); vibração só nos momentos grandes e
  sem alocação; fonte Roboto Black (Apache 2.0); tela SETTINGS (som, música, vibração, link da política — endereço na
  RT-9 —, opções de privacidade); HUD e menus na área segura; teste do pool estável. Aceite: validador, isolamento,
  EditMode 124/124, PlayMode 13/13, Wild Pack igual à linha de base (90/0, 117/0, isolamento, áudio 16, save 87/87),
  revisão de segurança (5 achados corrigidos). **AAB de release 34,6 MB** (APK 34,8 MB), chave de debug. **Gate 3×
  no release (2026-11-21, chefe do mundo 4, som e vibração ligados): quadro p95 16 ms, 0,0% > 50 ms, CPU p95 ≤ 4,8 ms,
  GPU p95 ≤ 4,3 ms, 0 coletas, PSS 228 MB — passou**; APK normal instalado no OPPO. Som aprovado pelo usuário (**D22 = A**, 2026-11-21).
  **Pendente do usuário** para a RT-9: criar a chave de upload real fora do repositório (passo em
  `rt8-producao.md`).

## RT-8b — Firebase Analytics na configuração mínima (D21 = A)

- **Escopo:** SDK do Firebase Analytics (nativo, versão fixa), adaptador atrás da interface da RT-7, coleta só depois
  do consentimento (Europa/Reino Unido: "sim" do UMP), sem ID de publicidade nem sinais de anúncio, `game_session`;
  política e Data safety; tamanho do APK/AAB; gate 3× no chefe do mundo 4.
- **Exige o usuário:** criar o projeto Firebase e salvar o `google-services.json` fora do repositório (feito em
  2026-11-21); OPPO conectado.
- **Status: feita em 2026-11-21** (`Stage RT.8b`; `rt8b-firebase.md`; kit K.9, pacote 0.4.0). Validador e
  isolamento PASS, EditMode 139/139, PlayMode 13/13, Wild Pack igual à linha de base (90/0 com o mesmo aviso,
  117/0, isolamento, áudio 16, save 87/87, anúncios offline 24/24). Manifesto do APK: as 9 flags mínimas `false`.
  **APK de release 35,9 MB (+1,1 MB), AAB 35,6 MB (+1,0 MB)**, chave de debug. Empresa portuguesa (fato de
  2026-11-21): medição só com consentimento explícito em todos os países; fora da Europa/Reino Unido nada é medido até
  a ficha **D23**; política reescrita para o RGPD (pt-PT, CNPD, direitos). **Aceite no aparelho (2026-11-21, OPPO):** `rt8b-firebase-check.sh` — manifesto com coleta, AD_ID, SSAID e sinais desligados (PASS); sem o "sim" do UMP o Firebase fica desligado e **nenhum evento sai** (esperado: a mensagem RGPD do AdMob só é publicada na RT-9, então os nomes dos eventos no DebugView ficam para lá). Gate 3× no chefe do mundo 4 com Firebase: quadro p95 16 ms (max 33), 0,0% > 50 ms, main p95 ≤ 6,2 ms, gpu p95 ≤ 4,4 ms, 0 coletas, PSS 256 MB — **passou** (`gate-rt8b-w4-oppo-60.txt`); APK normal instalado.

## RT-6v — Refazer o visual (direção v2) — D24 = A, em etapas (2026-11-21)

- **Origem:** revisão visual de 2026-11-21 (`direcao-visual-v2.md`): o dono acha o visual atual ultrapassado
  (caixas, luz chapada, paleta escura). Protótipo no Unity e capturas antes/depois em `docs/art/direcao-v2/`.
  A direção é escolhida na **D24** (`decisoes.md`); recomendação: **A, Brinquedo Brilhante**.
- **Escopo (direção escolhida, os 4 mundos):**
  1. *studiokit (entrega do kit antes, com o aceite do Wild Pack):* `RunnerClay` com brilho especular, ambiente
     de duas cores e cor de borda como propriedades opcionais (padrão = o desenho de hoje); shaders genéricos de céu
     em degradê e de sombra de contato suave; ferramenta de editor de malhas arredondadas (a partir de
     `Assets/Claybreak/Editor/Prototypes/SoftMeshBuilder.cs`).
  2. *RT-6v.1, base e mundo 1:* soldado novo do jogo (meta ≤ 600 vértices; poses pelo caminho do kit; skins e elites
     nas 4 regiões de cor; sem rosto), pista em platô com meio-fio, portões de vidro com número maior, torre e chefe do mundo
     1, céu, neblina e cenário em três planos numa malha por mundo. **Termina com o dono vendo no celular** (HUD e
     multidão cheia) e com a regra da Play sobre "público misto" conferida; só então segue a RT-6v.2.
  3. *RT-6v.2, mundos 2–4:* paleta, cenário, torre e chefe de cada mundo na mesma linguagem (temas mantidos).
  4. *RT-6v.3, UI e fechamento:* cores do tema na UI (pílulas do kit), título com contorno, ícone de trabalho,
     regravar `docs/art/telas/` e `rt6/`, GDD seção 9 atualizado e gate 3×.
- **Fora do escopo:** regras de jogo, economia, fases, áudio, monetização (nada muda).
- **Depende de:** D24 respondida (A, B ou C; com D a entrega é cancelada); RT-8b.
- **Aceite:** validador, testes EditMode/PlayMode e isolamento sem falha; `ArtDirectionCapture` regrava as telas
  na direção escolhida; nada de `Instantiate`/`Destroy` em runtime (regra 2.2.1); **gate 3× uma vez, no fim, no
  chefe do mundo 4** (`Tools/rt2-gate.sh`): quadro p95 ≤ 33,3 ms e < 5% > 50 ms, CPU p95 ≤ 11 ms, **GPU p95 ≤ 11 ms**
  (a métrica que a direção mais mexe; estimativa A: 5–6,5 ms), PSS ≤ 450 MB, alocação 0. Reserva prevista:
  soldado com menos vértices acima de ~150 em voo, se a GPU passar de ~8 ms.
- **Exige o usuário:** responder a D24; aprovar as capturas dos 4 mundos no fim (sem ficha nova se seguir a direção).
- **Status:** proposta em 2026-11-21; não iniciada.

## RT-9 — Preparação do soft launch

- **Notas da RT-8b (2026-11-21):** a empresa é portuguesa — checagem de marca de "Clay Siege" em **EUIPO** e **INPI
  Portugal**, além do TMview; publicar a mensagem GDPR no AdMob (Privacidade e mensagens); restringir a chave de API
  do Firebase ao pacote e à SHA-1; preencher na política os dados da empresa (denominação, NIPC, morada, e-mail);
  rever o consentimento dos **anúncios** fora da Europa sob o RGPD; resposta da D23.

- **Escopo:** ficha da loja (título, textos, capturas 9:16, ícone 512, gráfico
  1024×500, direção adulta), classificação etária (IARC) com evidência do
  código, público 13+, Data safety, política de privacidade publicada no site
  do estúdio, teste interno e fechado exigidos pela Play (regra vigente para o
  tipo de conta; **não pesquisada ainda**, ver riscos), definição dos países de
  soft launch e do protocolo de medição.
- **Depende de:** RT-8, RT-6v (se a D24 não for "manter"), conta Play (da publicação do Wild Pack), decisões de
  países e orçamento.
- **Aceite:** AAB assinado enviado a teste interno; ficha sem avisos de
  política; documento de declarações como o do Wild Pack
  (`docs/store/declaracoes.md`).
- **Exige o usuário:** criar o app e as faixas no Console, enviar o AAB,
  preencher declarações, aprovar textos.

## RT-10 — Soft launch e ajuste

- **Escopo:** publicar em países escolhidos; coletar D1, D7 e aceite do
  recompensado; ajustar dificuldade, preço de upgrades e frequência de anúncio
  por dados (**ajustes só por dados do jogo, sem mudar código quando
  possível**); corrigir falhas e ANRs do Console.
- **Metas do pedido [P, do usuário]:** **D1 ≥ 35%** e **D7 ≥ 12%**.
  **Referência [F]:** Mob Control D1 36,1%, D7 13,9% (AppMagic via Udonis,
  2026-07-02). A meta é coerente com um líder, **ambiciosa** para um jogo novo
  e sem UA [I]; se não atingir, o resultado vale como dado, não como falha do
  plano.
- **Pré-requisito crítico:** sem analytics não se mede D1/D7. Se o usuário
  mantiver "sem analytics", D1/D7 só podem ser estimados por **instalações
  ativas do Play Console** (limitado) [I]; ver decisão D4.
- **Critério para lançar:** metas atingidas **ou** decisão explícita do usuário
  de lançar abaixo delas; nenhuma falha aberta que bloqueie; Console sem avisos.
- **Depende de:** RT-9.
- **Aceite:** relatório de soft launch (N de instalações, D1, D7, aceite do
  recompensado, falhas); lista de ajustes aplicados; decisão de lançar, ajustar
  mais ou parar.
- **Exige o usuário:** orçamento de aquisição, países, decisão final.

## RT-11 — Lançamento

- **Escopo:** versão final; lançamento gradual ou total (decisão); notas da
  versão; verificação pós-lançamento (instalação pela loja, ANRs, avaliações);
  material de divulgação **sem custo** (vídeo curto do jogo, site
  jungledegree.com).
- **Depende de:** RT-10 e aprovação explícita do usuário.
- **Aceite:** app no ar; instalado pela loja no OPPO com gate aprovado.
- **Exige o usuário:** apertar "publicar" no Console; qualquer gasto com UA.

## RT-12 — Operação

- **Escopo:** ciclos de **2 semanas** [I]: ler Console e métricas, corrigir, 1
  mundo/torre nova por ciclo se a retenção pedir; responder avaliações; atualizar
  política e declarações quando algo mudar. Sem live ops pesado enquanto não
  houver analytics e backend.
- **Depende de:** RT-11.
- **Aceite:** nenhum: contínuo; revisão a cada 3 ciclos para decidir manter,
  evoluir ou encerrar.
- **Exige o usuário:** decidir investimento de tempo e de UA.

---

## Folga e riscos de prazo

- **Datas absolutas [I]:** contando a partir de 1/fev/2027 com a margem de
  20% (acumulado em semanas): RT-2 até 1,2 (início/meados de fev); RT-3 até 1,8;
  RT-4 até 4,2 (início de mar); RT-5 até 6,6 (meados de mar); RT-6 até 9,6
  (início de abr); RT-7 até 11,4; RT-8 até 12,6 (fim de abr); RT-9 até 13,8
  (início de mai); RT-10 até 18,6 (meados de jun); RT-11 até 21 (**~27/jun**).
  A folga até 30/jun é de **poucos dias**: qualquer atraso empurra o
  lançamento para jul/2027. O soft launch começa em meados de mai, dentro do
  mês do roadmap, mas já sem sobra.
- **Mitigações:** cortar escopo em vez de prazo (menos mundos no lançamento;
  intersticial e passe depois); RT-1 com alternativa B (fork) se estourar;
  RT-3 pode encurtar o RT-10 se o protótipo já mostrar números fortes **[I]**.
- **Desconhecido que pode estourar o calendário:** a regra da Play para teste
  fechado de contas novas (a pesquisa não achou; o Wild Pack passa por isso no
  8.7 e o resultado informa o prazo deste jogo) [F: pesquisa, "Conta Play"].
- **Paralelismo:** RT-0 e RT-1 correm junto com a operação do Wild Pack; sem
  outro desenvolvedor, qualquer incidente do Wild Pack consome as semanas
  deste plano. A margem de 20% **não** cobre incidentes grandes.
