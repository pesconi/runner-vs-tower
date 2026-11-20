# Perfil de sessão — Runner vs Tower

## Maestro

- **Branch de trabalho:** `main` (remote `origin`, github.com/pesconi/runner-vs-tower).
- **Plano:** `docs/plano-de-entregas.md`. Não há painel; a próxima entrega é a primeira RT-X da tabela
  "Visão geral" que não esteja riscada/marcada "concluída" (RT-0 e RT-1 já estão; RT-1 foi feita
  no repo `studiokit`, sem commit `Stage RT.1` aqui). Decisões em `docs/riscos-e-decisoes.md`.
- **Entrega:** `sed -n '/^## RT-X /,/^## RT-/p' docs/plano-de-entregas.md | head -60` (trocar X).
- **Validação:** RT-0: documentos gerados e coerentes. RT-2 em diante, em batchmode com o Editor
  fechado (`-batchmode -nographics -buildTarget Android -projectPath <projeto> -logFile <log>`):
  `-executeMethod Claybreak.Editor.PrototypeSceneBuilder.BuildAndQuit` (cena/dados),
  `-executeMethod Claybreak.Editor.ClaybreakValidation.RunAndQuit`,
  `-executeMethod JungleDegree.StudioKit.Editor.EngineIsolationValidation.RunAndQuit`,
  `-runTests -testPlatform EditMode` e `-runTests -testPlatform PlayMode`,
  `-executeMethod Claybreak.Editor.ClaybreakBuild.BuildAndroidAndQuit [-claybreakMeasure]`; e o gate 3x no
  OPPO CPH2791 com `Tools/rt2-gate.sh` (exige o usuário com o aparelho conectado; passo a passo em
  `docs/rt2-prototipo.md` seção 6).
- **Custo:** liberado pesquisa web gratuita e edição de documentos. Qualquer gasto pago, upload
  ao Play Console/AdMob ou cópia de código do Wild Pack (reuso só pelo pacote studiokit): parar e perguntar.
- **Contexto:** jogo hybrid-casual de multidão com portões matemáticos e assalto a uma torre,
  Unity 6000.6.7f1, URP, Android, retrato, estúdio Jungle Degree.
- **Concluída quando:** commit `Stage RT.X: <resumo>` em `origin/main`, árvore limpa, decisões
  da entrega registradas.
- **Decisões:** registro em `docs/decisoes.md`; pendentes na tabela da seção 2 de
  `docs/riscos-e-decisoes.md` (marcar "decidido em <data>: <opção>").
- **Revisor:** fable.
- **Qualidade:** padrão do maestro (limpo, escalável, seguro) + gate 3x de desempenho no OPPO
  CPH2791 + regra 2.2.1 (nada de Instantiate/Destroy em runtime).
- **Canal:** o usuário responde pelo celular; toda decisão em ficha autossuficiente, com push.

## Aprendizados

- 2026-11-20: o jogo ainda não tem `SISTEMA.md`; o status de cada entrega fica na própria seção
  RT-X do plano (linha "Status") e as decisões em `docs/decisoes.md`. Não criar `SISTEMA.md` sem
  pedido.
- 2026-11-20 (RT-2): se o Editor do Unity estiver aberto neste projeto, não fechá-lo sem o usuário;
  validar numa cópia em caminho curto (`C:\temp\rt2\proj`, manifest com caminho absoluto do kit; o
  scratchpad estoura o limite de 260 caracteres do Unity) e trazer de volta só os arquivos gerados.
