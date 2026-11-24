# Loja (Google Play) — Stampeep

Material da RT-9a (2026-11-24). **Nada foi enviado ao Play Console nem ao AdMob**: quem envia é o usuário, na RT-9b,
seguindo [`passo-a-passo-console.md`](passo-a-passo-console.md).

| O quê | Arquivo | Limite da Play hoje | Situação |
|---|---|---|---|
| Nome, descrição curta, descrição longa (en-US) | [`ficha/en-US.md`](ficha/en-US.md) | 30 / 80 / 4.000 caracteres | 25 / 77 / 1.245 (`Tools/store/check_listing.py`: PASS) |
| Ícone | [`graficos/icone-512.png`](graficos/icone-512.png) | 512×512 PNG 32 bits, até 1.024 KB | 512×512 RGBA, ~73 KB; é também o ícone do celular (T-RT9a-4) |
| Gráfico de destaque | [`graficos/destaque-1024x500.png`](graficos/destaque-1024x500.png) | 1024×500 JPEG/PNG 24 bits, sem alfa | 1024×500 RGB, ~160 KB |
| Capturas de telemóvel | [`graficos/capturas/`](graficos/capturas/) | 2–8; ≥ 3 em 9:16 ≥ 1080×1920 para promoção de jogos | 6 × 1080×1920 (9:16) |
| Declarações (Segurança dos dados, IARC, público, anúncios, acesso, ID de publicidade) | [`declaracoes.md`](declaracoes.md) | — | respostas com evidência do código |
| Política de privacidade | [`../politica-de-privacidade.md`](../politica-de-privacidade.md) + [`site/stampeep/privacy/index.html`](site/stampeep/privacy/index.html) | URL pública https | falta preencher os dados da empresa e publicar (passo 1) |
| Regras vigentes da Play (fontes e datas) | [`requisitos-play.md`](requisitos-play.md) | — | pesquisa de 2026-11-24 |
| Passo a passo da RT-9b | [`passo-a-passo-console.md`](passo-a-passo-console.md) | — | para o usuário |

## Capturas (ordem da loja)

1. `01-army-of-300` — mundo 4, fase do chefe: a manada no teto de 300, portão ×3.
2. `02-pick-the-gate` — mundo 2: ×2 ou +23, a escolha de perto.
3. `03-dodge-the-traps` — mundo 3: tronco de espinhos a cortar a multidão.
4. `04-storm-the-tower` — mundo 3: o exército lançado contra a torre e as defesas.
5. `05-crowd-shapes` — loja, aba SHAPES.
6. `06-home` — tela inicial (mundo 2, fase 4).

Todas são quadros desenhados pelo próprio jogo, de fases reais jogadas a partir do botão PLAY, sem retoque nem texto
por cima. Conferidas: nada cortado, nenhum "Clay Siege", nenhuma marca de terceiros.

## Como refazer

```text
Unity -batchmode -buildTarget Android -projectPath <projeto> -runTests -testPlatform PlayMode \
  -testFilter Claybreak.Tests.StoreCaptureTests -storeCaptureDir C:/temp/rt9a/cap2      (com GPU: sem -nographics)
python -X utf8 Tools/store/make_store_art.py C:/temp/rt9a/cap2      (capturas, destaque, ícone da loja e do celular)
python -X utf8 Tools/store/check_listing.py                          (limites e palavras da ficha)
python -X utf8 Tools/store/make_privacy_html.py [--check]            (página da política)
python Tools/store/check_16kb.py <apk ou aab>                         (bibliotecas prontas para páginas de 16 KB)
```

Se o jogo mudar, os quadros escolhidos podem mudar: olhar a pasta da captura e atualizar `PICKS`/`ICON_CROP` em
`make_store_art.py`. Depois de gerar o ícone, reconstruir a cena/build (o `ProjectConfiguration` aplica o ícone).
