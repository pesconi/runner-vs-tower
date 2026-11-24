"""RT-9a: checks every store listing in docs/store/ficha/*.md against the Google Play limits and the adult-direction
word list. Each listing has three fenced ```text blocks, in order: title, short description, full description.

Usage: python -X utf8 Tools/store/check_listing.py  (exit 1 on any problem)
"""
import pathlib
import re
import sys

LIMITS = (("title", 30), ("short description", 80), ("full description", 4000))
# Play metadata policy (rankings, prices, promotions, calls to action) and the studio's adult direction (R8, D25).
FORBIDDEN = ("best", "#1", "top", "free", "new", "sale", "discount", "download now", "install now", "number one",
             "kids", "kid", "children", "child", "cute", "family", "educational", "baby", "toddler", "clay siege")

def blocks(text):
    return re.findall(r"```text\n(.*?)\n```", text, re.S)

def main():
    root = pathlib.Path(__file__).resolve().parents[2] / "docs" / "store" / "ficha"
    problems = []
    for path in sorted(root.glob("*.md")):
        parts = blocks(path.read_text(encoding="utf-8"))
        if len(parts) != 3:
            problems.append(f"{path.name}: expected 3 text blocks, found {len(parts)}")
            continue
        for (name, limit), body in zip(LIMITS, parts):
            size = len(body)
            print(f"{path.name}: {name} {size}/{limit}")
            if size > limit:
                problems.append(f"{path.name}: {name} has {size} characters (limit {limit})")
            lowered = body.lower()
            for word in FORBIDDEN:
                if re.search(r"(?<![a-z])" + re.escape(word) + r"(?![a-z])", lowered):
                    problems.append(f"{path.name}: {name} uses '{word}'")
            if re.search(r"[\U0001F300-\U0001FAFF☀-➿]", body):
                problems.append(f"{path.name}: {name} has an emoji or symbol")
    for problem in problems:
        print("FAIL", problem)
    print("PASS" if not problems else f"{len(problems)} problem(s)")
    return 1 if problems else 0

if __name__ == "__main__":
    sys.exit(main())
