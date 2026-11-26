# Capturas do estilo v3 do jogo (RT-6v3.3 em diante)

Tela atual do jogo nas câmeras fixas do `StyleCaptureRig` (mesmos enquadramentos de `../antes/` e `../alvo/`), regravadas pelo
passo `style` do `Tools/rt4-check.sh` (PlayMode com GPU, `-testFilter Claybreak.Tests.StyleCaptureTests -styleCaptureDir <pasta>`).

| Arquivo | O que é |
|---|---|
| `corrida-300.png`, `corrida-300-m2..m4.png` | corrida com 300 na cor do mundo, mundos 1 a 4 |
| `assalto-300-m1..m3.png`, `assalto-300.png` | assalto do nível de gate (o mais pesado) com 300 em voo, mundos 1 a 4, cor do mundo |
| `torre-perto.png` | torre do chefe do mundo 4 de perto |
| `lider-perto.png` | Bibopzi de perto, três ângulos, material da multidão (vinil) |
| `telas.png` | tela inicial nas 4 proporções, UI desenhada fora do pós (como no celular) |
| `icone.png` | ícone atual 512 |
| `contraste.json` | cópia da medida de contraste (a do validador fica em `../contraste.json`) |

A medida de contraste (regra da D36: multidão com o maior croma, pista neutra longe em valor, defesas na cor complementar,
números dos portões ≥ 3:1, skins a ΔE ≥ 20 da pista) sai da mesma captura e vai para `docs/art/v3/contraste.json`, que o
`StyleGuideValidation` lê; ela carrega a impressão digital do visual e precisa ser refeita quando paleta, luz, pós ou
superfícies mudam. Decisões: `docs/decisoes.md`, RT-6v3.3.
