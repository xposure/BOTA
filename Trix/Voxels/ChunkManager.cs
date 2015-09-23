using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trix.Voxels
{
    public class ChunkManager
    {
        private const int GRID_SIZE = 8;
        public const int CHUNK_SIZE = 16;
        public const int CHUNK_SIZE2 = CHUNK_SIZE * CHUNK_SIZE;
        public const int CHUNK_SIZE3 = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
        public const int CHUNK_HEIGHT = 128;
        public const int CHUNKS_PER_COLUMN = CHUNK_HEIGHT / CHUNK_SIZE;

        private const int worldSize = GRID_SIZE * CHUNK_SIZE;

        private GraphicsDevice device;
        private ChunkColumn[,] grid = new ChunkColumn[GRID_SIZE, GRID_SIZE];
        private DefaultWorldGenerator worldGen = new DefaultWorldGenerator();

        public DefaultWorldGenerator WorldGenerator { get { return worldGen; } }


        public ChunkManager(GraphicsDevice device)
        {
            this.device = device;
        }

        public void Initialize()
        {
            worldGen.Init();
            worldGen.Start();

            var terrainTimer = new Stopwatch();
            var surfaceTimer = new Stopwatch();

            terrainTimer.Start();
            for (var x = 0; x < GRID_SIZE; x++)
            {
                for (var z = 0; z < GRID_SIZE; z++)
                {
                    grid[x, z] = new ChunkColumn(x, z, device);
                    grid[x, z].Init(this);
                }
            }
            terrainTimer.Stop();


            surfaceTimer.Start();
            for (var x = 0; x < GRID_SIZE; x++)
                for (var z = 0; z < GRID_SIZE; z++)
                    grid[x, z].UpdateMesh(this);

            surfaceTimer.Stop();

            System.Diagnostics.Trace.WriteLine("Terrain: " + terrainTimer.Elapsed.ToString());
            System.Diagnostics.Trace.WriteLine("Surface: " + surfaceTimer.Elapsed.ToString());


        }

        public uint GetVoxelByRelative(int cx, int cy, int cz, int x, int y, int z)
        {
            return GetVoxelByWorld(cx * CHUNK_SIZE + x, cy * CHUNK_SIZE + y, cz * CHUNK_SIZE + z);
        }

        public uint GetVoxelByWorld(int wx, int wy, int wz)
        {
            if (wx < 0 || wy < 0 || wz < 0 || wx >= worldSize || wy >= CHUNK_HEIGHT || wz >= worldSize)
                return 0;

            var cx = wx / CHUNK_SIZE;
            var cz = wz / CHUNK_SIZE;
            var cy = wy / CHUNK_SIZE;

            var column = grid[cx, cz];
            var volume = column[cy];

            return volume[wx - (cx * CHUNK_SIZE), wy - (cy * CHUNK_SIZE), wz - (cz * CHUNK_SIZE)];
        }

        public int Draw(GameTime gameTime, BasicEffect opaque, BasicEffect wireFrame, Camera camera)
        {

            var culled = 0;
            if (wireFrame != null)
            {
                var rast = new RasterizerState();
                rast.FillMode = FillMode.WireFrame;
                rast.CullMode = CullMode.None;
                //var depth = new DepthStencilState();
                //depth.DepthBufferFunction = CompareFunction.Never;
                //depth.DepthBufferEnable = true;
                //depth.DepthBufferWriteEnable = false;

                device.RasterizerState = rast;
                //device.DepthStencilState = depth;

                // TODO: Add your drawing code here
                foreach (EffectPass pass in wireFrame.CurrentTechnique.Passes)
                {
                    for (var x = 0; x < GRID_SIZE; x++)
                    {
                        for (var z = 0; z < GRID_SIZE; z++)
                        {
                            var column = grid[x, z];
                            for (var y = 0; y < CHUNKS_PER_COLUMN; y++)
                            {
                                //basicEffect.World = worldMatrix * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds / 2);
                                var chunk = column[y];
                                wireFrame.World = Matrix.CreateTranslation(chunk.WorldPosition);// *Matrix.CreateRotationZ((float)gameTime.TotalGameTime.TotalSeconds / 2);
                                pass.Apply();
                                chunk.Draw(camera);
                            }
                        }
                    }
                }
            }
            else
            {
                var rast = new RasterizerState();
                device.RasterizerState = rast;

                //var depth = DepthStencilState.Default;
                //device.DepthStencilState = depth;
                foreach (EffectPass pass in opaque.CurrentTechnique.Passes)
                {
                    for (var x = 0; x < GRID_SIZE; x++)
                    {
                        for (var z = 0; z < GRID_SIZE; z++)
                        {
                            var column = grid[x, z];
                            if (camera.Frustum.Intersects(column.AABB))
                            {
                                for (var y = 0; y < CHUNKS_PER_COLUMN; y++)
                                {
                                    //basicEffect.World = worldMatrix * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds / 2);
                                    var chunk = column[y];
                                    opaque.World = Matrix.CreateTranslation(chunk.WorldPosition);// *Matrix.CreateRotationZ((float)gameTime.TotalGameTime.TotalSeconds / 2);
                                    pass.Apply();
                                    if (!chunk.Draw(camera))
                                        culled++;
                                }
                            }
                            else
                                culled += ChunkManager.CHUNKS_PER_COLUMN;
                        }
                    }
                }
            }
            return culled;
        }
    }
}
