"""Bibopzi's face atlas (RT-6v3.4b, direcao-visual-v3.md 3.2 and D36): 8 expressions in one texture for StudioKit/ToonVinyl.

Run (repository root):  python -X utf8 Tools/art/bibopzi_faces.py [--preview <png>]
Writes Assets/Claybreak/Content/Style/BibopziFaces.png (512 x 512, 4 x 2 cells of 128 x 256 px, row 0 at the top) with the same
bytes every run. The Editor (BibopziFaceArt) imports it with mipmaps and puts it on the crowd material; the validator checks
the size, the empty margins and that it was written by this script (FACE_SCRIPT_HASH in the PNG text chunk).

Each cell is the front of the body as projected by Tools/blender/bibopzi.py (FACE_BOX, seen from the front: u grows to the
viewer's right, v up). The box covers the whole body (no masked vertex is ever clamped), so a cell is 0.4 m x 0.7 m: 128 x 256
px keeps about 330-370 px per meter both ways. Only the features are drawn (alpha 1); the rest is transparent, so the body color and the skin tint
show through, and the face itself never takes the tint (D36: "mascara 0 no rosto"). The screw nose stays geometry.
Strokes are at least 7 px of 128 so every expression still reads at 40 px (the crowd and the icon); EditMode tests check
that each pair of cells differs at that size.

Cells (FaceExpression in the game, same order): 0 neutral, 1 happy (good gate), 2 scared (bad gate, obstacle), 3 effort,
4 victory, 5 defeat, 6 blink, 7 "ui!" (launched in the assault).
"""

import hashlib
import io
import math
import os
import sys

import numpy as np
from PIL import Image, PngImagePlugin

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(HERE))
OUT = os.path.join(ROOT, "Assets", "Claybreak", "Content", "Style", "BibopziFaces.png")

CELL_W, CELL_H = 128, 256
COLUMNS, ROWS = 4, 2
SUPER = 4
# Must match Tools/blender/bibopzi.py FACE_BOX (x min, y min, x max, y max, meters, Unity object space).
FACE_BOX = (-0.2, -0.02, 0.2, 0.68)
# Empty band kept on every side of a cell (share of the cell; the material's inset is 0.06).
MARGIN = 0.08

# Landmarks of the head (meters, as in bibopzi.py: BOTTOM 0.03, HEIGHT 0.594).
# Fable review (RT-6v3.4b): the face sits ~0.03 m lower and a little nearer the center than the old geometry eyes, clear
# of the temple gear and of the leader's crown; the screw nose moved down with it (bibopzi.py NOSE_T).
EYE_X, EYE_Y = 0.068, 0.03 + 0.745 * 0.594 - 0.028
EYE_A, EYE_B = 0.056, 0.05
BROW_Y = 0.03 + 0.885 * 0.594 - 0.034
MOUTH_Y = 0.03 + 0.548 * 0.594 - 0.016

INK = (0x2B, 0x22, 0x33)
WHITE = (0xFB, 0xF7, 0xF0)
LINE = (0x1D, 0x2B, 0x3A)
MOUTH_IN = (0x6E, 0x22, 0x36)
TONGUE = (0xFF, 0x7A, 0x8A)
BLUSH = (0xFF, 0x8F, 0xA8)
SPARK = (0xFF, 0xD2, 0x4A)

NAMES = ["neutral", "happy", "scared", "effort", "victory", "defeat", "blink", "ouch"]


def script_hash():
    with open(os.path.abspath(__file__), "rb") as f:
        data = f.read().replace(b"\r\n", b"\n")
    return hashlib.sha256(data).hexdigest()


# ---------- signed distances (meters, viewer frame: x right, y up) ----------
class Canvas:
    def __init__(self):
        w, h = CELL_W * SUPER, CELL_H * SUPER
        x0, y0, x1, y1 = FACE_BOX
        px = (np.arange(w) + 0.5) / w
        py = (np.arange(h) + 0.5) / h
        self.x = (x0 + px * (x1 - x0))[None, :].repeat(h, 0)
        self.y = (y1 - py * (y1 - y0))[:, None].repeat(w, 1)
        self.rgb = np.zeros((h, w, 3))
        self.a = np.zeros((h, w))

    def paint(self, inside, color, alpha=1.0):
        k = inside.astype(float) * alpha
        self.rgb = self.rgb * (1 - k[..., None]) + np.array(color, float)[None, None, :] / 255.0 * k[..., None]
        self.a = self.a + (1 - self.a) * k

    def erase(self, inside):
        self.a = np.where(inside, 0.0, self.a)

    def rotated(self, cx, cy, degrees):
        r = math.radians(degrees)
        dx, dy = self.x - cx, self.y - cy
        return dx * math.cos(r) + dy * math.sin(r), -dx * math.sin(r) + dy * math.cos(r)

    def ellipse(self, cx, cy, a, b, degrees=0.0):
        u, v = self.rotated(cx, cy, degrees)
        return (u / a) ** 2 + (v / b) ** 2 <= 1.0

    def diamond(self, cx, cy, a, b, round_=0.25):
        u, v = np.abs(self.x - cx) / a, np.abs(self.y - cy) / b
        return (u ** (2 - round_) + v ** (2 - round_)) <= 1.0 if round_ else (u + v <= 1.0)

    def segment(self, ax, ay, bx, by, width):
        dx, dy = bx - ax, by - ay
        t = np.clip(((self.x - ax) * dx + (self.y - ay) * dy) / (dx * dx + dy * dy), 0, 1)
        return np.hypot(self.x - ax - t * dx, self.y - ay - t * dy) <= width * 0.5

    def arc(self, cx, cy, r, start, end, width):
        """Stroke of the circle (cx, cy, r) between the angles start..end (degrees, counter-clockwise from +x), round caps."""
        d = np.abs(np.hypot(self.x - cx, self.y - cy) - r) <= width * 0.5
        ang = np.degrees(np.arctan2(self.y - cy, self.x - cx)) % 360
        s, e = start % 360, end % 360
        within = (ang >= s) & (ang <= e) if s <= e else (ang >= s) | (ang <= e)
        out = d & within
        for a in (start, end):
            ex, ey = cx + r * math.cos(math.radians(a)), cy + r * math.sin(math.radians(a))
            out |= np.hypot(self.x - ex, self.y - ey) <= width * 0.5
        return out

    def polyline(self, points, width):
        out = np.zeros_like(self.a, bool)
        for (ax, ay), (bx, by) in zip(points, points[1:]):
            out |= self.segment(ax, ay, bx, by, width)
        return out

    def below(self, ax, ay, bx, by):
        """Below the line through a and b."""
        return (self.y - ay) * (bx - ax) - (self.x - ax) * (by - ay) < 0

    def cell(self):
        w, h = CELL_W, CELL_H
        a = self.a.reshape(h, SUPER, w, SUPER).mean(axis=(1, 3))
        prem = (self.rgb * self.a[..., None]).reshape(h, SUPER, w, SUPER, 3).sum(axis=(1, 3))
        cover = self.a.reshape(h, SUPER, w, SUPER).sum(axis=(1, 3))
        rgb = np.where(cover[..., None] > 0, prem / np.maximum(cover[..., None], 1e-9), 0.0)
        out = np.zeros((h, w, 4), np.uint8)
        out[..., :3] = np.round(np.clip(rgb, 0, 1) * 255).astype(np.uint8)
        out[..., 3] = np.round(np.clip(a, 0, 1) * 255).astype(np.uint8)
        return out


STROKE = 0.024  # 7.7 px of 128: the thinnest line, still 2.4 px at 40 px
LID = 0.026


def sides():
    return (-1, 1)


def eye_whites(c, scale_a=1.0, scale_b=1.0, dy=0.0):
    for s in sides():
        c.paint(c.ellipse(s * EYE_X, EYE_Y + dy, EYE_A * scale_a, EYE_B * scale_b), WHITE)


def outline_whites(c, scale_a=1.0, scale_b=1.0, dy=0.0, width=0.012):
    for s in sides():
        outer = c.ellipse(s * EYE_X, EYE_Y + dy, EYE_A * scale_a + width, EYE_B * scale_b + width)
        inner = c.ellipse(s * EYE_X, EYE_Y + dy, EYE_A * scale_a, EYE_B * scale_b)
        c.paint(outer & ~inner, INK)


def pupils(c, scale=1.0, dx=0.0, dy=-0.004, shine=True):
    for s in sides():
        cx = s * EYE_X + dx
        c.paint(c.diamond(cx, EYE_Y + dy, 0.024 * scale, 0.034 * scale), INK)
        if shine:
            c.paint(c.ellipse(cx - 0.008 * scale, EYE_Y + dy + 0.011 * scale, 0.0075 * scale, 0.0075 * scale), WHITE)


def brows(c, dy=0.0, tilt=-6.0, inner_up=0.0, length=0.046, width=STROKE):
    """tilt > 0 = the outer end higher; inner_up raises the inner end (worry)."""
    for s in sides():
        cx = s * 0.076
        half = length
        t = math.radians(tilt)
        ox, oy = cx + s * half * math.cos(t), BROW_Y + dy + half * math.sin(t)
        ix, iy = cx - s * half * math.cos(t), BROW_Y + dy - half * math.sin(t) + inner_up
        c.paint(c.segment(ix, iy, ox, oy, width), LINE)


def smile(c, width=0.05, depth=0.02, dy=0.0):
    pts = [(x, MOUTH_Y + dy + depth * (x / width) ** 2) for x in np.linspace(-width, width, 13)]
    c.paint(c.polyline(pts, STROKE), LINE)


def frown(c, width=0.045, depth=0.018, dy=0.0, width_stroke=STROKE):
    pts = [(x, MOUTH_Y + dy - depth * (x / width) ** 2 + depth) for x in np.linspace(-width, width, 13)]
    c.paint(c.polyline(pts, width_stroke), LINE)


def open_mouth(c, width, height, dy=0.0, tongue=True):
    """A D-shaped open smile: flat top, round bottom, dark inside with a tongue, ink rim."""
    cy = MOUTH_Y + dy + height * 0.35
    rim = c.ellipse(0, cy, width + 0.011, height + 0.011) & (c.y <= cy + 0.011)
    inside = c.ellipse(0, cy, width, height) & (c.y <= cy)
    c.paint(rim, LINE)
    c.paint(inside, MOUTH_IN)
    if tongue:
        c.paint(inside & c.ellipse(0, cy - height * 0.95, width * 0.62, height * 0.55), TONGUE)


def round_mouth(c, rx, ry, dy=0.0):
    c.paint(c.ellipse(0, MOUTH_Y + dy, rx + 0.011, ry + 0.011), LINE)
    c.paint(c.ellipse(0, MOUTH_Y + dy, rx, ry), MOUTH_IN)


def blush(c):
    for s in sides():
        c.paint(c.ellipse(s * 0.124, EYE_Y - 0.052, 0.03, 0.017), BLUSH, 0.85)


def happy_arcs(c, dy=0.0, r=0.04):
    for s in sides():
        c.paint(c.arc(s * EYE_X, EYE_Y - 0.018 + dy, r, 25, 155, STROKE * 1.15), INK)


def closed_lines(c):
    for s in sides():
        c.paint(c.arc(s * EYE_X, EYE_Y + 0.035, 0.045, 215, 325, STROKE * 1.1), INK)


def chevrons(c):
    """> <: squeezed shut, each pointing at the nose."""
    for s in sides():
        tip_x = s * (EYE_X - 0.03)
        back_x = s * (EYE_X + 0.032)
        c.paint(c.polyline([(back_x, EYE_Y + 0.034), (tip_x, EYE_Y), (back_x, EYE_Y - 0.034)], STROKE * 1.15), INK)


def sparkle(c, cx, cy, r):
    c.paint(c.diamond(cx, cy, r, r * 0.32, 0) | c.diamond(cx, cy, r * 0.32, r, 0), SPARK)


def neutral(c):
    eye_whites(c)
    outline_whites(c)
    pupils(c)
    brows(c)
    smile(c)


def happy(c):
    happy_arcs(c)
    brows(c, dy=0.014, tilt=-10.0)
    blush(c)
    open_mouth(c, 0.046, 0.032, dy=-0.004)


def scared(c):
    eye_whites(c, 1.14, 1.2, dy=0.004)
    outline_whites(c, 1.14, 1.2, dy=0.004)
    for s in sides():
        c.paint(c.ellipse(s * EYE_X, EYE_Y + 0.004, 0.0135, 0.0135), INK)
    brows(c, dy=0.016, tilt=-14.0, inner_up=0.018)
    pts = [(x, MOUTH_Y - 0.004 + 0.009 * math.sin(x / 0.05 * 2.5 * math.pi)) for x in np.linspace(-0.042, 0.042, 17)]
    c.paint(c.polyline(pts, STROKE * 1.5), LINE)


def effort(c):
    for s in sides():
        cx = s * EYE_X
        white = c.ellipse(cx, EYE_Y - 0.006, EYE_A, EYE_B * 0.62)
        c.paint(c.ellipse(cx, EYE_Y - 0.006, EYE_A + 0.012, EYE_B * 0.62 + 0.012), INK)
        c.paint(white, WHITE)
        c.paint(white & c.diamond(cx + s * -0.01, EYE_Y - 0.01, 0.022, 0.03), INK)
    brows(c, dy=-0.008, tilt=28.0, length=0.05, width=STROKE * 1.35)
    # Gritted teeth: a white bar with an ink rim and a middle line.
    y = MOUTH_Y + 0.006
    c.paint(c.ellipse(0, y, 0.058, 0.026) & (np.abs(c.y - y) <= 0.026), LINE)
    teeth = c.ellipse(0, y, 0.046, 0.016) & (np.abs(c.y - y) <= 0.016)
    c.paint(teeth, WHITE)
    c.paint(teeth & (np.abs(c.y - y) <= 0.0035), LINE)


def victory(c):
    eye_whites(c, 1.05, 1.08)
    outline_whites(c, 1.05, 1.08)
    for s in sides():
        cx = s * EYE_X
        c.paint(c.ellipse(cx, EYE_Y - 0.004, 0.03, 0.036), INK)
        sparkle(c, cx - 0.008, EYE_Y + 0.006, 0.034)
    brows(c, dy=0.018, tilt=-8.0)
    blush(c)
    open_mouth(c, 0.072, 0.046, dy=-0.012)


def defeat(c):
    """Big whites under heavy lids (a thick line falling to the outside), pupils looking down, a wide thick frown."""
    for s_ in sides():
        cx = s_ * EYE_X
        inner, outer = (cx - s_ * 0.07, EYE_Y + 0.016), (cx + s_ * 0.07, EYE_Y - 0.004)
        lid = c.below(*(inner + outer) if s_ > 0 else (outer + inner))
        white = c.ellipse(cx, EYE_Y, EYE_A * 1.08, EYE_B * 1.08) & lid
        c.paint(c.ellipse(cx, EYE_Y, EYE_A * 1.08 + 0.012, EYE_B * 1.08 + 0.012) & lid, INK)
        c.paint(white, WHITE)
        c.paint(white & c.diamond(cx, EYE_Y - 0.024, 0.024, 0.03), INK)
        c.paint(c.segment(inner[0], inner[1], outer[0], outer[1], STROKE * 1.3), INK)
    brows(c, dy=0.008, tilt=-20.0, inner_up=0.012)
    frown(c, width=0.068, depth=0.026, dy=-0.014, width_stroke=STROKE * 1.4)


def blink(c):
    closed_lines(c)
    brows(c)
    smile(c)


def ouch(c):
    chevrons(c)
    brows(c, dy=0.016, tilt=-10.0, inner_up=0.012)
    round_mouth(c, 0.03, 0.031, dy=-0.008)


DRAW = [neutral, happy, scared, effort, victory, defeat, blink, ouch]


def build():
    atlas = np.zeros((CELL_H * ROWS, CELL_W * COLUMNS, 4), np.uint8)
    mx, my = int(round(MARGIN * CELL_W)), int(round(MARGIN * CELL_H))
    for index, draw in enumerate(DRAW):
        c = Canvas()
        draw(c)
        cell = c.cell()
        alpha = cell[..., 3]
        band = np.ones_like(alpha, bool)
        band[my:CELL_H - my, mx:CELL_W - mx] = False
        if alpha[band].any():
            raise SystemExit(f"cell {index} ({NAMES[index]}) draws inside the {mx} x {my} px margin")
        row, column = divmod(index, COLUMNS)
        atlas[row * CELL_H:(row + 1) * CELL_H, column * CELL_W:(column + 1) * CELL_W] = cell
    return atlas


def write(atlas, path):
    info = PngImagePlugin.PngInfo()
    info.add_text("FACE_SCRIPT_HASH", script_hash())
    os.makedirs(os.path.dirname(path), exist_ok=True)
    buffer = io.BytesIO()
    Image.fromarray(atlas).save(buffer, format="PNG", pnginfo=info, optimize=False, compress_level=9)
    new = buffer.getvalue()
    old = open(path, "rb").read() if os.path.exists(path) else None
    if new != old:
        with open(path, "wb") as f:
            f.write(new)
        return True
    return False


def preview(atlas, path):
    """Each cell on a pink body color at full size and at 40 px wide (the crowd and the icon), for a quick look."""
    body = np.array((0xFF, 0x7E, 0xB6), float)
    tiles = []
    for index in range(len(DRAW)):
        row, column = divmod(index, COLUMNS)
        cell = atlas[row * CELL_H:(row + 1) * CELL_H, column * CELL_W:(column + 1) * CELL_W].astype(float)
        a = cell[..., 3:4] / 255.0
        rgb = (cell[..., :3] * a + body * (1 - a)).astype(np.uint8)
        big = Image.fromarray(rgb)
        small = big.resize((40, 80), Image.BOX).resize((128, 256), Image.NEAREST)
        tile = Image.new("RGB", (260, 256), (40, 40, 48))
        tile.paste(big, (0, 0))
        tile.paste(small, (132, 0))
        tiles.append(tile)
    sheet = Image.new("RGB", (264 * len(tiles), 256), (40, 40, 48))
    for i, t in enumerate(tiles):
        sheet.paste(t, (i * 264, 0))
    sheet.save(path)


def main(argv):
    atlas = build()
    changed = write(atlas, OUT)
    print(f"[faces] {OUT} {'written' if changed else 'unchanged'} ({atlas.shape[1]}x{atlas.shape[0]}, hash {script_hash()[:12]})")
    if "--preview" in argv:
        preview(atlas, argv[argv.index("--preview") + 1])


if __name__ == "__main__":
    main(sys.argv[1:])
