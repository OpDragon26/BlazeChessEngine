namespace Blaze;

public struct Move
{
    public (int file, int rank) Source;
    public (int file, int rank) Destination;
    public ulong Promotion;
    public byte Type;

    public Move((int file, int rank) source, (int file, int rank) destination, ulong promotion = 0b1111, byte type = 0b0000)
    {
        Source = source;
        Destination = destination;
        Promotion = promotion;
        Type = type;
    }
    
    /*
    Special moves
    0000 - regular move
    1000 - also regular move
    0001 - white double move
    1001 - black double move
    0010 - white short castle
    0011 - white long castle
    1010 - black short castle
    1011 - black long castle
    0100 - white en passant
    1100 - black en passant
    */
}