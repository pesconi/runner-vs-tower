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
