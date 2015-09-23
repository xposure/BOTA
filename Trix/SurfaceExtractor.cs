using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Trix;
using Trix.Rendering;
using Trix.Voxels;

/*
 * - Async Chunking
 *      Threaded chunk logic
 * - Loading
 *      Pulling data from HDD or generating it from noise
 * - Rebuilding
 *      There are different levels of rebuilding (light, mesh, water, etc?)
 * - Unloading
 * - Render
 * - Culling
 *      Breadth First Search using flood fill to determine what should be rendered
 */







