"""RT-6t.2: puts the Unity shape captures next to the three.js reference renders (Tools/rt6t/shape-captures.sh).

Usage: python -X utf8 compose_shapes.py <capture folder with unity/ and proto/> <output folder>
Writes formas-closeup.png (4 shapes x 3 views, reference | Unity) and formas-multidao.png (100 units per shape).
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont

VARIANTS = [
    ("Bolinha", "Bolinha (com olhos)"),
    ("BolinhaNoEyes", "Bolinha sem olhos"),
    ("Pintinho", "Pintinho (com olhos)"),
    ("PintinhoNoEyes", "Pintinho sem olhos"),
]
VIEWS = [("frente", "frente 3/4"), ("lado", "lado"), ("costas", "costas 3/4")]
WHITE = (255, 255, 255)
INK = (59, 47, 74)


def font(size):
    for name in ("arialbd.ttf", "arial.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def load(folder, name):
    path = os.path.join(folder, name)
    if not os.path.exists(path):
        sys.exit("missing " + path)
    return Image.open(path).convert("RGB")


def closeups(root, out):
    tile, gap, pair_gap, label_w, head_h = 360, 6, 28, 250, 86
    pair_w = 2 * tile + gap
    width = label_w + len(VIEWS) * pair_w + (len(VIEWS) - 1) * pair_gap
    height = head_h + len(VARIANTS) * (tile + gap)
    sheet = Image.new("RGB", (width, height), WHITE)
    draw = ImageDraw.Draw(sheet)
    big, small = font(30), font(22)
    for v, (_, view_label) in enumerate(VIEWS):
        x = label_w + v * (pair_w + pair_gap)
        draw.text((x + pair_w / 2, 26), view_label, fill=INK, font=big, anchor="mm")
        draw.text((x + tile / 2, 64), "protótipo (three.js)", fill=INK, font=small, anchor="mm")
        draw.text((x + tile + gap + tile / 2, 64), "Unity (ToonLit)", fill=INK, font=small, anchor="mm")
    for r, (vid, row_label) in enumerate(VARIANTS):
        y = head_h + r * (tile + gap)
        draw.multiline_text((label_w / 2, y + tile / 2), row_label.replace(" ", "\n", 1), fill=INK, font=big,
                            anchor="mm", align="center")
        for v, (view, _) in enumerate(VIEWS):
            x = label_w + v * (pair_w + pair_gap)
            sheet.paste(load(os.path.join(root, "proto"), f"proto-{vid}-{view}.png").resize((tile, tile)), (x, y))
            sheet.paste(load(os.path.join(root, "unity"), f"unity-{vid}-{view}.png").resize((tile, tile)), (x + tile + gap, y))
    sheet.save(os.path.join(out, "formas-closeup.png"), optimize=True)


def crowds(root, out):
    w, h, gap, pair_gap, head_h = 540, 960, 6, 28, 90
    pair_w = 2 * w + gap
    cols = 2
    width = cols * pair_w + (cols - 1) * pair_gap
    height = 2 * (head_h + h) + pair_gap
    sheet = Image.new("RGB", (width, height), WHITE)
    draw = ImageDraw.Draw(sheet)
    big, small = font(34), font(24)
    for i, (vid, label) in enumerate(VARIANTS):
        x = (i % cols) * (pair_w + pair_gap)
        y = (i // cols) * (head_h + h + pair_gap)
        draw.text((x + pair_w / 2, y + 28), label + " - 100 unidades, 9:16", fill=INK, font=big, anchor="mm")
        draw.text((x + w / 2, y + 68), "protótipo (parado na vaga)", fill=INK, font=small, anchor="mm")
        draw.text((x + w + gap + w / 2, y + 68), "Unity (pulando)", fill=INK, font=small, anchor="mm")
        sheet.paste(load(os.path.join(root, "proto"), f"proto-crowd-{vid}.png").resize((w, h)), (x, y + head_h))
        sheet.paste(load(os.path.join(root, "unity"), f"unity-crowd-{vid}.png").resize((w, h)), (x + w + gap, y + head_h))
    sheet.save(os.path.join(out, "formas-multidao.png"), optimize=True)


def main():
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    root, out = sys.argv[1], sys.argv[2]
    os.makedirs(out, exist_ok=True)
    closeups(root, out)
    crowds(root, out)


if __name__ == "__main__":
    main()
