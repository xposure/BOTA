using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Trix.Map;

namespace Trix.Rendering
{
    public class SurfaceExtractor
    {
        static uint[] mask = new uint[4096];
        static MaskLayout[] maskLayout = new MaskLayout[4096];

        public struct MaskLayout
        {
            private const int BACKFACE_BIT = 31;
            private const int AOFACE_BIT = 30;
            //occlusion = 0 - 11
            //flip = 12
            public uint data;

            public void Reset()
            {
                data = 0;
            }

            public bool AOFace
            {
                get { return (data & (1u << AOFACE_BIT)) > 0u; }
                set
                {
                    if (value)
                        data |= (1u << AOFACE_BIT);
                    else
                        data &= ((1u << AOFACE_BIT) ^ 0xffffffffu);
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

            public uint TotalOcclusion()
            {
                var total = 0u;
                for (var o = 0; o < 4; ++o)
                    total += GetOcclusion(o) < 3 ? 1u : 0u;
                return total;
            }
        }

        public static int ExtractMesh(VoxelVolume volume, bool disableGreedyMeshing = false, bool disableAO = false)
        {
            volume.PrepareMesh();

            VertexPositionColorNormal[] vertices = new VertexPositionColorNormal[5];

            var dims = volume.dims;

            var f = new Func<int, int, int, uint>((i, j, k) =>
            {
                if (i < 0 || j < 0 || k < 0 || i >= dims[0] || j >= dims[1] || k >= dims[2])
                    return volume.GetRelativeVoxel(i, j, k);
                    //return cm.GetVoxelByRelative(volume.X, volume.Y, volume.Z, i, j, k);

                var r = volume[i + dims[0] * (j + dims[1] * k)];
                if (r > 0 && (r & 0x1000000u) > 0u)
                {
                    r = 0;
                }
                return r;
            });

            //Sweep over 3-axes
            // d0 = x, d1 = y, d2 = z
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
                    var n = 0;
                    for (x[v] = 0; x[v] < dims[v]; ++x[v])
                    {
                        for (x[u] = 0; x[u] < dims[u]; ++x[u], ++n)
                        {
                            var a = f(x[0], x[1], x[2]);
                            var b = f(x[0] + q[0], x[1] + q[1], x[2] + q[2]);

                            maskLayout[n].data = 0u;
                            if ((a != 0) == (b != 0))
                            {
                                mask[n] = 0;
                            }
                            else if (a != 0)
                            {
                                mask[n] = x[d] > -1 ? a : 0;
                                maskLayout[n].BackFace = false;
                            }
                            else
                            {
                                mask[n] = x[d] < dims[d] - 1 ? b : 0;
                                maskLayout[n].BackFace = true;
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
                                        side1 = (f(x[0] + q[0] + posArea[t, 0], x[1] + q[1] + posArea[t, 1], x[2] + q[2] + posArea[t, 2]) > 0u ? 1u : 0u);
                                        side2 = (f(x[0] + q[0] + posArea[tt, 0], x[1] + q[1] + posArea[tt, 1], x[2] + q[2] + posArea[tt, 2]) > 0u ? 1u : 0u);
                                    }
                                    else
                                    {
                                        side1 = (f(x[0] + negArea[t, 0], x[1] + negArea[t, 1], x[2] + negArea[t, 2]) > 0u ? 1u : 0u);
                                        side2 = (f(x[0] + negArea[tt, 0], x[1] + negArea[tt, 1], x[2] + negArea[tt, 2]) > 0u ? 1u : 0u);
                                    }

                                    if (side1 > 0 && side2 > 0)
                                    {
                                        neighbors[t] = 0;
                                    }
                                    else
                                    {
                                        if (a != 0)
                                        {
                                            corner = (f(x[0] + q[0] + posArea[t, 0] + posArea[tt, 0], x[1] + q[1] + posArea[t, 1] + posArea[tt, 1], x[2] + q[2] + posArea[t, 2] + posArea[tt, 2]) > 0u ? 1u : 0u);
                                        }
                                        else
                                        {
                                            corner = (f(x[0] + negArea[t, 0] + negArea[tt, 0], x[1] + negArea[t, 1] + negArea[tt, 1], x[2] + negArea[t, 2] + negArea[tt, 2]) > 0u ? 1u : 0u);
                                        }
                                        neighbors[t] = 3u - (side1 + side2 + corner);
                                    }

                                    maskLayout[n].SetOcclusion(t, neighbors[t]);
                                }

                                uint a00 = neighbors[1],
                                     a01 = neighbors[2],
                                     a11 = neighbors[3],
                                     a10 = neighbors[0];

                                //if (a00 + a01 + a11 + a10 != 12)
                                //    maskLayout[n].AOFace = true;
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
                                if (disableGreedyMeshing || a.AOFace)
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

                                var normal = new Vector3(q[0], q[1], q[2]);
                                var aoFace = maskLayout[n].AOFace;

                                if (!maskLayout[n].BackFace)
                                {
                                    dv[v] = h;
                                    du[u] = w;
                                }
                                else
                                {
                                    du[v] = h;
                                    dv[u] = w;
                                    normal = -normal;
                                }

                                var cr = ((c >> 16) & 0xff) / 255f;
                                var cg = ((c >> 8) & 0xff) / 255f;
                                var cb = (c & 0xff) / 255f;

                                var ao = 0f;
                                var AOcurve = new float[] { 0.45f, 0.65f, 0.85f, 1.0f };
                                var auCurveFactor = new float[] { 1f, 0.99f, 0.98f, 0.97f, 0.96f };
                                var aoFactor = auCurveFactor[ maskLayout[n].TotalOcclusion()];
                                for (var o = 0; o < 4; ++o)
                                {
                                    var pao = disableAO ? 1f : AOcurve[maskLayout[n].GetOcclusion(o)];
                                    //vertices[o].Color = new Color(cr * pao, cg * pao, cb * pao);
                                    vertices[o].Color = new Color(cr * aoFactor, cg * aoFactor, cb * aoFactor);
                                    vertices[o].Normal = normal;
                                    ao += pao;
                                }
                                ao /= 4f;

                                if (aoFace)
                                {
                                    vertices[0].Position = new Vector3(x[0], x[1], x[2]);
                                    vertices[1].Position = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                    vertices[2].Position = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                    vertices[3].Position = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);
                                    vertices[4].Position = (vertices[0].Position + vertices[1].Position + vertices[2].Position + vertices[3].Position) / 4;
                                    vertices[4].Normal = normal;
                                    vertices[4].Color = new Color(cr * ao, cg * ao, cb * ao);
                                }
                                else
                                {
                                    vertices[0].Position = new Vector3(x[0], x[1], x[2]);
                                    vertices[1].Position = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                    vertices[2].Position = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                    vertices[3].Position = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);
                                }

                                var v0 = (ushort)volume.opaqueMesh.Add(vertices[0]);
                                var v1 = (ushort)volume.opaqueMesh.Add(vertices[1]);
                                var v2 = (ushort)volume.opaqueMesh.Add(vertices[2]);
                                var v3 = (ushort)volume.opaqueMesh.Add(vertices[3]);

                                if (aoFace)
                                {
                                    var v4 = (ushort)volume.opaqueMesh.Add(vertices[4]);
                                    volume.opaqueMesh.Triangle(v0, v4, v1);
                                    volume.opaqueMesh.Triangle(v1, v4, v2);
                                    volume.opaqueMesh.Triangle(v2, v4, v3);
                                    volume.opaqueMesh.Triangle(v3, v4, v0);

                                }
                                else
                                {
                                    volume.opaqueMesh.Quad(v0, v1, v2, v3);
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

            volume.opaqueMesh.Update();
            return volume.opaqueMesh.VertexCount;
        }

        public static int ExtractMesh(World world, Layer layer, bool disableGreedyMeshing = false, bool disableAO = false)
        {
            /*
            * Extract all faces
            *  - Store hidden faces (which should only be tops) in its own temporary list
            *  - Put all visible faces in to the vertex buffer
            *  - Mark the last primitive/index of visible faces and then insert hidden faces
            *  - When rendering above zLevel, only render up to the end of visible faces
            *      when rendering on the zlevel, render the entire buffer to show hidden faces
            */


            layer.mesh.Clear();

            VertexPositionColorNormal[] vertices = new VertexPositionColorNormal[4];

            var dims = new int[] { world.Size, 1, world.Size };

            var f = new Func<int, int, int, uint>((i, j, k) =>
            {
                if (i < 0 || k < 0 || i >= world.Size || k >= world.Size || j < 0 || j >= world.Depth)
                    return 0;

                MapCell cell;
                if (j != 0)
                    cell = world[i, k, j + layer.Depth];
                else
                    cell = layer[i, k];

                if(cell.Hidden)
                    return 0xff;

                return cell.ToUInt();
            });

            //Sweep over 3-axes
            // d0 = x, d1 = y, d2 = z
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
                    var n = 0;
                    for (x[v] = 0; x[v] < dims[v]; ++x[v])
                    {
                        for (x[u] = 0; x[u] < dims[u]; ++x[u], ++n)
                        {
                            var a = f(x[0], x[1], x[2]);
                            var b = f(x[0] + q[0], x[1] + q[1], x[2] + q[2]);

                            maskLayout[n].data = 0u;
                            if ((a != 0) == (b != 0))
                            {
                                mask[n] = 0;
                            }
                            else if (a != 0)
                            {
                                mask[n] = x[d] > -1 ? a : 0;
                                maskLayout[n].BackFace = false;
                            }
                            else
                            {
                                mask[n] = x[d] < dims[d] - 1 ? b : 0;
                                maskLayout[n].BackFace = true;
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
                                        side1 = (f(x[0] + q[0] + posArea[t, 0], x[1] + q[1] + posArea[t, 1], x[2] + q[2] + posArea[t, 2]) > 0u ? 1u : 0u);
                                        side2 = (f(x[0] + q[0] + posArea[tt, 0], x[1] + q[1] + posArea[tt, 1], x[2] + q[2] + posArea[tt, 2]) > 0u ? 1u : 0u);
                                    }
                                    else
                                    {
                                        side1 = (f(x[0] + negArea[t, 0], x[1] + negArea[t, 1], x[2] + negArea[t, 2]) > 0u ? 1u : 0u);
                                        side2 = (f(x[0] + negArea[tt, 0], x[1] + negArea[tt, 1], x[2] + negArea[tt, 2]) > 0u ? 1u : 0u);
                                    }

                                    if (side1 > 0 && side2 > 0)
                                    {
                                        neighbors[t] = 0;
                                    }
                                    else
                                    {
                                        if (a != 0)
                                        {
                                            corner = (f(x[0] + q[0] + posArea[t, 0] + posArea[tt, 0], x[1] + q[1] + posArea[t, 1] + posArea[tt, 1], x[2] + q[2] + posArea[t, 2] + posArea[tt, 2]) > 0u ? 1u : 0u);
                                        }
                                        else
                                        {
                                            corner = (f(x[0] + negArea[t, 0] + negArea[tt, 0], x[1] + negArea[t, 1] + negArea[tt, 1], x[2] + negArea[t, 2] + negArea[tt, 2]) > 0u ? 1u : 0u);
                                        }
                                        neighbors[t] = 3u - (side1 + side2 + corner);
                                    }

                                    maskLayout[n].SetOcclusion(t, neighbors[t]);
                                }

                                uint a00 = neighbors[1],
                                     a01 = neighbors[2],
                                     a11 = neighbors[3],
                                     a10 = neighbors[0];

                                //if (a00 + a01 + a11 + a10 != 12)
                                //    maskLayout[n].AOFace = true;
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
                                if (disableGreedyMeshing || a.AOFace)
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

                                var normal = new Vector3(q[0], q[1], q[2]);
                                var aoFace = maskLayout[n].AOFace;

                                if (!maskLayout[n].BackFace)
                                {
                                    dv[v] = h;
                                    du[u] = w;
                                }
                                else
                                {
                                    du[v] = h;
                                    dv[u] = w;
                                    normal = -normal;
                                }

                                var typeid = c & 0xff;
                                var meta = MapCellDescriptor.MapCells[typeid];

                                var cr = meta.Color.R / 255f;
                                var cg = meta.Color.G / 255f;
                                var cb = meta.Color.B / 255f;

                                var ao = 0f;
                                var AOcurve = new float[] { 0.45f, 0.65f, 0.85f, 1.0f };
                                var auCurveFactor = new float[] { 1f, 0.99f, 0.98f, 0.97f, 0.96f };
                                var aoFactor = auCurveFactor[maskLayout[n].TotalOcclusion()];
                                for (var o = 0; o < 4; ++o)
                                {
                                    var pao = disableAO ? 1f : AOcurve[maskLayout[n].GetOcclusion(o)];
                                    //vertices[o].Color = new Color(cr * pao, cg * pao, cb * pao);
                                    vertices[o].Color = new Color(cr * aoFactor, cg * aoFactor, cb * aoFactor);
                                    vertices[o].Normal = normal;
                                    ao += pao;
                                }
                                ao /= 4f;

                                if (aoFace)
                                {
                                    vertices[0].Position = new Vector3(x[0], x[1], x[2]);
                                    vertices[1].Position = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                    vertices[2].Position = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                    vertices[3].Position = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);
                                    vertices[4].Position = (vertices[0].Position + vertices[1].Position + vertices[2].Position + vertices[3].Position) / 4;
                                    vertices[4].Normal = normal;
                                    vertices[4].Color = new Color(cr * ao, cg * ao, cb * ao);
                                }
                                else
                                {
                                    vertices[0].Position = new Vector3(x[0], x[1], x[2]);
                                    vertices[1].Position = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                    vertices[2].Position = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                    vertices[3].Position = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);
                                }

                                var v0 = (ushort)layer.mesh.Add(vertices[0]);
                                var v1 = (ushort)layer.mesh.Add(vertices[1]);
                                var v2 = (ushort)layer.mesh.Add(vertices[2]);
                                var v3 = (ushort)layer.mesh.Add(vertices[3]);

                                if (aoFace)
                                {
                                    var v4 = (ushort)layer.mesh.Add(vertices[4]);
                                    layer.mesh.Triangle(v0, v4, v1);
                                    layer.mesh.Triangle(v1, v4, v2);
                                    layer.mesh.Triangle(v2, v4, v3);
                                    layer.mesh.Triangle(v3, v4, v0);

                                }
                                else
                                {
                                    layer.mesh.Quad(v0, v1, v2, v3);
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

            layer.mesh.Update();
            return layer.mesh.VertexCount;
        }

    }

}
