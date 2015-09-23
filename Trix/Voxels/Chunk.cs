using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Trix.Rendering;

namespace Trix.Voxels
{
    public class Chunk
    {
        /*
         * Questions
         *  - Are we lit?
         *  - Are we generated?
         *  - Do we have 
         * 
         */

        private BoundingBox aabb;
        private int x, y, z;
        private GraphicsDevice device;
        private VoxelVolume volume;
        public Vector3 Position { get { return new Vector3(x, y, z); } }

        public int WorldX { get { return x * Constants.CHUNK_SIZE; } }
        public int WorldY { get { return y * Constants.CHUNK_SIZE; } }
        public int WorldZ { get { return z * Constants.CHUNK_SIZE; } }
        public Vector3 WorldPosition { get { return Position * Constants.CHUNK_SIZE; } }
        public BoundingBox AABB { get { return aabb; } }

        public Chunk(int x, int y, int z, GraphicsDevice device)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.aabb = new BoundingBox(WorldPosition, WorldPosition + Vector3.One * Constants.CHUNK_SIZE);
            this.device = device;
        }

        public void Generate(ChunkManager cm)
        {
            var h = new int[] { 0, Constants.CHUNK_SIZE };
            var w = new int[] { 0, Constants.CHUNK_SIZE };
            var l = new int[] { 0, Constants.CHUNK_SIZE };

            //int[] d = { h[0] - l[0], h[1] - l[1], h[2] - l[2] };
            int[] d = { Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, Constants.CHUNK_SIZE };
            uint[] v = new uint[d[0] * d[1] * d[2]];
            volume = new VoxelVolume(device, x, y, z, v, new Dimensions(d));
            cm.WorldGenerator.GetChunk(x, z, y, volume);
        }

        public void UpdateMesh(ChunkManager cm)
        {
            SurfaceExtractor.ExtractMesh(volume);
        }

        public bool Draw(Camera camera)
        {
            if (camera.Frustum.Intersects(this.aabb))
            {
                this.volume.opaqueMesh.Draw();
                return true;
            }
            return false;
        }

        public uint this[int x, int y, int z]
        {
            get { return volume[x, y, z]; }
            set { volume[x, y, z] = value; }
        }
    }
}
