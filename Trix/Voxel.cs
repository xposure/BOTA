
public enum VoxelFlags : int 
{

}

public enum VoxelFormat : ulong
{
    //NAME = xxxxMASKxxxSHIFT
    TYPEID = 0x0000000F00000000UL
}

public struct Voxel
{
    public UIntBitArray data;

    public bool this[VoxelFlags flag]
    {
        get { return data[((int)flag) - 1]; }
        set { data[((int)flag) - 1] = value; }
    }

    public uint this[VoxelFormat range]
    {
        get { return data[(uint)((ulong)range >> 32)] >> (int)range; }
        set { data[(uint)((ulong)range >> 32)] = value >> (int)range; }
    }

    public byte TypeID { get { return (byte)this[VoxelFormat.TYPEID]; } }
}

