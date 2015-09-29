using System;

namespace Trix
{
    /*
     * Rendering 
     *  - Chunks need to be in layers, hide no visible cubes as gray
     *  - Cubes need to be voxelized
     *  - Should selection be a texture or another buffer? what about designations?
     *  - Floors?
     *  - Need custom shaders soon
     *  - Need font rendering for debug info
     * Async Chunking
     *  - Threaded chunk logic
     * Loading
     *  - Pulling data from HDD or generating it from noise
     * Rebuilding
     *  - There are different levels of rebuilding (light, mesh, water, etc?)
     * Unloading
     * Culling
     *  - Breadth First Search using flood fill to determine what should be rendered
     * Terrain
     *  - Mouse picking
     *  - 
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
