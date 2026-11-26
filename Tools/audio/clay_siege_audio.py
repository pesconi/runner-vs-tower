"""Stampeep audio, made by Jungle Degree (RT-8, redone as the wind-up toy set in RT-6v3.9): every sound effect and music loop.

Own work, synthesized from math only (no samples, no third-party audio): see docs/audio-licenses.md.
Deterministic (fixed seeds), so the same script always writes the same files.

    python Tools/audio/clay_siege_audio.py [output folder]

Default output: Assets/Claybreak/Content/Audio (44.1 kHz, 16-bit, mono WAV; Unity compresses on import).
File names are the clip names the scene builder looks up: one per kit sound role (Coin, GateGood, ...),
one per game cue (Cue<Name>) and MenuLoop / RunLoop.
"""
import os
import sys
import wave

import numpy as np
from scipy.signal import butter, fftconvolve, sosfilt

SR = 44100
TAU = 2.0 * np.pi


# ---------------------------------------------------------------- building blocks

def t_axis(seconds):
    return np.arange(int(seconds * SR)) / SR


def env_ad(seconds, attack, decay_rate):
    """Linear attack then exponential decay (decay_rate in 1/s)."""
    t = t_axis(seconds)
    a = np.clip(t / max(attack, 1e-4), 0.0, 1.0)
    return a * np.exp(-decay_rate * np.maximum(t - attack, 0.0))


def env_adsr(seconds, attack, decay, sustain, release):
    t = t_axis(seconds)
    e = np.full_like(t, sustain)
    e = np.where(t < attack, t / max(attack, 1e-4), e)
    d = (t >= attack) & (t < attack + decay)
    e = np.where(d, 1.0 - (1.0 - sustain) * (t - attack) / max(decay, 1e-4), e)
    rel_start = seconds - release
    r = t >= rel_start
    e = np.where(r, sustain * np.clip(1.0 - (t - rel_start) / max(release, 1e-4), 0.0, 1.0), e)
    return e


def sweep_phase(f0, f1, seconds, curve=1.0):
    """Phase of a pitch glide from f0 to f1 (curve > 1 drops fast first)."""
    t = t_axis(seconds)
    x = t / seconds
    f = f1 + (f0 - f1) * (1.0 - x) ** curve
    return TAU * np.cumsum(f) / SR


def sine(f, seconds):
    return np.sin(TAU * f * t_axis(seconds))


def tri(phase):
    return 2.0 / np.pi * np.arcsin(np.sin(phase))


def saw(phase, harmonics=12):
    out = np.zeros_like(phase)
    for k in range(1, harmonics + 1):
        out += ((-1) ** (k + 1)) * np.sin(k * phase) / k
    return out * (2.0 / np.pi)


def square(phase, harmonics=9):
    out = np.zeros_like(phase)
    for k in range(1, 2 * harmonics, 2):
        out += np.sin(k * phase) / k
    return out * (4.0 / np.pi)


def noise(seconds, seed):
    return np.random.default_rng(seed).uniform(-1.0, 1.0, int(seconds * SR))


def lowpass(x, cutoff, order=2):
    return sosfilt(butter(order, min(cutoff, SR * 0.45), "low", fs=SR, output="sos"), x)


def highpass(x, cutoff, order=2):
    return sosfilt(butter(order, cutoff, "high", fs=SR, output="sos"), x)


def bandpass(x, low, high, order=2):
    return sosfilt(butter(order, [low, min(high, SR * 0.45)], "band", fs=SR, output="sos"), x)


def swept_lowpass(x, f0, f1, blocks=64):
    """Time-varying low-pass (block-wise, crossfade-free is fine for noise)."""
    out = np.zeros_like(x)
    edges = np.linspace(0, len(x), blocks + 1).astype(int)
    zi = None
    for b in range(blocks):
        f = f0 + (f1 - f0) * b / max(blocks - 1, 1)
        sos = butter(2, min(max(f, 30.0), SR * 0.45), "low", fs=SR, output="sos")
        if zi is None:
            zi = np.zeros((sos.shape[0], 2))
        seg, zi = sosfilt(sos, x[edges[b]:edges[b + 1]], zi=zi)
        out[edges[b]:edges[b + 1]] = seg
    return out


def place(dest, src, start_seconds, gain=1.0):
    i = int(start_seconds * SR)
    n = min(len(src), len(dest) - i)
    if n > 0:
        dest[i:i + n] += src[:n] * gain
    return dest


def mix(*parts):
    n = max(len(p) for p in parts)
    out = np.zeros(n)
    for p in parts:
        out[:len(p)] += p
    return out


def fade(x, fade_in=0.002, fade_out=0.01):
    x = x.copy()
    a = int(fade_in * SR)
    b = int(fade_out * SR)
    if a > 0:
        x[:a] *= np.linspace(0.0, 1.0, a)
    if b > 0:
        x[-b:] *= np.linspace(1.0, 0.0, b)
    return x


def normalize(x, peak_db=-1.0):
    peak = np.max(np.abs(x))
    return x if peak == 0 else x * (10 ** (peak_db / 20.0) / peak)


def room(x, wet=0.18, seconds=0.9, seed=7):
    """Small, warm room: convolution with decaying filtered noise (deterministic)."""
    ir = noise(seconds, seed) * np.exp(-t_axis(seconds) * (6.9 / seconds))
    ir = lowpass(ir, 5000.0)
    ir[: int(0.012 * SR)] = 0.0
    ir /= np.sqrt(np.sum(ir ** 2)) + 1e-9
    tail = fftconvolve(x, ir)[: len(x) + int(seconds * SR)]
    dry = np.concatenate([x, np.zeros(len(tail) - len(x))])
    return dry * (1.0 - wet) + tail * wet * 0.6


def midi(n):
    return 440.0 * 2 ** ((n - 69) / 12.0)


# ---------------------------------------------------------------- instruments

def pluck(freq, seconds=0.35, bright=1.0, decay=9.0):
    """Marimba-like clay pluck: fundamental + soft overtone, quick decay."""
    t = t_axis(seconds)
    body = np.sin(TAU * freq * t) + 0.35 * bright * np.sin(TAU * freq * 4.0 * t) * np.exp(-t * 30)
    body += 0.15 * np.sin(TAU * freq * 2.0 * t) * np.exp(-t * 14)
    return body * env_ad(seconds, 0.003, decay)


def bell(freq, seconds=0.6, decay=6.0):
    t = t_axis(seconds)
    partials = [(1.0, 1.0), (2.76, 0.4), (5.4, 0.2), (8.93, 0.08)]
    out = sum(a * np.sin(TAU * freq * r * t) * np.exp(-t * decay * (1 + r * 0.25)) for r, a in partials)
    return out * env_ad(seconds, 0.002, 0.0)


def brass(freq, seconds, cutoff=1800.0, attack=0.04, vibrato=0.0):
    t = t_axis(seconds)
    f = freq * (1.0 + vibrato * np.sin(TAU * 5.5 * t) * np.clip(t / 0.25, 0, 1))
    phase = TAU * np.cumsum(f) / SR
    tone = saw(phase, 14) * 0.7 + saw(phase * 1.004, 14) * 0.3
    e = env_adsr(seconds, attack, 0.12, 0.75, min(0.18, seconds * 0.4))
    swell = lowpass(tone, cutoff)
    return swell * e


def kick(seconds=0.35, f0=150.0, f1=45.0, punch=1.0):
    body = np.sin(sweep_phase(f0, f1, seconds, 3.0)) * env_ad(seconds, 0.001, 9.0)
    click = lowpass(noise(0.012, 11), 3500.0) * punch * 0.6
    return mix(body, click)


def snare(seconds=0.22, seed=21):
    n = bandpass(noise(seconds, seed), 1200.0, 8000.0) * env_ad(seconds, 0.001, 22.0)
    tone = np.sin(sweep_phase(260.0, 180.0, seconds, 2.0)) * env_ad(seconds, 0.001, 30.0) * 0.5
    return mix(n, tone)


def hat(seconds=0.05, seed=31, open_=False):
    n = highpass(noise(seconds, seed), 7000.0) * env_ad(seconds, 0.0005, 18.0 if open_ else 70.0)
    return n


def shaker(seconds=0.09, seed=41):
    return bandpass(noise(seconds, seed), 4000.0, 11000.0) * env_adsr(seconds, 0.03, 0.02, 0.6, 0.04)


def thud(freq=80.0, seconds=0.25, seed=51, grit=0.5):
    """Wet clay thump: pitched body, a mid-range knock (what a phone speaker can play) and muffled noise."""
    body = np.sin(sweep_phase(freq * 1.8, freq, seconds, 2.5)) * env_ad(seconds, 0.001, 18.0)
    knock = np.sin(sweep_phase(freq * 6.5, freq * 4.0, seconds, 2.0)) * env_ad(seconds, 0.001, 38.0) * 0.6
    squish = bandpass(noise(seconds, seed), 250.0, 2500.0) * env_ad(seconds, 0.002, 30.0) * grit
    return mix(body * 0.8, knock, squish)


def rubble(seconds, density, seed, cutoff=2500.0, decay=3.0):
    """Many small clay chunks hitting the ground."""
    rng = np.random.default_rng(seed)
    out = np.zeros(int(seconds * SR))
    count = int(density * seconds)
    for _ in range(count):
        start = rng.uniform(0.0, seconds * 0.85) ** 1.4 / (seconds * 0.85) ** 0.4
        size = rng.uniform(0.3, 1.0)
        grain = thud(rng.uniform(90, 320) / size, 0.07 + 0.05 * size, int(rng.integers(1, 1 << 30)), 0.8)
        place(out, grain, start, size * np.exp(-decay * start / seconds) * rng.uniform(0.4, 1.0))
    return lowpass(out, cutoff)


# ---------------------------------------------------------------- sound effects (kit roles)









def sfx_finish():
    """Reached the firing line: a short war-horn stab over a drum."""
    horn = mix(brass(midi(57), 0.7, 1600.0, 0.05, 0.004), brass(midi(64), 0.7, 1600.0, 0.05, 0.004) * 0.7)
    drum = kick(0.4, 120.0, 50.0)
    out = mix(horn * 0.6, drum * 0.9)
    return room(out, 0.2, 0.6)


def sfx_victory():
    notes = [(0.0, 67, 0.16), (0.14, 72, 0.16), (0.28, 76, 0.16), (0.42, 79, 0.5), (0.9, 76, 0.16), (1.04, 84, 1.0)]
    out = np.zeros(int(2.3 * SR))
    for start, n, length in notes:
        place(out, brass(midi(n), length, 2600.0, 0.02), start, 0.45)
        place(out, bell(midi(n + 12), 0.5, 7.0), start, 0.12)
    for n in (60, 64, 67):
        place(out, brass(midi(n), 1.2, 1500.0, 0.05), 1.04, 0.22)
    place(out, kick(0.4, 110, 45), 1.04, 0.3)
    place(out, snare(0.3, 22), 1.04, 0.25)
    return room(out, 0.22, 1.0)


def sfx_defeat():
    out = np.zeros(int(2.0 * SR))
    for i, n in enumerate([55, 54, 53]):
        place(out, brass(midi(n), 0.38, 1100.0, 0.03, 0.006), i * 0.36, 0.5)
    seconds = 0.95
    ph = sweep_phase(midi(52), midi(50), seconds, 1.0)
    t = t_axis(seconds)
    wobble = 1.0 + 0.5 * np.sin(TAU * 6.0 * t)
    tail = lowpass(saw(ph, 12), 900.0) * env_adsr(seconds, 0.03, 0.1, 0.8, 0.4) * wobble
    place(out, tail, 1.08, 0.45)
    return room(out, 0.2, 0.8)






def sfx_ad_reward():
    out = np.zeros(int(1.0 * SR))
    for i, n in enumerate([76, 79, 83, 88, 91]):
        place(out, bell(midi(n), 0.5, 8.0), i * 0.07, 0.35)
    import stampeep_toy_audio as toy
    place(out, toy.toy_coin(), 0.32, 0.6)
    return room(out, 0.25, 0.7)


# ---------------------------------------------------------------- game cues

def cue_cannon_fire():
    seconds = 0.6
    boom = np.sin(sweep_phase(140.0, 38.0, seconds, 3.0)) * env_ad(seconds, 0.002, 7.0)
    boom += np.sin(sweep_phase(420.0, 220.0, seconds, 2.0)) * env_ad(seconds, 0.001, 22.0) * 0.5
    blast = swept_lowpass(noise(seconds, 91), 4500.0, 300.0) * env_ad(seconds, 0.001, 10.0) * 0.8
    crack = highpass(noise(0.03, 92), 1500.0) * env_ad(0.03, 0.0005, 120.0) * 0.5
    return room(mix(boom, blast, crack), 0.15, 0.5)


def cue_projectile_warn():
    """Falling whistle while a stone comes down on the army."""
    seconds = 0.6
    t = t_axis(seconds)
    f = 1700.0 - 900.0 * (t / seconds) ** 1.3
    f = f * (1.0 + 0.012 * np.sin(TAU * 9.0 * t))
    tone = np.sin(TAU * np.cumsum(f) / SR)
    air = bandpass(noise(seconds, 101), 1200.0, 3500.0) * 0.18
    e = env_adsr(seconds, 0.12, 0.05, 0.9, 0.08)
    return (tone * 0.55 + air) * e


def cue_stone_impact():
    seconds = 0.45
    hit = thud(70.0, seconds, 111, 0.9)
    crunch = rubble(0.4, 40, 112, 2200.0, 4.0) * 0.6
    return mix(hit, crunch)


def cue_tower_hit():
    """A soldier slams into the clay tower: short, muffled, frequent."""
    return thud(95.0, 0.14, 121, 0.7) * 0.8


def cue_floor_break():
    seconds = 0.9
    crack = highpass(noise(0.06, 131), 900.0) * env_ad(0.06, 0.0005, 60.0) * 0.6
    drop = thud(55.0, 0.5, 132, 1.0)
    debris = rubble(seconds, 55, 133, 2400.0, 3.5) * 0.8
    out = mix(crack, drop, np.concatenate([np.zeros(int(0.04 * SR)), debris]))
    return room(out, 0.18, 0.7)


def cue_wall_break():
    seconds = 0.6
    crack = highpass(noise(0.05, 141), 1200.0) * env_ad(0.05, 0.0005, 70.0) * 0.7
    body = thud(110.0, 0.3, 142, 0.9)
    debris = rubble(seconds, 45, 143, 3000.0, 4.0) * 0.6
    return mix(crack, body, debris)


def cue_sweep():
    seconds = 0.32
    n = noise(seconds, 151)
    t = t_axis(seconds)
    out = np.zeros_like(n)
    edges = np.linspace(0, len(n), 33).astype(int)
    for b in range(32):
        centre = 700.0 + 2600.0 * np.sin(np.pi * b / 31.0)
        out[edges[b]:edges[b + 1]] = bandpass(n, centre * 0.7, centre * 1.4)[edges[b]:edges[b + 1]]
    return out * env_adsr(seconds, 0.08, 0.05, 0.8, 0.15) * 0.8 + 0.0 * t


def cue_crate():
    out = np.zeros(int(0.5 * SR))
    place(out, thud(200.0, 0.1, 161, 0.5), 0.0, 0.5)
    place(out, bell(midi(81), 0.3, 10.0), 0.03, 0.5)
    place(out, bell(midi(88), 0.4, 9.0), 0.1, 0.5)
    return out


def cue_tower_fall():
    seconds = 2.6
    out = np.zeros(int(seconds * SR))
    place(out, highpass(noise(0.08, 171), 700.0) * env_ad(0.08, 0.0005, 40.0), 0.0, 0.6)
    rumble = lowpass(noise(seconds, 172), 180.0) * env_adsr(seconds, 0.15, 0.4, 0.7, 1.6) * 3.0
    place(out, rumble, 0.0, 0.6)
    for i, start in enumerate([0.05, 0.4, 0.75, 1.05]):
        place(out, thud(50.0 + 8 * i, 0.6, 173 + i, 1.0), start, 0.9 - 0.15 * i)
    place(out, rubble(2.2, 70, 180, 2600.0, 2.5), 0.1, 0.9)
    return room(out, 0.22, 1.2)


def cue_boss():
    seconds = 1.5
    out = np.zeros(int(1.9 * SR))
    for n, g in ((45, 0.5), (52, 0.35), (57, 0.25)):
        place(out, brass(midi(n), seconds, 900.0, 0.18, 0.005), 0.0, g)
    place(out, kick(0.6, 90.0, 35.0, 0.6), 0.0, 0.9)
    place(out, kick(0.6, 90.0, 35.0, 0.6), 0.45, 0.6)
    return room(out, 0.25, 1.0)


# ---------------------------------------------------------------- music

def render_loop(bars, bpm, beats_per_bar, voices):
    """Seamless loop: notes that ring past the end wrap to the start."""
    beat = 60.0 / bpm
    length = int(bars * beats_per_bar * beat * SR)
    out = np.zeros(length + int(3.0 * SR))
    for start_beat, sound, gain in voices:
        place(out, sound, start_beat * beat, gain)
    loop = out[:length].copy()
    tail = out[length:]
    loop[: len(tail)] += tail[: len(loop)]
    return loop


def menu_music():
    """Calm workshop tune, 96 BPM, 8 bars: C - Am - F - G, marimba melody, soft bass and pad."""
    bpm, bars = 96, 8
    chords = [(48, [60, 64, 67]), (45, [57, 60, 64]), (41, [57, 60, 65]), (43, [55, 59, 62])] * 2
    melody = [
        (0, 72), (1, 79), (2, 76), (3, 84),
        (4, 72), (5, 76), (6, 74), (7, 72),
        (8, 69), (9, 72), (10, 77), (10.5, 76), (11, 74),
        (12, 74), (13, 71), (14, 67), (15, 74),
        (16, 72), (17, 79), (18, 76), (19, 84),
        (20, 72), (21, 76), (22, 79), (23, 76),
        (24, 77), (25, 76), (26, 74), (26.5, 72), (27, 69),
        (28, 71), (29, 74), (30, 72),
    ]
    voices = []
    beat = 60.0 / bpm
    for bar, (root, triad) in enumerate(chords):
        b0 = bar * 4
        voices.append((b0, pluck(midi(root), 0.9, 0.3, 4.0), 0.55))
        voices.append((b0 + 2.5, pluck(midi(root + 7), 0.6, 0.3, 5.0), 0.35))
        pad = sum(lowpass(tri(TAU * midi(n) * t_axis(4 * beat)), 1200.0) for n in triad)
        voices.append((b0, pad * env_adsr(4 * beat, 0.5, 0.3, 0.7, 0.8), 0.06))
        for k in range(8):
            voices.append((b0 + k * 0.5, shaker(0.09, 41 + (k % 3)), 0.05 if k % 2 else 0.08))
    for start, n in melody:
        voices.append((start, pluck(midi(n), 0.6, 1.0, 7.0), 0.32))
        voices.append((start, bell(midi(n + 12), 0.4, 9.0), 0.05))
    loop = render_loop(bars, bpm, 4, voices)
    return np.tanh(normalize(room(loop, 0.2, 1.0)[: len(loop)], -2.0) * 1.1)




# ---------------------------------------------------------------- output

SOUNDS = {
    "Finish": sfx_finish,
    "Victory": sfx_victory,
    "Defeat": sfx_defeat,
    "AdReward": sfx_ad_reward,
    "CueCannonFire": cue_cannon_fire,
    "CueProjectileWarning": cue_projectile_warn,
    "CueStoneImpact": cue_stone_impact,
    "CueTowerHit": cue_tower_hit,
    "CueTowerFall": cue_tower_fall,
    "CueFloorBreak": cue_floor_break,
    "CueWallBreak": cue_wall_break,
    "CueSweep": cue_sweep,
    "CueCrate": cue_crate,
    "CueBoss": cue_boss,
}

MUSIC = {"MenuLoop": menu_music}


def trim_tail(x, floor_db=-54.0, keep=0.03):
    """Drops the silent end of a reverb tail (everything after the last sample above the floor)."""
    loud = np.nonzero(np.abs(x) > 10 ** (floor_db / 20.0))[0]
    end = min(len(x), (loud[-1] if len(loud) else 0) + int(keep * SR))
    return x[:end]


def write_wav(path, samples):
    data = np.clip(samples, -1.0, 1.0)
    pcm = (data * 32767.0).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(pcm.tobytes())


def main():
    import stampeep_toy_audio as toy
    SOUNDS.update(toy.TOY_SOUNDS)
    MUSIC.update(toy.WORLD_MUSIC)
    root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
    folder = sys.argv[1] if len(sys.argv) > 1 else os.path.join(root, "Assets", "Claybreak", "Content", "Audio")
    os.makedirs(folder, exist_ok=True)
    for name, make in SOUNDS.items():
        samples = fade(trim_tail(make()), 0.001, 0.015)
        samples = normalize(samples, -1.0)
        write_wav(os.path.join(folder, name + ".wav"), samples)
        print(f"{name:22s} {len(samples) / SR:5.2f} s")
    for name, make in MUSIC.items():
        samples = make()
        write_wav(os.path.join(folder, name + ".wav"), samples)
        edge = abs(samples[0] - samples[-1])
        print(f"{name:22s} {len(samples) / SR:5.2f} s  loop seam step {edge:.3f}")


if __name__ == "__main__":
    main()
