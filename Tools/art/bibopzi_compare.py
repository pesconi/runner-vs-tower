"""Side-by-side of the Bibopzi reference drawing ("Feijao", column A) and the game's shape (RT-6t.22).

Usage: python -X utf8 Tools/art/bibopzi_compare.py <capture folder> <reference png> <out folder>
  capture folder: where BibopziCaptureTests wrote bibopzi-perto.png and bibopzi-corrida-300.png
  reference png:  the character sheet (personagem repo / Drive 03-design/bibopzi_direcoes-2d_v2.png, 1500 px wide)
Writes bibopzi-vs-referencia.png (reference | front | 3/4 | 3/4 from behind) and bibopzi-tamanho-de-jogo.png (the
crowd of 300 cut around the leader at 1x and enlarged 3x, nearest neighbour, to judge the gear and the spring).
"""
import os
import sys

from PIL import Image, ImageDraw

PANEL = 600


def main(captures, reference, out):
    os.makedirs(out, exist_ok=True)
    sheet = Image.open(reference).convert("RGB")
    scale = sheet.width / 1500.0
    ref = sheet.crop([int(v * scale) for v in (60, 130, 460, 580)])
    ref = ref.resize((PANEL * ref.width // ref.height, PANEL), Image.LANCZOS)
    close = Image.open(os.path.join(captures, "bibopzi-perto.png")).convert("RGB")
    panels = [ref] + [close.crop((i * (PANEL + 8), 0, i * (PANEL + 8) + PANEL, PANEL)) for i in range(3)]
    labels = ["referencia (Feijao, coluna A)", "jogo: frente", "jogo: 3/4", "jogo: 3/4 de costas (camera da corrida)"]  # ASCII: PIL default font
    width = sum(p.width for p in panels) + 8 * (len(panels) - 1)
    result = Image.new("RGB", (width, PANEL + 36), "white")
    x = 0
    draw = ImageDraw.Draw(result)
    for panel, label in zip(panels, labels):
        result.paste(panel, (x, 36))
        draw.text((x + 8, 10), label, fill=(30, 30, 30))
        x += panel.width + 8
    result.save(os.path.join(out, "bibopzi-vs-referencia.png"))

    run = Image.open(os.path.join(captures, "bibopzi-corrida-300.png")).convert("RGB")
    first = run.crop((0, 0, 540, 1177))
    # The crowd sits in the middle of the run panel (as in docs/art/rt6t20/formas-corrida.png).
    cut = first.crop((110, 420, 430, 680))
    zoom = cut.resize((cut.width * 3, cut.height * 3), Image.NEAREST)
    game = Image.new("RGB", (cut.width + 8 + zoom.width, zoom.height), "white")
    game.paste(cut, (0, 0))
    game.paste(zoom, (cut.width + 8, 0))
    game.save(os.path.join(out, "bibopzi-tamanho-de-jogo.png"))


if __name__ == "__main__":
    if len(sys.argv) != 4:
        sys.exit(__doc__)
    main(*sys.argv[1:])
