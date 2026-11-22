# Runner vs Tower — GDD (rascunho de planejamento)

Data: 2026-11-19. Estúdio: Jungle Degree. Status: **proposta**; nada aqui foi
decidido pelo usuário nem implementado. Nome de trabalho: "Runner vs Tower"
(nome final é decisão pendente, ver `riscos-e-decisoes.md`).

Convenção: **[F]** = fato com fonte (pesquisa `docs/proximo-jogo-pesquisa.md`,
2026-11-18, ou `SISTEMA.md`/código lidos em 2026-11-19). **[I]** = inferência
ou proposta nossa, sem dado que a sustente ainda. **[P]** = parâmetro de design
a calibrar no protótipo (não é medida nem meta de mercado). Onde não há número
com fonte, não há número.

---

## 1. Conceito

Uma multidão corre por uma pista, passa por portões que somam, subtraem,
multiplicam ou dividem, e chega ao fim como um exército. Esse exército é
**lançado contra uma torre inimiga**; cada unidade que acerta tira vida da
torre. A torre tem vida, defesas e projéteis que matam unidades. Derrubar a
torre vence a fase.

Frase de venda [I]: *"Você constrói o exército correndo. Depois ele derruba a
torre."*

- **Gênero:** hybrid-casual (anúncio recompensado + IAP leve), retrato, uma mão.
- **Referências de gênero [F]:** Mob Control (Voodoo), Count Masters, Clash of
  Crowds. Mob Control: 302,1 mi de downloads totais, 81,6 mi em 2025, 26,4 mi de
  MAU em abr/2026, D1 36,1%, D7 13,9%, D30 4,7%, ~metade da receita vem de
  anúncio (Udonis/AppMagic, 2026-07-02).
- **Por que este jogo [F/I]:** nota 15/20 na tabela da pesquisa (a maior), reuso
  estimado em ~70% do motor, dificuldade M (4–6 semanas, estimativa **[I]** da pesquisa, sem
  contar a revisão da Play). Risco: "dono" do gênero é um publisher grande.

## 2. Público e classificação

- **13+, não direcionado a crianças** [F: decisão P3 do usuário, SISTEMA.md 7.27;
  `docs/adultizacao-ideias.md`]. Sem política de Famílias, sem tag infantil nos
  anúncios.
- Direção adulta, a mesma do Wild Pack [F: registro 7.31]: verbos de ação,
  paleta escura/quente, humor seco, sem bichinhos fofos, sem "amiguinhos".
- Combate de bonecos de massinha, **sem sangue**. Unidades "caem" e somem com o
  respingo curto de massinha já existente (`RunnerDeathEffects`). Classificação
  etária (IARC) será respondida na entrega do soft launch com evidência do
  código; **não presumir** o resultado [I].
- Ícone e nome: nada de olhos enormes nem sorrisos (lição da adultização).
  Ícone mostra a multidão contra a torre, não personagem fofo.

## 3. Loop central (30–90 s)

Alvo de duração: 30–90 s por fase [F: pesquisa, protótipo 1]. Divisão proposta
**[P]**:

| Bloco | Duração-alvo [P] | O que o jogador faz |
|---|---|---|
| 1. Corrida | 15–30 s | Arrasta para escolher a faixa; escolhe portões; desvia de obstáculos. Termina com N unidades. |
| 2. Assalto | 20–50 s | A multidão é lançada contra a torre; o jogador mira a faixa de lançamento e decide quando soltar cada onda (ver 4.2). |
| 3. Resultado | 5–10 s | Torre cai: vitória, recompensa, botão "de novo" ou "próxima". Torre sobrevive: derrota curta, "de novo" imediato. |

Princípios do loop **[I]**:
- Derrota nunca passa de 2 toques até "tentar de novo" (o critério do protótipo é
  % que pede "de novo" sem incentivo).
- A 1ª fase vence na 1ª tentativa para quase todos; a frustração só começa
  depois da 3ª fase (alinhado ao alvo da pesquisa: não frustrar antes da 3ª
  tentativa).
- Sessão média de poucas fases, para encaixar anúncios sem cortar o fluxo.

## 4. Mecânicas

### 4.1 Corrida e portões (reuso do Wild Pack)

Mesma lógica do motor [F: SISTEMA.md 2.3]: portões `+ − × ÷` em pares, geração
por `RandomGateCourse.PlanCourse` com alvo de melhor contagem e pares bônus/
castigo, obstáculos móveis e armadilhas fixas que matam por toque, moedas.
Diferenças propostas **[I]**: percurso **mais curto** que o do Wild Pack
(8 pares entre Z=15 e Z=120 hoje [F]; aqui ~3–5 pares **[P]**) porque o ponto
alto agora é o assalto; sem requisito de vitória na chegada (a chegada abre o
assalto com o que sobrar; zero unidades = derrota).

### 4.2 Lançamento da multidão (o coração novo)

Duas variantes de controle a testar no protótipo **[I]**; o protótipo decide:

- **L1 — "Soltar em ondas":** a multidão chega à linha de tiro; o jogador
  arrasta para escolher a faixa e **segura para lançar** um fluxo contínuo;
  soltar pausa o fluxo. Unidades em voo seguem pela faixa até a torre.
- **L2 — "Avalanche":** um toque lança toda a multidão de uma vez na faixa
  escolhida; mais simples, menos decisão.

Regras comuns **[P]**: unidade em voo não colide com unidade em voo (só com
portões do assalto, torre e defesas) para custar pouco de CPU; cada acerto na
torre tira vida fixa por unidade; unidade que acerta some (volta ao pool).

**Portões também no assalto:** uma ou duas fileiras de portões ficam entre a
linha de tiro e a torre, e o jogador pode **redirecionar a faixa** para passar
por eles (ex.: faixa A tem `×2`, faixa B tem `÷2` e menos defesa). Isso é onde a
decisão de assalto vive [I].

### 4.3 A torre

Campos propostos **[P]**: vida total, número de "andares" (fases de vida),
defesas e projéteis.

- **Vida:** barra visível, número grande (legibilidade em 5 e 120 unidades,
  mesmo princípio da direção de arte atual [F: SISTEMA.md 3.1]).
- **Defesas (tipos candidatos [I]):**
  1. *Canhão de lança-pedra:* projétil em arco que mata um grupo pequeno no
     ponto de queda; sombra/alvo avisa o impacto 0,5–1 s antes (tempo [P]).
  2. *Muralha:* bloqueia uma faixa até ser derrubada (vida própria).
  3. *Varredor:* barra que cruza a faixa em ciclo; passar na hora certa evita
     perda.
  4. *Escudo:* a torre ignora parte do dano até o jogador derrubar o gerador
     lateral.
- **Projéteis:** todos pré-alocados em pool (regra 2.2.1 do motor: nunca
  Instantiate/Destroy em runtime) [F]. Limite de projéteis simultâneos é
  parâmetro de dados, não constante solta.
- **Torres por mundo:** cada mundo tem silhueta e conjunto de defesas próprios
  (ver 7).

### 4.4 Chefes

Chefe = torre especial a cada fim de mundo **[I]**: mais andares, uma mecânica
própria (ex.: a torre **troca o portão bom por ruim** ao ser atingida) e
recompensa maior. Máximo de um chefe por mundo para conter conteúdo.

### 4.5 Fim de fase

Vitória: torre sem vida. Derrota: multidão zerada antes da torre cair.
Estrelas **[P]**: por unidades que sobraram, para dar motivo de rejogar sem
exigir analytics para balancear.

## 5. O "gancho novo" (diferencial em relação ao Mob Control)

Premissa: Mob Control é o líder do gênero com publisher grande [F]; competir
como clone é o maior risco do projeto (ver `riscos-e-decisoes.md`). O gancho
precisa ser **sentido nos primeiros 30 s** e caber no motor que já existe.

> Do que sabemos de Mob Control só há o que a pesquisa trouxe (números de
> mercado). **Não verificamos mecânica por mecânica o que ele ou clones já
> fazem**; cada opção abaixo precisa de uma varredura de loja de 1–2 h antes do
> protótipo (tarefa RT-0 do plano) para confirmar que o gancho não existe já.

### Opção A — "Corra, depois empurre" (corrida real que vira exército)

O exército é **construído por uma corrida de verdade** (o jogo que o estúdio já
sabe fazer: líder, manada que o segue, portões imprevisíveis, perigos) e só
depois é lançado. A corrida do primeiro bloco **decide a quantidade E a
composição** (ex.: colher moedas/itens na pista dá unidades especiais).
- *Por que diferencia [I]:* o jogador sente duas fantasias num jogo só (correr
  e invadir); a manada tem personalidade porque foi "viva" na pista.
- *Reuso:* ~100% da corrida; novo: assalto e torre. Menor risco de custo.
- *Risco:* pode parecer "Wild Pack + torre" (sem diferença sentida frente ao
  Wild Pack é o critério de descarte da pesquisa).

### Opção B — "A torre joga de volta" (portões e perigos vivos no assalto)

Durante o assalto, a **torre manipula os portões**: lança portões ruins na
faixa, fecha uma faixa e abre outra, e o jogador reage. O assalto vira duelo,
não só despejo de multidão.
- *Por que diferencia [I]:* decisão contínua; mais "esporte" do que "idle".
- *Reuso:* portões e geração por dados ajudam; novo: IA da torre e telegrafia.
- *Risco:* equilíbrio difícil sem analytics; mais carga cognitiva no celular e
  mais custo de CPU (portões dinâmicos).

### Opção C — "A multidão vira escada" (torre vertical, física de empilhar)

Em vez de dano por acerto, as unidades **empilham e escalam** a torre; quem
chega ao topo vence. Silhueta de torre vira o fator de jogo.
- *Por que diferencia [I]:* visual novo e imediato.
- *Reuso:* baixo; exige física/empilhamento 3D.
- *Risco:* alto (desempenho com 300 corpos em empilhamento, cânone do gate 3×;
  tempo de P&D). **[I]**: provavelmente não cabe no calendário.

### Recomendação: **Opção A**, com **um traço da B só nos chefes**

Justificativa **[I]**:
1. É a única que usa o ativo mais forte e já medido do estúdio (corrida de
   multidão com portões; gate 3× aprovado no OPPO [F: SISTEMA.md 5.4]).
2. Cabe no calendário (proto fev/2027) com 1 desenvolvedor.
3. A B, em versão leve, aparece só em chefes (a torre troca portão), onde o
   custo é limitado a poucos níveis.
4. A C tem o maior risco técnico e de calendário.

O que **invalida** a recomendação: se no teste do protótipo 3 de 5 testadores
descreverem o jogo como "igual ao Wild Pack" ou "igual ao Mob Control" na
pergunta "com que jogo isso se parece?". Nesse caso testar a B no mesmo
protótipo antes de descartar o projeto.

## 6. Progressão e meta

Tudo local (sem backend) [F: sem login, sem servidor — SISTEMA.md 1 e
`docs/store/declaracoes.md`].

- **Níveis:** sequência de fases por mundo; fases geradas por dados
  (`ChallengeDifficultyProfile`-like) com torres escolhidas de um catálogo
  [I]. Pesquisa cita ~30–50 fases geradas por dados para o candidato [F: "[I]"
  dela]; nosso alvo de lançamento é parâmetro do plano.
- **Upgrades (moeda das fases):** poucos e legíveis **[I]**: unidade inicial
  (começa com +N), dano por unidade, valor máximo de portão `+`, velocidade do
  lançamento. Cada upgrade tem 5–8 degraus **[P]**.
- **Unidades/cartas:** coleção de **tipos de unidade** (ex.: Pesada: mais
  dano e menos velocidade; Rápida; Escudeira: absorve um projétil). Entram por
  moeda ou por chefe, equipam-se 1–2 no loadout. Skins do roster atual
  reaproveitam o sistema de roster/skins existente [F: ThemeCharacterRoster].
- **Mundos:** cada mundo = tema (`GameTheme`-like) com torre, defesas e
  paleta próprias. Lançamento com poucos mundos **[P]**; mais mundos pós-
  lançamento como conteúdo (retenção).
- **Meta de longo prazo [I]:** completar mundos, fechar a coleção, subir upgrades
  até o teto; sem energia nem espera forçada.

## 7. Economia e monetização ética

Premissa [F]: o estúdio hoje só tem anúncio recompensado (AdMob + UMP), sem IAP
no app, sem analytics, sem backend. A pesquisa avisa que o dinheiro do gênero
vem de anúncio **e** IAP e que **não achou dado de receita por anúncio por
gênero** (a incerteza principal). Tudo abaixo é desenho, não previsão de receita.

### 7.1 Princípios (sem dark patterns)

- Anúncio recompensado é **escolha do jogador**, sempre com o ganho explicado
  antes de tocar.
- Nada de anúncio logo no 1º minuto, nem no meio de uma fase, nem depois de
  derrota imediata sem botão de recusa claro.
- Sem contagem regressiva falsa, sem "quase ganhou, veja um anúncio para
  continuar" disfarçado de obrigação, sem botão de fechar escondido, sem
  moeda premium ofuscada, sem loot box pago, sem compra por pressão.
- Sem notificações push; sem coleta de dados além do declarado (política atual:
  AdMob + UMP) [F].

### 7.2 Anúncio recompensado (centro do modelo)

Usos propostos **[I]**: dobrar a moeda da fase; continuar do ponto da derrota
**uma vez por fase** (devolve parte da multidão); girar a recompensa do chefe;
reforço no início da próxima fase. Todos opcionais.

### 7.3 Anúncio intersticial (com limite)

Proposta **[I]**: só entre fases (nunca durante), **depois de um mínimo de
fases jogadas** e **com intervalo mínimo de tempo** entre dois; desligado por
quem comprou "remover anúncios". Valores de mínimo e intervalo são **[P]** e
decisão do usuário (ver pendências). Intersticial é mecânica nova para o
estúdio (hoje só há recompensado [F]) e exige nova declaração/uso do SDK.

### 7.4 IAP e passe

- **Remover anúncios (IAP único):** remove intersticiais; **mantém** os
  recompensados opcionais (são benefício do jogador) [I]. Introduz IAP no
  estúdio: exige Play Billing, mudança em Data safety/ficha e política [I],
  decisão do usuário.
- **Passe opcional (temporada):** trilha de recompensas cosméticas e de
  conveniência; **nunca** poder exclusivo que quebre a progressão (sem
  pay-to-win) [I]. Exige conteúdo recorrente, então só depois do lançamento e
  só se a operação sustentar. Não prometer receita: sem dado.
- **Proibido por regra do projeto:** cassino/gambling, e qualquer coisa que
  caiba na regra "não infantil" da seção 2.

### 7.5 Referência de proporção [F, com ressalva]

Mob Control: ~metade da receita vem de anúncio e metade de IAP (Udonis/AppMagic,
2026-07-02). Isso é dado de **um** líder e não vale como previsão para um estúdio
sem UA paga e sem IAP no início.

## 8. Retenção

Premissa: **hoje missões diárias, login e analytics estão "fora do escopo até
decisão contrária"** [F: SISTEMA.md 1, Fora do escopo]. O que segue **depende de
o usuário reabrir esse escopo**.

- **Recompensa diária** (moeda ou unidade) com ciclo simples; sem punição por
  faltar [I].
- **Missões diárias leves** (3 por dia, locais, sem servidor) [I].
- **Eventos temporários** [I]: torres em desafio por tempo (ex.: fim de
  semana), com recompensa cosmética. Requer relógio local + data de corte
  embarcada **ou** backend; sem backend, só eventos pré-programados dentro do
  APK, o que é limitado e **a decidir**.
- **Chefes semanais** [I]: mesmo conteúdo, com modificador.
- **Alvos de retenção de soft launch:** D1 ≥ 35% e D7 ≥ 12% (meta do pedido;
  ver `plano-de-entregas.md`). **Referência [F]:** Mob Control D1 36,1%, D7 13,9%,
  D30 4,7% (AppMagic via Udonis, 2026-07-02). Meta de D30 ≥ 5% citada pela Azur
  para hybrid [F: PocketGamer.biz, 2026-04-28] e exige ~500 níveis + eventos —
  **fora do alcance do nosso 1º ano** [I]; D30 não é meta de lançamento.
- **Sem dado de retenção, nada disso se valida** (a pesquisa já alertou) →
  decisão de analytics é bloqueante para o soft launch.

## 9. Direção de arte e áudio

- **Arte [F] — direção "crowd toon" (aprovada em 2026-11-21; série RT-6t, fechada na RT-6t.12):**
  a multidão é feita de bonecos low-poly instanciados (formas **Bolinha** e **Pintinho**, escolhidas pelo
  jogador, 200–500 triângulos cada), com sombra toon de 3 faixas, sombra de disco no chão, quique e
  achatamento procedurais, nascimento elástico e morte balística (sem esqueleto, sem GameObject por boneco);
  até 300 bonecos (D27) desenhados em uma chamada por malha. Cada mundo tem uma **paleta** como dado (céu
  degradê em espaço de tela, neblina, chão, pista listrada, multidão, cenário e cores das estruturas):
  Pastel, Pôr do sol, Doce e Aurora. Cenário lateral instanciado e reciclado (árvores e pedras), pista com
  listras procedurais, sem sombras reais nem pós-processamento. Portões semitransparentes verde/vermelho com
  número em **Baloo 2** (OFL 1.1) com contorno, número flutuante e pop do contador. Detalhes:
  [`direcao-visual-crowd.md`](direcao-visual-crowd.md) e [`rt6t-crowd-toon.md`](rt6t-crowd-toon.md).
- **Motor do visual no kit [F]:** shaders toon, paletas, construtor de malhas, simulação instanciada (Burst),
  câmera, cenário instanciado e a rota de desenho de reserva para OpenGL ES ficam no `studiokit` desde a
  0.5.0 (K.11), como assemblies opcionais (`JungleDegree.StudioKit.Toon` e `.Swarm`). As receitas das formas,
  as paletas, os portões e a arte das torres são deste jogo. A massinha antiga (boneco do Blender, respingo)
  não é mais usada por este jogo; continua no kit para o Wild Pack.
- **Torres e chefes [F]:** arte toon própria (D28 = A): estruturas em `ToonLit` com a cor das 4 cores de
  estrutura/defesa da paleta do mundo; uma torre e um chefe por mundo; precisam ler bem em 3 faixas e a
  poucos pixels.
- **Áudio [F]:** o Wild Pack já tem SFX e trilhas CC0, gerenciador com vozes
  limitadas, mudo e vibração como pílulas e dados no tema (SISTEMA.md 7.35;
  `docs/audio-licenses.md`). Para este jogo o áudio é **mais central**: impacto
  na torre, queda da torre, aviso do projétil, chefe. Mesmo regime: **só fontes
  CC0 ou licença comercial gratuita**, sem serviço pago sem aprovação do
  usuário.
- **Vibração:** impacto de projétil e queda da torre, padrão já existente em
  dados (`GameHaptics`).

## 10. Plataforma e escopo técnico

Android, retrato, Unity 6 (6000.6.7f1), URP, aparelho de referência OPPO
CPH2791, **gate de folga 3×** (CPU principal p95 ≤ 11 ms, GPU p95 ≤ 11 ms,
PSS ≤ 450 MB) [F: SISTEMA.md 2.1 e 5.4]. Detalhes em `reuso-tecnico.md`.

## 11. Fora do escopo (v1)

Multiplayer, PvP, servidor próprio, login, conteúdo baixável, geração de
conteúdo por IA no app, editor de fases, iOS (decisão separada), cassino.
