"""Builds Assets/Claybreak/Content/Fonts/Baloo2-ExtraBold.ttf from the official Google Fonts copy.

Source: github.com/google/fonts, ofl/baloo2, pinned commit below (the only allowed source, RT-6t.9).
The repository ships only the variable font (wght 400-800); the game wants the ExtraBold (800) look,
so this pins the weight axis to 800 with fontTools' instancer. That is a "Modified Version" under the
SIL OFL 1.1, which is allowed; the font has no Reserved Font Name, so the family name stays "Baloo 2".

Usage:  python -I Tools/fonts/baloo2-extrabold.py <download dir> <output ttf>
The download dir must hold "Baloo2[wght].ttf" and "OFL.txt" fetched from the pinned commit:
  https://raw.githubusercontent.com/google/fonts/<COMMIT>/ofl/baloo2/Baloo2%5Bwght%5D.ttf
  https://raw.githubusercontent.com/google/fonts/<COMMIT>/ofl/baloo2/OFL.txt
Hashes are checked before anything is written. Register: docs/audio-licenses.md (Fonts).
Expected output (fontTools 4.59): SHA-256 73e6a48f3fce3efdd0dea8f4f444279e87763be97be07310739b56cefe29d3e2.
"""
import hashlib
import os
import sys

from fontTools.ttLib import TTFont
from fontTools.varLib import instancer

COMMIT = "c13390277dfb1fcc7415d84ef7ef9cfc1e52f8c1"
SOURCES = {
    "Baloo2[wght].ttf": "d47a6852548059b1db49a1319d06d499d546c3fa2237cf9eee9c43c8abb025c2",
    "OFL.txt": "ad09b05dc8bc678c9daf7c4c5f7ef1f55e5726127f4330b2e98e40b9dffcb860",
}
# Characters the game draws; the build fails if the instance loses any of them.
REQUIRED = "+-−×÷x0123456789%!?.:,/ ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" \
           "ÁÂÃÀÇÉÊÍÓÔÕÚáâãàçéêíóôõú"


def sha256(path):
    with open(path, "rb") as handle:
        return hashlib.sha256(handle.read()).hexdigest()


def main(download_dir, output):
    for name, expected in SOURCES.items():
        actual = sha256(os.path.join(download_dir, name))
        if actual != expected:
            sys.exit(f"{name}: SHA-256 {actual} is not the pinned {expected}")

    # recalcTimestamp off keeps the source's head.modified, so the output is byte-for-byte reproducible.
    font = TTFont(os.path.join(download_dir, "Baloo2[wght].ttf"), recalcTimestamp=False)
    static = instancer.instantiateVariableFont(font, {"wght": 800}, updateFontNames=True)
    cmap = static.getBestCmap()
    missing = [c for c in REQUIRED if ord(c) not in cmap]
    if missing:
        sys.exit("missing glyphs: " + " ".join(f"U+{ord(c):04X}" for c in missing))
    static.recalcTimestamp = False
    static.save(output)
    print(output, sha256(output), static["name"].getDebugName(4))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
