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

## 2026-11-20 — Gate 3× uma vez por entrega — usuário

Resposta do usuário ("Sim, uma vez só", via maestro): a medição de desempenho no OPPO (gate 3×) roda **uma
vez por entrega**, no fim, no mundo/nível mais pesado, e não mais por mundo; repete só se sair do limite.
Vale para as próximas entregas e para o aceite da RT-6. Na RT-6 a decisão chegou depois das medições por
mundo; o caso mais pesado foi o mundo 4 (CPU p95 até 9,1 ms, GPU 4,4 ms, quadro p95 16 ms, 197 MB), que
vale como a rodada final. O app de medição é fechado no celular ao terminar.

## 2026-11-20 — T-RT7-1 — Consentimento único e IDs de anúncio como dado, no kit — técnica (sessão RT-7)

O serviço de premiado do kit fazia sozinho o fluxo UMP e trazia a unidade de produção do Wild Pack fixa no
código: num segundo jogo, um build de release mostraria a unidade de outro app. Agora o kit tem
`MobileAdsGate` (UMP → `MobileAds.Initialize` uma vez por app, só com `CanRequestAds`), `AdUnitSettings`
(asset com as unidades de produção do jogo; vazio = unidades de teste do Google) e `InterstitialAdService`
+ `InterstitialPacing` (limites como dado, testados em EditMode). O premiado passou a usar o portão comum e
lê o `AdUnitSettings` quando atribuído; sem ele, mantém a unidade antiga (o Wild Pack não muda de cena nem de
comportamento). Pendência para o Wild Pack/kit: dar ao Wild Pack o seu `AdUnitSettings` e tirar a constante
do kit.

## 2026-11-20 — T-RT7-2 — Medição atrás de uma porta, sem SDK — técnica (sessão RT-7)

D4 diz "nada é instalado antes" da escolha da ferramenta. O kit ganhou `PlayerMetrics` (lista fechada de 4
eventos: `game_session` com dias desde a instalação, `level_start`, `level_end`, `rewarded_ad` com
oferecido/aceito/ganho; valores curtos, sem nenhum ID) e `IMetricsSink`. O release instala `NullMetricsSink`
e não permite coleta; builds de desenvolvimento escrevem só no log do aparelho. Escolhida a ferramenta (D21),
entra um adaptador e a coleta passa a seguir o consentimento.

## 2026-11-20 — T-RT7-3 — Premiado, continuar, intersticial e presente diário no jogo — técnica (sessão RT-7)

Usos do GDD 7.2 implementados: **dobrar as moedas** no resultado (uma vez por resultado) e **continuar uma
vez por fase** quando o exército cai no assalto (volta metade do exército que chegou à linha; oferta com "NO
THANKS" claro; só com pelo menos 10 unidades na linha). Os dois só aparecem com anúncio carregado e o ganho
escrito no botão. "Girar a recompensa do chefe" e "reforço no início" ficaram de fora (o GDD os marca como
propostas; dois usos bastam para medir o aceite). Intersticial: só depois de vitória, ao tocar NEXT, nunca
logo depois de um premiado escolhido; limites [P] 3 fases e 3 min antes do primeiro e entre dois; **desligado**
por `MonetizationContent.InterstitialOn` (D6). Presente diário (D5): um por dia do calendário local, ciclo
50-75-100-125-150-200-300 moedas [P] que só anda quando se pega (faltar não zera). Save continua na versão 3
(campos novos aditivos: `firstDay`, `dailyLastDay`, `dailyClaims`; ausentes leem 0 = "nunca").

## 2026-11-20 — T-RT7-4 — Estatística de hardware do Unity desligada — técnica (sessão RT-7)

`ProjectConfiguration` grava `submitAnalytics = 0` (Player setting de estatística de hardware enviada ao
Unity), para que o único SDK de rede do build seja o de anúncios que a política declara. Serviços Unity já
estavam desligados (`UnityConnectSettings`).

## Ficha D20 (RT-6, respondida em 2026-11-20: A)

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

**Resposta do usuário (2026-11-20): A** — "Segue como está, opção A". As recomendações (mundo 2: paleta A,
forte de palafitas, campanário; mundo 3: paleta B, moinho, martelo-pilão; mundo 4: paleta A, agulha,
agulha coroada) são a arte dos mundos 2–4 até o soft launch. Os ajustes da opção C ficam anotados para
rever se, jogando, o mundo 2 parecer escuro ou os portões do mundo 4 lerem mal.

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

## Ficha D21 (RT-7, respondida em 2026-11-20: A)

### DECISÃO D21 — Qual ferramenta gratuita mede os jogadores (analytics)

O que você está aprovando: o Clay Siege passa a usar o **Firebase Analytics** (do Google) para medir só o que
a D4 aprovou (quem volta 1 e 7 dias depois de instalar, início e fim de cada fase, e quantos aceitam o anúncio
premiado), na configuração mínima: **sem o ID de publicidade, com a coleta desligada até o jogo liberar**, e
com a política de privacidade e a seção "Segurança dos dados" da Play atualizadas antes do lançamento. Você
cria o projeto gratuito no Firebase (conta Google) quando a sessão pedir; a sessão instala o SDK (o kit de
programação da ferramenta), liga o adaptador e mede o peso no celular.

Por que agora: a D4 mandou escolher a ferramenta numa ficha própria e "nada é instalado antes disso". Não
trava nada hoje: a RT-7 já deixou os eventos prontos atrás de uma porta que não manda nada para fora do
aparelho. Trava a medição do soft launch (lançamento de teste, RT-10): sem ferramenta, as metas D1 ≥ 35% e
D7 ≥ 12% (D1/D7 = % de jogadores que voltam 1 / 7 dias depois de instalar) ficam sem número. Precisa sair
antes da RT-9 (preparação da loja), porque a política e o formulário da Play mudam com ela.

Contexto que você precisa saber:
- O jogo já tem os 4 eventos prontos e testados (fato, RT-7): sessão de jogo com "dias desde a instalação",
  início de fase, fim de fase (venceu/perdeu e segundos) e anúncio premiado (oferecido, aceito, ganho). Nenhum
  leva nome, e-mail ou ID do jogador.
- Hoje o único SDK que fala com a rede é o de anúncios do Google (AdMob), com o UMP (a janela de
  consentimento do Google). A política e o rascunho de Data safety da RT-7 dizem "sem analytics" (fato).
- **A janela UMP só aparece onde a lei europeia/britânica exige** (fato: documentação do Google). No Brasil e
  na maioria dos países ninguém vê pergunta nenhuma. Então "medir com consentimento" precisa de uma regra para
  fora da Europa: a recomendação é medir lá com aviso na política e a base legal de "interesse legítimo" da
  LGPD, dados mínimos e sem ID de publicidade, e na Europa/Reino Unido só com o "sim" da janela (inferência
  nossa; não é parecer jurídico).
- A Data safety é preenchida por **tipo de dado**, não por empresa: qualquer ferramenta acrescenta "atividade
  no app" (finalidade: análise) e um ID de instalação (fato: documentação de cada uma).
- Com poucas dezenas de instalações o D1/D7 é ruído; os relatórios do Firebase também escondem números muito
  pequenos e chegam com 1–2 dias de atraso (fato).

O que cada opção coleta e pesa:
- **Firebase Analytics (Google):** gratuito. Cria um ID da instalação do app, deduz localização aproximada pelo
  IP e registra sozinho primeiro uso e sessões; coleta o ID de publicidade **se não for desligado** (fato:
  página de Data safety do Firebase). Retenção por coorte (D1/D7) pronta. Conversa com o UMP pelo "modo de
  consentimento" do Google (fato; exige UMP recente e ajuste no painel do AdMob; não conferido no plugin Unity).
  O SDK do Firebase para Unity é o mais pesado das opções (inferência nossa: alguns MB; a medir no gate).
- **GameAnalytics:** gratuito, feito para jogos, painéis de funil e retenção prontos. Cria um ID aleatório
  próprio; o ID de publicidade pode ser desligado (fato: documentação dela). Uma empresa a mais na política.
  Consentimento ligado à mão no código. SDK leve (inferência nossa).
- **Unity Analytics:** gratuito até 50 mil jogadores ativos por mês, somando todos os jogos da conta (fato:
  tabela de preços do Unity). Coleta só os eventos que o jogo manda mais um ID de instalação; o consentimento é
  todo nosso, por uma API própria (fato). Uma empresa a mais na política.
- **Nenhum SDK, só Play Console:** zero peso, nada novo na política. Mas o relatório de retenção D1/D7 do Play
  Console é para jogos com **Play Games Services** (login de jogos do Google), que não temos; sem ele só existe
  "quem ainda tem o app instalado", que não é "voltou a jogar" (fato, conferido pela segunda opinião). Não dá o
  funil de fases nem o aceite do premiado.

Opções:
A) Firebase Analytics, configuração mínima (sem ID de publicidade, sem sinais do Google, coleta liberada pelo
   jogo).
   + D1/D7 e funil prontos sem servidor; mesmo fornecedor e mesma janela de consentimento dos anúncios.
   - Riscos: APK maior (chance alta; impacto baixo, medido no gate); o gerenciador de dependências do Firebase
     brigar com o do AdMob (chance média; impacto baixo, resolvido na sessão); configurar errado e coletar antes
     da hora (chance baixa; impacto médio) — mitigação: coleta desligada no manifesto e teste; Google concentra
     anúncios e dados (impacto baixo, jogo sem conta).
   Custo: grátis; ~1 sessão + você cria o projeto Firebase. Dá para voltar atrás? sim (tirar o SDK; a política
   volta ao texto atual).
B) GameAnalytics.
   + Painéis de jogo prontos; SDK leve.
   - Riscos: mais uma empresa na política (certo; impacto baixo); consentimento fora do UMP, ligado à mão (chance
     média de erro; impacto médio) — o kit só manda evento quando o jogo libera.
   Custo: grátis; ~1 sessão + conta GameAnalytics. Volta atrás? sim.
C) Unity Analytics.
   + Integrado ao Unity; só coleta o que o jogo manda.
   - Riscos: limite de 50 mil ativos por mês somando os jogos (chance baixa no soft launch; impacto médio se o
     Wild Pack crescer); consentimento por API própria (chance média de erro; impacto médio).
   Custo: grátis até o limite; ~1 sessão + projeto na nuvem Unity. Volta atrás? sim.
D) Nenhum SDK: só Play Console e AdMob.
   + Nada novo na política, na Data safety nem no APK.
   - Riscos: sem D1/D7 real, sem funil e sem aceite do premiado (certo; impacto alto: as metas do soft launch e o
     ajuste de dificuldade da RT-10 ficam sem número).
   Custo: zero. Volta atrás? sim.
E) Adiar até a RT-9.
   + Nada muda agora (o jogo já não manda nada).
   - Custo de adiar: política e Data safety da RT-9 podem ter de ser refeitas; o soft launch pode começar sem
     medição.

Recomendação: A — é a única que dá D1/D7 e funil sem servidor usando a mesma janela de consentimento dos
anúncios; na configuração mínima acrescenta à Data safety só "atividade no app" e o ID de instalação. O peso no
APK é medido no gate antes de ligar.
Segunda opinião (fable): concorda com A, com três ajustes (incorporados): ligar o Firebase sem ID de
publicidade e com a coleta desligada até o jogo liberar; renomear o evento de sessão (o nome `session_start` é
reservado do Firebase e seria descartado — já trocado para `game_session`); e definir o que vale no Brasil,
onde a janela UMP não aparece. Corrigiu a opção D: o D1/D7 do Play Console exige Play Games Services.
Se você não responder: nada é instalado; o jogo segue sem mandar nenhum evento para fora (como hoje) e a RT-8
continua normalmente.

Responda: A, B, C, D ou E, ou "A com <ajuste>", ou "explica melhor".

**Resposta do usuário (2026-11-20): A — Firebase Analytics** na configuração mínima (sem ID de
publicidade, sem sinais do Google, coleta desligada até o jogo liberar; fora da Europa/Reino Unido com aviso
na política e dados mínimos). Próximo passo: uma sessão instala o SDK, liga o adaptador, atualiza política e
Data safety e mede o peso no gate; o usuário cria o projeto gratuito no Firebase quando a sessão pedir. Deve
sair antes da RT-9.

## 2026-11-20 — T-RT8-1 — Sons extras e área segura no kit, vibração sem alocação — técnica (sessão RT-8)

Pauta 5.1 da RT-4: o kit ganhou a tabela de **sons extras** no `ThemeAudio` (`ThemeExtraSound`: id, clipe,
volume, variação de tom, intervalo mínimo e vibração própria opcional), tocada por `GameAudio.PlayExtra(índice)`
com as mesmas vozes pré-alocadas, o mesmo botão de som e as mesmas pausas dos papéis fixos. O jogo procura cada
id uma vez (`ClaybreakSfx` virou só o mapa "som do jogo → índice") e o componente próprio de vozes saiu.
`GameHaptics` passou a vibrar por JNI cru com referências e argumentos em cache (zero alocação por vibração; antes
cada vibração alocava objetos de ponte Java, o que alimentava o coletor de lixo durante a corrida) e ganhou
`Pulse`. Novo `SafeAreaFitter` (UI do kit) encaixa HUD e conteúdo dos menus na área segura do celular, com o véu
escuro ainda cobrindo a tela toda. O teste de "pool sem alocação" do kit ficou estável: o Editor às vezes aloca
nos próprios avisos de hierarquia quando um objeto liga/desliga; o caminho do pool é determinístico, então o teste
aceita a primeira de até 5 tentativas limpas. Pacote 0.3.0 (entrega K.8 do `studiokit`).

## 2026-11-20 — T-RT8-2 — Áudio próprio por síntese, fonte Roboto e build de release — técnica (sessão RT-8)

Sem áudio CC0 na máquina e sem autorização para baixar arquivos, o áudio definitivo é **obra própria**: um
sintetizador em Python versionado (`Tools/audio/clay_siege_audio.py`, sementes fixas) grava os 20 efeitos e as 2
músicas; licenças em `docs/audio-licenses.md` e o validador recusa clipe fora da lista. Os baques têm um "corpo
médio" (300–800 Hz) porque o alto-falante do celular quase não toca abaixo de ~250 Hz. Vibração revisada: só nos
momentos grandes (portões, caixote, muro/escudo, andar, queda da torre, chefe, derrota, recompensa) e nunca na
perda de unidade (vem em rajadas de até 300) nem na pedra do canhão (cai a cada 1,6–3 s). Fonte: Roboto Black (Apache 2.0, cópia sem modificação que já vem
num pacote da Unity; licença ao lado do arquivo). Release: `ClaybreakBuild` gera AAB (`-claybreakAab`) e APK como
player de release; sem chave, assina com a chave de debug do Unity; a chave de upload real só entra por variáveis
de ambiente apontando para fora do repositório, e as configurações do projeto voltam ao que eram depois do build.
Link da política de privacidade como dado (`MonetizationContent.PrivacyPolicyUrl`, vazio até a RT-9 publicar a
página; o validador recusa build de produção sem ele).

## Ficha D22 (RT-8, respondida em 2026-11-21: A)

### DECISÃO D22 — Aprovar o som e a vibração do jogo

O que você está aprovando: o som definitivo do Clay Siege é o que está no celular agora — 20 efeitos e 2 músicas
**feitos por nós** (criados por um programa do próprio projeto, sem nenhum som de terceiros, então sem licença a
pagar ou creditar) — e a vibração só nos momentos grandes; isso vale até o soft launch (lançamento de teste),
e qualquer som pode ser trocado depois com o mesmo programa.

Por que agora: é o aceite da RT-8 ("ouvir e aprovar o som"). Não trava código: o jogo já usa a recomendação.
Trava só o fechamento da RT-8; precisa sair antes da RT-9 (preparação da loja), porque o vídeo e as imagens da
loja usam o jogo como ele está.

Contexto que você precisa saber:
- O que trocou (fato, esta sessão): saíram os bipes provisórios da RT-2/RT-4. Entraram sons com cara de argila
  e de cerco: baque surdo quando um soldado bate na torre, estrondo do canhão, **assobio de queda 0,6 s antes da
  pedra cair** (o aviso de projétil que o plano pedia), impacto da pedra, estalo quando uma unidade se desfaz,
  desmoronamento de cada andar, **queda da torre** com escombros, muro quebrando, zunido do varredor, trompas
  graves do **chefe**, arpejo nos portões bons e "bwomp" nos ruins, fanfarra de vitória e trombone de derrota.
  Músicas: menu calmo (marimba, 96 batidas por minuto, 20 s em laço) e corrida em marcha (tambores, 128 bpm,
  15 s em laço, também no assalto).
- Por que "feitos por nós" e não CC0 (CC0 = domínio público, uso livre): a regra da RT-8 é só CC0 ou próprio,
  nada pago. Não havia som CC0 no computador e baixar arquivos da internet exige o seu ok; o som próprio cumpre a
  regra sem depender disso (fato). Qualidade: é síntese (sons gerados por conta, não gravados); soa mais
  "desenho animado" que um banco de sons gravados (inferência nossa; só ouvindo para julgar).
- Vibração (fato, esta sessão): antes vibrava a cada unidade perdida (até 300 por ataque). Agora vibra leve no
  portão bom, mais forte no ruim, toque no caixote, no muro ou escudo quebrando, em cada andar, longa na queda
  da torre, no chefe, na derrota e na recompensa. A pedra do canhão não vibra (cairia a cada 1,6–3 s e o ataque
  vibraria sem parar; ajuste da segunda opinião). Ligar a vibração nos ajustes dá um toque de teste.
- Os botões SOUND, MUSIC e VIBRATION saíram da tela inicial e foram para a nova tela **SETTINGS** (botão no
  canto superior esquerdo), junto com o link da política de privacidade (fato).
- Desempenho (gate 3× = teste no OPPO que exige folga de 3 vezes no tempo de cada quadro): **aprovado** em 2026-11-21 no build de release, com som,
  música e vibração ligados, no caso mais pesado (chefe do mundo 4, 299 soldados em voo, 12 pedras no ar):
  quadro p95 16 ms, nenhum quadro acima de 50 ms, CPU 4,8 ms e GPU 4,3 ms no pior trecho (limite 11 ms),
  zero coletas de lixo, memória 228 MB (limite 450 MB).
- Termos: "linha de tiro" = onde a corrida acaba e o exército começa a ser lançado contra a torre;
  "varredor" = a barra que varre a pista no assalto; "regra de chefe" = o efeito especial que o chefe liga a
  cada andar perdido (ex.: canhão mais rápido).

O que ouvir em cada tela (volume de mídia ligado, uns 5 minutos):
1. Tela inicial: música do menu; toque nos botões; SETTINGS → ligar/desligar som, música e vibração; BACK.
2. Corrida (fase 1): música de marcha; portão bom (arpejo + vibração leve), portão ruim ("bwomp" + vibração),
   caixote (sino + toque).
3. Linha de tiro: trompa com tambor.
4. Assalto: baques na torre, canhão, assobio antes da pedra cair e o impacto, estalos das unidades, muro e
   varredor (nas fases que têm essas defesas), andar caindo.
5. Fim: queda da torre (estrondo + vibração longa) junto com a fanfarra — os dois começam no mesmo instante; a
   música para e a tela de resultado fica em silêncio depois. Ou derrota (trombone + vibração).
6. Chefe (fase 10 de um mundo): trompas graves + tambores na entrada e a cada andar com regra de chefe.
7. Loja: som de moeda ao comprar; presente diário/anúncio premiado: brilho de recompensa.

Opções:
A) Aprovar como está.
   + Fecha a RT-8; sem custo; sem licença de terceiros; tudo reproduzível e ajustável.
   - Riscos: som sintetizado soar simples demais para alguns jogadores (chance média; impacto baixo — o soft
     launch mede retenção, não som); músicas curtas (15–20 s) cansarem em sessões longas (chance média; impacto
     baixo); sons grandes que se sobrepõem (queda + fanfarra; no chefe, trompa da linha de tiro + trompas do
     chefe) (certo; impacto baixo); ajustado para o alto-falante do celular, não testado em fone (chance baixa;
     impacto baixo). Tudo se ajusta depois sem mexer no código do jogo.
   Custo: zero. Dá para voltar atrás? sim, fácil.
B) Aprovar com ajustes: dizer quais sons ou vibrações mudar (ex.: "assobio mais baixo", "sem vibração no
   portão bom", "música mais longa", "fanfarra só depois da queda").
   + Mesmo caminho, só afinando.
   - Riscos: mais uma rodada curta de sessão (chance alta; impacto baixo).
   Custo: ~meia sessão. Volta atrás? sim.
C) Trocar por sons CC0 gravados (ex.: pacotes de áudio da Kenney, domínio público), mantendo a música própria
   ou trocando também.
   + Sons gravados, mais "reais".
   - Riscos: você precisa autorizar o download de cada pacote (nome, origem e tamanho na hora); casar 20 papéis
     com pacotes genéricos pode deixar lacunas, como o assobio e a queda da torre (chance média; impacto baixo);
     APK alguns MB maior (inferência nossa).
   Custo: grátis; ~1 sessão. Volta atrás? sim.
D) Adiar a escuta até o soft launch.
   + Nada a fazer agora.
   - Custo de adiar: vídeo e imagens da loja (RT-9) saem com um som que você não ouviu.

Recomendação: A — o som cobre todos os momentos que o plano pediu, cumpre a regra de licença sem depender de
download e é fácil de ajustar; se algo incomodar ao ouvir, B resolve em meia sessão.
Segunda opinião (fable): concorda com A. Pediu (incorporado): tirar a vibração de cada pedra, avisar da
sobreposição queda + fanfarra e do silêncio no resultado, dar o resultado do gate e explicar os termos.
Se você não responder: o jogo segue com este som (já instalado no OPPO); a RT-8 fica concluída com o aceite
"ouvir e aprovar o som" pendente, e a RT-9 espera a sua resposta antes de gravar vídeo e imagens da loja.

Responda: A, B + o que mudar, C, D, ou "explica melhor".

**Resposta do usuário (2026-11-21): A — aprovar como está.** O som próprio e a vibração da RT-8 são os
definitivos até o soft launch; o aceite "ouvir e aprovar o som" da RT-8 está fechado.

## 2026-11-21 — T-RT8b-1 — Firebase Analytics mínimo pelo SDK Android nativo — técnica (sessão RT-8b)

Implantação da D21 = A. O SDK Unity do Firebase não está no OpenUPM nem no registro da Unity, traz bibliotecas C++
por arquitetura e outro gerenciador de dependências; a medição só precisa de `logEvent`, `setConsent` e
`setAnalyticsCollectionEnabled`. Então: módulo `Assets/Plugins/Android/FirebaseAnalytics.androidlib` com uma única
dependência fixa (`firebase-analytics` 24.0.0) e as 9 flags mínimas `false` no manifesto (coleta, ID de publicidade,
ID do Android, sinais e consentimentos de anúncio, telas automáticas); adaptador `FirebaseAnalyticsSink` e regra
`MeasurementConsent` no kit (K.9, pacote 0.4.0, T22 do kit), sem nomear o jogo nem levar o SDK ao Wild Pack.
Consentimento: na Europa/Reino Unido (`IABTCF_gdprApplies=1`) só com propósitos TCF 1 e 8 e o Google (755) aceitos;
fora, liberado com aviso na política; sem resposta TCF, só depois de uma atualização UMP bem-sucedida sem opções de
privacidade exigidas. A regra é lida ao fim de cada fluxo UMP (também depois do formulário PRIVACY, que pode retirar
o consentimento). O `google-services.json` fica em `..\_privado\runner-vs-tower\` (fora de todos os repositórios;
variável `CLAYBREAK_GOOGLE_SERVICES_JSON` para outro caminho) e vira recurso Android no build (`ClaybreakFirebase`);
o build recusa o arquivo dentro de um repositório e o `.gitignore` o recusa. A permissão `AD_ID` continua no APK por
causa do AdMob (RT-7); o Firebase não lê o ID de publicidade. Peso: APK +1,1 MB, AAB +1,0 MB.

## 2026-11-21 — FATO — A Jungle Degree é uma empresa portuguesa (informado pelo usuário)

O usuário mora em Portugal e a empresa é **portuguesa**, não brasileira. Consequências para todas as entregas:
- o **RGPD** vale para todo o tratamento de dados da empresa, em qualquer país (art. 3.º, n.º 1), mais a regra
  ePrivacy (Lei n.º 41/2004) para gravar/ler no aparelho; a autoridade é a **CNPD**;
- textos legais (política de privacidade, Data safety, termos) citam a empresa portuguesa, o RGPD, a CNPD e os
  direitos do titular; o português dos textos para o público é o de Portugal;
- a checagem de marca da RT-9 passa a incluir **EUIPO** e **INPI Portugal** (INPI.pt), além do TMview (e o INPI do
  Brasil só se o Brasil for mercado);
- o soft launch, a conta do Play Console e o AdMob ficam com os dados da empresa portuguesa;
- a conta do Google Analytics do projeto Firebase "Clay Siege" foi criada com local Portugal.

## 2026-11-21 — T-RT8b-2 — Medição só com consentimento explícito em todos os países — técnica (sessão RT-8b)

Com o fato acima, a regra da D21 "fora da Europa/Reino Unido, medir com aviso e interesse legítimo (LGPD)" deixou de
ser a mais segura. Regra nova, a mais segura: medir só com consentimento explícito, em qualquer país. Na Europa e no
Reino Unido, o "sim" no UMP (propósito TCF 1, 8 ou 9, Google 755); nos demais países, só com um consentimento
próprio do jogo, que ainda não existe (`ClaybreakMetrics.GameConsentGiven = false`), então lá nada é medido. Isso
mede **menos** do que a D21 aprovou: a forma de medir fora da Europa vai ao usuário na ficha **D23**. Política
reescrita (pt-PT, responsável português, consentimento como base, direitos, CNPD, transferências) e Data safety
ajustada ("opcional"). `MobileAdsGate.ConsentInformationUpdated` saiu do kit (ficou sem uso).

## Ficha D23 (RT-8b, respondida em 2026-11-21: B)

### DECISÃO D23 — Medir jogadores fora da Europa: com que pedido de consentimento?

O que você está aprovando: a forma de pedir o "sim" dos jogadores **fora da Europa** para a medição mínima (Firebase,
sem nome nem ID de publicidade): **primeiro você confere no AdMob se a janela de consentimento do Google pode
aparecer em todos os países (opção E); se puder, ela vale para todos; se não puder, o jogo mostra uma pergunta
própria curta na primeira abertura fora da Europa (opção A)**. Só quem disser "sim" é medido.

Por que agora: a Jungle Degree é portuguesa, então o RGPD (a lei europeia de proteção de dados) vale para todos os
jogadores, em qualquer país, e a regra aprovada na D21 ("fora da Europa, medir com aviso na política") deixou de ser
a mais segura. Para não arriscar, a RT-8b já deixou o jogo **sem medir nada fora da Europa** até esta resposta. Não
trava a RT-9; trava os números do soft launch (lançamento de teste) se ele for feito fora da Europa.

Contexto que você precisa saber:
- Fato (você, 2026-11-21): empresa portuguesa; pelo art. 3.º, n.º 1, do RGPD, o regulamento vale para todo o
  tratamento dela, onde quer que o jogador esteja. A autoridade é a CNPD.
- Fato (documentação do Google): a janela de consentimento do Google (UMP) aparece por padrão só no Espaço
  Econômico Europeu, Reino Unido e Suíça (e há outra mensagem, de "não vender dados", para alguns estados dos EUA).
  No Brasil e na maior parte do mundo ninguém vê pergunta nenhuma.
- Memória não verificada (segunda opinião): no AdMob, em Privacidade e mensagens, a mensagem europeia teria uma opção
  de alcance "todos os países". Se existir, a mesma janela aparece para todo mundo e o jogo mede com o "sim" sem
  código novo. Você confere em 5 minutos: apps.admob.com > Privacidade e mensagens > Regulamentos europeus > a
  mensagem do app > países/alcance.
- Fato (código, RT-8b): hoje o jogo mede só quem disse "sim" na janela do Google; fora dela não mede. Os eventos são
  os 4 da D4 (sessão, início e fim de fase, premiado), sem nome nem ID de publicidade.
- Inferência nossa: o RGPD aceita "interesse legítimo" para estatísticas, mas a medição grava e lê um identificador
  no aparelho, o que na Europa exige consentimento (regra ePrivacy); se isso vale para aparelhos fora da Europa é
  discutível. Pedir o "sim" é a leitura mais segura. Não é parecer jurídico.
- Inferência nossa: uma pergunta de consentimento tem boa parte de "sim", mas não 100%; o D1/D7 (% de jogadores que
  voltam 1 / 7 dias depois de instalar) passa a ser medido só em quem aceitou, o que pode enviesar um pouco.
- Ligado a isto (segunda opinião): os **anúncios** fora da Europa também seguem o RGPD pela mesma razão. Pedir "sim"
  para estatística anônima e mostrar anúncios com ID de publicidade sem perguntar é difícil de defender. A opção E
  resolveria as duas coisas de uma vez; a A resolve só a medição (os anúncios ficam para a RT-9).
- Os países do soft launch ainda não foram escolhidos (RT-9/RT-10).

Opções:
E) Janela do Google para todos os países (se o AdMob permitir).
   + Uma janela só, para medição e anúncios, em todo o mundo; nenhum código novo; mesma regra para todos.
   - Riscos: a janela do Google é mais pesada que um sim/não (chance alta; impacto médio: mais gente recusa a medição
     e os anúncios personalizados, menos receita por anúncio no mundo todo); a opção pode não existir (não verificado).
   Custo: 5 minutos seus no AdMob + um teste no celular. Volta atrás? sim, fácil.
A) Pergunta própria no jogo fora da Europa ("Podemos medir como você joga, sem nome nem ID de publicidade, para
   melhorar o jogo? Sim / Não", "Não" tão fácil quanto "Sim", nada pré-marcado; interruptor em SETTINGS; o jogo guarda
   a data e a versão do texto aceito, como o RGPD pede para provar o consentimento).
   + Mede fora da Europa com consentimento claro; não mexe nos anúncios.
   - Riscos: mais uma tela na primeira abertura (chance alta; impacto baixo); parte diz "não" e o D1/D7 fica com menos
     gente (chance alta; impacto médio); se a RT-9 concluir que os anúncios também precisam de "sim" no mundo todo,
     esta tela vira retrabalho ou segunda pergunta (chance média; impacto baixo); nos estados dos EUA com mensagem
     própria do Google, a pergunta aparece junto dela (chance alta; impacto baixo).
   Custo: grátis; ~meia sessão (tela, save, testes) + o texto passa pela sua revisão. Volta atrás? sim, fácil.
B) Medir só na Europa (como está agora).
   + Nada a fazer.
   - Riscos: se o soft launch for fora da Europa, D1/D7 e funil ficam sem número (chance média; impacto alto na RT-10).
   Custo: zero. Volta atrás? sim.
C) Voltar ao que a D21 aprovou: fora da Europa medir sem perguntar, com aviso na política (interesse legítimo).
   + Mais dados, nenhuma tela a mais.
   - Riscos: empresa europeia medindo sem consentimento um identificador no aparelho pode ser questionada pela CNPD ou
     por um jogador (chance baixa; impacto alto: multa e apagar dados).
   Custo: zero (trocar uma linha). Volta atrás? sim.
D) Adiar até escolher os países do soft launch (RT-9).
   + Decide sabendo onde será o teste; se for na Europa, B basta.
   - Custo de adiar: se o teste for fora da Europa, E ou A vira trabalho em cima da hora.

Recomendação: **E, e A se E não existir ou a janela pesar demais** — conferir o AdMob custa 5 minutos e, se der,
resolve medição e anúncios com uma janela só; a pergunta própria (A) é o plano de reserva seguro. C não é recomendada
para uma empresa sujeita ao RGPD.
Segunda opinião (fable): achou a A segura, mas apontou a opção que faltava (janela do Google em "todos os países", de
memória, a confirmar no console) e a ligação com os anúncios; recomendou conferir isso antes e decidir medição e
anúncios juntos, com A como reserva. Incorporado (opção E, Suíça, prova do consentimento na A).
Se você não responder: o jogo continua medindo só onde a janela do Google aparece (opção B); nada mais espera por isto.

Responda: E, A, B, C ou D, ou "E, e se não der A", ou "explica melhor".

**Resposta do usuário (2026-11-21): B** — "Testes serão feitos na Europa, não se preocupe." O soft launch e os
testes serão na Europa; o jogo segue medindo só com o "sim" da janela do Google (EEE, Reino Unido, Suíça) e não
mede fora dela. Isto também orienta a D9 (países do soft launch: Europa). Rever esta ficha (E ou A), junto com
os anúncios fora da Europa, antes de publicar o jogo fora da Europa.

## 2026-11-21 — Direção visual "crowd toon" — pedido do usuário (plano proposto, aguardando aprovação)

- Fato: o usuário aprovou como padrão visual dos jogos o protótipo three.js "Crowd runner toon" (agora em
  `docs/reference/crowd-proto.html`) e deu a especificação (toon de 3 faixas, multidão instanciada de 1000+ a 60
  fps, formas Bolinha e Pintinho, animação procedural, formação girassol, 3 paletas, cenário reciclado, portões com
  Baloo 2). Substitui a direção v2 (D24, RT-6v.1), descartada e revertida em `fc1620e`.
- Pediu planejamento antes de implementar: plano em `docs/direcao-visual-crowd.md`, entregas RT-6t.1–6t.12 em
  `plano-de-entregas.md`, todas "proposta, aguardando aprovação do usuário".
- Fichas abertas pelo planejamento (texto completo na seção 5 de `direcao-visual-crowd.md`; segunda opinião
  fable incorporada): **D25** público/"fofura" (trava a 6t.2), **D27** teto da multidão (sem resposta: 300),
  **D26** skins × formas × paletas (trava a 6t.6), **D28** arte do assalto (sem resposta: A), **D29** nome "Clay
  Siege" sem massinha (trava a RT-9).

## 2026-11-21 — T-RT6t-1..9 — Decisões técnicas do plano crowd toon — técnica (sessão de planejamento)

- T-RT6t-1: motor escrito com namespaces/asmdefs do kit (`JungleDegree.StudioKit.Toon`, `.Swarm`), incubado em
  `Assets/StudioKitIncubator/` até a integração no jogo provar a API, e levado ao studiokit numa entrega só
  (RT-6t.12 = K.11, aceitação do Wild Pack uma vez); `RunnerCrowd`/`RunnerPool`/massinha do kit não mudam.
- T-RT6t-2: simulação e animação na CPU com Jobs + Burst; desenho indireto com buffer estruturado (1 + 1 chamadas);
  animação no vertex shader é o plano B medido.
- T-RT6t-3: shader HLSL próprio (`StudioKit/ToonLit`), não Shader Graph.
- T-RT6t-4: a primeira entrega é um portão de desempenho no teto do orçamento (500 tri, 1000 vivos, sombras, MSAA).
- T-RT6t-5: a corrida do jogo recebe a multidão nova cedo (6t.4).
- T-RT6t-6: Baloo 2 (SIL OFL 1.1) substitui a Roboto Black; TextMeshPro só onde há contorno.
- T-RT6t-7: caminho de reserva para OpenGL ES (`RenderMeshInstanced` em blocos) antes do soft launch (6t.11).
- T-RT6t-8: numeração RT-6t.x aqui e K.11 no studiokit (K.10 foi a direção revertida).
- T-RT6t-9: série sequencial (regra global de uma sessão por repositório); pares sem conflito de arquivos só com
  autorização do usuário.
- Também decidido pelo planejamento, provisório e trocável: 4ª paleta "Aurora" para o mundo 4 (o pedido tem 3
  paletas, o jogo tem 4 mundos; D12 = A).

## 2026-11-21 — Plano da direção visual "crowd toon" (RT-6t.1 a RT-6t.12) — **aprovado** — usuário

- O usuário aprovou em 2026-11-21 o plano `docs/direcao-visual-crowd.md` inteiro: entregas RT-6t.1 a RT-6t.12
  (seção 4), mapeamento técnico (seção 1), orçamento e plano B (seção 1.2) e as decisões técnicas T-RT6t-1..9
  (seção 6). As seções RT-6t.* de `plano-de-entregas.md` passam a "aprovada em 2026-11-21"; a série começa pela
  RT-6t.1 (portão de desempenho no OPPO).
- **D25, D27 e D26 continuam pendentes** (sem resposta do usuário): valem os padrões "se você não responder" das
  fichas — D25: a RT-6t.2 faz as duas variantes e para antes de escolher; D27: teto de 300 em tudo (o motor é feito
  para 1000 e a troca é dado); D26: trava a RT-6t.6. D28 (sem resposta: A) e D29 (trava a RT-9) seguem como estão.

## 2026-11-21 — T-RT6t1-1..9 — Portão técnico da multidão toon — técnica (sessão RT-6t.1)

- **T-RT6t1-1 — Desenho por `Graphics.RenderMeshPrimitives`, não `RenderMeshIndirect`.** A contagem viva já está na
  CPU (a simulação é Burst na CPU, T-RT6t-2), então um buffer de argumentos não ganha nada; continua **1 chamada**
  para os corpos e **1** para as sombras-bolha, as duas lendo o mesmo `GraphicsBuffer` estruturado. O formato da
  instância (`ToonInstance`, 64 bytes: transformação 3×4 + tinta, `w` livre para o dono) mora no assembly
  `Toon` (com o espelho `ToonInstanceData.hlsl`), para o cenário instanciado (6t.8) usar o mesmo caminho.
- **T-RT6t1-2 — Rampa gerada na carga a partir do `ToonLook`** (textura R8 linear, filtro point, clamp, uma
  texel por faixa) em vez de um asset de textura: um dado só (os níveis no `ToonLook`), nada para dessincronizar.
  `ToonLookApplier` escreve rampa, céu/chão do ambiente e intensidades como valores globais de shader.
- **T-RT6t1-3 — O URP asset do jogo não muda de sombras nesta entrega.** O jogo atual ainda usa a luz com sombra
  (`MainLightShadowsSupported = 1`); desligar no asset mudaria o visual do jogo antes da 6t.4. A `ToonLab` usa uma
  luz direcional sem sombra, e o URP pula o passo de sombras. MSAA e HDR são medidos por extras de lançamento
  (`--ei msaa`, `--ez hdr`) que mudam o asset em memória só no player; o padrão do jogo (MSAA e HDR) é decidido
  pelo resultado do gate e aplicado na 6t.4, quando a corrida usar o visual novo.
- **T-RT6t1-4 — Regra 2.2.1 também na incubadora e no laboratório.** O validador do jogo passa a varrer
  `Assets/Claybreak/Lab/Runtime` e `Assets/StudioKitIncubator/{Toon,Swarm}`; uma linha marcada `// load-time:`
  (criação/liberação na carga ou descarga do dono, nunca em jogo) é aceita, e fica visível na revisão. O validador
  de isolamento do kit cobre a incubadora (`folder Assets/StudioKitIncubator` na lista de termos do jogo).
- **T-RT6t1-5 — Cena `ToonLab` só no APK de medição.** `--ez toonlab true` desativa a cena do jogo antes do `Start`
  e carrega a `ToonLab` (gerada por `ToonLabBuilder`, chamado pelo construtor de cena). A lista de cenas do jogo
  continua só com `Claybreak.unity`.
- **T-RT6t1-6 — Pacotes.** `com.unity.burst` 2.0.0 e `com.unity.collections` 6.6.0 (as versões embutidas que o
  `packages-lock` já resolvia) passam a ser dependências diretas; `Unity.Mathematics` é módulo do motor no Unity
  6.6 (sem pacote). Shaders com `#pragma target 4.5` (buffer estruturado no vertex shader: Vulkan e GLES 3.1+); a
  reserva sem buffer é a 6t.11.
- **T-RT6t1-7 — Câmera da `ToonLab` em retrato.** A fórmula do protótipo (distância `1 + √n/22`, atrás da multidão,
  olhando 5 m à frente; espelhada para a frente +Z do Unity) com o campo de visão vertical aberto até a formação
  inteira caber na largura do retrato (50°–100°). O ajuste definitivo da câmera a 9:16 é da 6t.3.
- **T-RT6t1-8 — Sem dispositivo gráfico, a multidão simula sem desenhar.** Os testes PlayMode do `rt4-check` rodam
  com `-nographics` (dispositivo `Null`); ali o `SwarmRenderer` registra uma linha e só simula, em vez de acusar
  erro. Num aparelho real sem buffer estruturado no vertex shader continua sendo erro (a reserva é a 6t.11). O
  desenho é provado pelo mesmo teste rodado com GPU (`-runTests -testFilter ToonLabSceneTests` sem `-nographics`).
- **T-RT6t1-9 — Resultado do gate e padrão de antisserrilhado.** O gate passou com folga em todas as combinações
  (pior caso 1000 com MSAA 4× e HDR: GPU p95 5,5 ms de 11; `rt6t-crowd-toon.md`), então o visual novo pode usar
  **MSAA 4×** (item 22 do plano: "se couber no gate"); a 6t.4 liga no URP asset junto com a troca da multidão e
  regenera o gate na corrida. HDR não muda o custo no OPPO e fica como está. O validador do jogo passa a recusar
  shader com erro de compilação nos materiais da `ToonLab` (a primeira rodada no aparelho mediu o shader de erro
  magenta sem nenhum aviso em tempo de jogo).

## 2026-11-21 — T-RT6t3-1..8 (e 6b) — Formação, arraste e câmera da multidão toon — técnica (sessão RT-6t.3)

- **T-RT6t3-1 — Formação como tabela + job de seguimento antes do job de animação.** `SunflowerFormation` (kit,
  `Swarm`) guarda as fórmulas puras (offset, raio, fator de seguimento `1 − e^(−k·dt)`, ritmo por unidade, vaga com
  clamp na pista); a `SwarmSimulation` calcula a tabela de offsets da capacidade inteira **na carga** e, por quadro,
  move o centro (`cx += (alvo − cx)·(1 − e^(−8dt))`, na thread principal, como o protótipo) e encadeia dois jobs
  Burst paralelos: `SwarmFollowJob` (cada unidade segue `clamp(cx + offset)` no X com seu ritmo e o offset no Z) →
  `SwarmAnimateJob` (quique/squash). Zero trigonometria por quadro na formação, nada alocado em jogo.
- **T-RT6t3-2 — Posição da unidade = (x do mundo, z relativo ao centro).** O protótipo deixa a multidão parada no Z e
  move o mundo; aqui o centro tem `CenterZ` (vem do `transform` do `SwarmRenderer`), então a 6t.4 pode andar a
  multidão para a frente sem tocar nos arrays. A pista é `laneHalfWidth` centrada no X do `SwarmRenderer`
  (0 = sem pista); na 6t.4 o valor vem do nível.
- **T-RT6t3-3 — Ritmo por unidade escrito como no protótipo, com o espalhamento como dado.** `(1 − s/2) + s·((7i mod
  10)/10)` com `s = 0,4` dá exatamente `0,8 + 0,4·((7i mod 10)/10)` do protótipo. Correção de registro: a faixa real é
  0,8–1,16 (o plano dizia 0,8–1,2).
- **T-RT6t3-4 — Arraste no kit como fórmula pura, leitura do dedo no jogo.** `SwarmSteering` (kit) tem o alvo do
  arraste (`alvo no toque + Δx/largura da tela·16`, preso em `±(meia pista − 0,5)`) e o passo das setas (±1,5), com
  os números em `SwarmDragSettings`; quem lê o Input System é a `ToonLab` (na 6t.4, a corrida). O assembly `Swarm`
  não passa a depender do Input System. Toque que começa dentro do painel de teste não arrasta.
- **T-RT6t3-5 — Câmera: `SwarmCameraRig` + perfil `SwarmCameraProfile` (ScriptableObject) + fórmulas puras em
  `SwarmCameraMath`.** Distância `d → 1 + √n/22` suavizada `1 − e^(−2dt)`, posição `(lane + (cx − lane)·0,35,
  3,2 + 4,2d, z − (5,5 + 5,5d))`, alvo `(…, 0, z + 5)` — o protótipo espelhado para a frente +Z —, tremor de 0,35 s com
  `±(resto·0,6)/2` em X e Y (sorteio por `Unity.Mathematics.Random`, sem alocação) e o mesmo limite de passo de
  0,05 s do protótipo. Todos os números no perfil; o jogo terá um perfil por momento (corrida, assalto). O perfil da
  `ToonLab` é `Assets/Claybreak/Lab/SwarmCameraLab.asset` (criado uma vez pelo construtor e depois deixado como dado).
- **T-RT6t3-6 — Enquadramento de retrato: a pista inteira cabe, a câmera continua se afastando com a contagem e não
  "respira" no arraste.** Em tela larga nada muda (FOV 50, pose do protótipo). Em retrato (`SwarmCameraMath.FitLane`,
  aspecto real da tela): a pista + 0,7 m de margem por lado (trilhos, postes) precisa caber na largura na linha da
  multidão. O ajuste é resolvido **uma vez, para a multidão mínima (d = 1) com o centro na borda da pista** (pior
  caso): primeiro abre o FOV vertical até `MaxVerticalFov` = 65°; se não bastar, recua ao longo da linha de visão
  (mesmo ângulo, sem grande-angular). Esse recuo vira uma **distância extra fixa** somada à pose do protótipo, e o
  afastamento `1 + √n/22` continua por cima. Três tentativas descartadas, medidas na captura: (a) ajustar com o centro
  real da multidão — a câmera recuava até 5,6 m a cada arraste para a borda no OPPO; (b) ajustar a cada contagem — o
  ajuste engolia o afastamento (a câmera ficava a ~29 m com 10 e com 1000 unidades, contra o aceite "câmera se
  afastando"); (c) só abrir o FOV — ~81° em 9:16 (olho-de-peixe). 65° e a margem são dados do perfil (50 = só recuo;
  100 = só FOV).
- **T-RT6t3-6b — A neblina da `ToonLab` se afasta junto com o enquadramento.** Caber a pista em retrato põe a câmera
  1,4–1,9× mais longe que o protótipo (que foi feito deitado); com a neblina do protótipo (22–75 m) a multidão ficava
  dentro dela e desbotava. O rig expõe `FramingScale` (distância atual ÷ distância do protótipo na mesma contagem; 1 em
  tela larga) e a `ToonLab` multiplica início e fim da neblina por ele (opção `scaleFogWithFraming`); o plano de
  corte da câmera passa a 250 m e a pista do laboratório começa 60 m atrás. **Para a 6t.6:** o aplicador de paleta
  deve fazer o mesmo (neblina da paleta × `FramingScale` da câmera ativa), em vez de a `ToonLab` mexer no
  `RenderSettings`.
- **T-RT6t3-7 — Diminuir a contagem tira o anel de fora na hora.** Nascimento elástico e morte balística são da 6t.5;
  aqui a unidade nova aparece direto na sua vaga e a removida some (como o `SetCount` do protótipo sem o `dying`).
- **T-RT6t3-8 — Sem gate novo no OPPO; captura no Editor com GPU.** O aceite da 6t.3 pede testes e captura 9:16, não
  gate. O custo novo por quadro é um job Burst de ~1000 `float2` (bem abaixo da folga medida na 6t.1: CPU p95 1,9 ms de
  11). O painel de teste fica escondido nas execuções de medição (`--ez toonlab true`), então a medição da `ToonLab`
  continua sem a interface de teste; só o enquadramento mudou (câmera mais longe em retrato, multidão menor na tela, o
  que não aumenta o custo de GPU medido na 6t.1). A captura é um teste PlayMode que só roda com GPU e com `-toonLabCaptureDir` (renderiza a
  câmera do laboratório numa textura do tamanho do retrato); a próxima medição no aparelho é a da 6t.4, na corrida.

## 2026-11-21 — T-RT6t2-1..7 — As duas formas da multidão toon — técnica (sessão RT-6t.2)

- **T-RT6t2-1 — Receitas = `unitGeo()` do protótipo espelhado, com menos segmentos.** `UnitShapeRecipes` (Editor do
  jogo) monta as formas com o `LowPolyMeshBuilder` da incubadora seguindo posição, raio e escala de cada peça do
  protótipo; o ponto (x, y, z) do three.js vira (x, y, −z) e a rotação em X troca de sinal (frente +Z do Unity). O cone
  do three.js é centrado na altura e o do construtor tem a base em y = 0: a receita desloca meia altura ao longo do eixo
  girado (bico e rabinho). Orçamento (200–500): corpo 16×11 (o protótipo usa 18×14 na Bolinha e 16×12 no Pintinho,
  ambos acima de 500 com o resto), esferinhas 6×4 (o protótipo usa 8×6). **Bolinha 484 tri (412 sem olhos), Pintinho
  492 (420 sem olhos).** O "Feijão" do protótipo não entra (a direção pede só Bolinha e Pintinho).
- **T-RT6t2-2 — Variante sem olhos = a mesma receita sem as duas esferas dos olhos.** Nada mais muda (D25 opção C fala
  em "sem olhos ou olhos bem pequenos"; tirar é o caso que a ficha pediu pronto). As quatro malhas ficam em
  `Assets/Claybreak/Content/Generated/Units/` (`Bolinha`, `BolinhaNoEyes`, `Pintinho`, `PintinhoNoEyes`), geradas por
  `UnitShapeAssets` dentro do construtor da `ToonLab` (e portanto do construtor de cena), com GUID estável. A Bolinha
  provisória da 6t.1 (`Lab/Generated/BolinhaProvisional.asset`) foi apagada: a final a substitui (mesmo corpo, haste
  agora com tampas como no protótipo, 474 → 484).
- **T-RT6t2-3 — Máscara de tinta:** corpo e rabinho do Pintinho com alfa 255 (recebem a cor da paleta/skin); olhos
  #2b2233, folhas #5cc98a, bico e topete #ff9f43 com alfa 0 (cor fixa), como as cores fixas do protótipo.
- **T-RT6t2-4 — Regra do validador por forma (`UnitShapeRules`, compartilhada com os testes).** Para cada uma das
  quatro malhas: 1 submalha, 200–500 triângulos, índices 16 bits; todo vértice com alfa 255 ou 0 e as duas classes
  presentes; ponto mais baixo em y = 0; olhos só nas variantes com olhos e na frente (+Z); o detalhe da frente (olhos na
  Bolinha, bico no Pintinho) avançando para +Z além de 0,9 R — uma receita que esquecer o espelho three.js → Unity falha.
  Os testes EditMode provam que a regra pega olhos faltando/sobrando, a forma virada e o orçamento estourado, e conferem
  as posições do bico, topete, rabinho, olhos e folhas contra os números do protótipo.
- **T-RT6t2-5 — Seletor na `ToonLab` sem realocar nada.** `SwarmRenderer.SetUnitMesh` (incubadora, genérico) só troca a
  malha desenhada (o buffer de instâncias não depende dela). A `ToonLab` guarda as formas como dado (rótulo + malha com e
  sem olhos) e o painel ganhou uma segunda linha: **Bolinha, Pintinho, Com olhos, Sem olhos** (grade 4×2; os botões
  continuam filhos diretos do `TestPanel`). Padrão provisório: **Bolinha com olhos** (D25 aberta). No APK de medição:
  `--ei shape 0|1 --ez eyes true|false`; a linha `[ToonLab]` do log registra a forma.
- **T-RT6t2-6 — Capturas lado a lado reproduzíveis.** `Tools/rt6t/shape-captures.sh`: (1) teste PlayMode
  `UnitShapeCaptureTests` (só com GPU e `-shapeCaptureDir`) renderiza cada forma de perto (frente 3/4, lado, costas
  3/4, FOV 30, uma unidade parada sobre a cor da neblina) e como multidão de 100 em 9:16 com a câmera do laboratório,
  gravando a pose da câmera; (2) Chrome sem tela renderiza `Tools/rt6t/proto-shapes.html` — o código de forma, a rampa
  e as luzes do protótipo (three.js 0.160 da CDN), com um parâmetro a mais para tirar os olhos — das mesmas poses
  espelhadas; (3) `compose_shapes.py` monta `docs/art/rt6t/formas-closeup.png` e `formas-multidao.png`. Na multidão do
  protótipo as unidades ficam paradas na vaga (a página não anima); no Unity estão pulando.
- **T-RT6t2-7 — Sem gate no OPPO.** O aceite diz "sem gate (dentro do teto da 6t.1)": a 6t.1 passou com 474 tri por
  unidade; as formas finais têm no máximo 492 (+4%), com a mesma única chamada por multidão, e a GPU do caso mais pesado
  usou 5,5 ms de 11.

## 2026-11-21 — T-RT6t4-1..11 — A multidão instanciada na corrida — técnica (sessão RT-6t.4)

- **T-RT6t4-1 — `RunCrowd` (jogo) dirige o `SwarmRenderer` (incubadora).** O exército da corrida é o `Swarm`: nenhum
  objeto por unidade, nenhum líder. A contagem do `Swarm` **é o exército** (antes era exército + líder): `SetArmy`,
  `Army`, `MaximumArmy` (= capacidade), evento `ArmyChanged` (HUD). O **centro da formação** faz o papel do líder: é a
  posição que cruza portões e caixotes (`RunGateCourse.Track`, mesmo lado da pista pelo X) e que escolhe a faixa no
  assalto. O `RunCrowd` avança o objeto do `Swarm` em Z pela velocidade da corrida (`ClaybreakLevel.RunSpeed`, a mesma de
  antes), lê o arraste e roda antes do `Swarm` no quadro (`DefaultExecutionOrder(-50)`), sem alocar nada.
  `PlayerController`, `RunnerCrowd`, `CameraFollow` e o colisor da pista saíram da cena (o validador recusa a volta).
- **T-RT6t4-2 — Regras e save sem mudança.** `GateMath.Apply(portão, exército, teto)` igual; caixotes somam
  especiais até o teto; exército 0 num portão perde a fase; continuar devolve `ContinueArmy` (até o teto); estrelas e
  moedas pelo mesmo `armyAtLine`/`armyLeft`. O teto do exército continua 300 (antes: 300 seguidores do pool; agora:
  capacidade do `Swarm`, **D27 sem resposta = 300**, `ClaybreakSceneBuilder.CrowdCapacity`). Save v3 intocado: a cor da
  skin equipada vira a **tinta** do `Swarm` (`RunCrowd.Wear`), o save só guarda o id como antes.
- **T-RT6t4-3 — Forma por dado (D25 aberta).** `ClaybreakTheme.crowdShape` (malha) aponta para uma das formas geradas;
  o construtor só preenche quando está vazio (**Bolinha com olhos**, provisória) e depois respeita o asset. Trocar para
  a Bolinha sem olhos ou para o Pintinho = trocar essa referência no `ClaybreakTheme.asset` (o fluxo aplica
  `SwarmRenderer.SetUnitMesh` na partida; o construtor também põe a mesma malha no `Swarm` da cena). O validador exige
  que seja uma das quatro formas e a mesma do `Swarm`.
- **T-RT6t4-4 — O assalto fica como está (6t.7).** As unidades em voo continuam vindo do `RunnerPool` do kit (300
  objetos), lançadas **da posição da unidade do anel de fora** da multidão parada na linha de tiro
  (`RunCrowd.TryTakeOuterUnit`), na ordem de remoção do `Swarm`. O kit "aterra" as unidades do pool contra um
  `CharacterController` do tamanho do antigo líder; sem líder, a cena tem um `UnitGrounding` com esse controlador
  **desligado** (só dá o tamanho; nenhuma física). Some junto com o pool na 6t.7. A escala das unidades do assalto
  continua 0,5.
- **T-RT6t4-5 — Assets da multidão compartilhados.** O material da multidão, o da sombra-bolha e o disco da sombra
  foram **movidos** (`git mv`, GUID mantido) de `Lab/Generated` para `Content/Generated/Units/` (`CrowdToon.mat`,
  `CrowdShadow.mat`, `BlobShadowDisc.asset`), ao lado das formas: a `ToonLab` e a corrida usam os mesmos. O jogo tem os
  seus dados próprios, criados uma vez com os valores do protótipo e depois deixados para ajuste à mão:
  `Content/Data/SwarmCameraRun.asset` (perfil do rig) e `Content/Data/CrowdToonLook.asset` (rampa/ambiente, aplicado
  pelo `ToonLookApplier` no sol). O cenário e as torres seguem no Simple Lit (6t.8/6t.10); a neblina do jogo continua a
  de cada mundo (sem o ajuste de neblina da `ToonLab`).
- **T-RT6t4-6 — Arraste.** Fórmula do protótipo (`alvo no toque + Δx/largura · M`, presa a meia pista − 0,5 m) com
  M = 16 × 4/6 ≈ 10,7 m por largura de tela, a mesma sensação do protótipo (16 m numa pista de 6 m de meia largura)
  na pista de 8 m do jogo. Arrasta na corrida e no assalto; não arrasta no menu, no resultado nem na oferta de
  continuar (antes o líder ainda andava de lado atrás da tela de continuar).
- **T-RT6t4-7 — Portão ruim = remoção simples + tremor.** O anel de fora some na hora e a câmera treme 0,35 s
  (`SwarmCameraRig.Shake`); sem respingo de massinha na corrida. A morte balística é da 6t.5. O tropeço do líder deixou
  de existir (não há líder).
- **T-RT6t4-8 — Gate da corrida.** `--ez stress true --ez run true --ei world N` (e `ClaybreakFlow.EditorUsesDeviceGateRun`
  nos testes) roda **a corrida** do nível de gate do mundo N: o exército no teto (300) correndo na pista com o cenário
  do mundo, a torre e as defesas à frente, voltando ao começo 10 m antes da torre, sem nunca entrar no assalto (o nível
  de gate não tem portões de corrida — o validador agora exige isso — então a contagem não muda). `Tools/rt2-gate.sh
  <rótulo> 60 20 <apk> run:N`. Critério igual ao da `ToonLab` (quadro p95 ≤ 16,7 ms a 60 fps, CPU e GPU p95 ≤ 11 ms,
  < 5% acima de 50 ms, PSS ≤ 450 MB), no mundo 4 (o mais pesado nos gates anteriores). O gate do assalto continua o
  mesmo (`world N`).
- **T-RT6t4-9 — Validação separada:** `ClaybreakLevel/World/Catalog.TryValidate(tetoDaMultidão, capacidadeDeVoo)`:
  `startCount` ≤ teto da multidão (antes ≤ pool + 1), `maximumUnitsInFlight` ≤ pool. O validador do jogo confere o
  `Swarm` da cena (capacidade = teto, pista centrada com meia largura 4 m, forma do tema, materiais da multidão, rig com o
  perfil da corrida, `ToonLookApplier` com o look do jogo) e que só o pool e o `UnitGrounding` desligado têm colisores.
- **T-RT6t4-10 — MSAA e HDR do jogo sem mudança (MSAA desligado, HDR ligado).** A T-RT6t1-3 deixou a escolha para cá: o
  gate da 6t.1 mostrou que nenhuma das quatro combinações chega perto do teto e que HDR não custa nada no OPPO; MSAA 4×
  custa ~1 ms de GPU com 1000. Sem motivo de desempenho para mudar, fica como está; a decisão visual de serrilhado volta
  com as paletas e o cenário (6t.6/6t.8), olhando capturas do jogo inteiro.
- **T-RT6t4-11 (2026-11-22) — A sonda de medição é do jogo.** O `--ei fps N` (destrava o fps, `FrameRateGovernor`
  controlado de fora) e a sonda `DevFrameTimingProbe` (linhas `[DevTiming]`) eram ligados pelo `RunnerCrowd` do kit;
  sem ele, o `ClaybreakFlow.ApplyMeasurementExtras` faz o mesmo (só desenvolvimento/medição Android; `// load-time:`).
  Achado na primeira rodada do gate, que saiu sem CPU/GPU.

## 2026-11-22 — T-RT6t5-1..9 — Animações procedurais da multidão toon — técnica (sessão RT-6t.5)

- **T-RT6t5-1 — Fórmulas puras, testadas contra o protótipo traduzido ao pé da letra.** `SwarmAnimation` (inclinação
  `clamp(−vx·0,06, ±0,5)`, nascimento elástico `1 − 2^(−10a)·cos(3,3πa)` com `a = idade/0,45`, sombra `(1 − 0,35h)·s`,
  composição `T·Rx(inclinação à frente)·Rz(inclinação lateral)·S`) e `SwarmDying` (lançamento, passo balístico, escala,
  giro, alcance) são estáticas e Burst. Os testes EditMode reescrevem as expressões do `crowd-proto.html` (three.js) e
  comparam valor a valor e quadro a quadro; a matriz é conferida contra `Matrix4x4.TRS` com o Euler do three espelhado.
  Convenção: a frente aqui é +Z (no protótipo −Z), então a inclinação à frente e o giro em X trocam de sinal; os offsets
  do girassol não foram espelhados na 6t.3, e o empurrão da morte usa o offset da vaga, como o protótipo.
- **T-RT6t5-2 — Morte idêntica ao protótipo, inclusive no que ele tem de "por quadro".** Passo de Euler (gravidade 22
  primeiro, depois o movimento; altura nunca abaixo do chão), escala `max(0, 1 − idade/1,1)`, expira quando `idade ≥ 1,1`
  antes do passo, giro `(r·t, 0, 0,7·r·t)` pelo relógio (não pela idade), sorteios na mesma ordem (lado, cima, frente,
  giro). Ficar igual ao protótipo aprovado vale mais que independência da taxa de quadros (a 60 fps a diferença some).
- **T-RT6t5-3 — Anel de capacidade fixa.** `SwarmSimulation` tem um `NativeArray<SwarmDyingUnit>` alocado na carga
  (`SwarmRenderer.dyingCapacity`, 512 por padrão); entram em ordem de vaga (como o `push` do protótipo), cheio = sai o
  mais antigo (o `splice` dele); como todos vivem 1,1 s, os vencidos estão sempre na cabeça. Um `IJob` Burst encadeado
  depois da animação escreve os morrendo logo após os vivos no mesmo buffer: **1 chamada para vivos + morrendo**,
  sombras só para os vivos (como o protótipo). Nada de `Instantiate`/`Destroy`.
- **T-RT6t5-4 — Quem morre voa junto com a multidão.** A posição dos morrendo é guardada relativa ao centro (Z), como no
  protótipo, em que o mundo é que anda; sem isso, na corrida eles ficariam para trás a 8–11 m/s.
- **T-RT6t5-5 — A sombra lê a escala pronta.** `tint.w` da instância passou de "quique" para "escala da sombra" (já com o
  nascimento); o `StudioKit/BlobShadowInstanced` perdeu o `_LiftShrink` (o encolhimento virou dado em
  `SwarmAnimationSettings.ShadowShrink`). Morrendo = 0 (sem sombra).
- **T-RT6t5-6 — Remoção explícita.** `SwarmRemoval.Vanish` (recomeço, unidade tirada para o assalto) e `Die` (portão
  ruim; na `ToonLab`, baixar a contagem, como o "Recomeçar" do protótipo). Remove sempre o anel de fora (maior índice do
  girassol). Nascimentos sempre com o "pop" (início, caixote, continuar), como o protótipo. `ResetRun`/`RestartRun`
  limpam o anel (`ClearDying`). A cor de quem morre é a da hora da morte.
- **T-RT6t5-7 — Gate "1000 vivos + 512 morrendo" na `ToonLab`.** A corrida tem teto 300 (D27), então o caso pedido roda no
  laboratório: modo de rotação (`--ei dying D`, `Tools/rt2-gate.sh ... toonlab:1000:4:1:512`) que joga o anel de fora
  e o faz nascer de novo a 1,15·D/1,1 s por segundo, mantendo 1000 vivos (com nascimentos animados) e o anel cheio. O
  log `[DevFps]` ganhou `dying=D`. MSAA 4× e HDR ligado (o caso mais pesado da 6t.1).
- **T-RT6t5-8 — `SwarmRenderer.DrawBodies`.** Desligado desenha só as sombras: ferramenta para a captura do chão sob a
  multidão (R-T7c), sem afetar o jogo.
- **T-RT6t5-9 — Bounds da multidão crescem com os morrendo** pelo alcance balístico (`SwarmDying.Reach`), para o
  descarte por câmera nunca cortar quem voa.
- **T-RT6t5-10 — Correções da revisão independente (subagente fable, só leitura; nenhum bug sério).** (a) O desenho usa
  as contagens **escritas pelo último quadro** (`DrawCount`, `WrittenCount`), não as atuais: contagem mudada depois do
  agendamento (painel, rotação da `ToonLab`, testes) aparece no quadro seguinte, sem instância velha por um quadro.
  (b) Os limites de desenho cobrem a caixa de onde cada morrendo saiu (não só o raio da formação nova): de 1000 para 10,
  quem estava no anel de fora (até ~10 m) continua dentro. (c) Recomeço sem "pop": `ResetCount`/`SwarmSimulation.Reset`
  põem a multidão inteira na hora (`ResetRun`/`RestartRun` da corrida e o laço do gate). (d) A rotação da `ToonLab` joga
  unidades que ainda não apareceram (renascem e morrem antes do atraso do pop); a carga medida é a mesma (N + D
  instâncias), só o visual da rotação difere — aceito, é modo de medição. (e) O teste do nascimento confere o valor do
  protótipo pelo tempo de nascimento de cada unidade (`BirthTime`).

## 2026-11-22 — Divisão da RT-6t.6 em 6t.6a e 6t.6b — técnica (maestro, escopo)

A RT-6t.6 juntava duas partes de natureza diferente: paletas, céu e neblina (só dado e desenho, sem decisão aberta) e
formas e skins no menu com save v4 (depende da **D26**, e da **D25** para a variante de forma, ainda sem resposta).
Para a série não parar esperando o usuário, o maestro dividiu: **RT-6t.6a** = `ToonPalette` com cores de torre/defesa,
aplicador, céu degradê, neblina, as 4 paletas, `WorldLook` → paleta, a regra "skin padrão (BARRO) = cor do mundo" sem
mudar save nem loja, aviso de contraste no validador e capturas dos 4 mundos; **RT-6t.6b** = seletor de forma no menu,
save v4 com o campo "forma" e migração v3 → v4, skins conforme a D26, revisão de segurança do save. A 6t.7 e a 6t.8 só
precisam das paletas (tinta da multidão e cores do cenário) e passam a depender da 6t.6a; a 6t.6b não trava nenhuma
outra entrega e entra quando a D26 for respondida. Plano, tabela da direção visual e linha da D26 atualizados.

## 2026-11-22 — T-RT6t6a-1..9 — Paletas, céu e neblina — técnica (sessão RT-6t.6a)

- **T-RT6t6a-1 — "BARRO = cor do mundo" agora não antecipa a D26.** As três opções da ficha (A, B e C) fazem a skin
  padrão usar a cor da paleta do mundo; o que a D26 decide (formas no menu, cores e nomes das outras 8 skins, save v4)
  ficou para a 6t.6b. Regra em `Progression.CrowdColorIn(mundo)`: skin padrão (a primeira do catálogo, grátis; id
  desconhecido cai nela) → `paleta.Crowd`; qualquer outra skin → a cor dela em todos os mundos. O save guarda o mesmo id
  de skin; a loja não mudou (o quadradinho do BARRO ainda mostra a cor de argila — a 6t.6b troca junto com as skins). A
  mesma cor vai para a corrida (tinta do `Swarm`), para os bonecos do assalto e para os especiais; os bonecos do pool só
  são repintados quando a cor muda (menu e início de nível, nunca em jogo).
- **T-RT6t6a-2 — `ToonPalette` no kit (incubadora), conteúdo no jogo.** O ScriptableObject tem céu topo/base, neblina
  + início/fim, chão, pista, faixa, multidão, folhagem 2 tons, tronco, pedra, destaque e tinta de UI e torre clara/escura,
  detalhe de torre e defesa (para a 6t.10); todas opacas, validadas. Não nomeia jogo nem tema: as cores de torre se chamam `structureLight/Dark/Detail` no kit ("tower" é termo
  proibido do isolamento; o conteúdo do jogo as preenche como cores de torre). As 4 paletas são código
  do jogo (`Editor/PaletteContent.cs`), regravadas a cada construção da cena como o resto do conteúdo; o validador
  confere asset a asset os valores exatos e um teste EditMode guarda a tabela aprovada (seções 1.1 e 5.1 da direção).
  "Trocar a Aurora é editar um arquivo" continua valendo: é esse arquivo.
- **T-RT6t6a-3 — Cores de torre/defesa provisórias [P].** O usuário não deu esses valores; escolhidos a partir de cada
  paleta (claro/escuro do céu e da pedra, detalhe = destaque de UI). Ainda não aparecem na tela; a 6t.10 calibra por
  captura.
- **T-RT6t6a-4 — O horizonte tem uma fonte só.** `WorldLook.sky` saiu: a cor de neblina e o céu vêm da paleta
  (`WorldLook.palette`). Sol e ambiente continuam no `WorldLook` porque a arte antiga (cenário e torres em Simple Lit)
  ainda os lê até a 6t.8/6t.10. As fichas de opções da RT-6 (`docs/art/rt6`) e da RT-4 (`docs/art/rt4`) ficam como registro das
  decisões D17/D20 (a captura as regrava com a paleta nova; não entram no commit, como nas entregas anteriores).
- **T-RT6t6a-5 — Céu como skybox em espaço de tela.** Shader `StudioKit/SkyGradientScreen`: y de tela calculado do
  clip-space por pixel (com `_ProjectionParams.x`, que desfaz a inversão ao desenhar em textura), mistura em sRGB como o
  gradiente de canvas do protótipo e conversão para linear no fim; cores como globais brutas (`SetGlobalVector`, sem a
  conversão de `SetGlobalColor`). Sem neblina no céu (como o fundo do three). 1 desenho de tela cheia, sem textura. A
  câmera do jogo limpa para o skybox.
- **T-RT6t6a-6 — Aplicador.** `ToonPaletteApplier` (componente na raiz do cenário) escreve as globais do céu, o skybox e a
  neblina linear do `RenderSettings`; o `WorldScenery` chama `Apply` na troca de mundo/nível. Um `ApplyGlobally` estático
  serve às capturas de modo de edição. Só as globais que algum shader lê hoje (céu); pista, faixa etc. entram como
  globais quando a 6t.8 tiver os shaders.
- **T-RT6t6a-7 — Sombra fora da paleta.** O item 17 citava "sombra" entre os campos, mas a seção 1.1 diz que a sombra
  (#2b2233 a 0,18) não varia por paleta: continua no material da sombra-bolha. Campo sem uso não entrou.
- **T-RT6t6a-8 — Aviso de contraste (não reprova).** `ToonContrast` (kit): razão WCAG e ΔE CIE76 em CIELAB. O validador
  avisa quando a multidão fica a ΔE < 20 da pista, do chão ou das cores de portão da direção (#5ad1a0 / #ff6b81, que a
  6t.9 põe nos portões), e quando a tinta de UI sobre o destaque fica abaixo de 3:1 (texto grande). Com os valores do
  usuário saem 2 avisos, registrados em `rt6t-crowd-toon.md`, sem mudar cor (valores exatos aprovados).
- **T-RT6t6a-10 — Correções da revisão independente (subagente fable, só leitura).** (a) Bloqueio real: o kit citava
  "tower" (termo proibido do isolamento) — renomeado para "structure"; o passo `isolation` passou. (b) `Validate` não
  aloca mais (campo a campo). (c) As capturas de modo de edição reaplicam a paleta depois de abrir cada cena nova (o
  Unity descarrega a paleta carregada antes, e as globais do céu ficavam da foto anterior). Aceitos sem mudança:
  testes de captura que vazam textura só se falharem no meio (mesmo padrão das outras capturas). Observação de
  desenho, para a 6t.10: com a neblina 22–75 a torre distante fica lavada na cor da neblina, na corrida e no assalto
  (no Pastel quase some contra o céu) — a 6t.10 decide entre neblina por perfil de câmera, `fogEnd` maior por paleta
  ou cores de estrutura mais escuras.
- **T-RT6t6a-9 — Neblina 22–75 já vale para tudo; sem gate.** A neblina do protótipo cobre também o cenário antigo:
  silhuetas distantes somem até a 6t.8 trocar o cenário. Sem gate no OPPO: o aceite não pede, e o custo novo é 1 desenho
  de céu de tela cheia no lugar da limpeza em cor sólida (a 6t.12 faz o gate final de tudo).

## 2026-11-22 — T-RT6t8-1..11 — Cenário instanciado e pista listrada — técnica (sessão RT-6t.8)

- **T-RT6t8-1 — Cores do cenário por "slot", uma malha para os 4 mundos.** Variante nova do `StudioKit/ToonLit`
  (palavra-chave local `STUDIOKIT_TOON_SLOTS`): o RGB da cor de vértice deixa de ser cor e vira peso de três cores
  (`_ToonSlot0..2`, lineares). Árvore = tronco (R), folhagem escura (G), folhagem clara (B); pedra = pedra (R), folhagem
  clara (G, o "musgo" do protótipo); chão + trilhos = chão (R), faixa (G). O `WorldScenery.Apply` escreve as cores da
  paleta em blocos de propriedade (nada de material por mundo; trocar de mundo = poucas escritas). Descartado: assar a
  paleta em 8 malhas por mundo (troca de paleta exigiria reconstruir a cena).
- **T-RT6t8-2 — Reciclagem sem estado (`PropScatter`).** Cada objeto tem uma fase fixa ao longo da janela; a posição é
  função da âncora (câmera): a volta em que ele cai na janela sorteia lado, distância, escala (0,7–1,5) e giro por hash
  de (semente, camada, índice, volta). Igual à reciclagem do protótipo ("volta para o fundo em outro lugar"), mas
  determinística e válida também quando a câmera volta ao início (recomeço, gate que repete a corrida). Recalcula só nos
  quadros em que a câmera andou; sem alocação por quadro; testada sem GPU (`PropScatterTests`).
- **T-RT6t8-3 — Janela de 90 m em vez de 125.** 4 m atrás da câmera e 86 à frente: a neblina termina a 75 m, o resto do
  protótipo era invisível. Densidade igual à do protótipo: 90 árvores e 40 pedras em 125 m → 65 e 29 em 90 m.
- **T-RT6t8-4 — Densidade por mundo [P].** `WorldLook.trees/rocks` (substitui o `SceneryStyle`): Pastel 65/29 (a do
  protótipo), Pôr do sol 52/36, Doce 78/22, Aurora 44/44. A cena pré-aloca a maior de cada tipo; o validador confere.
- **T-RT6t8-5 — Faixa dos objetos 3,5 m depois da borda da pista** (o protótipo usa 2,5 m numa pista de 12 m; a nossa tem
  8 m e as torres chegam a 5,7 m do centro): de 7,5 m a 29,5 m do centro; a árvore mais larga (raio 1,5) fica a 6 m.
- **T-RT6t8-6 — Pista `StudioKit/StripedTrack` sem luz, como o protótipo** (lá é `MeshBasicMaterial`): listras pelo Z do
  mundo (`frac(z/4) < 0,5`, 2 m de cada cor, igual à textura repetida do protótipo), borda suavizada por `fwidth` (sem
  cintilar ao longe), neblina linear. Como a multidão anda para a frente, as listras "rolam" sozinhas, sem offset por
  quadro. A pista continua com 8 m e topo em y = 0 (portões e assalto leem os limites dela).
- **T-RT6t8-7 — Orçamento de desenho.** Cenário lateral = 3 chamadas: chão + 2 trilhos numa malha estática (1),
  árvores (1), pedras (1). A pista é 1 a mais (chão de jogo, não lateral). O validador reprova acima de 3 e se o cenário
  tiver qualquer outro `Renderer`.
- **T-RT6t8-8 — Retirado o cenário da RT-6.** `WorldSceneryArt`, `SceneryStyle`, as cores antigas do `WorldLook`
  (ground/field/prop/propDark/accent), os materiais `Materials/world1..4`, `Ground`/`Field` do `ArtPalette` e as
  silhuetas distantes (a neblina 22–75 já as apagava). Sol e ambiente continuam no `WorldLook` (a luz principal do
  toon). As `LookOptions` da RT-6 ficam como registro da D20 (agora só diferem em sol/ambiente).
- **T-RT6t8-9 — Pedra do protótipo com icosaedro subdividido.** `LowPolyMeshBuilder.AddIcosahedron(…, detail)` divide
  cada aresta em detail + 1 (como `IcosahedronGeometry(r, detail)` do three); a pedra usa detail 1 (160 triângulos);
  a árvore tem 50.
- **T-RT6t8-10 — Capturas de modo de edição desenham o cenário.** Sem `LateUpdate` no modo de edição, o
  `ArtDirectionCapture` chama `InstancedProps.Render(câmera)` antes de cada render manual e `Release` depois.
- **T-RT6t8-11 — Revisão independente (subagente fable, só leitura): nada sério no caminho de jogo.** Aplicados: o
  validador agora exige que a neblina de cada paleta termine antes do fundo da janela dos objetos (senão eles
  "apareceriam" do nada); teste novo do `InstancedProps` (limite da capacidade, uma chamada por camada mostrada,
  posição = função pura) e teste de giro/escala no lugar de um teste que não podia falhar; conversão de cor para o
  shader num lugar só (`ToonShaderIds.ShaderColor`); malha de chão das capturas marcada `DontSave`; `Release` das
  capturas em `finally`. Aceitos: pista e chão guardam a cor em bloco de propriedade (não salvo na cena; o fluxo aplica
  o mundo antes de qualquer tela); a pista só tem passo de forward, como o `ToonLit` (sem pré-passo de profundidade no
  URP do projeto).

## 2026-11-22 — D25 — Visual "fofo" (crowd toon) e público na Play — **A** — usuário

Aprovado: adotar o visual do protótipo como está (bolinhas e pintinhos com olhos, cores pastel) e continuar
declarando na Play o público de 13 anos para cima (13–15, 16–17, 18+), sem faixas infantis; responder "sim" se a Play
perguntar se a ficha pode atrair crianças sem querer, para receber a marca "Não é destinado a crianças"; compensar na
loja (ícone e capturas mostrando o desafio, sem rostos em destaque no ícone, textos sem palavras infantis). A regra do
GDD "sem bichinhos fofos / nada de olhos" deixa de valer para a multidão; continua valendo para o ícone, torres e
chefes (sem rosto). A variante "sem olhos" da RT-6t.2 fica como reserva. Na RT-9, conferir na Central de Ajuda da
Play como a pergunta é feita hoje (regras citadas de memória na ficha). Ficha: `direcao-visual-crowd.md` seção 5.

## 2026-11-22 — D26 — Skins × 2 formas × cor do mundo — **A** — usuário

Aprovado: (1) Bolinha e Pintinho livres para o jogador escolher no menu, sem custo, escolha gravada no save;
(2) a skin grátis padrão BARRO passa a ser "cor do mundo" (a multidão usa a cor da paleta de cada mundo); (3) as
outras 8 skins continuam sendo uma cor que vale em todos os mundos, mesmos preços e mesma ordem (D19), com cores e
nomes novos no estilo, conferidos contra as 4 paletas e as cores dos portões; (4) elites com tamanho e cor próprios.
Destrava a RT-6t.6b. Ficha: `direcao-visual-crowd.md` seção 5.

## 2026-11-22 — T-RT6t6b-1..9 — Formas e skins no menu, save v4 — técnica (sessão RT-6t.6b)

- **T-RT6t6b-1 — As formas são dado do catálogo (`ClaybreakCatalog.Shapes`, `CrowdShapeOption` id/título/malha), não do
  tema.** O catálogo já é "tudo o que o meta oferece" (skins com cor); o save guarda o id e é normalizado contra ele, como
  skins e elites. O `ClaybreakTheme.crowdShape` saiu (uma fonte só). O catálogo valida: pelo menos 1 forma, ids únicos no
  padrão de save, malha com 1 submalha. O validador exige que toda forma oferecida seja uma das geradas **com olhos**
  (D25 A); as sem olhos continuam geradas como reserva.
- **T-RT6t6b-2 — Ids `ball` e `chick`, títulos BALL e CHICK [P].** A interface do jogo é em inglês (PLAY, SHOP); os nomes
  são dado (`WorldContent.Shapes`), trocar é uma linha. `ball` é o padrão e a forma que todo save antigo já mostrava.
- **T-RT6t6b-3 — Save v4 = v3 + `shape`.** `SaveMigration.AddShape` dá `ball` (`ShapeBeforeV4`, constante do formato) a
  saves v1, v2 e v3; v4 lê direto. Id desconhecido, vazio ou nulo cai na primeira forma do catálogo na normalização; o
  `SaveValidator` reprova forma fora do catálogo. O backup do JSON original por versão (`Claybreak.Save.backup.v3`) já
  existia e vale para a migração nova. Fixture nova `v4-chick-bubblegum.json`; os `v1`–`v3` não foram tocados.
- **T-RT6t6b-4 — Seletor = um botão na tela inicial ("SHAPE: BALL"), sob a linha de elites**, que passa para a próxima
  forma do catálogo (escala para N formas sem mudar a tela). Grátis (D26 A), grava o save na hora e troca a malha da
  multidão que espera atrás do menu (troca de referência, nada criado). Fora do menu e no nível de gate não faz nada.
- **T-RT6t6b-5 — A corrida usa a forma do save** (`progression.Shape.mesh` no `Configure` e a cada troca). A cena começa
  com a forma padrão; o validador confere isso. O assalto continua com os bonecos antigos até a 6t.7.
- **T-RT6t6b-6 — O gate veste a forma de mais triângulos** (`Progression.MaximumLoadout`, pela contagem de índices: hoje o
  Pintinho, 492 contra 484), junto com a skin mais cara — o pior caso da corrida.
- **T-RT6t6b-7 — Skins (D26 A): mesmos ids, ordem e preços; cores e nomes novos.** Ids são formato de save e ficam
  (`ember`, `moss`… — nunca renomear id publicado). CORAL #ff8a5c 150, MANGO #ffc94a 300, SKY #6fa8ff 500, LILAC #b98cf0
  800, INDIGO #3d3a7c 1200, LAGOON #2fb3c4 1500, BERRY #e0569b 2000, BUBBLEGUM #ff9ed2 2500 (total 19.380 igual). Escolhidas
  por ΔE CIE76 ≥ 20 contra pista, faixa e chão das 4 paletas, contra os 2 portões de referência e entre si (menor: 22,5,
  INDIGO × chão do Pôr do sol; entre skins 26,0). A grátis (`clay`) passa a se chamar WORLD COLOR, mostra na loja a cor do
  mundo atual e diz "Each world's own color".
- **T-RT6t6b-8 — Aviso de contraste das skins no validador** (`ClaybreakValidation.Warnings`): cada skin paga contra pista,
  faixa e chão de cada mundo, os 2 portões e as outras skins; só avisa (como a 6t.6a). Com as cores escolhidas: nenhum aviso
  de skin.
- **T-RT6t6b-9 — Ordem da construção:** `ToonLabBuilder.Build` (gera as malhas das formas) passou para antes de
  `WorldContent.Write`, que agora aponta o catálogo para elas.
- **T-RT6t6b-10 — Revisão de segurança do save (subagente fable, só leitura): nada sério.** Save hostil não derruba o
  jogo, não corrompe estado nem dá item pago (o `shape` é limitado pelo teto de 64 KB, nulo vira vazio, desconhecido vira o
  padrão; formas são grátis); todos os caminhos v1–v4 terminam válidos; nada de `Instantiate`/`Destroy` no código de jogo.
  Aplicados: o validador reprova catálogo sem a forma `ball` (para onde migram os saves antigos); o botão some com uma
  forma só; teste do gate com a forma mais pesada em primeiro (pega "a última" por engano); comentário antigo corrigido.
  Aceitos: (a) o salto para v4 faz um APK anterior recusar o save novo (comportamento antigo, sem jogadores reais e a
  entrega pede v4); (b) cada toque no botão grava o save na hora (como loja e mundos); (c) capturas usam `AssetDatabase`
  sem `#if UNITY_EDITOR`, igual às capturas existentes (só rodam no Editor).

## 2026-11-22 — T-RT6t7-1..10 — Multidão instanciada no assalto — técnica (sessão RT-6t.7)

- **T-RT6t7-1 — `LooseSwarmRenderer` (incubadora, `Swarm`): unidades soltas, cada uma posta pelo dono.** As unidades em voo
  não seguem formação, então não cabem no `Swarm` (girassol); o renderizador novo guarda posição, velocidade lateral,
  tamanho, tinta e fase por unidade em `NativeArray` fixos, anima com o mesmo pulo/agachamento/inclinação do `Swarm`
  (um job Burst) e desenha com a mesma malha, o mesmo `ToonLit` e a mesma sombra-bolha: **1 chamada para os corpos (vivos +
  morrendo) e 1 para as sombras**. Remoção = a última unidade ocupa o índice livre, igual ao `AssaultField`, que mantém
  seus arrays de regra (x, z, faixa, armadura, tipo, velocidade) com o índice i = unidade i do renderizador.
- **T-RT6t7-2 — Anel de morte extraído (`SwarmDyingRing`)** do `SwarmSimulation` (comportamento igual; testes da 6t.5
  verdes) e usado pelos dois renderizadores. `SwarmDyingUnit.Size` (1 = malha) e `SwarmDying.Launch` com direção qualquer e
  tamanho, para unidades fora de formação; uma morte pode nascer acima do chão (bloco da torre).
- **T-RT6t7-3 — Regras sem mudança.** Mesma ordem portão → varredor → muro → escudo → torre, mesmos danos, frações,
  armadura, clones, reciclagem do nível de gate, `PeakInFlight`/`TowerHits` (conferido pelo revisor contra o `HEAD`).
  Só o visual mudou: quem cai no portão ÷ (0 saem), no varredor, no muro, no escudo e no canhão (jogado para longe do
  impacto) **morre pelo anel**; quem chega à torre some (a torre reage). O respingo de massinha do kit
  (`RunnerDeathEffects`) saiu do jogo: o andar da torre que cai joga 6 bonecos da cor da multidão por bloco, da altura do
  bloco. O tema ainda guarda `roster`/`deathEffect` (limpeza na 6t.12).
- **T-RT6t7-4 — Nível de gate:** a unidade reciclada deixa uma cópia morrendo no lugar, para o gate medir também as mortes
  (antes ele pagava o respingo).
- **T-RT6t7-5 — Elites por instância:** tamanho = `UnitType.scale` sobre a malha da multidão (Bruto 1,45, Corredor 0,9,
  Guardião 1,25) e cor do tipo; os comuns e os clones vestem a cor da skin no lançamento. O boneco em voo tem o tamanho
  do boneco da corrida (antes, o boneco de massinha era 0,5 do líder).
- **T-RT6t7-6 — Câmera do assalto = `SwarmCameraShot` (dado) que o `SwarmCameraRig` segura** (`HoldShot`/`ReleaseShot`):
  `Content/Data/SwarmCameraAssault.asset` (0, 17, −11) e `SwarmCameraAssaultBoss.asset` (0, 20, −14), foco 55% até a torre,
  60°, transição suave de 0,8 s no tempo real — o enquadramento do antigo `AssaultCamera`, que foi removido. Um tremor
  da corrida não passa para o plano do assalto.
- **T-RT6t7-7 — A forma escolhida no menu vale no assalto** (`ClaybreakFlow.UseShape` troca a malha da corrida e das
  unidades em voo; o gate usa a mais pesada).
- **T-RT6t7-8 — Pool do kit fora da cena:** saíram o `RunnerPool` com 300 `RunnerUnit`, o `UnitGrounding` e o
  `Generated/RunnerUnit.prefab`. `PoolSize` virou `FlightCapacity` = 300 (**D27 sem resposta: 300**, fases e economia
  iguais) e o anel do assalto tem 512. O validador reprova qualquer collider, `RunnerPool`/`RunnerUnit` na cena, e confere
  capacidades, malha padrão, materiais, sombra e os dois planos de câmera.
- **T-RT6t7-9 — Caixa de desenho por fase:** da linha de tiro − 8 m até a torre + 3 m, largura da pista + 2 m, 3 m de
  altura, crescida pelo alcance dos que morrem.
- **T-RT6t7-10 — Revisão independente (subagente fable, só leitura): nada sério.** Aplicados: transição da câmera em
  tempo real (não no passo limitado), tremor zerado ao entrar no plano, proteção contra renderizador desligado no meio
  (limpa o campo em vez de dessincronizar), morte do bloco da torre na altura do bloco, um assert que não podia falhar.

## 2026-11-22 — T-RT6t9-1..11 — Portões, Baloo 2 e feedback — técnica (sessão RT-6t.9)

- **T-RT6t9-1 — Fonte: Baloo 2 ExtraBold estática, derivada da variável oficial.** O `github.com/google/fonts`
  (`ofl/baloo2`) só tem `Baloo2[wght].ttf` (400–800). O Unity e o TextMeshPro leem a instância padrão (400) de uma fonte
  variável, e o visual pede 800; `Tools/fonts/baloo2-extrabold.py` confere os hashes da origem (commit fixado) e fixa o
  peso em 800 com o `instancer` do fontTools, de forma reprodutível (mesmo hash a cada rodada). É uma "versão modificada"
  permitida pela OFL 1.1; sem "Reserved Font Name", o nome continua. Só um peso no jogo (a UI antiga também era um peso só,
  Roboto Black). Licença e hashes em `audio-licenses.md` seção 2; a Roboto saiu.
- **T-RT6t9-2 — TextMeshPro com atlas estático.** `GameFonts` (Editor) gera `Baloo2-ExtraBold SDF.asset` (SDFAA, 64 pt,
  margem 8, atlas 1024², 124 caracteres: ASCII, `− × ÷ · …` e acentos do português) e o põe em modo **estático**: nada é
  rasterizado em jogo e a build não limpa o atlas. Só é refeito se faltar ou se faltar caractere. Os "TMP Essential
  Resources" foram importados do próprio pacote `com.unity.ugui` (sem download).
- **T-RT6t9-3 — Contorno por material (quatro presets no mesmo atlas):** portão bom (contorno #1f7a55), portão ruim
  (#a52a3a), número flutuante (contorno branco) e contador (#3b2f4a); shader `TextMeshPro/Mobile/Distance Field` com
  `OUTLINE_ON`. Trocar o lado do portão troca só `fontSharedMaterial` (sem instanciar material).
- **T-RT6t9-4 — Sinais verdadeiros.** `GateSpec.Label` passou de `x2`/`-30` para `×2`/`−30` (U+00D7, U+2212; `÷` já era).
  `GateSpec.WriteLabel(char[])` escreve o rótulo sem alocar e alimenta `TMP_Text.SetCharArray` nos portões da corrida, nos
  caixotes, na fileira do assalto e no número flutuante.
- **T-RT6t9-5 — Portões da corrida como o protótipo:** dois painéis com metade da pista menos 0,25 m (2,3 m de altura,
  #5ad1a0 / #ff6b81 a 0,45, Unlit transparente sem escrita de profundidade), rótulo de 0,85 m de "em" a 1,3 m, três postes
  brancos (r 0,14 m, 2,6 m) no `StudioKit/ToonLit` (malha de cilindro com cor de vértice branca e alfa 0 = sem tinta);
  saíram a verga e os postes de pedra. Fileira do assalto: mesmos materiais e postes brancos entre as faixas (saiu a
  barra de pedra). Pool de portões igual (5 pares, 4 caixotes, 3 faixas).
- **T-RT6t9-6 — Número flutuante:** `FloatingNumbers` (jogo) com 4 `TextMeshProUGUI` pré-alocados no HUD, usados em
  rodízio (o mais antigo é reaproveitado); curva do `@keyframes up` do protótipo (0%: escala 0,4 e transparente; 20%:
  sobe 20 px, escala 1,25; 100%: sobe 110 px, escala 1, some), cada trecho com saída suavizada, 1 px do protótipo =
  2,4 unidades do canvas (1920 de altura). Verde #2fbf86 / vermelho #ff4d6d. Só nos portões da corrida (a fileira do
  assalto age unidade a unidade).
- **T-RT6t9-7 — Pop do contador:** o contador da multidão virou `TextMeshProUGUI` (contorno) e sobe a 1,18 em 0,15 s e
  volta em 0,15 s (a transição do `.count.pop`). Um pop em andamento não reinicia (no assalto o número muda a cada
  poucos quadros). O `HudPulse` do kit não foi usado: ele não conhece TMP e o tween é de 10 linhas.
- **T-RT6t9-8 — Shake no portão ruim já estava ligado** desde a 6t.4/6t.5 (`ClaybreakFlow.OnGateCrossed` →
  `RunCrowd.Shake` → `SwarmCameraRig`); nada mudou.
- **T-RT6t9-9 — Fonte das telas:** `ClaybreakSceneBuilder.FontPath` aponta para a Baloo 2: toda `UI.Text` e os rótulos de
  vida (`TextMesh` das torres, muro e escudo) usam a TTF dinâmica, sem migrar telas para TMP (item 19b do plano).
- **T-RT6t9-10 — Testes:** EditMode — rótulos com os sinais verdadeiros e escrita sem alocação, curva do número e do pop,
  hashes da fonte e da licença, atlas estático com todos os glifos, presets com contorno no mesmo atlas. PlayMode — rótulos
  da corrida e do assalto em Baloo 2 com o preset do lado, número flutuante ao cruzar o portão (cor, texto, some em 1 s),
  pop do contador, nenhum objeto criado em jogo; captura com GPU `GateFeedbackTests` (`-gateCaptureDir`).
- **T-RT6t9-11 — Revisão independente (subagente fable, só leitura): nada sério.** Aplicados: o pop do contador é zerado
  ao esconder/mostrar o HUD (não fica congelado entre fases), o HUD confere as peças do número flutuante, operando
  negativo não escreve lixo no rótulo, o número some pelo `CanvasRenderer` (sem refazer a malha do TMP a cada quadro), a
  rampa das capturas de edição é liberada, e saíram duas sobras de código.

## 2026-11-22 — D28 sem resposta: vale A — produto (sessão RT-6t.10)

- **D28 — Arte do assalto no estilo novo: A aplicada por padrão** (ficha em `direcao-visual-crowd.md`, "Se você não
  responder: a 6t.10 aplica A"). Torres, chefes, canhão, muralha, escudo, varredor, projéteis, caixotes e efeitos ganham o
  sombreado de 3 faixas e as cores de torre/defesa de cada paleta, sem remodelar e sem rostos. Reversível; a aprovação
  visual do usuário fica como aceite pendente (capturas `docs/art/rt6t/chefes-oppo.png` e `chefes-perto.png`). Se ele
  responder B ou C depois, vira entrega própria.

## 2026-11-22 — T-RT6t10-1..8 — Arte do assalto no estilo toon — técnica (sessão RT-6t.10)

- **T-RT6t10-1 — Cores de estrutura como globais do `ToonLit`, não material por mundo.** As torres são reaproveitadas
  entre mundos (a Forja nos mundos 1 e 2, a torre do mundo 2 no 3, a do 3 no 4) e as defesas são as mesmas nos quatro:
  a cor tem de seguir a paleta do mundo ativo. O `StudioKit/ToonLit` ganhou `_StructureWeights` (por material: quanto
  de estrutura clara, escura, detalhe e defesa entra na tinta; o que falta até 1 fica branco) e quatro globais
  (`_StudioKitStructureLight/Dark/Detail`, `_StudioKitDefense`, lineares) que o `ToonPaletteApplier` escreve junto com o
  céu e a neblina. Trocar de mundo = 4 escritas globais; nada criado, nenhum bloco de propriedade (continua compatível
  com o SRP Batcher). Descartados: 4 conjuntos de materiais por mundo trocados em cada renderizador (listas, trocas a cada
  nível) e `MaterialPropertyBlock` por renderizador (tira do SRP Batcher). Com pesos 0, emissão 0 e neblina 1 o shader
  dá exatamente o resultado anterior: multidão, cenário e postes não mudam. Global nunca escrita (alfa 0, modo de edição
  antes de qualquer paleta) é lida como branco, não preto.
- **T-RT6t10-2 — Os papéis antigos viram misturas da paleta (sem remodelar).** `ArtPalette` mantém os nomes (Stone,
  Iron, Wood, Banner, Brick, Frost, Basalt...) para os construtores das torres não mudarem uma linha de forma; cada um
  vira uma mistura: pedra = clara/escura, ferro = defesa, estandarte = detalhe, madeira = escura + defesa, caixote =
  detalhe + escura etc. (tabela no código). Os temas dos mundos ficam nas formas, como a ficha diz.
- **T-RT6t10-3 — Efeitos: o mesmo shader com emissão.** Chamas, janelas acesas, brasas, lanternas e gelo passam de URP
  Unlit para `ToonLit` com `_ToonEmission` 1 e a cor de detalhe clareada (rosa no Pastel, coral no Pôr do sol, azul no
  Doce, laranja na Aurora). Sinais de jogo ficam fixos: barra de vida (vermelha), núcleo e cúpula do escudo (ciano), faixa
  de perigo, painéis dos portões. A sombra do projétil passa ao tom de sombra da direção (#2b2233) a 0,4 (mais forte que
  a da multidão, 0,18: ela avisa onde a pedra cai).
- **T-RT6t10-4 — Neblina das estruturas a 0,35.** A observação da 6t.6a (torre lavada na cor da neblina 22–75) se
  resolve com `_ToonFogAmount` por material: as estruturas levam 35% da neblina, o cenário e a pista seguem com 100%.
  Descartados: `fogEnd` maior por paleta (mexe na atmosfera da corrida aprovada) e neblina por perfil de câmera (mais
  peças para o mesmo efeito).
- **T-RT6t10-5 — Cores de torre do Pastel escurecidas [P].** Na primeira captura a torre do mundo 1 (#f6e9f2 / #c4a7c9)
  quase sumia contra o céu rosa; passou a #f1d2e1 / #ab86b8. As outras três paletas ficaram como a 6t.6a propôs. Todas
  continuam provisórias (o usuário não deu esses valores).
- **T-RT6t10-6 — Primitivas com cor de vértice.** O `ToonLit` lê a cor de vértice, e as primitivas do Unity não têm (um
  fluxo ausente não é garantidamente branco em toda API gráfica). `ToonPrimitives` gera cópias de cubo, cilindro e esfera
  com branco (alfa 1 = leva a tinta) em `Content/Generated`; o `Prim` usa a cópia quando o material é toon. Ao trocar o
  shader de um material antigo, o asset é sobrescrito por um material novo do mesmo shader (mesmo id), sem as
  propriedades do Simple Lit.
- **T-RT6t10-7 — Guardas.** Validador: todo renderizador opaco da torre, canhão, muralha, varredor, escudo, portões/caixotes
  e linha de tiro usa `ToonLit` com malha de cor de vértice (todas as posições de material). EditMode `AssaultArtTests`:
  os materiais do assalto commitados são toon, pesos em 0..1 com soma até 1, nenhum Simple Lit na pasta, primitivas
  brancas. PlayMode de captura `BossCaptureTests` (`-bossCaptureDir`, só com GPU): os 4 chefes no nível de gate de cada
  mundo, na câmera do jogo e de perto.
- **T-RT6t10-8 — Revisão independente (subagente fable, só leitura): nada sério.** Aplicados: o teste lê os assets
  commitados em vez de reconstruí-los, a troca de shader limpa as propriedades antigas, global não escrita vira branco,
  o validador cobre a linha de tiro e todas as posições de material e roda mesmo com peça faltando, a barra de vida
  voltou à cor exata de antes, a captura limpa câmera e texturas mesmo se falhar no meio.

## 2026-11-22 — RT-6t.10 — Aceite visual da arte do assalto — aprovado — usuário

- O usuário olhou as capturas dos 4 chefes da RT-6t.10 (`docs/art/rt6t/chefes-perto.png` e `chefes-oppo.png`) e
  respondeu "Aprovo". O aceite visual pendente da RT-6t.10 está resolvido; a entrega fica concluída sem pendência.
- **D28 = A confirmada** por essa aprovação: a arte do assalto fica com o sombreado toon de 3 faixas e as cores de
  torre/defesa da paleta de cada mundo, sem remodelar e sem rostos (antes valia A só por falta de resposta).

## 2026-11-22 — T-RT6t11-1..7 — Reserva de desenho para OpenGL ES — técnica (sessão RT-6t.11)

- **T-RT6t11-1 — Uma peça de desenho para as duas rotas (`ToonInstanceDraw`), usada pelos três desenhistas.** A reserva
  não fica só no `SwarmRenderer`: o assalto (`LooseSwarmRenderer`) e o cenário (`InstancedProps`) também liam buffer
  estruturado no vertex shader e quebrariam no mesmo aparelho. Os três guardam os mesmos `ToonInstance` e entregam à peça,
  que sobe para o buffer ou converte para blocos. Descartado: um segundo caminho dentro de cada desenhista (três cópias da
  mesma lógica).
- **T-RT6t11-2 — Escolha por palavra-chave global, sem material extra.** A rota liga/desliga `STUDIOKIT_TOON_BUFFER`
  (global) uma vez; os materiais instanciados continuam os mesmos assets. Descartados: um segundo material por desenhista
  (assets e fiação a mais no construtor de cena) e criar material em jogo (`Destroy` na saída fere a regra 2.2.1). Custo:
  o `ToonLit` e a sombra dobram de variantes; build sem mudança perceptível.
- **T-RT6t11-3 — Critério da escolha:** `supportsInstancing`, `maxComputeBufferInputsVertex > 0` e
  `graphicsShaderLevel >= 45`; lido uma vez. As variantes do buffer pedem `target 4.5` só com a palavra-chave
  (`#pragma target 4.5 STUDIOKIT_TOON_BUFFER`); as de blocos ficam em `target 3.5`, então rodam até em OpenGL ES 3.0.
- **T-RT6t11-4 — Blocos de 256 (`forcemaxcount:256`).** 256 matrizes = 16 KB, o menor bloco uniforme garantido pelo
  GLES 3; cabe porque `assumeuniformscaling nolightprobe nolightmap nolodfade` tiram o resto do bloco (a normal é
  calculada pela nossa conta, que aceita escala não uniforme). Cada bloco recebe uma fatia da matriz (`GetSubArray`, sem
  cópia) e as tintas no seu `MaterialPropertyBlock` (arrays de 256 fixos desde a carga).
- **T-RT6t11-5 — Sombras pelas mesmas matrizes, não por um segundo array.** O plano previa um array de matrizes de sombra
  calculado no mesmo job. Como a tinta por instância (com a escala da sombra em `w`) já vai por bloco, a sombra lê x, z e a
  escala da mesma instância, com a mesma conta da rota de buffer; o job único escreve matrizes e tintas para corpos e
  sombras. Menos memória e uma fonte só; a captura confirma o desenho igual.
- **T-RT6t11-6 — Rota forçável e troca em jogo.** `ToonInstanceRouting.Forced` (testes, APK de medição com
  `--ez chunks true`) troca a palavra-chave na hora e os desenhistas refazem a peça no quadro seguinte (única alocação
  fora da carga, e só quando alguém força). É o que permite a captura comparar as duas rotas no mesmo quadro congelado.
- **T-RT6t11-7 — Guardas e revisão.** Validador: materiais instanciados com instancing ligado. EditMode
  `ToonInstanceDrawTests`; PlayMode de captura `DrawRouteCaptureTests` (`-routeCaptureDir`, só com GPU, falha se mais de
  0,2% dos pixels diferirem). Revisor independente (subagente fable, só leitura): nada bloqueante; aplicados o comentário
  da margem zero do bloco e o job sem paralelismo para menos de 256 instâncias. Não aplicados: `multi_compile_vertex`
  (economia de variantes pequena, risco de mudar a seleção de variantes sem ganho medido) e passar `startInstance` em
  vez da fatia (a fatia deixa explícito que o array de tintas do bloco começa em 0).

## 2026-11-22 — T-RT6t12-1..6 — Motor no kit e fechamento da série — técnica (sessão RT-6t.12)

- **T-RT6t12-1 — Layout no pacote igual ao do plano, testes num assembly só.** `Runtime/Toon`, `Runtime/Swarm`,
  shaders em `Shared/Shaders` (o `BlobShadowInstanced` passa a incluir `ToonInstanceData.hlsl` da mesma pasta) e os
  testes no `JungleDegree.StudioKit.Tests` que já existia (ganhou as referências `Toon`, `Swarm`, `Unity.Burst`,
  `Unity.Collections`). Todos os `.meta` movidos junto (GUIDs iguais); só deixaram de existir o asmdef de testes da
  incubadora e os `.meta` das pastas vazias (`StudioKitIncubator`, `Swarm/Shaders`, `Tests`), que nada referencia.
- **T-RT6t12-2 — Dependências com as versões que os dois jogos já resolviam.** `com.unity.burst` 2.0.0 e
  `com.unity.collections` 6.6.0 no `package.json` (o URP 17.6 já as trazia nos dois jogos); `Unity.Mathematics` vem
  com elas. O `packages-lock.json` dos jogos só ganha as duas linhas na entrada do kit.
- **T-RT6t12-3 — O validador do jogo segue cobrindo o código do kit que ele desenha.** A regra 2.2.1 (nada de
  criar/destruir objetos nem física em tempo de jogo) continua sobre `Packages/com.jungledegree.studiokit/Runtime/Toon`
  e `/Swarm` (`Path.GetFullPath` resolve o caminho do pacote); a pasta da incubadora saiu da lista de isolamento (os
  asmdefs `JungleDegree.StudioKit.*` são achados pelo próprio validador do kit, agora dentro do pacote).
- **T-RT6t12-4 — Caminho antigo de massinha fora do jogo.** O tema (`ClaybreakTheme`) perdeu `roster` (corpo
  `ClayRunnerFrames` do kit) e `deathEffect` (respingo `RunnerSplat`): eram só validados, nada em jogo os usava desde a
  6t.4/6t.7. Os asmdefs do jogo deixaram de referenciar `JungleDegree.StudioKit.Characters`. O `Crowd` fica (o jogo
  usa a sonda `DevFrameTimingProbe`), e o validador continua recusando `RunnerPool`/`RunnerUnit`/`RunnerCrowd`/
  `PlayerController`/`CameraFollow` na cena. Varredura de órfãos por GUID em `Assets/Claybreak`, `Assets/Settings` e
  `Assets/Editor`: nenhum asset sem referência; `Terracotta.mat` só é citado por código porque é a cor dos estilos de
  torre alternativos (`ClayRuin`/`ClayTotem`, estilos que o `WorldContent` ainda pode escolher), e fica.
- **T-RT6t12-5 — Prova visual da multidão pelas capturas PlayMode.** A multidão só existe jogando (simulação e desenho
  em `Update`/`LateUpdate`); imitar isso no modo de edição exigiria API nova no kit só para ferramenta. As telas de
  `docs/art/telas` (modo de edição) seguem como prova de layout, HUD, torres e cenário; a multidão, os portões, o
  assalto e as rotas são provados pelas capturas PlayMode regravadas nesta entrega (`docs/art/rt6t`).
- **T-RT6t12-6 — Gate final nos dois casos mais pesados do nível de gate.** O nível de gate já cobre corrida (`run:4`,
  multidão no teto com o cenário do mundo 4) e assalto (`4`, torre do chefe, canhão, varredor, muro, escudo, 300 em voo);
  o gate final mede os dois em Vulkan, com o pacote 0.5.0. A rota de reserva GLES foi aprovada na 6t.11 e não mudou.

## 2026-11-22 — RT-6t.13 — Neblina quase zero e líder da multidão — usuário

O usuário jogou no OPPO e pediu dois ajustes (registrados pela sessão RT-6t.13):

- **Neblina:** "muito intensa"; remover ou deixar próximo de zero. Só o suficiente para o horizonte não cortar seco.
- **Líder da multidão — aprovado "como proposto":** um boneco da mesma forma escolhida no menu (Bolinha/Pintinho),
  ~1,6× maior, sempre na ponta da frente da multidão, com um destaque fixo (coroinha ou anel na cor de UI da paleta do
  mundo). É ele que o dedo controla; a multidão segue atrás (a formação girassol fica atrás do líder, não em volta). O
  portão que o líder atravessa é o que vale (substitui a regra do centro invisível); ao passar, o portão escolhido acende e
  o outro apaga. O líder não conta no número da multidão e não morre nos portões ruins; multidão em zero encerra a fase
  como antes.

## 2026-11-22 — T-RT6t13-1..7 — Neblina mínima e líder da multidão — técnica (sessão RT-6t.13)

- **T-RT6t13-1 — Neblina de 110 a 150 m nos 4 mundos (dado, `PaletteContent`).** Tudo o que o jogador olha (multidão,
  portões, torre a menos de ~110 m) fica sem neblina; ela só fecha o chão longe e termina antes do plano de corte da
  câmera (170 m, agora a constante `ClaybreakSceneBuilder.CameraFarClip`), então o horizonte não corta seco. As cores de
  neblina das paletas ficaram como estavam.
- **T-RT6t13-2 — Nova proteção contra árvore "aparecendo do nada": a janela do cenário passa do que a câmera vê limpo.**
  A regra da 6t.8 (neblina termina antes do fundo da janela de 90 m) deixou de valer. Escolhida a opção sem mudar o kit:
  janela de reciclagem de 180 m (`SceneryArt.PropWindowLength`), com a quantidade de árvores e pedras de cada mundo
  dobrada (mesma densidade). Descartadas: fade de escala/alpha na entrada (exige mudar `PropScatter` no studiokit, que
  outra sessão estava editando — K.13 — e mexeria no Wild Pack) e baixar o plano de corte (encurta o horizonte, que é o
  que a neblina menor quer mostrar). Custo: até 156 árvores + 88 pedras instanciadas em vez de 78 + 44, mesmas 2 chamadas
  de desenho; medido no gate.
- **T-RT6t13-3 — O validador pergunta o que importa: de alguma câmera do jogo dá para ver uma árvore surgir fora da
  neblina?** `PropWrapGuard` (editor, puro) calcula a menor profundidade de visão de um ponto onde uma árvore reaparece que
  ainda cai dentro do campo de visão — câmera da corrida em vários tamanhos de multidão (até o teto), à esquerda/centro/
  direita, ajustada ao retrato como o jogo faz (21:9, 19,5:9 e 16:9), e os dois enquadramentos do assalto nas distâncias
  de torre dos níveis — e o validador exige neblina completa antes dela e antes do plano de corte. Com 180 m o ponto mais
  perto visível fica a ~156 m (neblina termina em 150); a janela antiga de 90 m daria ~80 m (o teste EditMode prova que ela
  falharia com a neblina nova).
- **T-RT6t13-4 — Líder desenhado no mesmo pipeline instanciado, no jogo, sem mudar o kit.** `CrowdLeader` usa duas
  `ToonInstanceDraw` de 1 instância (corpo com a sombra em bolha; coroa) com o material da multidão e um material da
  coroa (`ToonLit` instanciado, emissão 0,5). Pula, achata e inclina com as mesmas fórmulas públicas do kit
  (`SwarmMotion`, `SwarmAnimation`); veste a forma e a cor da multidão; a coroa é um anel com 5 pontas
  (`LeaderArt`, ~200 triângulos) na cor `UiAccent` da paleta do mundo, com joias brancas. Sem GameObject por unidade, sem
  renderer próprio, nada criado em jogo (buffers alocados no `OnEnable`). +3 chamadas de desenho.
- **T-RT6t13-5 — A formação segue atrás do líder.** O `RunCrowd` passou a guardar a posição do líder (é ela que corre na
  velocidade da fase); o centro da formação girassol fica atrás dele pela profundidade do disco
  (`DepthStretch × raio(n)`) mais 0,9 m, suavizado quando a multidão cresce ou encolhe. O dedo continua mexendo o centro do
  enxame (o líder anda no X dele); a câmera continua seguindo a formação. `RunCrowd.Position` passou a ser o líder (portões,
  caixotes, linha de lançamento, faixa do assalto); `FormationCenter` dá o centro.
- **T-RT6t13-6 — Portões, caixotes e linha de lançamento pela posição do líder; feedback de acender/apagar.** A dupla de
  portões não some mais ao ser cruzada: o lado escolhido troca para um painel quase opaco da sua cor (`GateGoodLit`/
  `GateBadLit`) e o outro para um cinza apagado (`GateOff`) com o número a 30%; o resto da multidão atravessa o portão que o
  líder escolheu. Caixotes seguem a mesma lógica (o líder pega pelo lado em que está) — mais coerente com "o líder decide"
  e sem custo. A fase entra no assalto quando o líder chega à linha (antes, o centro).
- **T-RT6t13-7 — No assalto o líder fica parado na linha de lançamento comandando.** Ele não é lançado (as unidades saem
  do anel de fora como antes) e continua andando de lado com o dedo, marcando a faixa escolhida; nenhuma regra de dano
  mudou. Se a multidão zera, a fase acaba como antes (o líder não conta).
- **T-RT6t13-8 — Revisão independente (subagente fable, só leitura): nada bloqueante.** Aplicados: o teste do líder compara
  com a posição da corrida (o desenho do líder é do quadro anterior quando a corrotina retoma); a formação recua 3× mais
  rápido do que avança (depois de um portão de multiplicar o anel novo não passa à frente do líder); o construtor de cena
  copia para o líder os ajustes de pulo e animação do enxame (não desalinham se forem afinados).

## 2026-11-22 — RT-6t.13 (ajuste) — Sem neblina nenhuma — usuário

- **Decisão do usuário (jogou a build da 6t.13 no OPPO):** "Mano, tira essa neblina, ta muito ruim, fica parecendo que tem
  uma névoa me atrapalhando a ver o jogo." Neblina zero nos 4 mundos, na corrida e no assalto; as cores das paletas
  continuam as aprovadas.
- **T-RT6t13-9 — A neblina sai pelo jogo, não pelo kit.** O `ToonPaletteApplier` do kit sempre liga a neblina linear da
  paleta; o `WorldScenery` (jogo) a desliga logo depois de aplicar cada mundo (`RenderSettings.fog = false`) e o construtor
  grava a cena sem neblina. Todo shader com neblina (toon, pista listrada, sombras) roda a variante sem neblina — só fica
  mais barato. O kit não muda (sem aceite do Wild Pack); a cor e a faixa de neblina continuam no dado da paleta porque o
  kit as exige, mas nada as desenha.
- **T-RT6t13-10 — Caça a outras fontes de "véu":** sem pós-processamento (a câmera não tem dados adicionais do URP:
  sem bloom, vinheta, gradação ou tonemapping; gradação do URP em LDR); o céu é um gradiente em tela desenhado atrás de
  tudo (não mistura com o chão); camadas translúcidas são só os painéis dos portões (feedback aprovado na 6t.13);
  nenhuma cor de paleta foi clareada ou dessaturada. A única fonte de véu era a neblina.
- **T-RT6t13-11 — Sem neblina para esconder, as árvores entram pelo plano de corte.** A janela de reciclagem passa de 180
  para 200 m (árvores e pedras de cada mundo × 200/90 das de referência, mesma densidade) e o validador troca a regra:
  o ponto mais perto onde uma árvore reaparece à vista de qualquer câmera do jogo tem de ficar além do plano de corte
  (170 m). Medido: 172,5 m. O teste EditMode prova que a janela antiga (90 m) faria árvores surgirem na tela.
