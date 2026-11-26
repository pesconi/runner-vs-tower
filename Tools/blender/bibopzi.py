"""Bibopzi in three levels (RT-6v3.4a), built in Blender 5.3 without the interface and written as simple data.

Run (Git Bash, repository root):  Tools/blender/run.sh            (or: blender -b --factory-startup -P Tools/blender/bibopzi.py)
Writes Tools/blender/out/bibopzi_{detail,crowd,reserve}.json; the Editor generator (Claybreak/Build Unit Shapes, also run by
the scene builder) turns them into mesh assets. The game never needs Blender: the validator reads the JSON and checks that
its source hash matches these scripts.

The character is D3 "Feijao" of the character sheet (personagem/ia/2026-11/2026-11-25_bibopzi_direcoes-2d.svg, column A),
with the D36 additions (direcao-visual-v3.md section 10): bigger eyes readable at 40 px and a wind-up key on the back (the
crowd is seen from behind). Landmarks on every level: spring topknot, gear on HIS right temple (+X), cream belly, loose
hands, the key. Proportions are those of the RT-6t.22 recipe (UnitShapeRecipes.Bibopzi), so the crown seat and the
crowd spacing stay.

UV2 marks for the kit's animated layout (StudioKit/ToonVinyl, K.v3b): x spring weight (0 at the head, 1 at the tip),
y hand side (+1 his right hand on +X, -1 the left), z spin weight (1 on the key, which turns about meta.keyAxis through
meta.keyPivot), w face mask (1 on the front half of the body: the face atlas paints it). UV0: the body projected on the
face box (meta.faceBox) as seen from the front (u grows to the viewer's right, which is his -X; v up), 0 on the other
parts. The box holds the whole body, so every body vertex has its true projection: a triangle across the edge of the mask
(the sides) fades the mask without dragging the UV across the cell. RT-6v3.4b: eyes, brows and mouth are no longer geometry; they are the cells of the face atlas
(Tools/art/bibopzi_faces.py, same FACE_BOX), so the face changes with the game's events. The screw nose stays geometry.
"""

import math
import os
import sys

import bmesh
import bpy
from mathutils import Matrix, Vector

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import simple_mesh  # noqa: E402

BLENDER = "5.3"
OUT = os.path.join(HERE, "out")

# ---------- palette (index = vertex color) ----------
BODY, CREAM, ORANGE, EYES, WHITE, BROW, STEEL, KEY = range(8)
PALETTE = [
    simple_mesh.palette_entry("body", "#FFFFFF", True),     # takes the palette / skin tint
    simple_mesh.palette_entry("cream", "#FFF1DC", False),   # belly
    simple_mesh.palette_entry("orange", "#FF8A3D", False),  # gear, spring, hands, feet, key
    simple_mesh.palette_entry("eyes", "#2B2233", False),    # pupils
    simple_mesh.palette_entry("white", "#FBF7F0", False),   # eye whites, highlights
    simple_mesh.palette_entry("brow", "#1D2B3A", False),    # brows, mouth, screw slot
    simple_mesh.palette_entry("steel", "#C8CED6", False),   # screw nose, gear bolt
    simple_mesh.palette_entry("key", "#2E3E55", False),     # wind-up key: dark steel, apart from the orange and the pink (RT-6v3.4b)
]

# Parts (face attribute 'part'): color and UV2 marks follow the part.
P_BODY, P_BELLY, P_EYE_WHITE, P_PUPIL, P_SHINE, P_BROW, P_MOUTH, P_NOSE, P_SLOT, P_GEAR, P_BOLT, P_SPRING, P_HAND_R, \
    P_HAND_L, P_FOOT, P_KEY = range(16)
PART_COLOR = {P_BODY: BODY, P_BELLY: CREAM, P_EYE_WHITE: WHITE, P_PUPIL: EYES, P_SHINE: WHITE, P_BROW: BROW,
              P_MOUTH: BROW, P_NOSE: STEEL, P_SLOT: BROW, P_GEAR: ORANGE, P_BOLT: STEEL, P_SPRING: ORANGE,
              P_HAND_R: ORANGE, P_HAND_L: ORANGE, P_FOOT: ORANGE, P_KEY: KEY}

# ---------- the bean (Unity frame, meters; same numbers as UnitShapeRecipes.Bibopzi) ----------
BOTTOM = 0.03
HEIGHT = 0.594
BASE_HALF = 0.18
HEAD_HALF = 0.143
DEPTH = 0.82
FULLNESS = 2.78
HEAD_TOP = BOTTOM + HEIGHT

FACE_BOX = (-0.2, -0.02, 0.2, 0.68)  # x min, y min, x max, y max of the face projection (UV0); = Tools/art/bibopzi_faces.py


def half_width(t):
    k = min(1.0, max(0.0, (t - 0.5) / 0.26))
    s = k * k * (3 - 2 * k)
    return BASE_HALF + (HEAD_HALF - BASE_HALF) * s


def ring(t):
    return (1 - abs(2 * t - 1) ** FULLNESS) ** (1 / FULLNESS)


def field(p):
    t = (p[1] - BOTTOM) / HEIGHT
    w = half_width(min(1.0, max(0.0, t)))
    x = p[0] / w
    z = p[2] / (w * DEPTH)
    return math.sqrt(x * x + z * z) ** FULLNESS + abs(2 * t - 1) ** FULLNESS - 1


def bean_normal(p):
    h = 1e-4
    g = [field([p[0] + (h if i == 0 else 0), p[1] + (h if i == 1 else 0), p[2] + (h if i == 2 else 0)])
         - field([p[0] - (h if i == 0 else 0), p[1] - (h if i == 1 else 0), p[2] - (h if i == 2 else 0)]) for i in range(3)]
    n = Vector(g)
    return n.normalized() if n.length > 0 else Vector((0, 1, 0))


def bend_into_bean(u):
    """A point of the unit sphere (Unity frame) onto the bean."""
    sq = 2 / FULLNESS
    across = math.sqrt(u[0] * u[0] + u[2] * u[2])
    t = (math.copysign(abs(u[1]) ** sq, u[1]) + 1) * 0.5
    radius = across ** sq * half_width(t)
    if across > 1e-6:
        return Vector((u[0] / across * radius, BOTTOM + t * HEIGHT, u[2] / across * radius * DEPTH))
    return Vector((0, BOTTOM + t * HEIGHT, 0))


def surface(t, azimuth_degrees):
    """Bean surface at height fraction t and azimuth (0 = +X his right, 90 = +Z front, -90 = back)."""
    r = ring(t) * half_width(t)
    a = math.radians(azimuth_degrees)
    return Vector((math.cos(a) * r, BOTTOM + t * HEIGHT, math.sin(a) * r * DEPTH))


def front_azimuth(t, x):
    return math.degrees(math.acos(max(-1.0, min(1.0, x / (ring(t) * half_width(t))))))


# ---------- placing primitives (frames in the Unity frame, turned into Blender matrices) ----------
C = Matrix(((-1, 0, 0), (0, 0, -1), (0, 1, 0)))  # unity -> blender (to_blender as a matrix)
C_INV = C.inverted()


def blender_matrix(origin, x_axis, y_axis, z_axis):
    """Unity affine (columns = local axes with their scale) as the matrix of a Blender primitive built in its own frame."""
    a = Matrix((
        (x_axis[0], y_axis[0], z_axis[0], origin[0]),
        (x_axis[1], y_axis[1], z_axis[1], origin[1]),
        (x_axis[2], y_axis[2], z_axis[2], origin[2]),
        (0, 0, 0, 1)))
    c4 = C.to_4x4()
    return c4 @ a @ C_INV.to_4x4()


def place(position, scale, axes=None):
    x, y, z = axes or (Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1)))
    return blender_matrix(position, x * scale[0], y * scale[1], z * scale[2])


def on_bean(t, azimuth, lift, scale, spin=0.0):
    """Frame lying on the bean: local +Y along the surface normal, +X across, +Z down the surface."""
    p = surface(t, azimuth)
    n = bean_normal(p)
    x = Vector((0, 1, 0)).cross(n).normalized()
    z = x.cross(n).normalized()
    s = math.radians(spin)
    x, z = x * math.cos(s) - z * math.sin(s), x * math.sin(s) + z * math.cos(s)
    return place(p + n * lift, scale, (x, n, z))


def orient_outward(faces):
    """Every face facing away from the part's center (all parts are star-shaped about it). bmesh's recalc_face_normals
    is not deterministic from run to run, so the exporter would reorder triangles."""
    verts = list(dict.fromkeys(v for f in faces for v in f.verts))
    center = sum((v.co for v in verts), Vector()) / max(1, len(verts))
    for f in faces:
        f.normal_update()
        if f.normal.dot(f.calc_center_median() - center) < 0:
            f.normal_flip()


class Model:
    def __init__(self):
        self.bm = bmesh.new()
        self.part = self.bm.faces.layers.int.new("part")

    def _tag(self, geom, part):
        faces = [g for g in geom if isinstance(g, bmesh.types.BMFace)]
        if not faces:
            verts = [g for g in geom if isinstance(g, bmesh.types.BMVert)]
            faces = list(dict.fromkeys(f for v in verts for f in v.link_faces))
        for f in faces:
            f[self.part] = part
        orient_outward(faces)
        return faces

    def canonical(self):
        """A copy with faces in a fixed order (part, then position) and each face starting at its smallest corner.
        bmesh's primitives do not keep the same face order from run to run; the exported data must."""
        def key(co):
            return tuple(round(c, 6) for c in co)

        faces = []
        for f in self.bm.faces:
            corners = [key(v.co) for v in f.verts]
            corners = [c for i, c in enumerate(corners) if c != corners[i - 1]]  # drop collapsed corners
            if len(set(corners)) < 3:
                continue
            start = corners.index(min(corners))
            corners = corners[start:] + corners[:start]
            faces.append((f[self.part], sorted(corners), corners))
        faces.sort()
        out = bmesh.new()
        layer = out.faces.layers.int.new("part")
        verts = {}
        for part, _, corners in faces:
            loop = []
            for c in corners:
                v = verts.get((part, c))
                if v is None:
                    v = out.verts.new(c)
                    verts[(part, c)] = v
                loop.append(v)
            out.faces.new(loop)[layer] = part
        return out

    def dome(self, at, segments, rings, part):
        """A dome: at(u, v, h) places the unit hemisphere point (u, v across, h up); open rim at h = 0."""
        verts = [[self.bm.verts.new(simple_mesh.to_blender(at(0.0, 0.0, 1.0)))]]
        for k in range(1, rings + 1):
            phi = 0.5 * math.pi * k / rings
            ring_verts = []
            for j in range(segments):
                th = 2 * math.pi * j / segments
                ring_verts.append(self.bm.verts.new(simple_mesh.to_blender(
                    at(math.sin(phi) * math.cos(th), math.sin(phi) * math.sin(th), math.cos(phi)))))
            verts.append(ring_verts)
        faces = []
        for j in range(segments):
            jj = (j + 1) % segments
            faces.append(self.bm.faces.new((verts[0][0], verts[1][j], verts[1][jj])))
        for k in range(1, rings):
            for j in range(segments):
                jj = (j + 1) % segments
                faces.append(self.bm.faces.new((verts[k][j], verts[k + 1][j], verts[k + 1][jj], verts[k][jj])))
        up = Vector(simple_mesh.to_blender(at(0.0, 0.0, 1.0))) - Vector(simple_mesh.to_blender(at(0.0, 0.0, 0.0)))
        faces[0].normal_update()
        flip = faces[0].normal.dot(up) < 0
        for f in faces:
            f[self.part] = part
            if flip:
                f.normal_flip()
        return faces

    def sphere(self, matrix, segments, rings, part):
        r = bmesh.ops.create_uvsphere(self.bm, u_segments=segments, v_segments=rings, radius=1.0, matrix=matrix)
        return self._tag(r["verts"], part)

    def cylinder(self, matrix, segments, part):
        """Radius 1 around local Unity +Y, base at y = 0, top at y = 1, capped."""
        m = matrix @ Matrix.Translation((0, 0, 0.5))
        r = bmesh.ops.create_cone(self.bm, cap_ends=True, cap_tris=False, segments=segments, radius1=1.0, radius2=1.0,
                                  depth=1.0, matrix=m)
        return self._tag(r["verts"], part)

    def prism(self, matrix, outline, part, bevel=0.0, bevel_segments=0):
        """Outline (x, z) in the local plane, extruded from local y = 0 to 1 (Unity local axes)."""
        bottom = [self.bm.verts.new(matrix @ Vector(simple_mesh.to_blender((x, 0, z)))) for x, z in outline]
        top = [self.bm.verts.new(matrix @ Vector(simple_mesh.to_blender((x, 1, z)))) for x, z in outline]
        faces = [self.bm.faces.new(bottom[::-1]), self.bm.faces.new(top)]
        n = len(outline)
        for i in range(n):
            j = (i + 1) % n
            faces.append(self.bm.faces.new((bottom[i], bottom[j], top[j], top[i])))
        self._tag(faces, part)
        if bevel > 0 and bevel_segments > 0:
            edges = list(dict.fromkeys(e for f in faces for e in f.edges))
            result = bmesh.ops.bevel(self.bm, geom=edges, offset=bevel, offset_type='OFFSET', segments=bevel_segments,
                                     profile=0.5, affect='EDGES', clamp_overlap=True)
            self._tag(result["faces"], part)
            faces = list(dict.fromkeys(f for v in top + bottom if v.is_valid for f in v.link_faces))
        return faces

    def tube(self, points, radius_at, sides, part):
        """A tube along the points (Unity frame), radius_at(i) per point, open ends, parallel-transport frames."""
        rings = []
        tangent_prev = None
        normal = None
        for i, p in enumerate(points):
            a = points[max(0, i - 1)]
            b = points[min(len(points) - 1, i + 1)]
            tangent = (Vector(b) - Vector(a)).normalized()
            if normal is None:
                helper = Vector((1, 0, 0)) if abs(tangent.x) < 0.9 else Vector((0, 0, 1))
                normal = tangent.cross(helper).normalized()
            else:
                normal = (normal - tangent * normal.dot(tangent)).normalized()
            binormal = tangent.cross(normal)
            tangent_prev = tangent
            r = radius_at(i)
            ring_verts = []
            for k in range(sides):
                ang = 2 * math.pi * k / sides
                q = Vector(p) + (normal * math.cos(ang) + binormal * math.sin(ang)) * r
                ring_verts.append(self.bm.verts.new(simple_mesh.to_blender(q)))
            rings.append(ring_verts)
        faces = []
        for i in range(len(rings) - 1):
            for k in range(sides):
                kk = (k + 1) % sides
                faces.append(self.bm.faces.new((rings[i][k], rings[i][kk], rings[i + 1][kk], rings[i + 1][k])))
        for f in faces:
            f[self.part] = part
        # Orient outward: a face normal must point away from the axis point of its ring.
        for i in range(len(rings) - 1):
            center = Vector(simple_mesh.to_blender(points[i]))
            for k in range(sides):
                f = faces[i * sides + k]
                f.normal_update()
                if f.normal.dot(f.calc_center_median() - center) < 0:
                    f.normal_flip()
        return faces


# ---------- the levels ----------
# Resolutions per part: sphere (segments, rings), dome (segments, rings), tube (sides, steps), prism teeth and bevel.
LEVELS = {
    "detail": dict(range=(3000, 5000), body=(36, 26), belly=(20, 6), nose=(6, 3), slot=(4, 2), gear_teeth=8, gear_bevel=(0.0035, 1), bolt=8,
                   spring=(8, 32), ball=(12, 8), hand=(14, 10), foot=(12, 5), key_shaft=8, key_lobe=(12, 8),
                   key_hub=(8, 6)),
    "crowd": dict(range=(800, 1200), body=(16, 11), belly=(10, 3), nose=(6, 2), slot=(4, 1), gear_teeth=6, gear_bevel=None, bolt=None,
                  spring=(5, 12), ball=(6, 4), hand=(8, 6), foot=(8, 3), key_shaft=6, key_lobe=(8, 5), key_hub=None),
    "reserve": dict(range=(450, 550), body=(10, 8), belly=(8, 2), nose=(6, 1), slot=None, gear_teeth=5, gear_bevel=None, bolt=None,
                    spring=(4, 6), ball=(4, 3), hand=(6, 4), foot=(6, 2), key_shaft=4, key_lobe=(6, 4), key_hub=None),
}

NAMES = {"detail": "BibopziDetail", "crowd": "Bibopzi", "reserve": "BibopziReserve"}

# Landmark geometry (Unity frame).
SPRING_BASE = HEAD_TOP - 0.02
SPRING_TOP = HEAD_TOP + 0.105
BALL_RADIUS = 0.034
KEY_T = 0.6
NOSE_T = 0.603  # RT-6v3.4b: down with the painted face (Tools/art/bibopzi_faces.py), between the eyes and the mouth
HAND_Y = BOTTOM + 0.38 * HEIGHT


def key_frame():
    back = surface(KEY_T, -90.0)
    pivot = Vector((0.0, back.y, back.z - 0.065))
    return back, pivot


def onto_bean(q):
    """The bean surface point at the height of q, in the direction of q from the axis (depth-scaled)."""
    t = min(1.0, max(0.0, (q[1] - BOTTOM) / HEIGHT))
    r = ring(t) * half_width(t)
    dx, dz = q[0], q[2] / DEPTH
    length = math.hypot(dx, dz) or 1.0
    return Vector((dx / length * r, q[1], dz / length * r * DEPTH))


def decal_on_bean(t, x, a, b, height, lift, spin=0.0):
    """Placement of a dome decal centered on the front at height fraction t, x across: (u, v, h) in -1..1 x -1..1 x 0..1
    -> a point lying on the bean (the rim follows its curve) raised by lift + h * height along the bean normal."""
    p0 = surface(t, front_azimuth(t, x))
    n0 = bean_normal(p0)
    across = Vector((0, 1, 0)).cross(n0).normalized()
    down = across.cross(n0).normalized()
    s = math.radians(spin)
    c, sn = math.cos(s), math.sin(s)

    def at(u, v, h):
        du, dv = u * a, v * b
        q = p0 + across * (du * c - dv * sn) + down * (du * sn + dv * c)
        on = onto_bean(q)
        return on + bean_normal(on) * (lift + h * height)

    return at


def build(level):
    cfg = LEVELS[level]
    m = Model()

    # Body: a unit sphere bent into the bean (normals are the bean's own, written by the exporter).
    seg, rings_ = cfg["body"]
    r = bmesh.ops.create_uvsphere(m.bm, u_segments=seg, v_segments=rings_, radius=1.0, matrix=Matrix.Identity(4))
    for v in r["verts"]:
        v.co = Vector(simple_mesh.to_blender(bend_into_bean(simple_mesh.to_unity(v.co))))
    m._tag(r["verts"], P_BODY)

    # Belly as a decal lying on the bean (domes whose rim follows the body; nothing hidden under the skin).
    m.dome(decal_on_bean(0.3, 0.0, 0.122, 0.096, 0.012, -0.0015), *cfg["belly"], P_BELLY)
    # Eyes, brows and mouth: the face atlas on the masked front (RT-6v3.4b), not geometry.

    # Screw nose: a hexagonal head with a slanted slot.
    m.dome(decal_on_bean(NOSE_T, 0.0, 0.018, 0.018, 0.008, -0.001), *cfg["nose"], P_NOSE)
    if cfg["slot"]:
        m.dome(decal_on_bean(NOSE_T, 0.0, 0.015, 0.0035, 0.0015, 0.0075, 45.0), *cfg["slot"], P_SLOT)

    # Gear on HIS right temple (+X), turned a little to the front, facing out like an ear.
    teeth = cfg["gear_teeth"]
    step = 2 * math.pi / teeth
    profile = ((0.0, 0.058), (0.25, 0.08), (0.75, 0.08), (0.9, 0.058))
    if cfg["gear_bevel"]:
        profile = ((0.0, 0.058), (0.14, 0.058), (0.25, 0.08), (0.75, 0.08), (0.9, 0.058))
    outline = [(math.cos(k * step + f * step) * rad, math.sin(k * step + f * step) * rad)
               for k in range(teeth) for f, rad in profile]
    gear_matrix = on_bean(0.74, 12.0, -0.006, (1.0, 0.036, 1.0))
    m.prism(gear_matrix, outline, P_GEAR, *(cfg["gear_bevel"] or (0.0, 0)))
    if cfg["bolt"]:
        m.cylinder(on_bean(0.74, 12.0, 0.022, (0.02, 0.012, 0.02)), cfg["bolt"], P_BOLT)

    # Spring topknot: a coil out of the head, narrowing, with a ball on top.
    sides, steps = cfg["spring"]
    turns = 2.5
    pts = []
    for i in range(steps + 1):
        s = i / steps
        ramp = min(1.0, s / 0.12) * min(1.0, (1 - s) / 0.1)
        radius = (0.03 - 0.01 * s) * ramp
        a = 2 * math.pi * turns * s
        pts.append(Vector((math.cos(a) * radius, SPRING_BASE + (SPRING_TOP - SPRING_BASE) * s, math.sin(a) * radius)))
    wire = 0.019 if level != "detail" else 0.016
    m.tube(pts, lambda i: wire, sides, P_SPRING)
    m.sphere(place(Vector((0, SPRING_TOP + BALL_RADIUS * 0.7, 0)), (BALL_RADIUS,) * 3), *cfg["ball"], P_SPRING)

    # Loose ball hands beside the base (a clear gap to the body; no arms).
    for side, part in ((1, P_HAND_R), (-1, P_HAND_L)):
        m.sphere(place(Vector((side * 0.25, HAND_Y, 0.01)), (0.046,) * 3), *cfg["hand"], part)

    # Flat oval feet sticking out at the front: domes standing on the ground (the sole is never seen).
    for side in (-1, 1):
        cx = side * 0.08

        def foot(u, v, h, cx=cx):
            return Vector((cx + u * 0.07, h * 0.05, 0.12 - v * 0.102))

        m.dome(foot, *cfg["foot"], P_FOOT)

    # Wind-up key on the back (D36): a shaft out of the back and a two-lobed bow facing backward (the run camera sees
    # the butterfly from behind); it spins about the shaft (+Z) like a propeller.
    back, pivot = key_frame()
    shaft_len = back.z + 0.02 - pivot.z
    m.cylinder(place(pivot, (0.013, shaft_len, 0.013), (Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, -1, 0)))),
               cfg["key_shaft"], P_KEY)
    # RT-6v3.4b (fable): 1.4x wider flat wings, so the bow reads as a key, not one more orange ball.
    lobe = (0.092, 0.07, 0.02) if level == "detail" else (0.095, 0.072, 0.022)
    for side in (-1, 1):
        m.sphere(place(pivot + Vector((side * 0.1, 0.006, -0.006)), lobe), *cfg["key_lobe"], P_KEY)
    if cfg["key_hub"]:
        m.sphere(place(pivot + Vector((0, 0, -0.008)), (0.026, 0.026, 0.02)), *cfg["key_hub"], P_KEY)

    mesh = bpy.data.meshes.new(NAMES[level])
    canonical = m.canonical()
    canonical.to_mesh(mesh)
    canonical.free()
    m.bm.free()
    # Smooth everywhere (D36): no hard edge; the bevelled gear of the detailed level reads round by its geometry.
    for poly in mesh.polygons:
        poly.use_smooth = True
    return mesh


def vertex_data(part, p):
    x0, y0, x1, y1 = FACE_BOX
    face = part == P_BODY and p[2] > 0
    if part == P_BODY:
        # Seen from the front: the viewer's right is -X. Every masked vertex is inside the box (no clamp: the atlas UV is
        # interpolated linearly, so a clamped corner would stretch the cell).
        uv = ((x1 - p[0]) / (x1 - x0), (p[1] - y0) / (y1 - y0))
        if not (0.0 <= uv[0] <= 1.0 and 0.0 <= uv[1] <= 1.0):
            raise SystemExit(f"body vertex {tuple(p)} outside FACE_BOX")
    else:
        uv = (0.0, 0.0)
    spring = 0.0
    if part == P_SPRING:
        spring = min(1.0, max(0.0, (p[1] - HEAD_TOP) / (SPRING_TOP + BALL_RADIUS * 0.7 - HEAD_TOP)))
    hand = 1.0 if part == P_HAND_R else -1.0 if part == P_HAND_L else 0.0
    spin = 1.0 if part == P_KEY else 0.0
    return uv, (spring, hand, spin, 1.0 if face else 0.0)


def normal_override(part, p):
    return tuple(bean_normal(p)) if part == P_BODY else None


def main():
    if not bpy.app.version_string.startswith(BLENDER):
        raise SystemExit(f"Blender {BLENDER} is pinned; this is {bpy.app.version_string}")
    scripts = [os.path.join(HERE, "bibopzi.py"), os.path.join(HERE, "simple_mesh.py")]
    _, pivot = key_frame()
    meta = {"headTop": round(HEAD_TOP, 5), "springTop": round(SPRING_TOP + BALL_RADIUS * 1.7, 5),
            "keyPivot": [round(c, 5) for c in pivot], "keyAxis": [0.0, 0.0, 1.0], "faceBox": list(FACE_BOX)}
    for level in ("detail", "crowd", "reserve"):
        mesh = build(level)
        tris, verts = simple_mesh.export(
            mesh, os.path.join(OUT, f"bibopzi_{level}.json"), name=NAMES[level], level=level, palette=PALETTE,
            part_color=lambda part: PART_COLOR[part], vertex_data=vertex_data, normal_override=normal_override,
            meta=meta, script_paths=scripts, blender_version=bpy.app.version_string)
        lo, hi = LEVELS[level]["range"]
        print(f"[bibopzi] {level}: {tris} triangles, {verts} vertices (budget {lo}-{hi})")
        if not lo <= tris <= hi:
            raise SystemExit(f"{level}: {tris} triangles outside {lo}-{hi}")
        bpy.data.meshes.remove(mesh)


if __name__ == "__main__":
    try:
        main()
    except SystemExit as e:
        if e.code not in (None, 0):
            print(f"[bibopzi] FAILED: {e.code}")
            sys.stdout.flush()
            os._exit(1)
