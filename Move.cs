namespace Blaze;

public struct Move
{
    public (int file, int rank) source;
    public (int file, int rank) destination;
    public ulong promotion => 0b1111;
} 