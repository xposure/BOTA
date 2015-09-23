using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trix.Voxels
{
    public class ChunkColumn
    {
        //private byte[,] _sunlightDepth = new byte[ChunkManager.CHUNK_SIZE, ChunkManager.CHUNK_SIZE];
        private BoundingBox aabb;
        private Chunk[] chunks = new Chunk[ChunkManager.CHUNKS_PER_COLUMN];
        private int x, z;
        private GraphicsDevice device;

        public int X { get { return x; } }
        public int Z { get { return z; } }
        public Vector3 WorldPosition { get { return new Vector3(x * ChunkManager.CHUNK_SIZE, 0, z * ChunkManager.CHUNK_SIZE); } }
        public BoundingBox AABB { get { return aabb; } }

        public ChunkColumn(int x, int z, GraphicsDevice device)
        {
            this.x = x;
            this.z = z;
            this.aabb = new BoundingBox(WorldPosition, WorldPosition +
                new Vector3(ChunkManager.CHUNK_SIZE, ChunkManager.CHUNK_HEIGHT, ChunkManager.CHUNK_SIZE));
            this.device = device;

            for (var y = 0; y < chunks.Length; y++)
                chunks[y] = new Chunk(x, y, z, device);
        }

        public void Init(ChunkManager cm)
        {
            for (var y = 0; y < chunks.Length; y++)
                chunks[y].Generate(cm);
        }

        public void UpdateMesh(ChunkManager cm)
        {
            for (var y = 0; y < chunks.Length; y++)
                chunks[y].UpdateMesh(cm);
        }

        public Chunk this[int index]
        {
            get
            {
                if (index < 0)
                    return null;

                if (index >= chunks.Length)
                    return null;

                return chunks[index];
            }
        }
    }
}
