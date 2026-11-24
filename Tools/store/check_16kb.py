"""RT-9a: Google Play requirement for apps targeting Android 15+: every native library must load on 16 KB page devices
(ELF LOAD segments aligned to at least 16 KB). Reads each lib/**/*.so inside an APK or AAB and prints the smallest
LOAD alignment; exit 1 if any 64-bit library is below 16384.

Usage: python Tools/store/check_16kb.py <apk-or-aab>
"""
import struct
import sys
import zipfile


def load_alignments(data):
    if data[:4] != b"\x7fELF" or data[4] != 2:
        return None  # not a 64-bit ELF
    little = data[5] == 1
    order = "<" if little else ">"
    phoff = struct.unpack_from(order + "Q", data, 0x20)[0]
    phentsize, phnum = struct.unpack_from(order + "HH", data, 0x36)
    aligns = []
    for i in range(phnum):
        base = phoff + i * phentsize
        p_type = struct.unpack_from(order + "I", data, base)[0]
        if p_type == 1:  # PT_LOAD
            aligns.append(struct.unpack_from(order + "Q", data, base + 0x30)[0])
    return aligns


def main():
    bad = 0
    with zipfile.ZipFile(sys.argv[1]) as archive:
        for name in archive.namelist():
            if not name.endswith(".so") or "lib/" not in name:
                continue
            aligns = load_alignments(archive.read(name))
            if aligns is None:
                continue
            smallest = min(aligns)
            ok = smallest >= 16384
            bad += 0 if ok else 1
            print(f"{'OK  ' if ok else 'FAIL'} {smallest:>6} {name}")
    print("PASS: 16 KB ready" if bad == 0 else f"{bad} library(ies) below 16 KB")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
