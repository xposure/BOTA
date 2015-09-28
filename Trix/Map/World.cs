using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Trix.Map
{
    public class World
    {
        private GraphicsDevice device;
        private Layer[] layers;
        private int size;
        private int depth;

        public GraphicsDevice Device { get { return device; } }
        public int Size { get { return size; } }
        public int Depth { get { return depth; } }

        public World(GraphicsDevice device, int size, int depth)
        {
            this.device = device;
            this.size = size;
            this.depth = depth;
            this.layers = new Layer[depth];
            for (var i = 0; i < depth; ++i)
                this.layers[i] = new Layer(this, i);

        }

        public void Render(BasicEffect effect, Camera camera)
        {
            var d = (int)camera.Position.Y;
            for (var i = 0; i < depth && i < d ; ++i)
                layers[i].Render(effect);
        }

        public void Generate(DefaultWorldGenerator gen)
        {
        //    layers[0].Fill(MapCell.BEDROCK);
        //    layers[1].Fill(MapCell.BEDROCK);
        //    layers[2].Fill(MapCell.BEDROCK);
            //layers[1].Fill(MapCell.GRASS);
            gen.Init();
            gen.Start();
            gen.BuildWorld(0, 0, 0, this);

            for (var i = 0; i < depth; ++i)
            {
                layers[i].UpdateHiddenCells();
                layers[i].BuildMesh();
            }
        }

        public MapCell this[int x, int y, int z]
        {
            get
            {
                if (z < 0 || z >= depth || x < 0 || x >= size || y < 0 || y >= size)
                    return MapCell.AIR;

                return layers[z][x, y];
            }
            set
            {
                layers[z][x, y] = value;
            }
        }
        //public MapCell GetVoxel(Position p)
        //{
        //    return new MapCell();
        //}

        //public Chunk GetChunk(uint encodedId)
        //{
        //    Chunk chunk = null;
        //    if (!chunks.TryGetValue(encodedId, out chunk))
        //        return Chunk.Empty;

        //    return chunk;
        //}

        //public Layer GetLayer(int depth)
        //{
        //    System.Diagnostics.Debug.Assert(depth >= 0 && depth < this.depth);
        //    return layers[depth];
        //}

    }
}
