# Perfil de sessão — Runner vs Tower

## Maestro

- **Branch de trabalho:** `main` (remote `origin`, github.com/pesconi/runner-vs-tower).
- **Plano:** `docs/plano-de-entregas.md`. Não há painel; a próxima entrega é a primeira RT-X da tabela
  "Visão geral" que não esteja riscada/marcada "concluída" (RT-0 e RT-1 já estão; RT-1 foi feita
  no repo `studiokit`, sem commit `Stage RT.1` aqui). Decisões em `docs/riscos-e-decisoes.md`.
- **Entrega:** `sed -n '/^## RT-X /,/^## RT-/p' docs/plano-de-entregas.md | head -60` (trocar X).
- **Validação:** RT-0: documentos gerados e coerentes. RT-2 em diante, em batchmode com o Editor
  fechado (`-batchmode -nographics -buildTarget Android -projectPath <projeto> -logFile <log>`):
  `-executeMethod Claybreak.Editor.ClaybreakSceneBuilder.BuildAndQuit` (cena/dados),
  `-executeMethod Claybreak.Editor.ClaybreakValidation.RunAndQuit`,
  `-executeMethod JungleDegree.StudioKit.Editor.EngineIsolationValidation.RunAndQuit`,
  `-runTests -testPlatform EditMode` e `-runTests -testPlatform PlayMode`,
  `-executeMethod Claybreak.Editor.ClaybreakBuild.BuildAndroidAndQuit [-claybreakMeasure]`. Tudo isso de uma
  vez (mais a tabela de economia e de fases `docs/rt6-economia.md` e as capturas de arte, que regravam
  `docs/art/telas/` e `docs/art/rt6/`): `Tools/rt4-check.sh [/c/temp/rt4] ["passos"]`. E o gate 3x no OPPO
  CPH2791 com `Tools/rt2-gate.sh <rótulo> 60 20 <apk de medição> [mundo]` (decisão do usuário 2026-11-20: o gate roda UMA vez por entrega, no fim, no mundo/nível mais pesado;
  repetir só se sair do limite; ao terminar, fechar o app no celular com `adb shell am force-stop com.jungledegree.runnervstower`) (aparelho conectado e desbloqueado;
  passo a passo em `docs/rt4-fatia.md` seção 4).
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
- 2026-11-20 (RT-4): o daemon do adb e o cabo do OPPO caem entre comandos; rodar `adb start-server` antes e
  conferir `adb devices` na hora do gate. Unity em batchmode SEM `-nographics` renderiza (capturas de arte
  com `ArtDirectionCapture`). Em Python no Windows, gravar com `newline='
'` para não trocar o fim de linha.
- 2026-11-20 (RT-5): no gate, a última janela de 5 s coincide com a coleta de memória do script e pode
  estourar sozinha; se só ela falhar, rodar de novo uma vez e registrar as duas rodadas. O OPPO tem
  bloqueio com senha: se `isKeyguardShowing=true`, seguir com o resto e conferir de novo antes de fechar.
  O validador agora cobre catálogo e saves de exemplo (`Assets/Claybreak/Editor/SaveFixtures`); mudança
  de catálogo exige reconstruir a cena (as listas da loja e dos mundos são montadas dele).
- 2026-11-20 (RT-6): o `Tools/rt4-check.sh` completo passa de 10 min; rodar em segundo plano e esperar. Se um
  comando for morto, o Unity em batchmode continua rodando e trava o projeto para a próxima rodada: conferir
  `tasklist | grep Unity.exe` antes de rodar de novo. Conteúdo novo = dado em `WorldContent` (mundo por
  `WorldDefinition` + `WorldRecipe`), arte em `TowerArt`/`WorldSceneryArt`, e reconstruir a cena.
- 2026-11-20 (RT-7): a ferramenta Bash deste app troca as sequências barra-t, barra-r e barra-n dentro do comando e quebra heredoc com
  apóstrofo; scripts Python de edição vão para um arquivo (Write) e rodam de lá. `/security-review` falha aqui (sem
  `origin/HEAD` e com mudanças não commitadas): usar um revisor independente só leitura. O teste offline de anúncios do
  Wild Pack (`MultiplierRunner/Tests/RewardedAds`) compila por caminho os arquivos de `Runtime/Ads` do kit que o
  premiado usa: arquivo novo ali precisa entrar no `.csproj` dele (24/24 é a linha de base).
