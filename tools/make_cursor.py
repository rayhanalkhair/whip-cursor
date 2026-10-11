"""Draw cursor/whip-handle.cur: a 32x32 whip-handle pointer (stdlib only).

Usage: python tools/make_cursor.py
The hotspot (2, 2) is the handle tip where the app's whip attaches.
"""
import math
import struct
from pathlib import Path

SIZE = 32
HOTSPOT = (2, 2)
OUT = Path(__file__).resolve().parent.parent / "cursor" / "whip-handle.cur"

OUTLINE = (20, 12, 6)
GRIP = (122, 78, 40)
WRAP = (70, 44, 22)
POMMEL = (176, 128, 60)

# (kind, geometry, radius, rgb), painted in order
SHAPES = [
    ("seg", (2.5, 2.5, 19.0, 19.0), 3.6, OUTLINE),
    ("circle", (21.0, 21.0), 3.8, OUTLINE),
    ("seg", (2.5, 2.5, 19.0, 19.0), 2.4, GRIP),
    ("seg", (7.0, 10.0, 10.0, 7.0), 0.8, WRAP),
    ("seg", (11.5, 14.5, 14.5, 11.5), 0.8, WRAP),
    ("circle", (21.0, 21.0), 2.6, POMMEL),
]


def segment_distance(px, py, ax, ay, bx, by):
    dx, dy = bx - ax, by - ay
    t = max(0.0, min(1.0, ((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy)))
    return math.hypot(px - (ax + t * dx), py - (ay + t * dy))


def render():
    pixels = [[(0.0, 0.0, 0.0, 0.0) for _ in range(SIZE)] for _ in range(SIZE)]
    for kind, geometry, radius, rgb in SHAPES:
        sr, sg, sb = (c / 255 for c in rgb)
        for y in range(SIZE):
            for x in range(SIZE):
                cx, cy = x + 0.5, y + 0.5
                if kind == "seg":
                    d = segment_distance(cx, cy, *geometry)
                else:
                    d = math.hypot(cx - geometry[0], cy - geometry[1])
                a = max(0.0, min(1.0, radius + 0.5 - d))
                if a == 0.0:
                    continue
                br, bg, bb, ba = pixels[y][x]
                oa = a + ba * (1 - a)
                pixels[y][x] = (
                    (sr * a + br * ba * (1 - a)) / oa,
                    (sg * a + bg * ba * (1 - a)) / oa,
                    (sb * a + bb * ba * (1 - a)) / oa,
                    oa,
                )
    return pixels


def encode(pixels) -> bytes:
    xor = bytearray()
    for y in reversed(range(SIZE)):  # BMP rows are bottom-up
        for x in range(SIZE):
            r, g, b, a = pixels[y][x]
            xor += bytes((round(b * 255), round(g * 255), round(r * 255), round(a * 255)))
    and_mask = bytes(SIZE * 4)  # 1 bpp, 32 px = 4 bytes per row; alpha channel does the masking
    header = struct.pack("<IiiHHIIiiII", 40, SIZE, SIZE * 2, 1, 32, 0, len(xor) + len(and_mask), 0, 0, 0, 0)
    image = header + bytes(xor) + and_mask
    directory = struct.pack("<HHH", 0, 2, 1)
    entry = struct.pack("<BBBBHHII", SIZE, SIZE, 0, 0, HOTSPOT[0], HOTSPOT[1], len(image), 6 + 16)
    return directory + entry + image


def main() -> None:
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_bytes(encode(render()))
    print(f"wrote {OUT}")


if __name__ == "__main__":
    main()
