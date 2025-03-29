namespace Blaze;

public static class Bitboards
{
    /*
    The magic lookup returns a span of moves to be copied into the move array and its lenght, and a bitboard with squares that are captures, but might land on a friendly piece
    The returned moves all land on empty squares, while the bitboard shows moves that land on occupied squares. 
    Select the enemy pieces from those captured using the AND operation and a second magic lookup is initiated using that bitboard, which returns another span of moves
    */
    
    // 

    public static ulong[,] RookMasks = new ulong[8,8];
    public static ulong[,] BishopMasks = new ulong[8,8];
    
    public static ulong[,][] RookBlockers = new ulong[8,8][];
    public static ulong[,][] BishopBlockers = new ulong[8,8][];
    
    private const ulong File = 0x8080808080808080;
    private const ulong Rank = 0xFF00000000000000;
    
    private const ulong UpDiagonal = 0x102040810204080;
    private const ulong DownDiagonal = 0x8040201008040201;

    private static bool init;
    public static void Init()
    {
        if (init) return;
        init = true;
        
        // Create the masks for every square on the board
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 7; file >= 0; file--)
            {
                // The last bit also has to be evaluated in every direction, since it matters whether it's blocked or not
                RookMasks[file, rank] = (Rank >> (rank * 8)) ^ (File >> (7 - file));
                RookBlockers[file, rank] = Combinations(RookMasks[file, rank]);
                
                // bishop masks
                ulong relativeUD = UpDiagonal;
                ulong relativeDD = DownDiagonal;

                int UDPush = rank - file;
                int DDPush = rank + file - 7;

                if (UDPush >= 0)
                    relativeUD >>= UDPush * 8;
                else // negative
                    relativeUD <<= -UDPush * 8;
                if (DDPush >= 0)
                    relativeDD >>= DDPush * 8;
                else
                    relativeDD <<= -DDPush * 8;
                
                BishopMasks[file, rank] = relativeUD ^ relativeDD;
                BishopBlockers[file, rank] = Combinations(BishopMasks[file, rank]);
            }
        }
    }
    
    // reused code from my previous attempt
    // generates every bit combination using a mask
    private static ulong[] Combinations(ulong blockerMask)
    {
        // count how many on bits are there in the mask, that's going to give us the amount of combinations
        List<int> indices = new List<int>();
        int l = 0;
        for (int i = 0; i < 64; i++)
        {
            if (((blockerMask << 63 - i) >> 63) != 0)
            {
                l++;
                indices.Add(i);
            }
        }
        ulong[] combinations = new ulong[(int)Math.Pow(2, l)];
            
        // for each combination
        for (ulong i = 0; i < (ulong)combinations.Length; i++)
        {
            ulong combination = 0;
                
            // for each index in the mask, push the bits of the combination to the right indices 
            for (int j = 0; j < l; j++)
            {
                combination ^= ((i << 63 - j) >> 63) << indices[j];
            }
                
            combinations[i] = combination;
        }

        return combinations;
    }
    
    private const ulong Square = 0x8000000000000000;
    public static ulong GetSquare(int file, int rank) // overload that takes individual values
    {
        return Square >> (rank * 8 + 7 - file);
    }
    public static ulong GetSquare((int file, int rank) square) // overload that takes individual values
    {
        return Square >> (square.rank * 8 + 7 - square.file);
    }

    private static bool ValidSquare(int file, int rank)
    {
        return file >= 0 && file < 8 && rank >= 0 && rank < 7;
    }
}