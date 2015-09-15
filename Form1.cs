using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        //private Cell[,] grid = new Cell[0, 0];
        private Grid grid = new Grid();

        private bool? mouseFillState = null;

        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            grid.font = this.Font;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            grid.Draw(e.Graphics);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == (char)27)
                this.Close();
            else if (e.KeyChar == (char)32)
            {
                grid.Step();
                this.Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            resizeGrid();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var cell = grid.GetCellByMouse(e.X, e.Y);
                if (cell != null)
                {
                    if (cell.State == CellState.Block)
                        mouseFillState = false;
                    else
                        mouseFillState = true;

                    grid.SetState(cell, mouseFillState.Value ? CellState.Block : CellState.Empty);
                    this.Invalidate();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            mouseFillState = null;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var cell = grid.GetCellByMouse(e.X, e.Y);
                if (cell != null)
                {
                    var state = mouseFillState.Value ? CellState.Block : CellState.Empty;
                    if (cell.State != state)
                    {
                        grid.SetState(cell, state);
                        this.Invalidate();
                    }
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var cell = grid.GetCellByMouse(e.X, e.Y);
                if (cell != null)
                {
                    if (grid.Start != null && grid.End != null)
                    {
                        grid.SetState(grid.End, CellState.Empty);
                        grid.SetState(grid.Start, CellState.Empty);
                    }

                    if (grid.Start == null)
                        grid.SetState(cell, CellState.Start);
                    else
                        grid.SetState(cell, CellState.End);
                }
                this.Invalidate();
            }
        }

        private void resizeGrid()
        {
            grid.Resize(this.ClientRectangle.Width, this.ClientRectangle.Height);
        }
    }
}
