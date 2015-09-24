using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Trix.Rendering
{
    public class DynamicMesh<T>
        where T : struct, IVertexType
    {
        private GraphicsDevice _device;
        private VertexDeclaration _decl;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private List<T> _vertexData;
        private List<ushort> _indexData;

        public DynamicMesh(GraphicsDevice device)
        {
            _device = device;

            var t = new T();
            _decl = t.VertexDeclaration;

            _vertexData = new List<T>(1024);
            _indexData = new List<ushort>(1024);
        }

        public int VertexCount { get { return _vertexBuffer == null ? 0 : _vertexBuffer.VertexCount; } }
        public int PrimitiveCount { get { return _indexData.Count == 0 ? _vertexData.Count / 3 : _indexData.Count / 3; } }

        public ushort Add(T t)
        {
            _vertexData.Add(t);
            return (ushort)(_vertexData.Count - 1);
        }

        public void Triangle(ushort i0, ushort i1, ushort i2)
        {
            _indexData.Add(i0);
            _indexData.Add(i1);
            _indexData.Add(i2);
        }

        public void Quad(ushort i0, ushort i1, ushort i2, ushort i3)
        {
            _indexData.Add(i0);
            _indexData.Add(i2);
            _indexData.Add(i1);

            _indexData.Add(i0);
            _indexData.Add(i3);
            _indexData.Add(i2);
        }

        public void Clear()
        {
            _vertexData.Clear();
            _indexData.Clear();
        }

        public void Update()
        {
            if (_vertexData.Count > 0)
            {
                if (_vertexBuffer == null || _vertexBuffer.VertexCount < _vertexData.Count)
                {
                    if (_vertexBuffer != null)
                        _vertexBuffer.Dispose();

                    _vertexBuffer = new VertexBuffer(_device, _decl, _vertexData.Count * 3 / 2, BufferUsage.WriteOnly);
                }

                if (_indexData.Count > 0 && (_indexBuffer == null || _indexBuffer.IndexCount < _indexData.Count))
                {
                    if (_indexBuffer != null)
                        _indexBuffer.Dispose();

                    _indexBuffer = new IndexBuffer(_device, IndexElementSize.SixteenBits, _indexData.Count * 3 / 2, BufferUsage.WriteOnly);
                }

                _vertexBuffer.SetData<T>(_vertexData.ToArray());

                if (_indexBuffer != null && _indexData.Count > 0)
                    _indexBuffer.SetData(_indexData.ToArray());
            }
        }

        public void Draw()
        {
            if (_vertexBuffer == null)
                return;

            if (_indexBuffer != null && _indexBuffer.IndexCount > 0)
            {
                _device.Indices = _indexBuffer;
                _device.SetVertexBuffer(_vertexBuffer);
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexData.Count, 0, _indexData.Count / 3);
                _device.Indices = null;
            }
            else
            {
                _device.SetVertexBuffer(_vertexBuffer);
                _device.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexData.Count / 3);
            }

        }
    }
}
