# RT-2 — Protótipo (Claybreak)

Data: 2026-11-20. Entrega RT-2 de `plano-de-entregas.md`. **[F]** = conferido nesta sessão;
**[I]** = inferência ou proposta.

## 1. O que existe

Fase única jogável, módulo de jogo novo em `Assets/Claybreak/` (código em `Runtime/`, editor em
`Editor/`, testes em `Tests/`, cena e dados gerados em `Prototype/`). Consome o kit
`com.jungledegree.studiokit` 0.1.0 pelo `Packages/manifest.json` (`file:../../studiokit/...`); nada
foi copiado do Wild Pack.

| Bloco | Como ficou |
|---|---|
| Corrida | 4 pares de portões (`+10 / x2`, `x3 / -8`, `-15 / +20`, `÷2 / x2`) em z = 20, 40, 60, 80; líder e multidão do kit (`PlayerController`, `RunnerCrowd`, `RunnerPool` com 300 unidades), câmera do kit (`CameraFollow`). Começa com 10. Corrida ≈ 19 s a 5 m/s. Portões bons dão 160 unidades. |
| Assalto | **L1 "soltar em ondas"**: o líder para na linha de tiro; arrastar escolhe a faixa (3 faixas), segurar solta um fluxo de 12 unidades/s. Unidades em voo andam em lote (`AssaultField`: um `Update`, arrays pré-alocados, sem física, sem colisão entre unidades). |
| Fileira de portões do assalto | uma fileira, um portão por faixa: `x2`, `÷2`, `x3` (o `x3` fica atrás da muralha). |
| Torre | 250 de vida, 1 de dano por unidade, barra e número; cai afundando ao zerar. |
| Defesas | **Canhão**: pedra em arco na faixa com mais unidades, um tiro a cada 2,2 s, sombra avisa 0,9 s antes, mata num raio de 1,1 m; até 4 pedras no ar. **Muralha**: bloqueia a faixa da direita, 60 de vida, encolhe ao apanhar. |
| Projéteis | 32 pedras e 32 sombras pré-alocadas na cena; o nível limita quantas voam (regra 2.2.1: nada de Instantiate/Destroy em runtime). |
| Som | placeholders sintetizados pelo construtor (um por papel do kit + 2 loops), tocados pelo motor de som do kit (`GameAudio`, 6 vozes, intervalo mínimo por som). Vibração no impacto do canhão e na vitória (`GameHaptics` do kit). |
| Resultado | "TOWER DOWN!" / "ARMY LOST" e botão "AGAIN" (reinício imediato, sem recarregar a cena). |
| Nível do gate | `DeviceGateLevel.asset`: começa sozinho, 300 unidades em voo o tempo todo (unidade que acerta ou morre volta para a linha de tiro), canhão disparando no limite de 12 pedras. Só é usado pelo APK de medição com `--ez stress true`. |

Duração esperada da fase [I]: corrida ≈ 19 s + assalto ≈ 14–20 s (lançar até 160 unidades a
12/s, 3,5 s de voo, torre de 250) → 33–40 s, dentro dos 30–90 s do GDD, perto do piso.
Balanço [I]: com os portões bons (160) e a faixa `x2`, vence com folga (o GDD pede que a 1ª fase
vença na 1ª tentativa); uma corrida descuidada (~60) perde, a não ser que derrube a muralha e use o
`x3`. Se no aparelho a fase ficar abaixo de 30 s, subir a vida da torre ou baixar `launchRate` no
`PrototypeLevel.asset`. **Confirmar no aparelho.**

## 2. Decisões técnicas desta entrega

Registradas também em `decisoes.md` (T-RT2-1 a T-RT2-6).

1. **L1 em vez de L2.** A fileira de portões do assalto e a muralha só viram decisão se o jogador
   escolhe a faixa *enquanto* lança; com L2 (avalanche de um toque) a decisão é uma só. L1 também é
   o pior caso de CPU do lado da entrada (lançamento contínuo). L2 cabe depois como "segurar = soltar
   tudo" se o RT-3 pedir.
2. **Portões e percurso escritos aqui, não levados ao kit.** Os portões do Wild Pack dependem do
   `GameManager` e do `RandomGateCourse` dele (ficaram fora do kit pela K1). Levá-los ao kit agora
   exigiria desmontar o fluxo do Wild Pack e passar pelo aceite dele — grande e arriscado para um
   protótipo de uma semana. Aqui a regra é diferente (portões que agem sobre um fluxo de unidades no
   assalto), então o código é pequeno (`GateMath`, `RunGateCourse`) e a comparação para a pauta da
   arquitetura do kit (seção 5.1) fica anotada na seção 4.
3. **Nada de colisores nem física por quadro.** Portões, muralha, torre e explosão do canhão são
   checagens de distância ao longo da faixa (riscos R-D2 e R-D4). O validador do jogo reprova
   `Instantiate`, `Destroy`, `new GameObject`, `AddComponent` e `Physics.` no código de runtime.
4. **Sons placeholder sintetizados.** O plano dizia "placeholders de som já existentes do kit", mas o
   kit 0.1.0 só tem o motor de som; os clipes ficaram no Wild Pack como conteúdo e não podem ser
   copiados. Os placeholders são gerados por código (`PlaceholderAudio`), sem fonte de terceiros e
   sem licença a controlar; trocados na RT-8.
5. **ID de app de exemplo do AdMob.** O SDK de anúncios de que o kit depende derruba o app na
   abertura sem um ID de app; o protótipo não mostra anúncio. Usa-se o ID público de exemplo do
   Google (`ca-app-pub-3940256099942544~3347511713`), que não é conta do estúdio. IDs reais só na RT-7.
6. **IL2CPP + ARM64**, retrato, Android 8 (API 26)+, application ID que já estava no projeto
   (`com.jungledegree.runnervstower`); o nome do app é o nome de trabalho "Claybreak" (D1). Build
   assinada com a chave de depuração; nada sobe para lugar nenhum.
7. **Medição como no Wild Pack:** APK de medição = release com `RELEASE_MEASUREMENT` (lê `--ez stress`
   e o `--ei fps` do kit e liga o `DevFrameTimingProbe` do kit). Script `Tools/rt2-gate.sh` usa o
   `stage7-measure.sh` do kit.
8. **"Multidão" mínima para o gatilho de parada do R3** (`riscos-e-decisoes.md`): **150 unidades em
   voo** [I] (metade do pool). Se o gate só passar abaixo disso, o R3 dispara e o RT-3 decide.

## 3. O que foi cortado (e por quê)

- **Obstáculos, armadilhas e moedas na corrida**: ficaram no Wild Pack (fora do kit); não são o
  que o protótipo testa (o gancho é o assalto). Voltam com a fatia vertical (RT-4) via kit.
- **Geração de percurso por alvo** (`RandomGateCourse`): pares fixos por dado bastam para 1 fase.
- **Composição da multidão** (unidades especiais colhidas na pista, metade do gancho A): fica para a
  RT-4/RT-5; o protótipo testa só quantidade → assalto.
- **Estrelas, moeda, upgrades, menu, anúncios, save**: fora do escopo da RT-2.
- **Arte**: tudo em blocos de cor (exceto os bonecos de massinha do kit). Fontes legadas do Unity.
- **Andares da torre e outros tipos de defesa** (varredor, escudo): RT-6.
- **Clipes de áudio reais**: placeholders sintetizados (item 2.4).

## 4. Pauta do kit (arquitetura do studiokit, seção 5.1)

Comparação com o Wild Pack, para a hora de dar API às "regras da partida":
- **Portão que dispara ação**: aqui o mesmo portão age de dois jeitos — sobre a contagem (corrida)
  e sobre um fluxo de unidades, uma a uma (assalto). Uma API comum precisa dos dois modos.
- **Condição de vitória**: vitória é um evento externo à multidão (torre zerou), não a chegada.
- **Inimigo persistente**: a torre e as defesas vivem fora do percurso, com estado próprio.
- **Fase do kit**: o assalto é `Playing` para o kit; o líder fica parado com velocidade de percurso 0
  (`SetCourseSpeedMultiplier(0)`) e o arraste dele vira a mira — funcionou sem mudar o kit.
- **Gancho para o kit [I]**: `RunnerCrowd` não tem "tirar um seguidor sem soltá-lo no pool"; o jogo
  usa `RemoveHitFollower` + `RunnerPool.TryGet` (pega a mesma unidade de volta). Funciona, mas um
  `TryDetachFollower` seria mais claro se o segundo uso aparecer.

## 5. Validação feita nesta sessão

Resultados na seção 7. Desempenho **não** é julgado no Editor: só o gate 3× no OPPO aprova.

## 6. ACEITE PENDENTE — o que só dá para fazer com o OPPO CPH2791

O aparelho não estava conectado (`adb devices` vazio em 2026-11-20). Passo a passo, no PC do
estúdio, com o OPPO ligado por USB e a depuração USB ativa:

1. **Gerar os APKs** (Editor do Unity **fechado**), no PowerShell:
   ```powershell
   $unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe'
   & $unity -batchmode -quit -buildTarget Android -projectPath C:\projects\games\RunnerVsTower -executeMethod Claybreak.Editor.ClaybreakBuild.BuildAndroidAndQuit -claybreakMeasure -claybreakApk C:\temp\rt2\Claybreak-measure.apk -logFile C:\temp\rt2\build-measure.log
   ```
   Esperado: `C:\temp\rt2\Claybreak-measure.apk` e a linha `[ClaybreakBuild] PASS` no log.
   (Se o APK desta sessão ainda estiver em `C:\temp\rt2\`, pode pular este passo.)
2. **Conferir o aparelho:** `adb devices` deve listar o OPPO como `device`.
3. **Jogar a fase** (primeira impressão): `adb install -r C:\temp\rt2\Claybreak-measure.apk` e abrir
   "Claybreak" no celular. Jogar 3 vezes. Observar: a fase dura entre 30 e 90 s? dá para escolher a
   faixa arrastando e soltar segurando? o canhão avisa antes de cair? a muralha e os portões do
   assalto pesam na escolha? dá vontade de apertar "AGAIN"?
   A duração de cada partida aparece no log: `adb logcat -d -s Unity | findstr Claybreak`
   (linha `Level WON/LOST in N s`).
4. **Gate 3×** (Git Bash, na raiz do repositório): `Tools/rt2-gate.sh oppo-60 60 20` e depois
   `Tools/rt2-gate.sh oppo-120 120 20`. O app abre sozinho no nível do gate (300 em voo, canhão no
   limite de 12 pedras); não toque na tela. Cada execução grava `C:\temp\rt2\gate-<rótulo>.txt`.
5. **O que conferir no resultado:** linha `rt2-oppo-60: ... p95=..ms ... >50ms=..% ... PSS=..MB`
   (p95 ≤ 33,3 ms; > 50 ms abaixo de 5%; PSS ≤ 450 MB); linhas `[DevTiming] window=...`
   (`main p95` ≤ 11 ms e `gpu p95` ≤ 11 ms); linhas `[ClaybreakGate]` com `inFlight=300` e
   `projectiles=12/12` (ou perto) durante a janela.
6. **Se falhar:** baixar por dado, nesta ordem, no `Assets/Claybreak/Prototype/Data/DeviceGateLevel.asset`
   (e no nível jogado, se o limite for menor que o dele): `maximumUnitsInFlight` (300 → 250 → 200 →
   150) e `maximumProjectiles` (12 → 8 → 4); repetir os passos 1 e 4 e registrar o limite que passou.
   Abaixo de 150 em voo dispara o gatilho de parada do R3.
7. **Onde anotar:** seção 8 deste arquivo (tabela abaixo) e a linha "Status" da RT-2 no
   `plano-de-entregas.md`; commit `docs: RT-2 aceite no aparelho`.

## 7. Resultado da validação (sessão de 2026-11-20) [F]

Unity 6000.6.7f1 em batchmode, alvo Android, numa cópia do projeto em `C:\temp\rt2\proj` (o Editor
estava aberto neste projeto e não foi fechado; o repositório recebeu de volta só os arquivos gerados
e ficou idêntico à cópia validada, exceto dois assets de URP que o batchmode só reserializou).

| Verificação | Resultado |
|---|---|
| `PrototypeSceneBuilder.BuildAndQuit` (cena, dados, sons, materiais, pool de 300, 32 pedras) | PASS |
| `ClaybreakValidation.RunAndQuit` (níveis, tema, cena, pools, sem Instantiate/Destroy/física no runtime) | PASS |
| `EngineIsolationValidation.RunAndQuit` do kit, com a lista deste jogo | PASS (sem falha nova) |
| Testes EditMode (9 do kit + 15 do jogo) | 24/24 |
| Testes PlayMode (cena real: espera o 1º toque; nível do gate chega a 300 em voo e 12/12 pedras e se mantém) | 2/2 |
| APK de medição (`RELEASE_MEASUREMENT`) `C:\temp\rt2\Claybreak-measure.apk` | gerado, 34 MB |
| APK do protótipo `C:\temp\rt2\Claybreak.apk` | gerado, 34 MB |
| Manifesto (aapt) | `com.jungledegree.runnervstower`, minSdk 26, arm64-v8a, `UnityPlayerGameActivity` |

Os APKs ficam fora do repositório (`*.apk` no `.gitignore`); se sumirem de `C:\temp\rt2`, o passo 1
da seção 6 gera de novo.

## 7.1 Correção depois do primeiro teste no aparelho (2026-11-20) [F]

- **Bug:** ao tocar para começar, os bonecos caíam da pista e o jogo parecia travado até o assalto.
  Causa: o construtor tirava o colisor de todos os blocos de cenário, inclusive da pista; o líder do
  kit é um `CharacterController` com gravidade e caía no vazio (a multidão e a câmera junto), enquanto
  o avanço em z seguia disparando portões e o assalto. Os testes do Editor não jogavam a corrida.
- **Correção:** a pista mantém um `BoxCollider` (único colisor da cena); o validador reprova pista
  sem colisor ou líder fora dela; novo teste PlayMode joga a corrida inteira e exige o líder sobre a
  pista até a linha de tiro, com exército > 0 e corrida de 10–40 s. Logs `[ClaybreakGate] ... run`
  durante a corrida e `Run reached the firing line` no fim dela.
- **No OPPO, APK corrigido:** corrida com o líder em y = 0,08 o tempo todo e o exército mudando nos
  portões (caminho descuidado 10→20→60→45→22, perdeu em 22,6 s; caminho bom 10→20→60→80→160,
  **venceu em 28,4 s**, abaixo do piso de 30 s). Ajuste por dado depois disso: corrida a 5 m/s,
  lançamento 12/s, torre 250 (estimativa ≈ 33 s, com folga de ~50 acertos para vencer). **A duração
  com esse ajuste não foi medida no aparelho** (a tela do celular bloqueou): confirmar na partida do
  usuário (`adb logcat -d -s Unity | findstr Claybreak`, linha `Level WON/LOST in N s`).

## 8. Resultado no aparelho (preencher)

| Data | APK | Teste | p95 quadro | >50 ms | main p95 | gpu p95 | PSS | em voo | pedras | Passou? |
|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-20 | measure (`bb8f1ff`) | gate 60 fps | 16 ms | 0,0% | 4,3–6,2 ms | 3,8–4,3 ms | 190 MB | 299 | 11–12/12 | **Sim** |
| 2026-11-20 | measure (`bb8f1ff`) | gate 120 fps | 16 ms (max 33) | 0,0% | 4,3–5,3 ms | 3,8–4,2 ms | 190 MB | 299 | 11–12/12 | **Sim** (ver nota) |

Nota: no teste "120 fps" o aparelho rodou a ~62 fps (avgFPS 62,4); o pedido de 120 Hz não foi atendido
pelo app ou pelo modo de tela. Com main e gpu p95 abaixo de 6,2 ms contra o limite de 11 ms, a folga de 3×
vale para 60 fps; 120 fps real fica para conferir na RT-4. Saídas completas: `C:\temp\rt2\gate-oppo-60.txt`
e `C:\temp\rt2\gate-oppo-120.txt` (fora do repositório).

Primeira impressão do usuário (3 partidas: duração, vitória/derrota, "de novo?"):

-
