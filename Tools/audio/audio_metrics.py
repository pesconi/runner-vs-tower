"""Audio metrics and the before/after listening page (RT-6v3.9).

    python Tools/audio/audio_metrics.py                 # table of peak / loudness / duration of the game's clips
    python Tools/audio/audio_metrics.py --page          # also writes docs/audio/v3/escuta.html (open it on the PC or phone)

The metrics are the ones the game's validator uses (StyleFacts.ClipFact, same definitions, same targets from the StyleGuide):
  peak       highest sample, dBFS
  loudness   "active RMS": RMS in dBFS over the samples above -40 dBFS (silence and reverb tails do not count), the
             approximate volume of the clip while it sounds
  seconds    duration
Nothing here can say whether a sound is nice: the page exists so a person listens (the model does not hear).
"""
import html
import os
import sys
import wave

import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
AFTER = os.path.join(ROOT, "Assets", "Claybreak", "Content", "Audio")
BEFORE = os.path.join(ROOT, "docs", "audio", "v3", "antes")
PAGE = os.path.join(ROOT, "docs", "audio", "v3", "escuta.html")
GATE = 10 ** (-40.0 / 20.0)


def metrics(path):
    with wave.open(path, "rb") as w:
        n = w.getnframes()
        data = np.frombuffer(w.readframes(n), dtype="<i2").astype(np.float64) / 32768.0
        seconds = n / w.getframerate()
    peak = float(np.max(np.abs(data))) if n else 0.0
    active = data[np.abs(data) > GATE]
    rms = float(np.sqrt(np.mean(active ** 2))) if len(active) else 0.0
    to_db = lambda x: 20.0 * np.log10(x) if x > 0 else -120.0
    return dict(peak=to_db(peak), loudness=to_db(rms), seconds=seconds)


# event -> sound, in the order of the style guide's table (Tools mirror of StyleGuideContent.PlayedBy)
EVENTS = {
    "Coin": "compra na loja (sino curto duplo)",
    "GateGood": "passar por portão bom (catraca + 3 notas do motivo)",
    "GateBad": "portão ruim / perda por obstáculo (mola soltando)",
    "UnitLoss": "boneco estoura em peças e confete (assalto)",
    "ButtonClick": "toque de botão (tique de catraca)",
    "ButtonBack": "voltar (tique grave)",
    "CueNumberPop": "número do portão bom pulando",
    "CueKeyWind": "chave de corda girando ao começar a fase",
    "MenuLoop": "música do menu (motivo em dó maior)",
    "RunLoop": "música do mundo 1",
    "RunLoopW2": "música do mundo 2 (lá menor)",
    "RunLoopW3": "música do mundo 3 (fá maior, balanço)",
    "RunLoopW4": "música do mundo 4 (ré menor, mais rápida)",
}


def main():
    page = "--page" in sys.argv
    rows = []
    for name in sorted(f[:-4] for f in os.listdir(AFTER) if f.endswith(".wav")):
        after = metrics(os.path.join(AFTER, name + ".wav"))
        before_path = os.path.join(BEFORE, name + ".wav")
        before = metrics(before_path) if os.path.exists(before_path) else None
        rows.append((name, before, after))
        print(f"{name:22s} peak {after['peak']:6.1f}  loud {after['loudness']:6.1f}  {after['seconds']:5.2f} s")
    if page:
        write_page(rows)
        print("wrote", PAGE)


def cell(m):
    return "—" if m is None else f"pico {m['peak']:.1f} dBFS · volume {m['loudness']:.1f} dBFS · {m['seconds']:.2f} s"


def write_page(rows):
    changed = [r for r in rows if r[1] is not None or r[0] in EVENTS]
    out = ["<!doctype html><html lang='pt'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>",
           "<title>Stampeep — escuta do som (RT-6v3.9)</title>",
           "<style>body{font:16px system-ui;margin:16px;max-width:760px;background:#fff;color:#111}"
           "@media(prefers-color-scheme:dark){body{background:#111;color:#eee}}"
           ".c{border:1px solid #8886;border-radius:10px;padding:10px 12px;margin:10px 0}"
           "h2{font-size:17px;margin:0 0 4px}small{opacity:.75}audio{width:100%;margin:4px 0}</style></head><body>",
           "<h1>Som do Stampeep: antes e depois</h1>",
           "<p>Brinquedo de corda: catraca, mola, sino curto, estouro em peças, chave girando. Toda a música tem o mesmo motivo de 4 notas "
           "(sobe uma quinta, desce à terça, sobe à oitava) no tom de cada mundo. Tudo sintetizado por código; nada gerado por IA (D37 = C).</p>",
           "<p><b>Ouça com fone ou no alto-falante do celular.</b> Diga ao assistente: o que ficou bom, o que soa estranho, "
           "e se a música dos mundos se distingue.</p>"]
    for name, before, after in rows:
        if name not in EVENTS:
            continue
        out.append(f"<div class='c'><h2>{html.escape(name)}</h2><small>{html.escape(EVENTS[name])}</small>")
        if before is not None:
            out.append(f"<div><b>Antes</b> <small>{cell(before)}</small><audio controls preload='none' src='antes/{name}.wav'></audio></div>")
        out.append(f"<div><b>Depois</b> <small>{cell(after)}</small>"
                   f"<audio controls preload='none' src='../../../Assets/Claybreak/Content/Audio/{name}.wav'></audio></div></div>")
    out.append("<p><small>Gerado por Tools/audio/audio_metrics.py --page. Os arquivos de &laquo;depois&raquo; ficam em Assets/Claybreak/Content/Audio, "
               "relativos a esta página; abra-a de dentro da pasta do projeto.</small></p></body></html>")
    with open(PAGE, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out))


if __name__ == "__main__":
    main()
