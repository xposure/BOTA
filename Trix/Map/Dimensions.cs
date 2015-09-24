using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Trix.Map
{
    public class Dimensions
    {
        public int[] size;

        public Dimensions(int[] size)
        {
            this.size = size;
        }

        public int this[int index] { get { return size[index]; } }

        public int Width { get { return size[0]; } }
        public int Height { get { return size[1]; } }
        public int Depth { get { return size[2]; } }

        public Vector3 Size { get { return new Vector3(size[0], size[1], size[2]); } }
    }
}
