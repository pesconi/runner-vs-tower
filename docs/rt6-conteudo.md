# RT-6 — Produção: conteúdo (Clay Siege, nome de trabalho; código "Claybreak")

Data: 2026-11-20. Entrega RT-6 de `plano-de-entregas.md`, escopo da D12 = A (4 mundos × 10 fases) e da
D19 = A (a tabela da RT-5 é o ponto de partida; pagamento dos mundos e total da loja acertados aqui).
**[F]** = conferido nesta sessão; **[I]** = inferência ou proposta; **[P]** = valor provisório a calibrar.

Entregue mundo a mundo: `Stage RT.6a` (mundo 1 completo, geração por dados, escudo, regras de chefe,
visual por mundo, save v3), `RT.6b` (mundo 2), `RT.6c` (mundo 3), `RT.6d` (mundo 4 e economia final).

## 1. O que existe

| Pedido da RT-6 | Como ficou [F] |
|---|---|
| Geração de fases por dados | `WorldRecipe` (perfil de dificuldade do mundo) + `LevelGenerator`: a semente e o índice escolhem os portões da corrida (cada par é uma escolha real ou uma armadilha), a fileira do assalto e as faixas da muralha e do escudo; o perfil sobe o resto (exército inicial, andares, canhão, varredor, escudo, prêmio). A vida da torre não é escolhida à mão: é uma fração do **potencial** da fase (exército no melhor caminho × melhor portão da fileira) vezes o dano que o mundo espera. Fases feitas à mão entram no lugar de qualquer índice; as geradas sobem entre elas (o gerador tenta até 12 variações do layout para a vida natural caber na rampa). |
| Ajuste de dificuldade por perfil | Por mundo: o perfil diz o dano de upgrade para o qual o mundo foi calibrado (`expectedDamage`). Nas **voltas** (depois de vencer o chefe), as torres crescem também com o poder do jogador acima desse valor: (poder ÷ esperado)^0,7, nunca abaixo de 1. Poder = multiplicador de dano × (1 + ½ do bônus de lançamento). A primeira passada nunca é ajustada: comprar upgrade sempre aparece. É a correção que a ficha D19 pediu ("o topo dos upgrades deixa o mundo 1 fácil nas voltas altas"): com tudo no topo, o mundo 1 na volta 1 fica ~2,6× mais forte em vez de 1,25×. |
| Mundo 1 com 10 fases | As 6 fases da fatia e o chefe ficam iguais (números jogados no OPPO); as fases 7–9 são geradas e sobem entre a fase 6 (vida 470) e o chefe (600). Tabela em `docs/rt6-economia.md`. |
| Escudo (4ª defesa do GDD) | Gerador numa faixa lateral, entre a muralha e a torre; enquanto ele está de pé, uma cúpula cobre a frente da torre e ela ignora parte de cada golpe (fração do perfil, 40–60%). Unidades que chegam ao gerador batem nele; o jogador decide quantas gastar ali. Entra no mundo 2. |
| Chefes com regra própria | Regra por dado (`BossRule`), uma por mundo: **ShiftGates** (mundo 1: a fileira de portões gira), **ShieldReturns** (o escudo volta a cada andar), **Overheat** (canhão e varredor 25% mais rápidos a cada andar), **TurnGate** (o melhor multiplicador vira ÷2 a cada andar). |
| Visual por mundo | Cada mundo tem um `WorldLook` (estilo de cenário e paleta: céu/neblina, luz ambiente, sol, chão, campos, adereços, luzes). A cena traz um conjunto de cenário pré-alocado por mundo (só o do mundo atual aparece) e troca a cor da pista, do céu e do sol ao preparar a fase; nada é criado em jogo (2.2.1). |
| Gate por mundo | Um nível de gate por mundo (torre mais pesada do mundo, todas as defesas dele, cenário dele, 300 em voo, 12 pedras, loadout máximo); `Tools/rt2-gate.sh <rótulo> 60 20 <apk> <mundo>`. |
| Save | Versão 3: o mundo 1 passou de 7 para 10 fases (as novas antes do chefe). Saves v1 e v2 migram: estrelas das fases 1–6 ficam, as do chefe vão do 7º para o 10º lugar, quem estava no chefe encontra a primeira fase nova; moedas, voltas e o resto ficam. |
| Validador de completude do tema | `ClaybreakValidation` agora confere por mundo: aparência definida, estilo de cenário e céu que nenhum outro mundo usa, torre principal e chefe que nenhum mundo anterior usa, chefe com regra e 3+ andares, vida que não cai de uma fase para a outra e chefe acima de todas, e o nível de gate do mundo (torre do chefe, regra, escudo se o mundo tem). |

## 1.1 Os mundos

| Mundo | Cenário (recomendação em uso) | Torre própria | Chefe e regra | Defesas novas | Dano esperado | Abre com |
|---|---|---|---|---|---|---|
| 1 — marchas | crepúsculo, pedras, pilares, braseiros (RT-4) | bastião de pedra (+ forja) | cidadela — a fileira de portões gira | muralha, canhão, varredor | ×1,0 | — |
| 2 — pântano | noite verde-azulada, juncos, palafitas, lanternas verdes | forte de palafita (+ forja) | campanário afundado — o escudo volta a cada andar | **escudo** (da fase 3) | ×1,3 | chefe do 1 + 10 estrelas |
| 3 — fundição | fuligem e brasa, barris, chaminés, cadinhos, silhueta de fábrica | moinho de chaminés (+ forte de palafita) | martelo-pilão — canhão e varredor esquentam a cada andar | todas desde a fase 1; escudo da fase 4 | ×1,6 | chefe do 2 + 25 estrelas |

Rampa do mundo 2 (vida): 110 → 220 → 380 → 500 → 540 → 550 → 650 → 680 → 720 → chefe 800 (tabela
completa em `docs/rt6-economia.md`). As opções de arte de cada mundo novo estão em `docs/art/rt6/`
(`mundoN-paletas.png`, `-torres.png`, `-chefes.png`, A à esquerda) e o assalto de cada mundo na cena real em
`docs/art/telas/jogo-mundoN-fase.png` e `-chefe.png`.

## 2. Decisões

Técnicas (`decisoes.md`, T-RT6-1 a T-RT6-4): fases como `LevelSpec` gerado por perfil; visual por mundo
pré-alocado; escudo e regras de chefe como dado; ajuste das voltas pelo poder do jogador e save v3.

## 3. Validação

| Data | Etapa | Resultado |
|---|---|---|
| 2026-11-20 | RT.6a: cena, validador, isolamento do kit, EditMode, PlayMode, economia, APKs (`Tools/rt4-check.sh /c/temp/rt6`) | PASS; EditMode 101/101 (+ testes do gerador, do escudo, das regras e da migração v3), PlayMode 5/5 |
| 2026-11-20 | RT.6b (mundo 2): as mesmas etapas + capturas | PASS; EditMode 101/101, PlayMode 6/6 (novo: o gate de cada mundo mostra o cenário e a torre do chefe dele, com o escudo de pé onde o mundo tem) |
| 2026-11-20 | RT.6c (mundo 3): as mesmas etapas | PASS; EditMode 101/101, PlayMode 6/6 |

## 4. Gate 3× no OPPO CPH2791 (torre mais pesada de cada mundo, loadout máximo)

Critério: quadro p95 ≤ 33,3 ms, < 5% acima de 50 ms, PSS ≤ 450 MB, CPU main p95 ≤ 11 ms, GPU p95 ≤ 11 ms,
com 300 em voo e 12 pedras.

| Data | Mundo | p95 quadro | >50 ms | main p95 | gpu p95 | PSS | em voo | elites | pedras | Passou? |
|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-20 | 1 (cidadela) | 16 ms (max 16) | 0,0% | 4,5–5,5 ms | 3,9–4,3 ms | 182 MB | 299 | 100 | 11–12/12 | **Sim** |
| 2026-11-20 | 2 (campanário, escudo de pé) | 16 ms (max 16) | 0,0% | 4,4–5,6 ms | 4,1–4,5 ms | 195 MB | 299 | 100 | 11–12/12 | **Sim** |
| 2026-11-20 | 3 (martelo-pilão, escudo de pé) | 16 ms (max 16) | 0,0% | 4,1–6,5 ms | 4,0–4,4 ms | 199 MB | 299 | 100 | 12/12 | **Sim** |
