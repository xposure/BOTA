using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
    public enum CellState
    {
        Empty, Start, End, Block
    }

    public enum PathState
    {
        None, Open, Closed, Found
    }

    public class Grid
    {
        private HeapPriorityQueue<Cell> openList = new HeapPriorityQueue<Cell>(1000);
        private List<Cell> closedList = new List<Cell>();

        private int size = 40;
        private int width = -1, height = -1;
        private Bitmap gridBitmap = null;
        private Graphics gridGraphics = null;
        private Cell start, end;
        private Cell[,] cells = new Cell[0, 0];

        public Font font;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Cell Start { get { return start; } }
        public Cell End { get { return end; } }

        public void Draw(Graphics g)
        {
            g.DrawImageUnscaled(gridBitmap, new Point(0, 0));
        }

        public bool Resize(int w, int h)
        {
            var gw = w / size;
            var gh = h / size;
            if (gw != width || gh != height)
            {
                //resize grid
                var newGrid = new Cell[gw, gh];

                for (var x = 0; x < gw; x++)
                {
                    for (var y = 0; y < gh; y++)
                    {
                        if (x <= width || y <= height)
                            newGrid[x, y] = cells[x, y];

                        if (newGrid[x, y] == null)
                            newGrid[x, y] = new Cell() { x = x, y = y };
                    }
                }

                cells = newGrid;
                width = gw;
                height = gh;

                if (gridBitmap != null)
                {
                    gridBitmap.Dispose();
                    gridGraphics.Dispose();
                }

                var gbWidth = Math.Max(w, width * size + 2);
                var gbHeight = Math.Max(h, height * size + 2);

                gridBitmap = new Bitmap(gbWidth, gbHeight);

                gridGraphics = Graphics.FromImage(gridBitmap);
                gridGraphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, gbWidth, gbHeight));

                for (var x = 0; x < width; x++)
                    for (var y = 0; y < height; y++)
                        DrawCell(cells[x, y]);

                return true;
            }

            return false;
        }

        private void DrawCell(Cell cell)
        {
            var clientArea = new Rectangle(cell.x * size, cell.y * size, size, size);

            if (cell.State == CellState.Block)
                gridGraphics.FillRectangle(Brushes.Blue, clientArea);
            else if (cell.State == CellState.End)
                gridGraphics.FillRectangle(Brushes.Red, clientArea);
            else if (cell.State == CellState.Start)
                gridGraphics.FillRectangle(Brushes.Green, clientArea);
            else
                gridGraphics.FillRectangle(Brushes.Black, clientArea);

            gridGraphics.DrawRectangle(Pens.MediumPurple, clientArea);
            clientArea.Inflate(-1, -1);

            if (cell.Path == PathState.Open)
                gridGraphics.DrawRectangle(Pens.Green, clientArea);
            else if (cell.Path == PathState.Closed)
                gridGraphics.DrawRectangle(Pens.Teal, clientArea);
            else if (cell.Path == PathState.Found)
                gridGraphics.DrawRectangle(Pens.Yellow, clientArea);

            clientArea.Inflate(-1, -1);
            if (cell.parent != null)
            {
                var s = gridGraphics.MeasureString(cell.h.ToString(), font);
                gridGraphics.DrawString(cell.f.ToString(), font, Brushes.White, new Point(clientArea.X, clientArea.Y));
                gridGraphics.DrawString(cell.g.ToString(), font, Brushes.White, new Point(clientArea.X, clientArea.Y + size - (int)s.Height - 1));
                gridGraphics.DrawString(cell.h.ToString(), font, Brushes.White, new Point(clientArea.X + size - 1 - (int)s.Width, clientArea.Y + size - (int)s.Height - 1));

                var cx = (clientArea.Width / 2) + clientArea.Left;
                var cy = (clientArea.Height / 2) + clientArea.Top;

                var dx = cell.parent.x - cell.x;
                var dy = cell.parent.y - cell.y;
                var mod = 7;
                if (dx * dx + dy * dy == 1)
                    mod = 9;


                if (cell.Path == PathState.Found)
                {
                    mod += 2;
                    gridGraphics.DrawLine(Pens.White, cx, cy, cx + dx * mod, cy + dy * mod);
                    gridGraphics.FillEllipse(Brushes.Red, new Rectangle(cx - 5, cy - 5, 10, 10));
                    gridGraphics.DrawEllipse(Pens.White, new Rectangle(cx - 5, cy - 5, 10, 10));
                }
                else
                {
                    gridGraphics.DrawLine(Pens.White, cx, cy, cx + dx * mod, cy + dy * mod);
                    gridGraphics.FillEllipse(Brushes.Black, new Rectangle(cx - 3, cy - 3, 6, 6));
                    gridGraphics.DrawEllipse(Pens.White, new Rectangle(cx - 3, cy - 3, 6, 6));
                }
            }
        }

        public Cell GetCellByMouse(int mx, int my)
        {
            var x = mx / size;
            var y = my / size;

            if (x >= 0 && x < width && y >= 0 && y < height)
                return cells[x, y];

            return null;
        }

        public Cell GetCell(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
                return cells[x, y];

            return null;
        }


        public void SetState(Cell cell, CellState state)
        {
            if (state == CellState.Empty || state == CellState.Block)
            {
                if (cell.State == CellState.Start)
                {
                    cell.State = state;
                    start = null;
                }
                else if (cell.State == CellState.End)
                {
                    cell.State = state;
                    end = null;
                }
                else
                    cell.State = state;
            }
            else if (state == CellState.Start)
            {
                if (start != null)
                {
                    start.State = CellState.Empty;
                    DrawCell(start);
                }

                cell.State = CellState.Start;
                start = cell;
            }
            else if (state == CellState.End)
            {
                if (end != null)
                {
                    end.State = CellState.Empty;
                    DrawCell(end);
                }

                cell.State = CellState.End;
                end = cell;
            }

            DrawCell(cell);
        }

        public void Reset()
        {
            openList.Clear();
            closedList.Clear();
            start = null;
            end = null;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    cells[x, y].Reset();
                    DrawCell(cells[x, y]);
                }
            }
        }

        public void Step()
        {
            if (start != null && end != null)
            {
                if (openList.Count == 0)
                {
                    openList.Enqueue(start, 0);
                    Step();
                }
                else if (end.Path != PathState.None)
                {

                }
                else
                {
                    var current = openList.Dequeue();
                    current.Path = PathState.Closed;
                    closedList.Add(current);

                    foreach (var cell in AdjacentCells(current))
                    {
                        if (cell == end)
                        {
                            end.Path = PathState.Open;
                            end.g = current.CalculateG(end) + current.g;
                            end.parent = current;

                            var loop = end;
                            while (loop != null)
                            {
                                loop.Path = PathState.Found;
                                DrawCell(loop);
                                loop = loop.parent;
                            }
                        }
                        else if (cell.State == CellState.Empty)
                        {
                            if (cell.Path == PathState.None)
                            {
                                cell.g = current.CalculateG(cell) + current.g;
                                cell.h = end.CalculateH(cell);
                                cell.Path = PathState.Open;
                                cell.parent = current;
                                openList.Enqueue(cell, cell.f);
                                DrawCell(cell);
                            }
                            else if (cell.Path == PathState.Open)
                            {
                                var g = current.CalculateG(cell) + current.g;
                                var h = end.CalculateH(cell);
                                var f = g + h;
                                if (f < cell.f)
                                {
                                    //update parent;
                                    openList.Remove(cell);
                                    cell.parent = current;
                                    cell.g = g;
                                    cell.h = h;
                                    openList.Enqueue(cell, cell.f);
                                    DrawCell(cell);
                                }
                            }
                        }
                    }

                    DrawCell(current);
                }
            }
        }

        public IEnumerable<Cell> AdjacentCells(Cell cell)
        {
            var x0y0 = GetCell(cell.x - 1, cell.y - 1);
            var x1y0 = GetCell(cell.x, cell.y - 1);
            var x2y0 = GetCell(cell.x + 1, cell.y - 1);

            var x0y2 = GetCell(cell.x - 1, cell.y + 1);
            var x1y2 = GetCell(cell.x, cell.y + 1);
            var x2y2 = GetCell(cell.x + 1, cell.y + 1);

            var x0y1 = GetCell(cell.x - 1, cell.y);
            var x2y1 = GetCell(cell.x + 1, cell.y);

            if (x1y0 != null) yield return x1y0;
            if (x2y1 != null) yield return x2y1;
            if (x1y2 != null) yield return x1y2;
            if (x0y1 != null) yield return x0y1;

            if (x0y0 != null && x1y0 != null && x0y1 != null && (x1y0.State != CellState.Block && x0y1.State != CellState.Block)) yield return x0y0;
            if (x2y0 != null && x1y0 != null && x2y1 != null && (x1y0.State != CellState.Block && x2y1.State != CellState.Block)) yield return x2y0;
            if (x2y2 != null && x1y2 != null && x2y1 != null && (x1y2.State != CellState.Block && x2y1.State != CellState.Block)) yield return x2y2;
            if (x0y2 != null && x0y1 != null && x1y2 != null && (x0y1.State != CellState.Block && x1y2.State != CellState.Block)) yield return x0y2;
        }
    }

    public class Cell : PriorityQueueNode
    {
        public int x, y;
        public CellState State = CellState.Empty;
        public PathState Path = PathState.None;
        public int g, h;
        public Cell parent;

        public int f { get { return g + h; } }

        public void Reset()
        {
            g = 0;
            h = 0;
            parent = null;
            State = CellState.Empty;
            Path = PathState.None;
        }

        public int CalculateG(Cell cell)
        {
            var a = x - cell.x;
            var b = y - cell.y;
            return ((a * a) + (b * b)) <= 1 ? 10 : 14;
        }

        public int CalculateH(Cell cell)
        {
            return (int)(Math.Abs(x - cell.x) + Math.Abs(y - cell.y)) * 10;
        }
    }
}

