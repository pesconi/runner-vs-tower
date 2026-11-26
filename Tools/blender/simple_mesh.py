"""Simple-mesh exporter shared by the Blender scripts of Tools/blender (RT-6v3.4a).

Writes a Blender mesh as plain versioned data (JSON): positions, normals, UV, UV2, a palette index per vertex and the
triangle indices, already in Unity's frame, so the game's Editor generator (Claybreak.Editor.SimpleMeshData) turns it
into a mesh asset without Blender and without FBX (FBX stores a date and the importer welds and reorders vertices).

Frames: Blender is right-handed, +Z up, the character faces -Y and its right hand is on -X. Unity is left-handed, +Y up,
the character faces +Z and its right hand is on +X: unity = (-bx, bz, -by). That map is a reflection, so the triangle
winding is reversed on the way out (Unity's front faces are clockwise).
"""

import hashlib
import json
import math
import os

FORMAT = "jungledegree.simple-mesh/1"


def to_unity(v):
    return (-v[0], v[2], -v[1])


def to_blender(u):
    return (-u[0], -u[2], u[1])


def srgb_to_linear(c):
    c = c / 255.0
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def palette_entry(name, hex_rgb, tint):
    """A palette color: hex sRGB (the character sheet's value) stored as linear floats; tint = takes the skin."""
    r, g, b = (int(hex_rgb[i:i + 2], 16) for i in (1, 3, 5))
    return {"name": name, "srgb": hex_rgb.upper(), "linear": [round(srgb_to_linear(x), 6) for x in (r, g, b)], "tint": tint}


def source_hash(paths):
    """SHA-256 of the generating scripts, line endings normalized (the validator recomputes it from the repository)."""
    digest = hashlib.sha256()
    for path in paths:
        with open(path, "rb") as f:
            digest.update(f.read().replace(b"\r\n", b"\n"))
    return digest.hexdigest()


def _r(x, digits):
    v = round(x, digits)
    return 0.0 if v == 0 else v


def export(mesh, path, *, name, level, palette, part_color, vertex_data, normal_override, meta, script_paths, blender_version):
    """Exports a triangulated copy of `mesh` (a bpy Mesh with an int face attribute 'part').

    part_color(part) -> palette index; vertex_data(part, unity_position) -> (uv, uv2);
    normal_override(part, unity_position) -> unity normal or None (keep Blender's corner normal).
    Corners that share a vertex, a part and a normal become one vertex.
    """
    mesh.calc_loop_triangles()
    corner_normals = [tuple(cn.vector) for cn in mesh.corner_normals]
    parts = mesh.attributes["part"].data
    vertices = []
    lookup = {}
    indices = []
    for tri in mesh.loop_triangles:
        part = parts[tri.polygon_index].value
        corner = []
        for loop_index in tri.loops:
            vi = mesh.loops[loop_index].vertex_index
            p = to_unity(mesh.vertices[vi].co)
            n = normal_override(part, p)
            if n is None:
                n = to_unity(corner_normals[loop_index])
            length = math.sqrt(sum(c * c for c in n)) or 1.0
            n = tuple(c / length for c in n)
            key = (vi, part, tuple(_r(c, 4) for c in n))
            index = lookup.get(key)
            if index is None:
                uv, uv2 = vertex_data(part, p)
                index = len(vertices)
                lookup[key] = index
                vertices.append((p, n, uv, uv2, part_color(part)))
            corner.append(index)
        # Reflection to Unity: reverse the winding.
        indices.extend((corner[0], corner[2], corner[1]))

    if len(vertices) > 65535:
        raise ValueError(f"{name}: {len(vertices)} vertices do not fit 16-bit indices")

    data = {
        "format": FORMAT,
        "name": name,
        "level": level,
        "generator": {"scripts": [os.path.basename(p) for p in script_paths], "sourceSha256": source_hash(script_paths),
                      "blender": blender_version},
        "frame": "unity: meters, +Y up, +Z front, +X the character's right; clockwise front faces",
        "colorSpace": "linear",
        "palette": palette,
        "meta": meta,
        "vertexCount": len(vertices),
        "triangleCount": len(indices) // 3,
        "positions": [_r(c, 5) for v in vertices for c in v[0]],
        "normals": [_r(c, 4) for v in vertices for c in v[1]],
        "uv": [_r(c, 4) for v in vertices for c in v[2]],
        "uv2": [_r(c, 4) for v in vertices for c in v[3]],
        "colors": [v[4] for v in vertices],
        "indices": indices,
    }
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("{\n")
        keys = list(data.keys())
        for i, key in enumerate(keys):
            f.write(f'  "{key}": {json.dumps(data[key], separators=(",", ":"))}' + ("," if i < len(keys) - 1 else "") + "\n")
        f.write("}\n")
    return data["triangleCount"], data["vertexCount"]
