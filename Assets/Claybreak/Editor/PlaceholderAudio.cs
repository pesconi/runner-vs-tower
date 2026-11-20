using System;
using System.IO;
using JungleDegree.StudioKit.Core;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// RT-2 placeholder sounds, synthesized here (no third-party source, so no license to track). The
    /// kit ships only the sound engine; the Wild Pack's clips are its content and are never copied.
    /// One short WAV per <see cref="GameSound"/> role plus a menu and a run loop. Replaced in RT-8.
    /// </summary>
    public static class PlaceholderAudio
    {
        private const int SampleRate = 22050;

        public static AudioClip Get(string folder, GameSound sound)
        {
            return Write(folder, sound.ToString(), Synthesize(sound), false);
        }

        public static AudioClip GetMusic(string folder, bool run)
        {
            return Write(folder, run ? "RunLoop" : "MenuLoop", Loop(run), true);
        }

        private static float[] Synthesize(GameSound sound)
        {
            switch (sound)
            {
                case GameSound.Coin: return Thump(0.07f, 330f, 0.6f);
                case GameSound.GateGood: return Sweep(0.18f, 520f, 1040f, 0.5f);
                case GameSound.GateBad: return Sweep(0.22f, 420f, 180f, 0.5f);
                case GameSound.UnitLoss: return Thump(0.16f, 90f, 0.9f);
                case GameSound.Finish: return Notes(0.12f, 0.45f, 392f, 523f, 659f);
                case GameSound.Victory: return Notes(0.16f, 0.5f, 523f, 659f, 784f, 1046f);
                case GameSound.Defeat: return Notes(0.22f, 0.45f, 392f, 330f, 262f);
                case GameSound.ButtonClick: return Thump(0.04f, 900f, 0.4f);
                case GameSound.ButtonBack: return Thump(0.05f, 600f, 0.4f);
                default: return Sweep(0.3f, 660f, 1320f, 0.4f);
            }
        }

        // A clay "thock": a low sine with a fast pitch drop and decay.
        private static float[] Thump(float seconds, float frequency, float volume)
        {
            var samples = new float[(int)(seconds * SampleRate)];
            double phase = 0d;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / samples.Length;
                phase += 2d * Math.PI * frequency * (1f - 0.5f * t) / SampleRate;
                samples[i] = (float)Math.Sin(phase) * volume * (1f - t) * (1f - t);
            }

            return samples;
        }

        private static float[] Sweep(float seconds, float from, float to, float volume)
        {
            var samples = new float[(int)(seconds * SampleRate)];
            double phase = 0d;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / samples.Length;
                phase += 2d * Math.PI * Mathf.Lerp(from, to, t) / SampleRate;
                float envelope = Mathf.Min(1f, t * 20f) * (1f - t);
                samples[i] = (float)(Math.Sign(Math.Sin(phase)) * 0.3d + Math.Sin(phase) * 0.7d) * volume * envelope;
            }

            return samples;
        }

        private static float[] Notes(float noteSeconds, float volume, params float[] frequencies)
        {
            int noteLength = (int)(noteSeconds * SampleRate);
            var samples = new float[noteLength * frequencies.Length + noteLength];
            for (int n = 0; n < frequencies.Length; n++)
            {
                int length = n == frequencies.Length - 1 ? noteLength * 2 : noteLength;
                for (int i = 0; i < length; i++)
                {
                    float t = (float)i / length;
                    double phase = 2d * Math.PI * frequencies[n] * i / SampleRate;
                    samples[n * noteLength + i] += (float)Math.Sin(phase) * volume * Mathf.Min(1f, t * 30f) * (1f - t);
                }
            }

            return samples;
        }

        // Two-bar bass pulse; the run loop is faster and a fifth higher.
        private static float[] Loop(bool run)
        {
            float beat = run ? 0.25f : 0.5f;
            float[] roots = run ? new[] { 110f, 110f, 147f, 131f } : new[] { 98f, 87f, 98f, 73f };
            int beatLength = (int)(beat * SampleRate);
            var samples = new float[beatLength * 16];
            for (int b = 0; b < 16; b++)
            {
                float frequency = roots[b / 4];
                for (int i = 0; i < beatLength; i++)
                {
                    float t = (float)i / beatLength;
                    double phase = 2d * Math.PI * frequency * i / SampleRate;
                    samples[b * beatLength + i] = (float)Math.Sin(phase) * 0.35f * (1f - t) * Mathf.Min(1f, t * 50f);
                }
            }

            return samples;
        }

        private static AudioClip Write(string folder, string name, float[] samples, bool music)
        {
            string path = folder + "/" + name + ".wav";
            byte[] bytes = Encode(samples);
            bool changed = !File.Exists(path) || !File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes);
            if (changed)
            {
                File.WriteAllBytes(path, bytes);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            AudioClipLoadType loadType = music ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            if (settings.loadType != loadType || !importer.forceToMono)
            {
                settings.loadType = loadType;
                importer.defaultSampleSettings = settings;
                importer.forceToMono = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<AudioClip>(path)
                ?? throw new InvalidOperationException("Could not import " + path);
        }

        // 16-bit PCM mono WAV.
        private static byte[] Encode(float[] samples)
        {
            using var stream = new MemoryStream(44 + samples.Length * 2);
            using var writer = new BinaryWriter(stream);
            writer.Write(new[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
            writer.Write(36 + samples.Length * 2);
            writer.Write(new[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E', (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(SampleRate);
            writer.Write(SampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(new[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
            writer.Write(samples.Length * 2);
            foreach (float sample in samples)
            {
                writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
            }

            writer.Flush();
            return stream.ToArray();
        }
    }
}
