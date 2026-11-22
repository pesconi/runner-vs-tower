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
| RT-6t.1–6t.12 Direção visual "crowd toon" (multidão instanciada, toon, paletas, cenário, portões) — concluída em 2026-11-22 (motor no `studiokit` 0.5.0; aceite pendente: jogar os 4 mundos; `rt6t-crowd-toon.md`) | ~2,5 [I] | ~3 | antes da RT-9 |
| RT-6t.13 Ajustes pós-teste do usuário: neblina quase zero e líder da multidão — concluída em 2026-11-22; ajuste "sem neblina nenhuma" pedido pelo usuário feito, validado, gate 3× aprovado e APK instalado no OPPO; **aceite do usuário em 2026-11-22** ("Aprovo") | — | — | antes da RT-9 |
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

## RT-6t — Direção visual "crowd toon" (série RT-6t.1 a RT-6t.12)

- **Status: aprovada em 2026-11-21** (plano proposto e aprovado pelo usuário no mesmo dia; D25, D27 e D26 sem
  resposta: valem os padrões "se não responder" das fichas). Mapeamento técnico, o que muda no que existe, riscos, fichas D25–D29 e decisões técnicas
  T-RT6t-1..9: [`direcao-visual-crowd.md`](direcao-visual-crowd.md) — escopo, arquivos e aceite completos de
  cada entrega estão na seção 4.1 de lá; aqui ficam o cabeçalho e o prompt. Referência visual obrigatória:
  [`reference/crowd-proto.html`](reference/crowd-proto.html). Substitui a direção v2 (D24, RT-6v.1), revertida
  em `fc1620e`.
- **Regras da série:** o motor é escrito com namespaces/asmdefs do kit em `Assets/StudioKitIncubator/` (sem citar
  o jogo; validador de isolamento também sobre essa pasta) e só a RT-6t.12 o leva ao `studiokit` (K.11, pacote
  0.5.0, aceitação do Wild Pack uma vez); nada copiado do Wild Pack; regra 2.2.1 (nada de `Instantiate`/`Destroy`
  em jogo); gate 3× uma vez por entrega, no fim, no caso mais pesado; validação padrão = `Tools/rt4-check.sh`.
- **Ordem:** sequencial (regra global: uma sessão escrevendo por repositório): 6t.1 (portão) → 6t.2 → 6t.3 → 6t.4
  → 6t.5 → 6t.6a → 6t.7 → 6t.8 → 6t.9 → 6t.10 → 6t.11 → 6t.12. Só com autorização do usuário para duas sessões
  aqui: 6t.2 ∥ 6t.3, 6t.8 ∥ 6t.9, 6t.10 ∥ 6t.11 (arquivos separados; quem commita por último regenera a cena
  depois do `git pull --ff-only`).
- **Decisões do usuário:** D25 (público/"fofura"; trava a 6t.2), D27 (teto da multidão; sem resposta, 300), D26
  (skins × formas; trava a 6t.6b, que entra quando ela for respondida), D28 (arte do assalto; sem resposta, A na 6t.10), D29 (nome; trava a RT-9).

## RT-6t.1 — Portão técnico: 1 personagem, toon, instancing, 1000 unidades no OPPO

- **Escopo:** incubadora com `JungleDegree.StudioKit.Toon` (shader `StudioKit/ToonLit`, rampa 3×1 point,
  `ToonLook`, `LowPolyMeshBuilder`) e `JungleDegree.StudioKit.Swarm` mínimo (Burst com quique + squash, desenho
  indireto 1 + 1, sombras-bolha, `DevFpsOverlay`); Bolinha provisória no teto de 500 tri; cena `ToonLab`; modo de
  medição no APK e no script de gate; URP móvel sem sombras.
- **Depende de:** aprovação do plano.
- **Aceite:** validação padrão; **no OPPO, 1000 unidades a 60 fps passando o gate 3×** (CPU principal p95 ≤ 11
  ms, GPU p95 ≤ 11 ms, quadro p95 ≤ 16,7 ms, < 5% > 50 ms, PSS ≤ 450 MB), MSAA 1×/4× e HDR medidos, 2000 só
  registro, chamadas 1 + 1, API e GPU registradas. **Portão:** se falhar, plano B (seção 1.2 de
  `direcao-visual-crowd.md`); se ainda falhar, ficha ao usuário com o maior número que passou. A série só continua
  se passar.
- **Exige o usuário:** OPPO conectado e desbloqueado.
- **Status: concluída em 2026-11-21 — gate 3× no OPPO aprovado.** 1000 unidades a 60 fps em todas as
  combinações (MSAA 1×/4×, HDR desligado/ligado): pior caso CPU p95 1,9 ms, GPU p95 5,5 ms, quadro p95 16 ms, 0%
  > 50 ms, PSS 204 MB; 2000 (registro) também dentro do gate (GPU 8,2 ms com MSAA 4×). Vulkan, Mali-G1-Ultra MC12.
  Chamadas 1 + 1. Sem plano B. Detalhes e captura: [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md). A série segue na
  6t.2 (D25 sem resposta: faz as duas variantes e para antes de escolher).
- **Prompt da sessão:** "Entrega RT-6t.1 do Runner vs Tower (portão técnico da direção crowd toon). Leia
  `docs/direcao-visual-crowd.md` (seções 1, 1.2, 4.1 e 6) e abra `docs/reference/crowd-proto.html`. Escreva o
  motor em `Assets/StudioKitIncubator/` com namespaces e asmdefs do kit (`JungleDegree.StudioKit.Toon` e
  `.Swarm`), sem referenciar o jogo, e aponte o validador de isolamento também para essa pasta; não escreva no
  repositório studiokit. No jogo: Bolinha provisória (≤ 500 tri), cena `ToonLab`, modo de medição
  `--ez toonlab true --ei units N` no APK e no script de gate. Nada de Instantiate/Destroy em jogo. Rode a
  validação padrão e o gate 3× no OPPO a 60 fps com 1000 (registro com 2000; MSAA 1×/4×; HDR ligado/desligado).
  Se falhar, aplique o plano B na ordem e meça de novo; se ainda falhar, escreva a ficha com o maior número que
  passou e pare. Registre decisões técnicas em `docs/decisoes.md`, o resultado em `docs/rt6t-crowd-toon.md` (novo)
  e o status aqui."

## RT-6t.2 — As duas formas (Bolinha e Pintinho)

- **Escopo:** receitas das duas formas (R = 0,22), máscara de tinta, variante sem olhos (D25), assets gerados,
  regra de orçamento no validador (200–500 tri), seletor na `ToonLab`, capturas lado a lado com o protótipo.
- **Depende de:** RT-6t.1; **D25** (sem resposta: faz as duas variantes e para antes de escolher).
- **Aceite:** validação padrão; orçamento de triângulos; capturas conferidas por revisor independente.
- **Exige o usuário:** D25.
- **Status: concluída em 2026-11-21.** Bolinha (484 tri) e Pintinho (492) pelo `unitGeo()` do protótipo, cada um com e
  sem olhos (412/420), máscara de tinta, assets em `Content/Generated/Units/`; regra do validador para as quatro;
  seletor na `ToonLab` (Bolinha/Pintinho, com/sem olhos; padrão provisório: com olhos). Capturas lado a lado com o
  protótipo aprovadas com observações pelo revisor independente (só a calibração de luz do `ToonLit` difere);
  validação completa verde. Detalhes em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões T-RT6t2-1..7 em
  `decisoes.md`. **D25 segue sem resposta e agora trava só a 6t.4 em diante** (qual variante vai para o jogo).
  Próxima: 6t.4.
- **Prompt da sessão:** "Entrega RT-6t.2 do Runner vs Tower: as duas formas da multidão. Leia
  `docs/direcao-visual-crowd.md` (itens 5 e 6 da seção 1, RT-6t.2 na 4.1, ficha D25 e a resposta dela em
  `docs/decisoes.md`) e a função `unitGeo` do protótipo. Use o `LowPolyMeshBuilder` da incubadora. Gere Bolinha e
  Pintinho (200–500 tri, máscara de tinta), a variante pedida pela D25, a regra do validador, o seletor na
  `ToonLab` e as capturas lado a lado em `docs/art/rt6t/`."

## RT-6t.3 — Formação, controle e câmera

- **Escopo:** `SunflowerFormation`, seguir `centroX + offset` com variação e clamp, `SwarmCameraRig` (afastamento
  `1+√n/22`, shake), arraste e painel de teste na `ToonLab`, enquadramento 9:16 por dado.
- **Depende de:** RT-6t.1.
- **Aceite:** testes EditMode das fórmulas (iguais ao protótipo); captura 9:16 com 10 → 1000.
- **Exige o usuário:** nada.
- **Status: concluída em 2026-11-21.** Formação girassol, seguimento com clamp na pista, arraste e câmera iguais ao
  protótipo (testes EditMode comparam quadro a quadro com o laço `frame()`); `ToonLab` com painel (+100, 1000!,
  Recomeçar, Tremer). Retrato: a pista inteira cabe em 9:16 e no OPPO (FOV 65° + recuo fixo resolvido no pior caso),
  a câmera continua se afastando de 10 → 1000 e o arraste não dá zoom; neblina do laboratório acompanha o recuo.
  Capturas 9:16 e OPPO de 10 → 1000 e validação completa em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões
  T-RT6t3-1..8 em `decisoes.md`. Sem gate (o aceite não pede). Próxima: 6t.2 (formas) e depois 6t.4.
- **Prompt da sessão:** "Entrega RT-6t.3 do Runner vs Tower: formação girassol, arraste e câmera. Leia
  `docs/direcao-visual-crowd.md` (itens 11–13, RT-6t.3) e o laço `frame()` do protótipo. Escreva na incubadora
  (`Swarm`) e na `ToonLab`; fórmulas idênticas ao protótipo, com testes; enquadramento de retrato por dado."

## RT-6t.4 — Fatia fina: a multidão nova na corrida do jogo

- **Escopo:** a corrida usa o `Swarm` no lugar de `RunnerCrowd`/`PlayerController`/`RunnerPool` (arraste,
  `GateMath`, caixotes, tropeço, continuar, início), câmera pelo rig, cor da skin equipada como tinta (sem mudar o
  save), Bolinha, validador "níveis × capacidade do `Swarm`", nível de gate com a multidão máxima na corrida
  (teto da **D27**; sem resposta, 300).
- **Depende de:** RT-6t.2, RT-6t.3.
- **Aceite:** validação padrão completa; PlayMode da corrida verde; gate 3× na corrida.
- **Exige o usuário:** OPPO conectado; D27 só se o teto subir.
- **Status: concluída em 2026-11-21; gate 3× na corrida passou em 2026-11-22** (mundo 4, 300: CPU p95 2,7 ms, GPU
  3,0 ms, quadro 16 ms, 0% > 50 ms, PSS 298 MB; a sonda de medição passou do `RunnerCrowd` para o `ClaybreakFlow`). A corrida usa o `Swarm`
  (`RunCrowd`): sem líder nem objetos por unidade, centro da formação cruza portões e caixotes, arraste do protótipo,
  câmera pelo rig, tinta = cor da skin (save igual), forma pelo tema (Bolinha com olhos, D25 aberta), teto 300 (D27);
  assalto igual (pool do kit, lançado do anel de fora). Validação completa verde, PlayMode da corrida verde, captura com
  GPU e gate em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões T-RT6t4-1..10 em `decisoes.md`. Próxima: 6t.5.
- **Prompt da sessão:** "Entrega RT-6t.4 do Runner vs Tower: a multidão instanciada entra na corrida. Leia
  `docs/direcao-visual-crowd.md` (seção 2 inteira, RT-6t.4) e a resposta da D27 em `docs/decisoes.md`. Troque
  `RunnerCrowd`/`PlayerController`/`RunnerPool` da corrida pelo `Swarm` sem mudar regras nem save e sem
  Instantiate/Destroy; mantenha o assalto como está (a 6t.7 cuida dele); gate 3× na corrida no fim."

## RT-6t.5 — Animações

- **Escopo:** inclinação, nascimento elástico, morte balística num anel fixo, sombra que encolhe, ordem de
  remoção; na `ToonLab` e na corrida.
- **Depende de:** RT-6t.4.
- **Aceite:** testes das fórmulas contra o protótipo; capturas/vídeo; **gate 3× com 1000 vivos + 512 morrendo**;
  captura do chão sob a multidão (sombras somadas).
- **Exige o usuário:** OPPO conectado.
- **Status: concluída e aprovada em 2026-11-22: gate 3× no OPPO com 1000 vivos + 512 morrendo passou** (CPU principal
  p95 1,9 ms, GPU p95 6,4 ms, quadro p95 16 ms, 0% > 50 ms, PSS 177 MB).
  Inclinação lateral, nascimento elástico, morte balística num anel fixo de 512, sombra que encolhe e remoção do anel
  de fora, no `Swarm` (Burst, 1 chamada para vivos + morrendo, sombras só dos vivos); `ToonLab` (baixar a contagem e
  "÷2" matam, modo de rotação `--ei dying 512` para o gate) e corrida (portão ruim = morte + tremor; recomeço sem pop).
  Testes EditMode contra o protótipo traduzido, validação completa verde, capturas e chão sob a multidão (R-T7c: as
  sombras quase não se somam) em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões T-RT6t5-1..10 em `decisoes.md`.
  Gate: `Tools/rt2-gate.sh toon1000-d512 60 20 /c/temp/rt6t5/Claybreak-measure.apk toonlab:1000:4:1:512`. Próxima: 6t.6.
- **Prompt da sessão:** "Entrega RT-6t.5 do Runner vs Tower: animações procedurais. Leia
  `docs/direcao-visual-crowd.md` (itens 7 e 8, RT-6t.5) e o protótipo. Burst na incubadora, com testes; ligue a
  morte no portão ruim da corrida; gate 3× no OPPO com 1000 vivos + 512 morrendo."

## RT-6t.6a — Paletas, céu e neblina

- **Origem:** a RT-6t.6 foi dividida pelo maestro em 2026-11-22 (decisão técnica de escopo, `decisoes.md`): a 6t.6a
  é a parte que não depende da **D26**; formas e skins ficaram na RT-6t.6b.
- **Escopo:** `ToonPalette` (ScriptableObject do kit, com cores de torre/defesa para a 6t.10), aplicador, céu
  degradê em espaço de tela, neblina; Pastel, Pôr do sol, Doce (valores exatos do usuário) e a 4ª provisória
  "Aurora"; `WorldLook` → paleta; a multidão usa a cor da paleta do mundo como tinta quando a skin equipada é a
  padrão (BARRO), sem mudar o save nem a loja; validador com aviso de contraste; capturas dos 4 mundos.
- **Depende de:** RT-6t.5.
- **Aceite:** validação padrão; testes da paleta (valores exatos, contraste, regra da cor da multidão); capturas
  dos 4 mundos. Sem gate no OPPO (o aceite não pede: troca de cor e um desenho de céu de tela cheia).
- **Exige o usuário:** nada.
- **Status: concluída em 2026-11-22.** `ToonPalette` + aplicador + céu degradê em espaço de tela + neblina 22–75 na
  incubadora; as 4 paletas (valores exatos; Aurora provisória) como conteúdo; `WorldLook` → paleta; BARRO veste a cor
  do mundo (save e loja iguais); validador confere os valores exatos e avisa contraste (2 avisos, valores mantidos).
  Validação completa verde, capturas dos 4 mundos e detalhes em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões
  T-RT6t6a-1..10 em `decisoes.md`. Próxima: 6t.7 (a 6t.6b espera a D26).
- **Prompt da sessão:** "Entrega RT-6t.6a do Runner vs Tower: paletas, céu e neblina. Leia
  `docs/direcao-visual-crowd.md` (itens 2–4 e 17, seções 1.1 e 5.1, RT-6t.6) e o protótipo. `ToonPalette` com
  cores de torre/defesa, aplicador, céu degradê, neblina; as 4 paletas; `WorldLook` → paleta; BARRO usa a cor do
  mundo (save e loja iguais); validador com aviso de contraste; capturas dos 4 mundos."

## RT-6t.6b — Formas e skins no menu, save v4

- **Origem:** o resto da RT-6t.6 original (divisão de 2026-11-22, ver RT-6t.6a).
- **Escopo:** formas e skins conforme **D26** (e **D25**, que decide a variante de forma): seletor de forma no menu,
  save v4 com o campo "forma" e migração v3 → v4 com valor padrão, fixture de save v4, cores e nomes novos das
  skins se a D26 pedir (opção A: 8 cores novas no estilo, mesmos preços e ordem), aviso de contraste das skins
  contra as 4 paletas e as cores dos portões; revisão de segurança do save.
- **Depende de:** RT-6t.6a; **D26** (e **D25**). Pode entrar a qualquer momento depois da 6t.6a, assim que a D26
  for respondida (nenhuma outra entrega da série depende dela).
- **Aceite:** validação padrão; teste de migração do save v3 → v4; capturas (menu com o seletor, as formas nos
  mundos); revisão de segurança (save).
- **Exige o usuário:** D26 (e D25).
- **Status: concluída em 2026-11-22.** Formas no catálogo (Bolinha `ball` e Pintinho `chick`, com olhos — D25 A),
  botão "SHAPE" na tela inicial (grátis, troca a multidão na hora), save v4 com `shape` e migração v1/v2/v3 → v4 para
  `ball` (fixture `v4-chick-bubblegum.json`); a corrida usa a forma escolhida (o gate, a mais pesada); 8 skins com cores e
  nomes novos (CORAL, MANGO, SKY, LILAC, INDIGO, LAGOON, BERRY, BUBBLEGUM), mesmos ids, ordem e preços; a grátis vira
  WORLD COLOR; aviso de contraste das skins no validador (nenhum aviso). Validação completa verde, capturas e detalhes em
  [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões T-RT6t6b-1..10 em `decisoes.md`. Próxima: 6t.7.
- **Prompt da sessão:** "Entrega RT-6t.6b do Runner vs Tower: formas e skins. Leia `docs/direcao-visual-crowd.md`
  (RT-6t.6, fichas D25 e D26) e as respostas da D25 e da D26 em `docs/decisoes.md`. A 6t.6a já fez as paletas e a
  regra 'BARRO = cor do mundo'. Seletor de forma no menu, save v4 com o campo 'forma', migração v3 → v4 e
  fixture; skins conforme a D26; revisão de segurança antes do commit."

## RT-6t.7 — Multidão nova no assalto

- **Escopo:** `AssaultField` e defesas leem os índices do `Swarm` (mesmas regras), elites por instância, mortes
  pelo anel, limite em voo e valor por boneco conforme **D27**, `AssaultCamera` como perfil do rig, tabela de
  economia regenerada se o teto mudar.
- **Depende de:** RT-6t.6a.
- **Aceite:** validação padrão; PlayMode verde; gate 3× no chefe do mundo 4 com o assalto mais pesado.
- **Exige o usuário:** OPPO conectado.
- **Status: concluída em 2026-11-22.** As unidades em voo são instanciadas (`LooseSwarmRenderer`: 1 chamada para corpos
  + 1 para sombras, na forma e na skin da multidão; elites com tamanho e cor por instância); quem cai no portão ÷ ou nas
  defesas morre pela morte balística (anel extraído do `Swarm`); regras, fases e economia iguais (D27 sem resposta: 300);
  a câmera do assalto virou dois planos de dado do rig (`SwarmCameraShot`); saíram o pool de 300 objetos do kit, o
  `UnitGrounding` e o respingo de massinha. Validação completa verde, **gate 3× no chefe do mundo 4 passou** (CPU 2,6 ms,
  GPU 4,3 ms, 62,5 fps, PSS 292 MB). Detalhes e capturas em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões
  T-RT6t7-1..10 em `decisoes.md`. Próxima: 6t.9.
- **Prompt da sessão:** "Entrega RT-6t.7 do Runner vs Tower: a multidão instanciada no assalto. Leia
  `docs/direcao-visual-crowd.md` (seção 2, RT-6t.7) e a resposta da D27. Adapte `AssaultField` e as defesas para os
  arrays do `Swarm` sem mudar regras; se o teto mudar, regenere fases e economia pelas ferramentas existentes;
  gate 3× no chefe do mundo 4."

## RT-6t.8 — Cenário

- **Escopo:** `InstancedProps` com reciclagem, pista listrada no shader, trilhos, árvores (2 tons + tronco) e
  pedras, chão, densidade por mundo; substitui o cenário Simple Lit dos 4 mundos.
- **Depende de:** RT-6t.6a.
- **Aceite:** validação padrão; ≤ 3 chamadas para o cenário lateral; capturas dos 4 mundos.
- **Exige o usuário:** nada.
- **Status: concluída em 2026-11-22.** Pista listrada procedural (`StudioKit/StripedTrack`), trilhos e chão numa malha,
  árvores (2 tons + tronco) e pedras instanciadas e recicladas por `InstancedProps` (janela que segue a câmera, sem
  objetos), cores da `ToonPalette` por slots do `ToonLit`, densidade por mundo; o cenário Simple Lit dos 4 mundos saiu.
  Cenário lateral em 3 chamadas (validador confere). Validação completa, capturas dos 4 mundos e detalhes em
  [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões T-RT6t8-1..11 em `decisoes.md`.
- **Prompt da sessão:** "Entrega RT-6t.8 do Runner vs Tower: cenário instanciado e pista listrada. Leia
  `docs/direcao-visual-crowd.md` (itens 14 e 15, RT-6t.8). Não toque multidão, portões nem HUD."

## RT-6t.9 — Portões e feedback

- **Escopo:** painéis semitransparentes, postes, rótulos TextMeshPro em Baloo 2 com contorno (corrida e assalto),
  número flutuante, pop do contador, shake no portão ruim, Baloo 2 no HUD e nas telas; fonte só do repositório
  oficial do Google Fonts, com `OFL.txt` e hash.
- **Depende de:** RT-6t.7.
- **Aceite:** validação padrão; glifos `+ − × ÷` e acentos; capturas; licença registrada.
- **Exige o usuário:** nada.
- **Status: concluída em 2026-11-22.** Portões como o protótipo (painéis de meia pista #5ad1a0 / #ff6b81 a 0,45, postes
  brancos no `ToonLit`, rótulos TextMeshPro em Baloo 2 ExtraBold com contorno verde-escuro / vinho, na corrida, nos
  caixotes e na fileira do assalto), sinais verdadeiros `+ − × ÷`, número flutuante em pool de 4 com o "bounce" do
  protótipo, pop do contador (1,18), shake já ligado; Baloo 2 em todas as telas e no HUD. Fonte baixada só do
  `github.com/google/fonts` (commit fixado), `OFL.txt` e hashes em `audio-licenses.md`; a Roboto saiu. Validação completa
  verde (EditMode 221/221, PlayMode 22/22); glifos e acentos conferidos em captura com GPU. Detalhes e capturas em
  [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões T-RT6t9-1..11 em `decisoes.md`. Próxima: 6t.10.
- **Prompt da sessão:** "Entrega RT-6t.9 do Runner vs Tower: portões, texto Baloo 2 e feedback. Leia
  `docs/direcao-visual-crowd.md` (itens 18–21, RT-6t.9) e o protótipo (portões, `floatText`, `.count.pop`). Baixe a
  Baloo 2 só de github.com/google/fonts (ofl/baloo2), guarde `OFL.txt` e o hash. Não toque cenário nem pista."

## RT-6t.10 — Arte do assalto

- **Escopo:** conforme **D28** (sem resposta: A — sombreado toon e cores de torre/defesa da paleta nas torres,
  defesas, chefes, projéteis, caixotes e efeitos, sem remodelar e sem rostos).
- **Depende de:** RT-6t.8.
- **Aceite:** validação padrão; capturas dos 4 chefes; aprovação visual do usuário como aceite pendente.
- **Exige o usuário:** D28; olhar as capturas.
- **Status: concluída em 2026-11-22; aceite visual aprovado pelo usuário em 2026-11-22 ("Aprovo", capturas dos 4
  chefes), o que confirma D28 = A.** D28 sem resposta: A. Torres, chefes, canhão e pedras,
  muralha, escudo, varredor, caixotes e efeitos no `ToonLit` (3 faixas) com as cores de torre/defesa da paleta do mundo
  ativo (globais escritas na troca de mundo; nada criado em jogo), sem remodelar e sem rostos; efeitos com emissão; sinais
  de jogo (vida, escudo, perigo, portões) com cor fixa; estruturas com 35% da neblina (a torre não some mais na cor do
  horizonte); cores de torre do Pastel escurecidas. Validação completa verde. Capturas dos 4 chefes em
  [`art/rt6t/chefes-oppo.png`](art/rt6t/chefes-oppo.png) e [`chefes-perto.png`](art/rt6t/chefes-perto.png); detalhes em
  [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões T-RT6t10-1..8 em `decisoes.md`. Próxima: 6t.11.
- **Prompt da sessão:** "Entrega RT-6t.10 do Runner vs Tower: arte do assalto no estilo toon. Leia
  `docs/direcao-visual-crowd.md` (RT-6t.10, ficha D28) e a resposta da D28. Aplique a opção escolhida; capturas
  dos 4 mundos."

## RT-6t.11 — Reserva para OpenGL ES

- **Escopo:** caminho `RenderMeshInstanced` em blocos (variante de shader própria, matrizes de sombra no mesmo job),
  escolha em tempo de execução por `SystemInfo`, teste no OPPO forçando OpenGL ES 3.
- **Depende de:** RT-6t.7.
- **Aceite:** validação padrão; as duas rotas desenham igual (captura); gate 3× em GLES no OPPO.
- **Exige o usuário:** OPPO conectado.
- **Status: concluída em 2026-11-22.** Rota de reserva `RenderMeshInstanced` em blocos de 256 (variante própria do
  `ToonLit` e da sombra, matrizes e tintas num job Burst, sombras dos mesmos blocos) para a corrida, o assalto e o
  cenário; escolha por `SystemInfo` com palavra-chave global; forçável no APK de medição (`--ez chunks true`). As duas
  rotas desenham igual (0 pixels diferentes, [`art/rt6t/rotas-oppo.png`](art/rt6t/rotas-oppo.png)). Gate 3× no OPPO em
  OpenGL ES, assalto do mundo 4: CPU p95 2,5 ms, GPU p95 6,9 ms, quadro 16 ms, 0% > 50 ms, PSS 309 MB — passa. Em GLES o
  Mali do OPPO não lê buffer no vertex shader (`vertexBuffers=0`): a reserva é necessária nesse modo. Validação completa
  verde. Detalhes em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões T-RT6t11-1..7 em `decisoes.md`. Próxima: 6t.12.
- **Prompt da sessão:** "Entrega RT-6t.11 do Runner vs Tower: caminho de desenho de reserva para OpenGL ES. Leia
  `docs/direcao-visual-crowd.md` (item 9, R-T2, RT-6t.11). Implemente na incubadora, escolha por `SystemInfo`, e
  meça o gate 3× no OPPO com GLES forçado."

## RT-6t.12 — Levar o motor ao kit e fechar a série

- **Escopo:** mover a incubadora para o pacote com os `.meta` (K.11, 0.5.0, dependências no `package.json`,
  registro no plano do studiokit); aceitação do studiokit; retirar do jogo o caminho antigo de massinha; capturas
  novas; nível do gate completo; gate 3× final; GDD seção 9, `reuso-tecnico.md` 5.3 e `rt6t-crowd-toon.md`.
- **Depende de:** RT-6t.9, RT-6t.10, RT-6t.11.
- **Aceite:** validação padrão completa; isolamento do kit PASS; Wild Pack igual à linha de base (validadores
  1–3, save 87/87, isolamento, anúncios offline 24/24, build Android); gate 3× aprovado; nenhum asset órfão.
- **Exige o usuário:** OPPO conectado; jogar os 4 mundos (aceite pendente).
- **Status: concluída em 2026-11-22, com aceite pendente (o usuário jogar os 4 mundos).** Motor toon e
  multidão no `studiokit` 0.5.0 (K.11, tag `v0.5.0`, GUIDs preservados, Burst/Collections no `package.json`);
  incubadora removida; tema sem o corpo de massinha e o respingo; nenhum asset órfão. Validação completa verde
  (EditMode 229/229), isolamento PASS com o kit no pacote, Wild Pack igual à linha de base (90/0, 117/0, isolamento,
  áudio 16, save 87/87, anúncios offline 25/25 — linha de base da 8.6g —, APK do Forest), `TestProject` 92/92.
  Capturas regravadas em `art/rt6t/`. Gate 3× final no OPPO (Vulkan) **passou**: corrida do mundo 4 CPU p95 1,8 ms,
  GPU 4,4 ms, PSS 305 MB; assalto do mundo 4 CPU 2,3 ms, GPU 4,4 ms, PSS 313 MB; 60 fps, 0% > 50 ms, 0 B alocados. Detalhes em
  [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md); decisões T-RT6t12-1..6 em `decisoes.md`. Fecha a série RT-6t; próxima:
  RT-9.
- **Prompt da sessão:** "Entrega RT-6t.12 do Runner vs Tower: levar o motor ao studiokit e fechar a série. Leia
  `docs/direcao-visual-crowd.md` (seções 2, 4.1 e 6), `docs/rt6t-crowd-toon.md` e
  `../studiokit/docs/consumo-runner-vs-tower.md`. Mova `Assets/StudioKitIncubator/**` para o pacote preservando os
  GUIDs, registre a K.11 no plano do studiokit, rode a aceitação do studiokit depois do Unity deste projeto,
  remova o que ficou sem uso no jogo (confira referências e GUIDs), regrave capturas e rode o gate 3× final."

## RT-6t.13 — Ajustes pós-teste: neblina quase zero e líder da multidão

- **Origem:** o usuário jogou no OPPO em 2026-11-22 e pediu dois ajustes (decisão registrada em `decisoes.md`,
  "RT-6t.13 — Neblina quase zero e líder da multidão — usuário"): neblina quase imperceptível e um líder visível que
  decide o portão (desenho aprovado "como proposto").
- **Escopo:** (1) neblina mínima nas 4 `ToonPalette` (dado) e uma nova proteção contra cenário "aparecendo do nada" no
  lugar da regra da 6t.8, com o validador ajustado; (2) líder da multidão: mesma forma do menu, ~1,6×, sempre na frente,
  coroa na cor de UI do mundo, controlado pelo dedo, multidão atrás dele; o portão (e o caixote) que ele atravessa é o que
  vale, com o portão escolhido aceso e o outro apagado; não conta no número e não morre; no assalto fica na linha de
  lançamento. Desenhado no pipeline instanciado, sem Instantiate/Destroy em jogo.
- **Depende de:** RT-6t.12.
- **Aceite:** validação padrão completa; capturas (líder passando num portão, os 4 mundos com a neblina nova) em
  `art/rt6t/`; gate 3× no OPPO na corrida e no assalto do mundo 4; APK do jogo instalado no OPPO preservando o save.
- **Exige o usuário:** jogar e aprovar o líder e o visual sem neblina — **aprovado pelo usuário em 2026-11-22** ("Aprovo").
- **Status: concluída em 2026-11-22, com aceite pendente.** Neblina 110–150 m nos 4 mundos; janela do cenário de
  180 m (densidade igual) e validador novo (`PropWrapGuard`: árvore que reaparece à vista a 155,8 m, depois da neblina);
  líder (`CrowdLeader`, instanciado, coroa na cor de UI) decide portões e caixotes, portão escolhido aceso e o outro
  apagado, parado na linha no assalto. Validação completa verde (EditMode 305/305, PlayMode 24 + capturas), revisão
  independente sem bloqueio (3 ajustes aplicados). Capturas `art/rt6t/lider-oppo.png` e `mundos-oppo.png`. Gate 3× no OPPO
  **aprovado** (corrida e assalto do mundo 4: CPU p95 ≤ 2,5 ms, GPU p95 ≤ 4,9 ms, 60 fps, 0 B alocados) e APK do jogo
  instalado com o save; números em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md), seção RT-6t.13. Depois de jogar, o usuário
  pediu **sem neblina nenhuma**: desligada pelo jogo nos 4 mundos, árvores entrando pelo plano de corte (janela 200 m),
  validação completa verde (seção RT-6t.13, "Ajuste"). Capturas antes/depois no OPPO em
  `art/rt6t/neblina-antes-depois-oppo.png`, gate 3× de novo aprovado (CPU p95 ≤ 2,8 ms, GPU p95 ≤ 4,9 ms) e APK instalado.
  **Aceite:** o usuário jogou e aprovou o líder e o visual sem neblina em 2026-11-22 ("Aprovo"). Decisões: usuário e T-RT6t13-1..11 em `decisoes.md`.
- **Prompt da sessão:** "Entrega RT-6t.13 do Runner vs Tower (ajustes pós-teste do usuário, 2026-11-22): neblina quase
  zero nas 4 paletas com nova proteção contra pop-in no validador, e o líder da multidão (forma do menu, 1,6×, coroa na
  cor de UI, decide portões e caixotes, fica na linha de lançamento no assalto), no pipeline instanciado. Capturas em
  `docs/art/rt6t/`, validação completa, gate 3× no mundo 4 (corrida e assalto) e APK instalado no OPPO."

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
- **Depende de:** RT-8, conta Play (da publicação do Wild Pack), decisões de
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
