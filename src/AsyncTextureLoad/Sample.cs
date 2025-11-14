#define ENABLE_PROFILER

using System;
using UnityEngine.Profiling;

namespace AsyncTextureLoad;

internal struct Sample : IDisposable
{
    public Sample(string name)
    {
        Profiler.BeginSample(name);
    }

    public void Dispose()
    {
        Profiler.EndSample();
    }
}
