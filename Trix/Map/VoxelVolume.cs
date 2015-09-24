using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Trix.Rendering;

namespace Trix.Map
{
    public class VoxelVolume
    {
        protected GraphicsDevice _device;

        public DynamicMesh<VertexPositionColorNormal> opaqueMesh;
        public DynamicMesh<VertexPositionColorNormal> waterMesh;
        public Dimensions dims;
        //public int X, Y, Z;

        private uint[] data;
        public VoxelVolume(GraphicsDevice device, Dimensions dims)
        {
            _device = device;
            this.data = new uint[dims[0] * dims[1] * dims[2]];
            this.dims = dims;
        }

        public uint this[int index]
        {
            get
            {
                if (index < 0 || index > data.Length - 1)
                    return 0;
                return data[index];
            }
        }

        public uint this[int x, int y, int z]
        {
            get
            {
                var index = x + dims[0] * (y + dims[1] * z);
                if (index < 0 || index > data.Length - 1)
                    return 0;
                return data[index];
            }
            set
            {
                data[x + dims[0] * (y + dims[1] * z)] = value;
            }
        }

        public int Width { get { return dims[0]; } }
        public int Height { get { return dims[1]; } }
        public int Depth { get { return dims[2]; } }

        public virtual uint GetRelativeVoxel(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= dims[0] || y >= dims[1] || z >= dims[2])
                return 0;

            return this[x, y, z];
        }

        public void PrepareMesh()
        {
            if (opaqueMesh == null)
                opaqueMesh = new DynamicMesh<VertexPositionColorNormal>(_device);
            else
                opaqueMesh.Clear();

            if (waterMesh == null)
                waterMesh = new DynamicMesh<VertexPositionColorNormal>(_device);
            else
                waterMesh.Clear();
        }
    }
}
