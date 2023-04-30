using System;
using System.Buffers;

namespace LiverDie.Hospital.Generation
{
    /// <summary>
    /// A "rail" acts as a line where rooms are able to spawn.
    /// </summary>
    public class Rail : IDisposable
    {
        private readonly int _size;
        private readonly float[] _samples;
        private readonly int _sampleFrequency;

        public float Lower { get; set; }

        public float Upper { get; set; }

        public float SampleDistance { get; }

        public Rail(int size, int sampleFrequency)
        {
            _size = size;
            _sampleFrequency = sampleFrequency;
            _samples = ArrayPool<float>.Shared.Rent(size);
            SampleDistance = 1f / _sampleFrequency;
        }

        public float Evaluate(float position)
        {
            var index = (int)(position * _sampleFrequency);
            if (index >= _size)
                return _samples[_size - 1];

            return 0 > index ? _samples[0] : _samples[index];
        }

        public void AddDepthSample(float position, float value)
        {
            var index = (int)(position * _sampleFrequency);
            if (index >= _size || 0 > index)
                return;

            _samples[index] = value;
        }

        public void Dispose()
        {
            ArrayPool<float>.Shared.Return(_samples);
        }
    }
}
