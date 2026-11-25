"""RT-6v3.2: comparison sheets of the vinyl directions for decision D36.

Usage (repository root):
    python -X utf8 Tools/art/vinyl_sheets.py <concept folder> [antes=docs/art/v3/antes] [out=docs/art/v3/alvo]

<concept folder> is what VinylConceptTests wrote (-vinylConceptDir): one subfolder per direction (A, B, C and the mix M) with the
framings of StyleCaptureRig plus the "toy factory" tower and three icon sketches. The script copies each direction's
captures to <out>/<id>/ and writes the sheets the D36 card shows:
    comparar-corrida.png   before | A | B | C | M, the run with 300
    comparar-assalto.png   before | A | B | C | M, the heaviest assault with the world's tower
    comparar-home.png      before | A | B | C | M, the home screen (the 1080x2400 panel of the four shapes)
    comparar-lider.png     before / A / B / C / M, the default shape up close (three angles each)
    torres.png             per direction: the world's tower and the toy factory tower, far and near
    icones.png             per direction: icon sketches 1 (face in front), 2 (crowd from behind), 3 (back, tower)
Pure Pillow; no network.
"""
import shutil
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
FONT = ROOT / "Assets/Claybreak/Content/Fonts/Baloo2-ExtraBold.ttf"
DIRECTIONS = ["A", "B", "C", "M"]
NAMES = {
    "antes": "Hoje (antes)",
    "A": "A - vinil brilhante",
    "B": "B - pastel fosco",
    "C": "C - fim de tarde",
    "M": "M - mistura",
}
INK = (59, 47, 74)
PAPER = (255, 255, 255)
GAP = 12
LABEL = 64


def font(size):
    return ImageFont.truetype(str(FONT), size) if FONT.exists() else ImageFont.load_default()


def labelled(image, text, size=40):
    """The image with a label band on top."""
    out = Image.new("RGB", (image.width, image.height + LABEL), PAPER)
    out.paste(image.convert("RGB"), (0, LABEL))
    draw = ImageDraw.Draw(out)
    f = font(size)
    width = draw.textlength(text, font=f)
    draw.text(((image.width - width) / 2, 6), text, fill=INK, font=f)
    return out


def row(images):
    height = max(i.height for i in images)
    out = Image.new("RGB", (sum(i.width for i in images) + GAP * (len(images) - 1), height), PAPER)
    x = 0
    for image in images:
        out.paste(image, (x, 0))
        x += image.width + GAP
    return out


def column(images):
    width = max(i.width for i in images)
    out = Image.new("RGB", (width, sum(i.height for i in images) + GAP * (len(images) - 1)), PAPER)
    y = 0
    for image in images:
        out.paste(image, (0, y))
        y += image.height + GAP
    return out


def home_panel(path):
    """The third panel (1080x2400 at half size) of the four-shape home sheet, trimmed of the white below it."""
    sheet = Image.open(path).convert("RGB")
    panel_width = 540
    gutter = (sheet.width - 4 * panel_width) // 3
    x = 2 * (panel_width + gutter)
    return sheet.crop((x, 0, x + panel_width, 1200))


def rounded(image, radius_share=0.22):
    mask = Image.new("L", image.size, 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, image.width - 1, image.height - 1), int(image.width * radius_share), fill=255)
    out = Image.new("RGB", image.size, PAPER)
    out.paste(image.convert("RGB"), (0, 0), mask)
    return out


def save(image, path):
    image.save(path, optimize=True)
    print("wrote", path.relative_to(ROOT) if path.is_relative_to(ROOT) else path)


def main():
    concept = Path(sys.argv[1])
    antes = Path(sys.argv[2]) if len(sys.argv) > 2 else ROOT / "docs/art/v3/antes"
    out = Path(sys.argv[3]) if len(sys.argv) > 3 else ROOT / "docs/art/v3/alvo"
    out.mkdir(parents=True, exist_ok=True)
    for d in DIRECTIONS:
        target = out / d
        target.mkdir(exist_ok=True)
        for png in sorted((concept / d).glob("*.png")):
            shutil.copyfile(png, target / png.name)

    def source(d, name):
        return (antes if d == "antes" else concept / d) / name

    for framing, sheet in (("corrida-300.png", "comparar-corrida.png"), ("assalto-300.png", "comparar-assalto.png")):
        save(row([labelled(Image.open(source(d, framing)), NAMES[d]) for d in ["antes"] + DIRECTIONS]), out / sheet)
    save(row([labelled(home_panel(source(d, "telas.png")), NAMES[d]) for d in ["antes"] + DIRECTIONS]), out / "comparar-home.png")
    save(column([labelled(Image.open(source(d, "lider-perto.png")), NAMES[d]) for d in ["antes"] + DIRECTIONS]), out / "comparar-lider.png")

    towers = []
    for d in DIRECTIONS:
        cells = [
            labelled(Image.open(source(d, "torre-perto.png")), f"{d}: tema do mundo", 34),
            labelled(Image.open(source(d, "torre-perto-fabrica.png")), f"{d}: fabrica de brinquedo", 34),
            labelled(Image.open(source(d, "assalto-300-fabrica.png")), f"{d}: assalto, fabrica", 34),
        ]
        towers.append(row(cells))
    save(column(towers), out / "torres.png")

    icons = []
    captions = ["1: rosto em destaque", "2: multidao de costas", "3: de costas + torre"]
    for d in DIRECTIONS:
        icons.append(row([labelled(rounded(Image.open(source(d, f"icone-{i}.png"))), f"{d}{captions[i - 1]}", 30) for i in (1, 2, 3)]))
    save(column([labelled(Image.open(antes / "icone.png"), "Icone de hoje", 30)] + icons), out / "icones.png")


if __name__ == "__main__":
    main()
