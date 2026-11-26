"""Builds a grouped acceptance package (aceite agrupado): before/after pairs side by side and one HTML page to open on a phone.

Usage (repository root):  python -X utf8 Tools/art/aceite_pacote.py <out folder> <title> <pairs.json>
pairs.json: [{"title": "...", "note": "...", "before": "path or null", "after": "path"}, ...]
Writes <out>/NN-<slug>.jpg (before | after, labelled, max 1600 px wide) and <out>/index.html (images embedded, no
network). The page and the JPEGs are deterministic for the same inputs.
"""

import base64
import io
import json
import os
import re
import sys

from PIL import Image, ImageDraw, ImageFont

MAX_WIDTH = 1600
GAP = 16
LABEL = 44


def font(size):
    for name in ("arialbd.ttf", "arial.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def slug(text):
    return re.sub(r"[^a-z0-9]+", "-", text.lower()).strip("-")[:40]


def pair(before, after):
    images = [Image.open(p).convert("RGB") for p in (before, after) if p]
    labels = ["ANTES", "DEPOIS"] if before else ["DEPOIS"]
    height = max(i.height for i in images)
    images = [i.resize((round(i.width * height / i.height), height), Image.LANCZOS) for i in images]
    width = sum(i.width for i in images) + GAP * (len(images) - 1)
    sheet = Image.new("RGB", (width, height + LABEL), (24, 22, 32))
    draw = ImageDraw.Draw(sheet)
    x = 0
    f = font(30)
    for image, label in zip(images, labels):
        sheet.paste(image, (x, LABEL))
        draw.text((x + 12, 6), label, fill=(255, 210, 74) if label == "DEPOIS" else (200, 200, 210), font=f)
        x += image.width + GAP
    if sheet.width > MAX_WIDTH:
        sheet = sheet.resize((MAX_WIDTH, round(sheet.height * MAX_WIDTH / sheet.width)), Image.LANCZOS)
    return sheet


def main(out, title, pairs_path):
    os.makedirs(out, exist_ok=True)
    pairs = json.load(open(pairs_path, encoding="utf-8"))
    sections = []
    for n, item in enumerate(pairs, 1):
        sheet = pair(item.get("before"), item["after"])
        name = f"{n:02d}-{slug(item['title'])}.jpg"
        buffer = io.BytesIO()
        sheet.save(buffer, "JPEG", quality=86, optimize=True)
        data = buffer.getvalue()
        with open(os.path.join(out, name), "wb") as f:
            f.write(data)
        sections.append((item, name, base64.b64encode(data).decode("ascii")))
        print(f"[aceite] {name} {sheet.width}x{sheet.height} {len(data) // 1024} KB")
    html = [f"""<!doctype html><html lang="pt"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1"><title>{title}</title>
<style>body{{margin:0;padding:16px;background:#181620;color:#eee;font:16px/1.45 system-ui,sans-serif}}
h1{{font-size:22px}}h2{{font-size:18px;margin-top:28px}}img{{width:100%;height:auto;border-radius:8px}}
p{{color:#cfcfd8}}</style></head><body><h1>{title}</h1>"""]
    for item, name, b64 in sections:
        html.append(f"<h2>{item['title']}</h2><p>{item.get('note', '')}</p><img alt=\"{name}\" src=\"data:image/jpeg;base64,{b64}\">")
    html.append("</body></html>")
    with open(os.path.join(out, "index.html"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(html))


if __name__ == "__main__":
    main(*sys.argv[1:4])
