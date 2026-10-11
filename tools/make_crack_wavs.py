"""Synthesize placeholder whip-crack WAVs (stdlib only).

Usage: python tools/make_crack_wavs.py
Writes src/WhipCursor/Assets/sounds/crack{1,2,3}.wav. Replace them with real CC0 recordings any time.
"""
import math
import random
import struct
import wave
from pathlib import Path

RATE = 44100
OUT_DIR = Path(__file__).resolve().parent.parent / "src" / "WhipCursor" / "Assets" / "sounds"


def crack(seed: int, snap_decay: float, tail_decay: float, length_s: float = 0.35) -> list[float]:
    rng = random.Random(seed)
    samples = []
    prev_noise = 0.0
    low = 0.0
    for i in range(int(RATE * length_s)):
        t = i / RATE
        noise = rng.uniform(-1.0, 1.0)
        bright = noise - prev_noise  # first difference = crude high-pass → sharp "snap"
        prev_noise = noise
        low += 0.15 * (noise - low)  # one-pole low-pass → body of the crack
        snap = bright * math.exp(-t / snap_decay)
        tail = low * math.exp(-t / tail_decay) * 0.8
        samples.append(snap * 0.9 + tail)
    peak = max(abs(s) for s in samples) or 1.0
    return [s / peak * 0.9 for s in samples]


def write_wav(path: Path, samples: list[float]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(RATE)
        wav.writeframes(b"".join(struct.pack("<h", int(s * 32767)) for s in samples))


def main() -> None:
    variants = [(1, 0.010, 0.050), (2, 0.014, 0.065), (3, 0.008, 0.045)]
    for index, (seed, snap_decay, tail_decay) in enumerate(variants, start=1):
        target = OUT_DIR / f"crack{index}.wav"
        write_wav(target, crack(seed, snap_decay, tail_decay))
        print(f"wrote {target}")


if __name__ == "__main__":
    main()
