namespace ReservoirSimulation.Domain;

internal sealed class CorrelatedSpectralNoise
{
    private readonly Mode[] _modes;
    private readonly double _scale;

    internal CorrelatedSpectralNoise(
        int seed,
        ulong stream,
        int modeCount,
        double correlationLengthXM,
        double correlationLengthYM,
        double correlationLengthZM)
    {
        var random = new SplitMix64(((ulong)(uint)seed << 32) ^ stream);
        _modes = new Mode[modeCount];
        for (int index = 0; index < modeCount; index++)
        {
            _modes[index] = new Mode(
                random.NextGaussian() / correlationLengthXM,
                random.NextGaussian() / correlationLengthYM,
                random.NextGaussian() / correlationLengthZM,
                2 * Math.PI * random.NextDouble());
        }
        _scale = Math.Sqrt(2.0 / modeCount);
    }

    internal double At(double xM, double yM, double zM)
    {
        double value = 0;
        foreach (Mode mode in _modes)
            value += Math.Cos(mode.Kx * xM + mode.Ky * yM + mode.Kz * zM + mode.Phase);
        return Math.Clamp(value * _scale, -3.5, 3.5);
    }

    // ponytail: random Fourier features keep this slice dependency-free; upgrade to FFT-based geostatistics when conditioning scale demands it.
    private readonly record struct Mode(double Kx, double Ky, double Kz, double Phase);

    private sealed class SplitMix64(ulong seed)
    {
        private ulong _state = seed;

        internal double NextDouble() => (NextUInt64() >> 11) * (1.0 / 9_007_199_254_740_992.0);

        internal double NextGaussian()
        {
            double u1 = Math.Max(NextDouble(), double.Epsilon);
            double u2 = NextDouble();
            return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
        }

        private ulong NextUInt64()
        {
            ulong value = _state += 0x9E3779B97F4A7C15UL;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }
}
