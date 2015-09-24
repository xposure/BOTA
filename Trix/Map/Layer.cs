using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Trix.Rendering;

namespace Trix.Map
{
    public class Layer
    {
        private World world;
        private MapCell[] mapCells;
        private int zLevel;
        public DynamicMesh<VertexPositionColorNormal> mesh;

        public Layer(World world, int z)
        {
            this.world = world;
            this.mapCells = new MapCell[world.Size * world.Size];
            this.zLevel = z;
            this.mesh = new DynamicMesh<VertexPositionColorNormal>(world.Device);
        }

        public int Depth { get { return zLevel; } }

        public MapCell this[int index]
        {
            get { return mapCells[index]; }
            set { mapCells[index] = value; }
        }

        public MapCell this[int x, int y]
        {
            get
            {
                return mapCells[x + y * world.Size];
            }
            set
            {
                mapCells[x + y * world.Size] = value;
            }
        }

        public void Fill(MapCell cell)
        {
            var size2 = world.Size * world.Size;
            for (var i = 0; i < size2; ++i)
                mapCells[i] = cell;
        }

        public void Render(BasicEffect effect)
        {
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                effect.World = Matrix.CreateTranslation(new Vector3(0, zLevel, 0));
                pass.Apply();
                mesh.Draw();
            }
        }

        public void UpdateHiddenCells()
        {
            //TODO: optimize
            for(var x = 0; x < world.Size;++x)
            {
                for (var y = 0; y < world.Size; ++y)
                {
                    if (mapCells[x + y * world.Size].Meta.IsEmpty)
                        continue;

                    mapCells[x + y * world.Size].Hidden = false;

                    if (x > 0 && this[x - 1, y].Meta.IsEmpty)
                        continue;
                    else if (x < world.Size - 1 && this[x + 1, y].Meta.IsEmpty)
                        continue;
                    else if (y > 0 && this[x, y - 1].Meta.IsEmpty)
                        continue;
                    else if (y < world.Size - 1 && this[x, y + 1].Meta.IsEmpty)
                        continue;
                    else if (zLevel > 0 && world[x, y, zLevel - 1].Meta.IsEmpty)
                        continue;
                    else if (zLevel < world.Depth - 1 && world[x, y, zLevel + 1].Meta.IsEmpty)
                        continue;
                    
                    mapCells[x + y * world.Size].Hidden = true;
                }
            }
        }

        public void BuildMesh()
        {
            SurfaceExtractor.ExtractMesh(this.world, this);
            return;
            /*
            * Extract all faces
            *  - Store hidden faces (which should only be tops) in its own temporary list
            *  - Put all visible faces in to the vertex buffer
            *  - Mark the last primitive/index of visible faces and then insert hidden faces
            *  - When rendering above zLevel, only render up to the end of visible faces
            *      when rendering on the zlevel, render the entire buffer to show hidden faces
            */

            mesh.Clear();

            VertexPositionColorNormal[] vertices = new VertexPositionColorNormal[4];

            var dims = new int[] { world.Size, 1, world.Size };

            var f = new Func<int, int, int, MapCell>((x, y, z) =>
            {
                if (x < 0 || y < 0 || z < 0 || x >= dims[0] || y >= dims[1] || z >= dims[2])
                    return world[x, z, y];

                return mapCells[x + z * world.Size];
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

                q[d] = 1;

                for (x[d] = -1; x[d] < dims[d]; )
                {
                    //Compute mask
                    int[] du = { 0, 0, 0 };
                    int[] dv = { 0, 0, 0 };

                    for (x[v] = 0; x[v] < dims[v]; ++x[v])
                    {
                        for (x[u] = 0; x[u] < dims[u]; ++x[u])
                        {
                            var a = f(x[0], x[1], x[2]);
                            var b = f(x[0] + q[0], x[1] + q[1], x[2] + q[2]);

                            if ((!a.Meta.IsEmpty) == (!b.Meta.IsEmpty))
                            {
                                continue;
                            }

                            //x[u] = i; x[v] = j;
                            var normal = new Vector3(q[0], q[1], q[2]);
                            MapCellDescriptor meta;

                            if (!a.Meta.IsEmpty)
                            {
                                //normal quad
                                du[v] = 1;
                                dv[u] = 1;
                                meta = a.Meta;

                                vertices[0].Position = new Vector3(x[0], x[1], x[2]);
                                vertices[1].Position = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                vertices[2].Position = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                vertices[3].Position = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);

                            }
                            else
                            {
                                //continue;
                                //backface quad
                                dv[v] = 1;
                                du[u] = 1;
                                normal = -normal;
                                meta = b.Meta;

                                vertices[0].Position = new Vector3(x[0] + q[0], x[1] + q[1], x[2] + q[2]);
                                vertices[1].Position = new Vector3(x[0] + q[0] + du[0], x[1] + q[1] + du[1], x[2] + q[2] + du[2]);
                                vertices[2].Position = new Vector3(x[0] + q[0] + du[0] + dv[0], x[1] + q[1] + du[1] + dv[1], x[2] + q[2] + du[2] + dv[2]);
                                vertices[3].Position = new Vector3(x[0] + q[0] + dv[0], x[1] + q[1] + dv[1], x[2] + q[2] + dv[2]);

                            }

                            for (var p = 0; p < 4; ++p)
                            {
                                vertices[p].Color = meta.Color;
                                vertices[p].Normal = normal;
                            }


                            var v0 = mesh.Add(vertices[0]);
                            var v1 = mesh.Add(vertices[1]);
                            var v2 = mesh.Add(vertices[2]);
                            var v3 = mesh.Add(vertices[3]);
                            mesh.Quad(v0, v1, v2, v3);
                        }
                    }

                    //Increment x[d]
                    ++x[d];
                }
            }

            mesh.Update();
        }
    }
}
