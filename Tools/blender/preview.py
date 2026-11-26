"""Preview of simple-mesh JSON files (RT-6v3.4a): renders each file from the front, 3/4, back and the run camera.

Run: blender -b --factory-startup -P Tools/blender/preview.py -- <out.png> <file.json> [file.json ...]
Workbench render with the vertex colors (tinted parts in a teal stand-in for the palette), on a light background; one row
per file, one column per view. A quick look while shaping, not the game's shader (the game's captures are the
BibopziCaptureTests in PlayMode).
"""

import json
import math
import os
import sys

import bpy
from mathutils import Vector

TEAL = (0.10, 0.62, 0.55)


def load(path):
    with open(path, encoding="utf-8") as f:
        d = json.load(f)
    pos = d["positions"]
    n = d["vertexCount"]
    verts = [(-pos[3 * i], -pos[3 * i + 2], pos[3 * i + 1]) for i in range(n)]  # unity -> blender
    idx = d["indices"]
    faces = [(idx[3 * i], idx[3 * i + 2], idx[3 * i + 1]) for i in range(len(idx) // 3)]
    mesh = bpy.data.meshes.new(d["name"])
    mesh.from_pydata(verts, [], faces)
    nor = d["normals"]
    mesh.normals_split_custom_set_from_vertices([(-nor[3 * i], -nor[3 * i + 2], nor[3 * i + 1]) for i in range(n)])
    col = mesh.color_attributes.new("Col", 'FLOAT_COLOR', 'POINT')
    for i in range(n):
        e = d["palette"][d["colors"][i]]
        rgb = TEAL if e["tint"] else e["linear"]
        col.data[i].color = (*rgb, 1.0)
    obj = bpy.data.objects.new(d["name"], mesh)
    bpy.context.scene.collection.objects.link(obj)
    return obj


def main():
    argv = sys.argv[sys.argv.index("--") + 1:]
    out, files = argv[0], argv[1:]
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.render.engine = 'BLENDER_WORKBENCH'
    scene.display.shading.light = 'STUDIO'
    scene.display.shading.color_type = 'VERTEX'
    scene.display.shading.show_specular_highlight = True
    scene.render.film_transparent = False
    scene.world = bpy.data.worlds.new("w")
    scene.display.shading.background_type = 'VIEWPORT' if hasattr(scene.display.shading, "background_type") else None
    scene.render.resolution_x = 360
    scene.render.resolution_y = 420
    cam_data = bpy.data.cameras.new("cam")
    cam_data.type = 'ORTHO'
    cam_data.ortho_scale = 0.95
    cam = bpy.data.objects.new("cam", cam_data)
    scene.collection.objects.link(cam)
    scene.camera = cam
    views = [("front", 0, 0), ("34", 35, 8), ("back", 180, 0), ("run", 160, 35)]
    tiles = []
    for fi, path in enumerate(files):
        obj = load(path)
        for name, yaw, pitch in views:
            # Character faces -Y in Blender; a camera at -Y looks at the front.
            a, p = math.radians(yaw), math.radians(pitch)
            target = Vector((0, 0, 0.36))
            d = Vector((math.sin(a) * math.cos(p), -math.cos(a) * math.cos(p), math.sin(p)))
            cam.location = target + d * 3
            cam.rotation_euler = (-d).to_track_quat('-Z', 'Y').to_euler()
            tile = os.path.join(os.path.dirname(out), f"_tile_{fi}_{name}.png")
            scene.render.filepath = tile
            bpy.ops.render.render(write_still=True)
            tiles.append(tile)
        bpy.data.objects.remove(obj)
    # Stitch with Blender's image API.
    cols, rows = len(views), len(files)
    w, h = scene.render.resolution_x, scene.render.resolution_y
    sheet = bpy.data.images.new("sheet", w * cols, h * rows)
    pixels = [1.0] * (w * cols * h * rows * 4)
    for k, tile in enumerate(tiles):
        img = bpy.data.images.load(tile)
        src = list(img.pixels)
        r, c = rows - 1 - k // cols, k % cols
        for y in range(h):
            row = src[y * w * 4:(y + 1) * w * 4]
            start = ((r * h + y) * w * cols + c * w) * 4
            pixels[start:start + w * 4] = row
        bpy.data.images.remove(img)
        os.remove(tile)
    sheet.pixels = pixels
    sheet.filepath_raw = out
    sheet.file_format = 'PNG'
    sheet.save()


main()
