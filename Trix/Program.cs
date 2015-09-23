using System;

namespace Trix
{
    /*
     * Rendering 
     *  - Chunks need to be in layers, hide no visible cubes as gray
     *  - Culling
     *  - Cubes need to be voxelized
     *  - Should selection be a texture or another buffer? what about designations?
     *  - Floors?
     *  - Need custom shaders soon
     *  - Need font rendering for debug info
     *  
     */

#if WINDOWS || LINUX
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
#endif
}
