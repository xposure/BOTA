using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNoise;
using LibNoise.Modifiers;
using Microsoft.Xna.Framework;
using Trix.Rendering;
using Trix.Voxels;

namespace Trix
{
    public class DefaultWorldGenerator
    {
        public uint ToBlock(Color color)
        {
            return (uint)((color.R << 16) + (color.G << 8) + color.B);
        }

        public void Start()
        {
            //m.RegisterWorldGenerator(GetChunk);
            //m.RegisterOptionBool("DefaultGenCaves", false);
            //m.RegisterOptionBool("DefaultGenLavaCaves", false);

            BLOCK_STONE = ToBlock(Color.Gray);
            BLOCK_DIRT = ToBlock(Color.Brown);
            BLOCK_SAND = ToBlock(Color.Tan);
            BLOCK_CLAY = ToBlock(Color.SlateGray);
            BLOCK_BEDROCK = ToBlock(Color.DarkGray);
            BLOCK_AIR = 0;
            BLOCK_SNOW = ToBlock(Color.White);
            BLOCK_ICE = ToBlock(Color.LightBlue);
            BLOCK_GRASS = ToBlock(Color.Green);
            BLOCK_WATER = ToBlock(Color.Blue);
            BLOCK_GRAVEL = ToBlock(Color.LightGray);
            BLOCK_PUMPKIN = ToBlock(Color.Orange);
            BLOCK_RED_ROSE = ToBlock(Color.Red);
            BLOCK_YELLOW_FLOWER = ToBlock(Color.Yellow);
            BLOCK_LAVA = ToBlock(Color.OrangeRed);
        }


        bool started = false;
        public void GetChunk(int x, int y, int z, VoxelVolume chunk)
        {
            if (!started)
            {
                Init();
                started = true;
            }

            //for (var xx = 0; xx < ChunkManager.CHUNK_SIZE; xx++)
            //    for (var zz = 0; zz < ChunkManager.CHUNK_SIZE; zz++)
            //        chunk[xx, 0, zz] = 0xffffff;

            //chunk[0, 1, 0] = 0xffffff;
            //chunk[0, 1, 1] = 0xffffff;
            //chunk[0, 1, 2] = 0xffffff;
            //chunk[0, 1, 3] = 0xffffff;
            ////chunk[1, 2, 1] = 0xffffff;
            ////chunk[1, 2, 2] = 0xffffff;
            ////chunk[2, 2, 2] = 0xffffff;
            ////chunk[1, 0, 1] = 0xffffff;
            ////chunk[1, 0, 2] = 0xffffff;
            ////chunk[2, 0, 2] = 0xffffff;
            //return;

            bool addCaves = false; // (bool)m.GetOption("DefaultGenCaves");
            bool addCaveLava = false; // (bool)m.GetOption("DefaultGenLavaCaves");
            int ChunkSize = Constants.CHUNK_SIZE;// m.GetChunkSize();
            x *= ChunkSize;
            y *= ChunkSize;
            z *= ChunkSize;
            int chunksize = ChunkSize;
            var noise = new FastNoise();
            noise.Frequency = 0.01;

            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    int currentHeight = (byte)((finalTerrain.GetValue((xx + x) / 100.0f, 0, (yy + y) / 100.0f) * 60) + 64);
                    int ymax = currentHeight;

                    int biome = (int)(BiomeSelect.GetValue((x + xx) / 100.0f, 0, (y + yy) / 100.0f) * 2); //MD * 2
                    uint toplayer = BLOCK_DIRT;
                    if (biome == 0)
                    {
                        toplayer = BLOCK_DIRT;
                    }
                    if (biome == 1)
                    {
                        toplayer = BLOCK_SAND;
                    }
                    if (biome == 2)
                    {
                        toplayer = BLOCK_DIRT;
                    }
                    if (biome == 3)
                    {
                        toplayer = BLOCK_DIRT;
                    }
                    if (biome == 4)
                    {
                        toplayer = BLOCK_DIRT;
                    }
                    if (biome == 5)
                    {
                        toplayer = BLOCK_CLAY;
                    }

                    int stoneHeight = (int)currentHeight - ((64 - (currentHeight % 64)) / 8) + 1;

                    if (ymax < seaLevel)
                    {
                        ymax = seaLevel;
                    }
                    ymax++;
                    if (ymax > z + chunksize - 1)
                    {
                        ymax = z + chunksize - 1;
                    }
                    for (int bY = z; bY <= ymax; bY++)
                    {
                        uint curBlock = 0;

                        // Place bedrock
                        if (bY == 0)
                        {
                            curBlock = BLOCK_BEDROCK;
                        }
                        else if (bY < currentHeight)
                        {
                            if (bY < stoneHeight)
                            {
                                curBlock = BLOCK_STONE;
                                // Add caves
                                if (addCaves)
                                {
                                    if (caveNoise.GetValue((x + xx) / 4.0f, (bY) / 1.5f, (y + yy) / 4.0f) > cavestreshold)
                                    {
                                        if (bY < 10 && addCaveLava)
                                        {
                                            curBlock = BLOCK_LAVA;
                                        }
                                        else
                                        {
                                            curBlock = BLOCK_AIR;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                curBlock = toplayer;
                            }
                        }
                        else if ((currentHeight + 1) == bY && bY > seaLevel && biome == 3)
                        {
                            curBlock = BLOCK_SNOW;
                            continue;
                        }
                        else if ((currentHeight + 1) == bY && bY > seaLevel + 1)
                        {
                            if (biome == 1 || biome == 0)
                            {
                                continue;
                            }
                            double f = flowers.GetValue(x + xx / 10.0f, 0, y + yy / 10.0f);
                            if (f < -0.999)
                            {
                                curBlock = BLOCK_RED_ROSE;
                            }
                            else if (f > 0.999)
                            {
                                curBlock = BLOCK_YELLOW_FLOWER;
                            }
                            else if (f < 0.001 && f > -0.001)
                            {
                                curBlock = BLOCK_PUMPKIN;
                            }
                        }
                        else if (currentHeight == bY)
                        {
                            if (bY == seaLevel || bY == seaLevel - 1 || bY == seaLevel - 2)
                            {
                                curBlock = BLOCK_SAND;  // FF
                            }
                            else if (bY < seaLevel - 1)
                            {
                                curBlock = BLOCK_GRAVEL;  // FF
                            }
                            else if (toplayer == BLOCK_DIRT)
                            {
                                curBlock = BLOCK_GRASS;
                            }
                            else
                            {
                                curBlock = toplayer; // FF
                            }
                        }
                        else
                        {
                            if (bY <= seaLevel)
                            {
                                curBlock = BLOCK_WATER;  // FF
                            }
                            else
                            {
                                curBlock = BLOCK_AIR;  // FF
                            }
                            if (bY == seaLevel && biome == 3)
                            {
                                curBlock = BLOCK_ICE;
                            }
                        }
                        //var idx = xx + chunksize * ((yy) + chunksize * (bY - z));
                        chunk[xx, bY - z, yy] = curBlock;
                        //chunk[idx] = curBlock;
                        //chunk[m.Index3d(xx, yy, bY - z, chunksize, chunksize)] = curBlock;
                    }
                }
            }
        }

        int seaLevel = 62;
        uint BLOCK_STONE;
        uint BLOCK_DIRT;
        uint BLOCK_SAND;
        uint BLOCK_CLAY; //stone
        uint BLOCK_BEDROCK;
        uint BLOCK_AIR;
        uint BLOCK_SNOW; //todo
        uint BLOCK_ICE; //todo
        uint BLOCK_GRASS;
        uint BLOCK_WATER;
        uint BLOCK_GRAVEL;
        uint BLOCK_PUMPKIN; //hay
        uint BLOCK_RED_ROSE;
        uint BLOCK_YELLOW_FLOWER;
        uint BLOCK_LAVA;

        public void Init()
        {
            int Seed = 123456;

            BiomeBase.Frequency = (0.2);
            BiomeBase.Seed = (Seed - 1);
            BiomeSelect = new ScaleBiasOutput(BiomeBase);
            BiomeSelect.Scale = (2.5);
            BiomeSelect.Bias = (2.5);
            mountainTerrainBase.Seed = (Seed + 1);
            mountainTerrain = new ScaleBiasOutput(mountainTerrainBase);
            mountainTerrain.Scale = (0.5);
            mountainTerrain.Bias = (0.5);
            jaggieEdges = new Select(jaggieControl, terrainType, plain);
            plain.Value = (0.5);
            jaggieEdges.SetBounds(0.5, 1.0);
            jaggieEdges.EdgeFalloff = (0.11);
            jaggieControl.Seed = (Seed + 20);
            baseFlatTerrain.Seed = (Seed);
            baseFlatTerrain.Frequency = (0.2);
            flatTerrain = new ScaleBiasOutput(baseFlatTerrain);
            flatTerrain.Scale = (0.125);
            flatTerrain.Bias = (0.07);
            baseWater.Seed = (Seed - 1);
            water = new ScaleBiasOutput(baseWater);
            water.Scale = (0.3);
            water.Bias = (-0.5);
            terrainType.Seed = (Seed + 2);
            terrainType.Frequency = (0.5);
            terrainType.Persistence = (0.25);
            terrainType2.Seed = (Seed + 7);
            terrainType2.Frequency = (0.5);
            terrainType2.Persistence = (0.25);
            waterTerrain = new Select(terrainType2, water, flatTerrain);
            waterTerrain.EdgeFalloff = (0.1);
            waterTerrain.SetBounds(-0.5, 1.0);
            secondTerrain = new Select(terrainType, mountainTerrain, waterTerrain);
            secondTerrain.EdgeFalloff = (0.3);
            secondTerrain.SetBounds(-0.5, 1.0);
            finalTerrain = new Select(jaggieEdges, secondTerrain, waterTerrain);
            finalTerrain.EdgeFalloff = (0.2);
            finalTerrain.SetBounds(-0.3, 1.0);
            flowers.Seed = (Seed + 10);
            flowers.Frequency = (3);

            // Set up us the Perlin-noise module.
            caveNoise.Seed = (Seed + 22);
            caveNoise.Frequency = (1.0 / cavessize);
            caveNoise.OctaveCount = (4);
        }
        // Heightmap composition
        FastNoise BiomeBase = new FastNoise();
        ScaleBiasOutput BiomeSelect;
        RidgedMultifractal mountainTerrainBase = new RidgedMultifractal();
        ScaleBiasOutput mountainTerrain;
        Billow baseFlatTerrain = new Billow();
        ScaleBiasOutput flatTerrain;
        Billow baseWater = new Billow();
        ScaleBiasOutput water;
        FastNoise terrainType = new FastNoise();
        FastNoise terrainType2 = new FastNoise();
        Select waterTerrain;
        Select finalTerrain;
        Voronoi flowers = new Voronoi();
        Select jaggieEdges;
        Select secondTerrain;
        Constant plain = new Constant(0);
        Billow jaggieControl = new Billow();

        RidgedMultifractal caveNoise = new RidgedMultifractal();

        float cavessize = 15;
        float cavestreshold = 0.6f;
    }

}
