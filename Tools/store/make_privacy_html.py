"""RT-9a: builds the static privacy page from docs/politica-de-privacidade.md, so the published page and the reviewed
text never drift apart. Takes the "## English" and "## Português (Portugal)" sections (English first: the store
listing is en-US) and writes docs/store/site/stampeep/privacy/index.html, one self-contained file (no scripts, no
external fonts, no trackers) ready to copy to the studio site.

Placeholders such as `<NIPC>` stay visible and highlighted: fill them in the Markdown, run this again, and the page
says "PREENCHER" nowhere. `--check` fails while any placeholder is left (run it before publishing).

Usage: python -X utf8 Tools/store/make_privacy_html.py [--check]
"""
import html
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parents[2]
SOURCE = ROOT / "docs" / "politica-de-privacidade.md"
TARGET = ROOT / "docs" / "store" / "site" / "stampeep" / "privacy" / "index.html"
PLACEHOLDER = re.compile(r"`(<[^`>]+>)`")


def section(text, title):
    match = re.search(r"^## " + re.escape(title) + r"\n(.*?)(?=^---$)", text, re.S | re.M)
    if not match:
        raise SystemExit(f"section '{title}' not found in {SOURCE}")
    return match.group(1).strip()


def inline(text):
    out = html.escape(text)
    out = re.sub(r"`(&lt;[^`]+?&gt;)`", r'<mark class="fill">\1</mark>', out)
    out = re.sub(r"`([^`]+)`", r"<code>\1</code>", out)
    out = re.sub(r"\*\*(.+?)\*\*", r"<strong>\1</strong>", out)
    out = re.sub(r"(https?://[^\s<)]+[^\s<).,;])", r'<a href="\1">\1</a>', out)
    out = re.sub(r"(?<![/\w.])(www\.cnpd\.pt)", r'<a href="https://www.cnpd.pt">\1</a>', out)
    return out


def blocks(markdown):
    parts = []
    for block in re.split(r"\n\s*\n", markdown):
        lines = block.split("\n")
        if all(re.match(r"^- ", line) or line.startswith("  ") for line in lines):
            items, current = [], None
            for line in lines:
                if line.startswith("- "):
                    if current is not None:
                        items.append(current)
                    current = line[2:]
                else:
                    current += " " + line.strip()
            items.append(current)
            parts.append("<ul>\n" + "\n".join(f"<li>{inline(item)}</li>" for item in items) + "\n</ul>")
        elif any(line.startswith("- ") for line in lines):
            head = [line for line in lines if not line.startswith("- ") and not line.startswith("  ")]
            rest = "\n".join(lines[len(head):])
            parts.append(f"<p>{inline(' '.join(head))}</p>")
            parts.extend(blocks(rest))
        else:
            parts.append(f"<p>{inline(' '.join(line.strip() for line in lines))}</p>")
    return parts


PAGE = """<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Stampeep — Privacy Policy</title>
<meta name="robots" content="index, follow">
<style>
:root {{ --bg: #fbf6f1; --ink: #2b2433; --soft: #6b6275; --accent: #f08a2c; --line: #eadfd6; --fill: #ffe08a; }}
@media (prefers-color-scheme: dark) {{
  :root {{ --bg: #1d1a22; --ink: #f1ebf5; --soft: #b7aec0; --accent: #ffa65a; --line: #3a3442; --fill: #7a5c00; }}
}}
* {{ box-sizing: border-box; }}
body {{ margin: 0; background: var(--bg); color: var(--ink); font: 16px/1.6 system-ui, -apple-system, "Segoe UI",
  Roboto, sans-serif; }}
main {{ max-width: 760px; margin: 0 auto; padding: 32px 16px 64px; }}
h1 {{ font-size: 1.9rem; margin: 0 0 4px; }}
h2 {{ font-size: 1.3rem; margin: 40px 0 8px; padding-top: 16px; border-top: 1px solid var(--line); }}
nav a, a {{ color: var(--accent); }}
nav {{ color: var(--soft); margin-bottom: 8px; }}
code {{ font-size: .92em; }}
mark.fill {{ background: var(--fill); color: inherit; padding: 0 3px; border-radius: 3px; }}
ul {{ padding-left: 1.2em; }}
li {{ margin: 6px 0; }}
footer {{ margin-top: 48px; color: var(--soft); font-size: .9rem; }}
</style>
</head>
<body>
<main>
<h1>Stampeep — Privacy Policy</h1>
<nav><a href="#en">English</a> · <a href="#pt">Português (Portugal)</a></nav>
<section id="en" lang="en">
<h2>English</h2>
{english}
</section>
<section id="pt" lang="pt-PT">
<h2>Português (Portugal) — Política de privacidade</h2>
{portuguese}
</section>
<footer>Jungle Degree · Stampeep</footer>
</main>
</body>
</html>
"""


def main():
    text = SOURCE.read_text(encoding="utf-8")
    english = section(text, "English")
    portuguese = section(text, "Português (Portugal)")
    left = sorted(set(PLACEHOLDER.findall(english + portuguese)))
    page = PAGE.format(english="\n".join(blocks(english)), portuguese="\n".join(blocks(portuguese)))
    TARGET.parent.mkdir(parents=True, exist_ok=True)
    TARGET.write_text(page, encoding="utf-8", newline="\n")
    print(f"wrote {TARGET.relative_to(ROOT)} ({len(page)} bytes)")
    if left:
        print("PREENCHER antes de publicar: " + ", ".join(left))
        if "--check" in sys.argv:
            return 1
    elif "--check" in sys.argv:
        print("PASS: no placeholder left")
    return 0


if __name__ == "__main__":
    sys.exit(main())
