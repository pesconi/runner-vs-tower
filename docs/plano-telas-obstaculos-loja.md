# Plano: telas, obstáculos na corrida, formas na loja e mostrador de progresso (RT-6t.16 em diante)

Sessão de planejamento de 2026-11-23 (aberta pelo maestro). Nada foi implementado aqui. Aceite pendente: o usuário
aprova este plano e as fichas D31 e D32 (seção 5).

## 0. O pedido (palavras do usuário, 2026-11-23, depois de jogar no OPPO)

> "Erros que temos que tratar:
> 1 - layout da home e menus e afins, estão tudo estremamente primitivo, no shopping tudo ta cortado, nao tem como
> entender o que fazer, os botões devem ter cores mais vivas e sem esse retangulo branco que deve simular algum efeito
> de reflexo, remover isso do studiokit.
> 2 - Não tem desafio algum durante a corrida, tem que ter algo para atrapalhar e matar a crowd, ao chegar no boss
> aparece um tronco com espinhos que claramente deveria estar se movimentando de um lado pro outro, mas ele ta parado
> como efeito estático.
> 3 - O formato deve ser uma compra feita no mercado e que realmente traz um impacto visual ao jogo.
> 4 - o Mostrador de progresso nas fase está ocupando a tela toda."

## 1. Resumo para o celular

| # | Problema | Causa achada | Correção proposta | Entrega |
|---|---|---|---|---|
| 1a | Loja cortada | Canvas casa pela **altura** (1080×1920, `matchWidthOrHeight = 1`); num celular 20:9 a tela tem só ~864 unidades de largura, e as linhas da loja têm **930 fixas**, as abas ~970 | Canvas casa pela largura; linhas e abas esticam com a tela; capturas em 16:9, 19,5:9, 20:9 e 21:9 | 6t.16 (cortes) e 6t.17 (telas novas) |
| 1b | "Retângulo branco" nos botões | `ClayUi.AddGloss` do studiokit: cápsula branca a 26% no alto de cada botão; o jogo chama em todo `PillButton` | Jogo para de chamar (6t.16); o kit apaga a função e a opção (K.14) | 6t.16 + K.14 |
| 1c | Telas primitivas | Botões marrom/cinza escuros, cartões pretos, texto `UI.Text` sem contorno, sem hierarquia | Direção toon pastel do protótipo: botões vivos das paletas, Baloo 2 com contorno, cartões claros | 6t.17 |
| 2a | Tronco de espinhos parado | **Bug desde a RT-4a:** `SweeperDefense.Running` nunca recebe `true` (só `false`); o fluxo liga `AssaultField.Running`, que não repassa ao braço | O campo repassa `Running` ao braço; teste que prova o movimento | 6t.16 |
| 2b | Corrida sem desafio | Não existe obstáculo na corrida (só portões e caixotes); o tronco só existe no assalto | 5 obstáculos procedurais que matam parte da multidão, curva nas 40 fases | 6t.18 e 6t.19 |
| 3 | Forma como compra | Hoje BALL e CHICK são livres no menu (D26 = A, item 1) | Aba SHAPES na loja, BALL grátis, CHICK e 3 formas novas pagas (silhuetas distintas); save v5 mantém a escolha | 6t.20 (depende de D31 e D32) |
| 4 | Mostrador de progresso ocupa a tela | Os 10 pontos de fase da tela inicial são espaçados **92 unidades** fixas: a fileira mede ~880 e, num 20:9 (~864), vai de borda a borda e corta o ponto do chefe | Mostrador compacto numa cápsula (≤ 60% da largura), "FASE 4/10" com pontos pequenos | 6t.16 |

Ordem: **6t.16** (correções rápidas: tronco, mostrador, cortes, brilho no jogo) → **K.14** (kit, em paralelo com o que
vier depois) → **6t.17** (telas) → **6t.18** (obstáculos) → **6t.19** (obstáculos nas 40 fases e economia) →
**6t.20** (formas na loja). Fichas para o usuário: **D31** (leitura do pedido 3) e **D32** (catálogo e preços das
formas). O resto é técnico e fica registrado em `decisoes.md`.

## 2. Diagnóstico com evidência

### 2.1 Como as capturas foram feitas

- **OPPO:** `adb start-server` + `adb devices` às 2026-11-23: nenhum aparelho conectado. Nada foi rodado no celular
  (também não houve `force-stop` a fazer).
- **Editor:** usadas as capturas atuais do pipeline (`Tools/rt4-check.sh`, passo `capture`, regravadas em 2026-11-23
  01:28 na RT-6t.15, que não mexeu em UI), copiadas para [`art/rt6t16/`](art/rt6t16/) como `atual-*.png`.
- **Limite das capturas do editor:** elas saem em 1080×1920 (9:16). O OPPO é mais alto e estreito (~20:9). Como o
  Canvas casa pela altura, o celular mostra só a faixa central de ~864 das 1080 unidades. As imagens
  `simulado-20x9-*.png` marcam em vermelho o que fica fora dessa faixa. É simulação: elementos ancorados nas bordas
  (botão SETTINGS, moedas) se deslocam para dentro no aparelho e não são cortados; os ancorados no centro (linhas da
  loja, abas, pontos de fase, SHOP/WORLDS) são cortados como mostrado.
- A RT-6t.16 cria a captura das telas em várias proporções (seção 4), para que isto não dependa mais de simulação.

| Arquivo | O que mostra |
|---|---|
| `atual-home.png`, `simulado-20x9-menu.png` | Home: brilho branco em todo botão; pontos de fase de borda a borda; SHOP/WORLDS tocando a borda |
| `atual-loja-upgrades.png`, `atual-loja-elites.png`, `atual-loja-skins.png`, `simulado-20x9-loja-*.png` | Loja: cartões pretos de 930 de largura e abas que saem da tela no 20:9; botões de compra cortados à direita |
| `atual-mundos.png`, `atual-ajustes.png` | As outras telas de menu (mesmo estilo) |
| `atual-corrida-hud.png` | HUD da corrida: contador, moedas e barra da torre (não ocupa a tela) |
| `atual-assalto-chefe.png`, `atual-chefes-perto.png` | Assalto do chefe (o tronco fica parado no mesmo lugar em todas as capturas) |

### 2.2 Loja cortada (1a)

- `ClaybreakSceneBuilder.CreateCanvas`: `referenceResolution = 1080×1920`, `matchWidthOrHeight = 1f` (casa pela
  altura). Num 20:9 a altura vale 1920 e a largura vira 1920 × 9/20 ≈ **864** unidades; num 21:9, ~823.
- `List(...)`: a área de rolagem tem margem de 70 de cada lado (largura útil 864 − 140 = **724**), mas cada linha
  (`ListRow`) é criada com **930 de largura fixa**, centrada: sobram ~100 unidades cortadas de cada lado (o
  `RectMask2D` da lista corta o resto). O botão de compra fica a 140 da borda direita da linha, isto é, fora da
  área visível. É por isso que "não tem como entender o que fazer".
- As três abas (UPGRADES, ELITES, SKINS) somam ~970 de largura fixa: as das pontas saem da tela.
- A safe area (`SafeAreaFitter`) está certa em todas as páginas; o problema é só a largura fixa sobre um Canvas que
  casa pela altura.

### 2.3 Retângulo branco nos botões (1b)

- Origem: `studiokit` 0.7.0, `Runtime/UI/ClayUi.cs`, `AddGloss`: cria um filho `ClayGloss` (cápsula branca, alfa
  0,26, entre 12% e 42% da largura, no alto do botão).
- Quem chama:
  - **Este jogo:** `ClaybreakSceneBuilder.PillButton` chama sempre (todo botão do jogo tem o brilho).
  - **Wild Pack:** `MenuKit`, `MainMenuView` (também as faixas diagonais atrás dos menus) e `GameResultView`, todos
    atrás de `ThemeInterfaceStyle.DecorativeHighlights`, que está **desligado** nos 3 temas (`Default`, `Zombies`,
    `ForestFriends`): o Wild Pack não mostra o brilho hoje. Apagar a função quebra a compilação dele, então a K.14
    remove essas chamadas mortas junto (nada muda na tela do Wild Pack).
  - **Jogo incremental (K.12/K.13):** vive no `TestProject` do kit; só `TestProjectSetup.cs` chama `AddGloss` (um
    botão de exemplo). O código `Runtime/Incremental` e `Runtime/ItemFlow` não usa.

### 2.4 Telas primitivas (1c)

- Botões: cores escuras e terrosas (SETTINGS cinza, SHOP/WORLDS marrom, BACK cinza), cartões quase pretos
  (`CardColor`), fundo coberto por um véu escuro de 55%.
- Texto dos menus em `UI.Text` com a TTF dinâmica, sem contorno; só o HUD e os portões usam TextMeshPro com Baloo 2
  e contorno (RT-6t.9).
- Hierarquia: título, nome do mundo, estrelas, pontos, elites e forma têm quase o mesmo peso; a descrição das linhas
  da loja é pequena (30) e espremida.

### 2.5 Tronco de espinhos parado (2a)

- O "tronco com espinhos" é o `SweeperDefense` (braço com espinhos do assalto, GDD 4.3). Ele só se move dentro de
  `Update` quando `Running` é verdadeiro.
- `git log -S "sweeper.Running"`: a única escrita existe desde a RT-4a (`AssaultField`, `sweeper.Running = false`
  ao parar). **Nenhuma linha liga o braço.** O fluxo (`ClaybreakFlow`, ao chegar na linha de tiro e ao continuar)
  liga `field.Running = true`, mas a propriedade do `AssaultField` é automática e não repassa ao braço.
- Efeito: o braço fica parado em `minimumX` (fase 0 do cosseno), girando nada; a regra de morte (`Covers`) mata só
  quem passa naquela faixa fixa; a regra do chefe "superaquecer" acelera um braço que não anda. É **bug**, não
  animação faltando.

### 2.6 Corrida sem desafio (2b)

- A corrida tem portões em pares e caixotes de elite (`RunGateCourse`); não existe obstáculo. O GDD 4.1 previa
  "obstáculos móveis e armadilhas fixas que matam por toque" (do Wild Pack), mas os do Wild Pack (`RunnerObstacle`,
  `RunnerHazard`, `FixedTrapCourse`) ficaram no repo dele, dependem do `GameManager` e de colisores por boneco, e não
  servem para a multidão instanciada.
- O motor já tem o que falta para matar: a morte balística (`SwarmRemoval.Die`, anel que voa para fora, RT-6t.5) usada
  pelos portões ruins.

### 2.7 Mostrador de progresso (4)

- **Leitura nossa:** "mostrador de progresso nas fases" = os 10 pontos de fase da tela inicial (um por fase do mundo,
  o do chefe em vermelho). A barra da torre no HUD da corrida mede 520 × 64 no canto e não ocupa a tela.
- `CreateScreens`: pontos de 50, espaçados **92 fixos** (fileira de ~880 + escala 1,3 no ponto atual). Num 20:9 (~864)
  a fileira vai de borda a borda e o ponto do chefe é cortado (`simulado-20x9-menu.png`).
- Se o usuário quis dizer outra coisa, a correção da 6t.16 conferirá também a barra da torre e o HUD em todas as
  proporções; o aceite visual da 6t.16 no OPPO confirma.

## 3. Propostas

### 3.1 Telas (pedido 1)

Direção visual (referência: `docs/reference/crowd-proto.html` e as 4 paletas da RT-6t.6a):

- **Botões vivos** das paletas, sem brilho branco: ação principal (JOGAR) em coral/laranja forte (#ff9f43 → borda
  #f26a5a), secundárias em cores das paletas (SHOP menta, WORLDS céu, SETTINGS lilás), cancelar/voltar em creme com texto
  escuro. Placa inferior escura (o `AddSlab` do kit, que fica) e afundar ao tocar (`ClayPress`, fica).
- **Texto:** todo rótulo de menu passa para TextMeshPro com o atlas estático Baloo 2 ExtraBold e contorno (o mesmo do
  HUD, `GameFonts`), texto branco com contorno escuro sobre botão, escuro (#3b2f4a) sobre cartão claro.
- **Cartões** claros (creme #fff4ea com borda #f4b4c8) em vez de pretos; véu do fundo mais leve (o nível atrás aparece).
- **Hierarquia da home:** nome do jogo (Stampeep, quando a troca do nome entrar) → mundo e estrelas → mostrador de fase
  compacto → JOGAR grande no alcance do polegar → SHOP e WORLDS lado a lado → presente diário como selo, não botão
  cheio. Forma e elites saem da home para a loja (a forma vira compra, 6t.20; até lá, botão pequeno).
- **Loja legível em retrato estreito:** Canvas casa pela largura (`matchWidthOrHeight = 0`, decisão técnica T-6t16-1);
  linhas esticam com âncoras (largura = área útil), altura fixa e rolagem vertical; abas dividem a largura em partes
  iguais; botão de compra sempre dentro do cartão; preço com ícone de moeda; item comprado/vestido com selo; "não dá
  para comprar" em cinza claro com o preço visível; indicação de que a lista rola (fade no pé).
- **Telas cobertas:** home, loja (3 abas, mais SHAPES na 6t.20), mundos, ajustes, resultado (vitória e derrota),
  continuar, pausa. Todas dentro da safe area.
- **Prova:** capturas de todas as telas em 1080×1920 (16:9), 1080×2340 (19,5:9), 1080×2400 (20:9) e 1080×2520 (21:9),
  mais o OPPO. Nenhum elemento interativo fora da tela ou cortado (teste PlayMode que confere os retângulos de todos os
  botões contra a tela em cada proporção).
- **Brilho branco fora do kit (K.14, pedido explícito do usuário):** apagar `ClayUi.AddGloss`, a opção
  `ThemeInterfaceStyle.decorativeHighlights` e o texto do README; no Wild Pack, apagar as chamadas mortas
  (`MenuKit`, `MainMenuView`, `GameResultView`, `GameTheme.InterfaceDecorativeHighlights`,
  `ProfileMenuValidation`) e o campo nos 3 temas; no `TestProject`, a chamada do botão de exemplo. Versão 0.8.0 (remove
  API pública). Nada muda na tela do Wild Pack (o brilho já estava desligado lá).

### 3.2 Desafio na corrida (pedido 2)

**Correção do tronco (6t.16):** `AssaultField.Running` passa a ser propriedade com corpo que liga e desliga o braço;
teste EditMode/PlayMode que avança o tempo e confere que `ArmX` muda e que a morte acompanha o braço; captura do chefe
em dois instantes.

**Obstáculos novos (6t.18), todos procedurais, sem colisor e sem `Instantiate` em jogo:**

| Tipo | Movimento | Onde mata | Como desviar |
|---|---|---|---|
| Tronco de espinhos que balança | Pêndulo lateral (cosseno, devagar nas pontas) | Faixa x do tronco, numa linha z | Esperar a passagem ou trocar de lado |
| Serra circular | Corre de lado a lado num trilho, girando | Disco da serra | Ler o ritmo e passar atrás |
| Martelo | Sobe e bate no chão em ciclo, numa metade da pista | Retângulo no chão quando bate | Ficar na outra metade ou passar entre batidas |
| Buraco | Parado, cobre uma parte da pista | Área do buraco (bonecos caem) | Passar ao lado |
| Rolo de espinhos | Rola pela pista na direção da multidão, numa faixa | Faixa do rolo enquanto passa | Sair da faixa |

- **Mortes:** cada quadro, para cada obstáculo ativo perto da multidão, testa as posições dos bonecos (lado a lado com o
  que o assalto já faz com o braço) e mata com a morte balística existente (`SwarmRemoval.Die`), com teto de mortes por
  quadro para o efeito ficar legível. O contador cai junto, com o número flutuante vermelho dos portões.
- **Desenho:** malhas toon geradas no editor (como `TowerArt`), cor da paleta do mundo; **1 chamada de desenho por
  tipo** (desenho instanciado do kit, as duas rotas: buffer e blocos de 256 para GLES); objetos pré-alocados por fase
  (máximo por tipo fixo, como os caixotes) e reconfigurados a cada fase.
- **Janela de reação** (regra da RT-6t.15, `ReactionWindow`, `RunPace` 1,3): o obstáculo fica visível e legível ≥ 3 s
  antes da multidão chegar; nunca a menos de 1,6 s de um par de portões (a multidão não pode ter de decidir os dois ao
  mesmo tempo); o validador recusa fase fora disso, como já faz com os portões.
- **Limite de 300:** obstáculo só tira bonecos; a regra dos portões honestos (`GateMath.HighestPromise`) não muda.
- **Kit ou jogo:** os obstáculos nascem no jogo (`Assets/Claybreak/Runtime`), como as defesas do assalto; se outro
  jogo pedir, viram entrega K depois. Decisão técnica T-6t16-4.

**Distribuição nas 40 fases (6t.19), curva de dificuldade:**

- Mundo 1: fases 1 e 2 sem obstáculo (a 1ª fase vence na 1ª tentativa, GDD 3); fases 3–4 um tronco lento; daí em
  diante 1–2 obstáculos; chefe com o tronco e o martelo.
- Mundos 2–4: um tipo novo apresentado por mundo, sozinho e lento na primeira fase em que aparece; 2–3 obstáculos por
  fase no fim do mundo 4; chefes com uma combinação própria.
- **Alvo de perdas [P]:** jogador que desvia perde 5–10% do exército no mundo 1 e 15–25% no fim do mundo 4; quem não
  desvia perde até ~2× isso. O tempo da fase **não muda** (os obstáculos ocupam o percurso que já existe, mesma
  velocidade), então a ficha de dificuldade por tempo não é necessária.
- **Economia:** a vida das torres é calibrada sobre o exército esperado na linha de tiro; com perdas, ela é recalculada
  sobre (potencial × (1 − perda esperada do jogador que desvia)), para manter vitórias e estrelas como hoje. A tabela
  `rt6-economia.md` é regerada (vida, moedas por sobrevivente, tempo da loja); preços não mudam (D19). Se o tempo da loja
  mudar mais de 10%, vira ficha.

### 3.3 Forma como compra na loja (pedido 3)

**Leitura nossa** (confirmar na ficha D31): hoje BALL e CHICK são livres no botão "SHAPE" da home (D26 = A, item 1). O
usuário quer que a forma seja comprada na loja e que cada compra mude o visual do jogo de forma clara. Isso substitui o
item 1 da D26, por decisão dele.

Proposta de catálogo (preços na ficha D32):

| Forma | Como é | Triângulos | Situação |
|---|---|---|---|
| BALL (bolinha) | A atual | 484 | Grátis, padrão |
| CHICK (pintinho) | A atual | 492 | Paga |
| BUNNY (coelhinho) | Bolinha com 2 orelhas longas e rabo de pompom | 200–500 [P] | Nova, paga |
| FROGGY (sapinho) | Corpo achatado, olhos saltados no alto | 200–500 [P] | Nova, paga |
| PENGUIN (pinguim) | Corpo oval com barriga de outra cor e bico | 200–500 [P] | Nova, paga |

- As formas novas saem do construtor de malhas do jogo (`UnitShapeRecipes`): uma malha, 16 bits, olhos na frente, raio
  0,22, 200–500 triângulos (o validador já confere). Original, sem personagem de outro jogo.
- **Impacto visual de cada compra:** a forma troca a silhueta dos até 300 bonecos na corrida e no assalto, a multidão
  atrás do menu e a vitrine da loja; as skins (cor) valem para qualquer forma (9 cores × 5 formas). Cada forma pode ter um
  jeito próprio de quicar (mais alto no coelho, gingado no pinguim). O quique por forma faz parte do escopo; se exigir
  parâmetro novo no renderizador do kit, vira uma entrega K antes da 6t.20 (aceite do Wild Pack).
- **Critério de impacto visual (revisor):** as 5 formas precisam ser reconhecíveis de longe, na câmera da corrida com 300
  bonecos, não só em close. Hoje BALL e CHICK são esferas do mesmo tamanho e quase não se distinguem nessa câmera.
- **Na loja:** aba SHAPES com a figura de cada forma (imagem gerada no editor a partir da malha, sem custo em jogo),
  preço, VESTIR/VESTIDA. O botão "SHAPE" da home sai.
- **Save v5 = v4 + `ownedShapes`.** Migração: BALL sempre possuída; a forma gravada em `shape` (v4) entra como possuída
  (quem escolheu CHICK continua com CHICK, sem pagar); save sem `shape` fica com BALL. O validador do save reprova forma
  vestida que não está possuída; backup do JSON v4 como nas migrações anteriores. O gate continua vestindo a forma mais
  pesada.
- **Gate 3×:** necessário se a forma nova mais pesada passar dos 492 triângulos do CHICK (o gate veste a mais pesada).

### 3.4 Mostrador de progresso (pedido 4)

- Uma cápsula compacta no alto da home, centrada, **no máximo 60% da largura** da área segura: rótulo "FASE 4/10" e
  pontos de 22–26 com espaçamento calculado pela largura (não fixo), o do chefe com um ícone pequeno de coroa/torre.
- Na corrida, nada novo (a barra da torre já é discreta); a 6t.16 confere a barra e o contador em todas as proporções.

## 4. Entregas

Todas ficam antes da RT-9. "Paralelo" respeita a regra do projeto: **uma sessão escreve por repositório**. Então,
dentro deste repo, as entregas são em série; só a K.14 (repos `studiokit` e Wild Pack) roda em paralelo, e mesmo assim os
Unity em batchmode dos dois lados não podem rodar ao mesmo tempo (aprendizado da RT-8: um derruba o outro na partida).

### RT-6t.16 — Correções rápidas: tronco, mostrador, cortes e brilho no jogo

- **Escopo:** (1) braço de espinhos se move (`AssaultField.Running` repassa ao braço) com teste; (2) mostrador de fase
  compacto (3.4); (3) Canvas casa pela largura e as linhas, abas e botões de baixo da home esticam com a tela (só o
  suficiente para nada ficar cortado; o visual novo é da 6t.17); (4) o jogo para de chamar `ClayUi.AddGloss`; (5)
  ferramenta de captura das telas em 4 proporções (16:9, 19,5:9, 20:9, 21:9) e teste que reprova botão fora da tela.
- **Depende de:** RT-6t.15.
- **Aceite:** validação padrão completa verde; capturas das telas nas 4 proporções em `docs/art/rt6t16/` sem corte; teste
  do braço verde e captura do chefe em dois instantes com o braço em lugares diferentes; APK no OPPO. Gate 3×
  dispensado se a cena de jogo só ganhar a correção do braço (o braço já era desenhado; mover não muda o peso).
- **Olhar humano:** o usuário joga um chefe no OPPO (o tronco anda) e confere loja e home sem corte.

### K.14 (studiokit) — Tirar o brilho branco dos botões do kit

- **Escopo:** apagar `ClayUi.AddGloss`, `ThemeInterfaceStyle.decorativeHighlights`/`DecorativeHighlights` e o trecho do
  README; Wild Pack: apagar as chamadas mortas (3 telas, `GameTheme`, `ProfileMenuValidation`) e o campo dos 3 temas;
  `TestProject`: tirar a chamada do botão de exemplo; versão 0.8.0 e nota de mudança.
- **Depende de:** RT-6t.16 em `main` (o jogo já não chama `AddGloss`; senão este repo deixa de compilar).
- **Aceite:** aceitação do kit (EditMode do `TestProject`, isolamento PASS lá e nos jogos) e do Wild Pack (validadores,
  teste do save, anúncios offline 25/25); este jogo compila e passa EditMode com o kit novo. Sem gate 3× (nada muda no
  desenho em jogo).
- **Paralelo:** pode rodar junto com a 6t.17, 6t.18 ou 6t.19 (repos diferentes), alternando os Unity em batchmode.
- **Olhar humano:** nenhum (nada visível muda no Wild Pack nem no incremental).

### RT-6t.17 — Telas novas no estilo toon pastel

- **Escopo:** direção da seção 3.1 em home, loja, mundos, ajustes, resultado, continuar e pausa: botões vivos das paletas,
  texto TMP Baloo 2 com contorno, cartões claros, hierarquia da home, loja legível em retrato estreito; caracteres novos no
  atlas estático (`GameFonts.Characters`); testes PlayMode atualizados (caminhos `/Ui/Screens/<Tela>/Safe/...`).
- **Depende de:** RT-6t.16.
- **Aceite:** validação completa verde; capturas de todas as telas nas 4 proporções; teste de botões dentro da tela;
  gate 3× **uma vez** no fim (a UI muda o custo de desenho do menu sobre o nível) no mundo 4; APK no OPPO.
- **Olhar humano:** sim, aceite visual das telas pelo usuário (capturas + jogar no OPPO).

### RT-6t.18 — Obstáculos na corrida (motor e 5 tipos)

- **Escopo:** os 5 tipos da seção 3.2 com malha toon, movimento procedural, mortes pela morte balística, 1 chamada de
  desenho por tipo nas duas rotas (buffer e GLES), pré-alocação por fase; dado de fase (`LevelSpec`/`ClaybreakLevel`) com
  lista de obstáculos; regra de janela de reação no validador; fase de laboratório com os 5 tipos; testes EditMode
  (movimento, área de morte, janela) e PlayMode (multidão perde bonecos ao passar, nenhum `Instantiate`).
- **Depende de:** RT-6t.16 (não depende da 6t.17; pode vir antes dela se o usuário preferir jogo antes de tela).
- **Aceite:** validação completa verde; captura de cada tipo matando bonecos; gate 3× no mundo 4 com o máximo de
  obstáculos por fase e a multidão em 300; APK no OPPO.
- **Olhar humano:** sim, o usuário joga a fase de laboratório e diz se os obstáculos são legíveis e justos.

### RT-6t.19 — Obstáculos nas 40 fases e economia

- **Escopo:** distribuição e curva da seção 3.2 nas 40 fases (gerador + fases à mão do mundo 1 e chefes); vida das
  torres recalculada sobre o exército esperado; `rt6-economia.md` regerado com coluna de perda esperada; janela de reação
  verde em todas as fases.
- **Depende de:** RT-6t.18.
- **Aceite:** validação completa verde; tempo de fase igual ao da RT-6t.15 (±5%); tempo da loja dentro de ±10% dos 49 min
  (fora disso vira ficha); gate 3× só se a fase mais pesada tiver mais obstáculos que a medida na 6t.18; APK no OPPO.
- **Olhar humano:** sim, o usuário joga os 4 mundos e diz se a dificuldade está boa.

### RT-6t.20 — Formas compradas na loja

- **Escopo:** conforme D31 e D32: 3 formas novas no construtor de malhas, com silhueta distinta e quique próprio por
  forma; catálogo de formas com preço; aba SHAPES na loja com figura gerada no editor; save v5 com `ownedShapes` e migração (seção 3.3); revisão de segurança do save
  (revisor só leitura, como na 6t.6b); economia regerada com o total novo da loja.
- **Depende de:** RT-6t.17 (layout da loja) e respostas D31 e D32. Não depende das 6t.18/6t.19, mas se vier antes da
  6t.19 a linha de base do tempo da loja (49 min) passa a ser a nova, com as formas.
- **Aceite:** validação completa verde; fixtures de save v4 → v5 (BALL, CHICK, sem forma) verdes; capturas das 5 formas
  na câmera da corrida com 300 bonecos (reconhecíveis de longe) e no assalto, e da aba SHAPES nas 4 proporções; gate 3× se a forma mais pesada passar de 492 triângulos; APK
  no OPPO com o save atual do usuário (CHICK ou BALL mantida).
- **Olhar humano:** sim, o usuário aprova o visual das formas novas (capturas) antes do APK.

### Ordem e paralelismo

```
6t.16 ──► 6t.17 ──► 6t.20 (precisa de D31 + D32)
   │         └─(ou)─ 6t.18 ──► 6t.19
   └──► K.14 (studiokit + Wild Pack), em paralelo com 6t.17/6t.18/6t.19
```

- Ordem sugerida neste repo: 6t.16 → 6t.17 → 6t.18 → 6t.19 → 6t.20. A 6t.18 pode trocar de lugar com a 6t.17 sem
  conflito de conteúdo (só não podem rodar juntas: as duas reconstroem `Claybreak.unity` e mexem no
  `ClaybreakSceneBuilder`).
- A 6t.20 só depende da 6t.17 e das fichas; se as fichas chegarem cedo, pode vir antes da 6t.18.

## 5. Fichas de decisão (para o usuário)

Só o que é produto ou dinheiro. A numeração pula a D30 para não confundir com a métrica de retenção "D30" citada no GDD.
Telas, obstáculos, curva (sem mudar o tempo de fase) e o mostrador são técnicos: registrados em `decisoes.md`.

```text
DECISÃO D31 — O pedido "o formato deve ser uma compra na loja": o que muda

O que você está aprovando: as formas da multidão passam a ser compradas na loja, numa aba
SHAPES; a bolinha continua grátis; o pintinho e 3 formas novas (coelhinho, sapinho, pinguim) são
pagos, e quem já usa o pintinho fica com ele sem pagar; o botão "SHAPE" sai da tela inicial. Cada
forma tem de ser reconhecível de longe na multidão de 300 durante a corrida (silhueta própria e
jeito próprio de quicar), e esse é o critério de aceite. Isto substitui o item 1 da D26.

Por que agora: trava só a RT-6t.20 (formas na loja). As RT-6t.16 a 6t.19 seguem sem ela; o
desenho da aba entra na própria 6t.20.

Contexto que você precisa saber:
- Hoje: bolinha e pintinho são grátis no botão "SHAPE" da tela inicial (D26 = A, 2026-11-22, fato).
- Hoje a loja vende 4 melhorias, 2 elites e 8 cores de multidão; só as cores mudam o visual (fato).
- Sua frase: "O formato deve ser uma compra feita no mercado e que realmente traz um impacto
  visual ao jogo." Lemos "formato" como a forma dos bonecos (o único "formato" do jogo é o botão
  SHAPE). Também lemos a segunda metade como crítica: bolinha e pintinho são duas esferas de
  mesmo tamanho com olhos e, de longe, numa multidão de 300, quase não se distinguem (inferência
  nossa, das capturas). Por isso as formas novas precisam de silhueta bem diferente.
- Formas novas são baratas de fazer no gerador de malhas (200–500 triângulos, fato da RT-6t.2).
  Se a mais pesada passar dos 492 triângulos de hoje, o teste de desempenho no OPPO roda de novo.
- D25 (2026-11-22): o jogo é declarado 13+ na Play com o visual fofo. Mais três bichinhos reforçam
  o apelo infantil (inferência nossa); a ficha da loja na RT-9 precisa levar isso em conta.

Opções:
A) Formas compradas na loja: bolinha grátis; pintinho e 3 novas pagas (quem usa o pintinho
   fica com ele).
   + Toda forma comprada muda o jogo inteiro na hora; meta nova sem mexer em dificuldade.
   - Riscos: cobrar pelo pintinho, que era grátis ontem (chance alta; impacto baixo, não há
     jogadores ainda). Formas novas pouco distintas na câmera de jogo (chance média; impacto
     médio) — mitigação: aceite com capturas da corrida com 300 bonecos, não closes, e você
     aprova antes do APK. Apelo infantil maior (D25) (chance média; impacto baixo agora).
   Custo: 1 entrega (RT-6t.20). Dá para voltar atrás? sim, fácil (preço 0 = livre).
B) Bolinha e pintinho continuam grátis; só as 3 formas novas, bem diferentes, são pagas.
   + Mantém a D26 aprovada ontem; não cobra pelo que já era livre.
   - Riscos: a loja começa com só 3 formas pagas (chance alta; impacto baixo); o botão de troca
     grátis convive com a aba paga (pode confundir; chance média; impacto baixo).
   Custo: 1 entrega. Dá para voltar atrás? sim, fácil.
C) A + as melhorias da loja também ganham marca visual (ex.: DANO põe um capacetinho,
   LANÇAMENTO deixa um rastro).
   + "Toda compra se vê", a leitura mais ampla da sua frase.
   - Riscos: mais peças por boneco pesam no celular (chance média; impacto médio, exige teste
     de desempenho); podem poluir a multidão de 300 (chance média; impacto médio).
   Custo: +1 entrega depois da RT-6t.20. Dá para voltar atrás? sim, com custo.
D) Formas continuam grátis; só as compras atuais ficam mais visíveis (cores com padrão).
   + Não muda a D26.
   - Riscos: não atende "o formato deve ser uma compra" (chance alta; impacto médio).
   Custo: 1 entrega. Dá para voltar atrás? sim.

Recomendação: A — é a leitura mais direta de "formato", com o impacto visual virando critério
de aceite (silhuetas distintas na corrida e quique próprio). B é a alternativa se você não quiser
cobrar pelo pintinho; C pode vir depois.
Segunda opinião (fable): concorda com A, com a condição (já aplicada) de o impacto visual virar
critério de aceite e de citar a D25; sugeriu a opção B (bolinha e pintinho grátis).
Se você não responder: as RT-6t.16 a 6t.19 seguem; a RT-6t.20 espera.

Responda: A, B, C ou D, ou A + ajuste, ou "explica melhor".
```

```text
DECISÃO D32 — Preços das formas na loja

O que você está aprovando: os preços das formas pagas da aba SHAPES na opção escolhida abaixo;
preços das melhorias, elites e cores ficam iguais (D19); o tempo para comprar a loja inteira
sobe um pouco.

Por que agora: trava a RT-6t.20. Vale para a D31 A ou C (com B, o pintinho sai da lista e valem
os 3 preços de cima). Pode sair junto com a D31.

Contexto que você precisa saber:
- Moeda só se ganha jogando hoje; o anúncio recompensado e o presente diário (RT-7) vão dar mais
  moedas e não estão na simulação, então todos os tempos abaixo tendem a encurtar (fato: D6, RT-7;
  efeito é inferência nossa).
- A loja soma 19.380 moedas e sai inteira em ~49 min de jogo simulado, ~399 moedas por minuto em
  média (fato, `rt6-economia.md`, RT-6t.15). A renda cresce: ~160 por minuto no mundo 1 e bem mais
  nas voltas do mundo 4 (fato da mesma simulação). Com preços ×1,5 a loja sai em 61 min (fato).
- Os minutos das opções são interpolados entre 49 min (loja atual) e 61 min (×1,5) (inferência
  nossa); a entrega roda a simulação e registra o número real.
- Cores custam de 150 a 2.500 (fato). A forma muda mais o visual que a cor (inferência nossa).
- Forma e melhorias usam a mesma moeda: quem gasta em forma cedo atrasa as melhorias, e a vida das
  torres supõe as melhorias compradas (fato da D19; risco já existe com as cores).
- Se os obstáculos (RT-6t.19) mudarem o tempo da loja em mais de 10%, você recebe outra ficha.

Opções:
A) Escada — PINTINHO 300 · COELHINHO 1.000 · SAPINHO 1.600 · PINGUIM 2.400 (total 5.300).
   Loja inteira ~24,7 mil, ~55–56 min (+7).
   + Primeira forma ao alcance no mundo 1 (~2 min de renda dele); as outras junto com as cores
     mais caras.
   - Riscos: comprar o pintinho cedo atrasa uma ou duas melhorias (chance média; impacto baixo).
   Custo: nenhum. Volta atrás? sim, fácil (números numa tabela).
B) Barato — 300 · 600 · 900 · 1.200 (total 3.000). Loja ~22,4 mil, ~53 min (+4).
   + Formas chegam rápido; bom para o teste de vocês.
   - Riscos: acabam cedo e deixam de ser meta (chance média; impacto baixo).
   Custo: nenhum. Volta atrás? sim, fácil.
C) Caro — 1.000 · 2.000 · 3.000 · 4.000 (total 10.000). Loja ~29,4 mil, ~61 min (+12).
   + Meta mais longa; forma vira o item mais desejado.
   - Riscos: a primeira forma leva ~6 min no mundo 1 e compete com as melhorias (chance alta;
     impacto médio na sensação de progresso).
   Custo: nenhum. Volta atrás? sim, fácil.

Recomendação: A — uma forma barata como isca no começo e as outras na faixa das cores caras, sem
esticar a meta (+7 min); recalibrar no soft launch.
Segunda opinião (fable): concorda com A depois de corrigidos os minutos (a primeira versão dizia
+14 min por regra de três; o certo é ~+7) e sugeriu o pintinho barato, já aplicado (300).
Se você não responder: a RT-6t.20 usa A como provisório [P] quando chegar a vez dela.

Responda: A, B ou C, ou A + ajuste, ou "explica melhor".
```

## 6. Decisões técnicas desta sessão (também em `decisoes.md`)

- **T-6t16-1** Canvas casa pela largura (`matchWidthOrHeight = 0`); conteúdo vertical com âncoras e rolagem, para caber
  de 16:9 a 21:9.
- **T-6t16-2** A correção do braço é no `AssaultField` (propriedade que repassa), não no fluxo, para nenhum caminho novo
  esquecer o braço.
- **T-6t16-3** Leitura do pedido 4 = pontos de fase da home; a 6t.16 confere também o HUD em todas as proporções.
- **T-6t16-4** Obstáculos nascem no jogo (como as defesas do assalto), não no kit; viram K só se outro jogo pedir.
- **T-6t16-5** Obstáculos sem colisor: teste de área contra as posições da multidão, morte balística existente, 1
  chamada de desenho por tipo, pré-alocados por fase.
- **T-6t16-6** Vida das torres recalculada sobre o exército esperado de quem desvia; o tempo de fase não muda, então a
  curva não vira ficha (só se a loja mudar mais de 10%).
- **T-6t16-7** Remoção do brilho no kit é a K.14, versão 0.8.0 (API pública removida), depois de a 6t.16 tirar a chamada
  do jogo.
- **T-6t16-8** Capturas desta sessão: as do editor (OPPO desconectado) e simulação da faixa 20:9; a 6t.16 troca a
  simulação por capturas reais em 4 proporções.
