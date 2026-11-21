# Clay Siege — direção visual v2 (revisão antes da RT-9)

Data: 2026-11-21. Pedido do dono: "pense como aquela fase em que o Astro Bot vira uma bolinha"; o jogo atual
parece "anos-luz distante de uma estética de jogo 2026" e lembra "a Tomb Raider de peito triângulo". Ele quer algo
**bonito, simples, leve**. Decisão: **D24** (`decisoes.md`). Entrega proposta: **RT-6v** (`plano-de-entregas.md`).

**Limite legal.** A captura do Astro Bot que o dono mandou é de terceiros e **não está no repositório**. Dela usamos
só qualidades gerais, que nenhum jogo possui: formas macias e arredondadas, material brilhante de brinquedo, luz
suave e paleta viva. Nada de personagem, nível, robô, visor, antena ou composição do Astro Bot. O soldado proposto é
um feijão com capacete, escudo e pluma, **sem rosto nem olhos** (regra da D17 mantida). A torre é um forte de 4
tambores com telhados pontudos azul-noite e canhão, e a pista é um platô creme com meio-fio coral. É identidade própria do Clay Siege.

Prova visual feita no próprio Unity, sem arte de IA: `docs/art/direcao-v2/` (ver seção 4).

---

## 1. Diagnóstico: por que o visual atual parece de 1996

Olhando as telas atuais (`docs/art/telas/`, `rt4/`, `rt6/`) e o código que as desenha:

1. **Formas de caixa.** A torre, os portões, os postes, as pedras e a pista são cubos e cilindros crus do Unity
   (`Prim.Box`, `Prim.Cyl` em `TowerArt.cs` e `WorldSceneryArt.cs`). Não há quinas arredondadas, e cada degrau da
   torre é uma caixa em cima de outra. É exatamente a "geometria de triângulo" da época do PlayStation 1.
2. **Luz chapada.** Os materiais são *Simple Lit* de cor única (`ArtPalette.cs`), sem brilho especular, sem borda
   iluminada e com luz ambiente de uma cor só (`AmbientMode.Flat`). Cada face tem um tom e pronto. Não há volume.
3. **Paleta escura e barrenta.** O GDD pediu "paleta noturna/quente, adulta" (seção 9). O resultado é marrom sobre
   roxo-escuro, com pouco contraste entre pista, cenário e céu. No celular, sob luz do dia, vira uma mancha.
4. **Cenário vazio.** As laterais da pista são planos lisos com poucos postes e troncos soltos. Não há camadas
   (perto, meio, longe), não há céu (é cor sólida) e não há nada vivo (flores, árvores, nuvens).
5. **Portões sem cara.** São dois retângulos opacos azul e vermelho com o número pequeno. Não parecem objeto do
   mundo nem "vidro de brinquedo".
6. **Soldado sem identidade.** O boneco de massinha bege (427 vértices) é liso, mas facetado nas juntas, sem
   cor de time e sem brilho. Em multidão ele se vê (é o objeto mais claro da tela), mas como uma massa bege sem
   volume; não diz "exército".
7. **Sem sombra de contato.** Nada "pousa" no chão, então tudo parece flutuar.

O que **não** é problema: o desempenho tem muita folga (gate da RT-8 no chefe do mundo 4: GPU p95 4,3 ms contra
o limite de 11 ms; CPU p95 4,8 ms; PSS 228 MB). Há espaço para luz e forma melhores sem estourar o celular.

---

## 2. As três direções

As três usam a mesma geometria arredondada (é o que tira o "1996"). Mudam o material, a luz e a paleta. A folha
`docs/art/direcao-v2/direcoes-abc.png` mostra as três na câmera de corrida do jogo.

### A) Brinquedo Brilhante (recomendada)

- **Sensação:** brinquedo de vinil novo, saído da caixa; plástico macio que brilha no sol; manhã de céu limpo.
- **Formas:** tudo arredondado: feijões, cúpulas, tambores com borda boleada, platôs. Silhuetas simples e grandes,
  legíveis a poucos pixels.
- **Materiais:** plástico brilhante, com reflexo especular duplo (ponto forte e halo suave) e borda iluminada pela
  cor do céu. Vidro colorido nos portões.
- **Luz:** sol quente, luz "enrolada" (o lado da sombra não fica preto), ambiente em duas cores (céu em cima, chão
  quente embaixo), neblina na cor do horizonte e céu em degradê. Sombras de contato macias e redondas. Sem sombra
  em tempo real e sem pós-processamento.
- **Paleta (mundo 1):** pista creme-manteiga, meio-fio coral, grama menta, colinas verde-água, árvores
  redondas verdes e turquesa com uma laranja de vez em quando, flores brancas e amarelas, céu azul-claro. Exército
  azul-cobalto e ouro; inimigo coral e creme com telhados azul-noite e canhão escuro. O doce (rosa, pirulito) foi
  tirado depois da segunda opinião, para não puxar para o infantil.

| Peça | O que muda |
|---|---|
| **Soldado e multidão** | Feijão azul com capacete azul de aba e pluma dourada, punho e escudo dourado, sem rosto. A cor do time vem de cima, porque a câmera é alta e o que se vê é o capacete. As skins e elites trocam cores (as 4 regiões de cor do kit continuam) e acessórios. |
| **Pista** | Platô creme arredondado, mais alto que a grama, com meio-fio coral. Termina numa praça redonda sob a torre. |
| **Portões** | Moldura branca brilhante com bolas douradas e painel de vidro colorido (bom = azul, ruim = rosa-vermelho). Número 2,3× maior que hoje, branco com sombra azul-escura (sem a sombra, o branco sumia no vidro claro). |
| **Torre** | Forte de 4 tambores (os 4 andares que caem no assalto, como hoje), coral e creme alternados, ameias redondas, torreões e telhado pontudo azul-noite, janelas em arco, canhão escuro sobre a porta e bandeira. |
| **Chefe** | A mesma linguagem, maior e com uma peça de atitude por mundo (ex.: canhão-bola, coroa de espinhos redondos), além de brilho dourado mais forte. As regras de chefe não mudam. |
| **Cenário** | Três planos: perto (flores, pedrinhas), meio (árvores pirulito) e longe (colinas e nuvens), com neblina clara. Tudo numa malha só por mundo (barato). |
| **UI** | As pílulas atuais do kit (`ClayUi`) ficam, com cores novas: fundo creme, texto azul-escuro, botão principal dourado e sombra suave. O título "CLAY SIEGE" ganha contorno. |

- **Os 4 mundos (proposta para a RT-6v; temas mantidos e iluminados):** 1 vale ensolarado (acima); 2 pântano
  doce (verde-água, lilás, cogumelos e lanternas brilhantes); 3 fábrica de brinquedos (cobre polido, laranja, engrenagens
  redondas); 4 gelo (menta, lilás, cristais boleados). Cada mundo é só paleta, peças de cenário e torre/chefe, como hoje.
- **Risco principal:** fica com cara de jogo infantil. O jogo é 13+ "não direcionado a crianças" (GDD 2) e o risco
  R8 já previa isso (ver ficha D24). O protótipo já tirou rosto, rosa e pirulito; o resto se mede na RT-6v.1.

### B) Massinha Macia

- **Sensação:** massinha de modelar de verdade, apertada à mão, como num filme de stop-motion; tarde de luz morna.
- **Formas:** as mesmas formas arredondadas, com superfície levemente irregular ("marca do dedo").
- **Materiais:** fosco, quase sem brilho. Relevo de massinha feito por ruído no próprio shader, sem textura.
- **Luz:** sol morno e mais lateral, ambiente lilás e marrom, neblina creme.
- **Paleta:** terrosa e mais adulta: areia, terracota, sálvia, azul-ardósia, ocre.
- **Peças:** iguais às de A, com cores terrosas e acabamento fosco. A torre fica de terracota e osso, os portões de
  vidro fosco, e a UI em creme e terracota.
- **Ganho:** é a mais fiel ao nome "Clay" e a mais "adulta" das três.
- **Perda:** no celular, à distância de jogo, o relevo some e sobra um visual mais lavado e menos "2026". Custa mais
  GPU que A (ruído por pixel), e o relevo pode tremular em telas pequenas.

### C) Pastel Chapado

- **Sensação:** ilustração vetorial em pastel, como um app de meditação.
- **Formas:** as mesmas.
- **Materiais:** sem brilho. Luz em dois tons (luz e sombra recortadas) e cores claras e suaves.
- **Luz:** fraca e uniforme. Céu lavanda e pêssego.
- **Ganho:** é a mais barata no celular e a mais calma.
- **Perda:** o contraste cai muito: o vidro do portão fica quase da cor da pista e a multidão azul-clara perde
  força contra o fundo claro. Fica com cara de "genérico de 2019", e não de 2026.

---

## 3. Custo no celular (estimativa; o gate 3× decide)

Base medida (RT-8, OPPO CPH2791, chefe do mundo 4, 299 soldados em voo): GPU p95 4,0–4,3 ms, CPU p95 3,9–4,8 ms,
PSS 228 MB. Limites do gate: 11 ms, 11 ms e 450 MB. Ou seja, a GPU usa hoje cerca de 40% do limite.

| Item | Hoje | Direção A | Efeito estimado [inferência nossa] |
|---|---|---|---|
| Soldado | 427 vértices, shader `RunnerClay` (difusa enrolada + borda) | protótipo 907 vértices, mas meta de **≤ 600** no modelo final (menos segmentos; o protótipo é generoso); mesmo shader com brilho especular e ambiente de duas cores | O vértice é a parte cara da multidão, porque cada vértice lê a textura de poses. Com 300 soldados a 600 vértices, são ~180 mil vértices por quadro contra ~128 mil hoje (+40%). O brilho acrescenta ~15 operações por pixel. **+0,5 a 1,5 ms de GPU** no pior caso. |
| Sombras de contato | 1 quadrinho por soldado (kit, `RunnerGroundShadow`) | igual, com borda suave | ~0 |
| Céu | cor sólida | domo com degradê (1 desenho, 1 material) | ~0,1 ms |
| Cenário | caixas soltas, vários materiais | 1 malha por mundo, 1 material, cores no vértice | igual ou menor em CPU (menos desenhos); mais vértices, que ficam baratos sem poses |
| Torre/portões | caixas | tambores e cúpulas arredondados (~10–20 mil vértices) | ~0,1–0,3 ms |
| Pós-processamento e sombra em tempo real | nenhum | **nenhum** (regra mantida) | 0 |

- **A:** estimativa de GPU p95 entre 5 e 6,5 ms (limite 11); CPU sem mudança relevante; memória +5–15 MB
  (malhas). Deve passar com folga.
- **B:** A + ruído por pixel: +0,5–1 ms a mais que A.
- **C:** a mais barata, quase igual a hoje.

**O que o gate vai medir quando a RT-6v for feita:** o de sempre (`Tools/rt2-gate.sh`, build de medição, chefe do
mundo 4, 60 fps, multidão cheia em voo): quadro p95 ≤ 33,3 ms e < 5% acima de 50 ms; CPU principal p95 ≤ 11 ms;
**GPU p95 ≤ 11 ms** (o número que a direção nova mais mexe); PSS ≤ 450 MB; alocação zero por quadro. Uma rodada,
no fim da entrega, no caso mais pesado.

Medida de segurança já prevista: se a GPU ficar acima de ~8 ms, o soldado usa uma versão mais simples (menos
vértices) quando a multidão passa de ~150. É uma troca de malha, sem regra nova de jogo.

---

## 4. Prova visual (Unity, mesma câmera, antes e depois)

Feita por `Assets/Claybreak/Editor/Prototypes/DirectionV2Prototype.cs` (menu *Claybreak > Prototype > Capture
Direction v2*, ou batchmode `-executeMethod Claybreak.Editor.Prototypes.DirectionV2Prototype.CaptureAndQuit`, sem
`-nographics`). Monta cenas descartáveis: o **antes** com os construtores do próprio jogo (pista, cenário e torre do
mundo 1, portão igual ao `CreateRunGates`, soldado e material do kit) e o **depois** com malhas arredondadas geradas
por código (`SoftMeshBuilder.cs`) e os shaders do protótipo (`Assets/Claybreak/Prototypes/DirectionV2/`). Não mexe
na cena do jogo, nos materiais do jogo nem no kit.

| Arquivo | O que mostra |
|---|---|
| `antes-depois-corrida.png` | Câmera de corrida do jogo (9 m acima, 8 m atrás, olhando 6 m à frente): 44 soldados, um portão e a torre ao fundo. Esquerda = hoje; direita = A. |
| `antes-depois-perto.png` | A multidão e o portão de perto. |
| `antes-depois-torre.png` | A torre de perto. |
| `direcoes-abc.png` | As três direções na câmera de corrida, mesma geometria. |

Limites do protótipo (honestos): soldados parados, sem poses de corrida; 44 soldados (o jogo chega a 300); sem HUD;
renderização com antisserrilhado 4×, que o jogo no celular não usa (vale igual para o antes); só o mundo 1. Visto
da câmera do jogo, o soldado é uma bola azul de capacete: escudo e pluma só aparecem de perto. O soldado final
terá menos detalhe (≤ 600 vértices) que o das capturas. Nada disto foi medido no OPPO; o gate da RT-6v mede.

---

## 5. O que sai do studiokit e o que é só do jogo

Regra: o kit nunca nomeia jogo nem tema, e mudar o kit muda também o Wild Pack. **Nada foi mexido no kit nesta
sessão.** Proposta para a RT-6v (uma entrega do studiokit antes, com o aceite do Wild Pack):

**Kit (genérico, com o padrão = o visual de hoje, para o Wild Pack não mudar):**
- `RunnerClay.shader`: brilho especular opcional (força, tamanho), ambiente de duas cores (céu/chão) e cor da
  borda, como propriedades do material. Com tudo em zero, o desenho fica idêntico ao de hoje.
- Shader de céu em degradê e shader de sombra de contato suave (genéricos).
- Ferramenta de editor de malhas arredondadas (`SoftMeshBuilder`: tambor, cúpula, elipsoide, caixa boleada, anel),
  para gerar peças de cenário e de torre sem programa de modelagem.

**Só do jogo (Clay Siege):**
- O novo corpo do soldado (feijão com capacete, escudo e pluma) com as poses de corrida, feito pelo mesmo caminho do
  `ClayRunner` do kit (script de Blender e pose em textura), mas guardado no jogo. O Wild Pack continua com o boneco dele.
- As paletas dos 4 mundos (`WorldContent`/`ArtPalette`), as torres e os chefes (`TowerArt` refeito com peças
  arredondadas), os portões (`CreateRunGates`), o cenário (`WorldSceneryArt`), as cores da UI (tema do jogo) e o
  ícone.
- O texto do GDD seção 9 ("direção adulta, paleta noturna, não castelo de brinquedo"), que a D24 substitui se o dono
  aprovar A.

---

## 6. Custo em entregas

| Direção | Entregas | Semanas [I, com ~20%] |
|---|---|---|
| A | studiokit (shader, céu, ferramenta) + RT-6v em 3 partes: (1) base, soldado novo e mundo 1; (2) mundos 2–4, torres e chefes; (3) UI, ícone, capturas e gate 3× | ~3 |
| B | igual a A, com mais ajuste fino do relevo | ~3,5 |
| C | igual a A, com menos trabalho de luz e mais de contraste e texto | ~2,5 |
| Manter o atual | 0 | 0 |

As estimativas em semanas seguem a régua do plano. As entregas RT-4 a RT-8 levaram horas a poucos dias cada, então
na prática deve ser menos. Pela régua, a RT-9 e o soft launch andam ~3 semanas.

**Em etapas (recomendado):** a RT-6v.1 (soldado novo e mundo 1, com HUD e multidão cheia) termina com o dono
vendo no celular e com a regra da Play sobre público misto conferida. Só então se refazem os mundos 2 a 4. Se
não agradar, para-se ali com ~1 semana gasta.
