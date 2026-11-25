"""Pixel comparison of two capture folders (RT-6v3.1): the art series' "before and after" check.

Usage: python -X utf8 Tools/art/compare_captures.py <folder A> <folder B> [--tolerance 1.0] [--channel 8] [--diff <out folder>]
  Compares every PNG that both folders have (same name). A pixel differs when any channel differs by more than --channel
  (0-255, default 8: the GPU's dithering and 4x MSAA noise). A pair passes when the share of differing pixels is at most
  --tolerance percent (default 1.0: the crowd and its debris differ a little between runs; static framings differ by 0). Files present in only one folder fail. --diff writes an amplified difference image per pair.
Exit 0 when every pair passes, 1 otherwise. Prints one line per file.
"""
import argparse
import os
import sys

import numpy as np
from PIL import Image


def compare(path_a, path_b, channel):
    a = np.asarray(Image.open(path_a).convert("RGB"), dtype=np.int16)
    b = np.asarray(Image.open(path_b).convert("RGB"), dtype=np.int16)
    if a.shape != b.shape:
        return None, None, None, a.shape, b.shape
    delta = np.abs(a - b)
    differing = (delta.max(axis=2) > channel)
    share = 100.0 * differing.sum() / differing.size
    return share, int(delta.max()), delta, a.shape, b.shape


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("a")
    parser.add_argument("b")
    parser.add_argument("--tolerance", type=float, default=1.0)
    parser.add_argument("--channel", type=int, default=8)
    parser.add_argument("--diff")
    args = parser.parse_args()

    names_a = {n for n in os.listdir(args.a) if n.lower().endswith(".png")}
    names_b = {n for n in os.listdir(args.b) if n.lower().endswith(".png")}
    failed = False
    for name in sorted(names_a | names_b):
        if name not in names_a or name not in names_b:
            print(f"FAIL {name}: only in {'A' if name in names_a else 'B'}")
            failed = True
            continue
        share, peak, delta, shape_a, shape_b = compare(os.path.join(args.a, name), os.path.join(args.b, name), args.channel)
        if share is None:
            print(f"FAIL {name}: sizes differ {shape_a} vs {shape_b}")
            failed = True
            continue
        ok = share <= args.tolerance
        failed |= not ok
        print(f"{'PASS' if ok else 'FAIL'} {name}: {share:.4f}% of pixels differ by more than {args.channel} (largest channel difference {peak}; limit {args.tolerance}%)")
        if args.diff:
            os.makedirs(args.diff, exist_ok=True)
            Image.fromarray(np.clip(delta * 8, 0, 255).astype(np.uint8)).save(os.path.join(args.diff, name))
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
