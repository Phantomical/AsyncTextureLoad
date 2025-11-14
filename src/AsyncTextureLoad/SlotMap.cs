using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncTextureLoad;

class SlotMap<T>
{
    const ushort OCCUPIED_MASK = 1 << 15;

    readonly T[] data;
    readonly ushort[] gen;
    readonly List<ushort> freelist;

    public SlotMap(int capacity)
    {
        if (capacity < 0 || capacity > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        data = new T[capacity];
        gen = new ushort[capacity];
        freelist = [.. Enumerable.Range(0, capacity).Select(v => (ushort)v)];
    }

    public bool TryAdd(T value, out uint index)
    {
        ushort shortidx;
        lock (data)
        {
            if (!freelist.TryPop(out shortidx))
            {
                index = uint.MaxValue;
                return false;
            }
        }

        var idx = (uint)shortidx;
        var gen = (uint)this.gen[idx];
        this.gen[idx] |= OCCUPIED_MASK;

        data[idx] = value;
        index = (idx << 16) | gen;
        return true;
    }

    public bool Take(uint index, out T value)
    {
        uint idx = index >> 16;
        uint gen = index & 0xFFFF;

        value = default;
        if (idx >= data.Length)
            return false;

        if (this.gen[idx] != gen)
            return false;

        value = data[idx];
        this.gen[idx] = (ushort)((gen + 1) & ~OCCUPIED_MASK);
        this.data[idx] = default;

        lock (data)
        {
            freelist.Add((ushort)idx);
        }

        return true;
    }
}

static class ListExt
{
    public static bool TryPop<T>(this List<T> list, out T value)
    {
        if (list.Count == 0)
        {
            value = default;
            return false;
        }

        value = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return true;
    }
}
