using Hexagon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputerOpponent;

/// <summary>
/// Stores influence in a contiguous array so AI systems can read and update fields efficiently.
/// </summary>
public class BoardInfluenceMap : IInfluenceMap
{
    float[] values;

    public int Width { get; }
    public int Height { get; }
    public int Length => values.Length;

    public BoardInfluenceMap(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Width = width;
        Height = height;
        values = new float[width * height];
    }

    public float GetValue(int index)
    {
        ValidateIndex(index);
        return values[index];
    }

    public float GetValue(Hex hex)
    {
        return values[HexToIndex(hex)];
    }

    public void SetValue(int index, float value)
    {
        ValidateIndex(index);
        values[index] = value;
    }

    public void AddValue(int index, float value)
    {
        ValidateIndex(index);
        values[index] += value;
    }

    public void Clear()
    {
        Array.Clear(values, 0, values.Length);
    }

    public void Propagate(float decayFactor, int steps = 1)
    {
        if (decayFactor < 0f || decayFactor > 1f)
            throw new ArgumentOutOfRangeException(nameof(decayFactor));
        ArgumentOutOfRangeException.ThrowIfLessThan(steps, 1);

        for (var step = 0; step < steps; step++)
        {
            // Start with current values so source influence remains present while neighbors gain pressure.
            var propagatedValues = values.ToArray();

            for (var index = 0; index < values.Length; index++)
            {
                var sourceValue = values[index];
                if (sourceValue == 0f)
                    continue;

                var sourceHex = Hex.IndexToHex(index, Width);
                var decayedValue = sourceValue * decayFactor;
                var spreadPerNeighbor = decayedValue / 6f;

                foreach (var neighborHex in Hex.Neighbours(sourceHex))
                {
                    if (!TryHexToIndex(neighborHex, out var neighborIndex))
                        continue;

                    propagatedValues[neighborIndex] += spreadPerNeighbor;
                }
            }

            values = propagatedValues;
        }
    }

    public void AddRadialInfluence(Hex origin, float strength, int radius)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius));

        // Use ring-based falloff because AI threat is usually strongest at source and weaker with distance.
        for (var distance = 0; distance <= radius; distance++)
        {
            var ringInfluence = strength / (distance + 1f);
            var hexesInRing = Hex.HexRing(origin, distance, Width, Height);

            foreach (var hex in hexesInRing)
            {
                if (!TryHexToIndex(hex, out var index))
                    continue;

                values[index] += ringInfluence;
            }
        }
    }

    public float[] CopyValues()
    {
        return values.ToArray();
    }

    /// <summary>
    /// Combines weighted maps into a single decision field so different concerns (threat, opportunity, terrain) can be blended.
    /// </summary>
    public static BoardInfluenceMap Combine(int width, int height, IEnumerable<(IInfluenceMap map, float weight)> weightedMaps)
    {
        ArgumentNullException.ThrowIfNull(weightedMaps);

        var result = new BoardInfluenceMap(width, height);

        foreach (var weightedMap in weightedMaps)
        {
            var map = weightedMap.map;
            if (map == null)
                continue;

            if (map.Width != width || map.Height != height)
                throw new ArgumentException("All maps must have the same dimensions.", nameof(weightedMaps));

            for (var index = 0; index < result.Length; index++)
            {
                result.AddValue(index, map.GetValue(index) * weightedMap.weight);
            }
        }

        return result;
    }

    public static BoardInfluenceMap Combine(int width, int height, IEnumerable<IInfluenceMap> maps)
    {
        ArgumentNullException.ThrowIfNull(maps);

        return Combine(width, height, maps.Select(x => (x, 1f)));
    }

    int HexToIndex(Hex hex)
    {
        if (!TryHexToIndex(hex, out var index))
            throw new ArgumentOutOfRangeException(nameof(hex));

        return index;
    }

    bool TryHexToIndex(Hex hex, out int index)
    {
        var hexOffset = hex.q / 2;

        if (hex.q < 0 || hex.q >= Width || hex.r < -hexOffset || hex.r >= Height - hexOffset)
        {
            index = -1;
            return false;
        }

        index = Hex.HexToIndex(hex, Width, Height);
        return true;
    }

    void ValidateIndex(int index)
    {
        if (index < 0 || index >= values.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
    }
}