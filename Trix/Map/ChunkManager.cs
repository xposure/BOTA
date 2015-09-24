using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trix.Map
{
    public class ChunkManager
    {

        private GraphicsDevice device;
        private ChunkColumn[,] grid = new ChunkColumn[Constants.GRID_SIZE, Constants.GRID_SIZE];
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
            for (var x = 0; x < Constants.GRID_SIZE; x++)
            {
                for (var z = 0; z < Constants.GRID_SIZE; z++)
                {
                    grid[x, z] = new ChunkColumn(this, x, z, device);
                    grid[x, z].Init(this);
                }
            }
            terrainTimer.Stop();


            surfaceTimer.Start();
            for (var x = 0; x < Constants.GRID_SIZE; x++)
                for (var z = 0; z < Constants.GRID_SIZE; z++)
                    grid[x, z].UpdateMesh(this);

            surfaceTimer.Stop();

            System.Diagnostics.Trace.WriteLine("Terrain: " + terrainTimer.Elapsed.ToString());
            System.Diagnostics.Trace.WriteLine("Surface: " + surfaceTimer.Elapsed.ToString());


        }

        public uint GetVoxelByRelative(int cx, int cy, int cz, int x, int y, int z)
        {
            return GetVoxelByWorld(cx * Constants.CHUNK_SIZE + x, cy * Constants.CHUNK_SIZE + y, cz * Constants.CHUNK_SIZE + z);
        }

        public uint GetVoxelByWorld(int wx, int wy, int wz)
        {
            if (wx < 0 || wy < 0 || wz < 0 || wx >= Constants.worldSize || wy >= Constants.CHUNK_HEIGHT || wz >= Constants.worldSize)
                return 0;

            var cx = wx / Constants.CHUNK_SIZE;
            var cz = wz / Constants.CHUNK_SIZE;
            var cy = wy / Constants.CHUNK_SIZE;

            var column = grid[cx, cz];
            var volume = column[cy];

            return volume[wx - (cx * Constants.CHUNK_SIZE), wy - (cy * Constants.CHUNK_SIZE), wz - (cz * Constants.CHUNK_SIZE)];
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
                    for (var x = 0; x < Constants.GRID_SIZE; x++)
                    {
                        for (var z = 0; z < Constants.GRID_SIZE; z++)
                        {
                            var column = grid[x, z];
                            for (var y = 0; y < Constants.CHUNKS_PER_COLUMN; y++)
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
                    for (var x = 0; x < Constants.GRID_SIZE; x++)
                    {
                        for (var z = 0; z < Constants.GRID_SIZE; z++)
                        {
                            var column = grid[x, z];
                            if (camera.Frustum.Intersects(column.AABB))
                            {
                                for (var y = 0; y < Constants.CHUNKS_PER_COLUMN; y++)
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
                                culled += Constants.CHUNKS_PER_COLUMN;
                        }
                    }
                }
            }
            return culled;
        }
    }
}
