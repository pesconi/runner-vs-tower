# Licenças de áudio e fonte — Stampeep (RT-8; nome trocado na RT-6t.21, D29; som "brinquedo de corda" na RT-6v3.9)

Registro de toda mídia de terceiros ou própria que vai no build. Regra (plano, RT-8): só fontes CC0
ou feitas por nós, nada pago, cada arquivo listado aqui. O validador do jogo
(`ClaybreakValidation` → `ClaybreakAudio.TryValidateLicensed`) falha se um clipe usado pelo jogo não
estiver nesta lista ou se houver arquivo de áudio sem uso na pasta.

## 1. Áudio — feito pela Jungle Degree (obra própria)

- **Origem:** síntese por código, sem amostras e sem áudio de terceiros, **nada gerado por serviço de IA** (D37 = C):
  [`Tools/audio/clay_siege_audio.py`](../Tools/audio/clay_siege_audio.py) e, desde a RT-6v3.9,
  [`Tools/audio/stampeep_toy_audio.py`](../Tools/audio/stampeep_toy_audio.py) (Python + numpy/scipy,
  sementes fixas: o mesmo script grava sempre os mesmos arquivos). Gerado em 2026-11-20; efeitos de brinquedo e músicas por mundo em 2026-11-26.
  Métricas (pico, volume aproximado, duração) e página de escuta: `python Tools/audio/audio_metrics.py [--page]`.
- **Licença:** obra da Jungle Degree, todos os direitos do estúdio. Nenhuma atribuição a terceiros é
  devida. Os créditos do jogo (tela de ajustes) dizem "MUSIC AND SOUND: JUNGLE DEGREE".
- **Formato no repositório:** WAV mono 44,1 kHz 16 bits em `Assets/Claybreak/Content/Audio/`; no
  build, Vorbis (efeitos: descompactados ao carregar, qualidade 0,7; músicas: compactadas na memória,
  qualidade 0,45), definido por `Assets/Claybreak/Editor/ClaybreakAudio.cs`.
- **Alvos medidos (validador, guia de estilo):** pico -1 dBFS ± 1,5 nos efeitos e -3 ± 1,5 nas músicas; volume aproximado (RMS dos
  trechos acima de -40 dBFS) -15 ± 5 dBFS nos efeitos, -7 ± 3 no aviso de projétil (assobio contínuo) e -17 ± 3 nas músicas;
  duração 0,04–0,25 s nos toques de botão, 0,05–1,5 s nos efeitos, 0,5–3 s nas fanfarras e quedas, 10–25 s nas músicas.
  Clipe fora da lista abaixo ou fora desses alvos reprova no `ClaybreakValidation` (`StyleGuideValidation`).
- **Motivo comum:** 4 notas (graus 1-5-3-8) em todas as músicas, no tom e no timbre de cada mundo.
- **Substitui:** os sons sintetizados provisórios da RT-2/RT-4 (`PlaceholderAudio.cs`, removido).

| Arquivo | Papel no jogo | Onde toca |
|---|---|---|
| `Coin.wav` | moeda (papel `Coin` do kit): sino curto duplo | compra na loja |
| `GateGood.wav` | portão bom: catraca + 3 primeiras notas do motivo em sino curto | corrida, ao passar por portão que soma/multiplica |
| `GateBad.wav` | portão ruim: mola soltando | corrida, portão que subtrai/divide e perda por obstáculo |
| `UnitLoss.wav` | boneco estoura em peças e confete | assalto: pedra, muro, escudo |
| `Finish.wav` | chegada à linha de tiro (trompa + tambor) | fim da corrida |
| `Victory.wav` | vitória (fanfarra) | resultado, vitória |
| `Defeat.wav` | derrota (trombone triste) | resultado, derrota |
| `ButtonClick.wav` | toque de botão (tique de catraca) | menus |
| `ButtonBack.wav` | voltar (tique mais grave) | botões BACK |
| `AdReward.wav` | recompensa (brilho + moeda) | anúncio premiado, presente diário |
| `CueCannonFire.wav` | disparo do canhão (estrondo) | assalto, defesa canhão |
| `CueProjectileWarning.wav` | aviso de projétil (assobio de queda) | assalto, 0,6 s antes da pedra cair |
| `CueStoneImpact.wav` | impacto da pedra no chão (sem vibração: o canhão atira a cada 1,6–3 s) | assalto, onde a sombra marcava |
| `CueTowerHit.wav` | soldado batendo na torre (baque de argila) | assalto, cada golpe |
| `CueFloorBreak.wav` | andar da torre desmoronando | assalto, a cada andar perdido |
| `CueTowerFall.wav` | queda da torre (estrondo + escombros) | vitória, a torre cai |
| `CueWallBreak.wav` | muro/escudo quebrando | assalto |
| `CueSweep.wav` | varredor passando (zunido) | assalto, defesa varredor |
| `CueCrate.wav` | caixote de reforço | corrida, ao pegar o caixote |
| `CueBoss.wav` | chefe (trompas graves + tambores) | entrada do chefe e cada regra de chefe |
| `MenuLoop.wav` | música do menu, 96 BPM, 20 s em laço, motivo em dó maior | menus |
| `CueKeyWind.wav` | chave de corda girando (ticks que desaceleram + clunk) | início de cada fase |
| `CueNumberPop.wav` | número do portão pulando (pop subindo + sino) | portão bom |
| `RunLoop.wav` | música do mundo 1: dó maior, 124 BPM, 15 s em laço, líder em sino | corrida e assalto, mundo 1 |
| `RunLoopW2.wav` | música do mundo 2: lá menor, 132 BPM, 15 s em laço, líder pluck | corrida e assalto, mundo 2 |
| `RunLoopW3.wav` | música do mundo 3: fá maior, 112 BPM com balanço, 17 s em laço, líder metal | corrida e assalto, mundo 3 |
| `RunLoopW4.wav` | música do mundo 4: ré menor, 140 BPM, 14 s em laço, líder onda quadrada | corrida e assalto, mundo 4 |

## 2. Fontes

| Arquivo | Fonte | Licença | Origem | Observação |
|---|---|---|---|---|
| `Assets/Claybreak/Content/Fonts/Baloo2-ExtraBold.ttf` | Baloo 2 ExtraBold 1.700, © 2019 The Baloo 2 Project Authors (Ek Type) | SIL Open Font License 1.1 (texto em `Baloo2-OFL.txt`, ao lado; aviso e licença também gravados na própria fonte, campos 0, 13 e 14 da tabela `name`) | `github.com/google/fonts`, pasta `ofl/baloo2`, commit `c13390277dfb1fcc7415d84ef7ef9cfc1e52f8c1` (baixado em 2026-11-22): `Baloo2[wght].ttf` (variável, peso 400–800), SHA-256 `d47a6852548059b1db49a1319d06d499d546c3fa2237cf9eee9c43c8abb025c2`, e `OFL.txt`, SHA-256 `ad09b05dc8bc678c9daf7c4c5f7ef1f55e5726127f4330b2e98e40b9dffcb860` (o `Baloo2-OFL.txt` é ele, sem mudança) | **Versão modificada** (permitido pela OFL): o repositório só tem a fonte variável; `python -I Tools/fonts/baloo2-extrabold.py <pasta baixada> <saída>` confere os dois hashes de origem e fixa o eixo de peso em 800 (fontTools 4.59, `instancer`), byte a byte reprodutível: SHA-256 `73e6a48f3fce3efdd0dea8f4f444279e87763be97be07310739b56cefe29d3e2`. A fonte não tem "Reserved Font Name", então o nome "Baloo 2" continua. Créditos na tela de ajustes: "FONT: BALOO 2 (SIL OFL 1.1)". |
| `Assets/Claybreak/Content/Fonts/Baloo2-ExtraBold SDF.asset` (+ 4 materiais `Baloo2 *.mat`) | atlas SDF do TextMeshPro gerado da `Baloo2-ExtraBold.ttf` acima (124 caracteres: ASCII, `− × ÷ · …` e acentos do português) | a mesma OFL 1.1 (obra derivada da fonte) | gerado no Editor por `GameFonts` (Claybreak.Editor) a cada reconstrução da cena, se faltar | O teste `GateFeedbackEditTests` confere os dois hashes (fonte e licença) e os glifos. |
| `Assets/TextMesh Pro/**` | "TMP Essential Resources" da Unity (shaders, configurações, LiberationSans SDF de reserva) | licença do pacote `com.unity.ugui` (Unity Companion License) e OFL da LiberationSans (`LiberationSans - OFL.txt`, ao lado) | importado do próprio pacote `com.unity.ugui` 2.6.0 do projeto (nada baixado) | Necessário para o TextMeshPro funcionar; o jogo só desenha com a Baloo 2. |

A OFL 1.1 permite uso comercial, embutir no app e modificar, desde que a licença acompanhe a fonte (cumprido pelo
`Baloo2-OFL.txt` e pela tabela `name` da própria fonte, que vai dentro do app), que a fonte não seja vendida sozinha e que
uma versão modificada não use nome reservado (não há). A Roboto Black (Apache 2.0, RT-8) saiu do jogo na RT-6t.9.

## 3. O que não entra

- Nada do Wild Pack (sons e fontes dele são conteúdo daquele jogo; regra do CLAUDE.md).
- Nada pago e nenhum pacote baixado de loja de assets.
- Trocar ou acrescentar um som: editar o script, rodar
  `python Tools/audio/clay_siege_audio.py` (ele chama o `stampeep_toy_audio.py`), acrescentar a linha nesta tabela e reconstruir a cena.
