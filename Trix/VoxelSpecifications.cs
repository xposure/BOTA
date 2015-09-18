
using VSDataKVP = System.Collections.Generic.KeyValuePair<VSData, uint>;

public enum VSFlags : int
{
    WATER = 1,
    TRANSPARENT = 2
}

public enum VSData : ulong
{
    //NAME = xxxxMASKxxxSHIFT
    TYPEID = 0x0000000F00000000UL,
    TEST = 0x0000000000000000UL
}

public struct VoxelSpecification
{
    private static uint TYPEID_COUNT = 0;
    public readonly static VoxelSpecification Air;
    public readonly static VoxelSpecification Water = new VoxelSpecification(VSFlags.TRANSPARENT | VSFlags.WATER);

    private VoxelSpecification(VSFlags flags, params VSDataKVP[] vdata)
    {
        data = new UIntBitArray();
        var flag = (uint)flags;
        for (var i = 0; i < 32 && flag > 0; ++i)
        {
            var mask = 1u << i;
            var result = flag & mask;
            if (result > 0)
            {
                data[i] = true;
                flag ^= mask;
            }
        }

        foreach (var kvp in vdata)
        {
            System.Diagnostics.Debug.Assert(kvp.Key != VSData.TYPEID);
            this[kvp.Key] = kvp.Value;
        }

        this[VSData.TYPEID] = ++TYPEID_COUNT;
    }

    private UIntBitArray data;

    public bool this[VSFlags flag]
    {
        get { return data[((int)flag) - 1]; }
        set { data[((int)flag) - 1] = value; }
    }

    public uint this[VSData range]
    {
        get { return data[(uint)((ulong)range >> 32)] >> (int)range; }
        set { data[(uint)((ulong)range >> 32)] = value >> (int)range; }
    }

    public byte TypeID { get { return (byte)this[VSData.TYPEID]; } }
}