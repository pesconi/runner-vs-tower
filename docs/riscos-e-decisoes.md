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
| **D7** | **Conta Play e conta AdMob:** usar as do Wild Pack ou separar; DUNS; risco de conta única | O Wild Pack ainda está criando a conta (P1) | RT-9 | Mesma conta de organização; app e unidade de anúncio separados |
| **D8** | **Orçamento de UA e de ferramentas** (se houver) | Sem orçamento, o gênero (CPI alto, descoberta difícil [F]) exige ASO e orgânico; não há dado para dimensionar | RT-10 | Soft launch **sem** UA paga ou com valor pequeno e fixo, definido por você |
| **D9** | **Países do soft launch** — orientação do usuário em 2026-11-21: **Europa** (D23); falta escolher quais | CPM e CPI variam muito por país [F: Adjust 2026-04-15] | RT-9 | Escolher poucos países de idioma suportado; usar dados do Wild Pack para decidir |
| **D10** | **Idiomas** | Ficha e textos | RT-9 | Os mesmos do Wild Pack (P6) |
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
| **D27** | **Teto da multidão** (hoje 300; motor novo aguenta 1000) | Espetáculo × equilíbrio das 40 fases e peso do assalto no celular | RT-6t.4 (sem resposta: 300) | A: 1000 na corrida, até 300 em voo valendo mais — ficha em `direcao-visual-crowd.md` — **decidido em 2026-11-22: A** (entrega nova antes da RT-9) |
| **D28** | **Arte do assalto no estilo novo** (torres, defesas, chefes, efeitos) | O protótipo não mostra o assalto | RT-6t.10 (sem resposta: A) | **decidido em 2026-11-22: A** (aplicada por padrão na RT-6t.10: sombreado toon + cores da paleta, sem remodelar, sem rostos; confirmada pelo usuário ao aprovar as capturas `docs/art/rt6t/chefes-*.png`) — ficha em `direcao-visual-crowd.md` |
| **D29** | **Nome "Clay Siege" sem massinha** | Ícone, loja e domínio; checagem de marca pendente | RT-9 | B: 3 nomes novos checados + "Clay Siege", escolha antes da RT-9 — ficha em `direcao-visual-crowd.md` — **decidido em 2026-11-22: B** (EUIPO, INPI Portugal, TMview e lojas) |

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
