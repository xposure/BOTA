//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Trix.Voxels
//{
//    public class World
//    {
//        private Dictionary<uint, Chunk> chunks = new Dictionary<uint, Chunk>();

//        private int width, depth, height;

//        public int Width { get { return width; } }
//        public int Depth { get { return depth; } }
//        public int Height { get { return height; } }

//        public World(int width, int height, int depth)
//        {
//            this.width = width;
//            this.height = height;
//            this.depth = depth;
//        }

//        public void Generate()
//        {
//            layers = new Layer[depth];
//            for (var i = 0; i < depth; ++i)
//            {
//                layers[i] = new Layer();
//            }
//        }

//        public Voxel GetVoxel(Position p)
//        {
//            return new Voxel();
//        }

//        public Chunk GetChunk(uint encodedId)
//        {
//            Chunk chunk = null;
//            if (!chunks.TryGetValue(encodedId, out chunk))
//                return Chunk.Empty;

//            return chunk;
//        }

//        public Layer GetLayer(int depth)
//        {
//            System.Diagnostics.Debug.Assert(depth >= 0 && depth < this.depth);
//            return layers[depth];
//        }

//    }
//}
