# Licenças de áudio e fonte — Clay Siege (RT-8)

Registro de toda mídia de terceiros ou própria que vai no build. Regra (plano, RT-8): só fontes CC0
ou feitas por nós, nada pago, cada arquivo listado aqui. O validador do jogo
(`ClaybreakValidation` → `ClaybreakAudio.TryValidateLicensed`) falha se um clipe usado pelo jogo não
estiver nesta lista ou se houver arquivo de áudio sem uso na pasta.

## 1. Áudio — feito pela Jungle Degree (obra própria)

- **Origem:** síntese por código, sem amostras e sem áudio de terceiros:
  [`Tools/audio/clay_siege_audio.py`](../Tools/audio/clay_siege_audio.py) (Python + numpy/scipy,
  sementes fixas: o mesmo script grava sempre os mesmos arquivos). Gerado em 2026-11-20.
- **Licença:** obra da Jungle Degree, todos os direitos do estúdio. Nenhuma atribuição a terceiros é
  devida. Os créditos do jogo (tela de ajustes) dizem "MUSIC AND SOUND: JUNGLE DEGREE".
- **Formato no repositório:** WAV mono 44,1 kHz 16 bits em `Assets/Claybreak/Content/Audio/`; no
  build, Vorbis (efeitos: descompactados ao carregar, qualidade 0,7; músicas: compactadas na memória,
  qualidade 0,45), definido por `Assets/Claybreak/Editor/ClaybreakAudio.cs`.
- **Substitui:** os sons sintetizados provisórios da RT-2/RT-4 (`PlaceholderAudio.cs`, removido).

| Arquivo | Papel no jogo | Onde toca |
|---|---|---|
| `Coin.wav` | moeda (papel `Coin` do kit) | compra na loja |
| `GateGood.wav` | portão bom | corrida, ao passar por portão que soma/multiplica |
| `GateBad.wav` | portão ruim | corrida, portão que subtrai/divide |
| `UnitLoss.wav` | unidade perdida (estalo de argila) | assalto: pedra, muro, escudo |
| `Finish.wav` | chegada à linha de tiro (trompa + tambor) | fim da corrida |
| `Victory.wav` | vitória (fanfarra) | resultado, vitória |
| `Defeat.wav` | derrota (trombone triste) | resultado, derrota |
| `ButtonClick.wav` | toque de botão | menus |
| `ButtonBack.wav` | voltar | botões BACK |
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
| `MenuLoop.wav` | música do menu, 96 BPM, 20 s em laço | menus |
| `RunLoop.wav` | música da corrida e do assalto, 128 BPM, 15 s em laço | jogo |

## 2. Fontes

| Arquivo | Fonte | Licença | Origem | Observação |
|---|---|---|---|---|
| `Assets/Claybreak/Content/Fonts/Roboto-Black.ttf` | Roboto Black 1.100141 (2013), © Google 2012 | Apache License 2.0 (texto em `Roboto-LICENSE.txt`, ao lado; a licença também está gravada na própria fonte) | cópia sem modificação da Roboto que vem no pacote `com.unity.searcher` da Unity (nada baixado) | SHA-256 `b6a38ddfb6b7d92a644da3a175cab3858438b3c791486aeeca2094a611430f27`. Créditos na tela de ajustes: "FONT: ROBOTO (APACHE 2.0)". |

A Apache 2.0 permite uso comercial e redistribuição desde que a licença acompanhe o arquivo
(cumprido pelo `Roboto-LICENSE.txt`) e que mudanças sejam indicadas (não houve mudança).

## 3. O que não entra

- Nada do Wild Pack (sons e fontes dele são conteúdo daquele jogo; regra do CLAUDE.md).
- Nada pago e nenhum pacote baixado de loja de assets.
- Trocar ou acrescentar um som: editar o script, rodar
  `python Tools/audio/clay_siege_audio.py`, acrescentar a linha nesta tabela e reconstruir a cena.
