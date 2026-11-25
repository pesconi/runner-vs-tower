# Stampeep — Direção visual e sonora v3 ("acabamento premium")

Data: 2026-11-25. Autor: maestro (sessão do Runner vs Tower), a pedido do usuário. Revisor: fable (seção 9).
Estado: **plano**; nada implementado. Entregas: série **RT-6v3.x** (e as entregas **K.v3a–c** do `studiokit`) em
`plano-de-entregas.md`. Decisões do usuário: **D36** (imagem-alvo; rascunho, enviada só no fim da RT-6v3.2) e **D37**
(imagem/música por IA; recomendação C agora), fichas em `riscos-e-decisoes.md`.

Legenda: **[F]** fato verificado no repositório, no kit ou na ferramenta; **[I]** inferência nossa; **[P]** proposta a
confirmar na entrega.

## 0. Resumo em 10 linhas

1. O pedido, nas palavras do usuário (2026-11-25): "eu aprovo o personagem, mas não quero esse visual low poly, jogos
   com mecanicas já feitas exigem um nível de polimento grafico melhor para cativar o usuário"; "o padrão visual deve
   cobrir todos os outros pontos, como inimigos, portais, piso, background, efeitos sonoros, pense nas LLMs que temos
   disponível e o que seria necessário para fazer isso de forma mais autonoma possivel"; "Sim, escreve o plano".
2. Direção v3: **brinquedo de vinil premium** — formas lisas, arredondadas e chanfradas, sombreado suave (não mais 3
   faixas duras), brilho de borda e reflexo suave no personagem, cenário em plástico fosco, luz e céu por mundo.
3. Uma **fonte única de estilo como dado** no Unity (`StyleGuide`): paletas por mundo em 3 níveis, 4–5 materiais
   prontos, linguagem de formas, luz/céu/pós por mundo, efeitos e som. Um **validador recusa** o que sai do guia, com as
   regras ligadas por área à medida que cada área migra.
4. **Bibopzi em dois níveis**: detalhado (3–5 mil triângulos; líder, menus, loja) e de multidão (800–1.200); rosto como
   textura com expressões ligadas a eventos; amassar/esticar, atraso da mola e mãos que acompanham, por código.
5. **Inimigos, torres e chefes** arredondados e chanfrados, sem rostos (D17/D28). Se mantêm os temas por mundo (pedra,
   pântano, fundição, gelo) ou viram "fábrica de brinquedo" com as primitivas do Bibopzi é pergunta da D36.
6. **Quem faz**: Fable/Opus dirigem e criticam (sessão diferente da que implementou); Opus nas entregas de motor,
   Sonnet nas áreas; Blender 5.3 sem interface por script; construtor do kit evoluído; som sintetizado ou CC0.
7. **Ciclo autônomo** por área: gerar → capturas com câmeras fixas → crítica por agente com visão contra a imagem-alvo
   e o checklist → corrigir → gate 3× no OPPO → aceite do usuário em **3 pontos agrupados**, sem bloquear a série.
   A crítica por imagem **não** substitui o olho humano e o modelo **não ouve**.
8. O usuário entra em: imagem-alvo e o que ela implica (D36), 3 aceites agrupados, escuta do som, gastos (D37), jogar
   no OPPO.
9. Estimativa [I]: **16 entregas (13 do jogo + 3 do kit), 13,75 semanas base, ~16,5 com a margem**; caminho crítico
   ~15,3 (o som corre em paralelo); com ~1 semana de espera da D36 e ~1 de festas, fim por volta de **início de
   abr/2027**. O soft launch (meados de mai/2027) e o lançamento de jun/2027 se mantêm **se** a série terminar até
   meados de abr; depois disso, deslizam (seção 8).
10. Riscos principais: **custo por pixel** do shader e do pós no Mali do OPPO (medido antes da D36), e **o kit Toon
    ter outro consumidor ativo (match-3)**: o shader novo é irmão, o `ToonLit` fica intacto (seção 7).

## 1. Ponto de partida [F]

- Multidão instanciada no kit (`studiokit` 0.14.0, `Runtime/Swarm`, `Runtime/Toon`): 1 chamada para qualquer contagem
  (rota buffer, Vulkan) ou blocos de 256 (rota GLES, `ToonInstanceRouting`); `StudioKit/ToonLit` com rampa 3×1 de faixas
  duras, cor por vértice com máscara de tinta no alfa, **sem UV e sem textura**. `ToonInstanceData` tem 64 bytes,
  espelhado em C#; `tint.w` já é usado (escala da sombra, `SwarmDying`).
- O kit Toon tem **três** consumidores: este jogo, o match-3 (`Runtime/GridView`: `GridInstanceBatcher`,
  `GridPieceLook`, `GridEffects`, consumido por `file:`, com entregas recentes) e, indiretamente, a aceitação do Wild
  Pack (que não usa `Toon`/`Swarm`, T-RT6t-1, mas roda os validadores do kit).
- Formas: receitas no `UnitShapeRecipes` (200–500 triângulos) pelo `LowPolyMeshBuilder` (poucos segmentos, sem chanfro,
  sem UV). BIBOPZI é a forma padrão desde a RT-6t.22 (490 triângulos); as outras 5 seguem à venda (D31/D32).
- Quique da multidão calculado na CPU (`SwarmAnimateJob`, Burst) e entregue como matriz: o shader não conhece a fase.
- Teto da multidão: 300 (`ArmyLimit.Units`). Gate 3× (`Tools/rt2-gate.sh`): GPU p95 ≤ 11 ms, CPU principal p95 ≤ 11
  ms, quadro p95 ≤ 16,7 ms. Medido: RT-6t.1, 1000 × ~500 tri com GPU p95 5,5 ms (2000: 8,2 ms); RT-6t.17, mundo 4 com
  300 em voo, GPU p95 ≤ 4,3 ms. O volume de triângulos é o risco menor; o incerto é o custo por pixel e o pós.
- Torres, defesas e chefes: `ToonLit` com pesos de estrutura (D28 A: sem refazer modelos, temas por mundo mantidos);
  sem rostos (D17). Público declarado 13+ "não destinado a crianças" (D25 A), ícone sem rosto em destaque.
- Som: síntese própria (`Tools/audio/clay_siege_audio.py`), registrada em `audio-licenses.md` (D22 A).
- Telas: uGUI + TextMeshPro com Baloo 2 (RT-6t.17). Ícone e capturas da Play: RT-9a (`docs/store/`), visual atual.

## 2. Guia de estilo como dado (uma fonte única)

### 2.1 O asset

`StyleGuide` (ScriptableObject do jogo, `Assets/Claybreak/Content/Style/StyleGuide.asset`) [P]; os tipos genéricos que o
motor lê (níveis de cor, perfis de material, perfil de luz) vão para o kit, sem nomear jogo nem tema. Tudo que tem cor,
material, luz, efeito ou som no jogo **referencia** o guia; nada tem cor "solta" no código ou num `.mat`.

| Seção | Conteúdo | Quem lê |
|---|---|---|
| **Paletas por mundo** | 4 mundos × papéis (multidão, inimigo, estrutura, piso, faixa, cenário 1/2, céu topo/base, destaque e tinta de UI); cada papel em **3 níveis**: base, sombra, destaque. A paleta vale **antes do pós** (o tonemapping desloca a cor na tela) | aplicador de paleta evoluído, telas |
| **Materiais prontos** | 4–5 perfis: **vinil** (personagem: borda, especular suave, sombra quente), **plástico fosco** (cenário, piso, torres), **brilho de portão** (translúcido, borda acesa, sem escrever profundidade), **inimigo** (vinil escurecido, borda na cor de ameaça), **UI/efeito** (sem luz). Mesma luz e mesma rampa **suave** | shader irmão do kit (K.v3a), validador |
| **Linguagem de formas** | tudo arredondado e chanfrado (raio mínimo de chanfro por classe); nada de quina viva; proporções de referência do Bibopzi; primitivas comuns (feijão, engrenagem, mola, parafuso, porca) | receitas, Blender, revisão |
| **Luz, céu e pós por mundo** | luz principal (direção, cor), ambiente de 2 tons, céu (degradê + 1 camada de nuvens), pós barato: tonemapping neutro, cor, vinheta leve; bloom só se passar no gate | aplicador de mundo, Volume do URP |
| **Efeitos** | assinatura comum: **confete de engrenagem** (portão bom, vitória), **estalo de mola** (perda), poeira em anel (pouso), faísca de parafuso (acerto na torre); cores da paleta; limite de partículas por efeito e por tela | pool de efeitos (regra 2.2.1) |
| **Som** | assinatura "brinquedo de corda" (catraca, mola, sino curto); tabela evento → som; alvo de volume aproximado e pico por papel; música por mundo com o mesmo motivo de 4 notas | `ClaybreakAudio`, `audio-licenses.md` |
| **Migração** | por classe (unidade, inimigo, estrutura, portão, obstáculo, cenário, efeito, som, tela): `legado` ou `v3` | validador |

### 2.2 Validador do guia

`StyleGuideValidation` [P], chamado pelo `ClaybreakValidation` (e pelo `rt4-check`). Regras **sempre** ligadas: todo
material, efeito e clipe do jogo tem entrada no guia; nada fora de `audio-licenses.md`; orçamento por classe (4.1). Regras
ligadas **só nas classes marcadas `v3`** (cada entrega liga as suas): shader fora dos perfis, cor fora da paleta (ΔE CIE76
≤ 3 sobre os 3 níveis, medida antes do pós), malha sem chanfro (ângulo entre faces vizinhas acima do limite da classe,
salvo arestas marcadas), efeito acima do limite, clipe fora do alvo de volume/pico. Depois do pós, conferir de novo a regra
"skins com ΔE ≥ 20 da pista e dos portões" e a leitura dos números dos portões (captura + medida de contraste).

## 3. Personagem: Bibopzi em dois níveis

### 3.1 Modelos e pipeline

| Nível | Triângulos | Onde |
|---|---|---|
| **Detalhado** | 3.000–5.000 | líder da multidão, home, loja, resultado, ícone |
| **Multidão** | 800–1.200 | as 300 unidades da corrida e do assalto |
| **Reserva** | ~500 | a receita atual (RT-6t.22) com normais suaves e chanfro, se o gate reprovar a de multidão |

Pipeline determinístico [P]: scripts Python versionados em `Tools/blender/`, rodados com Blender **5.3 fixo**
(`C:\Program Files\Blender Foundation\Blender 5.3`, `blender -b -P`) [F: instalado]. O script **não** exporta FBX (o FBX
grava data e o importador do Unity reordena e solda vértices): grava **dados simples** (posições, normais, UV, UV2, cor em
espaço linear declarado, índices) num JSON/binário versionado; um gerador de Editor do jogo grava o `.asset` de malha, como
o `UnitShapeAssets` faz hoje. A validação do jogo lê os dados versionados e **não depende do Blender instalado**. Testes:
contagem de triângulos, marcos (`BibopziHasItsLandmarks` nos três níveis), eixo e escala.

Marcos obrigatórios em todos os níveis (o que afasta de Fall Guys/Among Us/Baymax, risco anotado no personagem): topete de
mola, engrenagem na têmpora direita dele, barriga creme, mãos soltas.

**As outras formas da loja** (D31/D32): ganham a versão lisa de multidão (mesma silhueta, preços intocados) na RT-6v3.4c;
quem veste outra forma usa o nível de multidão também como líder e nas telas, e os olhos por geometria como hoje
(sem atlas de expressões). Só o Bibopzi tem detalhado e rosto.

### 3.2 Rosto como textura

- Atlas pequeno (256×256 [P]) com 8 expressões: neutro, feliz (portão bom), susto (portão ruim/obstáculo), esforço,
  vitória, derrota, piscar, "ui!" (lançado no assalto); células com margem e mipmaps (na multidão o rosto tem poucos
  pixels); a área do rosto fica com máscara 0 (não pega a skin).
- Exige o que o kit não tem hoje: UV na malha e amostragem de textura no shader, e **um campo novo por instância** (o
  `tint.w` está ocupado): struct maior na rota de buffer e propriedade instanciada nova na rota de blocos de 256 (cabe no
  bloco de 16 KB). É a entrega **K.v3b**, medida nas duas rotas.
- Eventos do jogo → expressão por tabela no guia (dado), com duração e prioridade.

### 3.3 Animação por código (sem esqueleto)

- **Amassar e esticar**: já existe no quique (`SwarmMotionSettings`); ganha amassar no pouso e esticar na subida, com
  volume preservado.
- **Atraso da mola e mãos**: mola amortecida de verdade precisa de estado; o plano usa **atraso analítico de fase**: o job
  da CPU (que já roda por unidade) escreve fase e velocidade do quique por instância no buffer (K.v3b); o shader de
  vértice desloca os vértices marcados do topete com atraso de fase e as mãos em fase oposta (abrindo no susto/vitória).
  Pesos dos vértices em **UV2** (o alfa da cor é a máscara de tinta). Custo fixo, sem CPU extra relevante.
- Fórmulas puras, testáveis em EditMode, como as do kit hoje.

## 4. O resto do jogo no mesmo padrão

### 4.1 Orçamentos por classe [P, confirmados pelo gate]

| Classe | Triângulos | Chamadas |
|---|---|---|
| Unidade de multidão | 800–1.200 | 1 (+1 sombra) como hoje |
| Líder e telas | 3.000–5.000 | 1 |
| Inimigo comum (defesa) | 1.500–3.000 | 1 por tipo |
| Torre | 6.000–12.000 | ≤ 4 |
| Chefe | 8.000–15.000 | ≤ 4 |
| Portão (par) | ≤ 2.000 | ≤ 2 |
| Obstáculo | ≤ 2.500 | 1 por tipo |
| Peça de cenário | 300–1.500 | 1 por malha (instanciado) |

### 4.2 Áreas

- **Inimigos, torres e chefes**: arredondados e chanfrados, sem rosto (D17/D28), leitura de ameaça por cor de borda e luz;
  quebra em peças pré-alocadas ao perder vida (sem física). **Tema** (manter pedra/pântano/fundição/gelo, ou "fábrica de
  brinquedo" com as primitivas do Bibopzi): pergunta da D36, porque a D28 A manteve os temas.
- **Portões**: arco de vinil chanfrado, painel translúcido com borda acesa, número em Baloo 2 com contorno e relevo; cores
  bom/ruim do guia.
- **Obstáculos**: os 5 tipos (RT-6t.18) no padrão, sem mudar regra nem tempo.
- **Piso e pista**: faixas chanfradas com juntas de peças encaixadas; trilhos arredondados.
- **Cenário e céu**: árvores e pedras de brinquedo [P, sujeito à D25: nada que pareça doce/infantil demais], 1 camada de
  nuvens recortadas, luz por mundo.
- **Efeitos**: tabela da 2.1, todos com pool.
- **Telas e ícone**: botões de vinil (relevo, sombra), Bibopzi detalhado com expressões; ícone: a D25 pede "sem rosto em
  destaque" e o personagem tem rosto — **a pergunta vai na D36** (com 2–3 esboços de ícone), não no fim da série.
- **Som e música**: efeitos "brinquedo de corda", música por mundo com motivo comum.
- **Público (D25)**: robô bebê com 8 expressões e cenário de brinquedo aumentam a leitura infantil que a D25 quis evitar
  (13+, "não destinado a crianças"); o risco vai no contexto da D36, junto da imagem.

## 5. Quem faz o quê

| Papel | Quem | O que faz | O que **não** faz |
|---|---|---|---|
| Direção de arte, crítica, fichas | **Fable** ou **Opus**, sempre numa **sessão diferente** da que implementou | escreve o guia, compara capturas com a imagem-alvo e o checklist, escreve fichas | não aprova uma área: o aceite é do usuário |
| Motor (kit) e entregas de risco | sessões **Opus** (K.v3a–c, 6v3.2, 6v3.3, 6v3.4a/b) | shader irmão, layout de instância, construtor, conceitos medidos, personagem | não toca o `ToonLit` nem o layout atual (match-3) |
| Áreas do jogo | sessões **Sonnet** (6v3.1, 6v3.4c, 6v3.5–6v3.11), Opus se a área travar | materiais, receitas, efeitos, telas, som; skills `unity:craft-game-floors` (acabamento, juice, som, telas) e `unity:urp-postprocessing` (Volume, tonemapping, cor). **As regras do projeto (2.2.1, kit só por entrega K, gate) prevalecem sobre a skill** | não mexem no `studiokit` |
| Modelos detalhados | **Blender 5.3** sem interface, scripts em `Tools/blender/` | dados de malha versionados + relatório | nada de modelagem à mão sem script |
| Formas lisas da multidão | construtor do kit evoluído (**K.v3c**: perfil/torno, chanfro, normais suaves, UV) | receitas do jogo | — |
| Imagens de conceito (opcional) | gerador de imagem do **vidIQ** só se a **D37** aprovar | inspiração; nunca a folha do personagem como referência sem os termos lidos | nada no build |
| Música (opcional) | gerador de música do **vidIQ** só se a **D37** aprovar | temas por mundo | idem; o padrão é síntese própria ou CC0 |
| Efeitos sonoros | síntese própria (como hoje) ou **CC0** | cada arquivo em `audio-licenses.md` | nada pago, nada sem licença |

Custos [F, consulta só de leitura em 2026-11-25]: no vidIQ, 1 imagem custa **22 créditos** (mesmo preço nos 4 modelos de
imagem listados); a conta tem **18 de 150** créditos no ciclo atual (renova em 15/12/2026): hoje não dá para 1 imagem.
O gerador é feito para miniaturas de YouTube; a utilidade para conceito de jogo é incerta [I]. Preço da música e termos
de licença: **não verificados**.

## 6. Ciclo autônomo por área

```
gerar (script / receita / shader)
  → capturas automáticas com câmeras fixas (mesmas posições, sementes e instante; PlayMode com GPU)
  → crítica por agente com visão (outra sessão): imagem-alvo (D36) + referência do gênero + checklist do guia + antes/depois
  → corrigir (até 3 voltas; se não convergir, ficha com as capturas)
  → validador do guia + validação padrão (`rt4-check`)
  → gate 3× no OPPO (uma vez por entrega, no caso mais pesado; janela combinada com o match-3, que usa o mesmo aparelho)
  → segue para a próxima entrega; o aceite do usuário vem nos 3 pontos agrupados
```

- **Câmeras fixas** [P]: `StyleCaptureRig` com 6 enquadramentos por área (corrida com 300, assalto com 300 em voo,
  líder de perto, torre de perto, telas em 4 proporções, ícone 512).
- **Checklist** gerado do próprio `StyleGuide` (uma linha por regra), para a crítica não inventar critério.
- **Aceites agrupados (não bloqueantes)**: (1) depois da 6v3.4b — luz e personagem; (2) depois da 6v3.7 — mundo
  (inimigos, portões, obstáculos, piso, cenário); (3) depois da 6v3.10 — efeitos, telas, ícone, som. A série segue
  enquanto o usuário olha; retrabalho de um "não" é assumido no ponto seguinte.
- **Limites, ditos às claras:** a crítica por imagem pega desvio de cor, forma e composição, mas **não** substitui o
  olho humano ("parece premium?"), nem vê movimento; o modelo **não ouve**: som é medido por volume, pico, duração e
  espectro grosseiro, e a escuta é do usuário.

## 7. Riscos

| Risco | Chance [I] | Impacto | Mitigação |
|---|---|---|---|
| Custo por pixel (borda, especular, pós) no Mali do OPPO | média | alto | ensaio no OPPO **antes** da D36 (6v3.2: 300 × 1.200 tri, pós ligado/desligado, duas rotas); reserva: sem bloom, rampa simples, LOD (1.200 perto, ~500 longe) |
| Kit Toon compartilhado com o match-3 | alta (é preciso mudar o kit) | alto | **shader irmão** e **layout de instância novo e opcional**; `ToonLit` e `ToonInstanceData` atuais intactos; aceitação das K.v3 inclui o match-3 e o Wild Pack; uma sessão escrevendo no `studiokit` por vez |
| Imagem-alvo que o jogo não alcança | média | médio | conceitos feitos no Unity com shader de laboratório local e medidos no aparelho |
| Semelhança do Bibopzi com outros personagens em multidão | média | alto | marcos sempre visíveis (teste nos 3 níveis) |
| Público da Play (D25) com visual mais "fofo" | média | alto | pergunta na D36, com o ícone; nada de doce/infantil no cenário sem resposta |
| Licença de imagem/música por IA | média | alto | D37 (recomendação C agora); padrão próprio/CC0 |
| Esperas do usuário | alta | médio | aceites agrupados e não bloqueantes; só a D36 bloqueia |
| Arquivos compartilhados (cena regravada inteira, `StyleGuide`, `ArtPalette`, `ClaybreakSceneBuilder`) e 1 OPPO | alta se em paralelo | médio | áreas em série; paralelo só o som (6v3.9) e a geração de malhas no Blender |
| Capturas da loja (RT-9a) ficam velhas | certa | baixo | refeitas na 6v3.11; a ficha de produção não é publicada antes |

## 8. Calendário [I]

- Série: **13,75 semanas base, ~16,5 com 20%** (tabela no plano); o som (1,2 com margem) corre em paralelo, então o
  caminho crítico é ~15,3. Somando ~1 semana de espera da D36 (bloqueante) e ~1 de festas: **~17,3 semanas** a partir de
  1/dez/2026 → fim por volta de **início de abr/2027**. Não incluído: o resto da RT-9b (passos do usuário no Console) e
  incidentes do Wild Pack ou do match-3 no kit.
- Referência do plano original: RT-9 em início de mai/2027, soft launch (RT-10) de meados de mai a meados de jun,
  lançamento ~27/jun. Folga entre o fim previsto da série e meados de abr: **~2 semanas**; até o soft launch, ~6.
- Conclusão honesta: o lançamento de jun/2027 **se mantém se a série terminar até meados de abr/2027** (aceites
  agrupados respondidos em ~1 semana, gate sem reserva pesada). Um gate reprovado com reserva, ou a D36 voltando com
  "outra rodada", consome a folga; acima de ~2 semanas de atraso, o soft launch desliza para jun e o lançamento para
  jul/2027. Mitigação: cortar escopo (ex.: rosto com 4 expressões, música mantida por síntese) antes de cortar prazo.
- O teste interno da Play (RT-9b) segue com o visual atual.

## 9. Revisão independente

Revisor: fable (só leitura), 2026-11-25, sobre a primeira versão deste plano.

### 9.1 O que a revisão apontou e o que mudou

- **Alto** — o kit Toon tem outro consumidor (match-3): shader irmão e layout novo opcional obrigatórios; match-3 na
  aceitação das K; janela de escrita no `studiokit` e do OPPO combinada (seções 1, 5, 7; entregas K.v3a–c).
- **Alto** — a D36 seria escolhida sem medição: os conceitos passam a ser feitos no Unity com shader de laboratório local
  e ensaiados no OPPO; a ficha só sai com o número de cada opção; "Fato" virou "Inferência" (6v3.2, D36).
- **Alto** — a conta de calendário não fechava com as esperas: aceites agrupados em 3 pontos e não bloqueantes, orçamento
  de espera declarado, estimativa refeita com as K e a divisão da 6v3.4 (seção 8).
- **Alto** — "D37 A agora" não era executável (18 créditos, imagem a 22): recomendação passou a C agora, A só depois de
  15/12 com os termos lidos, teto de créditos e sem enviar a folha do personagem (D37).
- **Médio** — temas das torres (D17/D28) e público (D25, ícone) entram como perguntas da D36; validador com regras por
  classe migrada e paleta medida antes do pós; rosto e atraso da mola descritos com o que o kit realmente precisa (UV,
  campo novo, fase no buffer, UV2); Blender grava dados simples, não FBX; 6v3.4 dividida em a/b/c (com as outras formas);
  K como entregas próprias; paralelo só no som e no Blender.
- **Baixo** — "8 das 13 entregas do jogo" travadas pela D36 (o som não depende); D36 marcada como rascunho; skill
  `unity:craft-game-floors` nos prompts de acabamento; modelo por entrega; referência do gênero na imagem-alvo; teto do
  gate citado ao lado das medidas.
- Fora do escopo, anotado: o `CLAUDE.md` do jogo ainda diz kit 0.1.0 (está em 0.14.0).

### 9.2 Segunda opinião (fable), citada nas fichas

- **D36:** concorda em decidir só com as imagens na mão, com três condições: telas feitas no Unity com o número medido
  no OPPO de cada opção; a ficha pergunta junto o tema das torres e chefes (D17/D28) e o risco de público (D25), com o
  ícone; e uma referência do gênero lado a lado. Não enviar antes da RT-6v3.2.
- **D37:** discorda de "A agora": com 18 créditos e a imagem a 22, nada sai antes de 15/12 sem comprar crédito ou atrasar a
  D36; os termos não foram lidos; a imagem-alvo sai do motor de qualquer forma. Recomenda C agora; A depois de 15/12 com
  termos lidos, teto de créditos e sem enviar a folha do personagem; música só na RT-6v3.9, com preço e licença
  conferidos e comparada com CC0 ou pacote de licença clara; o modelo não ouve, a escuta é do usuário.
