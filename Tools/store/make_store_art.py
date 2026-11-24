"""RT-9a: Google Play graphics of Stampeep, made only from frames the game drew (StoreCaptureTests), no mock-ups.

Input: the folder written by
  Unity -batchmode -runTests -testPlatform PlayMode -testFilter Claybreak.Tests.StoreCaptureTests -storeCaptureDir <folder>
(GPU, no -nographics). Frame names are stable (world-level-index-phase), so the picks below repeat on a new capture
of the same code; if the game changes, look at the new frames and update PICKS / ICON_CROP.

Output:
  docs/store/graficos/capturas/0N-*.png   phone screenshots, 1080x1920 (9:16), RGB
  docs/store/graficos/destaque-1024x500.png  feature graphic, RGB (no alpha), game frame + the game's title font
  docs/store/graficos/icone-512.png        store icon, 512x512 32-bit PNG (opaque)
  Assets/Claybreak/Content/Icon/AppIcon.png  launcher icon source, 1024x1024 (same picture; ProjectConfiguration)

Usage: python -X utf8 Tools/store/make_store_art.py <capture folder>
"""
import pathlib
import sys

from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = pathlib.Path(__file__).resolve().parents[2]
OUT = ROOT / "docs" / "store" / "graficos"
FONT = ROOT / "Assets" / "Claybreak" / "Content" / "Fonts" / "Baloo2-ExtraBold.ttf"
APP_ICON = ROOT / "Assets" / "Claybreak" / "Content" / "Icon" / "AppIcon.png"

# Order = order in the store. Each is a real frame of a real level played from the PLAY button (mid-game save).
PICKS = [
    ("01-army-of-300", "w4-l10-04-run"),        # world 4 boss level: the stampede at the 300 cap, x3 gate taken
    ("02-pick-the-gate", "w2-l4-04-run"),       # world 2: x2 or +23, the choice up close
    ("03-dodge-the-traps", "w3-l8-12-run"),     # world 3: a swinging spiked log cuts the crowd
    ("04-storm-the-tower", "w3-l8-22-assault"), # world 3: the army streams at the tower and its defenses
    ("05-crowd-shapes", "menu-loja-formas"),    # shop, SHAPES tab
    ("06-home", "menu-home"),                   # home: world 2, level 4
]
FEATURE_SOURCE = "wide-w3-l8-run"   # 2048x1000, HUD hidden: yellow crowd, x2 / +53 gates, crate, world 3 green
ICON_SOURCE = "square-w1-l6-run"    # 1024x1024, HUD hidden: pink crowd with its crowned leader, x2 gate
ICON_CROP = (200, 330, 894, 1024)    # square box around the crowd and the x2 gate

TITLE_FILL = (247, 160, 58)         # the home title's orange (UiColors) and its dark outline
TITLE_OUTLINE = (92, 52, 40)


def load(folder, name):
    path = folder / f"{name}.png"
    if not path.exists():
        raise SystemExit(f"missing frame {path}: capture again or update the picks")
    return Image.open(path).convert("RGB")


def screenshots(folder):
    target = OUT / "capturas"
    target.mkdir(parents=True, exist_ok=True)
    for old in target.glob("*.png"):
        old.unlink()
    for name, frame in PICKS:
        image = load(folder, frame)
        if image.size != (1080, 1920):
            raise SystemExit(f"{frame} is {image.size}, expected 1080x1920")
        image.save(target / f"{name}.png", optimize=True)


def cover(image, width, height):
    scale = max(width / image.width, height / image.height)
    resized = image.resize((round(image.width * scale), round(image.height * scale)), Image.LANCZOS)
    left = (resized.width - width) // 2
    top = (resized.height - height) // 2
    return resized.crop((left, top, left + width, top + height))


def title(draw, center, text, size, stroke):
    font = ImageFont.truetype(str(FONT), size)
    box = draw.textbbox((0, 0), text, font=font, stroke_width=stroke)
    x = center[0] - (box[2] - box[0]) // 2 - box[0]
    y = center[1] - (box[3] - box[1]) // 2 - box[1]
    draw.text((x, y), text, font=font, fill=TITLE_FILL, stroke_width=stroke, stroke_fill=TITLE_OUTLINE)


def feature(folder):
    base = cover(load(folder, FEATURE_SOURCE), 1024, 500)
    # A soft cream band on the left third, so the name reads over the trees (the game's own panel color).
    band = Image.new("RGBA", base.size, (0, 0, 0, 0))
    ImageDraw.Draw(band).rounded_rectangle((36, 150, 420, 350), radius=40, fill=(255, 247, 238, 215))
    band = band.filter(ImageFilter.GaussianBlur(1.5))
    image = Image.alpha_composite(base.convert("RGBA"), band)
    draw = ImageDraw.Draw(image)
    title(draw, (228, 228), "STAMPEEP", 82, 6)
    font = ImageFont.truetype(str(FONT), 30)
    sub = "STORM THE TOWER"
    box = draw.textbbox((0, 0), sub, font=font)
    draw.text((228 - (box[2] - box[0]) // 2, 292), sub, font=font, fill=(92, 72, 98))
    image.convert("RGB").save(OUT / "destaque-1024x500.png", optimize=True)


def icon(folder):
    crop = load(folder, ICON_SOURCE).crop(ICON_CROP)
    big = crop.resize((1024, 1024), Image.LANCZOS)
    APP_ICON.parent.mkdir(parents=True, exist_ok=True)
    big.save(APP_ICON, optimize=True)
    big.resize((512, 512), Image.LANCZOS).convert("RGBA").save(OUT / "icone-512.png", optimize=True)


def main():
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    folder = pathlib.Path(sys.argv[1])
    OUT.mkdir(parents=True, exist_ok=True)
    screenshots(folder)
    feature(folder)
    icon(folder)
    for path in sorted(OUT.rglob("*.png")):
        with Image.open(path) as image:
            print(f"{path.relative_to(ROOT)}: {image.size[0]}x{image.size[1]} {image.mode} "
                  f"{path.stat().st_size // 1024} KB")


if __name__ == "__main__":
    main()
