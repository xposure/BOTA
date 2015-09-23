using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trix.Rendering
{
    public struct VertexPositionColorNormal : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal)
        {
            this.Position = position;
            this.Color = color;
            this.Normal = normal;
        }

        static VertexPositionColorNormal()
        {
            VertexElement[] elements = new VertexElement[] { 
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0) 
        };

            VertexDeclaration declaration = new VertexDeclaration(elements);
            VertexDeclaration = declaration;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        public static bool operator !=(VertexPositionColorNormal left, VertexPositionColorNormal right)
        {
            return !(left == right);
        }

        public static bool operator ==(VertexPositionColorNormal left, VertexPositionColorNormal right)
        {
            return left.Color == right.Color && left.Position == right.Position && left.Normal == right.Normal;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return (this == ((VertexPositionColorNormal)obj));
        }

        public override int GetHashCode()
        {
            return this.Color.GetHashCode() ^ this.Position.GetHashCode() ^ this.Normal.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{{Position:{0} Color:{1} Normal:{2}}}", new object[] { this.Position, this.Color, this.Normal });
        }
    }
}
