# Runner vs Tower — riscos e decisões pendentes

Data: 2026-11-19. **[F]** = fato com fonte (pesquisa de 2026-11-18, `SISTEMA.md`,
código); **[I]** = inferência nossa. Nenhuma decisão abaixo foi tomada. Este
documento não é aconselhamento jurídico: o que toca propriedade intelectual
precisa de revisão do usuário (e de advogado se houver dúvida real).

---

## 1. Riscos

Escala: probabilidade e impacto em Baixa/Média/Alta, **julgamento nosso [I]**,
sem medida.

### R1 — Clone percebido (Mob Control e similares)

- **Fato [F]:** Mob Control é do Voodoo, 302,1 mi de downloads totais, 81,6 mi
  em 2025, MAU máximo da série em abr/2026 (Udonis/AppMagic, 2026-07-02). A
  pesquisa já descreve o líder como "dono" do gênero e a saturação como média-
  alta. Count Masters passa de 100 mi de downloads no Play (listagem, baixa
  confiança).
- **Risco [I]:** jogador e loja compararem com o líder e preferirem o líder;
  ASO e UA mais caros contra marcas fortes. Probabilidade **Alta**, impacto
  **Alto**.
- **Mitigação:** gancho próprio (GDD 5, Opção A), arte e identidade próprias
  (massinha adulta, não neon de Voodoo), nome sem "Mob", "Count" ou "Crowd Run"
  genéricos; **critério de descarte** no portão RT-3 (3 de 5 testadores citam
  "igual a…"); varredura de loja em RT-0.
- **Gatilho de parada:** o protótipo falha o portão (ver plano RT-3).

### R2 — IP e semelhança com Mob Control

- **Fato [F]:** não há análise jurídica no repositório; a pesquisa só trata de
  mercado. Mecânicas de jogo em geral não são protegidas por direito autoral, mas
  **arte, nome, ícone, interface, sons, textos e a "aparência total" são**, e a
  Play tem política contra imitação e contra ficha que induz o usuário a
  confundir [I, a conferir na Central de Políticas; minha memória, não verifiquei
  nesta sessão].
- **Risco [I]:** denúncia por semelhança, remoção da ficha, perda de conta (a
  mesma conta publica o Wild Pack). Probabilidade **Média-baixa** se a identidade
  for própria; impacto **Alto** (a conta é única).
- **Mitigação:**
  1. **Não copiar** assets, nomes de modos, tipos de torre ou telas de nenhum
     jogo existente; referência só como categoria.
  2. Arte própria no pipeline de massinha do estúdio; todo asset externo só CC0
     com licença arquivada [F: SISTEMA.md 4.1].
  3. Nome, ícone e capturas diferentes **de propósito** (varredura em RT-0 e
     de novo em RT-9).
  4. Marcas registradas: pesquisar o nome final nas bases oficiais antes de
     comprar domínio, arte e ficha (decisão D1).
  5. Usar mecânicas genéricas do gênero (portões, multiplicar, lançar) e não
     imitar o conjunto específico de um título.
- **Nota:** o "Runner vs Tower" é só **nome de trabalho** e nunca vai à loja
  sem passar pela varredura de marca.

### R3 — Desempenho no celular fraco

- **Fatos [F]:** gate 3×: CPU principal p95 ≤ 11 ms, GPU p95 ≤ 11 ms, PSS ≤
  450 MB; medido com 300 corredores a ~7–10,5 ms de CPU (SISTEMA.md 5.4, 7.35).
  Não há aparelho fraco de referência; o alvo é Android 8+, ARM64, ~3 GB de RAM
  (decisão de 2026-11-16). O pico de 300 só existe no início da corrida do
  Wild Pack (7.35); neste jogo o assalto é o pico.
- **Risco [I]:** o assalto estoura a CPU principal; a margem é pequena (~1 ms).
  Probabilidade **Média**, impacto **Alto** (o jogo é inviável em aparelhos de
  entrada).
- **Mitigação:** movimento em lote de unidades em voo, sem física, projéteis em
  pool, limites por dados (reuso-tecnico.md 6.2 e 6.3); **medição no protótipo
  antes de arte**; se falhar, reduzir limites, não o gate.
- **Gatilho de parada:** protótipo não fecha o gate com no mínimo um número de
  unidades em voo que o jogo ainda considere "multidão" — **definido no RT-2
  (2026-11-20, T-RT2-6): 150 em voo**, a confirmar no RT-3; inferência.

### R4 — Monetização sem dados

- **Fatos [F]:** a receita citada nas fontes é IAP; a pesquisa **não achou**
  dado público de receita de anúncio por gênero, que é o que mais importa ao
  estúdio. Hoje o estúdio não tem IAP, intersticial, analytics nem backend. CPM
  de LatAm é bem menor que o de América do Norte (Adjust, 2026-04-15 [F]).
- **Risco [I]:** o jogo cair em D1/D7 aceitáveis mas não render o suficiente
  para justificar o ciclo; monetizar demais e derrubar a retenção; monetizar de
  menos e não cobrir o custo. Probabilidade **Alta** (incerteza estrutural),
  impacto **Médio**.
- **Mitigação:** monetização ética e **leve no início** (recompensado opcional,
  intersticial com limite, remover anúncios); calibrar por dados no soft
  launch; ter meta de receita só **depois** de medir eCPM real do AdMob (a
  conta do Wild Pack já gera esse dado; usar o aceite e o eCPM reais do Wild Pack
  como primeira medida **[I]**).
- **Gatilho de parada:** após soft launch, se o eCPM real e o D7 não suportarem
  nenhuma estimativa razoável, decidir parar ou mudar de gênero (candidatos 2
  e 3 da pesquisa).

### R5 — Sem analytics (bloqueio de validação)

- **Fato [F]:** SISTEMA.md lista "missões diárias, login ou analytics" como fora
  do escopo; a política de privacidade do Wild Pack **declara não usar
  analytics**; pesquisa: "sem dado de retenção/aceite não se valida nenhuma
  hipótese".
- **Risco [I]:** o soft launch não consegue medir D1/D7 (as metas pedem
  D1 ≥ 35%, D7 ≥ 12%). Probabilidade **Alta** se nada mudar, impacto **Alto**.
- **Mitigação:** decisão D4 antes do RT-7; opções: (a) analytics mínimo e
  consentido, (b) usar só retenção por instalações ativas do Play Console
  (limitada), (c) soft launch com medição manual em grupo pequeno.

### R6 — Prazo e capacidade de uma pessoa

- **Fato [F]:** um desenvolvedor com Claude Code; pesquisa estima M (4–6
  semanas) só para o jogo mínimo [I dela].
- **Risco [I]:** a janela até 30/jun tem folga de poucos dias (plano de
  entregas); o Wild Pack ainda pode consumir tempo; a extração do kit (RT-1) é
  a etapa mais incerta. Probabilidade **Média-alta**, impacto **Médio**.
- **Mitigação:** cortar escopo (menos mundos, sem passe), alternativa de fork,
  aceitar lançamento em jul/2027 se necessário.

### R7 — Regras da Play e conta única

- **Fato [F]:** a pesquisa registra que não investigou os requisitos de teste
  fechado para contas novas; a conta de organização do Wild Pack está em
  criação (P1 pendente).
- **Risco [I]:** novo app depende do que o Wild Pack aprender com a conta; uma
  violação em um app pode afetar a conta inteira. Probabilidade **Média**,
  impacto **Alto**.
- **Mitigação:** revisar a Central de Políticas e o questionário IARC com
  evidência de código; nunca publicar ficha sem checklist; manter 13+ e
  anúncios sem tag infantil.

### R8 — Percepção "infantil" ou violência

- **Fato [F]:** adultização do Wild Pack foi motivada pelo risco de a Play
  entender que o app apela a crianças (`docs/adultizacao-ideias.md`).
- **Risco [I]:** combate de bonecos pode ser lido como violência e mudar a
  classificação; massinha fofa pode ser lida como infantil. Probabilidade
  **Média**, impacto **Médio**.
- **Mitigação:** sem sangue; direção adulta; responder o IARC com o código na
  mão; revisar o ícone e a ficha com a mesma lista da adultização.

### R9 — Escopo oficial "apps quase idênticos"

- **Fato [F]:** SISTEMA.md veda "publicar vários apps quase idênticos".
- **Risco [I]:** conflito de regra com o próprio estúdio; Runner vs Tower **não**
  é quase idêntico (mecânica de assalto, outra meta), mas depende de reler a
  linha. Probabilidade **Baixa**, impacto **Baixo** (decisão simples, D3).

### R10 — Dependência de áudio/arte CC0 e de ferramentas

- **Fato [F]:** áudio e arte vêm de pacotes CC0 já usados no Wild Pack;
  `docs/audio-licenses.md`.
- **Risco [I]:** faltar arte de torre/defesa/chefe em CC0 com a direção adulta;
  gerar arte própria demanda tempo. Probabilidade **Média**, impacto **Médio**.
- **Mitigação:** priorizar formas simples de massinha geradas no Blender (como o
  corpo atual); só usar assets externos com licença arquivada.

---

## 2. Decisões pendentes do usuário

Cada decisão tem **por que importa**, **prazo** (antes de qual entrega) e
**recomendação [I]**. Marcar a resposta e a data. Situação em 2026-11-20 (fim da
RT-0): D1–D6, D11 e D14–D16 decididas; D7–D10, D12 e D13 seguem pendentes, cada
uma antes da entrega indicada.

| # | Decisão | Por que importa | Antes de | Recomendação [I] |
|---|---|---|---|---|
| **D1** | **Nome do jogo** (e checagem de marca) | Nome de trabalho "Runner vs Tower" é genérico, difícil de proteger e parecido com títulos de gênero; domínio, ícone e ficha dependem dele | RT-4 (arte), definitivo antes de RT-9 | Escolher 3 candidatos próprios e verificar marca antes de investir em arte; nome sem termos do líder — **decidido em 2026-11-20: nome de trabalho "Claybreak"**; **trocado em 2026-11-20 para "Clay Siege"** (D18, `decisoes.md`; checagem de marca em andamento) (checagem oficial de marca ainda pendente antes de arte/ícone/domínio, RT-4/RT-9) |
| **D2** | **Gancho** (A, B ou C do GDD 5) | Define o protótipo | RT-2 | **A**, com o traço B só em chefes — **decidido em 2026-11-20: D** (adiar até a varredura); **resolvida em 2026-11-20: A** (após `docs/varredura.md`) |
| **D3** | **Reler a regra "apps quase idênticos"** e aprovar o segundo app | Regra do SISTEMA.md; sem decisão não há entrega | RT-0 | Aprovar: o jogo é diferente — **decidido em 2026-11-20: A** (aprovado, com checagem de política da Play e identidade antes do soft launch) |
| **D4** | **Analytics** (nenhum / mínimo consentido / outro) | Sem dado não há D1/D7; muda política de privacidade, UMP e Data safety; contraria o "fora do escopo" | RT-7 (antes: RT-3 se medir no teste) | Analytics **mínimo** (só D1, D7, aceite de anúncio), com consentimento, e escolha da ferramenta pelo usuário (**sem chamadas pagas sem aprovação**) — **decidido em 2026-11-20: A** (mínimo consentido + funil de fases, ferramenta gratuita escolhida antes da RT-7; ver `decisoes.md`) |
| **D5** | **Escopo de retenção:** recompensa diária, missões, eventos | SISTEMA.md veda "missões diárias, login ou analytics" | RT-7 | Reabrir só recompensa diária e missões locais; eventos só depois do lançamento — **decidido em 2026-11-20: B** (só recompensa diária local; missões só se D7 < 12%; eventos pós-lançamento) |
| **D6** | **Monetização aprovada para v1:** intersticial? IAP remover anúncios? passe? | Cada item adiciona SDK, declarações, política e teste | RT-7 | v1 com recompensado + intersticial com limite + remover anúncios; passe só pós-lançamento — **decidido em 2026-11-20: B** (recompensado + intersticial pronto e desligado na 1ª turma do soft launch; sem IAP na v1) |
| **D7** | **Conta Play e conta AdMob:** usar as do Wild Pack ou separar; DUNS; risco de conta única | O Wild Pack ainda está criando a conta (P1) | RT-9b (criar o app) | A: mesma conta de organização e mesma conta AdMob business; app e unidade de anúncio próprios — ficha completa abaixo (RT-9a, 2026-11-24) — **decidido em 2026-11-24: A** |
| **D8** | **Orçamento de UA e de ferramentas** (se houver) | Sem orçamento, o gênero (CPI alto, descoberta difícil [F]) exige ASO e orgânico; não há dado para dimensionar | RT-10 | Soft launch **sem** UA paga ou com valor pequeno e fixo, definido por você |
| **D9** | **Países do soft launch** — orientação do usuário em 2026-11-21: **Europa** (D23); falta escolher quais | CPM e CPI variam muito por país [F: Adjust 2026-04-15] | RT-9b (produção) / RT-10 | A: Portugal, Irlanda, Países Baixos, Suécia, Dinamarca, Noruega, Finlândia (E: os mesmos em teste aberto) — ficha completa abaixo (RT-9a, 2026-11-24) — **decidido em 2026-11-24: A** |
| **D10** | **Idiomas** | Ficha e textos | RT-9b (criar o app) | A: só inglês (en-US) até o jogo ser traduzido; a ficha em inglês está pronta (`store/ficha/en-US.md`) — ficha completa abaixo (RT-9a, 2026-11-24) — **decidido em 2026-11-24: A** |
| **D11** | **Extração do kit:** pacote local (A), fork (B) | Reuso técnico, 5.2 | RT-1 | **Decidido em 2026-11-19: A** (ver `decisoes.md`, K1) |
| **D12** | **Quantidade de conteúdo no lançamento** (mundos, fases) | Define RT-6 e o prazo | RT-5 (trava a RT-6) | **4 mundos × 10 fases (40)**; se apertar, 3 + 1 no soft launch — ficha em `decisoes.md` (RT-5) — **decidido em 2026-11-20: A** (4 mundos × 10 fases) |
| **D13** | **Quem são os 5 testadores** do protótipo (portão) | Sem testadores não há portão | RT-3 | Gente fora do estúdio, que joga celular, com alguns que não conhecem o Wild Pack — **decidido em 2026-11-20: dispensado** (RT-3 cancelada; o usuário segue sem teste externo; ver `decisoes.md`) |
| **D14** | **Plataforma:** só Android | iOS exige conta Apple, build e revisão | RT-0 | Só Android na v1 — **decidido em 2026-11-20: A** (só Android na v1) |
| **D15** | **Prioridade em relação ao Wild Pack:** se o Wild Pack precisar de atualização, quem ganha as semanas | Uma pessoa só | RT-0 | Wild Pack tem prioridade nas semanas de incidente; o plano desliza — **decidido em 2026-11-20: B** (os dois jogos no mesmo ritmo) |
| **D16** | **Critério de "parar":** aceitar os gatilhos dos riscos R1, R3, R4 | Evita insistir em jogo que não funciona | RT-0 | Aceitar — **decidido em 2026-11-20: A** (só o portão RT-3; critérios pós-lançamento ficam para antes do RT-9); **portão RT-3 dispensado em 2026-11-20** — sem gatilho de parada antes do RT-9 |
| **D17** | **Direção de arte da torre e do chefe** (opções A/B/C de cada, em `docs/art/rt4/`) | Arte final da fatia (RT-4) e modelo das próximas torres | aceite da RT-4 | Torre A (bastião de pedra clara) + chefe A (cidadela) — ficha em `decisoes.md`; o jogo usa A/A provisórias |
| **D18** | **Nome "Claybreak" depois da checagem de marca** (há o jogo "Claybreak Range"; INPI não conferido) | Ícone, ficha da loja e domínio dependem do nome | RT-9 | Seguir como nome de trabalho; o usuário confere INPI/TMview/USPTO à mão; dois nomes reserva checados na RT-9 — ficha em `decisoes.md` — **checagem gratuita de "Clay Siege" em 2026-11-20: sem conflito no acessível; INPI/USPTO/TMview/WIPO a conferir à mão antes da RT-9** |
| **D19** | **Preços e curvas da loja** (4 upgrades, 3 elites, 6 skins; moedas por fase) | Ritmo da meta; base para a RT-6 calibrar os mundos | aceite da RT-5 | Aprovar a forma como ponto de partida; total e pagamento dos mundos acertados na RT-6; recalibrar no soft launch — ficha em `decisoes.md`; o jogo roda com a tabela [P] — **decidido em 2026-11-20: A** (tabela atual como ponto de partida) |
| **D20** | **Arte dos mundos 2, 3 e 4** (paleta, torre e chefe de cada um) — **decidido em 2026-11-20: A** | Cara do jogo na loja e no soft launch | aceite da RT-6 (não trava código) | Recomendações A (paleta B no mundo 3) como provisórias; trocar é uma linha — ficha em `decisoes.md` (RT-6); o jogo roda com as recomendações |
| **D21** | **Ferramenta gratuita de analytics** (Firebase, GameAnalytics, Unity Analytics ou nenhuma) — **decidido em 2026-11-20: A (Firebase)** | D4 exige a escolha antes de instalar; sem ela o soft launch não mede D1/D7 nem o funil | RT-9 (não trava a RT-7/RT-8) | Firebase Analytics na configuração mínima (sem ID de publicidade, coleta liberada pelo jogo) — ficha em `decisoes.md` (RT-7); **implantado na RT-8b** (2026-11-21, `rt8b-firebase.md`); empresa portuguesa → medir só com consentimento em todos os países; fora da Europa/Reino Unido sem medição até a **D23** |
| **D22** | **Aprovar o som e a vibração** (aceite da RT-8) — **decidido em 2026-11-21: A** | Fecha o aceite da RT-8; vídeo e imagens da loja (RT-9) usam o som do jogo | antes da RT-9 gravar vídeo/imagens (não trava código) | A: aprovar o som próprio como está (já no jogo e no OPPO) — ficha em `decisoes.md` (RT-8) |
| **D23** | **Medir fora da Europa: com que pedido de consentimento** (empresa portuguesa: RGPD para todos) — **decidido em 2026-11-21: B** (testes e soft launch na Europa; rever antes de sair da Europa) | Sem resposta, o jogo só mede onde a janela do Google aparece (Europa, Reino Unido, Suíça) | Antes do soft launch (RT-9/RT-10) | E (janela do Google para todos, se o AdMob permitir) e A (pergunta própria) como reserva — ficha em `decisoes.md` (RT-8b) |
| **D25** | **Visual "fofo" (crowd toon): para quem o jogo é declarado na Play** (13+ com marca "não destinado a crianças", público misto, ajustes sem olhos) | Conflita com a direção adulta do GDD; ficha da loja e regras de Famílias | RT-6t.4 em diante (qual forma vai para o jogo); RT-9 (loja). A RT-6t.2 já deixou as duas variantes prontas (com e sem olhos; padrão provisório na `ToonLab`: com olhos) | A: adotar como está, 13+, loja compensando — ficha em `direcao-visual-crowd.md` seção 5 — **decidido em 2026-11-22: A** |
| **D26** | **Skins × 2 formas × cor do mundo** | Loja (D19), save, cor da multidão por paleta | RT-6t.6b (a 6t.6a segue sem ela) | A: formas livres; BARRO = cor do mundo; 8 skins com cores novas, mesmos preços — ficha em `direcao-visual-crowd.md` — **decidido em 2026-11-22: A** |
| **D27** | **Teto da multidão** (hoje 300; motor novo aguenta 1000) | Espetáculo × equilíbrio das 40 fases e peso do assalto no celular | RT-6t.4 (sem resposta: 300) | A: 1000 na corrida, até 300 em voo valendo mais — ficha em `direcao-visual-crowd.md` — **decidido em 2026-11-22: teto 300 no jogo, configurável no motor; portões não passam de 300** (revisão do usuário) |
| **D28** | **Arte do assalto no estilo novo** (torres, defesas, chefes, efeitos) | O protótipo não mostra o assalto | RT-6t.10 (sem resposta: A) | **decidido em 2026-11-22: A** (aplicada por padrão na RT-6t.10: sombreado toon + cores da paleta, sem remodelar, sem rostos; confirmada pelo usuário ao aprovar as capturas `docs/art/rt6t/chefes-*.png`) — ficha em `direcao-visual-crowd.md` |
| **D29** | **Nome "Clay Siege" sem massinha** | Ícone, loja e domínio; checagem de marca pendente | RT-9 | B: 3 nomes novos checados + "Clay Siege", escolha antes da RT-9 — ficha em `direcao-visual-crowd.md` — **decidido em 2026-11-22: Stampeep** (substitui "Clay Siege" antes da RT-9; busca oficial de marca antes de qualquer gasto) |
| **D31** | **Forma da multidão como compra na loja** (leitura do pedido do usuário de 2026-11-23; substitui o item 1 da D26) | Loja, save v5, tela inicial | RT-6t.20 (as 6t.16–6t.19 seguem sem ela) | A: formas compradas na loja (bolinha grátis, pintinho e 3 novas pagas; quem usa o pintinho fica com ele), silhuetas distintas na câmera da corrida como aceite — ficha em `plano-telas-obstaculos-loja.md` seção 5 — **decidido em 2026-11-23: A** |
| **D32** | **Preços das formas** | Economia da loja (D19), tempo para comprar tudo | RT-6t.20 (sem resposta: A provisório) | A: 300 · 1.000 · 1.600 · 2.400 (loja ~55–56 min) — ficha em `plano-telas-obstaculos-loja.md` seção 5 — **decidido em 2026-11-23: A** |
| **D33** | **Proteger o nome "Stampeep": marca e domínio** (gasto; não bloqueia) | Marca e domínio do nome escolhido na D29 | antes da RT-9; **vale do SME Fund 2026 só até 04/12/2026** | C agora + pedir o vale já; marca PT (9 e 41) e UE só com opinião de agente — ficha abaixo, escrita na RT-6t.21 (2026-11-23) — **pendente** |

### Ficha D33 — Proteger o nome "Stampeep" (gasto pendente, escrita na RT-6t.21; não bloqueia nenhuma entrega)

```text
DECISÃO D33 — Pagar para proteger o nome "Stampeep"?

O que você está aprovando: a Jungle Degree gasta <valor da opção> para <comprar o domínio
stampeep.com> e/ou <pedir o registo da marca "STAMPEEP" em <Portugal / UE>, nas classes
<9 e 41>>. A IA não paga nem pede nada: o pedido e o pagamento são feitos por você nos sites
oficiais (ou por um agente de propriedade industrial que você contratar).

Por que agora: nada no jogo trava sem isto (o nome já está no jogo desde a RT-6t.21). Mas há
um prazo que vale dinheiro: o fundo da UE para pequenas empresas (SME Fund 2026) devolve até
75% das taxas de marca (teto citado: 700 € por empresa), e as candidaturas fecham em
04/12/2026. O vale tem de estar aprovado ANTES de pedir a marca, a aprovação leva tempo e,
segundo uma fonte, o vale tem de ser usado em 1 mês (prorrogável por mais 1). É reembolso:
paga-se a taxa inteira e recebe-se depois. Pedir o vale é grátis e não obriga a registar.

Contexto que você precisa saber:
- Busca oficial gratuita em 2026-11-23 (EUIPO = marcas da UE; INPI = marcas de Portugal):
  nenhuma marca "STAMPEEP" nem quase igual. Domínio stampeep.com livre.
- Ponto de atenção: existe a marca "STAMPEDE" registada para jogos (Sumo Group, britânica,
  classes 9 e 41, de ~2023) e outras "X STAMPEDE". Em inglês "stam-PEED" e "stam-PEEP" soam
  quase iguais. Leitura nossa + revisor: não é conflito que impeça usar o nome, mas o risco de
  o dono se opor a um registo (ou reclamar do uso) é MÉDIO. Não pesquisámos Reino Unido, EUA
  nem o nome nas lojas (Google Play, App Store). Não é parecer jurídico.
- Classes: 9 = jogos e software para baixar; 41 = entretenimento e jogos online;
  28 = brinquedos e jogos físicos (só para merchandising, dispensável agora).
- Taxas oficiais (sites oficiais, 2026-11-23):
  - Marca da UE (EUIPO, 27 países, inclui Portugal): 850 € com 1 classe, +50 € a 2ª, +150 €
    cada uma a partir da 3ª (pedido online). 9+41 = 900 €. Renova a cada 10 anos.
  - Marca de Portugal (INPI): 145,56 € com 1 classe, +36,90 € por classe a mais, +12,31 € na
    concessão (online). 9+41 = 194,77 €. Pedido feito em Portugal dá 6 meses de prioridade
    para estender à UE ou a outros países com a mesma data.
  - SME Fund 2026: até 75% de volta, teto de 700 € (fontes secundárias; confirmar no site).
    Com o vale: UE 9+41 ≈ 225 € líquidos; Portugal 9+41 ≈ 49 € líquidos.
  - Domínio .com: atacado 10,97 US$/ano desde 01/11/2026; registradores cobram em geral
    11–20 €/ano (estimativa).
  - Agente oficial de propriedade industrial (opinião sobre o risco STAMPEDE): custo não
    orçado; inferência nossa: algumas centenas de euros.
- Já decidido: D29 (2026-11-22) escolheu "Stampeep" e pediu busca oficial antes de gastar.

Opções:
A) Domínio + vale já + marca da UE (9 e 41) — com o vale aprovado, pede na EUIPO.
   + protege os 27 países de uma vez; o vale devolve até 675 €.
   - Riscos: oposição da Sumo (chance: média; impacto: alto — perde a taxa e talvez troque o
     nome) — mitigar ouvindo um agente antes; vale não sair (média; médio — paga cheio).
   Custo: ~15 € + 900 € (≈ 225 € com o vale). Volta atrás? domínio sim; a taxa não.
B) Domínio + vale já + marca de Portugal (9 e 41); estender à UE dentro dos 6 meses de
   prioridade, depois de ouvir um agente (e depois do soft launch, se houver tração).
   + barato e rápido; guarda a data para a UE.
   - Riscos: só Portugal fica protegido até estender (chance de cópia fora: baixa; impacto:
     médio); a Sumo também pode se opor em Portugal (baixa-média; médio — ~195 € perdidos).
   Custo: ~15 € + ~195 € (≈ 49 € com o vale). Volta atrás? domínio sim; a taxa não.
C) Só o domínio agora; marca depois do soft launch (RT-10), se o jogo mostrar tração.
   + gasta ~15 €; não paga marca de um jogo que pode não vingar.
   - Riscos: alguém registar STAMPEEP antes (baixa; alto); perde o vale de 2026; o risco
     STAMPEDE continua sobre o USO do nome, registado ou não (média; alto se publicado).
   Custo: ~11–20 €/ano. Volta atrás? sim.
D) Adiar tudo.
   - Riscos: perder o .com (baixa; médio — usar outro domínio); mesmo risco de uso do C.
   Custo: zero. Volta atrás? sim.
E) Rever o nome antes de gastar — pedir a opinião de um agente sobre STAMPEDE primeiro e,
   se ele achar o risco alto, voltar à D29 (o rename no jogo é uma constante + reconstruir).
   + evita pagar marca e loja num nome contestável. - Riscos: atrasa; pode perder o vale.
   Custo: a consulta. Volta atrás? sim.

Recomendação: B, com o domínio e o pedido do vale já (esta semana). É o caminho mais barato
que guarda a data e deixa a UE para depois da opinião de um agente, dentro dos 6 meses.
Segunda opinião (fable): concorda em comprar o domínio e pedir o vale já (grátis, não obriga
a registar); acha o risco STAMPEDE médio, não baixo, e começaria pela marca portuguesa
(9 e 41, ~195 €), indo para a UE só depois de ouvir um agente, dentro dos 6 meses de prioridade.
Se você não responder: nada é comprado nem pedido; o jogo segue com o nome; o vale de 2026
expira em 04/12; a RT-9 traz esta ficha de volta antes de publicar a página da loja.

Responda: A, B, C, D ou E, ou a letra + ajuste (ex.: "B só classe 9"), ou "explica melhor".
```

Fontes: EUIPO taxas https://www.euipo.europa.eu/en/trade-marks/before-applying/fees-payments · INPI taxas (Declaração
447/2024) https://justica.gov.pt/en-gb/Registos/Industrial-Property/Trademark/How-much-does-it-cost-to-submit-a-trademark-or-other-signs-application
· SME Fund 2026 https://intellectual-property-helpdesk.ec.europa.eu/news-events/news/applications-are-open-2026-sme-fund-2026-02-10_en,
https://inpi.justica.gov.pt/en-gb/SME-Fund/SME-Fund-2026-EU-Fund-for-the-Protection-of-IP-Rights,
https://ipoi.gov.ie/en/news-events/news-categories-/announcements/sme-fund-2026-continuing-support-for-smes-now-available.html
· .com https://investor.verisign.com/news-releases/news-release-details/verisign-reports-first-quarter-2026-results ·
RDAP https://rdap.verisign.com/com/v1/domain/stampeep.com (404 = livre). Busca completa em `decisoes.md` (2026-11-23).

### Fichas D7, D9 e D10 (escritas na RT-9a, 2026-11-24; bloqueiam a RT-9b, não a RT-9a)

As três são do usuário (conta, países, idiomas: produto e coisas difíceis de desfazer). A RT-9a terminou tudo o resto
sem elas: a ficha da loja está em inglês, as declarações valem para qualquer resposta, e o passo a passo da RT-9b diz
onde cada resposta entra.

```text
DECISÃO D7 — Em que conta publicar o Stampeep (Google Play e AdMob)?

O que você está aprovando: o Stampeep é publicado na mesma conta de programador Google Play
de organização da JUNGLE DEGREE, LDA (a do Wild Pack) e os anúncios usam a mesma conta
AdMob da empresa (a "business", não a pessoal), com um app AdMob e uma unidade de anúncio
premiado próprios do Stampeep.

Por que agora: é o primeiro passo da RT-9b (criar o app no Console). Sem esta resposta não
há onde criar o app nem de onde tirar os códigos de anúncio reais do jogo.

Contexto que você precisa saber:
- A conta Play de organização JUNGLE DEGREE, LDA já existe, está totalmente verificada e já
  publicou o Wild Pack em teste interno (2026-11-21). Fato (SISTEMA.md do Wild Pack, P1).
- A regra do teste fechado (12 pessoas durante 14 dias antes da produção) é escrita só para
  contas pessoais criadas depois de 13/11/2023 (fato: ajuda oficial da Play, lida em
  2026-11-24). Que ela não vale para a conta de organização é dedução nossa: a página não
  fala de organizações; confirmar no Console ao pedir produção.
- Na loja aparecem, para conta de organização: nome legal, D-U-N-S (número da empresa na
  Dun & Bradstreet), morada, e-mail e telefone do programador. Fato (ajuda da Play).
- Uma conta nova custaria a taxa única da Play (25 US$, memória não verificada) e outra
  verificação de identidade; uma segunda conta de organização exigiria outra empresa (outro
  D-U-N-S). O que a conta atual tem de valor é a verificação feita, não histórico de loja:
  o Wild Pack só chegou ao teste interno. Inferência nossa.
- Risco de conta única: se um app levar uma violação grave de política, o Google pode
  suspender a conta inteira, com os dois jogos; e as regras da Play preveem encerrar
  também contas relacionadas ao mesmo dono (memória, não verificado hoje) — separar não
  isola de verdade. Chance para nós: baixa (sem SDK estranho, declarações com evidência).
- AdMob partilhado: tráfego inválido (amigos ou testadores a tocar em anúncios reais) pode
  limitar ou suspender a conta AdMob inteira, com os dois jogos. É mais provável que uma
  violação da Play. Inferência do revisor, plausível.
- O app AdMob do Wild Pack ficou "Requer revisão / Veiculação limitada" até ser ligado à
  loja (fato, SISTEMA.md do Wild Pack); o do Stampeep deve passar pelo mesmo: anúncios
  reais podem não aparecer no teste interno.
- AdMob: a conta business (pub-4219609499469762) já tem o Wild Pack. Um app AdMob por jogo
  separa relatórios e revisão. Os códigos do AdMob são públicos (ficam dentro do app).
- Decisões já tomadas que isto afeta: D3 (segundo app aprovado), D6 (só premiado na v1,
  intersticial desligado), D21/D23 (Firebase, consentimento).

Opções:
A) Mesma conta Play de organização + mesma conta AdMob business, app e unidade próprios.
   + zero custo, conta já verificada, sem espera; tudo no mesmo login.
   - Riscos: suspensão de conta derruba os dois jogos (chance: baixa; impacto: alto) —
     mitigar com declarações honestas e nunca publicar sem o passo a passo; tráfego
     inválido no AdMob atinge os dois (chance: baixa-média; impacto: alto) — nunca tocar
     em anúncios reais próprios, testar com o OPPO registado como aparelho de teste; a
     IA confere que os IDs começam pela conta business (pub-4219609499469762), erro que já
     aconteceu no Wild Pack.
   Custo: zero. Dá para voltar atrás? Mudar de conta depois exige transferir o app
   (processo do Google, possível mas burocrático).
B) Conta Play pessoal nova só para o Stampeep; AdMob igual a A.
   + isola o risco de suspensão.
   - Riscos: conta pessoal nova cai na regra dos 12 testadores por 14 dias (chance: certa;
     impacto: médio — +2 semanas e 12 pessoas); a loja mostraria o seu nome pessoal, não
     a empresa (inferência nossa); o Google pode ligar as contas pelo mesmo dono de
     qualquer forma.
   Custo: taxa da conta + dias de verificação. Dá para voltar atrás? com custo.
C) Mesma conta Play (A), mas conta AdMob separada.
   + separa a receita.
   - Riscos: outra conta AdMob para a mesma empresa pode ser recusada ou ligada pelo Google
     (chance: média; impacto: médio); mais pagamentos e impostos a gerir.
   Custo: tempo. Dá para voltar atrás? sim.

Recomendação: A — é o que a tabela de decisões já recomendava desde a RT-0; a conta
verificada é um ativo, e o isolamento de B custa semanas sem tirar o risco de fato.
Segunda opinião (fable): concorda com A: a conta verificada já existe e separar não isola
de fato, porque o Google encerra contas relacionadas; pediu para juntar o risco de tráfego
inválido no AdMob partilhado e confirmar no Console se há exigência de teste fechado
(feito acima).
Se você não responder: a RT-9b não começa; nada mais trava (o jogo continua como está).

Responda: A, B ou C, ou a letra + ajuste, ou "explica melhor".
```

```text
DECISÃO D9 — Em quais países europeus fazer o soft launch

O que você está aprovando: o Stampeep fica disponível na Google Play, no soft launch
(primeira publicação aberta, para medir antes do lançamento), só em: Portugal, Irlanda,
Países Baixos, Suécia, Dinamarca, Noruega e Finlândia. Os outros países ficam fechados
até a RT-11.

Por que agora: o Play Console pede os países ao preparar a produção (RT-9b/RT-10). O teste
interno não precisa disto; a publicação aberta precisa.

Contexto que você precisa saber:
- Você já orientou (2026-11-21, D23 = B): testes e soft launch só na Europa; rever antes de
  sair dela. Fato (decisoes.md). Motivo técnico: o pedido de consentimento do Google (UMP)
  só aparece no Espaço Económico Europeu (EEE), Reino Unido e Suíça; fora disso o jogo não
  mede nada e o anúncio não pergunta antes (decisão técnica T-RT9a-3).
- O jogo está só em inglês (menus e textos). Fato (código). Países onde muita gente lê
  inglês dão um teste mais justo. Inferência nossa (sem estudo citado).
- Soft launch = publicar para poucos países, medir D1 e D7 (% de jogadores que voltam 1 e 7
  dias depois de instalar; metas D1 ≥ 35%, D7 ≥ 12%) e ajustar antes do lançamento.
- Sem dinheiro para anúncios de instalação (D8 pendente; recomendação: sem gasto), as
  instalações vêm só da loja e de divulgação gratuita: devem ser poucas dezenas a
  centenas. Inferência nossa. Com isso, em QUALQUER opção abaixo, D1 ≥ 35% e D7 ≥ 12%
  provavelmente não serão medidos com confiança (o Firebase ainda esconde números muito
  pequenos; registrado na D21). Os países pesam menos que a divulgação; vale decidir a
  D8 (orçamento) junto com esta.
- A política de privacidade está em inglês e português; nos nórdicos e Países Baixos fica
  em inglês, coerente com o jogo em inglês.
- Antes de abrir a produção, a Play revê o público-alvo (pode levar 7 dias ou mais). Fato
  (ajuda da Play).
- O preço do anúncio (CPM) e o custo por instalação variam muito por país (Adjust,
  2026-04-15, citado na RT-0); os nórdicos e a Irlanda costumam pagar mais por anúncio que
  Portugal. Inferência a partir dessa fonte.
- Portugal é o país da empresa (rede de contactos para pedir instalações). Reino Unido e
  Alemanha/França ficam de fora para não "gastar" os maiores mercados europeus antes do
  jogo estar ajustado. Inferência nossa (prática comum de soft launch).

Opções:
A) Portugal + Irlanda + Países Baixos + Suécia + Dinamarca + Noruega + Finlândia (7).
   + muito inglês, mercados ricos e pequenos; Portugal para a sua rede de contactos.
   - Riscos: poucas instalações orgânicas (chance: alta; impacto: médio — números
     fracos) — mitigar com divulgação gratuita em Portugal; amostra puxada por conhecidos
     (chance: alta; impacto: médio); conhecidos a tocar em anúncios = tráfego inválido
     (ver D7; pedir que não toquem em anúncios por cortesia).
   Custo: zero. Dá para voltar atrás? sim, países mudam a qualquer momento.
B) Só Portugal.
   + o mais simples; você controla quem instala.
   - Riscos: amostra ainda menor e enviesada para conhecidos (chance: alta; impacto: médio).
   Custo: zero. Dá para voltar atrás? sim.
C) Todo o EEE + Reino Unido + Suíça (~32 países).
   + mais instalações orgânicas.
   - Riscos: queima a primeira impressão nos mercados grandes (Reino Unido, Alemanha,
     França) com um jogo ainda por ajustar (chance: média; impacto: médio); jogadores que
     não leem inglês (impacto: baixo).
   Custo: zero. Dá para voltar atrás? sim, mas avaliações e primeira impressão ficam.
D) Adiar — escolher depois do teste interno.
   - Riscos: nenhum até a RT-10; só atrasa a preparação da produção.
E) Os mesmos países de A, mas em "teste aberto" em vez de produção.
   + a ficha aparece como acesso antecipado; as opiniões dos jogadores vão só para você,
     não ficam públicas (memória, não verificado hoje): tira o risco de primeira impressão.
   - Riscos: menos visibilidade na loja que a produção (chance: alta; impacto: baixo com
     poucas instalações de qualquer forma); passar depois à produção é mais um passo.
   Custo: zero. Dá para voltar atrás? sim.

Recomendação: A — mantém a Europa (D23), joga em inglês onde se lê inglês e guarda os
mercados grandes para o lançamento. E (os mesmos países em teste aberto) é a variante a
escolher se você preferir que nenhuma avaliação pública fique antes do lançamento.
Segunda opinião (fable): concorda com A, com ressalva: sem verba (D8) as instalações serão
poucas em qualquer opção e o D1/D7 será ruído, então os países importam menos que a
divulgação; sugeriu a opção E (teste aberto, sem avaliações públicas), incluída acima.
Se você não responder: a RT-9b faz o teste interno (não precisa de países) e para antes de
preparar a produção.

Responda: A, B, C, D ou E, ou a letra + ajuste (ex.: "A sem a Noruega"), ou "explica melhor".
```

```text
DECISÃO D10 — Em que idiomas fica a página da loja

O que você está aprovando: a página do Stampeep na Google Play fica só em inglês (en-US)
no teste interno e no soft launch; traduções da página entram quando o próprio jogo for
traduzido.

Por que agora: o Play Console pede o idioma padrão ao criar o app (primeiro passo da
RT-9b). O texto em inglês já está pronto e conferido nos limites da Play.

Contexto que você precisa saber:
- O jogo está só em inglês (menus, botões, textos). Fato (código). A política de
  privacidade está em inglês e português de Portugal.
- O Wild Pack usa inglês (en-US) e português do Brasil (decisão P6). Fato. A tabela de
  decisões recomendava "os mesmos do Wild Pack". Mas o Stampeep começa pela Europa (D23),
  onde o português é o de Portugal, e o jogo não está traduzido.
- Uma página em português de um jogo só em inglês pode decepcionar quem instala e gerar
  avaliações negativas. Inferência nossa.
- Sem tradução, a Play mostra a página em inglês em todos os países (comportamento padrão;
  a Play pode oferecer uma tradução automática ao leitor — memória, não verificado).
- Em Portugal, sem página em português, o jogo aparece menos em pesquisas em português.
  Impacto baixo: as instalações portuguesas devem vir pelo link que você partilhar.
  Inferência nossa.
- A IA traduz a página sem custo quando você pedir (é só texto); o idioma padrão escolhido
  ao criar o app não é fácil de trocar depois (memória não verificada; trocar o padrão é
  possível no Console, mas mexe em todas as traduções).

Opções:
A) Só inglês (en-US) por agora; traduzir a página junto com o jogo.
   + coerente com o jogo; nada a manter em dois idiomas.
   - Riscos: em Portugal a página fica em inglês (chance: certa; impacto: baixo).
   Custo: zero. Dá para voltar atrás? sim, adicionar idiomas é fácil.
B) Inglês (padrão) + português de Portugal (pt-PT) na página, avisando "jogo em inglês".
   + página na língua do país da empresa.
   - Riscos: expectativa de jogo traduzido, atenuada pelo aviso (chance: baixa-média;
     impacto: baixo); cada mudança futura da página tem de ser feita nos dois idiomas
     (chance: certa; impacto: baixo).
   Custo: zero (a IA traduz). Dá para voltar atrás? sim.
C) Inglês + pt-PT + pt-BR (como o Wild Pack).
   + pronto para o Brasil no lançamento.
   - Riscos: o Brasil não está no soft launch (D23/D9) — trabalho sem uso agora (chance:
     certa; impacto: baixo).
   Custo: zero. Dá para voltar atrás? sim.

D) Traduzir o próprio jogo para português de Portugal (um runner tem poucos textos) e a
   página junto — depois do soft launch.
   + coerente e ganha Portugal. - Riscos: mais escopo (chance: certa; impacto: médio:
   textos, fonte e testes das telas). Custo: tempo de uma entrega. Volta atrás? sim.

Recomendação: A — o jogo é inglês; a página em inglês é coerente, reversível e o inglês
como padrão serve para qualquer caminho futuro (B, C ou D só acrescentam).
Segunda opinião (fable): concorda com A (baixo risco, reversível); acha o argumento contra
B mais fraco do que a primeira versão desta ficha dizia (B avisa "jogo em inglês"), e B
vira a escolha certa se Portugal for a principal fonte de instalações por pesquisa;
sugeriu a opção D (traduzir o jogo depois), incluída.
Se você não responder: a RT-9b cria o app com inglês (en-US) como padrão, que serve para
qualquer opção (B e C só acrescentam traduções).

Responda: A, B, C ou D, ou "explica melhor".
```

### Dúvidas abertas registradas pela sessão (sem resposta)

1. Existe alguma restrição jurídica sobre o nome "Wild Pack" que atinja o
   segundo jogo (mesma marca guarda-chuva)? Não verificado.
2. O estúdio quer que o segundo jogo mantenha o **mesmo universo** do Wild Pack
   (mesmos personagens) ou um universo próprio? Afeta arte e ASO.
3. A regra de teste fechado da Play para contas novas e as exigências de IARC
   para "combate de bonecos" não foram pesquisadas nesta sessão (sem chamadas
   pagas e fora do escopo; precisam ser conferidas no Play Console).
4. O OPPO CPH2791 é o único aparelho; um segundo aparelho de entrada mudaria o
   risco R3, mas o usuário já decidiu não ter (2026-11-16).
5. Existe alguma meta de receita do estúdio que defina o "bom o bastante"?
   Sem número, o plano não consegue declarar sucesso comercial.
