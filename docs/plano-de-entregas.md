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
| RT-6t.14 Limite configurável da multidão (300 neste jogo) e portões que nunca passam dele (D27 revista) — concluída em 2026-11-22; pendente instalar o APK no OPPO | — | — | antes da RT-9 |
| RT-6t.15 Multidão mais rápida na corrida (+30% constante, pedido do usuário em 2026-11-23) — concluída em 2026-11-23 (`RunPace`; janela de reação verificada; loja em 49 min); APK instalado no OPPO; **aceite do usuário em 2026-11-23** ("Ficou boa") | — | — | antes da RT-9 |
| RT-6t.16 Correções rápidas: tronco de espinhos que anda, mostrador de fase compacto, loja e home sem corte em telas estreitas, jogo sem o brilho branco — concluída em 2026-11-23 (capturas em 4 proporções em `docs/art/rt6t16/`); APK instalado no OPPO; **aceite do usuário em 2026-11-23** ("Ficou bom") | — | — | antes da RT-9 |
| K.14 (studiokit) Tirar o brilho branco dos botões do kit (`ClayUi.AddGloss`), versão 0.8.0, chamadas mortas do Wild Pack fora — **planejada**; depois da 6t.16, em paralelo com as seguintes | — | — | antes da RT-9 |
| RT-6t.17 Telas novas no estilo toon pastel (botões vivos, Baloo 2, loja legível de 16:9 a 21:9) — concluída em 2026-11-23 (capturas `telas-*.png` nas 4 proporções em `docs/art/rt6t17/`); gate 3× no OPPO em 2026-11-23 aprovado (mundo 4, chefe, 300 em voo: CPU p95 ≤ 5,5 ms, GPU p95 ≤ 4,3 ms, PSS 288 MB, 0 quadros perdidos); APK instalado no OPPO; **aceite do usuário em 2026-11-23** ("Ficou bom") | — | — | antes da RT-9 |
| RT-6t.18 Obstáculos na corrida (5 tipos procedurais que matam parte da multidão) — concluída em 2026-11-23 (motor, 5 tipos, fase de laboratório; capturas em `docs/art/rt6t18/`); APK instalado no OPPO; gate 3× no OPPO em 2026-11-23 aprovado (corrida do mundo 4, 6 obstáculos, 300: CPU p95 ≤ 2,0 ms, GPU p95 ≤ 4,4 ms, PSS 298 MB, 0 quadros perdidos); **aceite do usuário em 2026-11-23** ("Ficou bom") | — | — | antes da RT-9 |
| RT-6t.19 Obstáculos nas 40 fases e economia recalculada — concluída em 2026-11-23 (mundo 1 ensina um tipo por vez, um tipo novo por mundo, 2–3 por fase no fim do mundo 4; vida das torres sobre o exército depois das perdas; `rt6-economia.md` com a perda esperada; fases ±5% do tempo da RT-6t.15, loja 49 → 50 min); gate 3× não se aplica (no máximo 3 obstáculos por fase, 6 já medidos); APK instalado no OPPO; **aceite do usuário em 2026-11-23** ("Ficou boa") | — | — | antes da RT-9 |
| RT-6t.20 Formas compradas na loja (save v5) — concluída em 2026-11-23 (5 formas com silhueta e quique próprios, aba SHAPES, botão SHAPE fora da home, save v5 com migração; loja 24.680 moedas em 58 min; capturas em `docs/art/rt6t20/`); gate 3× não se aplica (forma mais pesada continua 492 triângulos); **aceite do usuário em 2026-11-23** ("Aprovo, instala"); APK instalado no OPPO, save migrado v4 → v5 com a forma BALL mantida | — | — | antes da RT-9 |
| RT-6t.21 Rename para Stampeep (D29) — concluída em 2026-11-23 (nome no ícone do celular, título da home e créditos; subtítulo "STORM THE TOWER" mantido; política de privacidade e licenças de áudio; validador recusa "Clay Siege"; ícone sem identidade antiga, não muda; busca oficial EUIPO e INPI sem marca igual, risco médio com "STAMPEDE" anotado; ficha de gasto D33 pendente, não bloqueia); gate 3× não se aplica; APK instalado no OPPO com o save mantido | — | — | antes da RT-9 |
| RT-9a Material da loja e declarações (IA) — concluída em 2026-11-24 (ficha en-US, 6 capturas reais 9:16, ícone 512 também no celular, destaque 1024×500, declarações com evidência, política final + HTML, regras da Play pesquisadas, passo a passo da RT-9b; fichas D7, D9, D10 = A em 2026-11-24; `docs/store/`) | — | — | antes da RT-9b |
| RT-6t.22 BIBOPZI ("Feijão") vira a forma padrão da multidão (decisão do usuário em 2026-11-25) — concluída em 2026-11-25 (receita de 490 triângulos com mola, engrenagem, barriga creme, mãos soltas e pés; BALL continua grátis na loja; save v6 com migração: quem estava na BALL padrão passa ao BIBOPZI, quem comprou outra forma a mantém; capturas em `docs/art/rt6t22/`); gate 3× não se aplica (a mais pesada continua 492); APK instalado no OPPO (save v5 → v6, Bibopzi vestido); **aceite do usuário em 2026-11-25** ("eu aprovo o personagem"; o visual low poly será trocado pela série RT-6v3) | — | — | antes da RT-10 |
| RT-9b Console, AdMob e teste interno (usuário, com passagens curtas da IA) — **em curso**: D7, D9 e D10 = A (2026-11-24); passo 1 feito em 2026-11-24 (política completa publicada pela IA a pedido do usuário em https://jungledegree.com/stampeep/privacy/, site `92cf4d9`); passo 2 feito em 2026-11-24 (chave de upload criada pelo usuário fora do repositório, em `~/keys`); passo 4.5 feito em 2026-11-24 (IDs do AdMob em `_privado/runner-vs-tower/admob-ids.json`, fora do repositório, D35 = A, lidos pelo build do jogo na RT-9b.1); antes do passo 5: passo 4b (Firebase com o pacote novo, D34 = B, RT-9b.1); passos 3, 4, 4b e 9 feitos em 2026-11-24 (app no Play Console; app e unidade premiada no AdMob, IDs em `_privado` pela D35; app Android `com.jungledegree.stampeep` no Firebase com `google-services.json` novo em `_privado`; mensagem RGPD publicada). APK com Firebase gerado e instalado: a configuração entra no APK, mas o formulário de consentimento não carrega ("Account not approved yet" do AdMob, app novo em revisão), então a medição fica desligada, como manda o desenho; conferir de novo com `Tools/rt8b-firebase-check.sh` quando o AdMob aprovar o app. Passos 5, 6 e 10 feitos em 2026-11-25: AAB `Stampeep-0.1.0-1` (versão 1, 0.1.0) gerado pelo usuário em 2026-11-24 e conferido pelo maestro (assinado pela chave de upload, pacote `com.jungledegree.stampeep`, IDs reais do AdMob e config do Firebase dentro; IDs reais só no AAB, nunca no repositório); enviado ao teste interno, disponível para testadores; SHA-1 (públicas) da chave de assinatura do Google `08:3D:CF:7C:E6:DD:01:9D:3B:85:E0:21:7E:25:64:C6:AC:BF:76:C8`, da chave de upload `C9:2C:6C:A8:07:50:C3:F2:FC:2E:67:A3:89:2C:BE:FC:49:03:1F:40` e da chave de depuração deste PC (APKs de teste do maestro) `DD:B5:9B:B3:C6:DC:70:61:A0:9A:5D:AE:D9:FD:77:3A:9E:72:6E:82`, as três na restrição Android da chave de API do Firebase (projeto `clay-siege`). Nota: o kit guarda como reserva a unidade premiada do Wild Pack (`RewardedAdService`, usada só sem `AdUnitSettings`); o Stampeep usa a sua; tirar do kit numa entrega do studiokit. Próximo: passos 7 e 8 (declarações e ficha da loja) e 11 (conferir instalado pela Play) | 1 | 1,2 | início de mai |
| RT-9b.1 Trocar o application ID para `com.jungledegree.stampeep` (D34 = B) — concluída em 2026-11-24 (fonte única + `Tools/app-id.sh`, validador recusa o ID antigo, build sem Firebase com aviso até o `google-services.json` novo, passo 4b do Firebase; junto, D35 = A: IDs do AdMob fora do repositório, passo 4.5 feito); instalado no OPPO em 2026-11-24 pelo maestro (app antigo desinstalado; abre como Stampeep, `com.jungledegree.stampeep`; Firebase desligado até o passo 4b; AdMob responde "Account not approved yet", esperado para app novo) | — | RT-9b | antes do passo 5 da RT-9b |
| RT-6v3.1 Guia de estilo como dado e validador (sem mudar a tela) — **concluída** (2026-11-25; `direcao-visual-v3.md`) | 0,75 | 0,9 | dez/2026 |
| RT-6v3.2 Imagem-alvo medida: 2–3 telas-conceito no Unity + ensaio no OPPO + ficha **D36** — **concluída** (2026-11-25; D36 com o usuário) | 1 | 1,2 | dez/2026 |
| K.v3a (studiokit) Shader irmão de vinil (`ToonLit` intacto; aceitação com match-3 e Wild Pack) — **concluída** (2026-11-26; `studiokit` 0.15.0, tag `v0.15.0`; luz escolhida contra matcap pela medida no OPPO) | 0,75 | 0,9 | dez/2026 |
| RT-6v3.3 Vinil, plástico fosco, luz, céu e pós por mundo + gate 3× — **concluída em 2026-11-26** (gate 3× no OPPO dentro do teto nas duas rotas) | 0,75 | 0,9 | dez/2026–jan/2027 |
| K.v3b (studiokit) Layout de instância opcional (rosto, fase do quique, UV/UV2, atlas) — **concluída** (2026-11-26; `studiokit` 0.16.0, tag `v0.16.0`; medido no OPPO nas duas rotas) | 0,75 | 0,9 | jan/2027 |
| K.v3c (studiokit) Construtor liso e chanfrado (torno, chanfro, normais suaves, UV) — **concluída** (2026-11-26; `studiokit` 0.17.0, tag `v0.17.0`; primitivas antigas com os mesmos bytes) | 0,5 | 0,6 | jan/2027 |
| RT-6v3.4a Bibopzi detalhado (3–5 mil tri) e de multidão (800–1.200) por Blender (dados simples) + gate 3× — **concluída em 2026-11-26** (detalhado 4.990 no líder, multidão 1.160, reserva 546; chave de corda e olhos maiores; gate 3× no OPPO aprovado nas duas rotas, sem reserva) | 1 | 1,2 | jan/2027 |
| RT-6v3.4b Rosto (8 expressões) e animação do Bibopzi; aceite agrupado 1 — **concluída em 2026-11-26, com gate 3× no OPPO aprovado nas duas rotas e aceite agrupado 1 pendente** (atlas 8 rostos no líder, multidão e loja; tabela evento → expressão no guia; amassar/esticar pelo kit 0.18.0; mola, mãos e chave pelo layout animado; pacote em `docs/aceite/agrupado-1/`) | 1 | 1,2 | fev/2027 |
| RT-6v3.4c As outras 5 formas da loja no padrão — **planejada** | 0,75 | 0,9 | fev/2027 |
| RT-6v3.5 Inimigos, torres e chefes (tema pela D36) + gate 3× — **planejada** | 1,5 | 1,8 | fev/2027 |
| RT-6v3.6 Portões e obstáculos + gate 3× — **planejada** | 0,75 | 0,9 | fev–mar/2027 |
| RT-6v3.7 Piso, cenário e céu + gate 3×; aceite agrupado 2 — **planejada** | 1 | 1,2 | mar/2027 |
| RT-6v3.8 Efeitos visuais + gate 3× — **planejada** | 0,75 | 0,9 | mar/2027 |
| RT-6v3.9 Som e música (síntese/CC0; IA só com D37); escuta do usuário — **concluída em 2026-11-26, com aceite pendente (escuta)**; em paralelo | 1 | 1,2 | dez/2026–mar/2027 (paralelo) |
| RT-6v3.10 Telas e ícone; aceite agrupado 3 — **planejada** | 0,75 | 0,9 | mar/2027 |
| RT-6v3.11 Gate final e capturas da loja — **planejada** | 0,75 | 0,9 | início de abr/2027 |
| RT-10 Soft launch e ajuste | 4 | 4,8 | meados de mai–meados de jun |
| RT-11 Lançamento | 2 | 2,4 | meados–fim de jun |
| RT-12 Operação | contínuo | — | a partir de jul |

Soma até o lançamento (RT-2…RT-11): 17,5 semanas base, ~21 com a margem.
Série RT-6v3 + K.v3 (2026-11-25): 13,75 semanas base, ~16,5 com a margem; caminho crítico ~15,3 (o som corre em
paralelo); com a espera da D36 e as festas, fim por volta de início de abr/2027. Lançamento de jun/2027 mantido **se** a
série terminar até meados de abr (`direcao-visual-v3.md`, seção 8).
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

## RT-6t.14 — Limite configurável da multidão e portões honestos

- **Origem:** D27 revista pelo usuário em 2026-11-22 (`decisoes.md`): o motor aceita um teto X de bonecos; neste jogo
  X = 300, na corrida e no ataque; as contas dos portões nunca passam disso.
- **Escopo:** (1) uma única fonte do limite no jogo, entregue ao motor como dado; (2) gerador e fases à mão sem portão que
  prometa acima do limite, mantendo dificuldade e tempo de fase (D19); (3) testes EditMode; (4) validação completa.
- **Depende de:** RT-6t.13.
- **Aceite:** validação padrão completa verde; nenhum portão das 40 fases acima de 300 (teste e coluna "Maior promessa" de
  `rt6-economia.md`); APK instalado no OPPO com o save. Gate 3× dispensado (só dados e contas; render e cena iguais).
- **Status: concluída em 2026-11-22.** `ArmyLimit.Units = 300` é a fonte única; o kit não mudou (já recebia a capacidade
  como dado). O gerador encaixa no limite só os portões que passavam (11 das 40 fases tinham algum; agora 0), com as mesmas
  defesas e layouts; a vida segue a mesma fração do potencial. Mundo 1: fase 5 e chefe ajustados à mão. Tempos da economia
  iguais. Detalhes em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md), seção RT-6t.14; decisões T-RT6t14-1..6.
  Validação completa verde (EditMode 331/331, PlayMode 24 + capturas, APKs e AAB). **Pendente:** instalar o APK no OPPO
  (o celular não estava conectado): com o OPPO no cabo e desbloqueado, `adb install -r /c/temp/rt6t14/Claybreak.apk`.
- Histórico: entregue em 2026-11-22 (`Stage RT.6t.14`).

## RT-6t.15 — Multidão mais rápida na corrida

- **Origem:** o usuário jogou a RT-6t.14 no OPPO e pediu em 2026-11-23: "A crowd está lenta, aumentar cerca de 30% de
  velocidade, ou fazer uma aceleração durante a fase." Escolha do maestro: +30% constante em todos os mundos.
- **Escopo:** (1) velocidade de avanço da corrida ~30% maior numa fonte única, com lateral, câmera, líder e portões
  coerentes; (2) janela de reação aos portões verificada (mundo 4 e chefes); (3) tempos da economia (D19) registrados, sem
  mudar preços; (4) testes e validação completa; (5) APK no OPPO.
- **Depende de:** RT-6t.14.
- **Aceite:** validação padrão completa verde; janela de reação dentro dos limites em todas as 40 fases; tempos novos em
  `rt6-economia.md`; APK instalado no OPPO com o save. Gate 3× dispensado (peso da cena igual; o gate já mede a 12 m/s).
- **Status: concluída em 2026-11-23.** `RunPace.Factor = 1,3` é a fonte única: as fases correm de 6,5 m/s (mundo 1, fase 1)
  a 9,75 m/s (fim do mundo 4), antes 5 a 7,5. Sem aceleração: a janela ficou folgada (número do portão legível 3,54 s antes
  de chegar e 1,85 s entre pares no pior caso; antes 4,6 s e 2,4 s), agora conferida pelo validador. Corrida média 15,2 →
  11,7 s; loja inteira 53 → 49 min (preços iguais). Detalhes em [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md), seção
  RT-6t.15; decisões T-RT6t15-1..5.
- Histórico: entregue em 2026-11-23 (`Stage RT.6t.15`).

## RT-6t.16 — Correções rápidas: tronco, mostrador, cortes e brilho no jogo

- **Origem:** pedido do usuário em 2026-11-23 (plano em [`plano-telas-obstaculos-loja.md`](plano-telas-obstaculos-loja.md),
  seções 2, 3.2, 3.4 e 4).
- **Escopo:** (1) o braço de espinhos do chefe anda (`AssaultField.Running` repassa ao braço), com teste; (2) mostrador de
  fase compacto; (3) Canvas casa pela largura e linhas, abas e botões de baixo esticam com a tela; (4) o jogo não chama
  mais `ClayUi.AddGloss`; (5) captura das telas em 4 proporções e teste que reprova botão fora da tela.
- **Depende de:** RT-6t.15.
- **Aceite:** validação padrão completa verde; capturas nas 4 proporções sem corte; braço em dois instantes; APK no OPPO.
  Gate 3× dispensado (na cena de jogo só o braço passa a se mover; T-RT6t16-7).
- **Status: concluída em 2026-11-23; aceite do usuário pendente.** O braço agora varre a frente da torre e a faixa que mata
  vai com ele (`SweeperMotionTests`; captura `chefe-braco-dois-instantes.png`). A home ganhou uma cápsula "LEVEL x/10" com
  60% da largura e pontos espaçados pela largura. O Canvas tem sempre 1080 de largura (16:9 igual a antes; no OPPO a
  interface fica ~20% menor que na 6t.15) e loja, abas e botões esticam: nenhum botão fora da tela em 16:9, 19,5:9, 20:9 e
  21:9 (`ScreenFitTests`, que também grava `telas-*.png`). Sem brilho branco nos botões (o validador proíbe `ClayGloss`).
  Testes: EditMode 336/336; PlayMode 40/40 sem falha (26 passam; 14 são capturas que só rodam com GPU, rodadas à parte: 3/3 e 2/2). Decisões T-RT6t16-1..7. A K.14 (kit) fica liberada.
- Histórico: entregue em 2026-11-23 (`Stage RT.6t.16`).

## RT-6t.17 — Telas novas no estilo toon pastel

- **Origem:** pedido do usuário em 2026-11-23 ("layout da home e menus extremamente primitivos, no shopping tudo cortado,
  botões com cores mais vivas e sem o retângulo branco"); plano em [`plano-telas-obstaculos-loja.md`](plano-telas-obstaculos-loja.md),
  seções 3.1 e 4.
- **Escopo:** home, loja (3 abas), mundos, ajustes, resultado (vitória e derrota) e continuar no estilo toon pastel:
  botões vivos das paletas sem brilho, texto TMP Baloo 2 com contorno, cartões claros, hierarquia da home, loja legível
  de 16:9 a 21:9, coroa no ponto do chefe do mostrador, caracteres novos no atlas estático. A tela de formas segue como
  hoje (a compra de formas é a RT-6t.20). Não existe tela de pausa no jogo (T-RT6t17-8).
- **Depende de:** RT-6t.16.
- **Aceite:** validação completa verde; capturas de todas as telas nas 4 proporções; teste de botões (e textos) dentro da
  tela; gate 3× uma vez no fim, no mundo 4; APK no OPPO; olhar do usuário.
- **Status: concluída em 2026-11-23; aceite pendente.** Botões laranja/menta/céu/lilás com placa escura e sem brilho;
  todo rótulo de menu em TextMeshPro Baloo 2 (branco com contorno tinta sobre botão, tinta sobre cartão creme); estrela e
  moeda como ícones dentro do texto (a Baloo 2 não tem "★"). Home: ajustes e moedas no alto, nome, cartão do mundo com
  estrelas, mostrador com coroa no chefe, e embaixo PLAY grande, SHOP e WORLDS, com forma e presente como selos pequenos.
  Loja: cartão com elites e bônus sobre a lista, linhas creme com preço e ícone de moeda, indisponível em cinza claro com
  o preço legível, possuído em menta, degradê no pé só quando a lista rola. `ScreenFitTests` também reprova glifo fora
  do atlas e legenda maior que o botão. Testes: EditMode 514/514; PlayMode 40/40 sem falha (26 passam, 14 capturas com
  GPU; `ScreenFitTests` com GPU 1/1). APKs e AAB gerados com o kit no último commit (`465881b`), porque o `studiokit`
  tinha código não commitado de outra sessão que não compilava (T-RT6t17-9). Gate 3× e instalação no OPPO pendentes: o
  celular não estava conectado ao PC. Decisões T-RT6t17-1..9.
- Histórico: entregue em 2026-11-23 (`Stage RT.6t.17`).

## RT-6t.18 — Obstáculos na corrida (motor e 5 tipos)

- **Origem:** pedido do usuário em 2026-11-23 ("Não tem desafio algum durante a corrida, tem que ter algo para atrapalhar e
  matar a crowd"); plano em [`plano-telas-obstaculos-loja.md`](plano-telas-obstaculos-loja.md), seções 3.2 e 4.
- **Escopo:** 5 tipos procedurais (tronco de espinhos que balança, serra, martelo, buraco, rolo de espinhos) com malha
  toon, mortes pela morte balística, 1 chamada de desenho por tipo nas duas rotas, pré-alocação; lista de obstáculos no
  dado de fase; janela de reação no validador; fase de laboratório com os 5 tipos; testes EditMode e PlayMode; capturas;
  gate 3× no mundo 4 com o máximo de obstáculos e a multidão em 300; APK no OPPO. A distribuição nas 40 fases e a
  economia são da RT-6t.19.
- **Depende de:** RT-6t.16.
- **Status: concluída em 2026-11-23; aceite pendente (gate 3× e olhar do usuário).** Cinco tipos procedurais, todos
  instanciados (1 chamada por tipo em buffer e em blocos, conferido com GPU): tronco de espinhos em pêndulo, serra que vai
  e volta girando num trilho, martelo que sobe e bate numa metade da pista, buraco, rolo de espinhos que rola em direção à
  multidão numa faixa. Matam pela posição dos bonecos (sem colisor), com a morte balística do kit e um "−n" vermelho; cada
  fase pode ter até 6 (`ClaybreakLevel.obstacles`). O validador recusa obstáculo visto a menos de 3 s ou a menos de 1,6 s
  de um par de portões (laboratório: 4,06 s e 1,97 s). Fase de laboratório (`ObstacleLab`) com os 5 tipos, aberta no jogo
  instalado com `--ez obstacleLab true`: repete sozinha, não salva, sem métricas nem anúncio. Fase de medição de cada
  mundo com 6 obstáculos e a multidão em 300. As 40 fases seguem sem obstáculo (RT-6t.19). Testes: EditMode 635/635;
  PlayMode 43 sem falha (28 passam, 15 capturas só com GPU; a de obstáculos rodada à parte com GPU: passou). Numa corrida
  em linha reta pelo laboratório cada tipo matou de 37 a 91 bonecos. APKs e AAB gerados com o kit no último commit
  (`8b511e8`; o `studiokit` tinha arquivos não commitados da K.17). Decisões T-RT6t18-1..9 (inclui a necessidade de um
  `KillAt(índice)` no kit, registrada para uma entrega K). Revisão independente: sem problema sério; o risco apontado
  (obstáculos sobrepostos matando a mesma vaga várias vezes) foi corrigido com uma marca por bit.
- **Pendente (exige o celular desbloqueado):** gate 3× da corrida do mundo 4 —
  `Tools/rt2-gate.sh run-w4-obst 60 20 /c/temp/rt6t18/final/Claybreak-measure.apk run:4`, depois
  `adb shell am force-stop com.jungledegree.stampeep` e `adb install -r <APK atual>` (pacote novo desde a RT-9b.1; os APKs da RT-6t.18 têm o antigo e precisam ser gerados de novo) (o gate
  instala o APK de medição). Limites: CPU p95 ≤ 11 ms, GPU p95 ≤ 11 ms, PSS ≤ 450 MB. Olhar do usuário: abrir o
  laboratório com `adb shell am start -n com.jungledegree.stampeep/com.unity3d.player.UnityPlayerGameActivity --ez obstacleLab true`
  e dizer se os obstáculos são legíveis e justos (para voltar ao jogo normal, fechar o app e abrir pelo ícone).
- Histórico: entregue em 2026-11-23 (`Stage RT.6t.18`).

## RT-6t.19 — Obstáculos nas 40 fases e economia

- **Origem:** plano [`plano-telas-obstaculos-loja.md`](plano-telas-obstaculos-loja.md), seções 3.2 e 4 (pedido do usuário
  de 2026-11-23: desafio na corrida).
- **Escopo:** distribuição e curva dos 5 obstáculos nas 40 fases (gerador e fases à mão do mundo 1 e chefes); vida das
  torres recalculada sobre o exército esperado depois das perdas; `rt6-economia.md` com a coluna de perda esperada; janela
  de reação verde em todas as fases; tempo de fase igual ao da RT-6t.15 (±5%); tempo da loja dentro de ±10% dos 49 min.
- **Depende de:** RT-6t.18.
- **Status: concluída em 2026-11-23; aceite pendente (APK no OPPO e o olhar do usuário).**
  - **Curva:** mundo 1 começa limpo (fases 1 e 2 sem obstáculo) e ensina um tipo por vez: tronco lento sozinho nas fases
    3 e 4, martelo lento sozinho na 5, os dois juntos da 6 ao chefe. Cada mundo seguinte apresenta um tipo novo, sozinho e
    lento na fase 2, e o repete logo na fase 3: serra (mundo 2), buraco (mundo 3), rolo (mundo 4). No fim do mundo 4, 3
    obstáculos por fase; chefes com combinação própria (mundo 1 tronco + martelo; 2 martelo + serra; 3 serra + buraco +
    tronco; 4 rolo + serra + tronco). Máximo em qualquer fase: 3 (o gate da 6t.18 mediu 6, então não houve gate novo).
  - **Encaixe e tempo:** o gerador põe cada obstáculo depois de um par de portões, a 1,65 s dele na velocidade da fase
    (a regra pede 1,6 s), e abre a pista só o necessário; se a fase passaria de 5% do tempo que tinha, os obstáculos ocupam
    o lugar de um par de portões (nunca abaixo de 3) ou o último fica de fora. As 40 fases ficaram dentro de ±5% do
    tempo da RT-6t.15 (maior diferença +4,5%; corrida média 12,1 → 13,3 s). Janela: obstáculo visto com pelo menos 3,54 s e a pelo menos
    1,65 s de um par de portões.
  - **Perda esperada e vida:** estimada com as regras do jogo (`ObstacleLoss`: a formação real passando pelo movimento
    real de cada tipo, média sobre o momento de chegada). Quem desvia (acerta o lugar em ±0,5 m e o momento em ±0,3 s)
    perde 1–14% no mundo 1 (média 8%) e 14–21% nas três últimas fases do mundo 4; quem corre reto pelo meio perde 19–41%
    no mundo 1 e até 91% no mundo 4. A vida das torres é calibrada sobre o exército de quem desvia, então vitórias e
    estrelas ficam como antes para quem joga atento. **Fica para o olhar do usuário:** para quem não desvia os obstáculos
    são bem mais letais que o "até ~2×" do plano [P] (o rolo e o buraco quase não custam a quem desvia e custam muito a quem
    não desvia).
  - **Economia:** tempo da loja 49 → **50 min** (+2%, dentro de ±10%); preços iguais (D19). Tabela `rt6-economia.md`
    regerada com as colunas Obstáculos e Perda esperada (desvia / reto).
  - Validação completa verde: cena, validador, isolamento, EditMode 657 + 1 pulado (do kit, como antes), PlayMode 28 + 15
    capturas que só rodam com GPU, economia, capturas, APKs e AAB (kit no último commit, árvore do `studiokit` limpa).
    Testes novos em `ObstacleCurveTests` (curva, janela, tempo ±5%, perdas, vida, encaixe, estimador). Decisões
    T-RT6t19-1..6.
- **Pendente (exige o celular desbloqueado):** `adb install -r C:\temp\rt6t19\Claybreak.apk` com o celular desbloqueado (mantém o save) e o usuário jogar os 4 mundos e dizer se a dificuldade está
  boa.
- Histórico: entregue em 2026-11-23 (`Stage RT.6t.19`).

## RT-6t.20 — Formas compradas na loja

- **Escopo e aceite:** [`plano-telas-obstaculos-loja.md`](plano-telas-obstaculos-loja.md), RT-6t.20 e seção 3.3; D31 A e
  D32 A em `decisoes.md`.
- **Feito:** 3 formas novas no construtor de malhas (`UnitShapeRecipes`): COELHINHO (orelhas longas, 488 triângulos),
  SAPINHO (achatado e largo, olhos saltados, 488) e PINGUIM (oval alto, barriga branca, nadadeiras, 476); o PINTINHO virou
  um ovo mais alto, com a crista maior (492, como antes), para não se confundir com a BOLINHA de longe. Cada forma tem o
  seu quique (coelho alto e lento, sapo achatando, pinguim baixo e rápido, pintinho rápido inclinado) na multidão, no líder
  e no assalto. Catálogo com preço (BALL 0, CHICK 300, BUNNY 1.000, FROGGY 1.600, PENGUIN 2.400). Aba SHAPES na loja
  (4 abas) com a figura de cada forma desenhada no editor a partir da malha, pintada com a cor do exército; comprar veste
  na hora. Botão SHAPE fora da tela inicial. Save v5 (`ownedShapes`), migração v1–v4 → v5 com backup do JSON antigo;
  quem usava o PINTINHO continua com ele sem pagar. Decisões técnicas T-RT6t20-1..8 em `decisoes.md`.
- **Validação (2026-11-23):** `rt4-check` completo verde — cena, validador (fixtures v4 BALL, CHICK, sem forma e editada
  à mão, v5), isolamento, EditMode 684 (683 passam, 1 ignorado), PlayMode 44 (28 passam, 16 capturas ignoradas sem GPU),
  economia, capturas, APK, APK de medição e AAB. Capturas com GPU 4/4 (`ShapeShopCaptureTests`, `ScreenFitTests`,
  `ShapeSkinCaptureTests`).
- **Revisão de segurança do save:** revisor só leitura (fable); sem achado grave; os 3 baixos e a nota corrigidos
  (T-RT6t20-8).
- **Economia:** loja inteira 24.680 moedas (+5.300 das formas), comprada em **58 min** de jogo simulado (antes 50; o plano
  estimava 55–56); preços ×0,75 → 51 min, ×1,5 → 70 min (`rt6-economia.md`). Preços decididos pelo usuário (D32 A).
- **Gate 3×:** não se aplica: a forma mais pesada à venda continua o PINTINHO com 492 triângulos (teste
  `NoShapeOnOfferPassesThePintinhosTriangles`); o gate segue vestindo a mais pesada.
- **Capturas (`docs/art/rt6t20/`):** `formas-corrida.png` (5 formas na câmera da corrida com 300, dois momentos do
  quique), `formas-assalto.png` (5 formas no assalto mais pesado, 300 em voo), `formas-mundos.png` (5 formas nos 4
  mundos), `loja-formas-4-proporcoes.png` (aba SHAPES em 16:9, 19,5:9, 20:9 e 21:9), `home-4-proporcoes.png` (home sem o
  botão SHAPE), `figuras-loja.png` (as figuras da loja de perto).
- **Para o kit (entrega K futura, registrada aqui):** `SwarmRenderer` e `LooseSwarmRenderer` precisam de um
  `SetMotion(SwarmMotionSettings)` público. Hoje o jogo troca o quique pelo campo serializado (`SwarmMotionBridge`, com
  teste que falha se o kit renomear o campo); com o setter, a ponte vira uma chamada.
- **Pendente (olhar humano antes do APK):** o usuário aprovar o visual das formas pelas capturas. Depois:
  `adb install -r C:\temp\rt620\Claybreak.apk` (mantém o save; CHICK ou BALL do usuário continua vestida).
- Histórico: entregue em 2026-11-23 (`Stage RT.6t.20`).

## RT-6t.21 — Rename para Stampeep (D29)

- **Escopo:** pedido do usuário em 2026-11-23 ("Rename Stampeep"), a partir da D29 (2026-11-22: "Stampeep" substitui
  "Clay Siege" antes da RT-9). Muda tudo que o jogador vê; o id `com.jungledegree.runnervstower`, o save
  (`Claybreak.Save`), os namespaces e a pasta `Assets/Claybreak` não mudam. Busca oficial de marca gratuita (EUIPO,
  INPI), sem comprar domínio nem registar marca.
- **Aceite:** nenhum "Clay Siege" visível ao jogador (ícone do celular, home, créditos, documentos públicos); validação
  completa verde; capturas da home nas 4 proporções e do ícone em `docs/art/rt6t21/`; APK instalado com `adb install -r`
  mantendo o save; resultado da busca de marca com data e links em `decisoes.md`; ficha de gasto pendente escrita.
- **Feito:** nome numa constante só (`ProjectConfiguration.GameName`) que alimenta o `productName`, o título da home e a
  linha de créditos; subtítulo "STORM THE TOWER" mantido (a manada invade a torre); `ClaybreakValidation.CheckGameName`
  recusa outro `productName` e "Clay Siege" na cena; lista de isolamento com `stampeep` e `clay ?siege`;
  `politica-de-privacidade.md` (PT e EN) e `audio-licenses.md` com o nome novo. Ícone: o jogo usa o ícone padrão do
  Unity, sem texto nem identidade "Clay Siege", então não muda (o ícone 512 da loja é da RT-9). Decisões técnicas
  T-RT6t21-1..5 em `decisoes.md`.
- **Busca de marca (2026-11-23):** EUIPO STAMPEEP 0 marcas; INPI fonética: nada acima de 57% (STAMP). Ponto de atenção:
  marcas "STAMPEDE" vivas em jogos (classes 9 e 41, ex. Sumo Group); revisor independente (fable) classificou como risco
  médio a anotar, não conflito real. `stampeep.com` livre. Detalhe e links em `decisoes.md`; ficha de gasto **D33** em
  `riscos-e-decisoes.md` (marca PT ou UE e domínio, com taxas oficiais; vale do SME Fund 2026 até 04/12/2026). Não bloqueia.
- **Validação (2026-11-23):** `rt4-check` completo verde — cena, validador, isolamento, EditMode 684 (683 passam, 1
  ignorado), PlayMode 44 (28 passam, 16 capturas ignoradas sem GPU), economia, capturas, APK, APK de medição e AAB.
  `ScreenFitTests` com GPU 1/1. `aapt2` confirma o nome "Stampeep" no APK.
- **Gate 3×:** não se aplica (nada muda no desenho em jogo).
- **Capturas (`docs/art/rt6t21/`):** `home-4-proporcoes.png` (16:9, 19,5:9, 20:9, 21:9), `ajustes-creditos-4-proporcoes.png`
  (créditos com STAMPEEP), `icone-atual-apk.png` (ícone tirado do APK: o padrão do Unity), `oppo-home.png` (home no OPPO
  depois da instalação, save mantido: mundo 1, nível 6, 457 moedas).
- **Instalado:** `adb install -r C:\temp\rt6t21\Claybreak.apk` no OPPO em 2026-11-23 (celular desbloqueado), save
  mantido.
- **Para o kit (entrega K futura):** a lista de isolamento do `TestProject` do studiokit deve receber `stampeep` e
  `clay ?siege` (esta entrega não mexe no kit).
- **Pendente do usuário (não bloqueia):** ficha D33 (proteger o nome). Firebase: o projeto e o app no console ainda se
  chamam "Clay Siege" (nome interno, não aparece ao jogador); trocar fica a seu critério na RT-9.
- Histórico: entregue em 2026-11-23 (`Stage RT.6t.21`).

## RT-6t.16 a RT-6t.20 e K.14 — Telas, obstáculos, formas na loja e mostrador (planejadas)

- **Origem:** o usuário jogou no OPPO e pediu em 2026-11-23 (palavras dele em
  [`plano-telas-obstaculos-loja.md`](plano-telas-obstaculos-loja.md), seção 0): telas primitivas e loja cortada, botões
  sem o brilho branco (tirar do studiokit), desafio na corrida e o tronco de espinhos do chefe parado, forma como compra
  na loja com impacto visual, mostrador de progresso ocupando a tela.
- **Plano completo** (diagnóstico com evidência, propostas, escopo, dependências, aceite e olhar humano de cada
  entrega): [`plano-telas-obstaculos-loja.md`](plano-telas-obstaculos-loja.md). Capturas do diagnóstico em
  `docs/art/rt6t16/`.
- **Entregas:** RT-6t.16 correções rápidas (tronco, mostrador, cortes, brilho no jogo) → RT-6t.17 telas novas →
  RT-6t.18 obstáculos → RT-6t.19 obstáculos nas 40 fases e economia → RT-6t.20 formas na loja. K.14 no `studiokit` (e
  Wild Pack) depois da 6t.16, em paralelo com as seguintes (repos diferentes; Unity em batchmode um de cada vez).
- **Fichas:** D31 A e D32 A decididas pelo usuário em 2026-11-23.
- **Status:** plano aprovado em 2026-11-23; RT-6t.16 a 6t.19 aceitas; RT-6t.20 concluída com aceite pendente (capturas);
  K.14 no `studiokit`.

## RT-9 — Preparação do soft launch (dividida em 2026-11-24: RT-9a e RT-9b)

- **Notas da RT-8b (2026-11-21):** a empresa é portuguesa — checagem de marca de "Clay Siege" em **EUIPO** e **INPI
  Portugal**, além do TMview; publicar a mensagem GDPR no AdMob (Privacidade e mensagens); restringir a chave de API
  do Firebase ao pacote e à SHA-1; preencher na política os dados da empresa (denominação, NIPC, morada, e-mail);
  rever o consentimento dos **anúncios** fora da Europa sob o RGPD; resposta da D23.

- **Divisão (pedido do usuário "Preparar a RT-9", 2026-11-24):** a RT-9a é tudo o que a IA faz sozinha (material,
  declarações, pesquisa, roteiro); a RT-9b é o que só o usuário faz no Play Console, no AdMob, no site e no Google
  Cloud, com passagens curtas da IA (IDs do AdMob, conferências no OPPO). Escopo original: ficha da loja, IARC,
  público 13+, Data safety, política publicada, teste interno, países e protocolo de medição.

## RT-9a — Material da loja e declarações

- **Escopo:** ficha da loja (título ≤ 30, curta ≤ 80, longa ≤ 4000) nos idiomas da D10 (em aberto: inglês e ficha D10);
  capturas 9:16 reais do jogo, ícone 512, gráfico 1024×500; `docs/store/declaracoes.md` no modelo do Wild Pack (Data
  safety com evidência, IARC, 13+, anúncios, acesso, ID de publicidade); política final + HTML estático com marcadores
  para os dados da empresa; consentimento dos anúncios fora da Europa (nota da RT-8b); regras vigentes da Play para
  conta de organização nova; passo a passo da RT-9b; fichas D7, D9, D10; AAB assinado sem segredo no repositório.
- **Depende de:** RT-6t.21.
- **Aceite:** limites da ficha conferidos; capturas conferidas (nada cortado, nenhum "Clay Siege", nenhuma marca de
  terceiros); `rt4-check` verde se mexer em código; fichas completas no registro.
- **Exige o usuário:** nada para terminar; as fichas D7/D9/D10 e a aprovação das imagens e textos vão para a RT-9b.
- **Status: concluída em 2026-11-24** (`Stage RT.9a`). Tudo em [`store/`](store/README.md):
  - **Ficha en-US** (`store/ficha/en-US.md`): título "Stampeep: Storm the Tower" (25), curta 77, longa 1.245; cada frase
    com fonte no código; `Tools/store/check_listing.py` (limites + palavras proibidas) PASS.
  - **Gráficos** (`store/graficos/`): 6 capturas 1080×1920 de fases reais (mundos 2, 3 e 4, loja de formas, home),
    tiradas por `StoreCaptureTests` (save de meio de jogo, PLAY, portões escolhidos como um jogador, dedo simulado no
    assalto) e escolhidas por `Tools/store/make_store_art.py`; ícone 512 (a multidão rosa com o líder e o portão ×2) que
    passa a ser **também o ícone do celular** (antes era o padrão do Unity, T-RT9a-4); destaque 1024×500 com o título na
    fonte do jogo.
  - **Declarações** (`store/declaracoes.md`): Segurança dos dados (4 tipos, recolhidos e partilhados pelo AdMob),
    IARC (violência de fantasia leve, sem sangue), público 13+ com o risco do visual fofo, anúncios sim, acesso livre,
    ID de publicidade sim, outras não.
  - **Política** final (`politica-de-privacidade.md`) com 10 marcadores (5 PT + 5 EN) para os dados da empresa, e
    página `store/site/stampeep/privacy/index.html` gerada do Markdown (`Tools/store/make_privacy_html.py --check`).
    Endereço `https://jungledegree.com/stampeep/privacy/` já no jogo (`MonetizationContent.PrivacyPolicyUrl`).
  - **Consentimento fora da Europa (T-RT9a-3):** mensagem RGPD do AdMob segmentada "Em todo o lado" + distribuição só
    no EEE/Reino Unido/Suíça enquanto valer a D23 = B; sem código novo. Conferir `IABTCF_gdprApplies` fora do EEE antes
    da RT-11.
  - **Regras da Play** (`store/requisitos-play.md`, fontes oficiais lidas em 2026-11-24): teste fechado de 12 pessoas por
    14 dias só para contas **pessoais** criadas depois de 13/11/2023 (a da empresa é de organização: não se aplica,
    confirmar no Console); teste interno até 100 testadores; targetSdk 36 (o jogo já é 36); 16 KB a partir de
    01/02/2027 (o AAB já está pronto, `Tools/store/check_16kb.py`); ≥ 3 capturas 9:16 ≥ 1080×1920 para jogos serem
    promovidos (temos 6). **Calendário:** sem os 14 dias de teste fechado; contar até 7 dias de revisão do público-alvo
    antes da produção (RT-10).
  - **AAB assinado:** o fluxo existente (`ClaybreakBuild`, variáveis `CLAYBREAK_*`) assina com a chave de upload sem
    gravar nada no repositório; `Tools/store/build-signed-aab.ps1` é para o usuário rodar (pede as senhas sem mostrar,
    imprime o certificado). Falta a chave de upload do Stampeep (passo 2 do roteiro).
  - **Passo a passo da RT-9b** (`store/passo-a-passo-console.md`): 11 passos curtos.
  - **Fichas D7, D9, D10** completas em `riscos-e-decisoes.md`, com a segunda opinião (fable).
  - **Validação (2026-11-24):** `rt4-check` completo — cena, validador, isolamento, EditMode 684 (683 passam, 1
    ignorado), economia, capturas, APK, APK de medição e AAB verdes; o PlayMode acusou 1 falha esperada (o teste da
    página de ajustes supunha a política sem endereço; agora confere o link https sem abrir o navegador) e, corrigido,
    passou: 49 (28 passam, 21 capturas ignoradas sem GPU), com APK e AAB refeitos. `aapt2`: nome "Stampeep",
    targetSdk 36, ícone adaptativo = o do jogo; `check_16kb.py` PASS no AAB. `StoreCaptureTests` com GPU: 12/12.
    Revisão de segurança independente (só leitura) antes do commit. Decisões técnicas T-RT9a-1..8 em `decisoes.md`.
  - **Observação:** no ícone adaptativo, as máscaras redondas cortam a borda do "×2"; a multidão e o líder ficam no
    centro. Se o usuário quiser o "×2" inteiro no celular, refazer o recorte (`ICON_CROP`) na RT-9b.
- Histórico: entregue em 2026-11-24 (`Stage RT.9a`).

## RT-9b — Console, AdMob e teste interno (usuário)

- **Escopo:** o roteiro de `store/passo-a-passo-console.md`: publicar a política no site (com os dados da empresa);
  criar a chave de upload; criar o app no Play Console (D7, D10); criar o app e a unidade premiada no AdMob e passar os
  IDs à IA (que liga os IDs de produção, valida e commita, conferindo que são da conta business); gerar o AAB
  assinado; enviar ao teste interno e instalar pela Play no OPPO; preencher as declarações copiando de
  `store/declaracoes.md`; ficha da loja e gráficos; mensagem RGPD no AdMob ("Em todo o lado"); restringir a chave de
  API do Firebase ao pacote e às SHA-1 (assinatura do app e upload); conferir consentimento, anúncio e eventos no OPPO.
- **Depende de:** RT-9a; fichas **D7** e **D10** (criar o app); **D9** só para preparar a produção/soft launch.
- **Aceite:** AAB assinado enviado ao teste interno e instalado pela Play no OPPO; Console sem avisos de política; link
  da política abrindo a página publicada; mensagem RGPD aparecendo; eventos no DebugView com o "sim".
- **Exige o usuário:** quase tudo (Console, AdMob, site, Google Cloud, chave de upload, aprovar textos e imagens).
- **Calendário:** sem teste fechado obrigatório (conta de organização; confirmar). Se o Console exigir, +14 dias com
  12 testadores.

## RT-9b.1 — Trocar o application ID para `com.jungledegree.stampeep` (D34 = B)

- **Escopo:** o pacote passa de `com.jungledegree.runnervstower` para `com.jungledegree.stampeep` antes do primeiro
  envio à Play (quando fica fixo para sempre). Fonte única `ProjectConfiguration.ApplicationId` (as ferramentas de
  aparelho leem por `Tools/app-id.sh`), `ProjectSettings`, validador que recusa o ID antigo, build que sai sem Firebase
  (com aviso) enquanto o `google-services.json` for do pacote antigo, passo novo do Firebase e restrição da chave no
  `store/passo-a-passo-console.md`, documentos vivos. Namespaces, `Assets/Claybreak` e a chave do save não mudam
  (T-RT4-6). Decisões T-RT9b1-1..4.
- **Depende de:** D34 = B.
- **Aceite:** `Tools/rt4-check.sh` completo verde; validador recusa o ID antigo; APK com o pacote novo instalado no
  OPPO abrindo como "Stampeep" (app antigo desinstalado, progresso de teste perdido, aceito pelo usuário).
- **Exige o usuário:** o passo 4b do roteiro (app Android novo no Firebase e `google-services.json` novo em
  `_privado`); a IA valida depois com `Tools/rt8b-firebase-check.sh`.
- **Status:** concluída em 2026-11-24, com aceite pendente no celular. `rt4-check` completo verde: cena, validador,
  isolamento, EditMode 695/696 (0 falhas), PlayMode 28/49 (0 falhas), economia, capturas, APK do jogo, APK de medição
  e AAB; as etapas cena, validador, EditMode, APKs e AAB foram repetidas depois da correção da restauração e da revisão de segurança (abaixo). O APK do jogo sai com o pacote
  `com.jungledegree.stampeep`, nome "Stampeep", o app AdMob da conta business no manifesto e **sem Firebase** (aviso
  claro no log: o `google-services.json` ainda é do pacote antigo). O APK de medição fica com o app de exemplo do
  Google. Os assets commitados não guardam nenhum ID real (o validador confere).
  Junto (D35 = A, pedido do maestro): IDs do AdMob lidos de `_privado\runner-vs-tower\admob-ids.json` só no build do
  jogo; passo 4.5 do roteiro feito. Na primeira rodada, a restauração dos assets falhou depois do build (o build
  descarrega o asset do plug-in) e deixou os IDs nos assets; o validador acusaria, os arquivos foram restaurados pelo
  git antes de qualquer commit e a restauração agora recarrega os assets pelo caminho (T-RT9b1-5).
- **Pendente (exige o OPPO no cabo e desbloqueado):** o celular não estava conectado (`adb devices` vazio). Comandos:
  `adb start-server`, `adb shell dumpsys window | grep -i isKeyguardShowing` (tem de dar `false`),
  `adb uninstall com.jungledegree.runnervstower`, `adb install /c/temp/rt9b1c/Claybreak.apk`,
  `adb shell monkey -p com.jungledegree.stampeep -c android.intent.category.LAUNCHER 1`, e conferir que abre e mostra
  "Stampeep". Depois do passo 4b do roteiro (Firebase), gerar o APK de novo e rodar `Tools/rt8b-firebase-check.sh`.

## RT-6t.22 — BIBOPZI como forma padrão da multidão

- **Escopo:** decisão do usuário em 2026-11-25 ("O jogo ta apenas em teste, pode substituir a padrão"): o Bibopzi na
  direção "Feijão" (repositório `personagem`, D3 de 2026-11-25) substitui a BOLINHA como unidade padrão; as outras formas
  continuam na loja. Ícone, capturas da loja e ficha da Play não mudam nesta entrega.
- **Feito:** receita `UnitShapeRecipes.Bibopzi` (490 triângulos; 458 sem olhos): corpo de peça única em feijão (base
  larga, cabeça mais estreita, ombro suave) que recebe a cor do mundo/skin; barriga creme #FFF1DC; olhos brancos com
  losango escuro e sobrancelhas; nariz de parafuso; engrenagem laranja #FF8A3D na têmpora direita dele (+X, à esquerda de
  quem olha de frente) virada para fora; topete de mola; duas mãos esféricas soltas; dois pés achatados laranja. Catálogo:
  BIBOPZI primeiro (grátis, padrão, quique próprio, coroa do líder no topo da cabeça), BALL continua grátis, preços das
  outras iguais (D32 A). Save v6: migração v1–v5 → v6 (`SaveMigration.DefaultToBibopzi`), fixtures `v5-ball` e
  `v6-ball-picked`. Decisões técnicas T-RT6t22-1..5 em `decisoes.md`.
- **Validação (2026-11-25):** `rt4-check` completo verde — cena, validador (fixtures v1–v6, incluindo `v5-ball` e
  `v6-ball-picked`), isolamento, EditMode 715 (714 passam, 1 ignorado), PlayMode 50 (28 passam, 22 capturas ignoradas sem
  GPU), economia (tabela igual: as formas grátis não mudam a loja), capturas, APK, APK de medição e AAB. Capturas com GPU
  4/4 (`BibopziCaptureTests`, `ShapeShopCaptureTests` ×2, `ScreenFitTests`).
- **No tamanho de jogo** (`bibopzi-tamanho-de-jogo.png`): a mola aparece como uma ponta laranja sobre cada cabeça e as
  mãos como pontos laranja dos lados (lê-se bem no líder); a engrenagem vira um ponto laranja, sem forma de engrenagem; a
  barriga não aparece na corrida, porque a câmera vê a multidão de costas (aparece na loja, no menu de frente e de perto).
- Histórico: entregue em 2026-11-25 (`Stage RT.6t.22`).
- **Gate 3×:** não se aplica: o BIBOPZI tem 490 triângulos, abaixo dos 492 do PINTINHO, que continua a forma mais pesada
  vestida pelo nível de gate (teste `NoShapeOnOfferPassesThePintinhosTriangles`).
- **Capturas (`docs/art/rt6t22/`):** `bibopzi-vs-referencia.png` (desenho de referência, coluna A, ao lado da frente, 3/4 e
  3/4 de costas no shader do jogo, verde-água #22B3A6), `bibopzi-perto.png` (as mesmas, mais a frente na cor do mundo 1),
  `bibopzi-corrida-300.png` (câmera da corrida com 300, cor do mundo 1 e verde-água, dois momentos),
  `bibopzi-tamanho-de-jogo.png` (recorte da multidão em tamanho real e ampliado 3×), `formas-corrida.png` e
  `formas-assalto.png` (as 6 formas lado a lado), `loja-formas-4-proporcoes.png` (aba SHAPES com 6 formas).
- **Aceite do usuário em 2026-11-25** ("eu aprovo o personagem"); APK instalado no OPPO pelo maestro no mesmo dia
  (save v5 → v6, Bibopzi vestido, `docs/art/rt6t22/oppo-home.png`). Pendência original, já resolvida: aprovar pelas capturas; instalar o APK no OPPO
  (`adb install -r C:\temp\bib\Claybreak.apk`, mantém o save: quem estava na BALL passa ao BIBOPZI). Ícone, capturas da
  loja e ficha da Play com o Bibopzi ficam para quando você quiser (nada foi enviado ao Play Console).

## RT-6v3 — Direção visual e sonora v3, "acabamento premium" (planejada em 2026-11-25)

- **Origem:** pedido do usuário em 2026-11-25 ("não quero esse visual low poly, jogos com mecanicas já feitas exigem um
  nível de polimento grafico melhor para cativar o usuário"; "o padrão visual deve cobrir todos os outros pontos, como
  inimigos, portais, piso, background, efeitos sonoros"; "Sim, escreve o plano"). Direção, guia, papéis, ciclo, riscos e
  calendário: [`direcao-visual-v3.md`](direcao-visual-v3.md). Fichas: **D36** (imagem-alvo; rascunho, sai no fim da
  RT-6v3.2) e **D37** (imagem/música por IA; recomendação C agora) em `riscos-e-decisoes.md`.
- **Regras da série:** uma entrega por sessão; cada entrega que muda o desenho passa pelo gate 3× no OPPO no caso mais
  pesado, uma vez, em janela combinada com o match-3 (mesmo aparelho); mudança no motor é **entrega K própria** no
  `studiokit` (K.v3a–c), feita antes da entrega do jogo que a usa, com a aceitação do studiokit **incluindo o match-3 e o
  Wild Pack**, e com o `ToonLit` e o `ToonInstanceData` atuais intactos (shader irmão, layout novo opcional); uma sessão
  escrevendo no `studiokit` por vez; nada de Instantiate/Destroy em jogo (2.2.1); crítica por agente com visão numa
  sessão diferente da que implementou (ciclo da seção 6 de `direcao-visual-v3.md`); skills `unity:craft-game-floors` e
  `unity:urp-postprocessing` quando couberem, com as regras do projeto prevalecendo. Commit `Stage RT.6v3.N: ...`.
- **Aceites do usuário agrupados e não bloqueantes:** (1) depois da 6v3.4b (luz e personagem); (2) depois da 6v3.7
  (mundo); (3) depois da 6v3.10 (efeitos, telas, ícone, som). Só a D36 bloqueia (trava 8 das 13 entregas do jogo).
- **Ordem:** 6v3.1 → 6v3.2 (D36) → K.v3a → 6v3.3 → K.v3b → K.v3c → 6v3.4a → 6v3.4b → 6v3.4c → 6v3.5 → 6v3.6 → 6v3.7 →
  6v3.8 → 6v3.10 → 6v3.11, **em série** (a cena é regravada inteira e `StyleGuide`, `ArtPalette` e o construtor da cena
  são compartilhados). Em paralelo, só o som (6v3.9, depois da 6v3.1) e a geração de malhas no Blender.
- **Modelo por entrega:** Opus nas K.v3a–c, 6v3.2, 6v3.3, 6v3.4a e 6v3.4b; Sonnet nas demais (Opus se a área travar);
  crítica e fichas por Fable ou Opus.

## RT-6v3.1 — Guia de estilo como dado e validador (Sonnet)

- **Escopo:** `StyleGuide` (asset do jogo) com as seções da 2.1 de `direcao-visual-v3.md`, preenchido com os valores
  **atuais** do jogo e todas as classes marcadas `legado` (nada muda na tela); `StyleGuideValidation` ligado ao validador do
  jogo, com as regras sempre ligadas e as regras `v3` desligadas até cada classe migrar; `StyleCaptureRig` com as câmeras
  fixas da seção 6 e as capturas "antes"; checklist gerado do guia (`docs/art/v3/checklist.md`). Tipos genéricos no kit só
  se o motor precisar (então vira entrega K curta, antes).
- **Status:** concluída em 2026-11-25. `StyleGuide` (asset gerado por `StyleGuideContent`, todas as 12 classes `legado`), `StyleGuideValidation` ligado ao `ClaybreakValidation` (regras sempre ligadas + regras v3 por classe migrada), 23 testes `StyleGuideTests` (passa no jogo de hoje; reprova material/efeito/clipe sem entrada, malha acima do teto e, com a classe `v3`, cor, shader, malha, aresta, efeito e clipe fora do guia), `StyleCaptureRig` (6 câmeras fixas, 30 quadros fixos e semente), `docs/art/v3/checklist.md` gerado, capturas em `docs/art/v3/antes/` e `Tools/art/compare_captures.py`. `rt4-check` completo verde (EditMode 737/737, PlayMode 28 executados, APK, AAB). Comparação de pixels: telas de hoje (`docs/art/telas`) regravadas pelo passo `capture` idênticas ao commit (sem diff no git); duas rodadas do rig: estáticas 0%, corrida 0,68% e assalto 0,60% (multidão e destroços). Decisões T-RT6v3.1-1..8 em `decisoes.md`. Gate 3× não se aplica. Nada exige o usuário.
- **Depende de:** aprovação deste plano.
- **Aceite:** validação padrão verde; o validador passa no jogo de hoje e **reprova** casos de teste (material sem entrada,
  malha acima do orçamento, efeito e clipe sem entrada; e, com a classe marcada `v3`, cor e shader fora do guia);
  capturas "antes" em `docs/art/v3/antes/`; comparação de pixels com as capturas de hoje dentro da tolerância. Gate 3× não
  se aplica.
- **Exige o usuário:** nada (técnica).
- **Prompt da sessão:** "Entrega RT-6v3.1 do Runner vs Tower (Stampeep): guia de estilo como dado e validador. Leia
  `docs/direcao-visual-v3.md` (seções 2 e 6) e `.claude/sessao.md`. Crie o `StyleGuide` com os valores atuais do jogo e
  todas as classes `legado` (o jogo não pode mudar na tela), o `StyleGuideValidation` ligado ao `ClaybreakValidation`
  (regras v3 ligadas só nas classes migradas), o `StyleCaptureRig` com as 6 câmeras fixas por área e o checklist gerado do
  guia. Não mexa no `studiokit` (se precisar, pare e proponha uma entrega K). Rode o `rt4-check` e as capturas 'antes'.
  Registre decisões técnicas em `docs/decisoes.md` e o status aqui."

## RT-6v3.2 — Imagem-alvo medida: 2 a 3 telas-conceito no Unity + ensaio no OPPO (Opus)

- **Escopo:** 2–3 direções de "brinquedo de vinil premium" (ex.: A vinil brilhante e saturado; B pastel fosco com
  brilho só no personagem; C contraste alto com luz de fim de tarde [P]), cada uma nas **3 telas fixas** (corrida com 300,
  assalto com a torre, home com o Bibopzi suavizado), feitas **no Unity** numa cena de laboratório com **shader de
  laboratório local ao jogo** (sem tocar o kit). Para torres e chefes, duas versões: tema por mundo arredondado ×
  "fábrica de brinquedo" (D17/D28). 2–3 esboços de ícone (D25). Uma referência do gênero lado a lado (2–3 jogos do topo,
  por link, sem copiar arte). **Ensaio no OPPO** de cada opção com `Tools/rt6t-toonlab-gate.sh` (300 × 1.200 triângulos,
  borda e especular, pós ligado e desligado, Vulkan e GLES em blocos), registrando GPU p95 contra o teto de 11 ms.
  Imagens por IA só se a D37 aprovar, como inspiração. Completa a ficha **D36** com as imagens, os números, a
  recomendação e a segunda opinião.
- **Status:** concluída em 2026-11-25; **D36 com o usuário**. Shader de laboratório `Claybreak/VinylLab` (irmão do ToonLit, local ao jogo; `studiokit` intacto), direções como dado (`VinylDirection`: A vinil brilhante, B pastel fosco, C fim de tarde e M, a mistura recomendada pelo revisor), Bibopzi suavizado (490 → 1.960 tri), torre "fábrica de brinquedo" procedural (13.920 tri), telas nas câmeras fixas do `StyleCaptureRig` (`VinylConceptTests`) e folhas em `docs/art/v3/alvo/` (README com tudo). Ensaio no OPPO (300 × 1.960 tri, pós ligado/desligado, Vulkan e GLES): GPU p95 pior janela estável 4,1/7,6 ms hoje; A 4,7/7,6 e 5,6/7,1 com pós; B 4,6/7,9 e 7,6/8,5; C 4,5/8,3 e 5,4/7,5; M 5,3/8,1 e 5,6/7,7 (medida depois, com o celular desbloqueado) — tudo abaixo de 11 ms. Revisor fable nas telas e na ficha; D36 completa em `riscos-e-decisoes.md`. Validador, isolamento, EditMode 737/737 e PlayMode verdes. Decisões T-RT6v3.2-1..8 em `decisoes.md`.
- **Depende de:** 6v3.1 (câmeras fixas e checklist).
- **Aceite:** telas em `docs/art/v3/alvo/` com o enquadramento das capturas "antes"; números do ensaio no OPPO por opção;
  crítica do revisor (fable) contra o checklist; ficha D36 completa e enviada.
- **Exige o usuário:** **D36** (imagem-alvo + tema das torres + ícone/público); OPPO conectado para o ensaio.
- **Prompt da sessão:** "Entrega RT-6v3.2 do Stampeep: imagem-alvo medida. Leia `docs/direcao-visual-v3.md` (seções 2, 4
  e 7), as decisões D17, D25 e D28 e o rascunho da D36 em `docs/riscos-e-decisoes.md`. Faça 2 a 3 direções de 'brinquedo
  de vinil premium' nas 3 telas fixas, no Unity, com shader de laboratório local ao jogo (não toque o `studiokit`), duas
  versões de torre/chefe (tema por mundo × fábrica de brinquedo), 2–3 esboços de ícone e uma referência do gênero por link.
  Ensaie cada opção no OPPO com `Tools/rt6t-toonlab-gate.sh` (300 × 1.200 tri, pós ligado/desligado, duas rotas; combine a
  janela do aparelho). Imagens por IA só se a D37 estiver aprovada. Passe pelo revisor (fable), complete a D36 com
  imagens, números, recomendação e segunda opinião, envie e pare: a escolha é do usuário."

## K.v3a (studiokit) — Shader irmão de vinil (Opus)

- **Escopo:** no repositório `studiokit`, um shader **irmão** do `ToonLit` (ex.: `StudioKit/ToonVinyl` [P]) com rampa suave,
  borda, especular suave, sombra quente e os perfis do guia (vinil, fosco, portão, inimigo, UI), nas duas rotas de
  instancing (buffer e blocos de 256), lendo o **mesmo** `ToonInstanceData` de hoje; o `ToonLit` fica intacto. Testes do kit;
  `TestProject` com captura.
- **Depende de:** D36 (os parâmetros da opção escolhida); nenhuma outra sessão escrevendo no `studiokit`.
- **Aceite:** aceitação do studiokit (testes, isolamento, validadores e save do Wild Pack, **match-3 sem mudança na tela**);
  versão nova do kit com tag.
- **Exige o usuário:** nada.
- **Status:** concluída em 2026-11-26. `studiokit` 0.15.0, tag `v0.15.0` (decisão T43 lá; T-Kv3a-1..4 em `decisoes.md`).
  `StudioKit/ToonVinyl` irmão do `ToonLit` (mesmas propriedades, palavras-chave e `ToonInstanceData`, duas rotas),
  `ToonVinylLook`/`ToonVinylLookApplier` (luz principal quente, ambiente frio, borda, ombro de tom) e `ToonVinylSurface`
  (vinil, fosco, portão, inimigo, efeito/UI); barriga da A sem estouro (ombro + brilho limitado), brilho mantido.
  Matcap medido no OPPO (`VinylScene` do `TestProject`, 300 × 1.920 tri, 60 fps firmes; GPU p95 Vulkan `ToonLit`
  4,9–5,4 ms, luz 5,9–6,3, matcap 5,2–5,6; GLES ~15 ms em todos, leitura do medidor) e descartado: **luz**. Aceitação:
  `TestProject` EditMode 490/490 com GPU e isolamento PASS; match-3 sem arquivo usado alterado, isolamento PASS, EditMode
  99/99, fases PASS; Wild Pack igual à linha de base (90/0, 117/0, áudio 16, save 87/87, isolamento PASS); este jogo
  isolamento PASS e EditMode 747/751 (4 ignorados sem GPU; os testes do vinil com GPU neste projeto linear: 11/11). Revisão independente sem bloqueio. Capturas em `../studiokit/docs/kv3a/`. Nada exige o usuário.
- **Prompt da sessão:** "Entrega K.v3a do studiokit: shader irmão de vinil para o Stampeep (o kit não nomeia o jogo). Leia
  `RunnerVsTower/docs/direcao-visual-v3.md` (seções 1, 2.1, 7) e a resposta da D36. Escreva um shader novo ao lado do
  `ToonLit`, sem mudar o `ToonLit` nem o `ToonInstanceData`, nas duas rotas. Rode a aceitação do studiokit incluindo o
  match-3 e o Wild Pack. Confira antes que nenhuma outra sessão está escrevendo no `studiokit`."

## RT-6v3.3 — Vinil, plástico fosco, luz, céu e pós por mundo (Opus)

- **Escopo:** no jogo, os materiais passam ao shader da K.v3a pelos perfis do guia; luz, céu (degradê + camada de nuvens) e
  pós barato (tonemapping neutro, cor, vinheta; bloom só se passar) por mundo, vindos do guia, com a skill
  `unity:urp-postprocessing`; paleta medida antes do pós; conferência da regra "skins ΔE ≥ 20" e da leitura dos números dos
  portões depois do pós. Ainda com as malhas de hoje. Classes de luz e material marcadas `v3`.
- **Depende de:** K.v3a.
- **Aceite:** capturas das 6 câmeras nos 4 mundos contra a imagem-alvo (crítica de outra sessão); validador do guia verde;
  **gate 3× no OPPO** no caso mais pesado (assalto do mundo 4, 300 em voo, chefe), Vulkan e GLES em blocos.
- **Exige o usuário:** nada agora (entra no aceite agrupado 1).
- **Status:** concluída em 2026-11-26. Superfície `v3` em
  Unit, Leader, Enemy, Tower, Boss, Obstacle, Floor e Scenery (malhas ainda as de hoje): 34 materiais em `StudioKit/ToonVinyl`
  pelos perfis do `StyleGuide` (vinil, plástico fosco, estrutura, inimigo); luz, pós (sem tonemapping, cor, vinheta, bloom
  leve) e névoa colorida por mundo vindos do guia (`Style/Light*`, `Style/Post*`, aplicados pelo `WorldScenery`); paletas
  "fábrica noturna" nos 4 mundos. Regra de contraste da D36 medida no `StyleGuideValidation`, na paleta e na captura dos 4
  mundos (`docs/art/v3/contraste.json`, sonda por camadas; passo novo `style` no `rt4-check`): multidão com o maior croma na
  tela (76–95 na corrida contra no máximo 44), |ΔL*| multidão × pista 22–62, defesas a 99–169° de matiz, números dos portões
  5,0–9,0:1, skins a ΔE ≥ 23 da pista. Revisão: brilho no chão (topo 0 em Floor/Scenery), UI fora do pós (overlay conferido;
  capturas compõem a UI sem pós), números legíveis (contorno 0,40). Crítica por fable em 2 voltas + ajustes finais (veredito
  "pode seguir"); capturas em `docs/art/v3/telas/`. `rt4-check` completo verde: cena, captura de estilo, validador,
  isolamento, EditMode 753/753, PlayMode 28 executados, economia, capturas, APKs e AAB. Decisões T-RT6v3.3-1..10 em
  `decisoes.md`. Camada de nuvens adiada para a 6v3.7; emissão HDR (bloom nas janelas) para a K.v3b/6v3.5.
  **Gate 3× no OPPO CPH2791** (2026-11-26, com o celular reconectado; assalto do mundo 4, chefe, 300 em voo, APK de medição
  do commit `c483067`; janelas de 5 s 2–6): **Vulkan** (rota buffer) GPU p95 4,2–4,6 ms, CPU principal p95 2,2–2,9 ms,
  62,5 fps médios, quadro p95 16 ms, > 50 ms 0%, PSS 270 MB; **GLES em blocos** (`[ToonRoute] route=Chunks api=OpenGLES3`)
  GPU p95 7,8–8,4 ms, CPU principal p95 2,5–5,2 ms, 62,5 fps, quadro p95 16 ms, > 50 ms 0%, PSS 299 MB. Teto 11/11/16,7 ms:
  dentro nas duas rotas, sem repetição; nenhum "Shader error" no build. App fechado no aparelho (`am force-stop`).
- **Histórico:** entregue em 2026-11-26 (`Stage RT.6v3.3`); gate 3× no OPPO no mesmo dia, nas duas rotas.
- **Prompt da sessão:** "Entrega RT-6v3.3 do Stampeep: vinil, luz e pós. Leia `docs/direcao-visual-v3.md`, a resposta da
  D36 em `docs/decisoes.md` e as telas em `docs/art/v3/alvo/`. Use o shader da K.v3a pelos perfis do `StyleGuide`; luz, céu e
  pós por mundo do guia (skills unity:urp-postprocessing e unity:craft-game-floors; as regras do projeto prevalecem). Ciclo:
  capturas fixas → crítica contra o alvo → corrigir (até 3 voltas). Rode o `rt4-check` e o gate 3× no OPPO nas duas rotas.
  Registre decisões e status."

## K.v3b (studiokit) — Layout de instância novo: rosto, fase do quique, UV (Opus)

- **Escopo:** no `studiokit`, um layout de instância **novo e opcional** (struct maior na rota de buffer; propriedade
  instanciada nova na rota de blocos de 256) com índice de expressão e fase/velocidade do quique escritos pelo job da CPU;
  suporte a UV, UV2 e amostragem de um atlas no shader irmão; atraso analítico de fase para vértices marcados (UV2). O layout
  atual (64 bytes) continua sendo o padrão.
- **Depende de:** 6v3.3.
- **Aceite:** aceitação do studiokit (match-3 e Wild Pack incluídos); medição de custo nas duas rotas no `TestProject`.
- **Exige o usuário:** nada.
- **Status:** concluída em 2026-11-26. `studiokit` 0.16.0, tag `v0.16.0` (decisão T44 lá; T-Kv3b-1..5 em `decisoes.md`).
  Material `ToonVinyl` com `ToonVinylMotion.ApplyTo` (palavra-chave `STUDIOKIT_TOON_ANIMATED`) passa ao layout animado:
  80 bytes no buffer, `_ToonInstanceMotion` nos blocos de 256, sombra no mesmo layout; multidão (`SwarmRenderer`) e
  soltos do assalto (`LooseSwarmRenderer`) escrevem célula do rosto, fase e velocidade do quique e mãos
  (`SwarmExpression`, trocada pelo jogo nos eventos). Vértices marcados em UV2: x mola (antena/topete), y mãos (±1), z peça
  que gira (a **chave de corda** da D36, voltas inteiras por ciclo do quique), w rosto; rosto por `ToonFaceAtlas`
  (`STUDIOKIT_VINYL_FACE`, linha 0 em cima, margem por célula). Layout de 64 bytes e `ToonLit` intactos. Medição no OPPO
  (`VinylScene` do `TestProject`, 300 unidades, 60 fps firmes): GPU p95 Vulkan vinil 5,9–6,3 ms, animado (2.120 tri com
  rosto, mola, mãos e chave) 6,5–6,6 ms; GLES lê ~15 ms nos dois (leitura do medidor). Aceitação conjunta com a K.v3c
  abaixo. **Para a RT-6v3.4a/b:** pesos da mola e das mãos e a chave nas costas vão em UV2 (os scripts do Blender gravam
  esse canal); o material da multidão e o da sombra dela ligam a palavra-chave juntos (o kit recusa um sem o outro).
- **Prompt da sessão:** "Entrega K.v3b do studiokit: layout de instância opcional com expressão e fase do quique, UV/UV2 e
  atlas no shader irmão, atraso analítico de fase por vértice marcado. Leia `RunnerVsTower/docs/direcao-visual-v3.md`
  (seções 3.2 e 3.3). Não mude o layout atual nem o `ToonLit`. Aceitação do studiokit com match-3 e Wild Pack."

## K.v3c (studiokit) — Construtor de malhas liso e chanfrado (Sonnet)

- **Escopo:** `LowPolyMeshBuilder` ganha perfil/torno, chanfro, normais suaves por grupo e UV simples, sem mudar a saída
  das primitivas atuais (match-3 e as formas de hoje dão os mesmos bytes).
- **Depende de:** K.v3b (UV no shader); pode ser feita junto da K.v3b na mesma sessão se couber.
- **Aceite:** testes do construtor (contagem, normais para fora, UV no intervalo); saída das primitivas antigas idêntica;
  aceitação do studiokit.
- **Exige o usuário:** nada.
- **Status:** concluída em 2026-11-26, na mesma sessão da K.v3b. `studiokit` 0.17.0, tag `v0.17.0` (decisão T45 lá;
  T-Kv3c-1..2 em `decisoes.md`). `AddLathe` (perfil com ângulo de vinco, polos, tampas; `LatheProfiles.RoundedCylinder` e
  `Capsule`), `AddRoundedBox` (arestas e cantos arredondados em passos de ângulo igual), `SmoothNormals` (grupo),
  `ProjectUv` e `Mark` (UV e UV2 em qualquer trecho). Primitivas antigas com os mesmos bytes (impressão digital gravada com
  o construtor da 0.15.0 igual depois; match-3 regerado sem diff). Aceitação das duas: EditMode do `TestProject` 516/516
  com GPU, isolamento PASS; match-3 isolamento PASS, EditMode 99/99, evolução PASS; Wild Pack igual à linha de base
  (90/0, 117/0, áudio 16, save 87/87, isolamento PASS, anúncios offline 25/25); este jogo: isolamento PASS, EditMode 775
  de 783 (8 ignorados: desenho sem GPU) com o passo `scene` antes. Revisão independente (`fable`): nada bloqueante.
- **Prompt da sessão:** "Entrega K.v3c do studiokit: torno/perfil, chanfro, normais suaves e UV no `LowPolyMeshBuilder`,
  sem mudar a saída das primitivas atuais. Leia `RunnerVsTower/docs/direcao-visual-v3.md` (seção 5). Aceitação do studiokit."

## RT-6v3.4a — Bibopzi detalhado e de multidão: modelos e gate (Opus)

- **Escopo:** scripts do Blender 5.3 sem interface em `Tools/blender/` (versão fixa) que gravam **dados simples**
  versionados (posições, normais, UV, UV2, cor linear, índices; nada de FBX); gerador de Editor que grava os `.asset`; os
  dois níveis (3–5 mil e 800–1.200 tri) com os marcos e os pesos da mola e das mãos em UV2; `BibopziHasItsLandmarks` nos
  três níveis; orçamento por classe no validador. **Gate 3× no OPPO** com 300 Bibopzis de multidão no assalto do mundo 4;
  se falhar, a reserva na ordem (LOD por distância → modelo de ~500 suavizado) e ficha se nada passar.
- **Depende de:** K.v3b, K.v3c.
- **Aceite:** testes e validador; a validação não depende do Blender instalado; capturas de perto e com 300 contra o alvo;
  gate aprovado.
- **Exige o usuário:** OPPO para o gate.
- **Status:** concluída em 2026-11-26 (decisões T-RT6v3.4a-1..11 em `decisoes.md`). Scripts do Blender 5.3 em
  `Tools/blender/` (README lá; `run.sh` confere o determinismo) gravam dados simples em `Tools/blender/out/*.json`;
  `SimpleMeshData` + `UnitShapeAssets` gravam os `.asset` no passo `scene`; o validador confere o hash dos scripts e não
  precisa do Blender. Níveis: detalhado 4.990 tri (o líder, `CrowdShapeOption.leaderMesh`), multidão 1.160 (as 300, asset
  `Bibopzi` de sempre), reserva 546. Marcos nos três (mola, engrenagem, barriga creme, mãos soltas, olhos maiores, **chave de
  corda nas costas**), UV2 com mola, mãos e chave para o layout animado (a palavra-chave fica para a 6v3.4b). Orçamento por
  malha no guia (v3 e reserva antes da classe migrar) e aresta dura na regra de 60°. Corrigido o achado da K.v3b: os
  construtores de arte voltavam os materiais vinil para `ToonLit` (passo `capture`); agora mantêm o vinil e os 34 `.mat`
  estão commitados. Capturas em `docs/art/rt6v34a/` (três níveis de perto, corrida com 300, comparação com o alvo A) e telas
  do guia regravadas; crítica fable: "pode seguir", correções aplicadas. rt4-check verde (cena, style, validador, isolamento,
  EditMode, PlayMode, economia, capturas, APKs, AAB). Gate 3× no OPPO (assalto do mundo 4, 300 Bibopzis em voo): Vulkan GPU
  p95 4,9–5,4 ms, GLES em blocos 7,4–8,4 ms, CPU ≤ 4,0 ms, 62,5 fps, quadro p95 16 ms; reserva não usada. APK final (1.160 tri, +0,9% depois da crítica) não remedido: o OPPO bloqueou com senha; margem de GPU ~2× no teto.
- **Histórico:** entregue em 2026-11-26 (`Stage RT.6v3.4a`).
- **Prompt da sessão:** "Entrega RT-6v3.4a do Stampeep: Bibopzi detalhado e de multidão. Leia `docs/direcao-visual-v3.md`
  (seção 3.1), a folha do personagem no repositório `personagem` (D3 'Feijão') e as capturas da RT-6t.22. Escreva os
  scripts do Blender 5.3 (`blender -b -P`) que gravam dados simples versionados e o gerador de Editor que grava os `.asset`;
  marcos nos três níveis; gate 3× com 300 no assalto do mundo 4, reserva na ordem se falhar. Registre decisões e status."

## RT-6v3.4b — Rosto e animação do Bibopzi (Opus)

- **Escopo:** atlas de 8 expressões (células com margem, mipmaps, máscara 0 no rosto), tabela evento → expressão no guia,
  amassar/esticar no pouso, atraso da mola e mãos pelo layout da K.v3b; o detalhado com o mesmo rosto nas telas.
- **Depende de:** 6v3.4a.
- **Aceite:** testes EditMode das fórmulas e da tabela; capturas em sequência (susto, feliz, vitória); gate 3× se o custo
  mudar; **aceite agrupado 1** pedido ao usuário (luz e personagem: capturas + APK no OPPO), sem parar a série.
- **Exige o usuário:** aceite agrupado 1 (não bloqueia a 6v3.4c).
- **Status:** concluída em 2026-11-26 (decisões T-RT6v3.4b-1..9 em `decisoes.md`), **gate 3× no OPPO aprovado** (assalto do mundo 4, chefe, 300 Bibopzis em voo, layout animado com rosto, mola e chave):
  Vulkan (buffer) GPU p95 4,7–6,4 ms, CPU principal p95 ≤ 3,9 ms, 62,4 fps, quadro p95 16 ms, 0 perdidos, PSS 320 MB; GLES
  (blocos de 256) GPU p95 7,2–7,9 ms, CPU ≤ 3,8 ms, 62,5 fps, 0 perdidos, PSS 309 MB; `[ToonRoute]` conferido nas duas,
  captura do aparelho sem shader de erro; reserva não usada. APK de jogo instalado no OPPO; app fechado. **Aceite agrupado 1
  pendente** (não bloqueia a 6v3.4c): pacote em `docs/aceite/agrupado-1/` (`index.html` com antes/depois e roteiro no
  `README.md`), APK de jogo em `C:\temp\rt64b\Stampeep-RT-6v3.4b.apk` (fora do repositório). Atlas `BibopziFaces.png`
  (512 × 512, 8 células de 128 × 256, mipmaps, ASTC 4×4) desenhado por `Tools/art/bibopzi_faces.py` (hash conferido pelo
  validador); olhos, sobrancelhas e boca saíram da geometria (detalhado 4.382, multidão 1.028, reserva 502 tri); máscara
  UV2.w na frente do corpo e UV = projeção do corpo inteiro. Tabela evento → expressão em `StyleGuide.Faces` (dado, com
  duração e prioridade; no `checklist.md`), `FaceDirector` puro + `CrowdFace` no fluxo (portão bom/ruim, obstáculo,
  assalto, voo, vitória/derrota, piscar). Amassar no pouso/esticar na subida pela K.v3d (`studiokit` 0.18.0, tag
  `v0.18.0`, `SwarmMotion.ScaleY`, Bibopzi 12%/10%); mola, mãos e chave (aço escuro, 1,4×) pelo layout animado da K.v3b;
  o líder manda o vetor de movimento dele; coroa do Bibopzi 6 cm mais alta. Figura da loja do detalhado com o mesmo rosto.
  Testes EditMode novos (`BibopziFaceTests`: tabela, diretor, atlas, legível a 40 px, quique, mola, mãos, chave); EditMode
  804/804 (8 ignorados de GPU), PlayMode 28/28 (25 capturas ignoradas sem GPU), validador, isolamento, cena e `style` verdes; APKs de jogo e de medição.
  Capturas em `docs/art/rt6v34b/` (8 rostos, sequência descanso → susto → feliz → vitória, costas com a chave girando,
  atlas, figura da loja); telas do guia regravadas. Crítica fable: "pode seguir" com 4 correções, aplicadas.
- **Histórico:** entregue em 2026-11-26 (`Stage RT.6v3.4b`).
- **Prompt da sessão:** "Entrega RT-6v3.4b do Stampeep: rosto e animação. Leia `docs/direcao-visual-v3.md` (seções 3.2 e
  3.3). Atlas de expressões, tabela evento → expressão no guia, amassar/esticar, atraso da mola e mãos pelo layout da K.v3b.
  Capturas em sequência; gate se o custo mudar. Monte o pacote do aceite agrupado 1 (capturas e APK) e siga sem esperar."

## RT-6v3.4c — As outras formas da loja no padrão (Sonnet)

- **Escopo:** as 5 outras formas (D31/D32) em versão lisa de multidão (800–1.200 tri, mesma silhueta, preços e quiques
  intocados) pelo construtor da K.v3c; quem veste outra forma usa o nível de multidão como líder e nas telas, olhos por
  geometria; figuras da loja refeitas; `EveryPairOfShapesHasADifferentOutline` continua passando.
- **Depende de:** 6v3.4a (pode seguir sem o aceite agrupado 1).
- **Aceite:** testes de forma e silhueta; capturas das 6 formas na corrida e no assalto; gate se alguma passar a mais pesada.
- **Exige o usuário:** nada (entra no aceite agrupado 2).
- **Prompt da sessão:** "Entrega RT-6v3.4c do Stampeep: as outras formas da loja no padrão v3. Leia
  `docs/direcao-visual-v3.md` (seção 3.1) e as decisões D31/D32. Refaça as 5 formas lisas com o construtor da K.v3c, mesma
  silhueta e preços; figuras da loja; testes de silhueta; capturas. Registre decisões e status."

## RT-6v3.5 — Inimigos, torres e chefes (Sonnet)

- **Escopo:** defesas (canhão, muro, escudo, varredor), torres e os 4 chefes arredondados e chanfrados no tema que a D36
  escolheu (tema por mundo ou fábrica de brinquedo), sem rostos (D17/D28); leitura de ameaça por cor de borda e luz; quebra
  em peças pré-alocadas ao perder vida. Modelos por Blender (script, dados simples) ou receitas do kit, dentro do
  orçamento por classe.
- **Depende de:** 6v3.4a.
- **Aceite:** capturas fixas do assalto nos 4 mundos contra o alvo; validador do guia (classes marcadas `v3`); gate 3× no
  assalto mais pesado (mundo 4, chefe, 300 em voo).
- **Exige o usuário:** nada agora (aceite agrupado 2).
- **Prompt da sessão:** "Entrega RT-6v3.5 do Stampeep: inimigos, torres e chefes. Leia `docs/direcao-visual-v3.md`
  (seções 2 e 4), a resposta da D36 (tema) e as decisões D17 e D28. Remodele defesas, torres e chefes sem rostos, dentro do
  orçamento, com quebra em peças pré-alocadas (skill unity:craft-game-floors; regras do projeto prevalecem). Ciclo de
  capturas e crítica; `rt4-check` e gate 3× no assalto do mundo 4. Registre decisões e status."

## RT-6v3.6 — Portões e obstáculos (Sonnet)

- **Escopo:** portões (arco de vinil chanfrado, painel translúcido com borda acesa, número com relevo) e os 5 obstáculos
  (RT-6t.18) no padrão; cores bom/ruim do guia. Sem mudar regra, tempo nem janela de reação.
- **Depende de:** 6v3.5 (em série: cena e guia compartilhados).
- **Aceite:** capturas fixas da corrida com portões e cada obstáculo; `ReactionWindow` e `GateMath` sem mudança; leitura dos
  números conferida depois do pós; gate 3× na corrida mais pesada (mundo 4, 300, 3 obstáculos).
- **Exige o usuário:** nada agora (aceite agrupado 2).
- **Prompt da sessão:** "Entrega RT-6v3.6 do Stampeep: portões e obstáculos. Leia `docs/direcao-visual-v3.md` (seção 4).
  Remodele portões e os 5 obstáculos no padrão do guia, sem mudar regras nem tempos (skill unity:craft-game-floors; regras do
  projeto prevalecem). Ciclo de capturas e crítica; `rt4-check` e gate 3× na corrida do mundo 4. Registre decisões e status."

## RT-6v3.7 — Piso, cenário e céu (Sonnet)

- **Escopo:** pista com faixas chanfradas e juntas de peças, trilhos arredondados; cenário instanciado por mundo (nada que
  pareça doce/infantil demais, D25); céu com camada de nuvens recortadas; tudo do guia, com a skill
  `unity:craft-game-floors`.
- **Depende de:** 6v3.6.
- **Aceite:** capturas fixas dos 4 mundos; neblina mínima (`PropWrapGuard`) respeitada; gate 3× na corrida mais pesada;
  **aceite agrupado 2** pedido ao usuário (mundo: inimigos, portões, obstáculos, piso, cenário, formas), sem parar a série.
- **Exige o usuário:** aceite agrupado 2 (não bloqueia a 6v3.8).
- **Prompt da sessão:** "Entrega RT-6v3.7 do Stampeep: piso, cenário e céu. Leia `docs/direcao-visual-v3.md` (seções 2 e 4)
  e a D25; use a skill unity:craft-game-floors (regras do projeto prevalecem). Refaça pista, trilhos, cenário instanciado e
  céu nos 4 mundos a partir do guia. Ciclo de capturas e crítica; `rt4-check` e gate 3× na corrida do mundo 4. Monte o pacote
  do aceite agrupado 2 e siga sem esperar."

## RT-6v3.8 — Efeitos visuais (Sonnet)

- **Escopo:** a tabela de efeitos do guia (confete de engrenagem, estalo de mola, poeira em anel, faísca de parafuso,
  quebra da torre) com pools pré-alocados e limites por efeito e por tela.
- **Depende de:** 6v3.7.
- **Aceite:** capturas em sequência (3 instantes por efeito); validador (efeito sem entrada ou acima do limite reprova);
  nenhum Instantiate em jogo (teste); gate 3× no assalto mais pesado com os efeitos.
- **Exige o usuário:** nada agora (aceite agrupado 3; efeito é movimento: jogar no OPPO).
- **Prompt da sessão:** "Entrega RT-6v3.8 do Stampeep: efeitos visuais. Leia `docs/direcao-visual-v3.md` (seção 2.1).
  Implemente a tabela de efeitos com pools, cores da paleta e limites, sem Instantiate em jogo (skill
  unity:craft-game-floors; regras do projeto prevalecem). Capturas em sequência e crítica; `rt4-check` e gate 3× no assalto
  do mundo 4. Registre decisões e status."

## RT-6v3.9 — Som e música (Sonnet; em paralelo)

- **Status:** concluída em 2026-11-26, **aceite pendente (escuta do usuário, aceite agrupado 3; não bloqueia a série)**. Efeitos
  "brinquedo de corda" e 4 músicas por mundo com o motivo 1-5-3-8, tudo por síntese (`Tools/audio/stampeep_toy_audio.py`; D37 = C,
  nada por IA). Cues novos `KeyWind` e `NumberPop`; `ClaybreakTheme.SelectWorldMusic` (reflexão no `runMusic` do kit, paliativo:
  pendência K) escolhe o laço do mundo. Validador: classe `Sound` v3 com pico, volume aproximado e duração por clipe; tabela
  evento → som no `StyleGuide`/checklist; tudo em `audio-licenses.md`. `rt4-check` scene, validate, isolation, EditMode (739/739
  + 1 ignorado) e PlayMode (28/28 + ignorados) verdes. **Para o usuário:** abrir `docs/audio/v3/escuta.html` (PC, de dentro da pasta
  do projeto) e dizer o que ficou bom ou estranho. Detalhes técnicos em `decisoes.md` (2026-11-26).

- **Escopo:** efeitos sonoros "brinquedo de corda" (catraca, mola, sino curto) por síntese (evolução de `Tools/audio/`) ou
  CC0; música por mundo com o mesmo motivo de 4 notas; gerador de música do vidIQ só se a D37 aprovar, com os termos lidos
  e a licença registrada; tabela evento → som no guia; métricas automáticas (volume aproximado, pico, duração) no
  validador; tudo em `audio-licenses.md`. Toca só em `Tools/audio/`, `Content/Audio/` e na seção de som do guia.
- **Depende de:** 6v3.1. Corre em paralelo com as outras (arquivos próprios; não usa o OPPO para gate).
- **Aceite:** validador (clipe fora da lista ou fora do alvo reprova); página de escuta antes/depois com as métricas;
  **escuta do usuário** (o modelo não ouve), no aceite agrupado 3.
- **Exige o usuário:** escutar e aprovar; D37 se quiser música gerada.
- **Prompt da sessão:** "Entrega RT-6v3.9 do Stampeep: som e música. Leia `docs/direcao-visual-v3.md` (seções 2.1 e 5),
  `docs/audio-licenses.md` e a D37. Faça os efeitos 'brinquedo de corda' por síntese (ou CC0) e a música por mundo com o
  motivo comum (skill unity:craft-game-floors para o mapa de sons; regras do projeto prevalecem); métricas de volume e pico
  no validador; registre cada arquivo. Toque só nos arquivos de som. Prepare a escuta antes/depois e pare para o aceite: você
  não ouve o resultado."

## RT-6v3.10 — Telas e ícone (Sonnet)

- **Escopo:** telas no padrão (botões de vinil com relevo e sombra, painéis, Bibopzi detalhado com expressões na home, na
  loja e no resultado); ícone do celular e ícone 512 da loja conforme a resposta da D36 (rosto em destaque ou não, D25).
- **Depende de:** 6v3.8 (e 6v3.4b).
- **Aceite:** `ScreenFitTests` nas 4 proporções; capturas das telas e dos ícones; **aceite agrupado 3** (efeitos, telas,
  ícone, som) pedido ao usuário.
- **Exige o usuário:** aceite agrupado 3.
- **Prompt da sessão:** "Entrega RT-6v3.10 do Stampeep: telas e ícone. Leia `docs/direcao-visual-v3.md` (seção 4) e as
  respostas da D25 e da D36 (ícone). Refaça as telas no padrão do guia com o Bibopzi detalhado e as expressões (skill
  unity:craft-game-floors; regras do projeto prevalecem) e o ícone escolhido. `ScreenFitTests` nas 4 proporções e
  `rt4-check`. Monte o pacote do aceite agrupado 3. Não mexa na ficha da Play nem envie nada ao Play Console."

## RT-6v3.11 — Gate final e capturas da loja (Sonnet)

- **Escopo:** gate 3× final no OPPO no jogo inteiro (corrida e assalto do mundo 4, as duas rotas); capturas da loja refeitas
  (`StoreCaptureTests` + `Tools/store/make_store_art.py`), destaque 1024×500 e vídeo se houver; antes/depois de todas as
  câmeras fixas; `docs/store/` atualizado para o usuário subir; retrabalho pendente dos aceites agrupados.
- **Depende de:** 6v3.10 e 6v3.9; respostas dos 3 aceites agrupados.
- **Aceite:** gate 3× aprovado; material da loja novo em `docs/store/`; aceite final do usuário jogando no OPPO.
- **Exige o usuário:** jogar no OPPO e aceitar; subir capturas e ícone no Play Console (a sessão nunca envia).
- **Prompt da sessão:** "Entrega RT-6v3.11 do Stampeep: gate final e capturas da loja. Leia `docs/direcao-visual-v3.md` e
  `docs/store/README.md`. Aplique o retrabalho pedido nos aceites agrupados, rode o gate 3× no OPPO no caso mais pesado (duas
  rotas), refaça as capturas e o destaque da loja e monte o antes/depois de todas as câmeras fixas. Nada vai para o Play
  Console: deixe o material pronto para o usuário."

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
- **Depende de:** RT-9b (e a ficha D9). Revisão do público-alvo pela Play antes da produção: até 7 dias ou mais
  (`store/requisitos-play.md`).
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

- **Série RT-6v3 + K.v3 (2026-11-25) [I]:** ~16,5 semanas com a margem (caminho crítico ~15,3) + ~1 de espera da D36 + ~1
  de festas, a partir de 1/dez/2026 → fim por volta de início de abr/2027; folga de ~2 semanas até meados de abr e ~6 até
  o soft launch (meados de mai). Riscos próprios: custo por pixel no OPPO (ensaiado antes da D36), kit compartilhado com o
  match-3 (entregas K com a aceitação dele), D36 voltando com "outra rodada". Acima de ~2 semanas de atraso, o soft launch
  desliza para jun e o lançamento para jul/2027; antes disso, cortar escopo (rosto com 4 expressões, música por síntese).

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
- **Desconhecido que pode estourar o calendário:** ~~a regra da Play para teste
  fechado de contas novas~~ — **pesquisada na RT-9a (2026-11-24):** os 12
  testadores por 14 dias valem só para contas pessoais novas; a da empresa é de
  organização (`store/requisitos-play.md`). Resta a revisão do público-alvo
  (até 7 dias) antes da produção.
- **Paralelismo:** RT-0 e RT-1 correm junto com a operação do Wild Pack; sem
  outro desenvolvedor, qualquer incidente do Wild Pack consome as semanas
  deste plano. A margem de 20% **não** cobre incidentes grandes.
