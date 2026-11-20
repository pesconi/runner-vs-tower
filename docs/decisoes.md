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

## Fichas D17 e D18 (respondidas em 2026-11-20; texto original mantido)

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

**Resposta do usuário (2026-11-20): torre A, chefe A** — a recomendação. As provisórias viram a arte
final da fatia. Conferir no aparelho que a pedra da torre A ficou clara (a imagem de comparação
`torre-opcoes.png` ainda mostra a versão marrom).

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

**Resposta do usuário (2026-11-20): B — trocar o nome de trabalho para "Clay Siege".** Próximo passo:
checagem gratuita de marca e lojas para "Clay Siege" (mesmo roteiro da D1/D18, INPI à mão se não
abrir) e troca do nome do app instalado; nome final continua confirmado antes da RT-9.

## 2026-11-20 — D18 (continuação) — Checagem gratuita de "Clay Siege" — sessão RT-4

Busca de 2026-11-20 por "Clay Siege", "ClaySiege", "Siege of Clay" e "Clay Sieges":
- **Lojas:** Google Play, App Store (busca pública da Apple), Steam e itch.io (tag "clay") abriram e
  **nenhum app ou jogo tem esse nome**. Os mais próximos são jogos só com "Siege" (Siege Arena da Voodoo,
  City Siege, Hero Squad: Battle Siege) e jogos de massinha com outros nomes (Clay Guys, Clay Jam Classic,
  Clay Soldiers, No Man's Clay).
- **Marcas:** no espelho Trademarkia dos EUA não há CLAY SIEGE; "SIEGE" sozinho é palavra muito usada,
  com marca viva na classe 41 (Georgia Game Developers Association) e outra nas classes 9/36 — o "Clay"
  é o que torna o nome distinto (inferência nossa). **USPTO, EUIPO/TMview, WIPO e INPI não abriram** para
  busca automática.
- **Web:** nenhuma empresa ou marca com esses nomes.
- **Resultado:** sem conflito no que foi acessível; **não é nome liberado**. Antes da RT-9, conferir à
  mão (grátis, no navegador) INPI (busca.inpi.gov.br/pePI → Marca → Pesquisa básica → "CLAY SIEGE",
  pesquisa "Radical", classes 9 e 41), TMview (tmdn.org/tmview) e USPTO (tmsearch.uspto.gov).
- **Feito:** o nome exibido do app passou a "Clay Siege" e o menu mostra "CLAY SIEGE" com a chamada
  "STORM THE TOWER".

## 2026-11-20 — T-RT4-6 — Troca de nome só no que aparece — técnica (sessão RT-4)

Com o nome de trabalho "Clay Siege" (D18), muda só o nome exibido (`PlayerSettings.productName`, que vira
o rótulo do app no Android) e o título do menu. O application ID `com.jungledegree.runnervstower`, os
namespaces e assemblies `Claybreak`, a pasta `Assets/Claybreak`, a chave do save (`Claybreak.Save`) e o
repositório ficam como estão: trocar o ID quebraria a instalação por cima e o save do aparelho, e os
nomes internos não aparecem para o jogador. Renomear o ID, se for preciso, só uma vez e antes da
primeira publicação (RT-9).

## 2026-11-20 — D17 (continuação) — Arte final aplicada — sessão RT-4

Torre A (bastião de pedra clara, braseiro no topo) e chefe A (cidadela) passam de provisórios a arte
final da fatia (`TowerArt.ChosenTower`/`ChosenBoss`). As imagens de comparação de `docs/art/rt4/` foram
regeradas: a versão anterior saiu sem a luz do sol (só luz ambiente) e mostrava a torre A marrom escura;
o jogo já usava a pedra clara.

## 2026-11-20 — T-RT5-1 — Meta como catálogo de dados — técnica (sessão RT-5)

Tudo o que a meta oferece fica num asset só, `ClaybreakCatalog` (escrito por `WorldContent.WriteCatalog`):
mundos em ordem com a trava de estrelas, 4 trilhas de upgrade (exército, dano, portão, lançamento), 3
elites (Bruto, Corredor, Guardião) com loadout de 2 vagas, 6 skins. O mundo (`ClaybreakWorld`) ficou só
com id, fases e regra da volta; o upgrade e o Bruto saíram dele. As regras ficam em `Progression`
(pura, testada). O construtor monta uma linha de loja por item e uma linha por mundo a partir do
catálogo: mundo, item ou preço novo é dado + reconstruir, nunca código. O kit `studiokit` não tem save
nem economia; tudo fica no jogo (se o Wild Pack precisar do mesmo, vira pauta do kit).

## 2026-11-20 — T-RT5-2 — Save versão 2 por ids estáveis, migração do v1 — técnica (sessão RT-5)

- O save guarda só ids e contagens (`world1`, `army`, `brute`, ...), nunca índices: conteúdo novo não
  quebra save; id removido some na normalização. Os ids do save v1 ficam no catálogo para sempre (o
  validador reprova se saírem).
- Save v1 da fatia migra para v2 mantendo moedas, estrelas, volta, degrau do exército e o Bruto (que
  continua equipado, como era na fatia). O JSON original fica uma vez em `Claybreak.Save.backup.v1`.
- Leitura tolerante e limitada: JSON acima de 64 KB recusado, listas cortadas em 256, números limitados,
  save danificado ou de versão futura começa do zero sem travar (entrada externa, `decisao.md` seção 5).
- **Validador de save** (`SaveValidator`) roda no validador do jogo sobre saves de exemplo de cada versão
  (`Assets/Claybreak/Editor/SaveFixtures`) e nos testes; o save normalizado tem de passar e sobreviver a
  gravar e ler de novo.

## 2026-11-20 — T-RT5-3 — Elites, upgrades e skins no jogo — técnica (sessão RT-5)

- Elites por vaga do loadout: cada tipo tem tamanho, cor, dano, armadura e velocidade próprios; os
  caixotes da corrida alternam entre os tipos equipados; sai um elite a cada 3 lançamentos (dado).
- Dano: multiplicador com sobra acumulada (um +15% é exato num fluxo de golpes inteiros). Portão: soma
  em todo portão "+" da corrida (o rótulo mostra o valor final). Lançamento: multiplica unidades/segundo.
- Skin: cor da massinha do líder, da multidão e de toda a reserva, aplicada no menu (nada criado em
  jogo). A cor do respingo continua a do tema: o kit não permite trocá-la em tempo de jogo (pauta do kit).
- Guardião só na loja (a segunda opinião mostrou que, grátis na fase 6, o preço de 900 virava armadilha).
- O nível do gate roda o **loadout máximo** (todos os upgrades no topo, 2 elites, skin mais cara) e mantém
  o dano fixo da fatia na torre, para a torre sem fim durar a medição inteira.

## 2026-11-20 — T-RT5-4 — Tabela de economia gerada — técnica (sessão RT-5)

`EconomyReport` joga um jogador simulado com as regras reais e escreve `docs/rt5-economia.md` (passo
`economy` do `Tools/rt4-check.sh`). É a base de tempo da ficha D19 e a ferramenta para recalibrar com os
dados do soft launch: trocar as premissas pelas medidas e rodar de novo.

## 2026-11-20 — T-RT6-1 — Fases como dado gerado por perfil — técnica (sessão RT-6)

Toda fase é um `LevelSpec` (dado puro) escrito no asset da fase pelo conteúdo. As fases novas saem de um
`WorldRecipe` por mundo (o "ChallengeDifficultyProfile"-like do GDD 6) pelo `LevelGenerator`, determinístico
(mesma semente, mesmas fases em qualquer máquina). A vida da torre é uma fração do potencial da fase vezes o
dano esperado do mundo, então um layout mais difícil vem com torre mais fraca e vice-versa; fases feitas à
mão (as 7 da fatia) entram por índice e as geradas sobem entre elas. Gerador e perfil ficam no jogo, não no
kit: o kit não tem conceito de torre nem de fase de assalto (se o Wild Pack quiser fases por perfil, vira
pauta do kit).

## 2026-11-20 — T-RT6-2 — Visual por mundo pré-alocado na cena única — técnica (sessão RT-6)

Cada mundo tem um `WorldLook` (estilo de cenário e paleta) no próprio asset do mundo. O construtor monta um
conjunto de cenário por mundo e um material de pista por mundo; `WorldScenery` mostra um conjunto e aplica
céu, luz ambiente e sol ao preparar a fase. Continua uma cena só, nada criado em jogo (2.2.1); o custo é
memória de malhas estáticas inativas (pequena: primitivas). Torres: um corpo por `TowerKind`, como na RT-4;
`TowerArt.StyleFor` diz o estilo de cada tipo (uma linha por tipo; trocar a arte aprovada é trocar a linha).

## 2026-11-20 — T-RT6-3 — Escudo e regras de chefe como dado — técnica (sessão RT-6)

Escudo (4ª defesa do GDD 4.3): gerador numa faixa, cúpula na frente da torre, fração de bloqueio por fase; o
dano bloqueado usa a mesma sobra acumulada do upgrade de dano (metade bloqueada é exatamente metade). Chefe
= fase com `BossRule` (antes, o tipo de torre "Boss" e um booleano); quatro regras escritas no jogo, uma por
mundo. O nível de gate de cada mundo roda a regra e o escudo do mundo, com o escudo de pé o tempo todo (a
cúpula transparente é a parte mais cara para a GPU).

## 2026-11-20 — T-RT6-4 — Voltas ajustadas ao poder do jogador; save v3 — técnica (sessão RT-6)

Correção pedida na ficha D19: nas voltas, a vida das torres também cresce com (poder ÷ dano esperado do
mundo)^0,7, nunca abaixo de 1; a primeira passada nunca muda. O expoente fica no asset do mundo [P]. Save
versão 3 por causa do mundo 1 com 10 fases (estrelas do chefe mudam de lugar); v1 e v2 migram, e os saves de
exemplo e os testes cobrem as três versões.

## 2026-11-20 — T-RT6-5 — Pagamento dos mundos e total da loja (D19 A) — técnica (sessão RT-6)

Simulação (`docs/rt6-economia.md`, mesmas premissas da RT-5): cada mundo paga mais que o anterior (prêmios
40–200 no mundo 1, 110–350 no 2, 170–500 no 3, 240–700 no 4). Sem mudança, a primeira passada dos 4 mundos
(~13,2 mil moedas) pagaria quase a loja inteira da RT-5 (13.380), e a loja acabaria junto com o conteúdo
(risco apontado na D19). Os preços aprovados ficam iguais; entram 3 skins nas cores dos mundos novos
(MIRE, do pântano, 1.500; CINDER, da fundição, 2.000; FROST, do gelo, 2.500): loja de 19.380, a primeira passada paga ~68% dela e
o resto sai em ~10 min de voltas do mundo 4 (loja inteira em ~53 min simulados, 63 fases). O dano esperado
de cada mundo (1,0 / 1,3 / 1,5 / 1,75) acompanha os upgrades que a simulação já comprou ao chegar nele.
Recalibrar no soft launch com dados reais.

## Ficha D20 (RT-6, aguardando resposta)

### DECISÃO D20 — Arte dos mundos 2, 3 e 4 (paleta, torre e chefe)

O que você está aprovando: a aparência dos três mundos novos do lançamento, com a paleta, a torre e o chefe
marcados como recomendados abaixo (feitos por código, com formas simples, como a arte da fatia), como
**provisória até o soft launch** (lançamento de teste); trocar qualquer item por outra opção desta ficha é
trocar uma linha e reconstruir.

Por que agora: o plano pede a sua aprovação da arte dos mundos na RT-6 (conteúdo). Não trava nada: o jogo
já roda com as recomendações. Trocar é barato até o soft launch; depois, mudar a cara de um mundo que os
jogadores já viram pesa mais.

Contexto que você precisa saber:
- Os 4 mundos de 10 fases (D12 = A) estão prontos e jogáveis; o mundo 1 é o da fatia (arte aprovada na D17:
  bastião de pedra e cidadela, chão marrom e braseiros laranja). Cada mundo novo tem cenário próprio, uma
  torre nova, um chefe novo com regra própria, e reaproveita a torre do mundo anterior em 2 das 9 fases
  comuns (fato: conteúdo da RT-6).
- Tudo é feito por código, com caixas, cilindros e esferas de cor lisa, sem ferramenta ou asset pago
  (fato). O GDD (seção 9) pede paleta noturna e quente, adulta, torres com atitude (fortaleza, forja,
  fábrica, ruína) e massinha arredondada; as torres daqui são de formas duras, como as da fatia; "sem
  rostos" é regra das decisões D17/RT-4 (fato).
- As folhas de opções (`docs/art/rt6/`, A à esquerda) usam uma câmera parecida com a do jogo, mas um pouco
  mais afastada (a torre aparece menor que no jogo); as capturas do jogo de verdade, com a torre nova de
  cada mundo e o chefe, estão em `docs/art/telas/jogo-mundoN-fase.png` e `-chefe.png` (fato).
- As recomendações passaram no teste de desempenho do OPPO (gate 3×) com o chefe de cada mundo: quadro de
  16 ms (60 por segundo, 3× de folga), memória ~200 MB. As opções B não foram medidas no aparelho; são as
  mesmas peças, e uma troca seria medida de novo (fato/inferência nossa).
- Limites conhecidos (segunda opinião, conferido): a neblina de cada mundo tinge os portões (no mundo 4 o
  vermelho fica rosado e o azul fica perto da pista azul e dos cristais); quase todas as torres são pilhas de
  andares que afinam para cima; as capturas não mostram o exército nem números nos portões. A legibilidade
  com o exército correndo só se confere jogando.
- Regras de chefe (já no jogo, não fazem parte desta decisão): mundo 2 — o escudo volta a cada andar
  derrubado; mundo 3 — canhão e varredor aceleram a cada andar; mundo 4 — o melhor portão multiplicador vira
  divisor a cada andar.

MUNDO 2 — pântano (introduz o escudo: um gerador numa faixa que, de pé, faz a torre ignorar parte do dano)
- Paleta: **A (recomendada)** noite verde-azulada com lanternas verdes · B lodo âmbar com lampiões laranja.
- Torre: **A** forte de madeira escura sobre palafitas, paliçada e lanternas · B ruína de barro inclinada com
  musgo (era a opção C da D17; lê melhor no escuro, mas parece mais "brinquedo").
- Chefe: **A** campanário de pedra meio afundado, arcos acesos e um sino · B totem de anéis de barro com
  chifres (opção C da D17).
- Atenção: o mundo 2 A é o mais escuro; a torre de madeira escura é o item que pior se destaca do fundo.

MUNDO 3 — fundição
- Paleta: A fuligem e brasa (preto avermelhado, metal laranja) · **B (recomendada)** fumaça amarelada com
  lâmpadas brancas. Mudou para B por causa da segunda opinião: a A repetia o marrom com laranja do mundo 1.
- Torre: **A** moinho de tijolo com telhado em serra, duas chaminés, andares de janelas acesas · B gasômetro.
- Chefe: **A** martelo-pilão (duas colunas e uma travessa; a única silhueta que não é pilha) · B colosso da
  forja (quase a forja do mundo 1).

MUNDO 4 — picos gelados
- Paleta: **A (recomendada)** noite azul de neve com luz de gelo · B picos de cinza (preto-violeta, brilho
  magenta). Quase empate: a B separa melhor os portões e é mais "quente" como o GDD pede; a A é a que mais
  difere dos outros três mundos.
- Torre: **A** agulha de basalto e gelo, andares em losango, cristal no topo · B bastião de osso.
- Chefe: **A** agulha coroada (coroa de lascas e dois obeliscos) · B bastião de osso maior com chifres. Os
  dois chefes são a torre do mundo com extras: parecidos demais com ela (fraqueza conhecida).

Opções:
A) Aprovar as recomendações como provisórias até o soft launch (como o jogo está agora).
   + Nenhum trabalho a mais; cada mundo novo tem cor dominante e cenário próprios.
   - Riscos: formas simples podem parecer pobres perto de concorrentes com arte feita à mão (chance média;
     impacto médio na instalação) — mitigação: a loja (RT-8) e o soft launch medem, e a arte troca sem mexer
     no jogo. Mundo 2 escuro e portões tingidos no mundo 4 (chance média; impacto baixo) — conferir jogando.
     Chefe do mundo 4 parecido com a torre (chance alta de alguém notar; impacto baixo).
   Custo: nenhum. Dá para voltar atrás? sim, fácil.
B) Aprovar com trocas: diga quais itens vão para a outra opção (ex.: "mundo 4 paleta B").
   + Você escolhe item a item. Custo: minutos + um gate no aparelho para o mundo trocado. Volta atrás? sim.
   - Riscos: combinação não vista junta (chance baixa; impacto baixo) — refaço as capturas.
C) A com ajustes (sugestão da segunda opinião): clarear o mundo 2, deixar os portões sem a tinta da neblina
   em todos os mundos, refazer o chefe do mundo 4 com silhueta própria.
   + Corrige as fraquezas conhecidas antes de alguém de fora ver.
   - Riscos: uma sessão de trabalho e novo gate (chance alta de custar a sessão; impacto baixo, sem prazo).
D) Adiar até jogar os 4 mundos no celular.
   + Decide vendo como o jogador vê. Sem risco para o código (o jogo segue com as recomendações).

Recomendação: A, e jogar no celular antes de responder — trocar depois é uma linha, e o que realmente
importa (se a arte segura a instalação) só a loja e o soft launch medem. Se, jogando, o mundo 2 ou os
portões do mundo 4 incomodarem, responda C.
Segunda opinião (fable): concorda com A só como provisória e com ajustes; apontou que a paleta A do mundo 3
repetia o mundo 1 (trocada para B), que as capturas do jogo mostravam a torre reaproveitada em vez da nova
(refeitas), que as opções B não foram medidas no aparelho, a tinta dos portões no mundo 4, o mundo 2 escuro
e o chefe do mundo 4 parecido demais com a torre; sugeriu a opção C (incluída).
Se você não responder: o jogo segue com as recomendações como provisórias; nada mais espera por isso.

Responda: A, B + itens, C, D, ou "explica melhor".

## Fichas D19 e D12 (RT-5, aguardando resposta)

### DECISÃO D19 — Preços e curvas da loja (upgrades, elites, skins)

O que você está aprovando: a forma da loja que está no jogo agora (os efeitos e preços abaixo, subindo
~1,5× por degrau) como ponto de partida da v1, sabendo que o total da loja e o quanto cada mundo novo
paga serão acertados juntos na RT-6 (conteúdo) e recalibrados com dados reais no soft launch (lançamento
de teste).

Por que agora: a RT-5 pede a sua aprovação dos preços e curvas. Nada está travado: o jogo já roda com
estes valores marcados como provisórios [P]; mudar é editar números numa tabela e reconstruir, sem
quebrar saves (o save guarda só o degrau comprado). Mas a RT-6 vai calibrar a dificuldade dos mundos
supondo um nível de upgrade, então mexer muito nos preços depois desloca a dificuldade.

Contexto que você precisa saber:
- Moeda única, só ganha jogando (fato). Sem compra com dinheiro real na v1 (D6). A D6 permite anúncio
  recompensado na v1 (RT-7), e o GDD prevê "dobrar a moeda da fase" por anúncio e a recompensa diária
  (D5): os dois vão acelerar esta curva quando entrarem.
- Vitória paga: prêmio da fase (40 a 200) + 1 moeda por sobrevivente + ¼ do prêmio por estrela. Derrota
  paga ⅛ do prêmio. Depois do chefe o mundo recomeça 25% mais forte e paga 25% a mais por volta (fato:
  regras do jogo).
- Loja no jogo agora, 4 upgrades de 6 degraus (efeito total — preço do degrau):
  - EXÉRCITO (unidades a mais no início): +5 — 60 · +10 — 120 · +16 — 200 · +24 — 320 · +34 — 480 · +46 — 700
    (é o upgrade da fatia, sem mudança: quem já comprou mantém).
  - DANO (na torre e na muralha): +15% — 80 · +30% — 150 · +45% — 250 · +60% — 400 · +80% — 600 · +100% — 900
  - PORTÃO (somado a todo portão "+" da corrida): +1 — 100 · +2 — 180 · +3 — 300 · +5 — 450 · +7 — 650 · +10 — 950
  - LANÇAMENTO (unidades por segundo): +10% — 70 · +20% — 130 · +30% — 220 · +45% — 350 · +60% — 520 · +80% — 750
- Elites (unidades especiais; 2 equipadas; vêm em caixotes na corrida e saem a cada 3 lançamentos):
  BRUTO (bate 4×, aguenta 1 tiro) grátis ao vencer a fase 3; CORREDOR (bate 2×, 60% mais rápido) 600;
  GUARDIÃO (bate 2×, aguenta 3 tiros, mais lento) 900.
- Skins (cor da massinha do exército, feitas por código): BARRO grátis, BRASA 150, MUSGO 300, ARDÓSIA 500,
  OSSO 800, TINTA 1200. Loja inteira: 13.380 moedas.
- Tempo estimado (inferência nossa: jogador simulado com as regras reais, 80% de vitórias, ~55 s por
  vitória, compra sempre o mais barato; sem dado real, D4; tabela em `docs/rt5-economia.md`):
  - ~240 moedas por minuto; uma compra a cada 1–3 min no começo;
  - 1º degrau de cada upgrade: 1,5 a 4,3 min de jogo; 3º: 8 a 14 min; 5º: 23 a 30 min; último: 34 a 43 min;
  - Corredor ~45 min, Guardião ~48 min, loja inteira ~57 min (67 fases, 9 voltas do mundo 1);
  - a primeira passada do mundo 1 paga ~1.075 moedas (8% da loja).
  - A simulação não sabe se o tempo real será maior ou menor: ela ignora que os upgrades aumentam
    sobreviventes e estrelas (mais moedas) e que as voltas mais fortes fazem perder mais (menos moedas).
- No topo, dano +100% com lançamento +80% multiplicam o ataque por ~3,6, contra +25% de vida por volta:
  o mundo 1 fica fácil nas voltas altas (inferência nossa).

Opções:
A) Aprovar a forma desta tabela como ponto de partida; o total da loja e o pagamento dos mundos novos são
   acertados juntos na RT-6, e tudo recalibrado no soft launch.
   + Progresso visível já nos primeiros minutos, bom para o teste de vocês dois.
   + Não amarra o total antes de existirem os outros mundos.
   - Riscos: a loja pode acabar cedo e o jogador ficar sem meta (chance média; impacto médio na volta no
     7º dia) — mitigação: a RT-6 ajusta os pagamentos e traz itens novos; o soft launch mede.
     O topo dos upgrades deixa o mundo 1 fácil (chance média; impacto baixo) — ajustar na RT-6.
   Custo: nenhum. Dá para voltar atrás? sim, fácil (números numa tabela).
B) Preços ×2 (mesmos efeitos) — loja inteira em ~88 min (simulado).
   + Meta mais longa.
   - Riscos: começo mais lento (1º upgrade em ~3 min) e mais repetição do mundo 1 (chance média; impacto
     médio na diversão do teste). Volta atrás? sim, fácil.
C) Preços ×3 — loja inteira em ~112 min (simulado).
   + Meta longa, mais perto de jogos do gênero com centenas de níveis.
   - Riscos: com poucas dezenas de fases vira repetição (chance alta; impacto alto na volta no dia
     seguinte). Volta atrás? sim.
D) Adiar — a tabela fica provisória até o soft launch. Custo: a RT-6 calibra os mundos sobre uma curva
   sem aprovação.

Recomendação: A — sem dado de mercado, o melhor ponto de partida é uma curva com compra visível nos
primeiros minutos e barata de mudar; o total só faz sentido fixar quando os outros mundos existirem
(RT-6), e o soft launch existe para calibrar com dados.
Segunda opinião (fable): concorda com A como provisória. Apontou erros da primeira versão desta ficha,
já corrigidos: a D6 permite anúncio na v1; os tempos de B e C não escalam ×2 e ×3 (agora simulados); a
loja não "termina junto com o conteúdo" (a primeira passada paga só 8%); o Guardião a 900 era armadilha
porque saía grátis na fase 6 (agora é só da loja); e mexer em preço depois desloca a dificuldade.
Se você não responder: o jogo segue com esta tabela como provisória; nada mais espera por isso.

Responda: A, B, C ou D, ou "A com <ajuste>", ou "explica melhor".

**Resposta do usuário (2026-11-20): A** — a tabela atual é o ponto de partida da v1; total da loja e
pagamento dos mundos novos acertados na RT-6; recalibração no soft launch.

### DECISÃO D12 — Quanto conteúdo no lançamento (mundos e fases)

O que você está aprovando: a quantidade de mundos e de fases da versão de lançamento, que define o
tamanho da entrega RT-6 (conteúdo).

Por que agora: o plano pede esta decisão antes do fim da RT-5, porque ela define a RT-6. Não trava o
código: a tela de mundos já serve a qualquer número (cada mundo é um bloco de dados; a lista se monta
sozinha; abrir um mundo pede vencer o chefe do anterior e, se quisermos, um mínimo de estrelas). Trava o
tamanho e o prazo da RT-6.

Contexto que você precisa saber:
- Hoje existe 1 mundo de 7 fases (6 comuns + chefe); a primeira passada leva ~6–8 min (estimativa da
  fatia e da simulação; não medido); depois do chefe o mundo recomeça 25% mais forte, sem fim. Você jogou
  e respondeu "voltaria amanhã" (fato: aceite da RT-4).
- Uma fase nova custa pouco (um bloco de dados de ~15 linhas). O caro é cada **mundo**: paleta e
  cenário, 1–2 torres novas, um chefe com regra própria, talvez a 4ª defesa (o "escudo" do GDD), arte para
  você aprovar e o teste de desempenho (gate 3×) no OPPO de cada mundo (fato: aceite da RT-6). Cada mundo
  aceita de 5 a 10 fases (limite atual da tela, ajustável).
- O plano estima a RT-6 em 2,5 semanas (3 com folga) [I], sem dizer quantos mundos; ela inclui também
  "geração de fases por dados" e "ajuste de dificuldade por perfil".
- A pesquisa cita ~30–50 fases por dados como ordem de grandeza (inferência dela). Jogos híbridos que
  buscam D30 ≥ 5% (5% dos jogadores voltando no 30º dia) têm ~500 níveis e eventos (fato, Azur,
  2026-04-28) — fora do alcance do lançamento.
- Metas do soft launch: D1 ≥ 35% e D7 ≥ 12% (% que volta 1 e 7 dias depois de instalar). Qualquer opção
  abaixo (25 a 50 min de conteúdo) é jogada no 1º ou 2º dia: o 7º dia vai depender mais da volta mais
  forte, da loja e da recompensa diária (D5, RT-7) do que de 30 ou 48 fases (inferência nossa).
- A loja (D19) custa ~57 min de jogo no ritmo do mundo 1; os mundos novos vão pagar mais (RT-6).
- Estúdio de 2 pessoas, sem prazo.

Opções:
A) 4 mundos de 10 fases (40 fases): o mundo 1 ganha 3 fases e entram 3 mundos novos, cada um com paleta,
   1 torre nova e chefe novo; o escudo entra no mundo 2.
   + Dentro da faixa 30–50 com o menor número de mundos (o caro é o mundo, não a fase).
   + RT-6 com 3 mundos de arte e 3 gates no aparelho.
   - Riscos: 3 mundos podem não caber nas 2,5–3 semanas do plano (chance média; impacto médio no prazo,
     que não existe) — mitigação: entregar mundo a mundo. Pouca variedade visual com 10 fases por mundo
     (chance média; impacto baixo) — 2 torres por mundo.
   Custo: tempo da RT-6. Dá para voltar atrás? sim (mais mundos depois é dado + arte).
B) 6 mundos de 8 fases (48 fases).
   + Mais variedade visual.
   - Riscos: RT-6 com 5 mundos de arte e 5 gates, ~2× a opção A (chance alta; impacto médio: o soft
     launch atrasa). Volta atrás? sim.
C) 3 mundos de 10 fases (30 fases) no lançamento e o 4º mundo durante o soft launch.
   + Chega antes ao soft launch, que é quem mede de verdade; o 4º mundo vira a primeira novidade.
   - Riscos: no limite de baixo da faixa (chance média; impacto baixo, pelo motivo do contexto acima).
     Volta atrás? sim.
D) Adiar até jogarem a loja nova — custo: a RT-6 começa só pelo mundo 2 (que entra em qualquer opção).

Recomendação: A — 40 fases no meio da faixa com o menor custo de arte e de gate por fase; se a RT-6
apertar, cai naturalmente para C (o 4º mundo entra no soft launch) sem retrabalho.
Segunda opinião (fable): concorda com A, preferindo 10 fases por mundo (incorporado: a primeira versão
tinha ~9). Apontou que a primeira versão comparava mal o conteúdo com a loja e atribuía ao número de
fases um efeito no 7º dia sem base (corrigido no contexto), e sugeriu a opção C (3 mundos + 1 no soft
launch), incluída.
Se você não responder: a RT-6 começa pelo mundo 2 (que entra em qualquer opção) e para antes do 3º.

Responda: A, B, C ou D, ou "A com <ajuste>", ou "explica melhor".

**Resposta do usuário (2026-11-20): A** — 4 mundos de 10 fases (40 fases): o mundo 1 ganha 3 fases e
entram 3 mundos novos (paleta, 1 torre nova e chefe novo cada; escudo no mundo 2). Se a RT-6 apertar,
cai para C (4º mundo no soft launch) sem retrabalho.
