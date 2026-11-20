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
