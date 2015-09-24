using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Trix.Rendering;

namespace Trix.Map
{
    public class Chunk : VoxelVolume
    {
        /*
         * Questions
         *  - Are we lit?
         *  - Are we generated?
         *  - Do we have 
         * 
         */
        private ChunkManager cm;
        private BoundingBox aabb;
        private int x, y, z;
        public Vector3 Position { get { return new Vector3(x, y, z); } }

        public int WorldX { get { return x * Constants.CHUNK_SIZE; } }
        public int WorldY { get { return y * Constants.CHUNK_SIZE; } }
        public int WorldZ { get { return z * Constants.CHUNK_SIZE; } }
        public Vector3 WorldPosition { get { return Position * Constants.CHUNK_SIZE; } }
        public BoundingBox AABB { get { return aabb; } }

        public Chunk(ChunkManager cm, int x, int y, int z, GraphicsDevice device)
            : base(device, new Dimensions(new int[] { Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, Constants.CHUNK_SIZE }))
        {
            this.cm = cm;
            this.x = x;
            this.y = y;
            this.z = z;
            this.aabb = new BoundingBox(WorldPosition, WorldPosition + Vector3.One * Constants.CHUNK_SIZE);
        }

        public void Generate(ChunkManager cm)
        {
            var h = new int[] { 0, Constants.CHUNK_SIZE };
            var w = new int[] { 0, Constants.CHUNK_SIZE };
            var l = new int[] { 0, Constants.CHUNK_SIZE };

            //int[] d = { h[0] - l[0], h[1] - l[1], h[2] - l[2] };
            int[] d = { Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, Constants.CHUNK_SIZE };
            cm.WorldGenerator.GetChunk(x, z, y, this);
        }

        public void UpdateMesh(ChunkManager cm)
        {
            SurfaceExtractor.ExtractMesh(this);
        }

        public override uint GetRelativeVoxel(int x, int y, int z)
        {
            return cm.GetVoxelByRelative(this.x, this.y, this.z, x, y, z);
            //return base.GetRelativeVoxel(x, y, z);
        }

        public bool Draw(Camera camera)
        {
            if (camera.Frustum.Intersects(this.aabb))
            {
                this.opaqueMesh.Draw();
                return true;
            }
            return false;
        }

        //public uint this[int x, int y, int z]
        //{
        //    get { return volume[x, y, z]; }
        //    set { volume[x, y, z] = value; }
        //}
    }
}
