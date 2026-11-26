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
  repetir só se sair do limite; ao terminar, fechar o app no celular com `adb shell am force-stop com.jungledegree.stampeep` (D34; antes da RT-9b.1 era `com.jungledegree.runnervstower`)) (aparelho conectado e desbloqueado;
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
- 2026-11-21 (RT-8): dois Unity em batchmode ao mesmo tempo (este projeto e o Wild Pack) podem derrubar um deles na
  partida ("Failed to initialise UDS client", saída 21): rodar o aceite do Wild Pack depois, sozinho. Edições por script:
  o texto vai num arquivo de especificação (Write) e o Python lê com `python -X utf8` (sem isso o stdin vira cp1252 e
  "¢"/"·" não casam). O áudio é dado: `python Tools/audio/clay_siege_audio.py` regrava os WAV; o validador recusa clipe
  fora de `docs/audio-licenses.md`. Os testes PlayMode acham botões pelo caminho na hierarquia (`/Ui/Screens/<Tela>/Safe/...`).
- 2026-11-21 (RT-8b): os builds em batchmode sujam `Assets/Settings/*.asset`, `ProjectSettings/ProjectSettings.asset`
  (`AndroidKeystoreName`) e `ProjectSettings/AndroidResolverDependencies.xml` (fim de linha): reverter com `git checkout --`
  antes do commit. O Firebase entra pelo SDK Android nativo (`Assets/Plugins/Android/FirebaseAnalytics.androidlib`); o
  `google-services.json` fica em `..\_privado\runner-vs-tower\` e vira recurso no build. Prova no aparelho:
  `Tools/rt8b-firebase-check.sh`.
- 2026-11-21 (RT-6t.1): o passo `scene` do `rt4-check` regrava `Claybreak.unity` (fileIDs novos) e os `world3/*.mat`
  (`_Color` legado) mesmo sem mudança no jogo: reverter com `git checkout -- Assets/Claybreak/Content` se a entrega não
  mexeu no jogo, junto com os `Assets/Settings`/`ProjectSettings` já listados. PlayMode roda com `-nographics` (dispositivo
  `Null`, nada se desenha): para provar desenho, rodar o teste sem `-nographics` (`-runTests -testFilter <Classe>`).
  Gate da multidão toon: `Tools/rt6t-toonlab-gate.sh [apk]` (matriz MSAA/HDR + 2000 de registro, captura em `docs/art/rt6t/`).
- 2026-11-21 (RT-6t.1): shader que não compila vira magenta sem erro em tempo de jogo e os números do gate saem
  "bons" medindo o shader de erro. Antes de aceitar um gate de visual novo, olhar a captura do aparelho e o log do build
  (`grep "Shader error" <out>/logs/apk-measure.log`).
- 2026-11-21 (RT-6t.3): em teste PlayMode no batchmode, `WaitForEndOfFrame` nunca dispara (o teste fica preso para
  sempre): para capturar, pôr `camera.targetTexture` e ler a textura no quadro seguinte (`yield return null`). Captura
  da `ToonLab` em retrato: `-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.ToonLab -toonLabCaptureDir
  <pasta>` sem `-nographics`. Não rodar um passo do `rt4-check` enquanto outro Unity ainda está aberto no projeto.
- 2026-11-21 (RT-6t.2): capturas lado a lado com o protótipo: `Tools/rt6t/shape-captures.sh` (Unity com GPU + Chrome
  sem tela com SwiftShader renderizando `Tools/rt6t/proto-shapes.html`, que baixa o three.js 0.160 da CDN: precisa de
  rede). O PlayMode das formas usa `-shapeCaptureDir`; a ToonLab do jogo troca forma por `--ei shape 0|1 --ez eyes`.
- 2026-11-21 (RT-6t.4): a corrida é o `Swarm` (`RunCrowd`); as capturas do `ArtDirectionCapture` (modo de edição) não
  desenham a multidão. Prova visual da corrida: `-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.RunCaptureTests
  -runCaptureDir <pasta>` sem `-nographics`. Gate da corrida: `Tools/rt2-gate.sh <rótulo> 60 20 <apk> run:N` (mundo 4).
- 2026-11-22 (RT-6t.5): parar a tarefa de fundo do `rt4-check` não mata o script nem o Unity: ele segue para o próximo passo.
  Encerrar com PowerShell (`Get-CimInstance Win32_Process` filtrando `rt4-check`/`batchmode`, `Stop-Process`) e conferir que não
  sobrou `Unity.exe`. "Tundra build failed due to an internal error" (renomear cache do Bee) é transitório: repetir o passo.
  Captura das animações e do chão sob a multidão: `-testFilter Claybreak.Tests.AnimationCaptureTests -animationCaptureDir <pasta>`
  sem `-nographics`; gate das mortes: `toonlab:1000:4:1:512` (5º campo = morrendo).
- 2026-11-22 (RT-6t.6a): o passo `capture` regrava `docs/art/rt4` e `docs/art/rt6` (registros das decisões D17/D20): reverter
  com `git checkout --` antes do commit; `docs/art/telas` fica (é a tela atual). Nas capturas de modo de edição, abrir cena nova
  descarrega ScriptableObjects carregados antes (a paleta vira nula) e componentes não rodam `OnEnable`: recarregar e aplicar
  depois de abrir a cena. O kit (incubadora) não pode dizer "tower" (termo do isolamento). O nível de gate veste a skin mais cara
  (carga máxima), não o BARRO. Captura dos 4 mundos: `-testFilter Claybreak.Tests.WorldCaptureTests -worldCaptureDir <pasta>` sem `-nographics`.
- 2026-11-22 (RT-6t.8): o passo `scene` também regrava `Assets/Claybreak/Lab/ToonLab.unity` (só fileIDs) e
  `Generated/RunnerUnit.prefab` (1 linha) mesmo sem mudança neles: reverter com `git checkout --`. O cenário é instanciado
  (`InstancedProps`, sem objetos): nas capturas de modo de edição ele só aparece porque o `ArtDirectionCapture` chama
  `Render(câmera)` antes de cada render; ferramenta nova de captura em modo de edição precisa fazer o mesmo. Nos testes
  PlayMode, a câmera recua um pouco no começo da corrida (o rig enquadra a multidão): medir movimento depois de ~1 s.
- 2026-11-22 (RT-6t.6b): a forma da multidão é dado do catálogo (`ClaybreakCatalog.Shapes`) e vai no save v4 (`shape`); o
  nível de gate veste a skin mais cara E a forma mais pesada: capturas que queiram a skin padrão ou outra forma precisam
  trocar as duas (`TryWearSkin(0)` + `Crowd.Wear`, `Crowd.UseShape`). Captura do menu com o seletor, das formas e das skins
  nos 4 mundos: `-testFilter Claybreak.Tests.ShapeSkinCaptureTests -shapeSkinCaptureDir <pasta>` sem `-nographics`; para a UI
  sair na textura, pôr o canvas em `ScreenSpaceCamera` com a câmera da corrida.
- 2026-11-22 (RT-6t.7): as unidades em voo do assalto são o `LooseSwarmRenderer` (`flow.Field.Flyers`), índice i = unidade i
  do `AssaultField`. O `apk-measure` compila código que só existe com `RELEASE_MEASUREMENT` (sonda de quadro): ao tirar um
  `using`, rodar o passo `apk` antes de dar a validação por boa. Captura do assalto mais pesado (gate do mundo 4):
  `-testFilter Claybreak.Tests.AssaultCaptureTests -assaultCaptureDir <pasta>` sem `-nographics`. No nível 1 nada voa
  sem o jogador segurar.
- 2026-11-22 (RT-6t.9): texto com contorno é TextMeshPro com o atlas **estático** `Content/Fonts/Baloo2-ExtraBold SDF.asset`
  (`GameFonts`; só é refeito se faltar caractere): caractere novo em rótulo TMP precisa entrar em `GameFonts.Characters` (o
  teste `GateFeedbackEditTests` confere). `UI.Text` e `TextMesh` usam a TTF dinâmica. Captura dos portões, número flutuante,
  pop e glifos: `-testFilter Claybreak.Tests.GateFeedbackTests -gateCaptureDir <pasta>` sem `-nographics`. As capturas de
  modo de edição agora aplicam o `ToonLook` (antes, objetos `ToonLit` sem slots saíam pretos).
- 2026-11-22 (RT-6t.10): a arte do assalto é `ToonLit` com `_StructureWeights`; a cor vem das globais de estrutura/defesa
  que o `ToonPaletteApplier` escreve (captura de modo de edição precisa chamar `Apply`/`ApplyGlobally` da paleta). Material
  novo do assalto = papel novo em `ArtPalette` (mistura das 4 cores); primitiva toon precisa da malha com cor de vértice
  (`ToonPrimitives`, o `Prim` troca sozinho). Ao mudar o shader de um material existente, reverter os `.mat` para o HEAD
  antes de reconstruir não é preciso: o `ArtPalette` sobrescreve o asset limpo. Captura dos 4 chefes:
  `-testFilter Claybreak.Tests.BossCaptureTests -bossCaptureDir <pasta>` sem `-nographics`.
- 2026-11-22 (RT-6t.11): o desenho instanciado tem duas rotas (`ToonInstanceDraw`/`ToonInstanceRouting`): buffer (Vulkan) e
  blocos de 256 (GLES; o Mali do OPPO em GLES tem `vertexBuffers=0`). Material instanciado novo precisa de GPU instancing
  ligado (o validador confere). Gate em GLES com a reserva: `RT2_EXTRA="--ez chunks true -e unity -force-gles"
  Tools/rt2-gate.sh ...` (a linha `[ToonRoute]` do log diz API e rota). Captura comparando as rotas:
  `-testFilter Claybreak.Tests.DrawRouteCaptureTests -routeCaptureDir <pasta>` sem `-nographics`.
- 2026-11-22 (RT-6t.12): o motor toon/multidão está no pacote (`studiokit` 0.5.0, `Runtime/Toon`, `Runtime/Swarm`,
  `Shared/Shaders`); não há mais incubadora: mudança nesse código é entrega K no repo `studiokit` (aceitação do Wild Pack
  depois deste Unity). A linha de base do teste offline de anúncios do Wild Pack é **25/25** desde a 8.6g dele. O `TestProject`
  do studiokit (lista de isolamento ampla) pega nomes de jogo que a lista de cada jogo não pega: rodar o isolamento lá também.
  Varredura de órfãos por GUID: script em Python que cruza o `guid:` de cada `.meta` com as referências dos assets de texto.
- 2026-11-22 (RT-6t.13): outra família de sessões pode estar escrevendo no `studiokit` (K.13 estava com a árvore suja e um
  Unity em batchmode no `TestProject`): este jogo compila o código não commitado do kit pelo caminho `file:`. Antes de
  rodar Unity ou adb, conferir `git -C ../studiokit status --short` e os `Unity.exe` (com linha de comando) de outros
  projetos; resolver no jogo em vez de mexer no kit. A neblina mínima é guardada pelo `PropWrapGuard` (validador loga a
  distância do reaparecimento visível mais perto); captura do líder: `-testFilter Claybreak.Tests.LeaderCaptureTests
  -leaderCaptureDir <pasta>` sem `-nographics`. O líder é `RunCrowd.Position`; o centro da formação, `FormationCenter`.
- 2026-11-22 (RT-6t.14): o limite da multidão é `ArmyLimit.Units` (300), única fonte. Fase à mão ou portão novo precisa caber
  nele em qualquer caminho, com as caixas: o validador recusa (`GateMath.HighestPromise`; coluna "Maior promessa" em
  `rt6-economia.md`). O gerador escolhe a variante de layout no rascunho sem encaixe e só depois encaixa os portões, para que
  mudar o potencial não troque defesas e torres das fases.
- 2026-11-23 (RT-6t.15): a velocidade da corrida é `RunPace.Factor` (1,3) aplicado por `WorldContent.Specs` sobre o ritmo do
  percurso das receitas e fases à mão: mudar velocidade = mudar o fator (ou a receita) e reconstruir a cena. O validador loga a
  janela de reação (`ReactionWindow`, leitura ≥ 3 s, entre pares ≥ 1,6 s) e recusa fase abaixo. A economia soma corrida real
  (pista ÷ velocidade) + 39 s/19 s de assalto e resultado. O `adb install` do OPPO pode encontrar o celular bloqueado: conferir
  `isKeyguardShowing` e deixar o comando para o usuário se seguir bloqueado.
- 2026-11-23 (RT-6t.16): o Unity em batchmode pode travar ao sair depois dos testes (`abort_threads` no fim do log, processo
  vivo). Se o log parar de crescer, mate o processo e o `rt4-check.sh` órfão (`taskkill //PID <bash> //T //F`) e rode só as
  etapas que faltam (`Tools/rt4-check.sh <out> "playmode economy ..."`). Capturas das telas em 4 proporções e do braço do
  chefe: PlayMode com GPU (sem `-nographics`), `-testFilter ScreenFitTests -screenFitCaptureDir <pasta>` e
  `-testFilter SweeperMotionTests -sweeperCaptureDir <pasta>`. O `adb install -r` pode responder "failed to install" vazio
  e ter instalado: conferir `lastUpdateTime` e o tamanho do `base.apk` antes de repetir.
- 2026-11-23 (RT-6t.17): os menus são TMP (`Caption` no builder; presets `Baloo2 Ui`/`Baloo2 Ink`); cores em `UiColors`. A Baloo 2
  não tem "★": estrela e moeda são sprites TMP (`UiIcons.Star`/`Coin`, asset `Fonts/GameIcons`); nunca escrever "★" num rótulo.
  O `ScreenFitTests` reprova glifo fora do atlas e legenda maior que o botão. Se o `studiokit` de outra sessão não compilar,
  build com um clone do último commit do kit (manifest temporário para `file:C:/temp/kit-head/...`, depois
  `git checkout -- Packages/`), sem mexer no repo do kit.
- 2026-11-23 (RT-6t.18): obstáculos da corrida = `ClaybreakLevel.obstacles` (até 6), movidos por `ObstacleMotion` (puro) e
  desenhados pelo `RunObstacleCourse` (1 chamada por tipo); a janela de reação deles está no `ReactionWindow`. Fase de
  laboratório: `Data/ObstacleLab.asset`, aberta no jogo instalado com `--ez obstacleLab true` (não salva). Prova com GPU:
  `-testFilter Claybreak.Tests.ObstacleRunTests.CaptureEachKindKillingOnBothRoutes -obstacleCaptureDir <pasta>` sem
  `-nographics`. Mudança de tamanho da pista (fase mais longa, como o laboratório) regrava `GroundAndRails.asset`.
- 2026-11-23 (RT-6t.19): a curva de obstáculos é dado da `WorldRecipe` (tipos, fase do tipo novo, contagem, chefe, lentidão);
  o encaixe na pista é `LevelGenerator.PlaceObstacles` (fases à mão usam `WorldContent.Beat` + `WithObstacles`) e o tempo da
  fase é guardado por `FitObstacles` (±4,8% de corrida + `SecondsAfterRun`). Perda esperada = `ObstacleLoss` (cache; a vida
  usa quem desvia). Iterar a curva sem a validação completa: EditMode só com `-testFilter Claybreak.Tests.ObstacleCurveTests`
  (o teste loga a tabela de perdas por fase), ~3 min; não precisa reconstruir a cena para isso.
- 2026-11-23 (RT-6t.20): formas são itens do catálogo com preço, quique (`motion`) e figura da loja (`WorldContent.Shapes`);
  a figura é rasterizada por software no editor (`ShapeFigures`, roda com `-nographics`). O quique chega ao kit pela
  `SwarmMotionBridge` (campo serializado `motion`; o kit não tem setter). Save v5 (`ownedShapes`). Capturas das formas com
  GPU: `-testFilter Claybreak.Tests.ShapeShopCaptureTests -shapeShopCaptureDir <pasta>` (corrida com 300 e assalto); a aba
  SHAPES sai no `ScreenFitTests` (`telas-loja-formas.png`). Silhueta nova precisa passar em
  `EveryPairOfShapesHasADifferentOutline` (≥ 15% na altura, largura ou proporção). A ferramenta Bash também quebra heredoc
  com apóstrofo em `cat > arquivo <<EOF`: escrever o arquivo com Write.
- 2026-11-23 (RT-6t.21): o nome visível do jogo é `ProjectConfiguration.GameName` (productName, título e créditos; o
  validador recusa outro e "Clay Siege" na cena). Busca de marca no navegador: o EUIPO eSearch pela URL
  `#basic/1+1+1+1/100+100+100+100/<TERMO>` mostra o resultado da busca ANTERIOR ao trocar só o hash: recarregar a página
  a cada termo. INPI: "Pesquisa fonética (para Marcas)". Nome e ícone do APK: `aapt2 dump badging <apk>` (build-tools do
  SDK do Unity); o ícone é adaptativo (`mipmap/app_icon`), hoje o padrão do Unity.
- 2026-11-24 (RT-9a): material da loja em `docs/store/` (README com os comandos). Capturas da loja vêm de fases reais:
  `-testFilter Claybreak.Tests.StoreCaptureTests -storeCaptureDir <pasta>` com GPU (save de meio de jogo, PLAY, portões
  escolhidos por `GateMath`, dedo simulado no assalto com `InputSystem` em `IgnoreFocus` só no teste; ~15 min) e
  `python -X utf8 Tools/store/make_store_art.py <pasta>`; o nível de medição não serve (rótulo DEVICE GATE, portões x1).
  O `ProjectSettings.asset` agora guarda o ícone do celular (`m_BuildTargetPlatformIcons`, Unity 6 só aceita o tipo
  adaptativo): depois de um build, reverter só a linha `AndroidKeystoreName`, não o arquivo inteiro. A política tem
  endereço no jogo: teste que aperte o link da política abre o navegador da máquina (confira a URL, não aperte).
- 2026-11-24 (RT-9b.1): o pacote é `com.jungledegree.stampeep` (D34); as ferramentas de aparelho leem de
  `Tools/app-id.sh` (fonte `ProjectConfiguration.ApplicationId`), e o validador recusa o ID antigo. IDs do AdMob (D35) ficam
  em `..\_privado\runner-vs-tower\admob-ids.json`: o build do jogo os põe no `GoogleMobileAdsSettings`/`AdUnits.asset` só
  durante o build e restaura; se um build for morto no meio, `git checkout --` nesses dois antes de qualquer coisa (o
  validador e o próximo build recusam). Nunca escrever IDs reais do AdMob em arquivo do repositório. Batchmode Android
  reinicia o servidor do adb: outra família (match-3) mede no mesmo OPPO, então combinar a janela antes de rodar Unity/adb.
- 2026-11-25 (RT-6t.22): a forma padrão é `catalog.Shapes[0]` = `SaveMigration.DefaultShape` (`bibopzi`, save v6); preço 0
  = grátis e possuída por todos (BIBOPZI e BALL). O construtor do kit não tem torno: corpo de forma livre = esfera dobrada
  depois de `ToMesh` (os vértices do corpo vêm primeiro) com normais da própria superfície; detalhes assentados por
  `OnBibopzi`. Esfera 6×3 não chega ao raio na frente (anéis a 60°): conta isso no marcador de frente (> 0,9 R).
  Capturas de perto com o shader do jogo: `-testFilter Claybreak.Tests.BibopziCaptureTests -bibopziCaptureDir <pasta>` com
  GPU; comparação com a referência: `python -X utf8 Tools/art/bibopzi_compare.py <pasta> <png da folha> docs/art/rt6t22`.
- 2026-11-25 (RT-6v3.1): o guia de estilo é conteúdo em código (`StyleGuideContent`; classe vira v3 editando `Classes()`), regravado
  pelo passo `scene` junto de `docs/art/v3/checklist.md`; mudou paleta/material/clipe/efeito do jogo, o validador pede entrada no
  guia. Capturas fixas: `-testFilter Claybreak.Tests.StyleCaptureTests -styleCaptureDir <pasta>` (GPU) e
  `python -X utf8 Tools/art/compare_captures.py <antes> <depois>` (tolerância 1%). O passo `capture` do rt4-check ainda regrava
  `docs/art/rt4` e `rt6`: reverter com `git checkout --`; `docs/art/telas` sem diff = tela igual.
- 2026-11-25 (RT-6v3.2): laboratório de vinil = `Claybreak/VinylLab` (irmão do ToonLit, local ao jogo) + `VinylDirection.All`
  (A, B, C, M; dado único das telas e do celular). Telas-conceito: `-testFilter Claybreak.Tests.VinylConceptTests
  -vinylConceptDir <pasta>` com GPU (`-vinylConceptOnly M`, `-vinylConceptQuick` só a corrida, `-vinylConceptIcons` só líder e
  ícones) e `python -X utf8 Tools/art/vinyl_sheets.py <pasta>`. O editor grava `_VinylHero` nos `.mat` do jogo ao sair:
  `git checkout -- Assets/Claybreak/Content` depois. O jogo recusa partir com o chão fora do ToonLit (troca de shader só em
  memória e devolvida antes de cada cena). Ensaio: `TOONLAB_VINYL="0 1 2 3 4" Tools/rt6t-toonlab-gate.sh <apk> 300 0 v3`.
  Código que cria objetos em `Lab/Runtime` reprova no validador (2.2.1): o que só as capturas usam vai para o assembly de testes.
  O tonemapping Neutral acinzenta a paleta pastel (valores de tela): o laboratório usa sem tonemapping.
  No ensaio, conferir a linha `[ToonRoute]` de cada arquivo: com "Activity not started" no log, o `am start` caiu na instância
  aberta e a rodada "GLES" saiu em Vulkan (refazer). GLES sem pós às vezes lê ~20 ms de GPU com 60 fps firmes: repetir.
- 2026-11-26 (K.v3a, no `studiokit` 0.15.0): o shader de vinil do jogo é `StudioKit/ToonVinyl` (irmão do ToonLit:
  trocar `material.shader` basta; luz por `ToonVinylLookApplier`, perfis por `ToonVinylSurface.ApplyTo`). Os testes do
  kit rodam também no EditMode deste jogo (os 3 de desenho do vinil ficam ignorados com `-nographics`). Medir shader
  isolado: `VinylScene` do `TestProject` + `SCENE_EXTRA="--ei vinyl N ..." Tools~/scene-measure-run.sh`; numa cena
  gerada pela 1ª vez, conferir no logcat a linha da cena antes de aceitar números (referência nula = nada desenhado).
- 2026-11-26 (RT-6v3.3): a superfície de uma classe (`ClassStyle.surface`) vira `v3` antes das malhas; materiais vinil saem
  do `StyleGuideContent.ApplySurfaces` (último passo do construtor de cena; emissão e névoa ficam as da arte). Mudou paleta,
  luz, pós ou superfície: rodar o passo `style` do `rt4-check` antes do `validate` (regrava `docs/art/v3/telas` e
  `docs/art/v3/contraste.json`, que o validador recusa se for de outro visual). Os desenhos instanciados do kit guardam a
  camada do objeto ao criar o desenho: para mudar a camada em teste, forçar a outra rota por um quadro
  (`ToonInstanceRouting.Forced`) e soltar. Camadas 20–29 são da sonda de contraste. A ferramenta Bash troca `\n` dentro de
  heredoc Python: scripts de edição em arquivo (Write). Matar processo do Unity/rt4-check pelo PowerShell foi negado pelo
  classificador nesta sessão: esperar a rodada terminar antes de editar.
