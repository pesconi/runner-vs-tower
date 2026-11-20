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
