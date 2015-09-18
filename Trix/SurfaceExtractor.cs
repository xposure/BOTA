using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class VoxelMesh
{

}

public class ChunkManager
{
    private const int GRID_SIZE = 16;
    private const int CHUNK_SIZE = 1;
    private const int worldSize = GRID_SIZE * CHUNK_SIZE;

    GraphicsDevice device;
    Volume[,] grid = new Volume[GRID_SIZE, GRID_SIZE];

    public ChunkManager(GraphicsDevice device)
    {
        this.device = device;
    }

    public void Initialize()
    {
        var terrainTimer = new Stopwatch();
        var surfaceTimer = new Stopwatch();

        var sealevel = 8;
        var noise = NoiseType.Perlin;
        terrainTimer.Start();
        for (var x = 0; x < GRID_SIZE; x++)
        {
            for (var z = 0; z < GRID_SIZE; z++)
            {
                grid[x, z] = SurfaceExtractor.makeVoxels(this, x * CHUNK_SIZE, 0, z * CHUNK_SIZE,
                     new int[] { 0, 0, 0 },
                     new int[] { CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE },
                        Generators.GenerateHeight(x, 0, z, CHUNK_SIZE, CHUNK_SIZE, noise, sealevel)
                     );

                grid[x, z]._device = device;
            }
        }
        terrainTimer.Stop();


        surfaceTimer.Start();
        for (var x = 0; x < GRID_SIZE; x++)
            for (var z = 0; z < GRID_SIZE; z++)
                SurfaceExtractor.GenerateMesh2(this, device, null, grid[x, z], centered: true);
        surfaceTimer.Stop();

        System.Diagnostics.Trace.WriteLine("Terrain: " + terrainTimer.Elapsed.ToString());
        System.Diagnostics.Trace.WriteLine("Surface: " + surfaceTimer.Elapsed.ToString());


    }
    public uint GetVoxelByRelative(int cx, int cz, int x, int y, int z)
    {
        return GetVoxelByWorld(cx * CHUNK_SIZE + x, y, cz * CHUNK_SIZE + z);
    }

    public uint GetVoxelByWorld(int wx, int wy, int wz)
    {
        if (wx < 0 || wy < 0 || wz < 0 || wx >= worldSize || wy >= CHUNK_SIZE || wz >= worldSize)
            return 0;

        var cx = wx / CHUNK_SIZE;
        var cz = wz / CHUNK_SIZE;

        var volume = grid[cx, cz];
        return volume[wx - (cx * CHUNK_SIZE), wy, wz - (cz * CHUNK_SIZE)];
    }

    public void Draw(GameTime gameTime, BasicEffect basicEffect, Matrix worldMatrix, bool wireFrame)
    {
        var rast = new RasterizerState();
        rast.FillMode = wireFrame ? FillMode.WireFrame : FillMode.Solid;
        device.RasterizerState = rast;

        // TODO: Add your drawing code here
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
            for (var x = 0; x < GRID_SIZE; x++)
            {
                for (var z = 0; z < GRID_SIZE; z++)
                {
                    //basicEffect.World = worldMatrix * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds / 2);
                    basicEffect.World = worldMatrix * Matrix.CreateTranslation(new Vector3(x * CHUNK_SIZE - (GRID_SIZE * CHUNK_SIZE / 2), 0, z * CHUNK_SIZE - (GRID_SIZE * CHUNK_SIZE / 2))) * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds / 2);
                    pass.Apply();
                    grid[x, z].opaqueMesh.Draw();
                }
            }
        }

    }
}

public class DynamicMesh<T>
    where T : struct, IVertexType
{
    private GraphicsDevice _device;
    private VertexDeclaration _decl;
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private List<T> _vertexData;
    private List<ushort> _indexData;
    //private int _c

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

    public int Add(T t)
    {
        _vertexData.Add(t);
        return _vertexData.Count - 1;
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

            //_vertexData.Clear();
            //_indexData.Clear();
        }
    }

    public void Draw()
    {
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

public class Dimensions
{
    public int[] size;

    public Dimensions(int[] size)
    {
        this.size = size;
    }

    public int this[int index] { get { return size[index]; } }
}

public class Volume
{
    public GraphicsDevice _device;
    public ChunkManager cm;

    public DynamicMesh<VertexPositionColor> opaqueMesh;
    public DynamicMesh<VertexPositionColor> waterMesh;
    public Dimensions dims;
    public int X, Y, Z;

    private uint[] data;
    public Volume(ChunkManager _cm, int x, int y, int z, uint[] data, Dimensions dims)
    {
        cm = _cm;
        X = x;
        Y = y;
        Z = z;
        this.data = data;
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

    public uint this[int i, int j, int k]
    {
        get
        {
            var index = i + dims[0] * (j + dims[1] * k);
            if (index < 0 || index > data.Length - 1)
                return 0;
            return data[index];
        }
        set
        {
            data[i + dims[0] * (j + dims[1] * k)] = value;
        }
    }

    public int Width { get { return dims[0]; } }
    public int Height { get { return dims[1]; } }
    public int Depth { get { return dims[2]; } }

    public void PrepareMesh(GraphicsDevice device)
    {
        if (opaqueMesh == null)
            opaqueMesh = new DynamicMesh<VertexPositionColor>(device);
        else
            opaqueMesh.Clear();

        if (waterMesh == null)
            waterMesh = new DynamicMesh<VertexPositionColor>(device);
        else
            waterMesh.Clear();
    }

    public void UpdateMesh(GraphicsDevice device)
    {
        this.PrepareMesh(device);
        SurfaceExtractor.GenerateMesh(null, this);
        //SurfaceExtractor.GenerateWaterMesh(null, this);

        //opaqueMesh.RecalculateNormals();
        //waterMesh.RecalculateNormals();
        //mesh.UploadMeshData(true);


        //mesh.SetIndices()
    }

    public void RenderOpaque()
    {
        if (opaqueMesh != null && opaqueMesh.VertexCount > 0)
        {
            var p = new Vector3(X, 0, Z);
            var m = Matrix.CreateTranslation(p);
            //opaqueMesh.Draw();
            opaqueMesh.Draw();
        }
    }

    public void RenderAlpha()
    {
        if (waterMesh != null && waterMesh.VertexCount > 0)
        {
            var p = new Vector3(X, 0, Z);
            var m = Matrix.CreateTranslation(p);
            waterMesh.Draw();
            //waterMesh.Draw(m);
        }
    }
}
public class SurfaceExtractor
{
    static uint[] mask = new uint[4096];
    static MaskLayout[] maskLayout = new MaskLayout[4096];

    //public static List<Vector3> vertices = new List<Vector3>(65536);
    //public static List<int> faces = new List<int>(65536);
    //public static List<Vector2> uvs = new List<Vector2>(65536);
    //public static List<Vector3> normals = new List<Vector3>(65536);
    //public static List<Color> colors = new List<Color>(65536);


    public static Volume makeVoxels(ChunkManager cm, int x, int y, int z, int[] l, int[] h, Func<int, int, int, uint> f)
    {
        int[] d = { h[0] - l[0], h[1] - l[1], h[2] - l[2] };
        uint[] v = new uint[d[0] * d[1] * d[2]];
        int n = 0;
        for (var k = l[2]; k < h[2]; ++k)
            for (var j = l[1]; j < h[1]; ++j)
                for (var i = l[0]; i < h[0]; ++i, ++n)
                {
                    v[n] = f(i, j, k);
                }

        return new Volume(cm, x, y, z, v, new Dimensions(d));
    }

    public struct MaskLayout
    {
        private const int BACKFACE_BIT = 31;
        private const int FLIPFACE_BIT = 30;
        //occlusion = 0 - 11
        //flip = 12
        public uint data;

        public void Reset()
        {
            data = 0;
        }

        public bool FlipFace
        {
            get { return (data & (1u << FLIPFACE_BIT)) > 0u; }
            set
            {
                if (value)
                    data |= (1u << FLIPFACE_BIT);
                else
                    data &= ((1u << FLIPFACE_BIT) ^ 0xffffffffu);
            }
        }

        public bool BackFace
        {
            get { return (data & (1u << BACKFACE_BIT)) > 0u; }
            set
            {
                if (value)
                    data |= (1u << BACKFACE_BIT);
                else
                    data &= ((1u << BACKFACE_BIT) ^ 0xffffffffu);
            }
        }

        public void SetOcclusion(int vert, uint count)
        {
            data |= ((count & 3u) << (vert * 3));
        }

        public uint GetOcclusion(int vert)
        {
            return (data >> (vert * 3)) & 3u;
        }
    }

    public static float Pack(Vector2 input, int precision = 4096)
    {
        Vector2 output = input;
        output.X = (float)Math.Floor(output.X * (precision - 1));
        output.Y = (float)Math.Floor(output.Y * (precision - 1));

        return (output.X * precision) + output.Y;
    }

    public static float Pack(float x, float y, int precision = 4096)
    {
        return Pack(new Vector2(x, y), precision);
    }

    public static Vector2 Unpack(float input, int precision = 4096)
    {
        Vector2 output = Vector2.Zero;

        output.Y = input % precision;
        output.X = (float)Math.Floor(input / precision);

        return output / (precision - 1);
    }

    public static int GenerateMesh(ChunkManager cm, Volume volume, bool centered = false, bool disableGreedyMeshing = false, bool disableAO = false)
    {
        //vertices.Clear();
        //faces.Clear();
        //uvs.Clear();
        //normals.Clear();
        //colors.Clear();

        VertexPositionColor[] vertices = new VertexPositionColor[4];

        var dims = volume.dims;

        var f = new Func<int, int, int, uint>((i, j, k) =>
        {
            if (i < 0 || j < 0 || k < 0 || i >= dims[0] || j >= dims[1] || k >= dims[2])
                return cm.GetVoxelByRelative(volume.X, volume.Z, i, j, k);
            //return 0;

            var r = volume[i + dims[0] * (j + dims[1] * k)];
            if (r > 0 && (r & 0x1000000u) > 0u)
            {
                r = 0;
            }
            //1 + dims[0] * (1 + dims[1] * 1)
            //i    w    j   h    k
            //1 + 16 * (1 + 16 * 1)
            return r;
            //return 0;
        });

        //Sweep over 3-axes

        for (var d = 0; d < 3; ++d)
        {
            int i, j, k, l, w, h
              , u = (d + 1) % 3
              , v = (d + 2) % 3;
            int[] x = { 0, 0, 0 };
            int[] q = { 0, 0, 0 };
            int[,] posArea = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
            int[,] negArea = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

            if (mask.Length < dims[u] * dims[v])
            {
                mask = new uint[dims[u] * dims[v]];
                maskLayout = new MaskLayout[dims[u] * dims[v]];
            }
            q[d] = 1;

            posArea[0, u] = -1;
            posArea[1, v] = -1;
            posArea[2, u] = 1;
            posArea[3, v] = 1;

            negArea[0, v] = -1;
            negArea[1, u] = -1;
            negArea[2, v] = 1;
            negArea[3, u] = 1;

            for (x[d] = -1; x[d] < dims[d]; )
            {
                //Compute mask
                //TODO: Test if the AOMASK should be created outside of the block mask
                //the aomask generation might be causing a cache miss per loop
                var n = 0;
                for (x[v] = 0; x[v] < dims[v]; ++x[v])
                {
                    for (x[u] = 0; x[u] < dims[u]; ++x[u], ++n)
                    {
                        //int a = 0;
                        //if (x[d] < 0 && cm != null)
                        //    a = cm.GetBlock(volume.X + x[0], volume.Y + x[1], volume.Z + x[2]);
                        //else
                        var a = (0u <= x[d] ? f(x[0], x[1], x[2]) : 0u);
                        var b = (x[d] < dims[d] - 1 ? f(x[0] + q[0], x[1] + q[1], x[2] + q[2]) : 0u);

                        maskLayout[n].data = 0u;
                        if ((a != 0) == (b != 0))
                        {
                            mask[n] = 0;
                        }
                        else if (a != 0)
                        {
                            mask[n] = a;
                        }
                        else
                        {
                            maskLayout[n].BackFace = true;
                            mask[n] = b;
                        }

                        if (disableAO || mask[n] == 0)
                        {
                            //maskLayout[n].data = 4095u;
                        }
                        else
                        {
                            uint side1 = 0, side2 = 0, corner = 0;
                            var neighbors = new uint[4];

                            for (var t = 0; t < 4; ++t)
                            {
                                var tt = (t + 1) % 4;

                                if (a != 0)
                                {
                                    side1 = ((x[d] < dims[d] - 1 ? f(x[0] + q[0] + posArea[t, 0], x[1] + q[1] + posArea[t, 1], x[2] + q[2] + posArea[t, 2]) : 0u) > 0u ? 1u : 0u);
                                    side2 = ((x[d] < dims[d] - 1 ? f(x[0] + q[0] + posArea[tt, 0], x[1] + q[1] + posArea[tt, 1], x[2] + q[2] + posArea[tt, 2]) : 0u) > 0u ? 1u : 0u);
                                }
                                else
                                {
                                    side1 = ((x[d] < dims[d] - 1 ? f(x[0] + negArea[t, 0], x[1] + negArea[t, 1], x[2] + negArea[t, 2]) : 0u) > 0u ? 1u : 0u);
                                    side2 = ((x[d] < dims[d] - 1 ? f(x[0] + negArea[tt, 0], x[1] + negArea[tt, 1], x[2] + negArea[tt, 2]) : 0u) > 0u ? 1u : 0u);
                                }

                                if (side1 > 0 && side2 > 0)
                                {
                                    neighbors[t] = 0;
                                }
                                else
                                {
                                    if (a != 0)
                                    {
                                        corner = ((x[d] < dims[d] - 1u ? f(x[0] + q[0] + posArea[t, 0] + posArea[tt, 0], x[1] + q[1] + posArea[t, 1] + posArea[tt, 1], x[2] + q[2] + posArea[t, 2] + posArea[tt, 2]) : 0u) > 0u ? 1u : 0u);
                                    }
                                    else
                                    {
                                        corner = ((x[d] < dims[d] - 1u ? f(x[0] + negArea[t, 0] + negArea[tt, 0], x[1] + negArea[t, 1] + negArea[tt, 1], x[2] + negArea[t, 2] + negArea[tt, 2]) : 0u) > 0u ? 1u : 0u);
                                    }
                                    neighbors[t] = 3u - (side1 + side2 + corner);
                                }

                                maskLayout[n].SetOcclusion(t, neighbors[t]);
                            }

                            uint a00 = neighbors[0], a01 = neighbors[1], a11 = neighbors[2], a10 = neighbors[3];
                            if (a00 + a11 == a10 + a01)
                                maskLayout[n].FlipFace = Math.Max(a00, a11) < Math.Max(a10, a01);
                            else if (a00 + a11 < a10 + a01)
                                maskLayout[n].FlipFace = true;


                        }
                    }
                }
                //Increment x[d]
                ++x[d];
                //Generate mesh for mask using lexicographic ordering
                n = 0;
                for (j = 0; j < dims[v]; ++j)
                {
                    for (i = 0; i < dims[u]; )
                    {
                        var c = mask[n];
                        if (c != 0)
                        {
                            var a = maskLayout[n];
                            if (disableGreedyMeshing)
                            {
                                w = 1;
                                h = 1;
                            }
                            else
                            {
                                //Compute width
                                for (w = 1; c == mask[n + w] && (a.data == (maskLayout[n + w].data)) && i + w < dims[u]; ++w)
                                {
                                }
                                //Compute height (this is slightly awkward
                                var done = false;
                                for (h = 1; j + h < dims[v]; ++h)
                                {
                                    for (k = 0; k < w; ++k)
                                    {
                                        if (c != mask[n + k + h * dims[u]] || a.data != maskLayout[n + k + h * dims[u]].data)
                                        {
                                            done = true;
                                            break;
                                        }
                                    }
                                    if (done)
                                    {
                                        break;
                                    }
                                }
                            }

                            //Add quad
                            x[u] = i; x[v] = j;
                            int[] du = { 0, 0, 0 };
                            int[] dv = { 0, 0, 0 };

                            if (!maskLayout[n].BackFace)
                            {
                                dv[v] = h;
                                du[u] = w;
                            }
                            else
                            {
                                du[v] = h;
                                dv[u] = w;
                            }

                            var flip = maskLayout[n].FlipFace;

                            var cr = ((c >> 16) & 0xff) / 255f;
                            var cg = ((c >> 8) & 0xff) / 255f;
                            var cb = (c & 0xff) / 255f;

                            var aouvs = new float[4];
                            float[] AOcurve = new float[] { 0.0f, 0.6f, 0.8f, 1.0f };
                            Color[] ugh = new Color[] { 
                                new Color(1,0,0,1),
                                new Color(0,1,0,1),
                                new Color(0,0,1,1),
                                new Color(1,1,1,1)
                            };
                            for (var o = 0; o < 4; ++o)
                            {
                                //var ao = AOcurve[maskLayout[n].GetOcclusion(o)];
                                var ao = disableAO ? 1f : (maskLayout[n].GetOcclusion(o) / 3f);
                                ////var ao = disableAO ? 1f : (maskLayout[n].GetOcclusion(o) / 4f + 0.25f);
                                ////if (maskLayout[n].GetOcclusion(o) != 3u)
                                ////    ao = 0.5f;
                                ////else
                                ////    ao = 1f;
                                //var color = new Color(cr * ao, cg * ao, cb * ao, 1);
                                //colors.Add(color);
                                ////colors.Add(new Color(1,0,1,1));


                                vertices[o].Color = new Color(cr * ao, cg * ao, cb * ao);
                                //uvs.Add(new Vector2(ao, 0));


                                //colors.Add(ugh[maskLayout[n].GetOcclusion(o)]);

                                //aouvs[o] = ao;
                            }


                            //for (var o = 0; o < 4; ++o)
                            //{
                            //    uvs.Add(new Vector2(Pack(aouvs[0], aouvs[1]), Pack(aouvs[2], aouvs[3])));
                            //}

                            //var vertex_count = vertices.Count;
                            //int v0, v1, v2, v3;
                            if (centered)
                            {
                                vertices[0].Position = new Vector3(x[0], x[1], x[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f;
                                vertices[1].Position = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f;
                                vertices[2].Position = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f;
                                vertices[3].Position = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f;

                                //This vert generation code will make the 0,0,0 be the center of the mesh in worldspace
                                //vertices.Add(new Vector3(x[0], x[1], x[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f);
                                //vertices.Add(new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f);
                                //vertices.Add(new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f);
                                //vertices.Add(new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f);
                            }
                            else
                            {
                                vertices[0].Position = new Vector3(x[0], x[1], x[2]);
                                vertices[1].Position = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                vertices[2].Position = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                vertices[3].Position = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);

                                ////This vert generation code will make the edge of the mesh at 0,0,0
                                //vertices.Add(new Vector3(x[0], x[1], x[2]));
                                //vertices.Add(new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]));
                                //vertices.Add(new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]));
                                //vertices.Add(new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]));
                            }

                            //uvs.Add(new Vector2(0, 1));
                            //uvs.Add(new Vector2(1, 1));
                            //uvs.Add(new Vector2(1, 0));
                            //uvs.Add(new Vector2(0, 0));

                            var v0 = volume.opaqueMesh.Add(vertices[0]);
                            var v1 = volume.opaqueMesh.Add(vertices[1]);
                            var v2 = volume.opaqueMesh.Add(vertices[2]);
                            var v3 = volume.opaqueMesh.Add(vertices[3]);

                            if (flip)
                            {
                                volume.opaqueMesh.Quad((ushort)v1, (ushort)v2, (ushort)v3, (ushort)v0);
                                //faces.AddRange(new int[] { vertex_count + 1, vertex_count + 2, vertex_count + 3, 
                                //                            vertex_count + 1, vertex_count + 3, vertex_count });
                            }
                            else
                            {
                                volume.opaqueMesh.Quad((ushort)v0, (ushort)v1, (ushort)v2, (ushort)v3);

                                //faces.AddRange(new int[] { vertex_count, vertex_count + 1, vertex_count + 2, 
                                //                            vertex_count, vertex_count + 2, vertex_count + 3 });
                            }

                            //Zero-out mask
                            for (l = 0; l < h; ++l)
                            {
                                for (k = 0; k < w; ++k)
                                {
                                    mask[n + k + l * dims[u]] = 0;
                                    maskLayout[n + k + l * dims[u]].data = 4095u;
                                }
                            }
                            //Increment counters and continue
                            i += w; n += w;
                        }
                        else
                        {
                            ++i; ++n;
                        }
                    }
                }
            }
        }

        //volume.opaqueMesh.vertices = vertices.ToArray();
        //volume.opaqueMesh.colors = colors.ToArray();
        //volume.opaqueMesh.triangles = faces.ToArray();
        //volume.opaqueMesh.uv = uvs.ToArray();
        //volume.opaqueMesh.RecalculateNormals();
        volume.opaqueMesh.Update();

        return volume.opaqueMesh.VertexCount;
    }

    //public static int GenerateWaterMesh(ChunkManager cm, Volume volume, bool centered = false, bool disableGreedyMeshing = false)
    //{
    //    vertices.Clear();
    //    faces.Clear();
    //    uvs.Clear();
    //    normals.Clear();
    //    colors.Clear();

    //    var dims = volume.dims;

    //    var f = new Func<int, int, int, uint>((i, j, k) =>
    //    {
    //        if (i < 0 || j < 0 || k < 0 || i >= dims[0] || j >= dims[1] || k >= dims[2])
    //            return 0;

    //        var r = volume[i + dims[0] * (j + dims[1] * k)];
    //        if ((r & 0x1000000u) > 0u)
    //        {
    //            return r;
    //        }
    //        return 0;
    //        //1 + dims[0] * (1 + dims[1] * 1)
    //        //i    w    j   h    k
    //        //1 + 16 * (1 + 16 * 1)
    //        //return 0;
    //    });

    //    //Sweep over 3-axes

    //    for (var d = 1; d < 2; ++d)
    //    {
    //        int i, j, k, l, w, h
    //          , u = (d + 1) % 3
    //          , v = (d + 2) % 3;
    //        int[] x = { 0, 0, 0 };
    //        int[] q = { 0, 0, 0 };
    //        int[,] posArea = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
    //        int[,] negArea = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

    //        if (mask.Length < dims[u] * dims[v])
    //        {
    //            mask = new uint[dims[u] * dims[v]];
    //            maskLayout = new MaskLayout[dims[u] * dims[v]];
    //        }
    //        q[d] = 1;

    //        posArea[0, u] = -1;
    //        posArea[1, v] = -1;
    //        posArea[2, u] = 1;
    //        posArea[3, v] = 1;

    //        negArea[0, v] = -1;
    //        negArea[1, u] = -1;
    //        negArea[2, v] = 1;
    //        negArea[3, u] = 1;

    //        for (x[d] = -1; x[d] < dims[d]; )
    //        {
    //            //Compute mask
    //            //TODO: Test if the AOMASK should be created outside of the block mask
    //            //the aomask generation might be causing a cache miss per loop
    //            var n = 0;
    //            for (x[v] = 0; x[v] < dims[v]; ++x[v])
    //            {
    //                for (x[u] = 0; x[u] < dims[u]; ++x[u], ++n)
    //                {
    //                    var a = (0u <= x[d] ? f(x[0], x[1], x[2]) : 0u);

    //                    if ((a & 0x1000000u) > 0u)
    //                        mask[n] = a;

    //                }
    //            }
    //            //Increment x[d]
    //            ++x[d];
    //            //Generate mesh for mask using lexicographic ordering
    //            n = 0;
    //            for (j = 0; j < dims[v]; ++j)
    //            {
    //                for (i = 0; i < dims[u]; )
    //                {
    //                    var c = mask[n];
    //                    if (c != 0)
    //                    {
    //                        var a = maskLayout[n];
    //                        if (disableGreedyMeshing)
    //                        {
    //                            w = 1;
    //                            h = 1;
    //                        }
    //                        else
    //                        {
    //                            //Compute width
    //                            for (w = 1; c == mask[n + w] && (a.data == (maskLayout[n + w].data)) && i + w < dims[u]; ++w)
    //                            {
    //                            }
    //                            //Compute height (this is slightly awkward
    //                            var done = false;
    //                            for (h = 1; j + h < dims[v]; ++h)
    //                            {
    //                                for (k = 0; k < w; ++k)
    //                                {
    //                                    if (c != mask[n + k + h * dims[u]] || a.data != maskLayout[n + k + h * dims[u]].data)
    //                                    {
    //                                        done = true;
    //                                        break;
    //                                    }
    //                                }
    //                                if (done)
    //                                {
    //                                    break;
    //                                }
    //                            }
    //                        }

    //                        //Add quad
    //                        x[u] = i; x[v] = j;
    //                        for (var backface = 0; backface < 2; ++backface)
    //                        {
    //                            int[] du = { 0, 0, 0 };
    //                            int[] dv = { 0, 0, 0 };

    //                            if (backface == 0)
    //                            {
    //                                dv[v] = h;
    //                                du[u] = w;
    //                            }
    //                            else
    //                            {
    //                                du[v] = h;
    //                                dv[u] = w;
    //                            }

    //                            var flip = maskLayout[n].FlipFace;

    //                            var cr = ((c >> 16) & 0xff) / 255f;
    //                            var cg = ((c >> 8) & 0xff) / 255f;
    //                            var cb = (c & 0xff) / 255f;

    //                            for (var o = 0; o < 4; ++o)
    //                            {
    //                                colors.Add(new Color(cr, cg, cb));
    //                                uvs.Add(new Vector2(0, 0));
    //                            }

    //                            var vertex_count = vertices.Count;
    //                            var offset = Vector3.zero;
    //                            if (centered)
    //                                offset = new Vector3(dims[0], dims[1], dims[2]) / 2f;

    //                            //offset += new Vector3(0, 0.5f, 0);
    //                            vertices.Add(new Vector3(x[0], x[1], x[2]) - offset);
    //                            vertices.Add(new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]) - offset);
    //                            vertices.Add(new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]) - offset);
    //                            vertices.Add(new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]) - offset);


    //                            faces.AddRange(new int[] { vertex_count, vertex_count + 1, vertex_count + 2, vertex_count, vertex_count + 2, vertex_count + 3 });
    //                        }

    //                        //Zero-out mask
    //                        for (l = 0; l < h; ++l)
    //                        {
    //                            for (k = 0; k < w; ++k)
    //                            {
    //                                mask[n + k + l * dims[u]] = 0;
    //                                maskLayout[n + k + l * dims[u]].data = 4095u;
    //                            }
    //                        }
    //                        //Increment counters and continue
    //                        i += w; n += w;
    //                    }
    //                    else
    //                    {
    //                        ++i; ++n;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    volume.waterMesh.vertices = vertices.ToArray();
    //    volume.waterMesh.colors = colors.ToArray();
    //    volume.waterMesh.triangles = faces.ToArray();
    //    volume.waterMesh.uv = uvs.ToArray();
    //    volume.waterMesh.RecalculateNormals();

    //    return vertices.Count;
    //}



    public static int GenerateMesh2(ChunkManager cm, GraphicsDevice device, VoxelMesh mesh, Volume volume, bool centered = false, bool disableGreedyMeshing = false, bool disableAO = false)
    {
        volume.PrepareMesh(device);
        //vertices.Clear();
        //faces.Clear();
        //uvs.Clear();
        //normals.Clear();
        //colors.Clear();

        VertexPositionColor[] vertices = new VertexPositionColor[4];

        var dims = volume.dims;

        var f = new Func<int, int, int, uint>((i, j, k) =>
        {
            if (i < 0 || j < 0 || k < 0 || i >= dims[0] || j >= dims[1] || k >= dims[2])
                return cm.GetVoxelByRelative(volume.X, volume.Z, i, j, k);
            //return 0;

            var r = volume[i + dims[0] * (j + dims[1] * k)];
            if (r > 0 && (r & 0x1000000u) > 0u)
            {
                r = 0;
            }
            return r;
        });

        //Sweep over 3-axes
        for (var d = 0; d < 3; ++d)
        {
            int i, j, k, l, w, h
              , u = (d + 1) % 3
              , v = (d + 2) % 3;
            int[] x = { 0, 0, 0 };
            int[] q = { 0, 0, 0 };
            int[,] posArea = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
            int[,] negArea = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

            if (mask.Length < dims[u] * dims[v])
            {
                mask = new uint[dims[u] * dims[v]];
                maskLayout = new MaskLayout[dims[u] * dims[v]];
            }
            q[d] = 1;

            posArea[0, u] = -1;
            posArea[1, v] = -1;
            posArea[2, u] = 1;
            posArea[3, v] = 1;

            negArea[0, v] = -1;
            negArea[1, u] = -1;
            negArea[2, v] = 1;
            negArea[3, u] = 1;

            for (x[d] = -1; x[d] < dims[d]; )
            {
                //Compute mask
                //TODO: Test if the AOMASK should be created outside of the block mask
                //the aomask generation might be causing a cache miss per loop
                var n = 0;
                for (x[v] = 0; x[v] < dims[v]; ++x[v])
                {
                    for (x[u] = 0; x[u] < dims[u]; ++x[u], ++n)
                    {
                        //int a = 0;
                        //if (x[d] < 0 && cm != null)
                        //    a = cm.GetBlock(volume.X + x[0], volume.Y + x[1], volume.Z + x[2]);
                        //else
                        var a = (0u <= x[d] ? f(x[0], x[1], x[2]) : 0u);
                        var b = (x[d] < dims[d] - 1 ? f(x[0] + q[0], x[1] + q[1], x[2] + q[2]) : 0u);

                        maskLayout[n].data = 0u;
                        if ((a != 0) == (b != 0))
                        {
                            mask[n] = 0;
                        }
                        else if (a != 0)
                        {
                            mask[n] = a;
                        }
                        else
                        {
                            maskLayout[n].BackFace = true;
                            mask[n] = b;
                        }

                        if (disableAO || mask[n] == 0)
                        {
                            //maskLayout[n].data = 4095u;
                        }
                        else
                        {
                            uint side1 = 0, side2 = 0, corner = 0;
                            var neighbors = new uint[4];

                            for (var t = 0; t < 4; ++t)
                            {
                                var tt = (t + 1) % 4;

                                if (a != 0)
                                {
                                    side1 = ((x[d] < dims[d] - 1 ? f(x[0] + q[0] + posArea[t, 0], x[1] + q[1] + posArea[t, 1], x[2] + q[2] + posArea[t, 2]) : 0u) > 0u ? 1u : 0u);
                                    side2 = ((x[d] < dims[d] - 1 ? f(x[0] + q[0] + posArea[tt, 0], x[1] + q[1] + posArea[tt, 1], x[2] + q[2] + posArea[tt, 2]) : 0u) > 0u ? 1u : 0u);
                                }
                                else
                                {
                                    side1 = ((x[d] < dims[d] - 1 ? f(x[0] + negArea[t, 0], x[1] + negArea[t, 1], x[2] + negArea[t, 2]) : 0u) > 0u ? 1u : 0u);
                                    side2 = ((x[d] < dims[d] - 1 ? f(x[0] + negArea[tt, 0], x[1] + negArea[tt, 1], x[2] + negArea[tt, 2]) : 0u) > 0u ? 1u : 0u);
                                }

                                if (side1 > 0 && side2 > 0)
                                {
                                    neighbors[t] = 0;
                                }
                                else
                                {
                                    if (a != 0)
                                    {
                                        corner = ((x[d] < dims[d] - 1u ? f(x[0] + q[0] + posArea[t, 0] + posArea[tt, 0], x[1] + q[1] + posArea[t, 1] + posArea[tt, 1], x[2] + q[2] + posArea[t, 2] + posArea[tt, 2]) : 0u) > 0u ? 1u : 0u);
                                    }
                                    else
                                    {
                                        corner = ((x[d] < dims[d] - 1u ? f(x[0] + negArea[t, 0] + negArea[tt, 0], x[1] + negArea[t, 1] + negArea[tt, 1], x[2] + negArea[t, 2] + negArea[tt, 2]) : 0u) > 0u ? 1u : 0u);
                                    }
                                    neighbors[t] = 3u - (side1 + side2 + corner);
                                }

                                maskLayout[n].SetOcclusion(t, neighbors[t]);
                            }

                            uint a00 = neighbors[0], a01 = neighbors[1], a11 = neighbors[2], a10 = neighbors[3];
                            if (a00 + a11 == a10 + a01)
                                maskLayout[n].FlipFace = Math.Max(a00, a11) < Math.Max(a10, a01);
                            else if (a00 + a11 < a10 + a01)
                                maskLayout[n].FlipFace = true;


                        }
                    }
                }
                //Increment x[d]
                ++x[d];
                //Generate mesh for mask using lexicographic ordering
                n = 0;
                for (j = 0; j < dims[v]; ++j)
                {
                    for (i = 0; i < dims[u]; )
                    {
                        var c = mask[n];
                        if (c != 0)
                        {
                            var a = maskLayout[n];
                            if (disableGreedyMeshing)
                            {
                                w = 1;
                                h = 1;
                            }
                            else
                            {
                                //Compute width
                                for (w = 1; c == mask[n + w] && (a.data == (maskLayout[n + w].data)) && i + w < dims[u]; ++w)
                                {
                                }
                                //Compute height (this is slightly awkward
                                var done = false;
                                for (h = 1; j + h < dims[v]; ++h)
                                {
                                    for (k = 0; k < w; ++k)
                                    {
                                        if (c != mask[n + k + h * dims[u]] || a.data != maskLayout[n + k + h * dims[u]].data)
                                        {
                                            done = true;
                                            break;
                                        }
                                    }
                                    if (done)
                                    {
                                        break;
                                    }
                                }
                            }

                            //Add quad
                            x[u] = i; x[v] = j;
                            int[] du = { 0, 0, 0 };
                            int[] dv = { 0, 0, 0 };

                            if (!maskLayout[n].BackFace)
                            {
                                dv[v] = h;
                                du[u] = w;
                            }
                            else
                            {
                                du[v] = h;
                                dv[u] = w;
                            }

                            var flip = maskLayout[n].FlipFace;

                            var cr = ((c >> 16) & 0xff) / 255f;
                            var cg = ((c >> 8) & 0xff) / 255f;
                            var cb = (c & 0xff) / 255f;

                            var aouvs = new float[4];
                            float[] AOcurve = new float[] { 0.75f, 0.85f, 0.925f, 1.0f };
                            Color[] ugh = new Color[] { 
                                new Color(1,0,0,1),
                                new Color(0,1,0,1),
                                new Color(0,0,1,1),
                                new Color(1,1,1,1)
                            };
                            for (var o = 0; o < 4; ++o)
                            {
                                var ao = disableAO ? 1f : AOcurve[maskLayout[n].GetOcclusion(o)];
                                //var ao = disableAO ? 1f : (maskLayout[n].GetOcclusion(o) / 3f);
                                ////var ao = disableAO ? 1f : (maskLayout[n].GetOcclusion(o) / 4f + 0.25f);
                                ////if (maskLayout[n].GetOcclusion(o) != 3u)
                                ////    ao = 0.5f;
                                ////else
                                ////    ao = 1f;
                                //var color = new Color(cr * ao, cg * ao, cb * ao, 1);
                                //colors.Add(color);
                                ////colors.Add(new Color(1,0,1,1));

                                vertices[o].Color = new Color(cr * ao, cg * ao, cb * ao);
                                //colors.Add(new Color(cr, cg, cb));
                                //uvs.Add(new Vector2(ao, 0));
                                //colors.Add(ugh[maskLayout[n].GetOcclusion(o)]);

                                //aouvs[o] = ao;
                            }


                            //for (var o = 0; o < 4; ++o)
                            //{
                            //    uvs.Add(new Vector2(Pack(aouvs[0], aouvs[1]), Pack(aouvs[2], aouvs[3])));
                            //}

                            //var vertex_count = vertices.Count;
                            if (centered)
                            {
                                vertices[0].Position = new Vector3(x[0], x[1], x[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f;
                                vertices[1].Position = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f;
                                vertices[2].Position = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f;
                                vertices[3].Position = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f;

                                ////This vert generation code will make the 0,0,0 be the center of the mesh in worldspace
                                //vertices.Add(new Vector3(x[0], x[1], x[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f);
                                //vertices.Add(new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f);
                                //vertices.Add(new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f);
                                //vertices.Add(new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]) - new Vector3(dims[0], dims[1], dims[2]) / 2f);
                            }
                            else
                            {
                                vertices[0].Position = new Vector3(x[0], x[1], x[2]);
                                vertices[1].Position = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                vertices[2].Position = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                vertices[3].Position = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);

                                ////This vert generation code will make the edge of the mesh at 0,0,0
                                //vertices.Add(new Vector3(x[0], x[1], x[2]));
                                //vertices.Add(new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]));
                                //vertices.Add(new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]));
                                //vertices.Add(new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]));
                            }

                            //uvs.Add(new Vector2(0, 1));
                            //uvs.Add(new Vector2(1, 1));
                            //uvs.Add(new Vector2(1, 0));
                            //uvs.Add(new Vector2(0, 0));

                            var v0 = volume.opaqueMesh.Add(vertices[0]);
                            var v1 = volume.opaqueMesh.Add(vertices[1]);
                            var v2 = volume.opaqueMesh.Add(vertices[2]);
                            var v3 = volume.opaqueMesh.Add(vertices[3]);

                            if (flip)
                            {
                                volume.opaqueMesh.Quad((ushort)v1, (ushort)v2, (ushort)v3, (ushort)v0);
                                //faces.AddRange(new int[] { vertex_count + 1, vertex_count + 2, vertex_count + 3, 
                                //                            vertex_count + 1, vertex_count + 3, vertex_count });
                            }
                            else
                            {
                                volume.opaqueMesh.Quad((ushort)v0, (ushort)v1, (ushort)v2, (ushort)v3);

                                //faces.AddRange(new int[] { vertex_count, vertex_count + 1, vertex_count + 2, 
                                //                            vertex_count, vertex_count + 2, vertex_count + 3 });
                            }

                            //Zero-out mask
                            for (l = 0; l < h; ++l)
                            {
                                for (k = 0; k < w; ++k)
                                {
                                    mask[n + k + l * dims[u]] = 0;
                                    maskLayout[n + k + l * dims[u]].data = 4095u;
                                }
                            }
                            //Increment counters and continue
                            i += w; n += w;
                        }
                        else
                        {
                            ++i; ++n;
                        }
                    }
                }
            }
        }

        //mesh.PreMesh(vertices.Count);
        //mesh.UpdateVertices(vertices);
        //mesh.UpdateColors(colors);
        //mesh.UpdateIndices(faces);
        //mesh.UpdateUVs(uvs);
        //mesh.PostMesh();

        volume.opaqueMesh.Update();
        return volume.opaqueMesh.VertexCount;
    }

    //public static int GenerateWaterMesh2(VoxelMesh mesh, Volume volume, bool centered = false, bool disableGreedyMeshing = false)
    //{
    //    vertices.Clear();
    //    faces.Clear();
    //    uvs.Clear();
    //    normals.Clear();
    //    colors.Clear();

    //    var dims = volume.dims;

    //    var f = new Func<int, int, int, uint>((i, j, k) =>
    //    {
    //        if (i < 0 || j < 0 || k < 0 || i >= dims[0] || j >= dims[1] || k >= dims[2])
    //            return 0;

    //        var r = volume[i + dims[0] * (j + dims[1] * k)];
    //        if ((r & 0x1000000u) > 0u)
    //        {
    //            return r;
    //        }
    //        return 0;
    //        //1 + dims[0] * (1 + dims[1] * 1)
    //        //i    w    j   h    k
    //        //1 + 16 * (1 + 16 * 1)
    //        //return 0;
    //    });

    //    //Sweep over 3-axes

    //    for (var d = 1; d < 2; ++d)
    //    {
    //        int i, j, k, l, w, h
    //          , u = (d + 1) % 3
    //          , v = (d + 2) % 3;
    //        int[] x = { 0, 0, 0 };
    //        int[] q = { 0, 0, 0 };
    //        int[,] posArea = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
    //        int[,] negArea = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

    //        if (mask.Length < dims[u] * dims[v])
    //        {
    //            mask = new uint[dims[u] * dims[v]];
    //            maskLayout = new MaskLayout[dims[u] * dims[v]];
    //        }
    //        q[d] = 1;

    //        posArea[0, u] = -1;
    //        posArea[1, v] = -1;
    //        posArea[2, u] = 1;
    //        posArea[3, v] = 1;

    //        negArea[0, v] = -1;
    //        negArea[1, u] = -1;
    //        negArea[2, v] = 1;
    //        negArea[3, u] = 1;

    //        for (x[d] = -1; x[d] < dims[d]; )
    //        {
    //            //Compute mask
    //            //TODO: Test if the AOMASK should be created outside of the block mask
    //            //the aomask generation might be causing a cache miss per loop
    //            var n = 0;
    //            for (x[v] = 0; x[v] < dims[v]; ++x[v])
    //            {
    //                for (x[u] = 0; x[u] < dims[u]; ++x[u], ++n)
    //                {
    //                    var a = (0u <= x[d] ? f(x[0], x[1], x[2]) : 0u);

    //                    if ((a & 0x1000000u) > 0u)
    //                        mask[n] = a;

    //                }
    //            }
    //            //Increment x[d]
    //            ++x[d];
    //            //Generate mesh for mask using lexicographic ordering
    //            n = 0;
    //            for (j = 0; j < dims[v]; ++j)
    //            {
    //                for (i = 0; i < dims[u]; )
    //                {
    //                    var c = mask[n];
    //                    if (c != 0)
    //                    {
    //                        var a = maskLayout[n];
    //                        if (disableGreedyMeshing)
    //                        {
    //                            w = 1;
    //                            h = 1;
    //                        }
    //                        else
    //                        {
    //                            //Compute width
    //                            for (w = 1; c == mask[n + w] && (a.data == (maskLayout[n + w].data)) && i + w < dims[u]; ++w)
    //                            {
    //                            }
    //                            //Compute height (this is slightly awkward
    //                            var done = false;
    //                            for (h = 1; j + h < dims[v]; ++h)
    //                            {
    //                                for (k = 0; k < w; ++k)
    //                                {
    //                                    if (c != mask[n + k + h * dims[u]] || a.data != maskLayout[n + k + h * dims[u]].data)
    //                                    {
    //                                        done = true;
    //                                        break;
    //                                    }
    //                                }
    //                                if (done)
    //                                {
    //                                    break;
    //                                }
    //                            }
    //                        }

    //                        //Add quad
    //                        x[u] = i; x[v] = j;
    //                        for (var backface = 0; backface < 2; ++backface)
    //                        {
    //                            int[] du = { 0, 0, 0 };
    //                            int[] dv = { 0, 0, 0 };

    //                            if (backface == 0)
    //                            {
    //                                dv[v] = h;
    //                                du[u] = w;
    //                            }
    //                            else
    //                            {
    //                                du[v] = h;
    //                                dv[u] = w;
    //                            }

    //                            var flip = maskLayout[n].FlipFace;

    //                            var cr = ((c >> 16) & 0xff) / 255f;
    //                            var cg = ((c >> 8) & 0xff) / 255f;
    //                            var cb = (c & 0xff) / 255f;

    //                            for (var o = 0; o < 4; ++o)
    //                            {
    //                                colors.Add(new Color(cr, cg, cb));
    //                                uvs.Add(new Vector2(0, 0));
    //                            }

    //                            var vertex_count = vertices.Count;
    //                            var offset = Vector3.zero;
    //                            if (centered)
    //                                offset = new Vector3(dims[0], dims[1], dims[2]) / 2f;

    //                            //offset += new Vector3(0, 0.5f, 0);
    //                            vertices.Add(new Vector3(x[0], x[1], x[2]) - offset);
    //                            vertices.Add(new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]) - offset);
    //                            vertices.Add(new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]) - offset);
    //                            vertices.Add(new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]) - offset);


    //                            faces.AddRange(new int[] { vertex_count, vertex_count + 1, vertex_count + 2, vertex_count, vertex_count + 2, vertex_count + 3 });
    //                        }

    //                        //Zero-out mask
    //                        for (l = 0; l < h; ++l)
    //                        {
    //                            for (k = 0; k < w; ++k)
    //                            {
    //                                mask[n + k + l * dims[u]] = 0;
    //                                maskLayout[n + k + l * dims[u]].data = 4095u;
    //                            }
    //                        }
    //                        //Increment counters and continue
    //                        i += w; n += w;
    //                    }
    //                    else
    //                    {
    //                        ++i; ++n;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    mesh.PreMesh(vertices.Count);
    //    mesh.UpdateVertices(vertices);
    //    mesh.UpdateColors(colors);
    //    mesh.UpdateIndices(faces);
    //    mesh.UpdateUVs(uvs);
    //    mesh.PostMesh();

    //    return vertices.Count;
    //}

}
