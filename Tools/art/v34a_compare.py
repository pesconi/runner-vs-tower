"""RT-6v3.4a: the new Bibopzi against the D36 target, side by side (target A | before | now).

Usage: python -X utf8 Tools/art/v34a_compare.py <before folder> <out folder>
  before folder: docs/art/v3/telas as it was before the delivery (git show / a copy)
Reads docs/art/v3/alvo/A (target) and docs/art/v3/telas (now, written by the 'style' step).
Writes comparar-corrida.png, comparar-assalto.png (3 columns each) and comparar-lider.png (3 rows).
"""
import os
import sys

from PIL import Image, ImageDraw

TARGET = "docs/art/v3/alvo/A"
NOW = "docs/art/v3/telas"


def labelled(images, labels, vertical=False):
    gap, bar = 8, 30
    if vertical:
        w = max(i.width for i in images)
        h = sum(i.height + bar for i in images) + gap * (len(images) - 1)
        out = Image.new("RGB", (w, h), "white")
        y = 0
        for img, label in zip(images, labels):
            ImageDraw.Draw(out).text((6, y + 8), label, fill="black")
            out.paste(img.convert("RGB"), (0, y + bar))
            y += img.height + bar + gap
        return out
    w = sum(i.width for i in images) + gap * (len(images) - 1)
    h = max(i.height for i in images) + bar
    out = Image.new("RGB", (w, h), "white")
    x = 0
    for img, label in zip(images, labels):
        ImageDraw.Draw(out).text((x + 6, 8), label, fill="black")
        out.paste(img.convert("RGB"), (x, bar))
        x += img.width + gap
    return out


def main(before, out):
    os.makedirs(out, exist_ok=True)
    labels = ["alvo A (D36)", "antes (RT-6v3.3, receita 490 tri)", "agora (RT-6v3.4a, multidao 1150 tri + lider detalhado)"]
    for name, target in (("corrida-300.png", "corrida-300.png"), ("assalto-300.png", "assalto-300.png")):
        imgs = [Image.open(os.path.join(TARGET, target)), Image.open(os.path.join(before, name)), Image.open(os.path.join(NOW, name))]
        labelled(imgs, labels).save(os.path.join(out, "comparar-" + name.split("-")[0] + ".png"))
    imgs = [Image.open(os.path.join(TARGET, "lider-perto.png")), Image.open(os.path.join(before, "lider-perto.png")),
            Image.open(os.path.join(NOW, "lider-perto.png"))]
    labelled(imgs, labels, vertical=True).save(os.path.join(out, "comparar-lider.png"))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
