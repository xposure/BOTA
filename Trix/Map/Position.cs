using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trix.Map
{
    public struct Position
    {
        public int worldX, worldY;
        public int localX, localY, depth;
        public int chunkX, chunkY;
        public uint encodedID;

        private void genEncodedID()
        {
            System.Diagnostics.Debug.Assert(chunkX >= 0 && chunkX <= 0xfff);
            System.Diagnostics.Debug.Assert(chunkY >= 0 && chunkY <= 0xfff);
            System.Diagnostics.Debug.Assert(depth >= 0 && depth <= 0xff);

            var cx = (uint)(chunkX & 0xfff) << 20;
            var cy = (uint)(chunkY & 0xfff) << 8;
            var d = (uint)(depth & 0xff);
            encodedID = cx | cy | d;
        }

        public static Position FromWorld(int x, int y, int depth)
        {
            var p = new Position();
            p.localX = x % Constants.CHUNK_SIZE;
            p.localY = y % Constants.CHUNK_SIZE;
            p.depth = depth;
            p.chunkX = x / Constants.CHUNK_SIZE;
            p.chunkY = y / Constants.CHUNK_SIZE;
            p.worldX = x;
            p.worldY = y;
            p.genEncodedID();
            return p;
        }

        public static Position FromChunk(int cx, int cy, int depth)
        {
            var p = new Position();
            p.localX = 0;
            p.localY = 0;
            p.depth = depth;
            p.chunkX = cx;
            p.chunkY = cy;
            p.worldX = cx * Constants.CHUNK_SIZE;
            p.worldY = cy * Constants.CHUNK_SIZE;
            p.genEncodedID();

            return p;
        }
    }
}
