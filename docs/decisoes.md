# Registro de decisões

Formato: data — id — título — opção — quem decidiu. O texto aprovado vai inteiro.
Decisões técnicas tomadas pelo maestro/sessões ficam marcadas "técnica".

## 2026-11-19 — K1 — Como nasce o studiokit — **A** — usuário

**Texto aprovado:** o studiokit nasce de dentro do Wild Pack (repo MultiplierRunner):
(1) congelar a versão de teste interno atual com uma tag git; (2) organizar o código do
Wild Pack em blocos separados motor × jogo (asmdefs e namespaces), sem mudar comportamento,
conferindo com os validadores existentes e um teste de compatibilidade de save; (3) mover
os blocos de motor para o repositório `studiokit` (github.com/pesconi/studiokit), com os
`.meta` preservados; (4) o Wild Pack passa a usar o kit e o Runner vs Tower nasce em cima
dele.

**Ajustes vindos da segunda opinião (Fable), incorporados:**
- Não dividir o `GameManager` (1.660 linhas) agora: ele fica como módulo de jogo do Wild
  Pack. Os pontos de extensão do motor ("regras da partida": inimigo persistente, portão
  que dispara ação, condição de vitória, HUD e save por jogo) só são nomeados depois que
  existir o segundo jogo (Runner vs Tower), a partir da comparação entre os dois.
- Kit v0 = só o que já é comum (pool, multidão, visual, UI clay, áudio, anúncios,
  medição). Portões, percurso e fluxo de jogo ficam no jogo até o segundo uso.
- Distribuição por caminho local (`file:../studiokit`) enquanto a API muda; versão por tag
  git só quando o Wild Pack migrar.
- O kit tem um projeto de teste mínimo próprio para que mudanças sejam verificáveis sem
  abrir os jogos.

**Efeitos:** substitui a decisão D11 (resolvida como A). A entrega RT-1 deste plano passa a
ser executada pelo plano de entregas do repositório `studiokit`. Ordem do estúdio definida
pelo usuário: studiokit antes das demais decisões do Runner vs Tower.

## 2026-11-19 — Contexto do estúdio — usuário

Jungle Degree é uma empresa de duas pessoas, sem prazo definido. As datas do roadmap são
inspiração, não compromisso.

## 2026-11-20 — D3 — Autorizar o segundo app — **A** — usuário

**Texto aprovado:** o estúdio desenvolve e pretende publicar o Runner vs Tower como segundo
app. A regra do SISTEMA.md do Wild Pack ("publicar vários apps quase idênticos" fora do
escopo até decisão contrária) passa a significar: não fazer clones do mesmo jogo. Antes do
soft launch conferimos a política da Play e a identidade do app.

**Nota:** a política de Spam da Play lista como violação "criar vários apps com
funcionalidade, conteúdo e experiência muito parecidos"; o efeito na conta única é inferência
nossa. Se as duas lojas usam a mesma conta é a D7 (pendente).

## 2026-11-20 — D2 — Gancho do jogo — **D (adiar)** — usuário

**Texto aprovado:** a escolha do gancho espera a varredura de loja do RT-0 (1–2 h, gratuita).
A ficha D2 volta ao usuário com o resultado. Direção provisória (não decidida): Opção A com
traço da B nos chefes e protótipo com B alternável.

## 2026-11-20 — D16 — Critério de parada do protótipo — **A** — usuário

**Texto aprovado:** o Runner vs Tower para e voltamos à pesquisa de jogos se o protótipo
falhar no portão RT-3 (menos de 3 de 5 testadores pedem "de novo" sem incentivo). Antes de
parar, reduzimos limites por dados para tentar passar no gate 3× do OPPO; se o jogo agradar
mas o gancho não, testamos a Opção B uma vez (+1 semana). Critérios pós-lançamento
(retenção, receita) ficam para outra decisão antes do RT-9. Sem teto de esforço por ora.

## 2026-11-20 — D2 — Gancho do jogo (reapresentada após a varredura) — **A** — usuário

**Texto aprovado:** o protótipo (RT-2) será construído com a Opção A: o jogador corre uma
fase de verdade que define a quantidade e a composição do exército, e depois o exército é
lançado contra uma torre com vida e defesas. A promessa de venda é a torre, não a corrida.
Um traço da Opção B (a torre mexe nos portões) só nos chefes, e o protótipo deixa a B
alternável para teste.

**Base:** `docs/varredura.md` (2026-11-20): "corra e depois ataque" é padrão do gênero; não
foi achado jogo que una corrida real e assalto a torre com defesas. Limite: só texto de
fichas, sem capturas. Substitui a linha "D2 — adiar" acima; o portão RT-3 (D16) continua
sendo o teste de que o gancho é sentido como diferente.

## 2026-11-20 — D14 — Plataforma da primeira versão — **A** — usuário

**Texto aprovado:** a primeira versão do Runner vs Tower sai só para Android (Google Play).
iOS fica fora até uma decisão nova, depois de ver o resultado do Android.

## 2026-11-20 — D15 — Prioridade entre Wild Pack e Runner vs Tower — **B** — usuário

**Texto aprovado (opção B, a recomendação era A):** os dois jogos andam no mesmo ritmo; o
Wild Pack não passa automaticamente à frente quando precisar de atualização. Sem prazo fixo
para o novo jogo.

**Risco registrado:** um incidente no Wild Pack (bug, política da Play, anúncios) pode ficar
esperando enquanto o novo jogo avança (chance baixa, impacto alto: a conta da Play é a mesma).
**Como mitigar sem mudar a decisão:** o maestro avisa o usuário na hora em que um incidente
do Wild Pack aparecer, e a prioridade naquele caso volta a ser decisão dele.

## 2026-11-20 — D1 — Nome do jogo — **nome de trabalho "Claybreak"** — usuário

**Texto aprovado:** o nome de trabalho do jogo passa a ser **Claybreak** (antes: "Runner vs
Tower"). É provisório: a checagem oficial de marca (bases de marcas) e a varredura de
colisão na Play ainda precisam ser feitas antes de gastar com arte, ícone ou domínio
(RT-4 e RT-9).

**Base:** `docs/varredura.md` (busca web de 2026-11-20, não oficial): nenhum app com o nome
exato; existem o jogo de navegador *Claybreak Range* e o histórico *Clay Jam* (colisão
baixa a média).

**Não alterado nesta decisão:** o nome do repositório (`runner-vs-tower`), da pasta, o
application ID provisório (`com.jungledegree.runnervstower`) e o nome de exibição. Renomear
fica para quando o nome for confirmado pela checagem de marca; o application ID, em
especial, não deve mudar depois da primeira publicação.

## 2026-11-20 — D4 — Medição de jogadores (analytics) — **A** — usuário

**Texto aprovado:** o Claybreak terá, a partir da RT-7, medição mínima e consentida: só D1 e
D7 (quem volta 1 e 7 dias depois de instalar), aceite do anúncio recompensado e início/fim
de fase (onde o jogador começa e desiste). Ferramenta gratuita, escolhida numa ficha própria
antes da RT-7; nada é instalado antes disso.

**Ajustes da segunda opinião (Fable), incorporados:** funil de fases (início/fim) incluído
para o ajuste de dificuldade da RT-10; definir um número mínimo de instalações antes de ler
D1/D7 (com poucas dezenas, o número é ruído); o Claybreak terá política de privacidade e Data
safety próprias, declarando tudo o que o SDK coleta (a do Wild Pack diz "sem analytics").
Play Console não dá D1/D7 de "voltou a jogar" (memória, não verificado).

## 2026-11-20 — D5 — Recompensas de retorno — **B** — usuário

**Texto aprovado:** só no Claybreak (a regra do Wild Pack continua valendo lá), reabrir o
escopo para uma **recompensa diária** simples, local, sem servidor e sem punição por faltar.
Missões diárias só se o D7 do soft launch ficar abaixo da meta (12%). Eventos temporários só
depois do lançamento.

**Riscos registrados:** sem notificação push (regra do GDD), a recompensa diária tem efeito
menor; relógio local pode ser burlado (impacto baixo, jogo sem ranking); valores dependem da
economia da RT-5.

## 2026-11-20 — D6 — Monetização da v1 — **B** — usuário

**Texto aprovado:** a v1 terá anúncio recompensado (opcional, ganho explicado antes) e
anúncio intersticial entre fases com limite. O intersticial fica **pronto mas desligado** na
primeira turma do soft launch, até haver uma medida limpa de retenção; os limites (mínimo de
fases e intervalo) são definidos depois do RT-3, com o eCPM real do Wild Pack. **Sem compra
dentro do app** na v1 (nem "remover anúncios"); reavaliar com dados do soft launch.

**Por quê:** compra no app exige Play Billing, perfil de comerciante, dados fiscais,
testadores de licença, restauração de compra sem backend e suporte a reembolsos — peso
contínuo para duas pessoas. O intersticial ligado cedo pode derrubar o D1/D7 que o soft
launch quer medir (segunda opinião do Fable).

**Risco registrado:** avaliações reclamando de anúncio quando o intersticial for ligado sem
opção paga de remoção (chance média, impacto baixo; limites conservadores).

## 2026-11-20 — T-RT2-1 — Variante de lançamento do protótipo — **L1 "soltar em ondas"** — técnica (sessão RT-2)

L1 (arrastar escolhe a faixa, segurar solta um fluxo contínuo) em vez de L2 (avalanche de um
toque). Motivo: a fileira de portões do assalto e a muralha só viram decisão se o jogador pode
trocar de faixa enquanto lança; L1 também é o caso mais caro da entrada. L2 cabe depois como
variante se o RT-3 pedir. Detalhes em `docs/rt2-prototipo.md` seção 2.

## 2026-11-20 — T-RT2-2 — Portões do Claybreak escritos no jogo, não levados ao kit — técnica (sessão RT-2)

Os portões e o percurso do Wild Pack ficaram fora do kit (K1) presos ao `GameManager` dele.
Levá-los ao kit agora exigiria desmontar o fluxo do Wild Pack e passar pelo aceite dele, grande
e arriscado para um protótipo de uma semana; a regra aqui é outra (o portão também age sobre um
fluxo de unidades no assalto). Código pequeno no jogo (`GateMath`, `RunGateCourse`); a comparação
para a pauta do kit está em `docs/rt2-prototipo.md` seção 4.

## 2026-11-20 — T-RT2-3 — Sons placeholder sintetizados — técnica (sessão RT-2)

O kit 0.1.0 só tem o motor de som; os clipes são conteúdo do Wild Pack e não podem ser copiados.
O construtor do protótipo gera WAVs simples por código (sem fonte de terceiros, sem licença a
controlar). Trocados por áudio de verdade na RT-8.

## 2026-11-20 — T-RT2-4 — ID de app de exemplo do AdMob no protótipo — técnica (sessão RT-2)

O SDK de anúncios de que o kit depende exige um ID de app no manifesto Android. O protótipo não
mostra anúncio; usa o ID público de exemplo do Google (`ca-app-pub-3940256099942544~3347511713`),
que não pertence ao estúdio. IDs reais só na RT-7 (decisão D6).

## 2026-11-20 — T-RT2-5 — Medição do gate no protótipo — técnica (sessão RT-2)

APK de medição = release com `RELEASE_MEASUREMENT`; nível próprio (`DeviceGateLevel`) que começa
sozinho e mantém 300 unidades em voo e o canhão no limite de 12 pedras pelo tempo da medição;
script `Tools/rt2-gate.sh` sobre as ferramentas do kit. Falhou → baixar limites por dado
(unidades em voo, depois pedras), nunca o gate.

## 2026-11-20 — T-RT2-6 — "Multidão" mínima do gatilho de parada do R3 — técnica, a confirmar no RT-3

150 unidades em voo (metade do pool). Se o gate 3× só passar abaixo disso, o gatilho de parada do
R3 (`riscos-e-decisoes.md`) dispara e o RT-3 decide.

## 2026-11-20 — D13 / RT-3 — Portão de teste com 5 testadores — **dispensado** — usuário

Pergunta: o RT-3 (5 testadores de fora, portão "3 de 5 pedem de novo") é preciso para publicar?
Resposta dada: não é exigência da Google Play; era um portão interno para evitar investir ~17 semanas
num jogo que não agrada. Opções apresentadas: leve (5 pessoas do círculo, 1 fim de semana —
recomendada), mínimo (só o casal decide), juntar com o teste fechado da Play.
**Decisão do usuário:** seguir sem os testes. Ele jogou o protótipo, gostou, venceu na 2ª tentativa
e quer terminar o projeto de qualquer forma ("é um projeto legal que quero terminar").
Consequências: RT-3 dispensada; D13 deixa de existir; o critério de parada de D16 (que era só o
portão RT-3) fica sem gatilho antes do lançamento — os critérios pós-lançamento continuam para
antes do RT-9. Risco aceito: o jogo só foi validado pelo próprio estúdio; a primeira opinião de fora
virá no teste fechado da Play (se a conta exigir) ou no soft launch (RT-10). T-RT2-6 (multidão mínima
de 150) fica como limite técnico, sem portão: o gate 3× já passou com 299 em voo.

## 2026-11-20 — RT-4 — Aceite da fatia sem os testadores da RT-3 — ajuste de plano (sessão RT-4)

Com a RT-3 dispensada (D13), o aceite da RT-4 "segundo teste informal com os mesmos 5 + 2 novos" não
tem mais a quem se referir. Troca registrada no plano: **o usuário e a Flávia jogam a fatia no OPPO e
respondem "voltaria amanhã?"** (sem critério numérico).

## 2026-11-20 — T-RT4-1 — Fatia vertical numa cena só, conteúdo como código — técnica (sessão RT-4)

O mundo inteiro (7 fases, 3 corpos de torre, defesas, menu, HUD, resultado) vive numa cena só, com
tudo pré-alocado; trocar de fase reposiciona objetos, nunca cria (regra 2.2.1). As fases são escritas
em `Assets/Claybreak/Editor/WorldContent.cs` e o construtor regrava os assets a cada build: ajuste de
balanço vira mudança de código revisável, e um ajuste feito à mão no Inspector não se perde em silêncio.
Módulo renomeado de `Prototype/` para `Content/` (cena `Claybreak.unity`), com os `.meta` preservados.

## 2026-11-20 — T-RT4-2 — Menu, HUD e resultado escritos no jogo com as peças de UI do kit — técnica (sessão RT-4)

O kit (até UI.4) tem as peças de interface (`ClayUi`: pílula, sombra, brilho, aperto, área de toque,
ícones desenhados), mas não tem telas: `MainMenuView`/`GameResultView` ficaram no Wild Pack e falam de
moedas, nível e URL dele; levá-las ao kit exigiria desmontar o fluxo do Wild Pack com o aceite dele.
As telas deste jogo (`ClaybreakScreens`, `ClaybreakHud`) usam só as peças do kit. Candidatos ao kit
quando houver segundo uso: "tela inicial com cartões" e "resultado com estrelas".

## 2026-11-20 — T-RT4-3 — Sons próprios do jogo fora dos papéis do kit — técnica (sessão RT-4)

O motor de som do kit tem papéis fixos (`GameSound`). Queda da torre, tiro do canhão, andar caindo,
muralha, varredor, caixote e chefe são tocados por `ClaybreakSfx` (vozes pré-alocadas), respeitando o
mudo e a vibração do kit. Pauta do kit: uma tabela de "sons extras" no `ThemeAudio` eliminaria a classe.

## 2026-11-20 — T-RT4-4 — Unidade especial "Bruto", upgrade e save — técnica (sessão RT-4)

- Unidade especial (GDD 6 "Pesada"): o **Bruto**. Destrava ao vencer a fase 3; aparece em caixotes na
  corrida (gancho A: a corrida decide a composição); sai a cada 3º lançamento; maior, mais escuro, 4× de
  dano na torre e na muralha, aguenta 1 golpe (canhão ou varredor). Clones dos portões ×N são comuns.
- Upgrade (GDD 6): **exército inicial**, 6 degraus (+5 a +46 unidades), pago com moedas das fases.
- Save: JSON versionado (`SaveData` v1) no PlayerPrefs, leitura tolerante (save danificado ou de versão
  futura começa do zero, sem travar); valores fora da faixa são limitados. Preferências de som e
  vibração continuam nas chaves do kit.
- Depois do chefe o mundo recomeça com torres e muralhas 25% mais fortes por volta.
- Estrelas por sobreviventes (≥ 50% = 3, ≥ 20% = 2, senão 1); moedas = prêmio da fase + sobreviventes +
  bônus por estrela; derrota paga 1/8 do prêmio.

## 2026-11-20 — T-RT4-5 — Nível do gate com a torre mais pesada — técnica (sessão RT-4)

O nível de medição usa o corpo do chefe (o mais pesado), as três defesas ligadas (muralha, canhão com
12 pedras, varredor) e 300 unidades em voo recicladas. O validador reprova se faltar alguma condição.

## Fichas pendentes (aguardando o usuário)

### DECISÃO D17 — Direção de arte da torre e do chefe

O que você está aprovando: a aparência final da torre comum (fases 1, 2, 4 e 6) e do chefe (fase 7)
do primeiro mundo, entre as opções desenhadas; a escolhida vira a arte final da fatia e o modelo das
próximas torres.

Por que agora: a RT-4 pede arte final de pelo menos 1 torre e 1 chefe, e o plano exige a sua aprovação
da direção. Nada mais está travado: o jogo já roda com a opção recomendada (A e A) como provisória;
trocar é mudar duas linhas e reconstruir a cena.

Contexto que você precisa saber:
- Imagens: `docs/art/rt4/torre-opcoes.png` e `docs/art/rt4/chefe-opcoes.png` (letras A, B, C da
  esquerda para a direita); a torre A e o chefe A como aparecem no assalto (distância real de jogo)
  em `docs/art/rt4/jogo-assalto-torre.png` e `docs/art/rt4/jogo-assalto-chefe.png`; o menu em
  `docs/art/rt4/jogo-menu.png`. Cópias em `C:\temp\rt4\art\`.
- Todas são feitas de formas simples (blocos, cilindros, esferas) montadas por código — meio gratuito e
  próprio, sem ferramenta, asset ou geração de imagem paga (fato: é como a cena é construída).
- Cada torre tem 4 andares empilhados; cada andar que cai some de cima para baixo durante o assalto (é
  o que mostra o progresso). O canhão fica na frente.
- Direção do GDD (fato, seção 9 e seção 2): adulta, paleta escura e quente, torres com atitude
  (fortaleza, forja, fábrica, ruína), nada de castelo de brinquedo, sem rosto nem olhos.
- A forja (opção B da torre) já é a segunda torre do mundo (fases 3 e 5), seja qual for a escolha.

Opções (torre):
A) Bastião de pedra — fortaleza quadrada de pedra clara, ameias, estandartes vermelhos, tochas e um
   braseiro no topo (sem bandeira, depois da segunda opinião).
   + Lê bem de longe; contrasta com a forja das fases 3 e 5 e com a pista marrom.
   - Riscos: pode parecer "castelo genérico" (chance média; impacto baixo) — dá para dar atitude com
     detalhes depois (espetos, rachaduras). Volta atrás? sim, fácil.
B) Forja de ferro — tambores pretos com frestas brilhando, chaminé.
   + A mais "adulta" e marcante.
   - Riscos: torre comum e segunda torre ficariam iguais (chance alta; impacto médio: o mundo perde
     variedade) — exigiria desenhar outra segunda torre. Volta atrás? sim.
C) Ruína de barro — tambores de terracota tortos, rachaduras, musgo.
   + Casa com o tema massinha.
   - Riscos: a inclinação lê como "já quebrada" antes de levar dano (chance média; impacto médio na
     leitura do progresso). Volta atrás? sim.

Opções (chefe):
A) Cidadela gêmea — duas torres redondas laterais, coroa de espetos, portão com grade incandescente.
   + Silhueta claramente maior e diferente das torres comuns.
   - Riscos: parecida com o bastião A (chance média; impacto baixo) — a coroa e as torres laterais
     separam. Volta atrás? sim.
B) Colosso-forja — fornalha gigante com núcleo brilhando atrás de uma grade e duas chaminés.
   + Muito imponente.
   - Riscos: o brilho laranja compete com o aviso do canhão e com os portões (chance média; impacto
     médio na leitura). Volta atrás? sim.
C) Totem de barro — anéis de barro empilhados com chifres de osso.
   + O mais estranho e memorável.
   - Riscos: os chifres leem como pedaços soltos (chance média; impacto baixo); fica próximo da ruína C.
     Volta atrás? sim.
D) Adiar — segue com A e A provisórias até você jogar a fatia. Custo: a RT-4 fica com "arte final"
   pendente de aprovação.

Recomendação: torre A + chefe A — a dupla com melhor leitura nas 3 faixas e a poucos pixels, e o mundo
fica com três silhuetas distintas (bastião, forja, cidadela).
Segunda opinião (fable): concorda com o chefe A; discordou da torre A como estava (marrom sobre
cenário marrom, a de menor contraste, e com a bandeira lembrando "castelo de brinquedo") e pediu
captura na distância de jogo. Incorporado: pedra clara, bandeira trocada por braseiro, captura
`jogo-assalto-torre.png`. Sugeriu também, como alternativa, a ruína C "endireitada" (contrasta e casa
com o barro) — se preferir, responda "torre C reta".
Se você não responder: o jogo segue com A e A como provisórias; nada mais espera por isso.

Responda: "torre A, chefe A" (ou outra combinação), ou "explica melhor".

### DECISÃO D18 — O nome "Claybreak": seguir, trocar ou checar à mão

O que você está aprovando: o que fazer com o nome de trabalho "Claybreak" depois da checagem gratuita
de marca de 2026-11-20.

Por que agora: o plano pede a checagem antes de arte final e ícone. Não trava a RT-4: as telas do jogo
não mostram o nome (o menu diz "STORM THE TOWER"); só o nome do app instalado no celular ainda é
"Claybreak", para testes internos. Precisa sair antes do ícone e da ficha da loja (RT-9).

Contexto que você precisa saber (pesquisa gratuita de 2026-11-20):
- Lojas: existe um jogo de navegador **"Claybreak Range"** (tiro ao prato, da Gudossi, listado para PC,
  Android e iOS, lançado em jun/2026; fato: playgama.com/game/claybreak-range) e **"Claybreaker - VR
  Clay Shooting"** no Steam (Ammonite Design; fato: store.steampowered.com/app/657450). Gênero
  diferente do nosso. Na Google Play não apareceu app chamado "Claybreak".
- Marcas: nas bases que abriram, nenhuma marca "CLAYBREAK" nas classes 9 (software e jogos) e 41
  (entretenimento). Há "CLAYBREAKER" nos EUA na classe 1 (produto químico de perfuração), sem relação.
  USPTO, EUIPO/TMview e WIPO não carregaram sem navegador (resultado parcial).
- **O INPI (Brasil) não abriu** para a busca automática. Para conferir à mão (5 minutos, grátis):
  busca.inpi.gov.br/pePI → "Continuar" → Marca → Pesquisa básica → CLAYBREAK com pesquisa "Radical" →
  olhar as classes 9 e 41; repetir com "CLAY BREAK".
- A varredura anterior (D1) também citou o histórico *Clay Jam* (nome parecido, colisão baixa a média).
- Atenção (corrigido pela segunda opinião): "Claybreak Range" **também é jogo** listado para Android,
  portanto nas mesmas classes 9 e 41 que as nossas; só o gênero é outro. Lançado em jun/2026, o dono
  pode registrar a marca antes de nós (não verificado se já registrou).
- Inferência nossa: risco jurídico baixo a médio; a busca na loja vai misturar os resultados.

Opções:
A) Seguir com "Claybreak" como nome de trabalho; você faz à mão a busca do INPI (passo acima) e, se
   quiser, também a do TMview (tmdn.org/tmview) e a do USPTO (tmsearch.uspto.gov), grátis, com
   CLAYBREAK; a sessão da RT-9 leva dois nomes reserva já checados; nome final antes da RT-9.
   + Nada muda agora; o jogo continua sem o nome na arte.
   - Riscos: se o INPI tiver registro, troca-se depois (chance baixa; impacto baixo agora, porque ícone
     e ficha ainda não existem). Volta atrás? sim, fácil.
B) Trocar já o nome de trabalho (ideias não checadas: "Clay Siege", "Mudstorm", "Clayforce") e fazer
   nova checagem.
   + Evita a mistura de busca com "Claybreak Range".
   - Riscos: a nova checagem pode achar outro conflito (chance média); gasta uma sessão. Volta atrás? sim.
C) Fixar "Claybreak" como nome final agora.
   - Riscos: mistura de busca na loja (chance alta; impacto médio no tráfego orgânico) e INPI não
     checado. Volta atrás? com custo depois da publicação.

Recomendação: A — nada da fatia depende do nome, e a decisão final fica melhor perto da ficha da loja,
com o INPI conferido por você.
Segunda opinião (fable): concorda com A, desde que o risco seja dito como "mesma classe" (corrigido
acima) e a checagem manual inclua TMview e USPTO (incluído na opção A).
Se você não responder: segue A (o nome não aparece na arte do jogo).

Responda: A, B (com o nome que preferir), C, ou "explica melhor".
