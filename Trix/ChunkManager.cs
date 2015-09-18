//using System.Collections;
//using System.Collections.Generic;

//public struct Position
//{
//    public int worldX, worldY;
//    public int localX, localY, depth;
//    public int chunkX, chunkY;
//    public uint encodedID;

//    private void genEncodedID()
//    {
//        System.Diagnostics.Debug.Assert(chunkX >= 0 && chunkX <= 0xfff);
//        System.Diagnostics.Debug.Assert(chunkY >= 0 && chunkY <= 0xfff);
//        System.Diagnostics.Debug.Assert(depth >= 0 && depth <= 0xff);

//        var cx = (uint)(chunkX & 0xfff) << 20;
//        var cy = (uint)(chunkY & 0xfff) << 8;
//        var d = (uint)(depth & 0xff);
//        encodedID = cx | cy | d;
//    }

//    public static Position FromWorld(int x, int y, int depth)
//    {
//        var p = new Position();
//        p.localX = x % ChunkManager.CHUNK_SIZE;
//        p.localY = y % ChunkManager.CHUNK_SIZE;
//        p.depth = depth;
//        p.chunkX = x / ChunkManager.CHUNK_SIZE;
//        p.chunkY = y / ChunkManager.CHUNK_SIZE;
//        p.worldX = x;
//        p.worldY = y;
//        p.genEncodedID();
//        return p;
//    }

//    public static Position FromChunk(int cx, int cy, int depth)
//    {
//        var p = new Position();
//        p.localX = 0;
//        p.localY = 0;
//        p.depth = depth;
//        p.chunkX = cx;
//        p.chunkY = cy;
//        p.worldX = cx * ChunkManager.CHUNK_SIZE;
//        p.worldY = cy * ChunkManager.CHUNK_SIZE;
//        p.genEncodedID();

//        return p;
//    }    
//}

//public class World
//{
//    private Dictionary<uint, Chunk> chunks = new Dictionary<uint, Chunk>();

//    private int width, depth, height;

//    public int Width { get { return width; } }
//    public int Depth { get { return depth; } }
//    public int Height { get { return height; } }

//    public World(int width, int height, int depth)
//    {
//        this.width = width;
//        this.height = height;
//        this.depth = depth;
//    }

//    public void Generate()
//    {
//        layers = new Layer[depth];
//        for (var i = 0; i < depth; ++i)
//        {
//            layers[i] = new Layer();
//        }
//    }

//    public Voxel GetVoxel(Position p)
//    {
//        return new Voxel();
//    }

//    public Chunk GetChunk(uint encodedId)
//    {
//        Chunk chunk = null;
//        if (!chunks.TryGetValue(encodedId, out chunk))
//            return Chunk.Empty;

//        return chunk;
//    }

//    public Layer GetLayer(int depth)
//    {
//        System.Diagnostics.Debug.Assert(depth >= 0 && depth < this.depth);
//        return layers[depth];
//    }
    
//}

//public class Layer
//{
//    //public Dictionary<string, GameObject> unityGameObjects = new Dictionary<string, GameObject>();
//    public Dictionary<uint, Chunk> chunks = new Dictionary<uint, Chunk>();

//    public Chunk GetChunk(Position p)
//    {

//    }
//}

//public class Chunk
//{
//    private bool isEmpty = false;
//    private ChunkManager manager;

//    public readonly static Chunk Empty = new Chunk() { isEmpty = true };

//    //public Chunk NegativeX {  get { return manager.getc }}
//}

//public class ChunkManager : RenderQueue
//{
//    //new code

    

//    //old code

//    public Material opaqueMaterial;
//    public Material waterMaterial;
//    public int resolution = 64;
//    public NoiseType noise = NoiseType.Perlin;
//    public float zoom = 1f;
//    public float offset = 0f;
//    public bool wireframe = false;

//    public const int GRID_SIZE = 16; // MAX 64
//    public const int CHUNK_HALFSIZE = 8;
//    public const int CHUNK_SIZE = CHUNK_HALFSIZE + CHUNK_HALFSIZE; //MAX 64
//    public const int MAP_DEPTH = 32; //MAX 256;

//    private Volume[,] chunks = new Volume[GRID_SIZE, GRID_SIZE];

//    private Volume hover;
//    private int sealevel = 8;

//    public void Start()
//    {
//        hover = SurfaceExtractor.makeVoxels(null, 0, 0, 0,
//                    new int[] { 0, 0, 0 },
//                    new int[] { 1, 1, 1 },
//                        (i, j, k) =>
//                        {
//                            return 0xff0000;
//                        }
//                    );

//        hover.PrepareMesh();
//        SurfaceExtractor.GenerateMesh(null, hover, centered: true);

//        for (var x = 0; x < GRID_SIZE; ++x)
//        {
//            for (var z = 0; z < GRID_SIZE; ++z)
//            {
//                var chunk = SurfaceExtractor.makeVoxels(this, x * CHUNK_SIZE, 0, z * CHUNK_SIZE,
//                    new int[] { 0, 0, 0 },
//                    new int[] { CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE },
//                       Generators.GenerateHeight(x, 0, z, CHUNK_SIZE, CHUNK_SIZE, noise, sealevel)
//                    );

//                chunks[x, z] = chunk;
//            }
//        }

//        var opaque = Resources.Load<GameObject>("Prefabs/ChunkOpaque");
//        var water = Resources.Load<GameObject>("Prefabs/ChunkWater");
//        for (var x = 0; x < GRID_SIZE; ++x)
//        {
//            for (var y = 0; y < GRID_SIZE; ++y)
//            {
//                chunks[x, y].UpdateMesh();
//                {
//                    var r = Instantiate(opaque, new Vector3(chunks[x, y].X, chunks[x, y].Y, chunks[x, y].Z), Quaternion.identity);
//                    r.name = string.Format("CHUNK_{0}_{1}_OPAQUE", x, y);
//                    var rgo = (GameObject)r;
//                    rgo.transform.parent = this.transform;
//                    var filter = rgo.GetComponent<MeshFilter>();
//                    filter.mesh = chunks[x, y].opaqueMesh;
//                }
//                {
//                    var r = Instantiate(water, new Vector3(chunks[x, y].X, chunks[x, y].Y, chunks[x, y].Z), Quaternion.identity);
//                    r.name = string.Format("CHUNK_{0}_{1}_WATER", x, y);
//                    var rgo = (GameObject)r;
//                    rgo.transform.parent = this.transform;
//                    var filter = rgo.GetComponent<MeshFilter>();
//                    filter.mesh = chunks[x, y].waterMesh;
//                }
//            }
//        }


//    }

//    public void OnGUI()
//    {
//        var r = RayCast(Input.mousePosition);
//        var p = r.direction;
//        var start = r.origin;
//        var end = r.direction * 100f + r.origin;

//        GUI.Label(new Rect(0, 0, 200, 20), start.ToString());
//        GUI.Label(new Rect(0, 10, 200, 20), end.ToString());

//        foreach (var rp in GridRayTracer.Trace(start, end))
//        {
//            var wx = (int)rp.x;
//            var wy = (int)rp.y;
//            var wz = (int)rp.z;

//            if (GetBlock(wx, wy, wz) > 0)
//            {
//                GUI.Label(new Rect(0, 20, 200, 20), rp.ToString());
//                return;
//            }
//        }

//        GUI.Label(new Rect(0, 20, 200, 20), "No hits");
//    }

//    public uint GetBlock(int wx, int wy, int wz)
//    {
//        if (wy < 0 || wy >= CHUNK_SIZE)
//            return 0;

//        if (wx < 0 || wx >= CHUNK_SIZE * GRID_SIZE)
//            return 0;

//        if (wz < 0 || wz >= CHUNK_SIZE * GRID_SIZE)
//            return 0;

//        var cx = wx / CHUNK_SIZE;
//        var cz = wz / CHUNK_SIZE;
//        var ox = wx % CHUNK_SIZE;
//        var oz = wz % CHUNK_SIZE;

//        var chunk = chunks[cx, cz];
//        return chunk[ox, wy, oz];
//    }

//    public Ray RayCast(Vector3 mp)
//    {
//        return Camera.main.ScreenPointToRay(mp);
//    }

//    Vector3 start, end;

//    public override void RenderWater()
//    {

//        GL.wireframe = wireframe;
//        if (waterMaterial != null)
//        {
//            for (int pass = 0; pass < waterMaterial.passCount; pass++)
//            {
//                waterMaterial.SetPass(pass);
//                for (var x = 0; x < GRID_SIZE; ++x)
//                    for (var y = 0; y < GRID_SIZE; ++y)
//                        chunks[x, y].RenderAlpha();
//            }
//        }
//        GL.wireframe = false;
//    }

//    public override void RenderOpaque()
//    {
//        //GL.wireframe = wireframe;
//        //if (opaqueMaterial != null)
//        //{
//        //    for (int pass = 0; pass < opaqueMaterial.passCount; pass++)
//        //    {
//        //        opaqueMaterial.SetPass(pass);
//        //        for (var x = 0; x < GRID_SIZE; ++x)
//        //            for (var y = 0; y < GRID_SIZE; ++y)
//        //                chunks[x, y].RenderOpaque();
//        //    }
//        //}
//        //GL.wireframe = false;

//        //if (Input.GetKey(KeyCode.Z))
//        {
//            var r = RayCast(Input.mousePosition);
//            var p = r.direction;
//            start = r.origin;
//            end = r.direction * 100f + r.origin;
//        }

//        //HACK: hover is using water's shader
//        foreach (var rp in GridRayTracer.Trace(start, end))
//        {
//            var wx = (int)rp.x;
//            var wy = (int)rp.y;
//            var wz = (int)rp.z;
//            //Graphics.DrawMeshNow(hover.mesh, m);

//            if (GetBlock(wx, wy, wz) > 0)
//            {
//                var m = Matrix4x4.TRS(new Vector3(wx, wy, wz) + new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, Vector3.one * 1.1f);
//                Graphics.DrawMeshNow(hover.opaqueMesh, m);
//                return;
//            }
//        }

//    }
//}

