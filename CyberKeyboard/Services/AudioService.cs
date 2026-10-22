using System;
using System.IO;
using System.Media;

namespace CyberKeyboard.Services
{
    public class AudioService
    {
        private SoundPlayer? _clickPlayer;
        private SoundPlayer? _specialPlayer;
        public bool IsSoundEnabled { get; set; } = true;

        public AudioService()
        {
            try
            {
                byte[] clickWav = GenerateBeepWav(1100, 18, 0.15f);
                _clickPlayer = new SoundPlayer(new MemoryStream(clickWav));
                _clickPlayer.Load();

                byte[] specialWav = GenerateBeepWav(800, 25, 0.20f);
                _specialPlayer = new SoundPlayer(new MemoryStream(specialWav));
                _specialPlayer.Load();
            }
            catch
            {
                // Audio initialization fallback
            }
        }

        public void PlayKeySound(bool isSpecial = false)
        {
            if (!IsSoundEnabled) return;
            try
            {
                if (isSpecial)
                {
                    _specialPlayer?.Play();
                }
                else
                {
                    _clickPlayer?.Play();
                }
            }
            catch
            {
                // Silent fail
            }
        }

        private static byte[] GenerateBeepWav(int frequency, int durationMs, float volume)
        {
            int sampleRate = 22050;
            short bitsPerSample = 16;
            short channels = 1;

            int totalSamples = (sampleRate * durationMs) / 1000;
            int byteRate = sampleRate * channels * (bitsPerSample / 8);
            short blockAlign = (short)(channels * (bitsPerSample / 8));
            int dataSize = totalSamples * (bitsPerSample / 8);
            int chunkSize = 36 + dataSize;

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // RIFF chunk descriptor
            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(chunkSize);
            writer.Write(new[] { 'W', 'A', 'V', 'E' });

            // "fmt " sub-chunk
            writer.Write(new[] { 'f', 'm', 't', ' ' });
            writer.Write(16); // SubChunk1Size (16 for PCM)
            writer.Write((short)1); // AudioFormat (1 for PCM)
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);

            // "data" sub-chunk
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            // Generate sine wave with soft exponential decay for a crisp tech click
            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;
                double decay = Math.Exp(-5.0 * i / totalSamples); // Fast soft decay
                double sample = Math.Sin(2.0 * Math.PI * frequency * t) * decay * volume;
                short sampleShort = (short)(sample * short.MaxValue);
                writer.Write(sampleShort);
            }

            return ms.ToArray();
        }
    }
}
