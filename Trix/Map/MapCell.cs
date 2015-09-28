using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Trix.Map
{
            

    public class MapCellDescriptor
    {
        public const byte AIR = 0;
        public const byte GRASS = 1;
        public const byte BEDROCK = 2;
        public const byte STONE = 3;
        public const byte SAND = 4;
        public const byte CLAY = 5;
        public const byte SNOW = 6;
        public const byte ICE = 7;
        public const byte WATER = 8;
        public const byte GRAVEL = 9;
        public const byte PUMPKIN = 10;
        public const byte REDROSE = 11;
        public const byte YELLOWFLOWER = 12;
        public const byte LAVA = 13;
        public const byte DIRT = 14;
        public const byte HIDDEN = 255;

        public static readonly MapCellDescriptor[] MapCells = new MapCellDescriptor[256];

        static MapCellDescriptor()
        {
            MapCells[AIR] = new MapCellDescriptor(Color.Transparent, isEmpty: true);
            MapCells[GRASS] = new MapCellDescriptor(Color.Green);
            MapCells[BEDROCK] = new MapCellDescriptor(Color.DarkGray);
            MapCells[STONE] = new MapCellDescriptor(Color.Gray);
            MapCells[SAND] = new MapCellDescriptor(Color.Tan);
            MapCells[CLAY] = new MapCellDescriptor(Color.SlateGray);
            MapCells[SNOW] = new MapCellDescriptor(Color.White);
            MapCells[ICE] = new MapCellDescriptor(Color.LightBlue);
            MapCells[WATER] = new MapCellDescriptor(Color.Blue);
            MapCells[GRAVEL] = new MapCellDescriptor(Color.LightGray);
            MapCells[PUMPKIN] = new MapCellDescriptor(Color.Orange);
            MapCells[REDROSE] = new MapCellDescriptor(Color.Red);
            MapCells[YELLOWFLOWER] = new MapCellDescriptor(Color.Yellow);
            MapCells[LAVA] = new MapCellDescriptor(Color.OrangeRed);
            MapCells[DIRT] = new MapCellDescriptor(Color.Brown);

            MapCells[HIDDEN] = new MapCellDescriptor(Color.Black);

            for (var i = 0; i < MapCells.Length; ++i)
                if (MapCells[i] == null)
                    MapCells[i] = MapCells[AIR];
        }

        public readonly bool IsEmpty = false;
        public readonly Color Color;

        public MapCellDescriptor(Color color, bool isEmpty = false)
        {
            Color = color;
            IsEmpty = isEmpty;
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 2)]
    public struct MapCell
    {
        private const byte HIDDEN_BIT = 0;

        public byte TypeID;
        private byte options;

        public bool Hidden
        {
            get { return (options & (1 << HIDDEN_BIT)) > 0; }
            set
            {
                if (value)
                    options |= (1 << HIDDEN_BIT);
                else
                    options &= ( 0xff ^ (1 << HIDDEN_BIT));
            }
        }

        public MapCellDescriptor Meta { get { return MapCellDescriptor.MapCells[TypeID]; } }

        public uint ToUInt()
        {
            return TypeID + (uint)(options << 8);
        }

        public static MapCell AIR { get { return new MapCell() { TypeID = MapCellDescriptor.AIR }; } }
        public static MapCell GRASS { get { return new MapCell() { TypeID = MapCellDescriptor.GRASS }; } }
        public static MapCell BEDROCK { get { return new MapCell() { TypeID = MapCellDescriptor.BEDROCK }; } }
        public static MapCell STONE { get { return new MapCell() { TypeID = MapCellDescriptor.STONE }; } }
        public static MapCell SAND { get { return new MapCell() { TypeID = MapCellDescriptor.SAND }; } }
        public static MapCell CLAY { get { return new MapCell() { TypeID = MapCellDescriptor.CLAY }; } }
        public static MapCell SNOW { get { return new MapCell() { TypeID = MapCellDescriptor.SNOW }; } }
        public static MapCell ICE { get { return new MapCell() { TypeID = MapCellDescriptor.ICE }; } }
        public static MapCell WATER { get { return new MapCell() { TypeID = MapCellDescriptor.WATER }; } }
        public static MapCell GRAVEL { get { return new MapCell() { TypeID = MapCellDescriptor.GRAVEL }; } }
        public static MapCell PUMPKIN { get { return new MapCell() { TypeID = MapCellDescriptor.PUMPKIN }; } }
        public static MapCell REDROSE { get { return new MapCell() { TypeID = MapCellDescriptor.REDROSE }; } }
        public static MapCell YELLOWFLOWER { get { return new MapCell() { TypeID = MapCellDescriptor.YELLOWFLOWER }; } }
        public static MapCell LAVA { get { return new MapCell() { TypeID = MapCellDescriptor.LAVA }; } }
        public static MapCell DIRT { get { return new MapCell() { TypeID = MapCellDescriptor.DIRT }; } }
        public static MapCell HIDDEN { get { return new MapCell() { TypeID = MapCellDescriptor.HIDDEN }; } }
    }
}
