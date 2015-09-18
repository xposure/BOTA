using System;
using LibNoise;
using LibNoise.Combiner;
using LibNoise.Filter;
using LibNoise.Primitive;

public enum NoiseType
{
    Billow,
    RidgedMultiFractal,
    Voronoi,
    Mix,
    Perlin
}

public partial class Generators
{

    public static System.Func<int, int, int, uint> GenerateHeight(int cx, int cy, int cz, int width, int depth, NoiseType noise = NoiseType.Perlin, int sealevel = 8)
    {
        IModule3D moduleBase;
        switch (noise)
        {
            case NoiseType.Billow:
                moduleBase = new Billow();
                break;

            case NoiseType.RidgedMultiFractal:
                moduleBase = new RidgedMultiFractal();
                break;

            case NoiseType.Voronoi:
                moduleBase = new Voronoi();
                break;

            case NoiseType.Mix:
                SimplexPerlin perlin = new SimplexPerlin();
                RidgedMultiFractal rigged = new RidgedMultiFractal();
                moduleBase = new Add(perlin, rigged);
                break;

            default:
                moduleBase = new SimplexPerlin();
                break;

        }

        var data = new uint[width, depth];
        for (var x = 0; x < width; ++x)
        {
            for (var z = 0; z < depth; ++z)
            {
                var wx = cx * width + x;
                var wz = cz * depth + z;
                data[x, z] = (uint)(moduleBase.GetValue(wx * 0.035f, wz * 0.035f, 0.95f) * 2 + 8);
                data[x, z] += (uint)(moduleBase.GetValue(wx * 0.015f, wz * 0.015f, 0.15f) * moduleBase.GetValue(wx * 0.025f, wz * 0.025f, 0.45f) * 7 + 2);
            }
        }

        var random = new Random(1234);
        return (i, j, k) =>
        {
            if (data[i, k] > j)
            {
                if (sealevel > j)
                    return 0xD6D68Eu;

                var terrainNoise = random.NextDouble();
                if (terrainNoise < 0.005f)
                    return 0x88A552u;
                else if (terrainNoise < 0.01f)
                    return 0x9CCB6Bu;

                return 0x5F9E35u;
            }
            else if (sealevel > j)
                return 0x177B0E5u;


            return 0;
        };
    }
}
