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
        public const byte HIDDEN = 255;

        public static readonly MapCellDescriptor[] MapCells = new MapCellDescriptor[256];

        static MapCellDescriptor()
        {
            MapCells[AIR] = new MapCellDescriptor(Color.Transparent, isEmpty: true);
            MapCells[GRASS] = new MapCellDescriptor(Color.Green);
            MapCells[BEDROCK] = new MapCellDescriptor(Color.DarkGray);
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
        public static MapCell HIDDEN { get { return new MapCell() { TypeID = MapCellDescriptor.HIDDEN }; } }
    }
}
